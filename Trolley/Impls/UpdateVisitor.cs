using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

class UpdateVisitor : SqlVisitor
{
    private bool isFrom = false;
    private bool isJoin = false;
    private string whereSql = string.Empty;
    private string setSql = string.Empty;

    public UpdateVisitor(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, Type entityType, char tableStartAs = 'a')
        : base(dbFactory, connection, transaction, tableStartAs)
    {
        this.tables = new();
        this.tableAlias = new();
        this.tables.Add(new TableSegment
        {
            EntityType = entityType,
            Mapper = this.dbFactory.GetEntityMap(entityType),
            AliasName = tableStartAs.ToString()
        });
        switch (this.ormProvider.DatabaseType)
        {
            case DatabaseType.SqlServer:
                this.tables[0].AliasName = this.ormProvider.GetTableName(this.tables[0].Mapper.TableName);
                break;
        }
    }
    public string BuildSql(out List<IDbDataParameter> dbParameters)
    {
        var entityTableName = this.ormProvider.GetTableName(this.tables[0].Mapper.TableName);
        var builder = new StringBuilder($"UPDATE {entityTableName} ");
        switch (this.ormProvider.DatabaseType)
        {
            case DatabaseType.MySql:
                if (this.isNeedAlias) builder.Append("a ");
                if (isJoin && this.tables.Count > 1)
                {
                    for (var i = 1; i < this.tables.Count; i++)
                    {
                        var tableSegment = this.tables[i];
                        var tableName = tableSegment.Body;
                        if (string.IsNullOrEmpty(tableName))
                        {
                            tableSegment.Mapper ??= this.dbFactory.GetEntityMap(tableSegment.EntityType);
                            tableName = this.ormProvider.GetTableName(tableSegment.Mapper.TableName);
                        }
                        builder.Append($"{tableSegment.JoinType} {tableName} {tableSegment.AliasName}");
                        builder.Append($" ON {tableSegment.OnExpr}");
                    }
                }
                builder.Append("SET ");
                builder.Append(this.setSql);
                break;
            case DatabaseType.Postgresql:
                if (this.isNeedAlias) builder.Append("a ");
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
                            tableSegment.Mapper ??= this.dbFactory.GetEntityMap(tableSegment.EntityType);
                            tableName = this.ormProvider.GetTableName(tableSegment.Mapper.TableName);
                        }
                        builder.Append($"{tableName} {tableSegment.AliasName}");
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
                            tableSegment.Mapper ??= this.dbFactory.GetEntityMap(tableSegment.EntityType);
                            tableName = this.ormProvider.GetTableName(tableSegment.Mapper.TableName);
                        }
                        builder.Append($"{tableName} {tableSegment.AliasName}");
                    }
                }
                break;
            case DatabaseType.Oracle:
                if (this.isNeedAlias) builder.Append("a ");
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
    public UpdateVisitor From(params Type[] entityTypes)
    {
        this.isNeedAlias = true;
        this.isFrom = true;
        switch (this.ormProvider.DatabaseType)
        {
            case DatabaseType.MySql:
                throw new NotSupportedException("MySql不支持Update From语法，支持Update InnerJoin/LeftJoin语法");
            case DatabaseType.Oracle:
                throw new NotSupportedException("Oracle不支持Update From语法，支持Update Set Field=(subQuery)语法");
        }
        int tableIndex = this.tableStartAs + this.tables.Count;
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
    public UpdateVisitor Join(string joinType, Type entityType, Expression joinOn)
    {
        this.isNeedAlias = true;
        this.isJoin = true;
        switch (this.ormProvider.DatabaseType)
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
            AliasName = $"{(char)(97 + this.tables.Count)}",
            JoinType = joinType
        };
        this.tables.Add(joinTable);
        this.InitTableAlias(lambdaExpr);
        joinTable.OnExpr = this.VisitConditionExpr(lambdaExpr.Body);
        return this;
    }
    public UpdateVisitor Set(Expression fieldsExpr, object fieldValue = null)
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
                setFields.Add(this.AddMemberElement(fieldValue, memberMapper));
                break;
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out memberMapper))
                        continue;

                    //只一个成员访问，没有设置语句，什么也不做，忽略
                    var argumentExpr = newExpr.Arguments[i];
                    if (argumentExpr is MemberExpression newMemberExpr && newMemberExpr.Member.Name == memberInfo.Name)
                        continue;
                    setFields.Add(this.AddMemberElement(new SqlSegment { Expression = argumentExpr }, memberMapper));
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

                    //只一个成员访问，没有设置语句，什么也不做，忽略
                    var argumentExpr = memberAssignment.Expression;
                    if (argumentExpr is MemberExpression newMemberExpr && newMemberExpr.Member.Name == memberAssignment.Member.Name)
                        continue;
                    setFields.Add(this.AddMemberElement(new SqlSegment { Expression = argumentExpr }, memberMapper));
                }
                break;
        }
        if (setFields != null && setFields.Count > 0)
        {
            for (int i = 0; i < setFields.Count; i++)
            {
                if (i > 0) builder.Append(',');
                if (this.isNeedAlias)
                {
                    switch (this.ormProvider.DatabaseType)
                    {
                        case DatabaseType.MySql:
                        case DatabaseType.Oracle:
                            builder.Append("a.");
                            break;
                    }
                }
                builder.Append($"{ormProvider.GetFieldName(setFields[i].MemberMapper.FieldName)}={setFields[i].Value}");
            }
        }
        this.setSql = builder.ToString();
        return this;
    }
    public SqlSegment SetValue(Expression valueExpr, out List<IDbDataParameter> dbParameters)
    {
        var result = this.VisitAndDeferred(new SqlSegment { Expression = valueExpr });
        dbParameters = this.dbParameters;
        return result;
    }
    public SqlSegment SetValue(Expression fieldsExpr, Expression valueExpr, out List<IDbDataParameter> dbParameters)
    {
        this.InitTableAlias(fieldsExpr as LambdaExpression);
        var result = this.VisitAndDeferred(new SqlSegment { Expression = valueExpr });
        dbParameters = this.dbParameters;
        return result;
    }
    public UpdateVisitor SetFromQuery(Expression fieldsExpr, Expression valueExpr = null)
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
                var memberExpr = lambdaExpr.Body as MemberExpression;
                if (!entityMapper.TryGetMemberMap(memberExpr.Member.Name, out memberMapper))
                    throw new ArgumentException($"模型{entityMapper.EntityType.FullName}不存在成员{memberMapper.MemberName}");

                if (valueExpr.GetParameters(out argumentParameters)
                   && argumentParameters.Exists(f => f.Type == typeof(IFromQuery)))
                {
                    var lambdaValuesExpr = valueExpr as LambdaExpression;
                    var newLambdaExpr = Expression.Lambda(lambdaValuesExpr.Body, lambdaValuesExpr.Parameters.ToList());
                    var sql = this.VisitFromQuery(newLambdaExpr, out var isNeedAlias);
                    setFields.Add(new SetField { MemberMapper = memberMapper, Value = $"({sql})" });
                    if (isNeedAlias) this.isNeedAlias = true;
                }
                else setFields.Add(this.AddMemberElement(new SqlSegment { Expression = valueExpr }, memberMapper));
                break;
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out memberMapper))
                        continue;

                    //只一个成员访问，没有设置语句，什么也不做，忽略
                    var argumentExpr = newExpr.Arguments[i];
                    if (argumentExpr is MemberExpression newMemberExpr && newMemberExpr.Member.Name == memberInfo.Name)
                        continue;

                    if (argumentExpr.GetParameters(out argumentParameters)
                        && argumentParameters.Exists(f => f.Type == typeof(IFromQuery)))
                    {
                        var newLambdaExpr = Expression.Lambda(argumentExpr, lambdaExpr.Parameters.ToList());
                        var sql = this.VisitFromQuery(newLambdaExpr, out var isNeedAlias);
                        if (isNeedAlias) this.isNeedAlias = true;
                        setFields.Add(new SetField { MemberMapper = memberMapper, Value = $"({sql})" });
                    }
                    else setFields.Add(this.AddMemberElement(new SqlSegment { Expression = argumentExpr }, memberMapper));
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

                    //只一个成员访问，没有设置语句，什么也不做，忽略
                    var argumentExpr = memberAssignment.Expression;
                    if (argumentExpr is MemberExpression newMemberExpr && newMemberExpr.Member.Name == memberAssignment.Member.Name)
                        continue;

                    if (argumentExpr.GetParameters(out argumentParameters)
                        && argumentParameters.Exists(f => f.Type == typeof(IFromQuery)))
                    {
                        var newLambdaExpr = Expression.Lambda(argumentExpr, lambdaExpr.Parameters.ToList());
                        var sql = this.VisitFromQuery(newLambdaExpr, out var isNeedAlias);
                        if (isNeedAlias) this.isNeedAlias = true;
                        setFields.Add(new SetField { MemberMapper = memberMapper, Value = $"({sql})" });
                    }
                    else setFields.Add(this.AddMemberElement(new SqlSegment { Expression = argumentExpr }, memberMapper));
                }
                break;
        }
        if (setFields != null && setFields.Count > 0)
        {
            for (int i = 0; i < setFields.Count; i++)
            {
                if (i > 0) builder.Append(',');
                if (this.isNeedAlias)
                {
                    switch (this.ormProvider.DatabaseType)
                    {
                        case DatabaseType.MySql:
                        case DatabaseType.Oracle:
                            builder.Append("a.");
                            break;
                    }
                }
                builder.Append($"{ormProvider.GetFieldName(setFields[i].MemberMapper.FieldName)}={setFields[i].Value}");
            }
        }
        this.setSql = builder.ToString();
        return this;
    }
    public UpdateVisitor Where(Expression whereExpr)
    {
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.whereSql = " WHERE " + this.VisitConditionExpr(lambdaExpr.Body);
        return this;
    }
    public UpdateVisitor And(Expression whereExpr)
    {
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.whereSql += " AND " + this.VisitConditionExpr(lambdaExpr.Body);
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

            //各种类型值的属性访问，如：DateTime,TimeSpan,String.Length,List.Count,
            if (this.ormProvider.TryGetMemberAccessSqlFormatter(sqlSegment, memberExpr.Member, out formatter))
            {
                //Where(f=>... && f.CreatedAt.Month<5 && ...)
                //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
                var targetSegment = this.Visit(sqlSegment.Next(memberExpr.Expression));
                return sqlSegment.Change(formatter.Invoke(targetSegment), false);
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
                tableSegment.Mapper ??= this.dbFactory.GetEntityMap(tableSegment.EntityType);
                var memberMapper = tableSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                var fieldName = this.ormProvider.GetFieldName(memberMapper.FieldName);
                if (this.isNeedAlias)
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

        //各种类型的常量或是静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (this.ormProvider.TryGetMemberAccessSqlFormatter(sqlSegment, memberExpr.Member, out formatter))
            return sqlSegment.Change(formatter(null), false);

        //访问局部变量或是成员变量，当作常量处理,直接计算，如果是字符串变成参数@p
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId)); orderIds
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        return this.EvaluateAndParameter(sqlSegment);
    }
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
        if (newExpr.Type.Name.StartsWith("<>"))
        {
            var builder = new StringBuilder();
            var entityMapper = this.tables[0].Mapper;
            var setFields = new List<SetField>();
            for (int i = 0; i < newExpr.Arguments.Count; i++)
            {
                var memberInfo = newExpr.Members[i];
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                    continue;
                setFields.Add(this.AddMemberElement(sqlSegment.Next(newExpr.Arguments[i]), memberMapper));
            }
            if (setFields != null && setFields.Count > 0)
            {
                for (int i = 0; i < setFields.Count; i++)
                {
                    if (i > 0) builder.Append(',');
                    if (this.isNeedAlias)
                    {
                        switch (this.ormProvider.DatabaseType)
                        {
                            case DatabaseType.MySql:
                            case DatabaseType.Oracle:
                                builder.Append("a.");
                                break;
                        }
                    }
                    builder.Append($"{ormProvider.GetFieldName(setFields[i].MemberMapper.FieldName)}={setFields[i].Value}");
                }
            }
            return sqlSegment.Change(builder.ToString());
        }
        return this.EvaluateAndParameter(sqlSegment);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        var builder = new StringBuilder();
        var entityMapper = this.tables[0].Mapper;
        var setFields = new List<SetField>();
        for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
        {
            if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                throw new NotImplementedException($"不支持除MemberBindingType.Assignment类型外的成员绑定表达式, {memberInitExpr.Bindings[i]}");
            var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
            if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
                continue;
            setFields.Add(this.AddMemberElement(sqlSegment.Next(memberAssignment.Expression), memberMapper));
        }
        if (setFields != null && setFields.Count > 0)
        {
            for (int i = 0; i < setFields.Count; i++)
            {
                if (i > 0) builder.Append(',');
                if (this.isNeedAlias)
                {
                    switch (this.ormProvider.DatabaseType)
                    {
                        case DatabaseType.MySql:
                        case DatabaseType.Oracle:
                            builder.Append("a.");
                            break;
                    }
                }
                builder.Append($"{ormProvider.GetFieldName(setFields[i].MemberMapper.FieldName)}={setFields[i].Value}");
            }
        }
        return sqlSegment.Change(builder.ToString());
    }
    public override SqlSegment EvaluateAndParameter(SqlSegment sqlSegment)
    {
        var member = Expression.Convert(sqlSegment.Expression, typeof(object));
        var lambda = Expression.Lambda<Func<object>>(member);
        var getter = lambda.Compile();
        var objValue = getter();
        if (objValue == null)
            return SqlSegment.Null;

        var parameterName = sqlSegment.ParameterName;
        if (string.IsNullOrEmpty(parameterName))
            parameterName = this.ormProvider.ParameterPrefix + this.parameterPrefix + this.dbParameters.Count.ToString();
        this.dbParameters ??= new();
        this.dbParameters.Add(this.ormProvider.CreateParameter(parameterName, sqlSegment.Value));
        return sqlSegment.Change(sqlSegment.ParameterName, false);
    }
    private void InitTableAlias(LambdaExpression lambdaExpr)
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
    private SetField AddMemberElement(object fieldValue, MemberMap memberMapper)
    {
        if (fieldValue is DBNull)
            return new SetField { MemberMapper = memberMapper, Value = "NULL" };
        else
        {
            var parameterName = ormProvider.ParameterPrefix + memberMapper.MemberName;
            this.dbParameters ??= new();
            if (memberMapper.NativeDbType.HasValue)
                this.dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType.Value, fieldValue));
            else this.dbParameters.Add(ormProvider.CreateParameter(parameterName, fieldValue));
            return new SetField { MemberMapper = memberMapper, Value = parameterName };
        }
    }
    private SetField AddMemberElement(SqlSegment sqlSegment, MemberMap memberMapper)
    {
        var parameterName = this.ormProvider.ParameterPrefix + memberMapper.MemberName;
        sqlSegment.ParameterName = parameterName;
        sqlSegment = this.VisitAndDeferred(sqlSegment);

        if (sqlSegment == SqlSegment.Null)
            return new SetField { MemberMapper = memberMapper, Value = "NULL" };
        else
        {
            if (sqlSegment.IsConstantValue)
            {
                if (!sqlSegment.IsParameter)
                {
                    this.dbParameters ??= new();
                    if (memberMapper.NativeDbType.HasValue)
                        this.dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType.Value, sqlSegment.Value));
                    else this.dbParameters.Add(ormProvider.CreateParameter(parameterName, sqlSegment.Value));
                }
                return new SetField { MemberMapper = memberMapper, Value = parameterName };
            }
            return new SetField { MemberMapper = memberMapper, Value = sqlSegment.ToString() };
        }
    }
}
