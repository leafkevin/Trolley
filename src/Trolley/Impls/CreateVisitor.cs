﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

class CreateVisitor : SqlVisitor
{
    private string selectSql = null;
    private string whereSql = null;

    public CreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix)
    {
        this.tables = new();
        this.tableAlias = new();
        this.tables.Add(new TableSegment
        {
            EntityType = entityType,
            Mapper = this.mapProvider.GetEntityMap(entityType)
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
                tableSegment.Mapper ??= this.mapProvider.GetEntityMap(tableSegment.EntityType);
                tableName = this.ormProvider.GetTableName(tableSegment.Mapper.TableName);
            }
            if (i > 1) builder.Append(',');
            builder.Append(tableName + " " + tableSegment.AliasName);
        }
        if (!string.IsNullOrEmpty(this.whereSql))
            builder.Append(this.whereSql);
        dbParameters = this.dbParameters;
        return builder.ToString();
    }
    public CreateVisitor From(Expression fieldSelector)
    {
        var lambdaExpr = fieldSelector as LambdaExpression;
        for (int i = 0; i < lambdaExpr.Parameters.Count; i++)
        {
            var parameterExpr = lambdaExpr.Parameters[i];
            var tableSegment = new TableSegment
            {
                EntityType = parameterExpr.Type,
                Mapper = this.mapProvider.GetEntityMap(parameterExpr.Type),
                AliasName = $"{(char)(this.tableAsStart + i)}"
            };
            this.tables.Add(tableSegment);
        }
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
        this.isWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.whereSql = " WHERE " + this.VisitConditionExpr(lambdaExpr.Body);
        this.isWhere = false;
        return this;
    }
    public CreateVisitor And(Expression whereExpr)
    {
        this.isWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.whereSql += " AND " + this.VisitConditionExpr(lambdaExpr.Body);
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
            if (this.ormProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            {
                //Where(f=>... && f.OrderNo.Length==10 && ...)
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
                if (memberMapper.MemberType.IsEnumType(out var expectType, out _))
                {
                    Type targetType = null;
                    if (this.ormProvider.MapDefaultType(memberMapper.NativeDbType) == typeof(string))
                        targetType = typeof(string);
                    else targetType = expectType;
                    sqlSegment.ExpectType = expectType;
                    sqlSegment.TargetType = targetType;
                }

                var fieldName = this.ormProvider.GetFieldName(memberMapper.FieldName);
                //都需要带有别名
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
        if (this.ormProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            return formatter.Invoke(this, sqlSegment);

        //访问局部变量或是成员变量，当作常量处理,直接计算，如果是字符串变成参数@p
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId)); orderIds
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        return this.Evaluate(sqlSegment);
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

                this.AddMemberElement(i, new SqlSegment { Expression = newExpr.Arguments[i] }, memberInfo, insertBuilder, fromBuilder);
            }
            insertBuilder.Append(fromBuilder);
            return sqlSegment.ChangeValue(insertBuilder.ToString());
        }
        return this.Evaluate(sqlSegment);
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
            this.AddMemberElement(i, new SqlSegment { Expression = memberAssignment.Expression }, memberAssignment.Member, insertBuilder, fromBuilder);
        }
        insertBuilder.Append(fromBuilder);
        return sqlSegment.ChangeValue(insertBuilder.ToString());
    }
    private void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.tableAlias.Clear();
        lambdaExpr.Body.GetParameters(out var parameters);
        if (parameters == null || parameters.Count == 0)
            return;
        for (int i = 0; i < lambdaExpr.Parameters.Count; i++)
        {
            var parameterExpr = lambdaExpr.Parameters[i];
            this.tableAlias.Add(parameterExpr.Name, this.tables[i + 1]);
        }
    }
    private void AddMemberElement(int index, SqlSegment sqlSegment, MemberInfo memberInfo, StringBuilder insertBuilder, StringBuilder fromBuilder)
    {
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
            if (sqlSegment.IsConstantValue)
            {
                if (!sqlSegment.IsParameter)
                {
                    this.dbParameters ??= new();
                    IDbDataParameter dbParameter = null;
                    var parameterName = this.ormProvider.ParameterPrefix + this.parameterPrefix + this.dbParameters.Count.ToString();
                    if (memberMapper.NativeDbType != null)
                        dbParameter = this.ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, sqlSegment.Value);
                    else dbParameter = this.ormProvider.CreateParameter(parameterName, sqlSegment.Value);

                    if (memberMapper.TypeHandler != null)
                    {
                        if (sqlSegment.IsArray)
                        {
                            var sqlSegments = sqlSegment.Value as List<SqlSegment>;
                            sqlSegment.Value = sqlSegments.Select(f => f.Value).ToArray();
                        }
                        memberMapper.TypeHandler.SetValue(this.ormProvider, dbParameter, sqlSegment.Value);
                    }
                    else dbParameter.Value = this.ormProvider.ToFieldValue(sqlSegment.Value, memberMapper.NativeDbType);

                    this.dbParameters.Add(dbParameter);
                    sqlSegment.Value = parameterName;
                    sqlSegment.IsParameter = true;
                    sqlSegment.IsConstantValue = false;
                }
                fromBuilder.Append(sqlSegment.Value.ToString());
            }
            else fromBuilder.Append(sqlSegment.ToString());
        }
    }
}
