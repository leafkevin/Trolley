using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;
using System.Text;

namespace Trolley;

public class UpdateVisitor : SqlVisitor, IUpdateVisitor
{
    protected bool IsFrom { get; set; } = false;
    protected bool IsJoin { get; set; } = false;
    protected List<UpdateField> UpdateFields { get; set; } = new();
    protected string FixedSql { get; set; }
    protected List<IDbDataParameter> FixedDbParameters { get; set; } = new();
    protected Action<IDbCommand, UpdateVisitor, StringBuilder, object, int> BulkSetFieldsInitializer { get; set; }

    public UpdateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", List<IDbDataParameter> dbParameters = null)
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix, "", dbParameters)
    {
        this.Tables = new()
        {
            new TableSegment
            {
                EntityType = entityType,
                Mapper = this.MapProvider.GetEntityMap(entityType),
                AliasName = tableAsStart.ToString()
            }
        };
        this.TableAlias = new();
        this.DbParameters ??= new();
    }
    public virtual string BuildSql(out List<IDbDataParameter> dbParameters)
    {
        var entityTableName = this.OrmProvider.GetTableName(this.Tables[0].Mapper.TableName);
        var builder = new StringBuilder($"UPDATE {entityTableName} ");
        var aliasName = this.Tables[0].AliasName;
        if (this.IsNeedAlias && aliasName.Length == 1)
            builder.Append($"{aliasName} ");

        if (this.IsJoin && this.Tables.Count > 1)
        {
            for (var i = 1; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                var tableName = tableSegment.Body;
                if (string.IsNullOrEmpty(tableName))
                {
                    tableSegment.Mapper ??= this.MapProvider.GetEntityMap(tableSegment.EntityType);
                    tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
                }
                builder.Append($"{tableSegment.JoinType} {tableName} {tableSegment.AliasName}");
                builder.Append($" ON {tableSegment.OnExpr} ");
            }
        }
        int index = 0;
        bool hasWhere = false;
        builder.Append("SET ");
        if (this.UpdateFields != null && this.UpdateFields.Count > 0)
        {
            foreach (var setField in this.UpdateFields)
            {
                if (setField.Type == UpdateFieldType.Where)
                {
                    hasWhere = true;
                    continue;
                }
                if (index > 0) builder.Append(',');
                switch (setField.Type)
                {
                    case UpdateFieldType.SetField:
                    case UpdateFieldType.SetValue:
                        if (this.IsNeedAlias) builder.Append($"{aliasName}.");
                        builder.Append($"{this.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={setField.Value}");
                        break;
                    case UpdateFieldType.RawSql:
                        builder.Append(setField.Value);
                        break;
                }
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
                {
                    tableSegment.Mapper ??= this.MapProvider.GetEntityMap(tableSegment.EntityType);
                    tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
                }
                builder.Append($"{tableName} {tableSegment.AliasName}");
            }
        }
        if (!string.IsNullOrEmpty(this.WhereSql) || hasWhere)
            builder.Append(" WHERE ");
        if (hasWhere)
        {
            index = 0;
            foreach (var setField in this.UpdateFields)
            {
                if (setField.Type != UpdateFieldType.Where)
                    continue;
                if (index > 0) builder.Append(" AND ");
                if (this.IsNeedAlias) builder.Append($"{aliasName}");
                builder.Append($"{this.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={setField.Value}");
                index++;
            }
        }
        if (!string.IsNullOrEmpty(this.WhereSql))
        {
            if (hasWhere)
                builder.Append(" AND ");
            builder.Append(this.WhereSql);
        }

        dbParameters = this.DbParameters;
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
    public virtual IUpdateVisitor Set(Expression fieldsAssignment)
    {
        var lambdaExpr = fieldsAssignment as LambdaExpression;
        var entityMapper = this.Tables[0].Mapper;
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                        continue;

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = newExpr.Arguments[i], MemberMapper = memberMapper });
                    //只一个成员访问，没有设置语句，什么也不做，忽略
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberInfo.Name)
                        continue;
                    this.AddMemberElement(sqlSegment, memberMapper);
                }
                break;
            case ExpressionType.MemberInit:
                this.InitTableAlias(lambdaExpr);
                var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
                        continue;

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberAssignment.Expression });
                    //只一个成员访问，没有设置语句，什么也不做，忽略
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberAssignment.Member.Name)
                        continue;
                    this.AddMemberElement(sqlSegment, memberMapper);
                }
                break;
        }
        return this;
    }
    public virtual IUpdateVisitor Set(Expression fieldSelector, object fieldValue)
    {
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var entityMapper = this.Tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        this.AddMemberElement(memberMapper, fieldValue, false);
        return this;
    }
    public virtual IUpdateVisitor SetWith(Expression fieldsAssignment)
    {
        var entityMapper = this.Tables[0].Mapper;
        var lambdaExpr = fieldsAssignment as LambdaExpression;
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                        continue;

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = newExpr.Arguments[i] });
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
                        continue;
                    this.AddMemberElement(sqlSegment, memberMapper);
                }
                break;
            case ExpressionType.MemberInit:
                this.InitTableAlias(lambdaExpr);
                var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
                        continue;

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberAssignment.Expression });
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
                        continue;
                    this.AddMemberElement(sqlSegment, memberMapper);
                }
                break;
        }
        return this;
    }
    public virtual IUpdateVisitor SetWith(Expression fieldsSelectorOrAssignment, object updateObj, bool isExceptKey = false)
    {
        var entityMapper = this.Tables[0].Mapper;
        if (fieldsSelectorOrAssignment != null)
        {
            MemberMap memberMapper = null;
            var lambdaExpr = fieldsSelectorOrAssignment as LambdaExpression;
            switch (lambdaExpr.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var memberExpr = lambdaExpr.Body as MemberExpression;
                    memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
                    this.AddMemberElement(memberMapper, updateObj);
                    break;
                case ExpressionType.New:
                    this.InitTableAlias(lambdaExpr);
                    var newExpr = lambdaExpr.Body as NewExpression;
                    for (int i = 0; i < newExpr.Arguments.Count; i++)
                    {
                        var memberInfo = newExpr.Members[i];
                        if (!entityMapper.TryGetMemberMap(memberInfo.Name, out memberMapper))
                            continue;

                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = newExpr.Arguments[i] });
                        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
                            this.AddMemberElement(memberMapper, updateObj);
                        else this.AddMemberElement(sqlSegment, memberMapper);
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
                        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
                            this.AddMemberElement(memberMapper, updateObj);
                        else this.AddMemberElement(sqlSegment, memberMapper);
                    }
                    break;
            }
        }
        else
        {
            var parametersInitializer = RepositoryHelper.BuildUpdateSetWithParameters(this, entityMapper.EntityType, updateObj, isExceptKey);
            parametersInitializer.Invoke(this, this.UpdateFields, this.DbParameters, updateObj);
        }
        return this;
    }
    public virtual IUpdateVisitor SetFrom(Expression fieldsAssignment)
    {
        this.IsNeedAlias = true;
        var entityMapper = this.Tables[0].Mapper;
        var lambdaExpr = fieldsAssignment as LambdaExpression;
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
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
                        var sql = this.VisitFromQuery(newLambdaExpr, out var isNeedAlias);
                        if (isNeedAlias) this.IsNeedAlias = true;
                        this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = $"({sql})" });
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
                this.InitTableAlias(lambdaExpr);
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
                        var sql = this.VisitFromQuery(newLambdaExpr, out var isNeedAlias);
                        if (isNeedAlias) this.IsNeedAlias = true;
                        this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = $"({sql})" });
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
        return this;
    }
    public virtual IUpdateVisitor SetFrom(Expression fieldSelector, Expression valueSelector)
    {
        this.IsNeedAlias = true;
        var entityMapper = this.Tables[0].Mapper;
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);

        this.InitTableAlias(valueSelector as LambdaExpression);
        var sql = this.VisitFromQuery(valueSelector as LambdaExpression, out var isNeedAlias);
        if (isNeedAlias) this.IsNeedAlias = true;
        this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = $"({sql})" });
        return this;
    }
    public virtual IUpdateVisitor SetBulkFirst(Expression fieldsSelectorOrAssignment, object updateObjs)
    {
        var entityMapper = this.Tables[0].Mapper;
        var fixSetFields = new List<UpdateField>();
        var lambdaExpr = fieldsSelectorOrAssignment as LambdaExpression;
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = lambdaExpr.Body as MemberExpression;
                    var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
                    var parameterName = $"{this.OrmProvider.ParameterPrefix}{this.MultiParameterPrefix}{memberExpr.Member.Name}";
                    this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = parameterName });
                }
                break;
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                        continue;

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = newExpr.Arguments[i] });
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
                    {
                        var parameterName = $"{this.OrmProvider.ParameterPrefix}{this.MultiParameterPrefix}{memberInfo.Name}";
                        this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = parameterName });
                    }
                    else this.AddMemberElement(sqlSegment, memberMapper, fixSetFields, this.FixedDbParameters);
                }
                break;
            case ExpressionType.MemberInit:
                this.InitTableAlias(lambdaExpr);
                var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
                        continue;

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberAssignment.Expression });
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
                    {
                        var parameterName = $"{this.OrmProvider.ParameterPrefix}{this.MultiParameterPrefix}{memberMapper.MemberName}";
                        this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = parameterName });
                    }
                    else this.AddMemberElement(sqlSegment, memberMapper, fixSetFields, this.FixedDbParameters);
                }
                break;
        }

        var fixedBuilder = new StringBuilder($"UPDATE {this.OrmProvider.GetTableName(entityMapper.TableName)} SET ");
        if (fixSetFields.Count > 0)
        {
            for (int i = 0; i < fixSetFields.Count; i++)
            {
                var fixSetField = fixSetFields[i];
                if (i > 0) fixedBuilder.Append(',');
                fixedBuilder.Append($"{this.OrmProvider.GetFieldName(fixSetField.MemberMapper.FieldName)}={fixSetField.Value}");
            }
        }
        this.FixedSql = fixedBuilder.ToString();

        var setFields = this.UpdateFields.FindAll(f => !f.MemberMapper.IsKey);
        this.BulkSetFieldsInitializer = (command, visitor, builder, updateObj, index) =>
        {
            builder.Append(visitor.FixedSql);
            if (visitor.FixedDbParameters != null && visitor.FixedDbParameters.Count > 0 && index == 0)
                visitor.FixedDbParameters.ForEach(f => command.Parameters.Add(f));
            if (fixSetFields.Count > 0) builder.Append(',');
            int setIndex = 0;
            foreach (var setField in visitor.UpdateFields)
            {
                if (setIndex > 0) builder.Append(',');
                if (setField.Type == UpdateFieldType.SetValue)
                    builder.Append($"{visitor.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={setField.Value}");
                //SetField
                else
                {
                    var fieldValue = visitor.EvaluateAndCache(updateObj, setField.MemberMapper.MemberName);
                    builder.Append($"{visitor.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={setField.Value}{index}");
                    command.Parameters.Add(visitor.OrmProvider.CreateParameter(setField.MemberMapper, $"{setField.Value}{index}", fieldValue));
                }
                setIndex++;
            }
            builder.Append(" WHERE ");
            for (int i = 0; i < entityMapper.KeyMembers.Count; i++)
            {
                var keyMember = entityMapper.KeyMembers[i];
                if (i > 0) builder.Append(" AND ");
                var parameterName = $"{visitor.OrmProvider.ParameterPrefix}k{keyMember.MemberName}{index}";
                builder.Append($"{visitor.OrmProvider.GetFieldName(keyMember.FieldName)}={parameterName}");
                var fieldValue = visitor.EvaluateAndCache(updateObj, keyMember.MemberName);
                command.Parameters.Add(visitor.OrmProvider.CreateParameter(keyMember, parameterName, fieldValue));
            }
        };
        return this;
    }
    public virtual void SetBulk(StringBuilder builder, IDbCommand command, object updateObj, int index)
        => this.BulkSetFieldsInitializer.Invoke(command, this, builder, updateObj, index);
    public virtual IUpdateVisitor WhereWith(object whereObj, bool isOnlyKeys = false)
    {
        var entityMapper = this.Tables[0].Mapper;
        var whereInitializer = RepositoryHelper.BuildUpdateWhereWithParameters(this, entityMapper.EntityType, whereObj, isOnlyKeys);
        whereInitializer.Invoke(this, this.UpdateFields, this.DbParameters, whereObj);
        return this;
    }
    public virtual IUpdateVisitor Where(Expression whereExpr)
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
        return this;
    }
    public virtual IUpdateVisitor And(Expression whereExpr)
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
    protected void InitTableAlias(LambdaExpression lambdaExpr)
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
    protected void AddMemberElement(MemberMap memberMapper, object memberValue, bool isEntity = true)
    {
        if (memberValue is DBNull || memberValue == null)
        {
            this.UpdateFields.Add(new UpdateField { Type = UpdateFieldType.SetValue, MemberMapper = memberMapper, Value = "NULL" });
            return;
        }
        var fieldValue = isEntity ? this.EvaluateAndCache(memberValue, memberMapper.MemberName) : memberValue;
        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        this.DbParameters.Add(this.OrmProvider.CreateParameter(memberMapper, parameterName, fieldValue));
        this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = parameterName });
    }
    protected void AddMemberElement(SqlSegment sqlSegment, MemberMap memberMapper)
    {
        if (sqlSegment == SqlSegment.Null)
        {
            this.UpdateFields.Add(new UpdateField { Type = UpdateFieldType.SetValue, MemberMapper = memberMapper, Value = "NULL" });
            return;
        }
        sqlSegment.IsParameterized = true;
        sqlSegment.MemberMapper = memberMapper;
        sqlSegment.ParameterName = memberMapper.MemberName;
        this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = this.GetQuotedValue(sqlSegment) });
    }
    protected void AddMemberElement(SqlSegment sqlSegment, MemberMap memberMapper, List<UpdateField> setFields, List<IDbDataParameter> dbParameters)
    {
        if (sqlSegment == SqlSegment.Null)
        {
            setFields.Add(new UpdateField { Type = UpdateFieldType.SetValue, MemberMapper = memberMapper, Value = "NULL" });
            return;
        }

        if (sqlSegment.IsConstant || sqlSegment.IsVariable)
        {
            //只有常量和变量才有可能是数组
            if (sqlSegment.IsArray && sqlSegment.Value is List<SqlSegment> sqlSegments)
                sqlSegment.Value = sqlSegments.Select(f => f.Value).ToArray();

            var parameterName = this.OrmProvider.ParameterPrefix + this.ParameterPrefix + dbParameters.Count.ToString();
            IDbDataParameter dbParameter = null;
            if (memberMapper != null)
                dbParameter = this.OrmProvider.CreateParameter(memberMapper, parameterName, sqlSegment.Value);
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
