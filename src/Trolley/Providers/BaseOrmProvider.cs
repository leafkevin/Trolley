using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

public abstract class BaseOrmProvider : IOrmProvider
{
    public abstract DatabaseType DatabaseType { get; }
    public virtual string ParameterPrefix => "@";
    public virtual string SelectIdentitySql => ";SELECT @@IDENTITY";

    public abstract IDbConnection CreateConnection(string connectionString);
    public abstract IDbDataParameter CreateParameter(string parameterName, object value);
    public abstract IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value);
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
    public abstract object GetNativeDbType(Type type);
    public abstract object GetNativeDbType(int nativeDbType);
    public abstract bool IsStringDbType(int nativeDbType);
    public abstract string CastTo(Type type);
    public virtual string GetQuotedValue(Type expectType, object value)
    {
        if (expectType == typeof(bool))
            return (bool)value ? "1" : "0";
        if (expectType == typeof(string))
            return "'" + value.ToString().Replace("\\", "\\\\").Replace("'", @"\'") + "'";
        if (expectType == typeof(DateTime))
            return $"'{value:yyyy-MM-dd HH:mm:ss}'";
        if (value is SqlSegment sqlSegment)
        {
            if (sqlSegment == SqlSegment.Null || !sqlSegment.IsConstantValue)
                return sqlSegment.Value.ToString();
            return this.GetQuotedValue(sqlSegment.Value);
        }
        return value.ToString();
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
    public abstract bool TryGetMemberAccessSqlFormatter(SqlSegment originalSegment, MemberInfo memberInfo, out MemberAccessSqlFormatter formatter);
    public abstract bool TryGetMethodCallSqlFormatter(SqlSegment originalSegment, MethodInfo methodInfo, out MethodCallSqlFormatter formatter);
    public override int GetHashCode() => HashCode.Combine(this.DatabaseType);

    public static Func<string, IDbConnection> CreateConnectionDelegate(Type connectionType)
    {
        var constructor = connectionType.GetConstructor(new Type[] { typeof(string) });
        var connStringExpr = Expression.Parameter(typeof(string), "connectionString");
        var instanceExpr = Expression.New(constructor, connStringExpr);
        return Expression.Lambda<Func<string, IDbConnection>>(
             Expression.Convert(instanceExpr, typeof(IDbConnection))
             , connStringExpr).Compile();
    }
    public static Func<string, object, IDbDataParameter> CreateDefaultParameterDelegate(Type dbParameterType)
    {
        var constructor = dbParameterType.GetConstructor(new Type[] { typeof(string), typeof(object) });
        var parametersExpr = new ParameterExpression[] {
            Expression.Parameter(typeof(string), "name"),
            Expression.Parameter(typeof(object), "value") };
        var instanceExpr = Expression.New(constructor, parametersExpr[0], parametersExpr[1]);
        var convertExpr = Expression.Convert(instanceExpr, typeof(IDbDataParameter));
        return Expression.Lambda<Func<string, object, IDbDataParameter>>(convertExpr, parametersExpr).Compile();
    }
    public static Func<string, object, object, IDbDataParameter> CreateParameterDelegate(Type dbTypeType, Type dbParameterType, PropertyInfo valuePropertyInfo)
    {
        var constructor = dbParameterType.GetConstructor(new Type[] { typeof(string), dbTypeType });
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();
        var nameExpr = Expression.Parameter(typeof(string), "name");
        var dbTypeExpr = Expression.Parameter(typeof(object), "dbType");
        var valueExpr = Expression.Parameter(typeof(object), "value");
        var resultExpr = Expression.Variable(dbParameterType, "result");
        blockParameters.Add(resultExpr);

        var nativeDbTypeExpr = Expression.Convert(dbTypeExpr, dbTypeType);
        blockBodies.Add(Expression.Assign(resultExpr, Expression.New(constructor, nameExpr, nativeDbTypeExpr)));
        blockBodies.Add(Expression.Call(resultExpr, valuePropertyInfo.GetSetMethod(), valueExpr));
        var resultLabelExpr = Expression.Label(typeof(IDbDataParameter));
        blockBodies.Add(Expression.Return(resultLabelExpr, Expression.Convert(resultExpr, typeof(IDbDataParameter))));
        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(IDbDataParameter))));
        return Expression.Lambda<Func<string, object, object, IDbDataParameter>>(Expression.Block(blockParameters, blockBodies), nameExpr, dbTypeExpr, valueExpr).Compile();
    }
}
