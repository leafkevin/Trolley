﻿using System;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

public delegate IDbConnection CreateNativeDbConnectionDelegate(string connectionString);
public delegate IDbDataParameter CreateDefaultNativeParameterDelegate(string name, object value);
public delegate IDbDataParameter CreateNativeParameterDelegate(string name, int nativeDbType, object value);
public abstract class BaseOrmProvider : IOrmProvider
{
    public abstract DatabaseType DatabaseType { get; }
    public virtual string ParameterPrefix => "@";
    public virtual string SelectIdentitySql => ";SELECT @@IDENTITY";

    public abstract IDbConnection CreateConnection(string connectionString);
    public abstract IDbDataParameter CreateParameter(string parameterName, object value);
    public abstract IDbDataParameter CreateParameter(string parameterName, int nativeDbType, object value);
    public virtual string GetTableName(string entityName) => entityName;
    public virtual string GetFieldName(string propertyName) => propertyName;
    public virtual string GetPagingTemplate(int skip, int? limit, string orderBy = null)
    {
        var builder = new StringBuilder("SELECT /**fields**/ FROM /**tables**/ /**others**/");
        if (!String.IsNullOrEmpty(orderBy)) builder.Append($" {orderBy}");
        if (limit.HasValue) builder.Append($" LIMIT {limit}");
        builder.Append($" OFFSET {skip}");
        return builder.ToString();
    }
    public abstract int GetNativeDbType(Type type);
    public abstract string CastTo(Type type);
    public virtual string GetQuotedValue(Type fieldType, object value)
    {
        if (fieldType == typeof(bool))
            return (bool)value ? "1" : "0";
        if (fieldType == typeof(string))
            return "'" + value.ToString().Replace("\\", "\\\\").Replace("'", @"\'") + "'";
        if (fieldType == typeof(DateTime))
            return $"'{value:yyyy-MM-dd HH:mm:ss}'";
        if (value is SqlSegment sqlSegment)
        {
            if (sqlSegment == SqlSegment.Null || !sqlSegment.IsConstantValue)
                return sqlSegment.Value.ToString();
            return this.GetQuotedValue(sqlSegment.Value);
        }
        return value.ToString();
    }
    public abstract bool TryGetMemberAccessSqlFormatter(MemberInfo memberInfo, out MemberAccessSqlFormatter formatter);
    public abstract bool TryGetMethodCallSqlFormatter(MethodInfo methodInfo, out MethodCallSqlFormatter formatter);
    public virtual CreateNativeDbConnectionDelegate CreateConnectionDelegate(Type connectionType)
    {
        var constructor = connectionType.GetConstructor(new Type[] { typeof(string) });
        var connStringExpr = Expression.Parameter(typeof(string), "connectionString");
        var instanceExpr = Expression.New(constructor, connStringExpr);
        return Expression.Lambda<CreateNativeDbConnectionDelegate>(
             Expression.Convert(instanceExpr, typeof(IDbConnection))
             , connStringExpr).Compile();
    }
    public virtual CreateDefaultNativeParameterDelegate CreateDefaultParameterDelegate(Type dbParameterType)
    {
        var constructor = dbParameterType.GetConstructor(new Type[] { typeof(string), typeof(object) });
        var parametersExpr = new ParameterExpression[] {
            Expression.Parameter(typeof(string), "name"),
            Expression.Parameter(typeof(object), "value") };
        var instanceExpr = Expression.New(constructor, parametersExpr[0], parametersExpr[1]);
        var convertExpr = Expression.Convert(instanceExpr, typeof(IDbDataParameter));
        return Expression.Lambda<CreateDefaultNativeParameterDelegate>(convertExpr, parametersExpr).Compile();
    }
    public virtual CreateNativeParameterDelegate CreateParameterDelegate(Type dbTypeType, Type dbParameterType, PropertyInfo dbTypePropertyInfo)
    {
        var constructor = dbParameterType.GetConstructor(new Type[] { typeof(string), typeof(object) });
        var parametersExpr = new ParameterExpression[] {
            Expression.Parameter(typeof(string), "name"),
            Expression.Parameter(typeof(int), "dbType"),
            Expression.Parameter(typeof(object), "value") };

        var returnLabel = Expression.Label(typeof(IDbDataParameter));
        var instanceExpr = Expression.New(constructor, parametersExpr[0], parametersExpr[2]);
        var dbTypeExpr = Expression.Convert(parametersExpr[1], dbTypeType);
        return Expression.Lambda<CreateNativeParameterDelegate>(
            Expression.Block(
                Expression.Call(instanceExpr, dbTypePropertyInfo.GetSetMethod(), dbTypeExpr),
                Expression.Return(returnLabel, Expression.Convert(instanceExpr, typeof(IDbDataParameter))),
                Expression.Label(returnLabel, Expression.Default(typeof(IDbDataParameter))))
            , parametersExpr).Compile();
    }
    public virtual string GetBinaryOperator(ExpressionType nodeType) =>
       nodeType switch
       {
           ExpressionType.Equal => "=",
           ExpressionType.NotEqual => "<>",
           ExpressionType.GreaterThan => ">",
           ExpressionType.GreaterThanOrEqual => ">=",
           ExpressionType.LessThan => "<",
           ExpressionType.LessThanOrEqual => "<=",
           ExpressionType.AndAlso => "AND",
           ExpressionType.OrElse => "OR",
           ExpressionType.Add => "+",
           ExpressionType.Subtract => "-",
           ExpressionType.Multiply => "*",
           ExpressionType.Divide => "/",
           ExpressionType.Modulo => "%",
           ExpressionType.Coalesce => "COALESCE",
           ExpressionType.And => "&",
           ExpressionType.Or => "|",
           ExpressionType.ExclusiveOr => "^",
           ExpressionType.LeftShift => "<<",
           ExpressionType.RightShift => ">>",
           _ => nodeType.ToString()
       };
    public virtual SqlSegment FormatBinary(ExpressionType nodeType, SqlSegment leftSegment, SqlSegment rightSegment)
    {
        leftSegment.IsExpression = true;
        var operators = this.GetBinaryOperator(nodeType);
        if (nodeType == ExpressionType.Coalesce)
            return leftSegment.Change($"{operators}({leftSegment},{rightSegment})", false);
        //if (operators == "-")
        //{
        //    if (leftSegment.Expression.Type == typeof(DateTime))
        //    {
        //        if (rightSegment.Expression.Type != typeof(DateTime) && rightSegment.Expression.Type != typeof(TimeSpan))
        //            throw new NotSupportedException("DateTime类型的表达式只支持DateTime和TimeSpan类型的减法操作");
        //        if (leftSegment.IsConstantValue && rightSegment.IsConstantValue) ;
        //        var methodInfo = typeof(DateTime).GetMethod(nameof(DateTime.Subtract), new Type[] { rightSegment.Expression.Type });
        //        if (this.TryGetMethodCallSqlFormatter(methodInfo, out var sqlFormatter))
        //            return leftSegment.Change(sqlFormatter.Invoke(leftSegment, null, rightSegment));
        //    }
        //}
        return leftSegment.Change($"{this.GetQuotedValue(leftSegment)}{operators}{this.GetQuotedValue(rightSegment)}", false);
    }
}
