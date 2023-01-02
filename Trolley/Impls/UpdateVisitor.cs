using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

class UpdateVisitor : SqlVisitor
{
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
    }
    public string BuildSql(out List<IDbDataParameter> dbParameters)
    {
        var entityTableName = this.ormProvider.GetTableName(this.tables[0].Mapper.TableName);
        var builder = new StringBuilder($"UPDATE {entityTableName} ");
        switch (this.ormProvider.DatabaseType)
        {
            case DatabaseType.MySql:
                builder.Append("a ");
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
                builder.Append(" SET ");
                builder.Append(this.setSql);
                break;
            case DatabaseType.Postgresql:
                builder.Append("a SET ");
                builder.Append(this.setSql);
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
                break;
            case DatabaseType.MsSql:
                builder.Append("SET ");
                builder.Append(this.setSql);
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
                break;
            case DatabaseType.Oracle:
                throw new NotSupportedException("Oracle暂时不支持UPDATE FROM语句");
        }
        if (!string.IsNullOrEmpty(this.whereSql))
            builder.Append(this.whereSql);
        dbParameters = this.dbParameters;
        return builder.ToString();
    }
    public UpdateVisitor From(params Type[] entityTypes)
    {
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
        var lambdaExpr = joinOn as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.tables.Add(new TableSegment
        {
            EntityType = entityType,
            AliasName = $"{(char)(97 + this.tables.Count)}",
            JoinType = joinType,
            OnExpr = this.VisitConditionExpr(lambdaExpr.Body)
        });
        return this;
    }
    public UpdateVisitor Set(Expression setExpr, object fieldValue = null)
    {
        if (setExpr == null)
            throw new ArgumentNullException(nameof(setExpr));
        var lambdaExpr = setExpr as LambdaExpression;

        var entityMapper = this.tables[0].Mapper;
        var builder = new StringBuilder();
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                if (fieldValue == null)
                    throw new ArgumentNullException(nameof(fieldValue));
                if (!string.IsNullOrEmpty(this.setSql))
                    builder.Append(this.setSql);
                var memberExpr = lambdaExpr.Body as MemberExpression;
                var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
                var parameterName = ormProvider.ParameterPrefix + memberMapper.MemberName;
                if (builder.Length > 0)
                    builder.Append(',');
                if (this.ormProvider.DatabaseType == DatabaseType.MySql)
                    builder.Append("a.");
                builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                this.dbParameters ??= new();
                this.dbParameters.Add(ormProvider.CreateParameter(parameterName, fieldValue));
                break;
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out _))
                        continue;
                    this.AddMemberElement(new SqlSegment { Expression = newExpr.Arguments[i] }, memberInfo, builder);
                }
                break;
            case ExpressionType.MemberInit:
                this.InitTableAlias(lambdaExpr);
                var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out _))
                        continue;
                    this.AddMemberElement(new SqlSegment { Expression = memberAssignment.Expression }, memberAssignment.Member, builder);
                }
                break;
        }
        this.setSql = builder.ToString();
        return this;
    }
    public UpdateVisitor Set<TField>(Expression setExpr, IQuery<TField> subQuery)
    {
        if (setExpr == null)
            throw new ArgumentNullException(nameof(setExpr));
        var lambdaExpr = setExpr as LambdaExpression;

        var entityMapper = this.tables[0].Mapper;
        var builder = new StringBuilder();
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                if (subQuery == null)
                    throw new ArgumentNullException(nameof(subQuery));

                if (!string.IsNullOrEmpty(this.setSql))
                    builder.Append(this.setSql);
                var memberExpr = lambdaExpr.Body as MemberExpression;
                var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
                if (builder.Length > 0)
                    builder.Append(',');
                if (this.ormProvider.DatabaseType == DatabaseType.MySql)
                    builder.Append("a.");
                builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}=");
                var sql = subQuery.ToSql(out var dbDataParameters);
                builder.Append($"({sql})");
                this.dbParameters ??= new();
                if (dbDataParameters != null && dbDataParameters.Count > 0)
                    dbDataParameters.ForEach(f => this.dbParameters.Add(f));
                break;
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out _))
                        continue;
                    this.AddMemberElement(new SqlSegment { Expression = newExpr.Arguments[i] }, memberInfo, builder);
                }
                break;
            case ExpressionType.MemberInit:
                this.InitTableAlias(lambdaExpr);
                var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out _))
                        continue;
                    this.AddMemberElement(new SqlSegment { Expression = memberAssignment.Expression }, memberAssignment.Member, builder);
                }
                break;
            default:
                throw new NotSupportedException("只支持MemberAccess类型表达式");
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
            if (this.ormProvider.TryGetMemberAccessSqlFormatter(memberExpr.Member, out formatter))
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
                if (sqlSegment.HasDeferred)
                {
                    sqlSegment.HasField = true;
                    sqlSegment.IsConstantValue = false;
                    sqlSegment.TableSegment = tableSegment;
                    sqlSegment.FromMember = memberMapper.Member;
                    sqlSegment.Value = $"{tableSegment.AliasName}.{fieldName}";
                    return this.VisitBooleanDeferred(sqlSegment);
                }
                sqlSegment.HasField = true;
                sqlSegment.IsConstantValue = false;
                sqlSegment.TableSegment = tableSegment;
                sqlSegment.FromMember = memberMapper.Member;
                sqlSegment.Value = $"{tableSegment.AliasName}.{fieldName}";
                return sqlSegment;
            }
        }
        //各种类型的常量或是静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (this.ormProvider.TryGetMemberAccessSqlFormatter(memberExpr.Member, out formatter))
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
            for (int i = 0; i < newExpr.Arguments.Count; i++)
            {
                var memberInfo = newExpr.Members[i];
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out _))
                    continue;
                this.AddMemberElement(sqlSegment.Next(newExpr.Arguments[i]), memberInfo, builder);
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
        for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
        {
            if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                throw new NotImplementedException($"不支持除MemberBindingType.Assignment类型外的成员绑定表达式, {memberInitExpr.Bindings[i]}");
            var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
            if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out _))
                continue;
            this.AddMemberElement(sqlSegment.Next(memberAssignment.Expression), memberAssignment.Member, builder);
        }
        return sqlSegment.Change(builder.ToString());
    }
    private void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.tableAlias.Clear();
        for (int i = 0; i < this.tables.Count; i++)
        {
            var parameterName = lambdaExpr.Parameters[i].Name;
            this.tableAlias.Add(parameterName, this.tables[i]);
        }
    }
    private void AddMemberElement(SqlSegment sqlSegment, MemberInfo memberInfo, StringBuilder builder)
    {
        var parameterName = this.ormProvider.ParameterPrefix + memberInfo.Name;
        sqlSegment.ParameterName = parameterName;
        sqlSegment = this.VisitAndDeferred(sqlSegment);
        var entityMapper = this.tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberInfo.Name);
        if (builder.Length > 0)
            builder.Append(',');
        if (this.ormProvider.DatabaseType == DatabaseType.MySql)
            builder.Append("a.");
        builder.Append(this.ormProvider.GetFieldName(memberMapper.FieldName) + "=");
        if (sqlSegment == SqlSegment.Null)
            builder.Append("NULL");
        else
        {
            if (sqlSegment.HasField)
                builder.Append(sqlSegment.ToString());
            else
            {
                builder.Append(parameterName);
                if (!sqlSegment.IsParameter)
                    this.dbParameters.Add(this.ormProvider.CreateParameter(parameterName, sqlSegment.Value));
            }
        }
    }
}
