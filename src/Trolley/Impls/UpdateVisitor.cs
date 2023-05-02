using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class UpdateVisitor : SqlVisitor, IUpdateVisitor
{
    protected bool isFrom = false;
    protected bool isJoin = false;
    protected string whereSql = string.Empty;
    protected string setSql = string.Empty;

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
        switch (this.OrmProvider.DatabaseType)
        {
            case DatabaseType.MySql:
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
                break;
            case DatabaseType.Postgresql:
                if (this.IsNeedAlias) builder.Append("a ");
                builder.Append("SET ");
                builder.Append(this.setSql);
                if (this.isFrom && this.tables.Count > 1)
                {
                    builder.Append(" FROM ");
                    for (var i = 1; i < this.tables.Count; i++)
                    {
                        var tableSegment = this.tables[i];
                        var tableName = tableSegment.Body;
                        if (string.IsNullOrEmpty(tableName))
                        {
                            tableSegment.Mapper ??= this.mapProvider.GetEntityMap(tableSegment.EntityType);
                            tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
                        }
                        builder.Append($"{tableName} {tableSegment.AliasName} ");
                    }
                }
                break;
            case DatabaseType.SqlServer:
                builder.Append("SET ");
                builder.Append(this.setSql);
                if (this.isFrom && this.tables.Count > 1)
                {
                    builder.Append(" FROM ");
                    for (var i = 1; i < this.tables.Count; i++)
                    {
                        var tableSegment = this.tables[i];
                        var tableName = tableSegment.Body;
                        if (string.IsNullOrEmpty(tableName))
                        {
                            tableSegment.Mapper ??= this.mapProvider.GetEntityMap(tableSegment.EntityType);
                            tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
                        }
                        builder.Append($"{tableName} {tableSegment.AliasName}");
                    }
                }
                break;
            case DatabaseType.Oracle:
                if (this.IsNeedAlias) builder.Append("a ");
                if (this.isFrom || this.isJoin)
                    throw new NotSupportedException("Oracle不支持UPDATE FROM/JOIN语句");
                builder.Append("SET ");
                builder.Append(this.setSql);
                break;
        }
        if (!string.IsNullOrEmpty(this.whereSql))
            builder.Append(this.whereSql);
        dbParameters = this.dbParameters;
        return builder.ToString();
    }
    public virtual IUpdateVisitor From(params Type[] entityTypes)
    {
        this.IsNeedAlias = true;
        this.isFrom = true;
        switch (this.OrmProvider.DatabaseType)
        {
            case DatabaseType.MySql:
                throw new NotSupportedException("MySql不支持Update From语法，支持Update InnerJoin/LeftJoin语法");
            case DatabaseType.Oracle:
                throw new NotSupportedException("Oracle不支持Update From语法，支持Update Set Field=(subQuery)语法");
        }
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
        switch (this.OrmProvider.DatabaseType)
        {
            case DatabaseType.Postgresql:
                throw new NotSupportedException("PostgreSql不支持Update Join语法，支持Update From语法");
            case DatabaseType.SqlServer:
                throw new NotSupportedException("SqlServer不支持Update Join语法，支持Update From语法");
            case DatabaseType.Oracle:
                throw new NotSupportedException("Oracle不支持Update Join语法，支持Update Set Field=(subQuery)语法");
        }
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
    public virtual IUpdateVisitor Set(Expression fieldsExpr, object fieldValue = null)
    {
        var lambdaExpr = fieldsExpr as LambdaExpression;
        var entityMapper = this.tables[0].Mapper;
        MemberMap memberMapper = null;
        var setFields = new List<SetField>();

        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(this.setSql))
        {
            builder.Append(this.setSql);
            builder.Append(',');
        }
        switch (lambdaExpr.Body.NodeType)
        {
            //单个字段设置
            case ExpressionType.MemberAccess:
                var memberExpr = lambdaExpr.Body as MemberExpression;
                memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
                setFields.Add(this.AddMemberElement(memberMapper, fieldValue));
                break;
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out memberMapper))
                        continue;

                    var argumentExpr = newExpr.Arguments[i];
                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                    //只一个成员访问，没有设置语句，什么也不做，忽略
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberInfo.Name)
                        continue;
                    setFields.Add(this.AddMemberElement(sqlSegment, memberMapper));
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

                    var argumentExpr = memberAssignment.Expression;
                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                    //只一个成员访问，没有设置语句，什么也不做，忽略
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberAssignment.Member.Name)
                        continue;
                    setFields.Add(this.AddMemberElement(sqlSegment, memberMapper));
                }
                break;
        }
        if (setFields != null && setFields.Count > 0)
        {
            for (int i = 0; i < setFields.Count; i++)
            {
                if (i > 0) builder.Append(',');
                if (this.IsNeedAlias)
                {
                    switch (this.OrmProvider.DatabaseType)
                    {
                        case DatabaseType.MySql:
                        case DatabaseType.Oracle:
                            builder.Append("a.");
                            break;
                    }
                }
                builder.Append($"{OrmProvider.GetFieldName(setFields[i].MemberMapper.FieldName)}={setFields[i].Value}");
            }
        }
        this.setSql = builder.ToString();
        return this;
    }
    public virtual SetField SetValue(MemberMap memberMapper, Expression valueExpr)
    {
        this.dbParameters.Clear();
        var sqlSegment = new SqlSegment { Expression = valueExpr };
        sqlSegment = this.VisitAndDeferred(sqlSegment);
        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
            return new SetField { MemberMapper = memberMapper };

        if (this.isParameterized)
            sqlSegment.ParameterName = memberMapper.MemberName;
        var result = this.AddMemberElement(sqlSegment, memberMapper);
        if (this.dbParameters.Count > 0)
        {
            result.DbParameters ??= new();
            result.DbParameters.AddRange(this.dbParameters);
        }
        return result;
    }
    public virtual SetField SetValue(Expression fieldsExpr, MemberMap memberMapper, Expression valueExpr)
    {
        this.InitTableAlias(fieldsExpr as LambdaExpression);
        this.dbParameters = new List<IDbDataParameter>();
        return this.SetValue(memberMapper, valueExpr);
    }
    public virtual IUpdateVisitor SetFromQuery(Expression fieldsExpr, Expression valueExpr = null)
    {
        var lambdaExpr = fieldsExpr as LambdaExpression;
        var entityMapper = this.tables[0].Mapper;
        MemberMap memberMapper = null;
        List<ParameterExpression> argumentParameters = null;
        var setFields = new List<SetField>();
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(this.setSql))
        {
            builder.Append(this.setSql);
            builder.Append(',');
        }
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = lambdaExpr.Body as MemberExpression;
                    memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
                    lambdaExpr = valueExpr as LambdaExpression;
                    this.InitTableAlias(lambdaExpr);
                    var sql = this.VisitFromQuery(lambdaExpr, out var isNeedAlias);
                    if (isNeedAlias) this.IsNeedAlias = true;
                    setFields.Add(new SetField { MemberMapper = memberMapper, Value = $"({sql})" });
                }
                break;
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out memberMapper))
                        continue;

                    var argumentExpr = newExpr.Arguments[i];
                    if (argumentExpr.GetParameters(out argumentParameters)
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
                    if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out memberMapper))
                        continue;

                    var argumentExpr = memberAssignment.Expression;
                    if (argumentExpr.GetParameters(out argumentParameters)
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
                {
                    switch (this.OrmProvider.DatabaseType)
                    {
                        case DatabaseType.MySql:
                        case DatabaseType.Oracle:
                            builder.Append("a.");
                            break;
                    }
                }
                builder.Append($"{OrmProvider.GetFieldName(setFields[i].MemberMapper.FieldName)}={setFields[i].Value}");
            }
        }
        this.setSql = builder.ToString();
        return this;
    }
    public virtual IUpdateVisitor Where(Expression whereExpr)
    {
        this.isWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.whereSql = " WHERE " + this.VisitConditionExpr(lambdaExpr.Body);
        this.isWhere = false;
        return this;
    }
    public virtual IUpdateVisitor And(Expression whereExpr)
    {
        this.isWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.whereSql += " AND " + this.VisitConditionExpr(lambdaExpr.Body);
        this.isWhere = false;
        return this;
    }
    public override SqlSegment VisitConstant(SqlSegment sqlSegment)
    {
        if (this.isParameterized || sqlSegment.IsParameterized)
            return this.ToParameter(base.VisitConstant(sqlSegment));
        return base.VisitConstant(sqlSegment);
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

        //只有WithBy场景在此参数化，暂时不做参数化，走parameters的参数化处理
        return this.ConvertTo(sqlSegment);
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
                {
                    fieldValue = this.OrmProvider.ToFieldValue(fieldValue, memberMapper.NativeDbType);
                    dbParameter = this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue);
                }
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
                IDbDataParameter dbParameter = null;
                var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
                if (this.dbParameters.Exists(f => f.ParameterName == parameterName))
                    parameterName = this.OrmProvider.ParameterPrefix + this.parameterPrefix + this.dbParameters.Count.ToString();

                if (sqlSegment.IsArray && sqlSegment.Value is List<SqlSegment> sqlSegments)
                    sqlSegment.Value = sqlSegments.Select(f => f.Value).ToArray();

                if (memberMapper.TypeHandler != null)
                {
                    if (memberMapper.NativeDbType != null)
                        dbParameter = this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, sqlSegment.Value);
                    else dbParameter = this.OrmProvider.CreateParameter(parameterName, sqlSegment.Value);
                    memberMapper.TypeHandler.SetValue(this.OrmProvider, dbParameter, sqlSegment.Value);
                }
                else
                {
                    if (memberMapper.NativeDbType != null)
                    {
                        sqlSegment.Value = this.OrmProvider.ToFieldValue(sqlSegment.Value, memberMapper.NativeDbType);
                        dbParameter = this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, sqlSegment.Value);
                    }
                    else dbParameter = this.OrmProvider.CreateParameter(parameterName, sqlSegment.Value);
                }
                this.dbParameters.Add(dbParameter);
                sqlSegment.Value = parameterName;
                sqlSegment.IsParameter = true;
                sqlSegment.IsConstantValue = false;
                return new SetField { MemberMapper = memberMapper, Value = sqlSegment.Value.ToString() };
            }
            return new SetField { MemberMapper = memberMapper, Value = sqlSegment.ToString() };
        }
    }
}
