using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class CreateVisitor : SqlVisitor, ICreateVisitor
{
    protected List<CommandSegment> deferredSegments = new();

    public List<string> OnlyFieldNames { get; set; }
    public List<string> IgnoreFieldNames { get; set; }
    public List<FieldsSegment> InsertFields { get; set; } = new();
    public ActionMode ActionMode { get; set; }
    public bool IsReturnIdentity { get; set; }

    public CreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
    {
        this.DbKey = dbKey;
        this.OrmProvider = ormProvider;
        this.MapProvider = mapProvider;
        this.ShardingProvider = shardingProvider;
        this.IsParameterized = isParameterized;
        this.TableAsStart = tableAsStart;
        this.ParameterPrefix = parameterPrefix;
    }
    public virtual void Initialize(Type entityType, bool isMultiple = false, bool isFirst = true)
    {
        if (!isMultiple)
        {
            this.Tables = new();
            this.TableAliases = new();
            this.Tables.Add(new TableSegment
            {
                EntityType = entityType,
                AliasName = "a",
                Mapper = this.MapProvider.GetEntityMap(entityType)
            });
        }
        if (!isFirst) this.Clear();
    }
    public virtual string BuildCommand(IDbCommand command, bool isReturnIdentity, out List<ReaderField> readerFields)
    {
        string sql = null;
        readerFields = null;
        this.IsReturnIdentity = isReturnIdentity;
        if (this.ActionMode == ActionMode.Bulk)
            sql = this.BuildWithBulkSql(command, out readerFields);
        else
        {
            this.DbParameters ??= command.Parameters;
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
                }
            }
            sql = this.BuildSql(out readerFields);
        }
        return sql;
    }
    public virtual MultipleCommand CreateMultipleCommand()
    {
        return new MultipleCommand
        {
            CommandType = MultipleCommandType.Insert,
            EntityType = this.Tables[0].EntityType,
            Body = this.deferredSegments,
            Tables = this.Tables,
            IgnoreFieldNames = this.IgnoreFieldNames,
            OnlyFieldNames = this.OnlyFieldNames,
            RefQueries = this.RefQueries,
            IsNeedTableAlias = this.IsNeedTableAlias
        };
    }
    public virtual void BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex)
    {
        this.IsMultiple = true;
        this.CommandIndex = commandIndex;
        this.deferredSegments = multiCommand.Body as List<CommandSegment>;
        this.Tables = multiCommand.Tables;
        this.IgnoreFieldNames = multiCommand.IgnoreFieldNames;
        this.OnlyFieldNames = multiCommand.OnlyFieldNames;
        this.RefQueries = multiCommand.RefQueries;
        this.IsNeedTableAlias = multiCommand.IsNeedTableAlias;
        if (sqlBuilder.Length > 0) sqlBuilder.Append(';');
        if (this.deferredSegments.Count > 0 && this.deferredSegments[0].Type == "WithBulk")
            this.ActionMode = ActionMode.Bulk;
        sqlBuilder.Append(this.BuildCommand(command, false, out var readerFields));
        this.ReaderFields = readerFields;
    }
    public virtual string BuildSql(out List<ReaderField> readerFields)
    {
        readerFields = null;
        var entityType = this.Tables[0].EntityType;
        var entityMapper = this.Tables[0].Mapper;
        var tableName = entityMapper.TableName;
        if (this.ShardingProvider.TryGetShardingTable(entityType, out _))
        {
            if (!this.Tables[0].IsSharding)
                throw new Exception($"实体表{entityType.FullName}有配置分表，当前操作未指定分表，请调用UseTable或UseTableBy方法指定分表");
            tableName = this.Tables[0].Body;
        }
        tableName = this.OrmProvider.GetTableName(tableName);
        var fieldsBuilder = new StringBuilder($"INSERT INTO {tableName} (");

        var valuesBuilder = new StringBuilder();
        valuesBuilder.Append(" VALUES (");
        for (int i = 0; i < this.InsertFields.Count; i++)
        {
            var insertField = this.InsertFields[i];
            if (i > 0)
            {
                fieldsBuilder.Append(',');
                valuesBuilder.Append(',');
            }
            fieldsBuilder.Append(insertField.Fields);
            valuesBuilder.Append(insertField.Values);
        }
        fieldsBuilder.Append(')');
        valuesBuilder.Append(')');

        if (this.IsReturnIdentity)
        {
            if (!entityMapper.IsAutoIncrement)
                throw new Exception($"实体{entityMapper.EntityType.FullName}表未配置自增长字段，无法返回Identity值");
            valuesBuilder.Append(this.OrmProvider.GetIdentitySql(this.Tables[0].EntityType));
        }
        fieldsBuilder.Append(valuesBuilder);
        valuesBuilder.Clear();
        var sql = fieldsBuilder.ToString();
        fieldsBuilder.Clear();
        fieldsBuilder = null;
        valuesBuilder = null;
        return sql;
    }
    public virtual void WithBy(object insertObj)
    {
        this.ActionMode = ActionMode.Single;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBy",
            Value = insertObj
        });
    }
    public virtual void WithByField(Expression fieldSelector, object fieldValue)
    {
        this.ActionMode = ActionMode.Single;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithByField",
            Value = (fieldSelector, fieldValue)
        });
    }
    public virtual void WithBulk(IEnumerable insertObjs, int bulkCount)
    {
        this.ActionMode = ActionMode.Bulk;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulk",
            Value = (insertObjs, bulkCount)
        });
    }
    public virtual void IgnoreFields(string[] fieldNames)
    {
        this.IgnoreFieldNames ??= new();
        this.IgnoreFieldNames.AddRange(fieldNames);
    }
    public virtual void IgnoreFields(Expression fieldsSelector)
        => this.IgnoreFieldNames = this.VisitFields(fieldsSelector);
    public virtual void OnlyFields(string[] fieldNames)
    {
        this.OnlyFieldNames ??= new();
        this.OnlyFieldNames.AddRange(fieldNames);
    }
    public virtual void OnlyFields(Expression fieldsSelector)
        => this.OnlyFieldNames = this.VisitFields(fieldsSelector);
    public virtual string BuildWithBulkSql(IDbCommand command, out List<ReaderField> readerFields)
    {
        //多命令查询或是ToSql才会走到此分支
        //多语句执行，一次性不分批次
        var builder = new StringBuilder();
        (var isNeedSplit, var tableName, var insertObjs, _, var firstInsertObj,
            var headSqlSetter, var valuesSqlSetter, readerFields) = this.BuildWithBulk(command);

        Action<string, IEnumerable> executor = (tableName, insertObjs) =>
        {
            headSqlSetter.Invoke(command.Parameters, builder, tableName, firstInsertObj);
            int index = 0;
            foreach (var insertObj in insertObjs)
            {
                if (index > 0) builder.Append(',');
                valuesSqlSetter.Invoke(command.Parameters, builder, insertObj, index.ToString());
                index++;
            }
        };
        if (isNeedSplit)
        {
            var entityType = this.Tables[0].EntityType;
            var tabledInsertObjs = RepositoryHelper.SplitShardingParameters(this.DbKey, this.MapProvider, this.ShardingProvider, entityType, insertObjs);
            int index = 0;
            foreach (var tabledInsertObj in tabledInsertObjs)
            {
                if (index > 0) builder.Append(';');
                executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
                index++;
            }
        }
        else executor.Invoke(tableName, insertObjs);
        var sql = builder.ToString();
        builder.Clear();
        builder = null;
        return sql;
    }
    public virtual (bool, string, IEnumerable, int, object, Action<IDataParameterCollection, StringBuilder, string, object>,
        Action<IDataParameterCollection, StringBuilder, object, string>, List<ReaderField>) BuildWithBulk(IDbCommand command)
    {
        bool isNeedSplit = false;
        object firstInsertObj = null;
        Type insertObjType = null;

        (var insertObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;
        foreach (var entity in insertObjs)
        {
            firstInsertObj = entity;
            break;
        }
        insertObjType = firstInsertObj.GetType();
        var tableName = this.Tables[0].Mapper.TableName;
        var entityType = this.Tables[0].EntityType;

        if (this.ShardingProvider.TryGetShardingTable(entityType, out _))
        {
            //有设置分表，优先使用分表，没有设置分表，则根据数据的字段确定分表
            if (!string.IsNullOrEmpty(this.Tables[0].Body))
                tableName = this.Tables[0].Body;
            //未指定分表，需要根据数据字段确定分表
            else isNeedSplit = true;
        }
        Action<IDataParameterCollection, StringBuilder, string, object> headSqlSetter = null;
        Action<IDataParameterCollection, StringBuilder, object, string> valuesSqlSetter = null;
        var fieldsSqlPartSetter = RepositoryHelper.BuildCreateFieldsSqlPart(this.OrmProvider, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames);
        var valuesSqlPartSetter = RepositoryHelper.BuildCreateValuesSqlParametes(this.OrmProvider, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames, true);

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
                    default: throw new NotSupportedException("批量插入后，只支持WithBy/IgnoreFields/OnlyFields操作");
                }
            }

            var fixedDbParameters = this.DbParameters.Cast<IDbDataParameter>().ToList();
            headSqlSetter = (dbParameters, builder, tableName, insertObj) =>
            {
                builder.Append($"INSERT INTO {this.OrmProvider.GetFieldName(tableName)} (");
                for (int i = 0; i < this.InsertFields.Count; i++)
                {
                    var insertField = this.InsertFields[i];
                    if (i > 0) builder.Append(',');
                    builder.Append(insertField.Fields);
                }
                fieldsSqlPartSetter.Invoke(builder, insertObj);
                builder.Append(" VALUES ");
                if (fixedDbParameters.Count > 0)
                    fixedDbParameters.ForEach(f => dbParameters.Add(f));
            };
            var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>;

            valuesSqlSetter = (dbParameters, builder, insertObj, suffix) =>
            {
                builder.Append('(');
                for (int i = 0; i < this.InsertFields.Count; i++)
                {
                    var insertField = this.InsertFields[i];
                    if (i > 0) builder.Append(',');
                    builder.Append(insertField.Values);
                }
                typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.OrmProvider, insertObj, suffix);
                builder.Append(')');
            };
            this.DbParameters = command.Parameters;
        }
        else
        {
            (_, var typedHeadSqlSetter, var sqlSetter) = RepositoryHelper.BuildCreateSqlParameters(this.OrmProvider, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames, true, false);
            headSqlSetter = (dbParameters, builder, tableName, insertObj) =>
            {
                typedHeadSqlSetter.Invoke(builder, tableName, insertObj);
                builder.Append(" VALUES ");
            };
            valuesSqlSetter = sqlSetter as Action<IDataParameterCollection, StringBuilder, object, string>;
        }
        return (isNeedSplit, tableName, insertObjs, bulkCount, firstInsertObj, headSqlSetter, valuesSqlSetter, null);
    }
    public virtual void VisitWithBy(object insertObj)
    {
        var entityType = this.Tables[0].EntityType;
        var insertObjType = insertObj.GetType();
        var fieldsSqlPartSetter = RepositoryHelper.BuildCreateFieldsSqlPart(this.OrmProvider, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames);
        var valuesSqlPartSetter = RepositoryHelper.BuildCreateValuesSqlParametes(this.OrmProvider, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames, this.IsMultiple);

        var fieldsBuilder = new StringBuilder();
        fieldsSqlPartSetter.Invoke(fieldsBuilder, insertObj);
        var valuesBuilder = new StringBuilder();
        if (this.IsMultiple)
        {
            var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>;
            typedValuesSqlPartSetter.Invoke(this.DbParameters, valuesBuilder, this.OrmProvider, insertObj, $"_m{this.CommandIndex}");
        }
        else
        {
            var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object>;
            typedValuesSqlPartSetter.Invoke(this.DbParameters, valuesBuilder, this.OrmProvider, insertObj);
        }
        this.InsertFields.Add(new FieldsSegment
        {
            Fields = fieldsBuilder.ToString(),
            Values = valuesBuilder.ToString()
        });
        //未明确指定分表，根据字段数据进行分表
        if (this.ShardingProvider.TryGetShardingTable(entityType, out var shardingTable) && string.IsNullOrEmpty(this.Tables[0].Body))
            this.Tables[0].Body = RepositoryHelper.GetShardingTableName(this.DbKey, this.MapProvider, this.ShardingProvider, entityType, insertObjType, insertObj);
    }
    public virtual void VisitWithByField(object deferredSegmentValue)
    {
        (var fieldSelector, var fieldValue) = ((Expression, object))deferredSegmentValue;
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var entityMapper = this.Tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        if (memberMapper.IsIgnore || memberMapper.IsIgnoreInsert)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}被忽略插入，IsIgnore：{memberMapper.IsIgnore}，IsIgnoreInsert：{memberMapper.IsIgnoreInsert}");
        if (memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}不允许插入，IsRowVersion：{memberMapper.IsRowVersion}");

        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
        var dbFieldValue = memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, memberMapper.UnderlyingType, fieldValue);
        this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue));
        this.InsertFields.Add(new FieldsSegment
        {
            Fields = this.OrmProvider.GetFieldName(memberMapper.FieldName),
            Values = parameterName
        });
    }
    public virtual List<string> VisitFields(Expression fieldsSelector)
    {
        var lambdaExpr = fieldsSelector as LambdaExpression;
        var entityMapper = this.Tables[0].Mapper;
        this.TableAliases.Clear();
        this.TableAliases.Add(lambdaExpr.Parameters[0].Name, this.Tables[0]);
        var fieldNames = new List<string>();
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.New:
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
                        continue;

                    if (newExpr.Arguments[i] is not MemberExpression memberExpr)
                        throw new NotSupportedException($"不支持的表达式访问，只支持MemberAccess访问，Path:{newExpr.Arguments[i]}");
                    fieldNames.Add(memberMapper.FieldName);
                }
                break;
            case ExpressionType.MemberInit:
                var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
                        continue;

                    if (memberAssignment.Expression is not MemberExpression memberExpr)
                        throw new NotSupportedException($"不支持的表达式访问，只支持MemberAccess访问，Path:{memberAssignment.Expression}");
                    fieldNames.Add(memberMapper.FieldName);
                }
                break;
        }
        if (fieldNames.Count > 0)
            return fieldNames;
        return null;
    }
    public virtual void Clear()
    {
        this.Tables?.Clear();
        this.TableAliases?.Clear();
        this.ReaderFields?.Clear();
        this.WhereSql = null;
        this.TableAsStart = 'a';
        this.IsNeedTableAlias = false;
        this.deferredSegments.Clear();
        this.InsertFields.Clear();
    }
    public override void Dispose()
    {
        base.Dispose();
        this.deferredSegments = null;
        this.InsertFields = null;
        this.OnlyFieldNames = null;
        this.IgnoreFieldNames = null;
    }
}
