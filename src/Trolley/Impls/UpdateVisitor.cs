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
    public virtual string BuildCommand(IDbCommand command)
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
        var sql = this.BuildSql();
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
    public virtual void BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex)
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
        sqlBuilder.Append(this.BuildCommand(command));
    }
    public virtual string BuildSql()
    {
        //多个分表，采用分表名替换，有update join情况，可能join的表也存在分表
        var tableName = this.GetTableName(this.Tables[0]);
        var builder = new StringBuilder($"UPDATE {tableName} ");
        var aliasName = this.Tables[0].AliasName;
        if (this.IsNeedTableAlias)
            builder.Append($"{aliasName} ");

        if (this.IsJoin && this.Tables.Count > 1)
        {
            for (var i = 1; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                tableName = this.GetTableName(this.Tables[i]);
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
        if (this.IsFrom && this.Tables.Count > 1)
        {
            builder.Append(" FROM ");
            for (var i = 1; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                tableName = this.GetTableName(this.Tables[i]);
                builder.Append($"{tableName} {tableSegment.AliasName}");
            }
        }
        if (!string.IsNullOrEmpty(this.WhereSql))
        {
            builder.Append(" WHERE ");
            builder.Append(this.WhereSql);
        }
        return builder.ToString();
    }
    public virtual IUpdateVisitor From(params Type[] entityTypes)
    {
        this.IsNeedTableAlias = true;
        this.IsFrom = true;
        int tableIndex = this.TableAsStart + this.Tables.Count;
        for (int i = 0; i < entityTypes.Length; i++)
        {
            var aliasName = $"{(char)(tableIndex + i)}";
            var entityType = entityTypes[i];
            var mapper = this.MapProvider.GetEntityMap(entityType);
            this.Tables.Add(new TableSegment
            {
                TableType = TableType.Entity,
                EntityType = entityType,
                Mapper = mapper,
                AliasName = aliasName,
                Path = aliasName,
                IsMaster = true
            });
        }
        return this;
    }
    public virtual IUpdateVisitor Join(string joinType, Type entityType, Expression joinOn)
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
        joinTable.OnExpr = this.VisitConditionExpr(lambdaExpr.Body);
        return this;
    }
    public virtual IUpdateVisitor SetWith(object updateObj)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetWith",
            Value = updateObj
        });
        return this;
    }
    public virtual IUpdateVisitor Set(Expression fieldsAssignment)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "Set",
            Value = fieldsAssignment
        });
        return this;
    }
    public virtual IUpdateVisitor SetField(Expression fieldSelector, object fieldValue)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetField",
            Value = (fieldSelector, fieldValue)
        });
        return this;
    }
    public virtual IUpdateVisitor SetFrom(Expression fieldsAssignment)
    {
        this.IsNeedTableAlias = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetFrom",
            Value = fieldsAssignment
        });
        return this;
    }
    public virtual IUpdateVisitor SetFrom(Expression fieldSelector, Expression valueSelector)
    {
        this.IsNeedTableAlias = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetFromField",
            Value = (fieldSelector, valueSelector)
        });
        return this;
    }
    public virtual IUpdateVisitor IgnoreFields(string[] fieldNames)
    {
        this.IgnoreFieldNames ??= new();
        this.IgnoreFieldNames.AddRange(fieldNames);
        return this;
    }
    public virtual IUpdateVisitor IgnoreFields(Expression fieldsSelector)
    {
        this.IgnoreFieldNames ??= new();
        this.VisitFields(fieldsSelector, f => this.IgnoreFieldNames.Add(f.FieldName));
        return this;
    }
    public virtual IUpdateVisitor OnlyFields(string[] fieldNames)
    {
        this.OnlyFieldNames ??= new();
        this.OnlyFieldNames.AddRange(fieldNames);
        return this;
    }
    public virtual IUpdateVisitor OnlyFields(Expression fieldsSelector)
    {
        this.OnlyFieldNames ??= new();
        this.VisitFields(fieldsSelector, f => this.OnlyFieldNames.Add(f.FieldName));
        return this;
    }
    public virtual IUpdateVisitor SetBulk(IEnumerable updateObjs, int bulkCount)
    {
        this.ActionMode = ActionMode.Bulk;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetBulk",
            Value = (updateObjs, bulkCount)
        });
        return this;
    }
    public virtual (IEnumerable, int, string, Action<IDataParameterCollection>, Action<StringBuilder, string>, Action<IDataParameterCollection, StringBuilder, object, string>) BuildSetBulk(IDbCommand command)
    {
        Type updateObjType = null;
        var entityType = this.Tables[0].EntityType;
        (var updateObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;
        foreach (var updateObj in updateObjs)
        {
            updateObjType = updateObj.GetType();
            break;
        }
        var tableName = this.Tables[0].Mapper.TableName;
        if (this.ShardingProvider.TryGetShardingTable(entityType, out _))
        {
            if (!this.Tables[0].IsSharding) throw new Exception($"实体表{entityType.FullName}有配置分表，当前操作未指定分表，请调用UseTable/UseTableBy/UseTableByRange方法指定分表");
            if (this.ShardingTables[0].ShardingType > ShardingTableType.MasterFilter)
                throw new NotSupportedException($"批量更新不支持{this.ShardingTables[0].ShardingType}方式分表");
            if (this.ShardingTables[0].ShardingType == ShardingTableType.SingleTable)
                tableName = this.Tables[0].Body;
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

        var setCommandInitializer = RepositoryHelper.BuildUpdateSetPartSqlParameters(this.OrmProvider, this.MapProvider, entityType, updateObjType, this.OnlyFieldNames, this.IgnoreFieldNames, true);
        var typedSetCommandInitializer = setCommandInitializer as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>;
        var whereCommandInitializer = RepositoryHelper.BuildWhereSqlParameters(false, this.OrmProvider, this.MapProvider, entityType, updateObjType, true, true, true, false, nameof(updateObjs));
        var typeWhereCommandInitializer = whereCommandInitializer as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>;

        Action<IDataParameterCollection> firstCommandInitializer = null;
        if (this.FixedDbParameters != null && this.FixedDbParameters.Count > 0)
            firstCommandInitializer = dbParameters => this.FixedDbParameters.ToList().ForEach(f => dbParameters.Add(f));

        Action<IDataParameterCollection, StringBuilder, object, string> commandInitializer = null;
        commandInitializer = (dbParameters, builder, updateObj, suffix) =>
        {
            typedSetCommandInitializer.Invoke(dbParameters, builder, this.OrmProvider, updateObj, suffix);
            builder.Append(" WHERE ");
            typeWhereCommandInitializer.Invoke(dbParameters, builder, this.OrmProvider, updateObj, suffix);
        };
        Action<StringBuilder, string> headSqlSetter = null;
        if (string.IsNullOrEmpty(fixedHeadUpdateSql))
            headSqlSetter = (builder, tableName) => builder.Append($"UPDATE {this.OrmProvider.GetTableName(tableName)} ");
        else headSqlSetter = (builder, tableName) => builder.Append($"UPDATE {this.OrmProvider.GetTableName(tableName)} {fixedHeadUpdateSql}");
        return (updateObjs, bulkCount, tableName, firstCommandInitializer, headSqlSetter, commandInitializer);
    }
    public virtual IUpdateVisitor WhereWith(object whereObj)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WhereWith",
            Value = whereObj
        });
        return this;
    }
    public virtual IUpdateVisitor Where(Expression whereExpr)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "Where",
            Value = whereExpr
        });
        return this;
    }
    public virtual IUpdateVisitor And(Expression whereExpr)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "And",
            Value = whereExpr
        });
        return this;
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
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
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
                    if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
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
        this.IsNeedTableAlias = true;
        var entityMapper = this.Tables[0].Mapper;
        (var fieldSelector, var valueSelector) = ((Expression, Expression))deferredSegmentValue;
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);

        this.InitTableAlias(valueSelector as LambdaExpression);
        var sql = this.VisitFromQuery(valueSelector as LambdaExpression);
        this.UpdateFields.Add(this.OrmProvider.GetFieldName(memberMapper.FieldName) + $"=({sql})");
    }
    protected virtual void VisitWhereWith(object whereObj)
    {
        var entityType = this.Tables[0].EntityType;
        var whereObjType = whereObj.GetType();
        var commandInitializer = RepositoryHelper.BuildWhereSqlParameters(true, this.OrmProvider, this.MapProvider, entityType, whereObjType, true, this.IsMultiple, true, false, "whereObj");
        string whereSql = null;
        if (this.IsMultiple)
        {
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string, string>;
            whereSql = typedCommandInitializer.Invoke(this.DbParameters, this.OrmProvider, whereObj, $"_m{this.CommandIndex}");
        }
        else
        {
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            whereSql = typedCommandInitializer.Invoke(this.DbParameters, this.OrmProvider, whereObj);
        }
        if (!string.IsNullOrEmpty(this.WhereSql))
            this.WhereSql += " AND ";
        this.WhereSql += whereSql;
    }
    protected virtual void VisitWhere(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.LastWhereNodeType = OperationType.None;
        var whereSql = this.VisitConditionExpr(lambdaExpr.Body);
        if (!string.IsNullOrEmpty(this.WhereSql))
            this.WhereSql += " AND ";
        this.WhereSql += whereSql;
        this.IsWhere = false;
    }
    protected virtual void VisitAnd(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        if (this.LastWhereNodeType == OperationType.Or)
        {
            this.WhereSql = $"({this.WhereSql})";
            this.LastWhereNodeType = OperationType.And;
        }
        var conditionSql = this.VisitConditionExpr(lambdaExpr.Body);
        if (this.LastWhereNodeType == OperationType.Or)
        {
            conditionSql = $"({conditionSql})";
            this.LastWhereNodeType = OperationType.And;
        }
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
