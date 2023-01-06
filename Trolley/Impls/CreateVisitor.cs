using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Trolley;

class CreateVisitor : SqlVisitor
{
    private string selectSql = null;
    private string whereSql = null;

    public CreateVisitor(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, Type entityType, char tableStartAs = 'a')
        : base(dbFactory, connection, transaction, tableStartAs)
    {
        this.tables = new();
        this.tableAlias = new();
        this.tables.Add(new TableSegment
        {
            EntityType = entityType,
            Mapper = this.dbFactory.GetEntityMap(entityType)
        });
    }
    public string BuildSql(out List<IDbDataParameter> dbParameters)
    {
        var entityTableName = this.ormProvider.GetTableName(this.tables[0].Mapper.TableName);
        var builder = new StringBuilder($"INSERT INTO {entityTableName} {this.selectSql} FROM ");
        for (var i = 1; i < this.tables.Count; i++)
        {
            var tableSegment = this.tables[i];
            var tableName = tableSegment.Body;
            if (string.IsNullOrEmpty(tableName))
            {
                tableSegment.Mapper ??= this.dbFactory.GetEntityMap(tableSegment.EntityType);
                tableName = this.ormProvider.GetTableName(tableSegment.Mapper.TableName);
            }
            if (i > 1) builder.Append(',');
            builder.Append($"{tableName} {tableSegment.AliasName}");
        }
        if (!string.IsNullOrEmpty(this.whereSql))
            builder.Append(this.whereSql);
        dbParameters = this.dbParameters;
        return builder.ToString();
    }
    public CreateVisitor From(Expression fieldSelector)
    {
        var lambdaExpr = fieldSelector as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        var sqlSegment = new SqlSegment { Expression = lambdaExpr.Body };
        sqlSegment = lambdaExpr.Body.NodeType switch
        {
            ExpressionType.New => this.VisitNew(sqlSegment),
            ExpressionType.MemberInit => this.VisitMemberInit(sqlSegment),
            _ => throw new NotImplementedException("不支持的表达式，只支持New或MemberInit表达式，如: new { a.Id, b.Name + &quot;xxx&quot; } 或是new User { Id = a.Id, Name = b.Name + &quot;xxx&quot; }")
        };
        this.selectSql = sqlSegment.ToString();
        return this;
    }
    public CreateVisitor Where(Expression whereExpr)
    {
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.whereSql = this.VisitConditionExpr(lambdaExpr.Body);
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
                //Where(f=>... && f.OrderNo.Length==10 && ...)
                //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
                var targetSegment = this.Visit(sqlSegment.Next(memberExpr.Expression));
                return sqlSegment.Change(formatter.Invoke(targetSegment));
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

                if (sqlSegment.HasDeferred)
                {
                    sqlSegment.HasField = true;
                    sqlSegment.IsConstantValue = false;
                    sqlSegment.TableSegment = tableSegment;
                    sqlSegment.FromMember = memberMapper.Member;
                    sqlSegment.Value = fieldName;
                    return this.VisitBooleanDeferred(sqlSegment);
                }
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
            var insertBuilder = new StringBuilder("(");
            var fromBuilder = new StringBuilder(") SELECT ");
            var entityMapper = this.tables[0].Mapper;
            for (int i = 0; i < newExpr.Arguments.Count; i++)
            {
                var memberInfo = newExpr.Members[i];
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out _))
                    continue;
                this.AddMemberElement(i, sqlSegment.Next(newExpr.Arguments[i]), memberInfo, insertBuilder, fromBuilder);
            }
            insertBuilder.Append(fromBuilder);
            return sqlSegment.Change(insertBuilder.ToString());
        }
        return this.EvaluateAndParameter(sqlSegment);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        var insertBuilder = new StringBuilder("(");
        var fromBuilder = new StringBuilder(") SELECT ");
        var entityMapper = this.tables[0].Mapper;
        for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
        {
            if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                throw new NotImplementedException($"不支持除MemberBindingType.Assignment类型外的成员绑定表达式, {memberInitExpr.Bindings[i]}");
            var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
            if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out _))
                continue;
            this.AddMemberElement(i, sqlSegment.Next(memberAssignment.Expression), memberAssignment.Member, insertBuilder, fromBuilder);
        }
        insertBuilder.Append(fromBuilder);
        return sqlSegment.Change(insertBuilder.ToString());
    }
    private void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.tableAlias.Clear();
        for (int i = 0; i < lambdaExpr.Parameters.Count - 1; i++)
        {
            var parameterExpr = lambdaExpr.Parameters[i];
            var tableSegment = new TableSegment
            {
                EntityType = parameterExpr.Type,
                AliasName = $"{(char)(this.tableStartAs + i)}"
            };
            this.tables.Add(tableSegment);
            this.tableAlias.Add(parameterExpr.Name, tableSegment);
        }
    }
    private void AddMemberElement(int index, SqlSegment sqlSegment, MemberInfo memberInfo, StringBuilder insertBuilder, StringBuilder fromBuilder)
    {
        var parameterName = this.ormProvider.ParameterPrefix + memberInfo.Name;
        sqlSegment.ParameterName = parameterName;
        sqlSegment = this.VisitAndDeferred(sqlSegment);
        var entityMapper = this.tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberInfo.Name);
        if (index > 0)
        {
            insertBuilder.Append(',');
            fromBuilder.Append(',');
        }
        insertBuilder.Append(this.ormProvider.GetFieldName(memberMapper.FieldName));
        if (sqlSegment == SqlSegment.Null)
            fromBuilder.Append("NULL");
        else
        {
            if (sqlSegment.HasField)
                fromBuilder.Append(sqlSegment.ToString());
            else
            {
                fromBuilder.Append(parameterName);
                if (!sqlSegment.IsParameter)
                    this.dbParameters.Add(this.ormProvider.CreateParameter(parameterName, sqlSegment.Value));
            }
        }
    }
}
