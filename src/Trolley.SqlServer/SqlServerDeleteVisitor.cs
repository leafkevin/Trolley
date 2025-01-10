using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.SqlServer;

public class SqlServerDeleteVisitor : DeleteVisitor
{
    private static readonly ConcurrentDictionary<int, (bool, string, Action<StringBuilder, string, string>, object)> deleteCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, (bool, string, Action<StringBuilder, string, string>, object)> deleteMultiCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, (bool, string, Action<StringBuilder, string, string>, object)> deleteBulkCommandInitializerCache = new();

    public bool IsOutput { get; set; }
    public string OutputSql { get; set; }

    public SqlServerDeleteVisitor(DbContext dbContext, char tableAsStart = 'a')
        : base(dbContext, tableAsStart) { }

    public override string BuildCommand(ITheaCommand command, out List<SqlFieldSegment> readerFields)
    {
        string sql = null;
        this.DbParameters ??= command.Parameters;
        foreach (var deferredSegment in this.deferredSegments)
        {
            switch (deferredSegment.Type)
            {
                case "Where":
                    this.VisitWhere(deferredSegment.Value as Expression);
                    break;
                case "And":
                    this.VisitAnd(deferredSegment.Value as Expression);
                    break;
                case "OutputFields":
                    this.VisitOutputFields(deferredSegment.Value as string);
                    break;
                case "OutputExpression":
                    this.VisitOutputExpression(deferredSegment.Value as LambdaExpression);
                    break;
            }
        }
        readerFields = this.ReaderFields;
        var isOutputSql = !string.IsNullOrEmpty(this.OutputSql);

        if (this.IsWhereKeys)
        {
            var entityType = this.Tables[0].EntityType;
            var whereKeys = this.deferredSegments[0].Value;
            Type whereObjType = null;
            var isBulk = whereKeys is IEnumerable && whereKeys is not string && whereKeys is not IDictionary<string, object>;
            IEnumerable entities = null;
            if (isBulk)
            {
                entities = whereKeys as IEnumerable;
                foreach (var entity in entities)
                {
                    whereObjType = entity.GetType();
                    break;
                }
            }
            else whereObjType = whereKeys.GetType();
            (var isMultiKeys, var origName, var headSqlSetter, var whereSqlSetter) = BuildDeleteCommandInitializer(this.DbContext, entityType, whereObjType, this.IsMultiple, isBulk, isOutputSql);
            int index = 0;
            var builder = new StringBuilder();
            var whereSqlBuilder = new StringBuilder();

            Action sqlExecuter = null;
            if (isBulk)
            {
                Func<int, string> suffixGetter = index => this.IsMultiple ? $"_m{this.CommandIndex}{index}" : $"{index}";
                var typedWhereSqlSetter = whereSqlSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
                sqlExecuter = () =>
                {
                    var jointMark = isMultiKeys ? " OR " : ",";
                    foreach (var entity in entities)
                    {
                        if (index > 0) whereSqlBuilder.Append(jointMark);
                        typedWhereSqlSetter.Invoke(command.Parameters, whereSqlBuilder, this.DbContext, entity, suffixGetter.Invoke(index));
                        index++;
                    }
                    if (!isMultiKeys) whereSqlBuilder.Append(')');
                };
            }
            else
            {
                if (this.IsMultiple)
                {
                    var typedWhereSqlSetter = whereSqlSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
                    sqlExecuter = () => typedWhereSqlSetter.Invoke(command.Parameters, whereSqlBuilder, this.DbContext, whereKeys, $"_m{this.CommandIndex}");
                }
                else
                {
                    var typedWhereSqlSetter = whereSqlSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object>;
                    sqlExecuter = () => typedWhereSqlSetter.Invoke(command.Parameters, whereSqlBuilder, this.DbContext, whereKeys);
                }
            }
            if (!string.IsNullOrEmpty(this.Tables[0].TableSchema))
                headSqlSetter = (builder, tableName, outputSql) => headSqlSetter.Invoke(builder, this.Tables[0].TableSchema + "." + tableName, outputSql);
            if (this.ShardingTables != null && this.ShardingTables.Count > 0)
            {
                var tableNames = this.ShardingTables[0].TableNames;
                sqlExecuter.Invoke();
                for (int i = 0; i < tableNames.Count; i++)
                {
                    if (i > 0) builder.Append(';');
                    headSqlSetter.Invoke(builder, tableNames[i], this.OutputSql);
                    builder.Append(whereSqlBuilder);
                }
            }
            else
            {
                sqlExecuter.Invoke();
                headSqlSetter.Invoke(builder, this.Tables[0].Body ?? origName, this.OutputSql);
                builder.Append(whereSqlBuilder);
            }
            sql = builder.ToString();
            builder.Clear();
            whereSqlBuilder.Clear();
        }
        else
        {
            var builder = new StringBuilder();
            if (this.ShardingTables != null && this.ShardingTables.Count > 0)
            {
                var tableSegment = this.ShardingTables[0];
                var tableNames = tableSegment.TableNames;
                for (int i = 0; i < tableNames.Count; i++)
                {
                    if (i > 0) builder.Append(';');
                    builder.Append("DELETE FROM ");
                    builder.Append(this.OrmProvider.GetTableName(tableNames[i]));
                    if (isOutputSql) builder.Append(this.OutputSql);
                    builder.Append(" WHERE ");
                    builder.Append(this.WhereSql);
                }
            }
            else
            {
                var tableName = this.Tables[0].Body ?? this.Tables[0].Mapper.TableName;
                builder.Append($"DELETE FROM {this.OrmProvider.GetTableName(tableName)}");
                if (isOutputSql) builder.Append(this.OutputSql);
                builder.Append(" WHERE ");
                builder.Append(this.WhereSql);
            }
            sql = builder.ToString();
        }
        return sql;
    }
    public override string BuildTableShardingsSql()
    {
        var builder = new StringBuilder($"SELECT a.name FROM sys.objects a,sys.schemas b WHERE a.schema_id=b.schema_id AND A.type='U' AND ");
        var schemaBuilders = new Dictionary<string, StringBuilder>();
        foreach (var tableSegment in this.ShardingTables)
        {
            if (tableSegment.ShardingType > ShardingTableType.MultiTable)
            {
                var tableSchema = tableSegment.TableSchema ?? this.DefaultTableSchema;
                if (!schemaBuilders.TryGetValue(tableSchema, out var tableBuilder))
                    schemaBuilders.Add(tableSchema, tableBuilder = new StringBuilder());

                if (tableBuilder.Length > 0) tableBuilder.Append(" OR ");
                tableBuilder.Append($"a.name LIKE '{tableSegment.Mapper.TableName}%'");
            }
        }
        if (schemaBuilders.Count > 1)
            builder.Append('(');
        int index = 0;
        foreach (var schemaBuilder in schemaBuilders)
        {
            if (index > 0) builder.Append(" OR ");
            builder.Append($"b.name='{schemaBuilder.Key}' AND ({schemaBuilder.Value.ToString()})");
            index++;
        }
        if (schemaBuilders.Count > 1)
            builder.Append(')');
        return builder.ToString();
    }
    public override SqlFieldSegment VisitMemberAccess(SqlFieldSegment sqlSegment)
    {
        var memberExpr = sqlSegment.Expression as MemberExpression;
        MemberAccessSqlFormatter formatter = null;
        if (memberExpr.Expression != null)
        {
            //Where(f=>... && !f.OrderId.HasValue && ...)
            //Where(f=>... f.OrderId.Value==10 && ...)
            //Select(f=>... ,f.OrderId.HasValue  ...)
            //Select(f=>... ,f.OrderId.Value==10  ...)
            if (Nullable.GetUnderlyingType(memberExpr.Member.DeclaringType) != null)
            {
                if (memberExpr.Member.Name == nameof(Nullable<bool>.HasValue))
                {
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlFieldSegment.Null });
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                    return this.Visit(sqlSegment.Next(memberExpr.Expression));
                }
                else if (memberExpr.Member.Name == nameof(Nullable<bool>.Value))
                    return this.Visit(sqlSegment.Next(memberExpr.Expression));
                else throw new ArgumentException($"不支持的MemberAccess操作，表达式'{memberExpr}'返回值不是boolean类型");
            }

            //各种类型实例成员访问，如：DateTime,TimeSpan,String.Length,List.Count
            if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            {
                //Where(f=>... && f.CreatedAt.Month<5 && ...)
                //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
                var targetSegment = sqlSegment.Next(memberExpr.Expression);
                sqlSegment = formatter.Invoke(this, targetSegment);
                sqlSegment.SegmentType = memberExpr.Type;
                return sqlSegment;
            }

            if (memberExpr.IsParameter(out _))
            {
                //Where(f => f.Amount > 5)
                //Select(f => new { f.OrderId, f.Disputes ...})
                var tableSegment = this.Tables[0];
                var memberMapper = tableSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                if (memberMapper.IsIgnore)
                    throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberMapper.MemberName}是忽略成员无法访问");
                if (memberMapper.MemberType.IsEntityType(out _) && !memberMapper.IsNavigation && memberMapper.TypeHandler == null)
                    throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberExpr.Member.Name}不是值类型，未配置为导航属性也没有配置TypeHandler");

                var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                sqlSegment.HasField = true;
                sqlSegment.TableSegment = tableSegment;
                sqlSegment.FromMember = memberMapper.Member;
                sqlSegment.SegmentType = memberMapper.MemberType;
                if (memberMapper.UnderlyingType.IsEnum)
                    sqlSegment.ExpectType = memberMapper.UnderlyingType;
                sqlSegment.NativeDbType = memberMapper.NativeDbType;
                sqlSegment.TypeHandler = memberMapper.TypeHandler;
                if (this.IsOutput) fieldName = "DELETED." + fieldName;
                else if (this.IsNeedTableAlias) fieldName = tableSegment.AliasName + "." + fieldName;
                sqlSegment.Body = fieldName;
                return sqlSegment;
            }
        }

        if (memberExpr.Member.DeclaringType == typeof(DBNull))
            return SqlFieldSegment.Null;

        //各种静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
        {
            sqlSegment = formatter.Invoke(this, sqlSegment);
            sqlSegment.SegmentType = memberExpr.Type;
            return sqlSegment;
        }

        //访问局部变量或是成员变量，当作常量处理，直接计算，后面统一做参数化处理
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId)); orderIds
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        this.Evaluate(sqlSegment);

        //这里不做参数化，后面统一走参数化处理
        sqlSegment.IsConstant = false;
        sqlSegment.IsVariable = true;
        sqlSegment.SegmentType = memberExpr.Type;
        return sqlSegment;
    }
    public override SqlFieldSegment VisitNew(SqlFieldSegment sqlSegment)
    {
        if (this.IsOutput)
        {
            var lambdaExpr = sqlSegment.Expression as LambdaExpression;
            var newExpr = lambdaExpr.Body as NewExpression;
            if (newExpr.Type.Name.StartsWith("<>"))
            {
                this.InitTableAlias(sqlSegment.Expression as LambdaExpression);
                var readerFields = new List<SqlFieldSegment>();
                this.ReaderFields = readerFields;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    var fieldSqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = newExpr.Arguments[i] });
                    this.GetQuotedValue(fieldSqlSegment, true);
                    if (fieldSqlSegment.IsConstant || fieldSqlSegment.IsVariable || fieldSqlSegment.HasParameter || fieldSqlSegment.IsExpression
                        || fieldSqlSegment.IsMethodCall || fieldSqlSegment.FromMember != null && fieldSqlSegment.FromMember.Name != memberInfo.Name)
                        fieldSqlSegment.IsNeedAlias = true;
                    fieldSqlSegment.TargetMember = memberInfo;
                    fieldSqlSegment.SegmentType = memberInfo.GetMemberType();
                    readerFields.Add(fieldSqlSegment);
                }
                return sqlSegment.ChangeValue(readerFields);
            }
        }
        return sqlSegment.ChangeValue(sqlSegment.Expression.Evaluate(), true);
    }
    public override SqlFieldSegment VisitMemberInit(SqlFieldSegment sqlSegment)
    {
        if (this.IsOutput)
        {
            var lambdaExpr = sqlSegment.Expression as LambdaExpression;
            var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
            var readerFields = new List<SqlFieldSegment>();
            this.ReaderFields = readerFields;
            for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
            {
                if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                    throw new NotSupportedException("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");
                var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                var memberInfo = memberAssignment.Member;
                var fieldSqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberAssignment.Expression });
                this.GetQuotedValue(fieldSqlSegment, true);
                if (fieldSqlSegment.IsConstant || fieldSqlSegment.IsVariable || fieldSqlSegment.HasParameter || fieldSqlSegment.IsExpression
                    || fieldSqlSegment.IsMethodCall || fieldSqlSegment.FromMember != null && fieldSqlSegment.FromMember.Name != memberInfo.Name)
                    fieldSqlSegment.IsNeedAlias = true;
                fieldSqlSegment.TargetMember = memberInfo;
                fieldSqlSegment.SegmentType = memberInfo.GetMemberType();
                readerFields.Add(fieldSqlSegment);
            }
            return sqlSegment.ChangeValue(readerFields);
        }
        return sqlSegment.ChangeValue(sqlSegment.Expression.Evaluate(), true);
    }
    public void Output(string fieldNames)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "OutputFields",
            Value = fieldNames
        });
    }
    public void Output(Expression fieldsSelector)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "OutputExpression",
            Value = fieldsSelector
        });
    }
    public void VisitOutputFields(string fieldNames)
    {
        this.ReaderFields = new();
        var entityType = this.Tables[0].EntityType;
        var upperFieldNames = fieldNames.ToUpper();
        if (fieldNames == "*")
        {
            upperFieldNames = "DELETED.*";
            fieldNames = "DELETED.*";
        }
        this.OutputSql = $" OUTPUT {fieldNames}";

        if (upperFieldNames == "DELETED.*")
        {
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
    }
    public void VisitOutputExpression(LambdaExpression fieldsSelector)
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
            default:
                this.VisitAndDeferred(new SqlFieldSegment { Expression = fieldsSelector });
                for (int i = 0; i < this.ReaderFields.Count; i++)
                {
                    var readerField = this.ReaderFields[i];
                    if (i > 0) builder.Append(',');
                    builder.Append(readerField.Body);
                    if (readerField.IsNeedAlias)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(readerField.TargetMember.Name)}");
                }
                break;
        }
        this.OutputSql = builder.ToString();
        this.IsOutput = false;
        builder.Clear();
    }
    public virtual void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.TableAliases.Clear();
        lambdaExpr.Body.GetParameterNames(out var parameters);
        if (parameters == null || parameters.Count == 0)
            return;
        int index = 0;
        foreach (var parameterExpr in lambdaExpr.Parameters)
        {
            if (!parameters.Contains(parameterExpr.Name))
            {
                index++;
                continue;
            }
            if (this.TableAliases.ContainsKey(parameterExpr.Name))
                continue;
            this.TableAliases.Add(parameterExpr.Name, this.Tables[index]);
            index++;
        }
    }
    public static (bool, string, Action<StringBuilder, string, string>, object) BuildDeleteCommandInitializer(DbContext dbContext, Type entityType, Type whereObjType, bool isMultiple, bool isBulk, bool hasFixedSql = false)
    {
        var ormProvider = dbContext.OrmProvider;
        var mapProvider = dbContext.MapProvider;
        var cacheKey = RepositoryHelper.GetCacheKey(ormProvider.OrmProviderType, mapProvider, entityType, whereObjType, isMultiple, isBulk, hasFixedSql);
        var commandInitializerCache = isBulk ? deleteBulkCommandInitializerCache : isMultiple ? deleteMultiCommandInitializerCache : deleteCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var isMultiKeys = entityMapper.KeyMembers.Count > 1;
            Action<StringBuilder, string, string> headSqlSetter = null;
            bool isInExpr = isBulk && !isMultiKeys;
            if (hasFixedSql)
            {
                if (isInExpr)
                    headSqlSetter = (builder, tableName, fixedSql) => builder.Append($"DELETE FROM {ormProvider.GetTableName(tableName)}{fixedSql} WHERE {ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)} IN (");
                else headSqlSetter = (builder, tableName, fixedSql) => builder.Append($"DELETE FROM {ormProvider.GetTableName(tableName)}{fixedSql} WHERE ");
            }
            else
            {
                if (isInExpr)
                    headSqlSetter = (builder, tableName, fixedSql) => builder.Append($"DELETE FROM {ormProvider.GetTableName(tableName)} WHERE {ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)} IN (");
                else headSqlSetter = (builder, tableName, fixedSql) => builder.Append($"DELETE FROM {ormProvider.GetTableName(tableName)} WHERE ");
            }
            var whereSqlParametersSetter = RepositoryHelper.BuildWhereSqlParametersPart(dbContext, entityType, whereObjType, 1, false, true, false, isInExpr, isMultiple, isBulk);
            var tableName = entityMapper.TableName;
            return (isMultiKeys, tableName, headSqlSetter, whereSqlParametersSetter);
        });
    }
}