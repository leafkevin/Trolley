using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.SqlServer;

public class SqlServerCreateVisitor : CreateVisitor, ICreateVisitor
{
    public string LockName { get; set; }
    public string FromSql { get; set; }
    public string OutputSql { get; set; }
    public bool IsOutput { get; set; }

    public SqlServerCreateVisitor(DbContext dbContext, char tableAsStart = 'a')
        : base(dbContext, tableAsStart) { }

    public override string BuildCommand(ITheaCommand command, bool isReturnIdentity, out List<SqlFieldSegment> readerFields)
    {
        string sql = null;
        this.IsReturnIdentity = isReturnIdentity;
        if (this.ActionMode == ActionMode.Bulk)
            sql = this.BuildBulkSql(command, out readerFields);
        else
        {
            //多命令执行时，第二次以后DbParameters有值，并且就是command.Parameters
            //当Insert Select From操作时，DbParameters也有值，但不是command.Parameters，需要赋值到command.Parameters
            if (this.DbParameters != null && this.DbParameters != command.Parameters)
            {
                foreach (var dbParameter in this.DbParameters)
                {
                    command.Parameters.Add(dbParameter);
                }
                this.DbParameters = command.Parameters;
            }
            else this.DbParameters ??= command.Parameters;

            foreach (var deferredSegment in this.deferredSegments)
            {
                switch (deferredSegment.Type)
                {
                    case "WithBy":
                        this.VisitWithBy(deferredSegment.Value);
                        break;
                    case "WithByField":
                        this.VisitWithByField(deferredSegment.Value);
                        break;
                    case "OutputFields":
                        this.VisitOutputFields(deferredSegment.Value as string);
                        break;
                    case "OutputExpression":
                        this.VisitOutputExpression(deferredSegment.Value as LambdaExpression);
                        break;
                }
            }
            sql = this.BuildSql(out readerFields);
        }
        return sql;
    }
    public override string BuildSql(out List<SqlFieldSegment> readerFields)
    {
        readerFields = this.ReaderFields;
        if (!string.IsNullOrEmpty(this.FromSql))
            return this.FromSql;

        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        var entityMapper = tableSegment.Mapper;

        var tableName = this.GetTableName();
        if (this.OutputSql != null && this.IsReturnIdentity)
            throw new NotSupportedException("返回Identity，不支持同时Returning操作");

        string tailSql = null;
        if (this.IsReturnIdentity)
        {
            if (!entityMapper.IsAutoIncrementKey)
                throw new NotSupportedException($"实体{entityMapper.EntityType.FullName}表未配置自增长字段，无法返回Identity值");
            tailSql = this.OrmProvider.GetIdentitySql(null);
        }
        return $"INSERT INTO {tableName} ({this.FieldsBuilder}){this.OutputSql} VALUES ({this.ValuesBuilder}){tailSql}";
    }
    public override string BuildBulkSql(ITheaCommand command, out List<SqlFieldSegment> readerFields)
    {
        //多命令查询或是ToSql才会走到此分支
        //多语句执行，一次性不分批次
        var builder = new StringBuilder();
        (var tableName, var tabledInsertObjs, var insertObjs, _, var firstSqlSetter,
            var loopSqlSetter, _, readerFields) = this.BuildWithBulk(command);
        void Execute(string tableName, IEnumerable insertObjs)
        {
            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
            int index = 0;
            foreach (var insertObj in insertObjs)
            {
                if (index > 0) builder.Append(',');
                loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                index++;
            }
        }
        if (tabledInsertObjs != null)
        {
            int index = 0;
            foreach (var tabledInsertObj in tabledInsertObjs)
            {
                if (index > 0) builder.Append(';');
                Execute(tabledInsertObj.Key, tabledInsertObj.Value);
                index++;
            }
        }
        else Execute(tableName, insertObjs);
        var sql = builder.ToString();
        builder.Clear();
        return sql;
    }
    public override (string, Dictionary<string, List<object>>, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
        Action<IDataParameterCollection, StringBuilder, DbContext, object, string>, string, List<SqlFieldSegment>) BuildWithBulk(ITheaCommand command)
    {
        object firstInsertObj = null;
        Type insertObjType = null;
        (var insertObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;
        foreach (var entity in insertObjs)
        {
            firstInsertObj = entity;
            insertObjType = entity.GetType();
            break;
        }
        string tableName = null;
        Dictionary<string, List<object>> tabledInsertObjs = null;
        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        if (this.ShardingProvider != null && this.ShardingProvider.TryGetTableSharding(entityType, out var tableShardingInfo))
        {
            if (tableSegment.IsSharding)
                tableName = tableSegment.Body;
            else tabledInsertObjs = this.SplitShardingParameters(tableShardingInfo, insertObjs);
        }
        else tableName = tableSegment.Mapper.TableName;

        string fixedSql = "(";
        List<IDbDataParameter> fixedDbParameters = null;
        if (this.deferredSegments.Count > 1)
        {
            this.DbParameters = new TheaDbParameterCollection();
            for (int i = 1; i < this.deferredSegments.Count; i++)
            {
                var deferredSegment = this.deferredSegments[i];
                switch (deferredSegment.Type)
                {
                    case "WithBy":
                        this.VisitWithBy(deferredSegment.Value);
                        break;
                    case "WithByField":
                        this.VisitWithByField(deferredSegment.Value);
                        break;
                    case "OutputFields":
                        this.VisitOutputFields(deferredSegment.Value as string);
                        break;
                    case "OutputExpression":
                        this.VisitOutputExpression(deferredSegment.Value as LambdaExpression);
                        break;
                    default: throw new NotSupportedException("批量插入后，只支持WithBy/IgnoreFields/OnlyFields/Output操作");
                }
                if (this.DbParameters.Count > 0)
                    fixedDbParameters = this.DbParameters.Cast<IDbDataParameter>().ToList();
            }
            fixedSql = $"({this.ValuesBuilder}";
        }

        var entityMapper = tableSegment.Mapper;
        var fieldsSetter = RepositoryHelper.BuildCreateFieldsSqlPart(this.DbContext, entityType, insertObjType, false, this.OnlyFieldNames, this.IgnoreFieldNames);
        var valuesSetter = RepositoryHelper.BuildCreateValuesSqlPart(this.DbContext, entityType, insertObjType, true, false, this.OnlyFieldNames, this.IgnoreFieldNames);
        var typedValuesSetter = valuesSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;

        string headSql = "INSERT INTO ";
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            headSql = $"INSERT INTO {this.OrmProvider.GetTableName(tableSegment.TableSchema)}";

        //生成批量Fields SQL
        fieldsSetter.Invoke(this.FieldsBuilder, this.DbContext, firstInsertObj);

        var readerFields = this.ReaderFields;

        var fieldsSql = $"({this.FieldsBuilder}){this.OutputSql} VALUES ";
        this.FieldsBuilder.Clear();
        this.ValuesBuilder.Clear();

        Action<IDataParameterCollection, StringBuilder, string> firstSqlSetter = null;
        if (fixedDbParameters != null)
        {
            firstSqlSetter = (dbParameters, builder, tableName) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} {fieldsSql}");
                fixedDbParameters.ForEach(f => dbParameters.Add(f));
            };
        }
        else firstSqlSetter = (dbParameters, builder, tableName) => builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} {fieldsSql}");

        Action<IDataParameterCollection, StringBuilder, DbContext, object, string> loopSqlSetter = null;
        loopSqlSetter = (dbParameters, builder, dbContext, insertObj, suffix) =>
        {
            builder.Append(fixedSql);
            typedValuesSetter.Invoke(dbParameters, builder, dbContext, insertObj, suffix);
            builder.Append(')');
        };
        this.DbParameters = command.Parameters;
        return (tableName, tabledInsertObjs, insertObjs, bulkCount, firstSqlSetter, loopSqlSetter, null, readerFields);
    }
    public void WithLock(string lockName) => this.LockName = lockName;
    public void Output(string fieldNames)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "OutputFields",
            Value = fieldNames
        });
    }
    public virtual void Output(Expression fieldsSelector)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "OutputExpression",
            Value = fieldsSelector
        });
    }
    public void WithBulkCopy(IEnumerable insertObjs, int? timeoutSeconds)
    {
        this.ActionMode = ActionMode.BulkCopy;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulkCopy",
            Value = (insertObjs, timeoutSeconds)
        });
    }
    public (IEnumerable, int?) BuildWithBulkCopy() => ((IEnumerable, int?))this.deferredSegments[0].Value;
    public void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.TableAliases.Clear();
        lambdaExpr.Body.GetParameters(out var parameters);
        if (parameters == null || parameters.Count == 0)
            return;
        foreach (var parameterExpr in parameters)
        {
            if (this.TableAliases.ContainsKey(parameterExpr.Name))
                continue;
            this.TableAliases.Add(parameterExpr.Name, this.Tables[0]);
        }
    }
    public string VisitOutputFields(string fieldNames)
    {
        this.ReaderFields = new();
        this.OutputSql = $" OUTPUT {fieldNames}";
        var entityType = this.Tables[0].EntityType;
        if (fieldNames == "*")
        {
            this.OutputSql = $" OUTPUT INSERTED.{fieldNames}";
            var entityMapper = this.Tables[0].Mapper;
            foreach (var memberMapper in entityMapper.MemberMaps)
            {
                if (memberMapper.IsIgnore || memberMapper.IsNavigation)
                    continue;
                this.ReaderFields.Add(new SqlFieldSegment
                {
                    FieldType = SqlFieldType.Field,
                    FromMember = memberMapper.Member,
                    TargetMember = memberMapper.Member,
                    SegmentType = memberMapper.MemberType,
                    NativeDbType = memberMapper.NativeDbType,
                    MappedTargetType = memberMapper.MappedTargetType,
                    TypeHandler = memberMapper.TypeHandler,
                    Body = memberMapper.FieldName
                });
            }
        }
        else
        {
            this.ReaderFields.Add(new SqlFieldSegment
            {
                FieldType = SqlFieldType.RawSql,
                Body = fieldNames
            });
        }
        return this.OutputSql;
    }
    public string VisitOutputExpression(LambdaExpression fieldsSelector)
    {
        this.IsOutput = true;
        this.ReaderFields = new();
        var entityMapper = this.Tables[0].Mapper;
        var builder = new StringBuilder(" OUTPUT ");
        this.InitTableAlias(fieldsSelector);
        switch (fieldsSelector.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = fieldsSelector.Body as MemberExpression;
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberExpr });
                    this.GetQuotedValue(sqlSegment, true);
                    sqlSegment.TargetMember = memberExpr.Member;
                    sqlSegment.SegmentType = memberExpr.Type;
                    builder.Append(sqlSegment.Body);
                    if (sqlSegment.IsNeedAlias || sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall
                        || sqlSegment.FromMember != null && sqlSegment.FromMember.Name != sqlSegment.TargetMember.Name)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberExpr.Member.Name)}");
                    this.ReaderFields.Add(sqlSegment);
                }
                break;
            case ExpressionType.New:
                var newExpr = fieldsSelector.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = newExpr.Arguments[i] });
                    this.GetQuotedValue(sqlSegment, true);
                    sqlSegment.TargetMember = memberInfo;
                    sqlSegment.SegmentType = memberInfo.GetMemberType();
                    if (i > 0) builder.Append(',');
                    builder.Append(sqlSegment.Body);
                    if (sqlSegment.IsNeedAlias || sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberInfo.Name)}");
                    this.ReaderFields.Add(sqlSegment);
                }
                break;
            case ExpressionType.MemberInit:
                var memberInitExpr = fieldsSelector.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                        throw new NotSupportedException("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");

                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberAssignment.Expression });
                    this.GetQuotedValue(sqlSegment, true);
                    sqlSegment.TargetMember = memberAssignment.Member;
                    sqlSegment.SegmentType = memberAssignment.Member.GetMemberType();
                    if (i > 0) builder.Append(',');
                    builder.Append(sqlSegment.Body);
                    if (sqlSegment.IsNeedAlias || sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberAssignment.Member.Name)}");
                    this.ReaderFields.Add(sqlSegment);
                }
                break;
            case ExpressionType.Parameter:
                foreach (var memberMapper in entityMapper.MemberMaps)
                {
                    if (memberMapper.IsIgnore || memberMapper.IsNavigation)
                        continue;
                    this.ReaderFields.Add(new SqlFieldSegment
                    {
                        FieldType = SqlFieldType.Field,
                        FromMember = memberMapper.Member,
                        TargetMember = memberMapper.Member,
                        SegmentType = memberMapper.MemberType,
                        NativeDbType = memberMapper.NativeDbType,
                        MappedTargetType = memberMapper.MappedTargetType,
                        TypeHandler = memberMapper.TypeHandler,
                        Body = memberMapper.FieldName
                    });
                }
                builder.Append("INSERTED.*");
                break;
            default:
                this.VisitAndDeferred(new SqlFieldSegment { Expression = fieldsSelector });
                for (int i = 0; i < this.ReaderFields.Count; i++)
                {
                    var readerField = this.ReaderFields[i];
                    if (i > 0) builder.Append(',');
                    builder.Append(readerField.Body);
                    if (readerField.IsNeedAlias || readerField.IsConstant || readerField.IsVariable || readerField.HasParameter || readerField.IsExpression || readerField.IsMethodCall)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(readerField.TargetMember.Name)}");
                }
                break;
        }
        this.OutputSql = builder.ToString();
        this.IsOutput = false;
        builder.Clear();
        return this.OutputSql;
    }
    public override SqlFieldSegment VisitMemberAccess(SqlFieldSegment sqlSegment)
    {
        if (this.IsOutput)
        {
            var memberExpr = sqlSegment.Expression as MemberExpression;
            if (!this.Tables[0].Mapper.TryGetMemberMap(memberExpr.Member.Name, out var memberMapper))
                throw new MissingMemberException($"类{this.Tables[0].EntityType.FullName}未找到成员{memberExpr.Member.Name}");

            //在解析过程中，引用原值时使用别名，最后再设置IsNeedTableAlias
            var fieldName = $"INSERTED.{this.OrmProvider.GetFieldName(memberMapper.FieldName)}";

            sqlSegment.HasField = true;
            sqlSegment.SegmentType = memberMapper.MemberType;
            sqlSegment.FromMember = memberMapper.Member;
            sqlSegment.NativeDbType = memberMapper.NativeDbType;
            sqlSegment.MappedTargetType = memberMapper.MappedTargetType;
            sqlSegment.TypeHandler = memberMapper.TypeHandler;
            sqlSegment.Body = fieldName;
            return sqlSegment;
        }
        return base.VisitMemberAccess(sqlSegment);
    }
    public override void Dispose()
    {
        base.Dispose();
        this.LockName = null;
        this.OutputSql = null;
    }
}
