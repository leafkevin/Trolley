using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

public class UpdateVisitor : SqlVisitor, IUpdateVisitor
{
    protected List<CommandSegment> deferredSegments = new();
    protected bool isNeedSplitShardingTables = false;
    protected TableShardingInfo tableShardingInfo = null;
    protected Dictionary<string, object> shardingDependOnValues = null;

    public List<string> OnlyFieldNames { get; set; }
    public List<string> IgnoreFieldNames { get; set; }
    public ActionMode ActionMode { get; set; }
    public bool IsFrom { get; set; }
    public bool IsJoin { get; set; }
    public List<string> UpdateFields { get; set; }
    public string FixedSql { get; set; }
    public bool HasWhere { get; protected set; }

    public UpdateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
    {
        this.DbContext = dbContext;
        this.TableAsStart = tableAsStart;
        this.Tables = new()
        {
            new TableSegment
            {
                TableType = TableType.Entity,
                EntityType = entityType,
                AliasName = "a",
                Mapper = this.MapProvider.GetEntityMap(entityType)
            }
        };
    }
    public virtual string BuildCommand(DbContext dbContext, ITheaCommand command, out List<SqlFieldSegment> readerFields)
    {
        string sql = null;
        readerFields = null;
        var builder = new StringBuilder();
        switch (this.ActionMode)
        {
            case ActionMode.Bulk:
                {
                    //此SQL只能用在多命令查询时和返回ToSql两个场景
                    (var shardingType, var shardingTables, var updateObjs, var bulkCount, var fixedSqlSetter, var loopSqlSetter, _) = this.BuildWithBulk(command);
                    int index = 0;
                    fixedSqlSetter?.Invoke(command.Parameters);
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var tableParameters = tabledUpdateObjs[tableName];
                            foreach (var updateObj in tableParameters)
                            {
                                loopSqlSetter.Invoke(command.Parameters, builder, tableName, updateObj, index);
                                index++;
                            }
                        }
                    }
                    else
                    {
                        foreach (var updateObj in updateObjs)
                        {
                            switch (shardingType)
                            {
                                case ShardingTableType.None:
                                case ShardingTableType.SingleTable:
                                    loopSqlSetter.Invoke(command.Parameters, builder, shardingTables as string, updateObj, index);
                                    break;
                                case ShardingTableType.MultiTable:
                                case ShardingTableType.ShardingTableMap:
                                    var tableNames = shardingTables as List<string>;
                                    foreach (var tableName in tableNames)
                                    {
                                        loopSqlSetter.Invoke(command.Parameters, builder, tableName, updateObj, index);
                                    }
                                    break;
                            }
                            index++;
                        }
                    }
                    sql = builder.ToString();
                }
                break;
            case ActionMode.Single:
                {
                    this.UpdateFields = new();
                    this.DbParameters ??= command.Parameters;
                    foreach (var deferredSegment in this.deferredSegments)
                    {
                        switch (deferredSegment.Type)
                        {
                            case "Set":
                                this.VisitSet(deferredSegment.Value as Expression);
                                break;
                            case "SetFrom":
                                this.IsNeedTableAlias = true;
                                this.VisitSet(deferredSegment.Value as Expression);
                                break;
                            case "SetField":
                                this.VisitSetField(deferredSegment.Value);
                                break;
                            case "SetWith":
                                this.VisitSetWith(deferredSegment.Value);
                                break;
                            case "SetFromField":
                                this.IsNeedTableAlias = true;
                                this.VisitSetFromField(deferredSegment.Value);
                                break;
                            case "Where":
                                this.VisitWhere(deferredSegment.Value as Expression);
                                break;
                            case "WhereWith":
                                this.VisitWhereWith(deferredSegment.Value);
                                break;
                            case "And":
                                this.VisitAnd(deferredSegment.Value as Expression);
                                break;
                            case "Or":
                                this.VisitOr(deferredSegment.Value as Expression);
                                break;
                        }
                    }

                    var aliasName = this.Tables[0].AliasName;
                    if (this.IsNeedTableAlias)
                        builder.Append($"{aliasName} ");

                    if (this.IsJoin)
                    {
                        for (var i = 1; i < this.Tables.Count; i++)
                        {
                            var tableSegment = this.Tables[i];
                            var tableName = this.GetTableName(tableSegment);
                            builder.Append($"{tableSegment.JoinType} {tableName} {tableSegment.AliasName}");
                            builder.Append($" ON {tableSegment.OnExpr} ");
                        }
                    }

                    int index = 0;
                    builder.Append("SET ");
                    if (this.UpdateFields.Count > 0)
                    {
                        foreach (var setField in this.UpdateFields)
                        {
                            if (index > 0) builder.Append(',');
                            if (this.IsNeedTableAlias) builder.Append($"{aliasName}.");
                            builder.Append(setField);
                            index++;
                        }
                    }
                    if (!string.IsNullOrEmpty(this.WhereSql))
                    {
                        builder.Append(" WHERE ");
                        builder.Append(this.WhereSql);
                    }
                    sql = builder.ToString();
                    builder.Clear();

                    if (this.IsJoin)
                    {
                        builder.Append($"UPDATE {this.GetTableName(this.Tables[0])} {sql}");
                        sql = builder.ToString();
                        if (this.ShardingTables != null && this.ShardingTables.Count > 0)
                            sql = dbContext.BuildShardingTablesSqlByFormat(this, sql, ";");
                    }
                    else
                    {
                        Action<string> headSqlSetter = null;
                        //处理有tableSchema的场景
                        var tableSchema = this.Tables[0].TableSchema;
                        if (!string.IsNullOrEmpty(tableSchema))
                            headSqlSetter = tableName => builder.Append($"UPDATE {this.OrmProvider.GetTableName(tableSchema + "." + tableName)} ");
                        else headSqlSetter = tableName => builder.Append($"UPDATE {this.OrmProvider.GetTableName(tableName)} ");

                        if (this.ShardingTables != null && this.ShardingTables.Count > 0)
                        {
                            var tableNames = this.ShardingTables[0].TableNames;
                            for (int i = 0; i < tableNames.Count; i++)
                            {
                                if (i > 0) builder.Append(';');
                                headSqlSetter.Invoke(tableNames[i]);
                                builder.Append(sql);
                            }
                        }
                        else
                        {
                            var tableName = this.Tables[0].Mapper.TableName;
                            headSqlSetter.Invoke(this.Tables[0].Body ?? tableName);
                            builder.Append(sql);
                        }
                        sql = builder.ToString();
                    }
                }
                break;
        }
        builder.Clear();
        return sql;
    }
    public virtual (ShardingTableType, object, IEnumerable, int, Action<IDataParameterCollection>, Action<IDataParameterCollection, StringBuilder, string, object, int>, List<SqlFieldSegment>) BuildWithBulk(ITheaCommand command)
    {
        (var updateObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;

        object firstUpdateObj = null;
        Type updateObjType = null;
        foreach (var updateObj in updateObjs)
        {
            firstUpdateObj = updateObj;
            updateObjType = updateObj.GetType();
            break;
        }

        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        var hasOnlyFields = this.OnlyFieldNames != null && this.OnlyFieldNames.Count > 0;
        var hasIgnoreFields = this.IgnoreFieldNames != null && this.IgnoreFieldNames.Count > 0;
        var valueFieldSegments = new List<ValueFieldSegment>();
        var keyFieldSegments = new List<ValueFieldSegment>();

        var headSql = "UPDATE";
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            headSql += $" {this.OrmProvider.GetTableName(tableSegment.TableSchema)}.";
        Action<IDataParameterCollection> fixedSqlSetter = null;
        Action<IDataParameterCollection, StringBuilder, string, object, int> loopSqlSetter = null;
        string fixedHeadSql = "SET ", fixedTailSql = ";";
        List<IDbDataParameter> fixedDbParameters = null;

        if (this.deferredSegments.Count > 1)
        {
            var tempDbParameters = new TheaDbParameterCollection();
            this.DbParameters = tempDbParameters;
            //先解析其他sql，生成固定sql
            for (int i = 1; i < this.deferredSegments.Count; i++)
            {
                var deferredSegment = this.deferredSegments[i];
                switch (deferredSegment.Type)
                {
                    case "Set":
                        this.VisitSet(deferredSegment.Value as Expression);
                        break;
                    case "SetField":
                        this.VisitSetField(deferredSegment.Value);
                        break;
                    case "SetWith":
                        this.VisitSetWith(deferredSegment.Value);
                        break;
                    //分区表，二级分区为时间分区，为了提高性能，增加额外的时间条件命中二级时间分区
                    case "Where":
                        this.VisitWhere(deferredSegment.Value as Expression);
                        break;
                    case "WhereWith":
                        this.VisitWhereWith(deferredSegment.Value);
                        break;
                    case "And":
                        this.VisitAnd(deferredSegment.Value as Expression);
                        break;
                    case "Or":
                        this.VisitOr(deferredSegment.Value as Expression);
                        break;
                    default: throw new NotSupportedException("SetBulk操作后，只支持Set/IgnoreFields/OnlyFields/Where/And/Or操作");
                }
            }
            if (this.DbParameters.Count > 0)
            {
                fixedDbParameters = tempDbParameters.ToList();
                fixedSqlSetter = dbParameters => fixedDbParameters.ForEach(f => dbParameters.Add(f));
            }
            if (this.UpdateFields.Count > 0)
            {
                fixedHeadSql = $"SET {string.Join(",", this.UpdateFields)},";
                if (!string.IsNullOrEmpty(this.WhereSql))
                    fixedTailSql = $" AND {this.WhereSql};";
            }
            this.DbParameters = command.Parameters;
        }

        var valueFieldsInitializer = RepositoryHelper.BuildWithBulkFilterFieldsCommandInitializer(this.DbContext, entityType, updateObjType, 2, hasOnlyFields, hasIgnoreFields)
            as Action<IDataParameterCollection, List<ValueFieldSegment>, List<ValueFieldSegment>, DbContext, List<string>, List<string>, object>;
        valueFieldsInitializer.Invoke(this.DbParameters, keyFieldSegments, valueFieldSegments, this.DbContext, this.OnlyFieldNames, this.IgnoreFieldNames, firstUpdateObj);

        loopSqlSetter = (dbParameters, builder, tableName, updateObj, index) =>
        {
            builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} SET {fixedHeadSql}");
            for (int i = 0; i < valueFieldSegments.Count; i++)
            {
                if (i > 0) builder.Append(',');

                var valueField = valueFieldSegments[i];
                var fieldName = this.OrmProvider.GetFieldName(valueField.MemberMapper.FieldName);
                var parameterName = $"{this.OrmProvider.ParameterPrefix}{valueField.MemberMapper.MemberName}{index}";
                builder.Append($"{fieldName}={parameterName}");
                var fieldValue = valueField.ValueGetter.Invoke(updateObj);
                dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, valueField.MemberMapper.NativeDbType, fieldValue));
            }
            builder.Append(" WHERE ");
            for (int i = 0; i < keyFieldSegments.Count; i++)
            {
                if (i > 0) builder.Append(" AND ");

                var keyField = keyFieldSegments[i];
                var fieldName = this.OrmProvider.GetFieldName(keyField.MemberMapper.FieldName);
                var parameterName = $"{this.OrmProvider.ParameterPrefix}{keyField.MemberMapper.MemberName}{index}";
                builder.Append($"{fieldName}={parameterName}");
                var fieldValue = keyField.ValueGetter.Invoke(updateObj);
                dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, keyField.MemberMapper.NativeDbType, fieldValue));
            }
            builder.Append(fixedTailSql);
        };

        var shardingType = ShardingTableType.None;
        object shardingTables = tableSegment.Mapper.TableName;
        if (this.ShardingProvider != null && this.ShardingProvider.TryGetTableSharding(entityType, out var tableShardingInfo))
        {
            if (tableSegment.IsSharding)
            {
                if (!string.IsNullOrEmpty(tableSegment.Body))
                {
                    shardingTables = tableSegment.Body;
                    shardingType = ShardingTableType.SingleTable;
                }
                else
                {
                    shardingTables = tableSegment.TableNames;
                    shardingType = ShardingTableType.MultiTable;
                }
            }
            else
            {
                shardingType = ShardingTableType.SplitTables;
                shardingTables = this.SplitShardingParameters(updateObjType, tableShardingInfo, updateObjs, firstUpdateObj);
            }
        }
        return (shardingType, shardingTables, updateObjs, bulkCount, fixedSqlSetter, loopSqlSetter, null);
    }
    public virtual void Join(string joinType, Type entityType, Expression joinOn)
    {
        this.IsNeedTableAlias = true;
        this.IsJoin = true;
        var lambdaExpr = joinOn as LambdaExpression;
        var aliasName = $"{(char)(this.TableAsStart + this.Tables.Count)}";
        var joinTable = new TableSegment
        {
            TableType = TableType.Entity,
            EntityType = entityType,
            Mapper = this.MapProvider.GetEntityMap(entityType),
            AliasName = aliasName,
            JoinType = joinType,
            Path = aliasName,
            IsMaster = true
        };
        this.Tables.Add(joinTable);
        this.InitTableAlias(lambdaExpr);
        joinTable.OnExpr = this.VisitConditionExpr(lambdaExpr.Body, out _);
    }
    public virtual void SetWith(object updateObj)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetWith",
            Value = updateObj
        });
    }
    public virtual void Set(Expression fieldsAssignment)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "Set",
            Value = fieldsAssignment
        });
    }
    public virtual void SetField(Expression fieldSelector, object fieldValue)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetField",
            Value = (fieldSelector, fieldValue)
        });
    }
    public virtual void SetFrom(Expression fieldsAssignment)
    {
        this.IsNeedTableAlias = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetFrom",
            Value = fieldsAssignment
        });
    }
    public virtual void SetFrom(Expression fieldSelector, Expression valueSelector)
    {
        this.IsNeedTableAlias = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetFromField",
            Value = (fieldSelector, valueSelector)
        });
    }
    public virtual void IgnoreFields(string[] fieldNames)
    {
        this.IgnoreFieldNames ??= new();
        this.IgnoreFieldNames.AddRange(fieldNames.Select(f => f.ToLower()));
    }
    public virtual void IgnoreFields(Expression fieldsSelector)
    {
        this.IgnoreFieldNames ??= new();
        this.VisitFields(fieldsSelector, f => this.IgnoreFieldNames.Add(f.FieldName.ToLower()));
    }
    public virtual void OnlyFields(string[] fieldNames)
    {
        this.OnlyFieldNames ??= new();
        this.OnlyFieldNames.AddRange(fieldNames.Select(f => f.ToLower()));
    }
    public virtual void OnlyFields(Expression fieldsSelector)
    {
        this.OnlyFieldNames ??= new();
        this.VisitFields(fieldsSelector, f => this.OnlyFieldNames.Add(f.FieldName.ToLower()));
    }
    public virtual void SetBulk(IEnumerable updateObjs, int bulkCount)
    {
        this.ActionMode = ActionMode.Bulk;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetBulk",
            Value = (updateObjs, bulkCount)
        });
        var tableSegment = this.Tables[0];
        this.isNeedSplitShardingTables = this.ShardingProvider != null && this.ShardingProvider.TryGetTableSharding(tableSegment.EntityType, out this.tableShardingInfo)
          && !tableSegment.IsSharding && tableSegment.ShardingTableGetter == null;
        if (this.isNeedSplitShardingTables && (this.tableShardingInfo.DependOnMembers == null || this.tableShardingInfo.DependOnMembers.Count == 0))
            throw new InvalidOperationException($"实体表{tableShardingInfo.EntityType.FullName}已设置分表，但未指定分表名，也未指定依赖成员，无法确定分表，原表名：{tableSegment.Mapper.TableName}");
    }
    public virtual void WhereWith(object whereObj)
    {
        this.HasWhere = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WhereWith",
            Value = whereObj
        });
    }
    public virtual void Where(Expression whereExpr)
    {
        this.HasWhere = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "Where",
            Value = whereExpr
        });
    }
    public virtual void And(Expression whereExpr)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "And",
            Value = whereExpr
        });
    }
    public virtual void Or(Expression whereExpr)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "Or",
            Value = whereExpr
        });
    }
    public override SqlFieldSegment VisitNew(SqlFieldSegment sqlSegment)
    {
        if (sqlSegment.Expression.IsParameter(out _))
            throw new NotSupportedException($"不支持的表达式访问,{sqlSegment.Expression}");
        //当作常量处理
        return sqlSegment.ChangeValue(sqlSegment.Expression.Evaluate(), true);
    }
    public override SqlFieldSegment VisitMemberInit(SqlFieldSegment sqlSegment)
    {
        if (sqlSegment.Expression.IsParameter(out _))
            throw new NotSupportedException($"不支持的表达式访问,{sqlSegment.Expression}");
        //当作常量处理
        return sqlSegment.ChangeValue(sqlSegment.Expression.Evaluate(), true);
    }
    public override SqlFieldSegment VisitMethodCall(SqlFieldSegment sqlSegment)
    {
        //把方法返回值当作常量处理
        sqlSegment = base.VisitMethodCall(sqlSegment);
        if (!sqlSegment.HasField && !sqlSegment.HasParameter && !sqlSegment.IsMethodCall)
            sqlSegment.IsConstant = true;
        return sqlSegment;
    }
    public virtual void Clear()
    {
        this.Tables?.Clear();
        this.TableAliases?.Clear();
        this.ReaderFields?.Clear();
        this.WhereSql = null;
        this.IsFromQuery = false;
        this.TableAsStart = 'a';
        this.IsNeedTableAlias = false;

        this.IsFrom = false;
        this.IsJoin = false;
        this.deferredSegments.Clear();
        this.UpdateFields.Clear();
        this.FixedSql = null;
    }
    public override void Dispose()
    {
        base.Dispose();
        this.deferredSegments = null;
        this.UpdateFields = null;
        this.FixedSql = null;
        this.OnlyFieldNames = null;
        this.IgnoreFieldNames = null;
    }
    public virtual void VisitSetField(object deferredSegmentValue)
    {
        (var fieldSelector, var fieldValue) = ((Expression, object))deferredSegmentValue;
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = this.EnsureMemberVisit(lambdaExpr.Body) as MemberExpression;
        var entityMapper = this.Tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        if (memberMapper.IsIgnore || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}被忽略更新，IsIgnore：{memberMapper.IsIgnore}，IsIgnoreUpdate：{memberMapper.IsIgnoreUpdate}");
        if (memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}不允许更新，IsRowVersion：{memberMapper.IsRowVersion}");

        this.AddMemberElement(memberMapper, fieldValue, false);
    }
    public virtual void VisitSetWith(object updateObj)
    {
        var entityType = this.Tables[0].Mapper.EntityType;
        var updateObjType = updateObj.GetType();
        //单独更新多个字段，通过一个多字段实体类型，都当作单个实体类型处理
        var hasOnlyFields = this.OnlyFieldNames != null && this.OnlyFieldNames.Count > 0;
        var hasIgnoreFields = this.IgnoreFieldNames != null && this.IgnoreFieldNames.Count > 0;

        var commandInitializer = RepositoryHelper.BuildWithFilterFieldsCommandInitializer(this.DbContext, entityType, updateObjType, 2, hasOnlyFields, hasIgnoreFields)
            as Action<IDataParameterCollection, List<string>, DbContext, List<string>, List<string>, object>;
        commandInitializer.Invoke(this.DbParameters, this.UpdateFields, this.DbContext, this.OnlyFieldNames, this.IgnoreFieldNames, updateObj);
    }
    public virtual void VisitSet(Expression fieldsAssignment)
    {
        var entityMapper = this.Tables[0].Mapper;
        var lambdaExpr = fieldsAssignment as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
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

                    var argumentExpr = newExpr.Arguments[i];
                    if (argumentExpr.GetParameters(out var argumentParameters)
                        && argumentParameters.Exists(f => f.Type == typeof(IFromQuery)))
                    {
                        var newLambdaExpr = Expression.Lambda(argumentExpr, lambdaExpr.Parameters.ToList());
                        (var sql, _, _) = this.VisitFromQuery(newLambdaExpr);
                        this.UpdateFields.Add($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}=({sql})");
                    }
                    else
                    {
                        var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = argumentExpr });
                        //只一个成员访问，没有设置语句，什么也不做，忽略
                        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberInfo.Name)
                            continue;
                        this.AddMemberElement(sqlSegment, memberMapper);
                    }
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

                    var argumentExpr = memberAssignment.Expression;
                    if (argumentExpr.GetParameters(out var argumentParameters)
                        && argumentParameters.Exists(f => f.Type == typeof(IFromQuery)))
                    {
                        var newLambdaExpr = Expression.Lambda(argumentExpr, lambdaExpr.Parameters.ToList());
                        (var sql, _, _) = this.VisitFromQuery(newLambdaExpr);
                        this.UpdateFields.Add($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}=({sql})");
                    }
                    else
                    {
                        var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = argumentExpr });
                        //只一个成员访问，没有设置语句，什么也不做，忽略
                        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberAssignment.Member.Name)
                            continue;
                        this.AddMemberElement(sqlSegment, memberMapper);
                    }
                }
                break;
        }
    }
    public virtual void VisitSetFromField(object deferredSegmentValue)
    {
        var entityMapper = this.Tables[0].Mapper;
        (var fieldSelector, var valueSelector) = ((Expression, Expression))deferredSegmentValue;
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = this.EnsureMemberVisit(lambdaExpr.Body) as MemberExpression;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);

        if (memberMapper.IsIgnore || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}被忽略更新，IsIgnore：{memberMapper.IsIgnore}，IsIgnoreUpdate：{memberMapper.IsIgnoreUpdate}");
        if (memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}不允许更新，IsRowVersion：{memberMapper.IsRowVersion}");

        this.InitTableAlias(valueSelector as LambdaExpression);
        (var sql, _, _) = this.VisitFromQuery(valueSelector as LambdaExpression);
        this.UpdateFields.Add($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}=({sql})");
    }
    public virtual void VisitWhereWith(object whereObj)
    {
        var entityType = this.Tables[0].EntityType;
        var whereObjType = whereObj.GetType();
        var whereSqlSetter = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, whereObjType, true, false, true)
            as Func<IDataParameterCollection, DbContext, object, string>;
        var conditionSql = whereSqlSetter.Invoke(this.DbParameters, this.DbContext, whereObj);
        if (string.IsNullOrEmpty(this.WhereSql))
        {
            this.WhereSql = conditionSql;
            this.LastWhereOperationType = OperationType.None;
        }
        else
        {
            if (this.LastWhereOperationType == OperationType.Or)
                this.WhereSql = $"({this.WhereSql})";
            this.WhereSql += " AND " + conditionSql;
            this.LastWhereOperationType = OperationType.And;
        }
    }
    public virtual void VisitWhere(Expression whereExpr)
    {
        if (!string.IsNullOrEmpty(this.WhereSql))
        {
            this.VisitAnd(whereExpr);
            return;
        }
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.LastWhereOperationType = OperationType.None;
        this.WhereSql = this.VisitConditionExpr(lambdaExpr.Body, out var operationType);
        this.LastWhereOperationType = operationType;
        this.IsWhere = false;
    }
    public virtual void VisitAnd(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        var conditionSql = this.VisitConditionExpr(lambdaExpr.Body, out var operationType);
        if (string.IsNullOrEmpty(this.WhereSql))
        {
            this.WhereSql = conditionSql;
            this.LastWhereOperationType = operationType;
        }
        else
        {
            if (this.LastWhereOperationType == OperationType.Or)
                this.WhereSql = $"({this.WhereSql})";
            if (operationType == OperationType.Or)
                conditionSql = $"({conditionSql})";
            this.WhereSql += " AND " + conditionSql;
            this.LastWhereOperationType = OperationType.And;
        }
        this.IsWhere = false;
    }
    public virtual void VisitOr(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        var conditionSql = this.VisitConditionExpr(lambdaExpr.Body, out var operationType);
        if (string.IsNullOrEmpty(this.WhereSql))
        {
            this.WhereSql = conditionSql;
            this.LastWhereOperationType = operationType;
        }
        else
        {
            if (this.LastWhereOperationType == OperationType.And)
                this.WhereSql = $"({this.WhereSql})";
            if (operationType == OperationType.And)
                conditionSql = $"({conditionSql})";
            this.WhereSql += " OR " + conditionSql;
            this.LastWhereOperationType = OperationType.Or;
        }
        this.IsWhere = false;
    }
    public virtual void VisitFields(Expression fieldsSelector, Action<MemberMap> fieldsAction)
    {
        var lambdaExpr = fieldsSelector as LambdaExpression;
        var entityMapper = this.Tables[0].Mapper;
        MemberMap memberMapper = null;
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                var memberExpr = lambdaExpr.Body as MemberExpression;
                memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
                fieldsAction.Invoke(memberMapper);
                break;
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out memberMapper))
                        continue;

                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment
                    {
                        Expression = newExpr.Arguments[i],
                        NativeDbType = memberMapper.NativeDbType,
                        TypeHandler = memberMapper.TypeHandler
                    });
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberInfo.Name)
                        fieldsAction.Invoke(memberMapper);
                }
                break;
            case ExpressionType.MemberInit:
                this.InitTableAlias(lambdaExpr);
                var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out memberMapper))
                        continue;

                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberAssignment.Expression });
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberAssignment.Member.Name)
                        fieldsAction.Invoke(memberMapper);
                }
                break;
        }
    }
    public virtual void AddMemberElement(MemberMap memberMapper, object memberValue, bool isEntity = true)
    {
        if (memberValue is DBNull || memberValue == null)
        {
            this.UpdateFields.Add($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}=NULL");
            return;
        }
        var fieldValue = isEntity ? memberMapper.Member.Evaluate(memberValue) : memberValue;
        if (this.isNeedSplitShardingTables && this.tableShardingInfo.DependOnMembers.Contains(memberMapper.MemberName))
            this.shardingDependOnValues[memberMapper.MemberName] = fieldValue;

        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        if (memberMapper.TypeHandler != null)
            fieldValue = memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, fieldValue);
        else
        {
            var targetType = this.OrmProvider.MapDefaultType(memberMapper);
            var valueGetter = this.OrmProvider.GetParameterValueGetter(memberValue.GetType(), targetType, false, this.DbContext);
            fieldValue = valueGetter.Invoke(fieldValue);
        }
        this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
        this.UpdateFields.Add($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
    }
    public virtual void AddMemberElement(SqlFieldSegment sqlSegment, MemberMap memberMapper)
    {
        if (sqlSegment == SqlFieldSegment.Null)
        {
            this.UpdateFields.Add($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}=NULL");
            return;
        }
        if (sqlSegment.IsConstant || sqlSegment.IsVariable)
        {
            var fieldValue = sqlSegment.Value;
            if (this.isNeedSplitShardingTables && this.tableShardingInfo.DependOnMembers.Contains(memberMapper.MemberName))
                this.shardingDependOnValues[memberMapper.MemberName] = sqlSegment.Value;

            var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
            if (memberMapper.TypeHandler != null)
                fieldValue = memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, fieldValue);
            else
            {
                var targetType = this.OrmProvider.MapDefaultType(memberMapper);
                var valueGetter = this.OrmProvider.GetParameterValueGetter(sqlSegment.SegmentType, targetType, !memberMapper.IsRequired, this.DbContext);
                fieldValue = valueGetter.Invoke(fieldValue);
            }
            this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
            this.UpdateFields.Add($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
        }
    }
    public Dictionary<string, List<object>> SplitShardingParameters(Type updateObjType, TableShardingInfo tableShardingInfo, IEnumerable updateObjs, object sampleObj)
    {
        var result = new Dictionary<string, List<object>>();
        var origTableName = this.Tables[0].Mapper.TableName;

        //优先使用本次设置的分表名获取委托来获取分表名
        if (this.Tables[0].ShardingTableGetter != null)
        {
            var tableNameGetter = this.Tables[0].ShardingTableGetter;
            foreach (var updateObj in updateObjs)
            {
                var tableName = tableNameGetter.DynamicInvoke(updateObj) as string;
                if (string.IsNullOrEmpty(tableName))
                {
                    var jsonTypeHandler = this.OrmProvider.GetTypeHandler(typeof(JsonTypeHandler));
                    throw new InvalidOperationException($"手动设置的分表名获取委托无法获取分表名，原表名：{origTableName}，当前参数：{jsonTypeHandler.ToFieldValue(this.OrmProvider, updateObj)}");
                }
                if (!result.TryGetValue(tableName, out var myParameters))
                    result.Add(tableName, myParameters = new List<object>());
                myParameters.Add(updateObj);
            }
        }
        else
        {
            //使用分表规则获取分表名，根据依赖的字段值执行分表规则委托获取分表名
            if (tableShardingInfo.DependOnMembers == null || tableShardingInfo.DependOnMembers.Count == 0)
                throw new InvalidOperationException($"实体表{tableShardingInfo.EntityType.FullName}已设置分表，但未指定分表名，也未指定依赖的成员，无法确定分表，原表名：{origTableName}");

            var fieldValueGetters = new List<Func<object, object>>();
            bool TryAddMemberGetter(string memberName)
            {
                for (int i = 1; i < this.deferredSegments.Count; i++)
                {
                    var deferredSegment = this.deferredSegments[i];
                    switch (deferredSegment.Type)
                    {
                        case "Set":
                            this.VisitSet(deferredSegment.Value as Expression);
                            break;
                        case "SetField":
                            (var fieldSelector, var fieldValue) = ((Expression, object))deferredSegment.Value;
                            var lambdaExpr = fieldSelector as LambdaExpression;
                            var memberExpr = this.EnsureMem berVisit(lambdaExpr.Body) as MemberExpression;
                            if (memberExpr.Member.Name == memberName)
                            {
                                fieldValueGetters.Add(f => fieldValue);
                                return true;
                            }
                            break;
                        case "SetWith":
                            var updateObj = deferredSegment.Value;
                            var myInsertObjType = updateObj.GetType();
                            if (RepositoryHelper.TryGetMemberGetter(myInsertObjType, memberName.ToLower(), updateObj, out var memberGetter))
                            {
                                fieldValueGetters.Add(f => memberGetter.Invoke(updateObj));
                                return true;
                            }
                            break;
                        //分区表，二级分区为时间分区，为了提高性能，增加额外的时间条件命中二级时间分区
                        case "Where":
                            this.VisitWhere(deferredSegment.Value as Expression);
                            break;
                        case "WhereWith":
                            this.VisitWhereWith(deferredSegment.Value);
                            break;
                        case "And":
                            this.VisitAnd(deferredSegment.Value as Expression);
                            break;
                        case "Or":
                            this.VisitOr(deferredSegment.Value as Expression);
                            break;
                        default: throw new NotSupportedException("SetBulk操作后，只支持Set/IgnoreFields/OnlyFields/Where/And/Or操作");


                        case "WithByField":
                            (var fieldSelector, var fieldValue) = ((Expression, object))deferredSegment.Value;
                            var lambdaExpr = fieldSelector as LambdaExpression;
                            var memberExpr = this.EnsureMemberVisit(lambdaExpr.Body) as MemberExpression;
                            if (memberExpr.Member.Name == memberName)
                            {
                                fieldValueGetters.Add(f => fieldValue);
                                return true;
                            }
                            break;
                    }
                }
                return false;
            }

            foreach (var memberName in tableShardingInfo.DependOnMembers)
            {
                if (RepositoryHelper.TryGetMemberGetter(updateObjType, memberName.ToLower(), sampleObj, out var memberGetter))
                {
                    fieldValueGetters.Add(memberGetter);
                    continue;
                }
                if (!TryAddMemberGetter(memberName))
                    throw new InvalidOperationException($"实体表{tableShardingInfo.EntityType.FullName}已设置分表，依赖的成员{memberName}在插入对象类型{updateObjType.FullName}中不存在，无法确定分表，原表名：{origTableName}");
            }
            Func<object, string> tableNameGetter = insertObj =>
            {
                var fieldValus = new List<object>();
                foreach (var fieldValueGetter in fieldValueGetters)
                    fieldValus.Add(fieldValueGetter.Invoke(insertObj));
                return tableShardingInfo.Rule.Invoke(origTableName, fieldValus.ToArray()) as string;
            };

            foreach (var insertObj in updateObjs)
            {
                var tableName = tableNameGetter.Invoke(insertObj);
                if (string.IsNullOrEmpty(tableName))
                    throw new InvalidOperationException($"分表规则无法获取分表名，原表名：{origTableName}，当前参数：{this.DbContext.JsonTypeHandler.ToFieldValue(this.OrmProvider, insertObj)}");
                if (!result.TryGetValue(tableName, out var myParameters))
                    result.Add(tableName, myParameters = new List<object>());
                myParameters.Add(insertObj);
            }
        }
        return result;
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
            if (typeof(IAggregateSelect).IsAssignableFrom(parameterExpr.Type))
                continue;
            if (typeof(IFromQuery).IsAssignableFrom(parameterExpr.Type))
                continue;
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
}