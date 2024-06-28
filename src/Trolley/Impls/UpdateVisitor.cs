using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class UpdateVisitor : SqlVisitor, IUpdateVisitor
{
    protected List<CommandSegment> deferredSegments = new();

    public List<string> OnlyFieldNames { get; set; }
    public List<string> IgnoreFieldNames { get; set; }
    public ActionMode ActionMode { get; set; }
    public bool IsFrom { get; set; }
    public bool IsJoin { get; set; }
    public List<string> UpdateFields { get; set; }
    public string FixedSql { get; set; }
    public bool HasWhere { get; protected set; }
    public TheaDbParameterCollection FixedDbParameters { get; set; }

    public UpdateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
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
                TableType = TableType.Entity,
                EntityType = entityType,
                AliasName = "a",
                Mapper = this.MapProvider.GetEntityMap(entityType)
            });
        }
        if (!isFirst) this.Clear();
    }
    public virtual string BuildCommand(DbContext dbContext, IDbCommand command)
    {
        string sql = null;
        var builder = new StringBuilder();
        switch (this.ActionMode)
        {
            case ActionMode.Bulk:
                {
                    //此SQL只能用在多命令查询时和返回ToSql两个场景
                    (var updateObjs, var bulkCount, var tableName, var firstParametersSetter,
                        var firstSqlParametersSetter, var headSqlSetter, var sqlSetter) = this.BuildWithBulk(command);
                    Func<int, string> suffixGetter = index => this.IsMultiple ? $"_m{this.CommandIndex}{index}" : $"{index}";

                    Action<object, int> sqlExecuter = null;
                    if (this.ShardingTables != null && this.ShardingTables.Count > 0)
                    {
                        sqlExecuter = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            var tableNames = this.ShardingTables[0].TableNames;
                            headSqlSetter.Invoke(builder, tableNames[0]);
                            firstSqlParametersSetter.Invoke(command.Parameters, builder, this.OrmProvider, updateObj, suffixGetter.Invoke(index));

                            for (int i = 1; i < tableNames.Count; i++)
                            {
                                builder.Append(';');
                                headSqlSetter.Invoke(builder, tableNames[i]);
                                sqlSetter.Invoke(builder, this.OrmProvider, updateObj, suffixGetter.Invoke(index));
                            }
                        };
                    }
                    else
                    {
                        sqlExecuter = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            headSqlSetter.Invoke(builder, tableName);
                            firstSqlParametersSetter.Invoke(command.Parameters, builder, this.OrmProvider, updateObj, suffixGetter.Invoke(index));
                        };
                    }

                    int index = 0;
                    firstParametersSetter?.Invoke(command.Parameters);
                    foreach (var updateObj in updateObjs)
                    {
                        sqlExecuter.Invoke(updateObj, index);
                        index++;
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
                    if (this.UpdateFields != null && this.UpdateFields.Count > 0)
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
                        Action<string> headSqlSetter = tableName => builder.Append($"UPDATE {this.OrmProvider.GetTableName(tableName)} ");
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
        builder = null;
        return sql;
    }
    public virtual MultipleCommand CreateMultipleCommand()
    {
        return new MultipleCommand
        {
            CommandType = MultipleCommandType.Update,
            EntityType = this.Tables[0].EntityType,
            Body = this.deferredSegments,
            Tables = this.Tables,
            IgnoreFieldNames = this.IgnoreFieldNames,
            OnlyFieldNames = this.OnlyFieldNames,
            RefQueries = this.RefQueries,
            IsNeedTableAlias = this.IsNeedTableAlias,
            IsJoin = this.IsJoin
        };
    }
    public virtual void BuildMultiCommand(DbContext dbContext, IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex)
    {
        this.IsMultiple = true;
        this.CommandIndex = commandIndex;
        this.deferredSegments = multiCommand.Body as List<CommandSegment>;
        this.Tables = multiCommand.Tables;
        this.IgnoreFieldNames = multiCommand.IgnoreFieldNames;
        this.OnlyFieldNames = multiCommand.OnlyFieldNames;
        this.RefQueries = multiCommand.RefQueries;
        this.IsJoin = multiCommand.IsJoin;
        this.IsNeedTableAlias = multiCommand.IsNeedTableAlias;
        if (sqlBuilder.Length > 0) sqlBuilder.Append(';');
        if (this.deferredSegments.Count > 0 && this.deferredSegments[0].Type == "SetBulk")
            this.ActionMode = ActionMode.Bulk;
        sqlBuilder.Append(this.BuildCommand(dbContext, command));
    }
    public (IEnumerable, int, string, Action<IDataParameterCollection>, Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>,
        Action<StringBuilder, string>, Action<StringBuilder, IOrmProvider, object, string>) BuildWithBulk(IDbCommand command)
    {
        Type updateObjType = null;
        (var updateObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;
        foreach (var updateObj in updateObjs)
        {
            updateObjType = updateObj.GetType();
            break;
        }
        if (this.deferredSegments.Count > 1)
        {
            this.FixedDbParameters = new TheaDbParameterCollection();
            this.DbParameters = this.FixedDbParameters;
            //先解析其他sql，生成固定sql
            this.UpdateFields = new();

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
                    default: throw new NotSupportedException("SetBulk操作后，只支持Set/IgnoreFields/OnlyFields操作");
                }
            }
            this.DbParameters = command.Parameters;
        }
        //多命令查询时，第二次以后，DbParameters有值，不能再赋值
        else this.DbParameters ??= command.Parameters;

        int index = 0;
        var builder = new StringBuilder();
        var aliasName = this.Tables[0].AliasName;
        //sql server表别名就是表名，长度>1
        if (this.IsNeedTableAlias && aliasName.Length == 1)
            builder.Append($"{aliasName} ");
        builder.Append("SET ");
        if (this.UpdateFields != null && this.UpdateFields.Count > 0)
        {
            foreach (var setField in this.UpdateFields)
            {
                if (index > 0) builder.Append(',');
                if (this.IsNeedTableAlias) builder.Append($"{aliasName}.");
                builder.Append(setField);
                index++;
            }
            builder.Append(',');
        }
        var fixedHeadUpdateSql = builder.ToString();
        builder.Clear();
        var entityType = this.Tables[0].EntityType;
        Action<StringBuilder, string> headSqlSetter = (builder, tableName) => builder.Append($"UPDATE {this.OrmProvider.GetTableName(tableName)} {fixedHeadUpdateSql}");
        (var origName, _, var firstSqlParametersSetter, var sqlSetter) = RepositoryHelper.BuildUpdateSqlParameters(this.OrmProvider, this.MapProvider, entityType, updateObjType, true, this.OnlyFieldNames, this.IgnoreFieldNames);
        Action<IDataParameterCollection> firstParametersSetter = null;
        if (this.FixedDbParameters != null && this.FixedDbParameters.Count > 0)
            firstParametersSetter = dbParameters => this.FixedDbParameters.ToList().ForEach(f => dbParameters.Add(f));
        var typedSqlSetter = sqlSetter as Action<StringBuilder, IOrmProvider, object, string>;
        var typedFirstSqlParametersSetter = firstSqlParametersSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>;
        var tableName = this.Tables[0].Body ?? origName;
        return (updateObjs, bulkCount, tableName, firstParametersSetter, typedFirstSqlParametersSetter, headSqlSetter, typedSqlSetter);
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
        this.IgnoreFieldNames.AddRange(fieldNames);
    }
    public virtual void IgnoreFields(Expression fieldsSelector)
    {
        this.IgnoreFieldNames ??= new();
        this.VisitFields(fieldsSelector, f => this.IgnoreFieldNames.Add(f.FieldName));
    }
    public virtual void OnlyFields(string[] fieldNames)
    {
        this.OnlyFieldNames ??= new();
        this.OnlyFieldNames.AddRange(fieldNames);
    }
    public virtual void OnlyFields(Expression fieldsSelector)
    {
        this.OnlyFieldNames ??= new();
        this.VisitFields(fieldsSelector, f => this.OnlyFieldNames.Add(f.FieldName));
    }
    public virtual void SetBulk(IEnumerable updateObjs, int bulkCount)
    {
        this.ActionMode = ActionMode.Bulk;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetBulk",
            Value = (updateObjs, bulkCount)
        });
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
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        if (sqlSegment.Expression.IsParameter(out _))
            throw new NotSupportedException($"不支持的表达式访问,{sqlSegment.Expression}");
        //当作常量处理
        return sqlSegment.Change(sqlSegment.Expression.Evaluate(), true);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        if (sqlSegment.Expression.IsParameter(out _))
            throw new NotSupportedException($"不支持的表达式访问,{sqlSegment.Expression}");
        //当作常量处理
        return sqlSegment.Change(sqlSegment.Expression.Evaluate(), true);
    }
    public override SqlSegment VisitMethodCall(SqlSegment sqlSegment)
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
        this.FixedDbParameters?.Clear();
    }
    public override void Dispose()
    {
        base.Dispose();
        this.deferredSegments = null;
        this.UpdateFields = null;
        this.FixedSql = null;
        this.FixedDbParameters = null;
        this.OnlyFieldNames = null;
        this.IgnoreFieldNames = null;
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
            this.TableAliases.Add(parameterExpr.Name, this.Tables[index]);
            index++;
        }
    }
    public virtual void VisitSetField(object deferredSegmentValue)
    {
        (var fieldSelector, var fieldValue) = ((Expression, object))deferredSegmentValue;
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
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
        var entityMapper = this.Tables[0].Mapper;
        var entityType = entityMapper.EntityType;
        var updateObjType = updateObj.GetType();
        var commandInitializer = RepositoryHelper.BuildUpdateSetWithPartSqlParameters(this.OrmProvider, this.MapProvider, entityType, updateObjType, this.OnlyFieldNames, this.IgnoreFieldNames, this.IsMultiple);
        if (this.IsMultiple)
        {
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, IOrmProvider, List<string>, object, string>;
            typedCommandInitializer.Invoke(this.DbParameters, this.OrmProvider, this.UpdateFields, updateObj, $"_m{this.CommandIndex}");
        }
        else
        {
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, IOrmProvider, List<string>, object>;
            typedCommandInitializer.Invoke(this.DbParameters, this.OrmProvider, this.UpdateFields, updateObj);
        }
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
                        var sql = this.VisitFromQuery(newLambdaExpr);
                        this.UpdateFields.Add(this.OrmProvider.GetFieldName(memberMapper.FieldName) + $"=({sql})");
                    }
                    else
                    {
                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
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
                        var sql = this.VisitFromQuery(newLambdaExpr);
                        this.UpdateFields.Add(this.OrmProvider.GetFieldName(memberMapper.FieldName) + $"=({sql})");
                    }
                    else
                    {
                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
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
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);

        if (memberMapper.IsIgnore || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}被忽略更新，IsIgnore：{memberMapper.IsIgnore}，IsIgnoreUpdate：{memberMapper.IsIgnoreUpdate}");
        if (memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}不允许更新，IsRowVersion：{memberMapper.IsRowVersion}");

        this.InitTableAlias(valueSelector as LambdaExpression);
        var sql = this.VisitFromQuery(valueSelector as LambdaExpression);
        this.UpdateFields.Add(this.OrmProvider.GetFieldName(memberMapper.FieldName) + $"=({sql})");
    }
    protected virtual void VisitWhereWith(object whereObj)
    {
        var entityType = this.Tables[0].EntityType;
        var whereObjType = whereObj.GetType();
        var whereSqlParametersSetter = RepositoryHelper.BuildSqlParametersPart(this.OrmProvider, this.MapProvider, entityType, whereObjType, false, false, false, true, true, false, this.IsMultiple, false, null, null, " AND ", null);
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(this.WhereSql))
            builder.Append($"{this.WhereSql} AND ");
        if (this.IsMultiple)
        {
            var typedWhereSqlParametersSetter = whereSqlParametersSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>;
            typedWhereSqlParametersSetter.Invoke(this.DbParameters, builder, this.OrmProvider, whereObj, $"_m{this.CommandIndex}");
        }
        else
        {
            var typedWhereSqlParametersSetter = whereSqlParametersSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object>;
            typedWhereSqlParametersSetter.Invoke(this.DbParameters, builder, this.OrmProvider, whereObj);
        }
        this.WhereSql = builder.ToString();
        builder.Clear();
        builder = null;
    }
    protected virtual void VisitWhere(Expression whereExpr)
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
    protected virtual void VisitAnd(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        if (this.LastWhereOperationType == OperationType.Or)
            this.WhereSql = $"({this.WhereSql})";
        var conditionSql = this.VisitConditionExpr(lambdaExpr.Body, out var operationType);
        if (operationType == OperationType.Or)
            conditionSql = $"({conditionSql})";
        this.LastWhereOperationType = OperationType.And;
        if (!string.IsNullOrEmpty(this.WhereSql))
            this.WhereSql += " AND " + conditionSql;
        else this.WhereSql = conditionSql;
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

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = newExpr.Arguments[i], MemberMapper = memberMapper });
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

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberAssignment.Expression });
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
            this.UpdateFields.Add(this.OrmProvider.GetFieldName(memberMapper.FieldName) + "=NULL");
            return;
        }
        var fieldValue = isEntity ? memberMapper.Member.Evaluate(memberValue) : memberValue;
        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
        var dbFieldValue = memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, memberMapper.UnderlyingType, fieldValue);
        this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue));
        this.UpdateFields.Add($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
    }
    public virtual void AddMemberElement(SqlSegment sqlSegment, MemberMap memberMapper)
    {
        if (sqlSegment == SqlSegment.Null)
        {
            this.UpdateFields.Add(this.OrmProvider.GetFieldName(memberMapper.FieldName) + "=NULL");
            return;
        }
        object fieldValue = sqlSegment.Value;
        if (sqlSegment.IsConstant || sqlSegment.IsVariable)
        {
            var parameterName = this.OrmProvider.ParameterPrefix + this.ParameterPrefix + this.DbParameters.Count.ToString();
            if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
            var dbFieldValue = memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, memberMapper.UnderlyingType, fieldValue);
            this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue));
            fieldValue = parameterName;
        }
        this.UpdateFields.Add($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={fieldValue}");
    }
}
