﻿using System;
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
    public abstract Type NativeDbTypeType { get; }
    public abstract IDbConnection CreateConnection(string connectionString);
    public abstract IDbDataParameter CreateParameter(string parameterName, object value);
    public abstract IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value);
    public virtual IQueryVisitor NewQueryVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new QueryVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
    public virtual ICreateVisitor NewCreateVisitor(string dbKey, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new CreateVisitor(dbKey, this, mapProvider, entityType, isParameterized, tableAsStart, parameterPrefix);
    public virtual IUpdateVisitor NewUpdateVisitor(string dbKey, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new UpdateVisitor(dbKey, this, mapProvider, entityType, isParameterized, tableAsStart, parameterPrefix);
    public virtual IDeleteVisitor NewDeleteVisitor(string dbKey, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new DeleteVisitor(dbKey, this, mapProvider, entityType, isParameterized, tableAsStart, parameterPrefix);
    public virtual string GetTableName(string entityName) => entityName;
    public virtual string GetFieldName(string propertyName) => propertyName;
    public virtual string GetPagingTemplate(int? skip, int? limit, string orderBy = null)
    {
        var builder = new StringBuilder("SELECT /**fields**/ FROM /**tables**/ /**others**/");
        if (!String.IsNullOrEmpty(orderBy)) builder.Append($" {orderBy}");
        if (limit.HasValue) builder.Append($" LIMIT {limit}");
        if (skip.HasValue) builder.Append($" OFFSET {skip}");
        return builder.ToString();
    }
    //public virtual string Take(int limit, string orderBy = null)
    //{
    //    string result = "";
    //    if (!String.IsNullOrEmpty(orderBy)) result = orderBy;
    //    result += $" LIMIT {limit}";
    //    return result;
    //}
    public abstract object GetNativeDbType(Type type);
    public abstract Type MapDefaultType(object nativeDbType);
    public abstract string CastTo(Type type, object value);
    public virtual string GetQuotedValue(Type expectType, object value)
    {
        if (value == null) return "NULL";
        if (expectType == typeof(bool))
            return Convert.ToBoolean(value) ? "1" : "0";
        if (expectType == typeof(string))
            return "'" + value.ToString().Replace("\\", "\\\\").Replace("'", @"\'") + "'";
        if (expectType == typeof(DateTime))
            return $"'{Convert.ToDateTime(value):yyyy-MM-dd HH:mm:ss}'";
        if (value is SqlSegment sqlSegment)
        {
            if (sqlSegment == SqlSegment.Null || !sqlSegment.IsConstantValue)
                return sqlSegment.ToString();
            return this.GetQuotedValue(sqlSegment.Value);
        }
        return value.ToString();
    }
    public virtual object ToFieldValue(object fieldValue, object nativeDbType)
    {
        if (fieldValue == null)
            return DBNull.Value;

        var result = fieldValue;
        var fieldType = fieldValue.GetType();
        if (fieldType.IsNullableType(out var underlyingType))
            result = Convert.ChangeType(result, underlyingType);

        if (nativeDbType != null)
        {
            var defaultType = this.MapDefaultType(nativeDbType);
            if (defaultType == underlyingType)
                return result;

            //Gender? gender = Gender.Male;
            //(int)gender.Value;
            if (underlyingType.IsEnum)
            {
                if (defaultType == typeof(string))
                    result = result.ToString();
                else result = Convert.ChangeType(result, defaultType);
            }
            else if (underlyingType == typeof(Guid))
            {
                if (defaultType == typeof(string))
                    result = result.ToString();
                if (defaultType == typeof(byte[]))
                    result = ((Guid)result).ToByteArray();
            }
            else if (underlyingType == typeof(TimeSpan) || underlyingType == typeof(TimeOnly))
            {
                if (defaultType == typeof(long))
                {
                    if (result is TimeSpan timeSpan)
                        result = timeSpan.Ticks;
                    if (result is TimeOnly timeOnly)
                        result = timeOnly.Ticks;
                }
            }
            else result = Convert.ChangeType(result, defaultType);
        }
        return result;
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
    public abstract bool TryGetMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter);
    public abstract bool TryGetMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
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
