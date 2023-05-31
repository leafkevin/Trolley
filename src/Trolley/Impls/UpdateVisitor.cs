using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;

namespace Trolley;

public class UpdateVisitor : SqlVisitor, IUpdateVisitor
{
    private static ConcurrentDictionary<int, Func<object, object>> getterCache = new();
    protected bool isFrom = false;
    protected bool isJoin = false;
    protected string whereSql = string.Empty;
    protected string setSql = string.Empty;
    protected bool isFirst = true;
    protected List<SetField> setFields = null;
    protected List<IDbDataParameter> fixedDbParameters = null;

    public UpdateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix)
    {
        this.tables = new();
        this.tableAlias = new();
        this.tables.Add(new TableSegment
        {
            EntityType = entityType,
            Mapper = this.mapProvider.GetEntityMap(entityType),
            AliasName = tableAsStart.ToString()
        });
    }
    public virtual string BuildSql(out List<IDbDataParameter> dbParameters)
    {
        var entityTableName = this.OrmProvider.GetTableName(this.tables[0].Mapper.TableName);
        var builder = new StringBuilder($"UPDATE {entityTableName} ");

        if (this.IsNeedAlias) builder.Append("a ");
        if (this.isJoin && this.tables.Count > 1)
        {
            for (var i = 1; i < this.tables.Count; i++)
            {
                var tableSegment = this.tables[i];
                var tableName = tableSegment.Body;
                if (string.IsNullOrEmpty(tableName))
                {
                    tableSegment.Mapper ??= this.mapProvider.GetEntityMap(tableSegment.EntityType);
                    tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
                }
                builder.Append($"{tableSegment.JoinType} {tableName} {tableSegment.AliasName}");
                builder.Append($" ON {tableSegment.OnExpr} ");
            }
        }
        builder.Append("SET ");
        builder.Append(this.setSql);

        if (!string.IsNullOrEmpty(this.whereSql))
            builder.Append(" WHERE " + this.whereSql);
        dbParameters = this.dbParameters;
        return builder.ToString();
    }
    public virtual IUpdateVisitor From(params Type[] entityTypes)
    {
        this.IsNeedAlias = true;
        this.isFrom = true;
        int tableIndex = this.TableAsStart + this.tables.Count;
        for (int i = 0; i < entityTypes.Length; i++)
        {
            this.tables.Add(new TableSegment
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
        this.isJoin = true;
        var lambdaExpr = joinOn as LambdaExpression;
        var joinTable = new TableSegment
        {
            EntityType = entityType,
            AliasName = $"{(char)('a' + this.tables.Count)}",
            JoinType = joinType
        };
        this.tables.Add(joinTable);
        this.InitTableAlias(lambdaExpr);
        joinTable.OnExpr = this.VisitConditionExpr(lambdaExpr.Body);
        return this;
    }
    public virtual IUpdateVisitor Set(Expression fieldsExpr)
    {
        var lambdaExpr = fieldsExpr as LambdaExpression;
        var entityMapper = this.tables[0].Mapper;
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(this.setSql))
        {
            builder.Append(this.setSql);
            builder.Append(',');
        }
        var setFields = new List<SetField>();
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
                        setFields.Add(new SetField { MemberMapper = memberMapper, Value = $"({sql})" });
                    }
                    else
                    {
                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                        //只一个成员访问，没有设置语句，什么也不做，忽略
                        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberInfo.Name)
                            continue;
                        setFields.Add(this.AddMemberElement(sqlSegment, memberMapper));
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
                        setFields.Add(new SetField { MemberMapper = memberMapper, Value = $"({sql})" });
                    }
                    else
                    {
                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                        //只一个成员访问，没有设置语句，什么也不做，忽略
                        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberAssignment.Member.Name)
                            continue;
                        setFields.Add(this.AddMemberElement(sqlSegment, memberMapper));
                    }
                }
                break;
        }
        if (setFields != null && setFields.Count > 0)
        {
            for (int i = 0; i < setFields.Count; i++)
            {
                if (i > 0) builder.Append(',');
                if (this.IsNeedAlias)
                    builder.Append("a.");
                builder.Append($"{this.OrmProvider.GetFieldName(setFields[i].MemberMapper.FieldName)}={setFields[i].Value}");
            }
        }
        this.setSql = builder.ToString();
        return this;
    }
    public virtual IUpdateVisitor SetValue(Expression fieldsExpr, object fieldValue)
    {
        var lambdaExpr = fieldsExpr as LambdaExpression;
        var entityMapper = this.tables[0].Mapper;
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(this.setSql))
        {
            builder.Append(this.setSql);
            builder.Append(',');
        }
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        SetField setField = null;
        if (fieldValue is LambdaExpression fromQueryExpr)
        {
            if (fromQueryExpr.Body.GetParameters(out var argumentParameters)
                && argumentParameters.Exists(f => f.Type == typeof(IFromQuery)))
            {
                this.InitTableAlias(fromQueryExpr);
                var newLambdaExpr = Expression.Lambda(fromQueryExpr.Body, fromQueryExpr.Parameters.ToList());
                var sql = this.VisitFromQuery(newLambdaExpr, out var isNeedAlias);
                if (isNeedAlias) this.IsNeedAlias = true;
                setField = new SetField { MemberMapper = memberMapper, Value = $"({sql})" };
            }
        }
        else setField = this.AddMemberElement(memberMapper, fieldValue);
        if (this.IsNeedAlias)
            builder.Append("a.");
        builder.Append($"{this.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={setField.Value}");
        this.setSql = builder.ToString();
        return this;
    }
    public virtual string WithBy(Expression fieldsExpr, object parameters, out List<IDbDataParameter> dbParameters)
    {
        //暂时不支持批量
        var parameterType = parameters.GetType();
        var lambdaExpr = fieldsExpr as LambdaExpression;
        var entityMapper = this.tables[0].Mapper;
        var setFields = new List<SetField>();
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = lambdaExpr.Body as MemberExpression;
                    var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
                    var fieldValue = this.EvaluateAndCache(this.OrmProvider, memberMapper, parameterType, parameters);
                    setFields.Add(this.AddMemberElement(memberMapper, fieldValue));
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

                    var argumentExpr = newExpr.Arguments[i];
                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
                    {
                        var fieldValue = this.EvaluateAndCache(this.OrmProvider, memberMapper, parameterType, parameters);
                        setFields.Add(this.AddMemberElement(memberMapper, fieldValue));
                    }
                    else setFields.Add(this.AddMemberElement(sqlSegment, memberMapper));
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
                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
                    {
                        var fieldValue = this.EvaluateAndCache(this.OrmProvider, memberMapper, parameterType, parameters);
                        setFields.Add(this.AddMemberElement(memberMapper, fieldValue));
                    }
                    else setFields.Add(this.AddMemberElement(sqlSegment, memberMapper));
                }
                break;
        }

        var builder = new StringBuilder();
        builder.Append($"UPDATE {this.OrmProvider.GetTableName(entityMapper.TableName)} SET ");
        if (setFields != null && setFields.Count > 0)
        {
            for (int i = 0; i < setFields.Count; i++)
            {
                var setField = setFields[i];
                if (i > 0) builder.Append(',');
                builder.Append($"{this.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={setField.Value}");
            }
        }
        if (entityMapper.KeyMembers == null || entityMapper.KeyMembers.Count == 0)
            throw new Exception($"模型{entityMapper.EntityType.FullName}未配置主键信息");

        builder.Append(" WHERE ");
        for (int i = 0; i < entityMapper.KeyMembers.Count; i++)
        {
            var keyMember = entityMapper.KeyMembers[i];
            if (i > 0) builder.Append(" AND ");
            var parameterName = $"{this.OrmProvider.ParameterPrefix}k{keyMember.MemberName}";
            builder.Append($"{this.OrmProvider.GetFieldName(keyMember.FieldName)}={parameterName}");
            var keyValue = this.EvaluateAndCache(this.OrmProvider, keyMember, parameterType, parameters);
            if (keyMember.NativeDbType != null)
                this.dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, keyMember.NativeDbType, keyValue));
            else this.dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, keyValue));
        }
        var sql = builder.ToString();
        dbParameters = this.dbParameters;
        return sql;
    }
    public virtual List<IDbDataParameter> WithBulkBy(Expression fieldsExpr, StringBuilder builder, object parameters, int index, out List<IDbDataParameter> fixedDbParameters)
    {
        var lambdaExpr = fieldsExpr as LambdaExpression;
        var entityMapper = this.tables[0].Mapper;
        Type parameterType = null;
        if (this.isFirst)
        {
            parameterType = parameters.GetType();
            this.dbParameters ??= new();
            this.setFields = new List<SetField>();
            this.fixedDbParameters = new List<IDbDataParameter>();
            switch (lambdaExpr.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    {
                        var memberExpr = lambdaExpr.Body as MemberExpression;
                        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
                        setFields.Add(new SetField { MemberMapper = memberMapper });
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

                        var argumentExpr = newExpr.Arguments[i];
                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
                            setFields.Add(new SetField { MemberMapper = memberMapper });
                        //有变量会产生参数
                        else setFields.Add(this.AddMemberElement(sqlSegment, memberMapper, this.fixedDbParameters));
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
                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
                            setFields.Add(new SetField { MemberMapper = memberMapper });
                        //有变量会产生参数
                        else setFields.Add(this.AddMemberElement(sqlSegment, memberMapper, this.fixedDbParameters));
                    }
                    break;
            }
            this.isFirst = false;
        }
        else this.dbParameters.Clear();
        builder.Append($"UPDATE {this.OrmProvider.GetTableName(entityMapper.TableName)} SET ");
        for (int i = 0; i < setFields.Count; i++)
        {
            var setField = setFields[i];
            string parameterName = null;
            if (string.IsNullOrEmpty(setField.Value))
            {
                var fieldValue = this.EvaluateAndCache(this.OrmProvider, setField.MemberMapper, parameterType, parameters);
                parameterName = this.OrmProvider.ParameterPrefix + setField.MemberMapper.MemberName + index.ToString();
                var dbParameter = this.CreateParameter(setField.MemberMapper, parameterName, fieldValue);
                this.dbParameters.Add(dbParameter);
            }
            else parameterName = setField.Value + index.ToString();
            if (i > 0) builder.Append(',');
            builder.Append($"{this.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={parameterName}");
        }
        builder.Append(" WHERE ");
        for (int i = 0; i < entityMapper.KeyMembers.Count; i++)
        {
            var keyMember = entityMapper.KeyMembers[i];
            if (i > 0) builder.Append(" AND ");
            var parameterName = $"{this.OrmProvider.ParameterPrefix}k{keyMember.MemberName}{index}";
            builder.Append($"{this.OrmProvider.GetFieldName(keyMember.FieldName)}={parameterName}");
            var keyValue = this.EvaluateAndCache(this.OrmProvider, keyMember, parameterType, parameters);
            if (keyMember.NativeDbType != null)
                this.dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, keyMember.NativeDbType, keyValue));
            else this.dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, keyValue));
        }
        fixedDbParameters = this.fixedDbParameters;
        return this.dbParameters;
    }
    public virtual IUpdateVisitor Where(Expression whereExpr)
    {
        this.isWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.lastWhereNodeType = OperationType.None;
        this.whereSql = this.VisitConditionExpr(lambdaExpr.Body);
        this.isWhere = false;
        return this;
    }
    public virtual IUpdateVisitor And(Expression whereExpr)
    {
        this.isWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        if (this.lastWhereNodeType == OperationType.Or)
        {
            this.whereSql = $"({this.whereSql})";
            this.lastWhereNodeType = OperationType.And;
        }
        var conditionSql = this.VisitConditionExpr(lambdaExpr.Body);
        if (this.lastWhereNodeType == OperationType.Or)
        {
            conditionSql = $"({conditionSql})";
            this.lastWhereNodeType = OperationType.And;
        }
        if (!string.IsNullOrEmpty(this.whereSql))
            this.whereSql += " AND " + conditionSql;
        else this.whereSql = conditionSql;
        this.isWhere = false;
        return this;
    }
    public override SqlSegment VisitMemberAccess(SqlSegment sqlSegment)
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
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlSegment.Null });
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                    return sqlSegment.Next(memberExpr.Expression);
                }
                else if (memberExpr.Member.Name == nameof(Nullable<bool>.Value))
                    return sqlSegment.Next(memberExpr.Expression);
                else throw new ArgumentException($"不支持的MemberAccess操作，表达式'{memberExpr}'返回值不是boolean类型");
            }

            //各种类型实例成员访问，如：DateTime,TimeSpan,String.Length,List.Count
            if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            {
                //Where(f=>... && f.CreatedAt.Month<5 && ...)
                //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
                var targetSegment = sqlSegment.Next(memberExpr.Expression);
                return formatter.Invoke(this, targetSegment);
            }

            if (memberExpr.IsParameter(out var parameterName))
            {
                //Where(f=>... && f.Amount>5 && ...)
                //Include(f=>f.Buyer); 或是 IncludeMany(f=>f.Orders)
                //Select(f=>new {f.OrderId, ...})
                //Where(f=>f.Order.Id>10)
                //Include(f=>f.Order.Buyer)
                //Select(f=>new {f.Order.OrderId, ...})
                //GroupBy(f=>new {f.Order.OrderId, ...})
                //GroupBy(f=>f.Order.OrderId)
                //OrderBy(f=>new {f.Order.OrderId, ...})
                //OrderBy(f=>f.Order.OrderId)
                var tableSegment = this.tableAlias[parameterName];
                tableSegment.Mapper ??= this.mapProvider.GetEntityMap(tableSegment.EntityType);
                var memberMapper = tableSegment.Mapper.GetMemberMap(memberExpr.Member.Name);

                if (memberMapper.IsIgnore)
                    throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberMapper.MemberName}是忽略成员无法访问");
                if (memberMapper.MemberType.IsEntityType() && !memberMapper.IsNavigation && memberMapper.TypeHandler == null)
                    throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberExpr.Member.Name}不是值类型，未配置为导航属性也没有配置TypeHandler");

                //.NET 枚举类型有时候会解析错误，解析成对应的数值类型，如：a.Gender ?? Gender.Male == Gender.Male
                //如果枚举类型对应的数据库类型是字符串，就会有问题，需要把数字变为枚举，再把枚举的名字入库。
                if (this.isWhere && memberMapper.MemberType.IsEnumType(out var expectType, out _))
                {
                    var targetType = this.OrmProvider.MapDefaultType(memberMapper.NativeDbType);
                    sqlSegment.ExpectType = expectType;
                    sqlSegment.TargetType = targetType;
                }

                var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                if (this.IsNeedAlias)
                    fieldName = tableSegment.AliasName + "." + fieldName;

                sqlSegment.HasField = true;
                sqlSegment.IsConstantValue = false;
                sqlSegment.TableSegment = tableSegment;
                sqlSegment.FromMember = memberMapper.Member;
                sqlSegment.Value = fieldName;
                return sqlSegment;
            }
        }

        if (memberExpr.Member.DeclaringType == typeof(DBNull))
            return SqlSegment.Null;

        //各种静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            return formatter.Invoke(this, sqlSegment);

        //访问局部变量或是成员变量，当作常量处理,直接计算，如果是字符串变成参数@p
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId)); orderIds
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        sqlSegment = this.Evaluate(sqlSegment);

        //只有WithBy场景会走此参数化
        this.ConvertTo(sqlSegment);
        sqlSegment.IsConstantValue = true;
        sqlSegment.IsExpression = false;
        sqlSegment.IsMethodCall = false;
        return sqlSegment;
    }
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
        if (newExpr.IsParameter(out _))
            throw new NotSupportedException($"不支持的表达式访问,{newExpr}");
        return this.Evaluate(sqlSegment);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        if (memberInitExpr.IsParameter(out _))
            throw new NotSupportedException($"不支持的表达式访问,{memberInitExpr}");
        return this.Evaluate(sqlSegment);
    }
    public override SqlSegment VisitMethodCall(SqlSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        if (methodCallExpr.Method.DeclaringType == typeof(Sql)
            || typeof(IAggregateSelect).IsAssignableFrom(methodCallExpr.Method.DeclaringType))
            return this.VisitSqlMethodCall(sqlSegment);

        SqlSegment target = null;
        if (methodCallExpr.Object != null)
            target = new SqlSegment { Expression = methodCallExpr.Object };

        SqlSegment[] arguments = null;
        if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count > 0)
        {
            var argumentSegments = new List<SqlSegment>();
            //如果target为null，第一个参数，直接使用现有对象sqlSegment
            for (int i = 0; i < methodCallExpr.Arguments.Count; i++)
            {
                argumentSegments.Add(new SqlSegment { Expression = methodCallExpr.Arguments[i] });
            }
            arguments = argumentSegments.ToArray();
        }
        if (!sqlSegment.IsDeferredFields && this.OrmProvider.TryGetMethodCallSqlFormatter(methodCallExpr, out var formatter))
            return formatter.Invoke(this, target, sqlSegment.DeferredExprs, arguments);

        var lambdaExpr = Expression.Lambda(sqlSegment.Expression);
        var objValue = lambdaExpr.Compile().DynamicInvoke();
        if (objValue == null)
            return SqlSegment.Null;

        //把方法返回值当作常量处理
        return sqlSegment.Change(objValue, true, false, false);
    }
    protected void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.tableAlias.Clear();
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
            this.tableAlias.Add(parameterExpr.Name, this.tables[index]);
            index++;
        }
    }
    protected SetField AddMemberElement(MemberMap memberMapper, object fieldValue)
    {
        if (fieldValue is DBNull)
            return new SetField { MemberMapper = memberMapper, Value = "NULL" };
        else
        {
            this.dbParameters ??= new();
            IDbDataParameter dbParameter = null;
            var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
            if (this.dbParameters.Exists(f => f.ParameterName == parameterName))
                parameterName = this.OrmProvider.ParameterPrefix + this.parameterPrefix + this.dbParameters.Count.ToString();

            if (memberMapper.TypeHandler != null)
            {
                if (memberMapper.NativeDbType != null)
                    dbParameter = this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue);
                else dbParameter = this.OrmProvider.CreateParameter(parameterName, fieldValue);
                memberMapper.TypeHandler.SetValue(this.OrmProvider, dbParameter, fieldValue);
            }
            else
            {
                if (memberMapper.NativeDbType != null)
                {
                    fieldValue = this.OrmProvider.ToFieldValue(fieldValue, memberMapper.NativeDbType);
                    dbParameter = this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue);
                }
                else dbParameter = this.OrmProvider.CreateParameter(parameterName, fieldValue);
            }
            this.dbParameters.Add(dbParameter);
            return new SetField { MemberMapper = memberMapper, Value = parameterName };
        }
    }
    protected SetField AddMemberElement(SqlSegment sqlSegment, MemberMap memberMapper)
    {
        if (sqlSegment == SqlSegment.Null)
            return new SetField { MemberMapper = memberMapper, Value = "NULL" };
        else
        {
            if (sqlSegment.IsConstantValue)
            {
                this.dbParameters ??= new();
                var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
                if (this.dbParameters.Exists(f => f.ParameterName == parameterName))
                    parameterName = this.OrmProvider.ParameterPrefix + this.parameterPrefix + this.dbParameters.Count.ToString();

                if (sqlSegment.IsArray && sqlSegment.Value is List<SqlSegment> sqlSegments)
                    sqlSegment.Value = sqlSegments.Select(f => f.Value).ToArray();
                var dbParameter = this.CreateParameter(memberMapper, parameterName, sqlSegment.Value);
                this.dbParameters.Add(dbParameter);
                sqlSegment.Value = parameterName;
                sqlSegment.IsParameter = true;
                sqlSegment.IsConstantValue = false;
                return new SetField { MemberMapper = memberMapper, Value = sqlSegment.Value.ToString() };
            }
            return new SetField { MemberMapper = memberMapper, Value = sqlSegment.ToString() };
        }
    }
    protected SetField AddMemberElement(SqlSegment sqlSegment, MemberMap memberMapper, List<IDbDataParameter> dbParameters)
    {
        if (sqlSegment == SqlSegment.Null)
            return new SetField { MemberMapper = memberMapper, Value = "NULL" };
        else
        {
            if (sqlSegment.IsConstantValue)
            {
                var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
                if (dbParameters.Exists(f => f.ParameterName == parameterName))
                    parameterName = this.OrmProvider.ParameterPrefix + this.parameterPrefix + dbParameters.Count.ToString();

                if (sqlSegment.IsArray && sqlSegment.Value is List<SqlSegment> sqlSegments)
                    sqlSegment.Value = sqlSegments.Select(f => f.Value).ToArray();

                var dbParameter = this.CreateParameter(memberMapper, parameterName, sqlSegment.Value);
                dbParameters.Add(dbParameter);
                sqlSegment.Value = parameterName;
                sqlSegment.IsParameter = true;
                sqlSegment.IsConstantValue = false;
                return new SetField { MemberMapper = memberMapper, Value = sqlSegment.Value.ToString() };
            }
            return new SetField { MemberMapper = memberMapper, Value = sqlSegment.ToString() };
        }
    }
    protected IDbDataParameter CreateParameter(MemberMap memberMapper, string parameterName, object fieldValue)
    {
        IDbDataParameter dbParameter = null;
        if (memberMapper.TypeHandler != null)
        {
            if (memberMapper.NativeDbType != null)
                dbParameter = this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue);
            else dbParameter = this.OrmProvider.CreateParameter(parameterName, fieldValue);
            memberMapper.TypeHandler.SetValue(this.OrmProvider, dbParameter, fieldValue);
        }
        else
        {
            if (memberMapper.NativeDbType != null)
            {
                var parameters = this.OrmProvider.ToFieldValue(fieldValue, memberMapper.NativeDbType);
                dbParameter = this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, parameters);
            }
            else dbParameter = this.OrmProvider.CreateParameter(parameterName, fieldValue);
        }
        return dbParameter;
    }
    protected object EvaluateAndCache(IOrmProvider ormProvider, MemberMap memberMapper, Type type, object parameters)
    {
        var cacheKey = HashCode.Combine(type, memberMapper.MemberName);
        if (!getterCache.TryGetValue(cacheKey, out var getter))
        {
            var objExpr = Expression.Parameter(typeof(object), "obj");
            Expression valueExpr = Expression.PropertyOrField(Expression.Convert(objExpr, type), memberMapper.MemberName);
            if (memberMapper.NativeDbType != null)
            {
                var ormProviderExpr = Expression.Constant(ormProvider);
                var methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.ToFieldValue));
                var fieldValueExpr = Expression.Convert(valueExpr, typeof(object));
                var nativeDbTypeExpr = Expression.Constant(memberMapper.NativeDbType, typeof(object));
                valueExpr = Expression.Call(ormProviderExpr, methodInfo, fieldValueExpr, nativeDbTypeExpr);
            }
            getter = Expression.Lambda<Func<object, object>>(Expression.Convert(valueExpr, typeof(object)), objExpr).Compile();
            getterCache.TryAdd(cacheKey, getter);
        }
        return getter.Invoke(parameters);
    }
}
