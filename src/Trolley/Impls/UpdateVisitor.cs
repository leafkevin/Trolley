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
    public bool IsBulk { get; set; }
    public bool IsFrom { get; set; }
    public bool IsJoin { get; set; }
    public List<FieldsSegment> UpdateFields { get; set; }
    public List<FieldsSegment> WhereFields { get; set; }
    public bool HasFixedSet { get; set; }
    public string FixedSql { get; set; }
    public List<IDbDataParameter> FixedDbParameters { get; set; } = new();

    public UpdateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
    {
        this.DbKey = dbKey;
        this.OrmProvider = ormProvider;
        this.MapProvider = mapProvider;
        this.IsParameterized = isParameterized;
        this.TableAsStart = tableAsStart;
        this.ParameterPrefix = parameterPrefix;
    }
    public virtual void Initialize(Type entityType, bool isFirst = true)
    {
        if (isFirst)
        {
            this.Tables = new();
            this.TableAlias = new();
        }
        //clear
        else this.Clear();
        this.Tables.Add(new TableSegment
        {
            EntityType = entityType,
            AliasName = "a",
            Mapper = this.MapProvider.GetEntityMap(entityType)
        });
    }
    public string BuildCommand(IDbCommand command)
    {
        string sql = null;
        this.DbParameters = command.Parameters;
        foreach (var deferredSegment in this.deferredSegments)
        {
            switch (deferredSegment.Type)
            {
                case "Set":
                case "SetFrom":
                    this.VisitSet(deferredSegment.Value as Expression);
                    break;
                case "SetField":
                    this.VisitSetField(deferredSegment.Value);
                    break;
                case "SetWith":
                    this.VisitSetWith(deferredSegment.Value);
                    break;
                case "SetFromField":
                    this.VisitSetFromField((FieldFromQuery)deferredSegment.Value);
                    break;
                case "SetBulk":
                    this.IsBulk = true;
                    continue;
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
        if (this.IsBulk) sql = this.BuildMutilBulkSql(command);
        if (sql == null) sql = this.BuildSql();
        command.CommandText = sql;
        return sql;
    }
    public virtual MultipleCommand CreateMultipleCommand()
    {
        return new MultipleCommand
        {
            CommandType = MultipleCommandType.Update,
            EntityType = this.Tables[0].EntityType,
            Body = this.deferredSegments
        };
    }
    public void BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex)
    {
        this.IsMultiple = true;
        this.CommandIndex = commandIndex;
        this.deferredSegments = multiCommand.Body as List<CommandSegment>;
        if (sqlBuilder.Length > 0) sqlBuilder.Append(';');
        sqlBuilder.Append(this.BuildCommand(command));
    }
    public virtual string BuildSql()
    {
        var entityTableName = this.OrmProvider.GetTableName(this.Tables[0].Mapper.TableName);
        var builder = new StringBuilder($"UPDATE {entityTableName} ");
        var aliasName = this.Tables[0].AliasName;
        if (this.IsNeedAlias)
            builder.Append($"{aliasName} ");

        if (this.IsJoin && this.Tables.Count > 1)
        {
            for (var i = 1; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                var tableName = tableSegment.Body;
                if (string.IsNullOrEmpty(tableName))
                    tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
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
                if (this.IsNeedAlias) builder.Append($"{aliasName}.");
                builder.Append($"{setField.Fields}={setField.Values}");
                index++;
            }
        }

        if (this.IsFrom && this.Tables.Count > 1)
        {
            builder.Append(" FROM ");
            for (var i = 1; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                var tableName = tableSegment.Body;
                if (string.IsNullOrEmpty(tableName))
                    tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
                builder.Append($"{tableName} {tableSegment.AliasName}");
            }
        }
        if (!string.IsNullOrEmpty(this.WhereSql) || (this.WhereFields != null && this.WhereFields.Count > 0))
            builder.Append(" WHERE ");
        bool hasWhere = false;
        if (this.WhereFields != null && this.WhereFields.Count > 0)
        {
            index = 0;
            foreach (var whereField in this.WhereFields)
            {
                if (index > 0) builder.Append(" AND ");
                if (this.IsNeedAlias) builder.Append($"{aliasName}");
                builder.Append($"{whereField.Fields}={whereField.Values}");
                index++;
            }
            hasWhere = true;
        }
        if (!string.IsNullOrEmpty(this.WhereSql))
        {
            if (hasWhere)
                builder.Append(" AND ");
            builder.Append(this.WhereSql);
        }
        return builder.ToString();
    }
    public virtual IUpdateVisitor From(params Type[] entityTypes)
    {
        this.IsNeedAlias = true;
        this.IsFrom = true;
        int tableIndex = this.TableAsStart + this.Tables.Count;
        for (int i = 0; i < entityTypes.Length; i++)
        {
            this.Tables.Add(new TableSegment
            {
                EntityType = entityTypes[i],
                AliasName = $"{(char)(tableIndex + i)}"
            });
        }
        return this;
    }
    public virtual IUpdateVisitor Join(string joinType, Type entityType, Expression joinOn)
    {
        this.IsNeedAlias = true;
        this.IsJoin = true;
        var lambdaExpr = joinOn as LambdaExpression;
        var joinTable = new TableSegment
        {
            EntityType = entityType,
            AliasName = $"{(char)('a' + this.Tables.Count)}",
            JoinType = joinType
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
        this.IsNeedAlias = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetFrom",
            Value = fieldsAssignment
        });
        return this;
    }
    public virtual IUpdateVisitor SetFrom(Expression fieldSelector, Expression valueSelector)
    {
        this.IsNeedAlias = true;
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
        this.VisitFields(fieldsSelector, f => this.IgnoreFieldNames.Add(this.OrmProvider.GetFieldName(f.FieldName)));
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
        this.VisitFields(fieldsSelector, f => this.OnlyFieldNames.Add(this.OrmProvider.GetFieldName(f.FieldName)));
        return this;
    }
    public virtual IUpdateVisitor SetBulk(IEnumerable updateObjs, int bulkCount)
    {
        this.IsBulk = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetBulk",
            Value = (updateObjs, bulkCount)
        });
        return this;
    }
    public virtual (IEnumerable, int, Action<StringBuilder, object, string>) BuildSetBulk(IDbCommand command)
    {
        var entityMapper = this.Tables[0].Mapper;
        this.DbParameters = command.Parameters;
        if (this.deferredSegments.Count > 1)
        {
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
                    case "Where":
                        this.VisitWhere(deferredSegment.Value as Expression);
                        break;
                    case "WhereWith":
                        this.VisitWhereWith(deferredSegment.Value);
                        break;
                    case "And":
                        this.VisitAnd(deferredSegment.Value as Expression);
                        break;
                    default: throw new NotSupportedException("批量更新后，只支持Set/IgnoreFields/OnlyFields/Where/And操作");
                }
            }
        }
        var builder = new StringBuilder($"UPDATE {this.OrmProvider.GetTableName(entityMapper.TableName)} SET ");
        var aliasName = this.Tables[0].AliasName;
        //sql server表别名就是表名，长度>1
        if (this.IsNeedAlias && aliasName.Length == 1)
            builder.Append($"{aliasName} ");

        int index = 0;
        string fixedHeadUpdateSql = null;
        if (this.UpdateFields != null && this.UpdateFields.Count > 0)
        {
            foreach (var setField in this.UpdateFields)
            {
                if (index > 0) builder.Append(',');
                if (this.IsNeedAlias) builder.Append($"{aliasName}.");
                builder.Append($"{setField.Fields}={setField.Values}");
                index++;
            }
        }
        fixedHeadUpdateSql = builder.ToString();

        string fixedWhereSql = null;
        builder.Clear();
        builder.Append(" WHERE ");
        if (!string.IsNullOrEmpty(this.WhereSql))
            builder.Append(this.WhereSql);
        fixedWhereSql = builder.ToString();

        List<IDbDataParameter> fixedDbParameters = null;
        if (this.DbParameters.Count > 0)
            fixedDbParameters = this.DbParameters.Cast<IDbDataParameter>().ToList();

        var entityType = this.Tables[0].EntityType;
        (var updateObjs, int bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;
        object updateObj = null;
        foreach (var entity in updateObjs)
        {
            updateObj = entity;
            break;
        }
        var updateObjType = updateObj.GetType();
        var setCommandInitializer = RepositoryHelper.BuildUpdateSetPartSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, updateObjType, this.OnlyFieldNames, this.IgnoreFieldNames, true);
        var typedSetCommandInitializer = setCommandInitializer as Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>;

        builder.Clear();
        this.DbParameters.Clear();
        Action<StringBuilder, object, string> commandInitializer = null;
        commandInitializer = (builder, updateObj, suffix) =>
        {
            builder.Append(fixedHeadUpdateSql);
            typedSetCommandInitializer.Invoke(this.DbParameters, this.OrmProvider, builder, updateObj, suffix);
            builder.Append(fixedWhereSql);
            index = 0;
            foreach (var whereField in this.WhereFields)
            {
                if (index > 0) builder.Append(" AND ");
                if (this.IsNeedAlias) builder.Append($"{aliasName}");
                builder.Append($"{whereField.Fields}={whereField.Values}");
                index++;
            }
            if (fixedDbParameters.Count > 0)
                fixedDbParameters.ForEach(f => this.DbParameters.Add(f));
        };
        return (updateObjs, bulkCount, commandInitializer);
    }
    public virtual string BuildMutilBulkSql(IDbCommand command)
    {
        (var updateObjs, _, var commandInitializer) = this.BuildSetBulk(command);
        int index = 0;
        var builder = new StringBuilder();
        foreach (var updateObj in updateObjs)
        {
            if (index > 0) builder.Append(';');
            commandInitializer.Invoke(builder, updateObj, $"_m{this.CommandIndex}{index}");
            index++;
        }
        return builder.ToString();
    }
    public virtual void SetBulkHead(StringBuilder builder)
    {
        if (this.FixedDbParameters != null && this.FixedDbParameters.Count > 0)
            this.FixedDbParameters.ForEach(f => this.DbParameters.Add(f));
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
    public virtual IUpdateVisitor Where(Expression whereExpr, bool isWhereObj = false)
    {
        if (isWhereObj)
        {
            this.IgnoreFieldNames ??= new();
            this.WhereFields ??= new();
            this.VisitFields(whereExpr, f =>
            {
                this.IgnoreFieldNames.Add(this.OrmProvider.GetFieldName(f.FieldName));
                this.WhereFields.Add(new FieldsSegment { Fields = this.OrmProvider.GetFieldName(f.FieldName) });
            });
        }
        else
        {
            this.deferredSegments.Add(new CommandSegment
            {
                Type = "Where",
                Value = whereExpr
            });
        }
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
        return this.Evaluate(sqlSegment);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        if (sqlSegment.Expression.IsParameter(out _))
            throw new NotSupportedException($"不支持的表达式访问,{sqlSegment.Expression}");
        return this.Evaluate(sqlSegment);
    }
    public override SqlSegment VisitMethodCall(SqlSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        if (methodCallExpr.Method.DeclaringType == typeof(Sql)
            || typeof(IAggregateSelect).IsAssignableFrom(methodCallExpr.Method.DeclaringType))
            return this.VisitSqlMethodCall(sqlSegment);

        if (!sqlSegment.IsDeferredFields && this.OrmProvider.TryGetMethodCallSqlFormatter(methodCallExpr, out var formatter))
            return formatter.Invoke(this, methodCallExpr, methodCallExpr.Object, sqlSegment.DeferredExprs, methodCallExpr.Arguments.ToArray());

        var lambdaExpr = Expression.Lambda(sqlSegment.Expression);
        var objValue = lambdaExpr.Compile().DynamicInvoke();
        if (objValue == null)
            return SqlSegment.Null;

        //把方法返回值当作常量处理
        return sqlSegment.Change(objValue, true, false, false);
    }
    public void Clear()
    {
        this.Tables?.Clear();
        this.TableAlias?.Clear();
        this.ReaderFields?.Clear();
        this.WhereSql = null;
        this.LastWhereNodeType = OperationType.None;
        this.IsFromQuery = false;
        this.TableAsStart = 'a';
        this.IsNeedAlias = false;

        this.IsFrom = false;
        this.IsJoin = false;
        this.deferredSegments.Clear();
        this.UpdateFields.Clear();
        this.FixedSql = null;
        this.FixedDbParameters.Clear();
    }
    public override void Dispose()
    {
        base.Dispose();
        this.deferredSegments = null;
        this.UpdateFields = null;
        this.FixedSql = null;
        this.FixedDbParameters = null;
    }
    public void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.TableAlias.Clear();
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
            this.TableAlias.Add(parameterExpr.Name, this.Tables[index]);
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
        this.AddMemberElement(this.UpdateFields, memberMapper, fieldValue, false);
    }
    public virtual void VisitSetWith(object updateObj)
    {
        var entityMapper = this.Tables[0].Mapper;
        var entityType = entityMapper.EntityType;
        var updateObjType = updateObj.GetType();
        var commandInitializer = RepositoryHelper.BuildUpdateSetWithPartSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, updateObjType, this.OnlyFieldNames, this.IgnoreFieldNames, this.IsMultiple);
        if (this.IsMultiple)
        {
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, IOrmProvider, List<FieldsSegment>, object, string>;
            typedCommandInitializer.Invoke(this.DbParameters, this.OrmProvider, this.UpdateFields, (object)updateObj, $"_m{this.CommandIndex}");
        }
        else
        {
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, IOrmProvider, List<FieldsSegment>, object>;
            typedCommandInitializer.Invoke(this.DbParameters, this.OrmProvider, this.UpdateFields, (object)updateObj);
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
                        this.UpdateFields.Add(new FieldsSegment { Fields = this.OrmProvider.GetFieldName(memberMapper.FieldName), Values = $"({sql})" });
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
                        this.UpdateFields.Add(new FieldsSegment { Fields = this.OrmProvider.GetFieldName(memberMapper.FieldName), Values = $"({sql})" });
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
        this.IsNeedAlias = true;
        var entityMapper = this.Tables[0].Mapper;
        (var fieldSelector, var valueSelector) = ((Expression, Expression))deferredSegmentValue;
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);

        this.InitTableAlias(valueSelector as LambdaExpression);
        var sql = this.VisitFromQuery(valueSelector as LambdaExpression);
        this.UpdateFields.Add(new FieldsSegment { Fields = this.OrmProvider.GetFieldName(memberMapper.FieldName), Values = $"({sql})" });
    }
    protected virtual void VisitWhereWith(object whereObj)
    {
        var entityType = this.Tables[0].EntityType;
        var whereObjType = whereObj.GetType();
        var commandInitializer = RepositoryHelper.BuildWhereSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObjType, true, this.IsMultiple, true, "whereObj", null);
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
        //TODO:别名测试
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
    protected void AddMemberElement(List<FieldsSegment> fieldsSegments, MemberMap memberMapper, object memberValue, bool isEntity = true)
    {
        if (memberValue is DBNull || memberValue == null)
        {
            fieldsSegments.Add(new FieldsSegment { Type = "SQL", Fields = this.OrmProvider.GetFieldName(memberMapper.FieldName), Values = "NULL" });
            return;
        }
        var fieldValue = isEntity ? this.EvaluateAndCache(memberValue, memberMapper.Member) : memberValue;
        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
        this.OrmProvider.AddDbParameter(this.DbKey, this.DbParameters, memberMapper, parameterName, fieldValue);
        fieldsSegments.Add(new FieldsSegment { Type = "SQL", Fields = this.OrmProvider.GetFieldName(memberMapper.FieldName), Values = parameterName });
    }
    protected void AddMemberElement(SqlSegment sqlSegment, MemberMap memberMapper)
    {
        if (sqlSegment == SqlSegment.Null)
        {
            this.UpdateFields.Add(new FieldsSegment { Type = "SQL", Fields = this.OrmProvider.GetFieldName(memberMapper.FieldName), Values = "NULL" });
            return;
        }
        if (sqlSegment.IsConstant || sqlSegment.IsVariable)
        {
            //只有常量和变量才有可能是数组
            if (sqlSegment.IsArray && sqlSegment.Value is List<SqlSegment> sqlSegments)
                sqlSegment.Value = sqlSegments.Select(f => f.Value).ToArray();

            var parameterName = this.OrmProvider.ParameterPrefix + this.ParameterPrefix + this.DbParameters.Count.ToString();
            if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
            this.OrmProvider.AddDbParameter(this.DbKey, this.DbParameters, sqlSegment.MemberMapper, parameterName, sqlSegment.Value);
            this.UpdateFields.Add(new FieldsSegment { Type = "SQL", Fields = this.OrmProvider.GetFieldName(memberMapper.FieldName), Values = parameterName });
        }
    }
    protected void AddMemberElement(SqlSegment sqlSegment, MemberMap memberMapper, List<UpdateField> setFields, List<IDbDataParameter> dbParameters)
    {
        if (sqlSegment == SqlSegment.Null)
        {
            setFields.Add(new UpdateField { Type = UpdateFieldType.SetValue, MemberMapper = memberMapper, Value = "NULL" });
            return;
        }
        //此种场景是表达式或函数调用，如：f => new {  Name = f.Name + "_1", Content = new { ... } }
        if (sqlSegment.IsConstant || sqlSegment.IsVariable)
        {
            //只有常量和变量才有可能是数组
            if (sqlSegment.IsArray && sqlSegment.Value is List<SqlSegment> sqlSegments)
                sqlSegment.Value = sqlSegments.Select(f => f.Value).ToArray();

            var parameterName = this.OrmProvider.ParameterPrefix + this.ParameterPrefix + dbParameters.Count.ToString();
            if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
            IDbDataParameter dbParameter = null;
            if (memberMapper != null)
                this.OrmProvider.AddDbParameter(this.DbKey, this.DbParameters, memberMapper, parameterName, sqlSegment.Value);
            else dbParameter = this.OrmProvider.CreateParameter(parameterName, sqlSegment.Value);

            dbParameters.Add(dbParameter);
            sqlSegment.Value = parameterName;
            //sqlSegment.IsParameter = true;
            //sqlSegment.IsVariable = false;
            //sqlSegment.IsConstant = false;
        }
        setFields.Add(new UpdateField { MemberMapper = memberMapper, Value = sqlSegment.Value.ToString() });
    }
}
