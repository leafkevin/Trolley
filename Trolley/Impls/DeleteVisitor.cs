using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Trolley;

class DeleteVisitor : SqlVisitor
{
    private readonly TableSegment tableSegment;
    private string whereSql = string.Empty;

    public DeleteVisitor(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, Type entityType, char tableStartAs = 'a')
        : base(dbFactory, connection, transaction, tableStartAs)
    {
        this.tableSegment = new TableSegment
        {
            EntityType = entityType,
            Mapper = dbFactory.GetEntityMap(entityType)
        };
    }
    public string BuildSql(out List<IDbDataParameter> dbParameters)
    {
        var entityMapper = this.tableSegment.Mapper;
        var entityTableName = this.ormProvider.GetTableName(entityMapper.TableName);
        var builder = new StringBuilder($"DELETE FROM {entityTableName}");

        if (!string.IsNullOrEmpty(this.whereSql))
            builder.Append(this.whereSql);
        dbParameters = this.dbParameters;
        return builder.ToString();
    }
    public DeleteVisitor Where(Expression whereExpr)
    {
        var lambdaExpr = whereExpr as LambdaExpression;
        this.whereSql = " WHERE " + this.VisitConditionExpr(lambdaExpr.Body);
        return this;
    }
    public DeleteVisitor And(Expression whereExpr)
    {
        var lambdaExpr = whereExpr as LambdaExpression;
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
                //Where(f=>... && f.OrderNo.Length==10 && ...)
                //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
                var targetSegment = this.Visit(sqlSegment.Next(memberExpr.Expression));
                return sqlSegment.Change(formatter.Invoke(targetSegment));
            }

            if (memberExpr.IsParameter(out _))
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
                var memberMapper = this.tableSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                var fieldName = this.ormProvider.GetFieldName(memberMapper.FieldName);

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
            var builder = new StringBuilder();
            var entityMapper = this.tableSegment.Mapper;
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
        var entityMapper = this.tableSegment.Mapper;
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
    private void AddMemberElement(SqlSegment sqlSegment, MemberInfo memberInfo, StringBuilder builder)
    {
        var parameterName = this.ormProvider.ParameterPrefix + memberInfo.Name;
        sqlSegment.ParameterName = parameterName;
        sqlSegment = this.VisitAndDeferred(sqlSegment);
        var entityMapper = this.tableSegment.Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberInfo.Name);
        if (builder.Length > 0)
            builder.Append(',');
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