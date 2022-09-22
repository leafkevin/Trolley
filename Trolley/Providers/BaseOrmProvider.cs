using System;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley.Providers;

public abstract class BaseOrmProvider : IOrmProvider
{
    public virtual string ParameterPrefix => "@";
    public virtual string SelectIdentitySql => ";SELECT @@IDENTITY";
    public virtual bool IsSupportArrayParameter => false;


    public abstract IDbConnection CreateConnection(string connectionString);
    public abstract IDbDataParameter CreateParameter(string parameterName, object value);
    public virtual string GetTableName(string entityName) => entityName;
    public virtual string GetFieldName(string propertyName) => propertyName;
    public virtual string GetPagingTemplate(int skip, int? limit, string orderBy = null)
    {
        var builder = new StringBuilder("SELECT /**fields**/ FROM /**tables**/ WHERE /**conditions**/");
        if (!String.IsNullOrEmpty(orderBy)) builder.Append($" {orderBy}");
        if (limit.HasValue) builder.Append($" LIMIT {limit}");
        builder.Append($" OFFSET {skip}");
        return builder.ToString();
    }
    public abstract int GetNativeDbType(Type type);
    public virtual string GetQuotedValue(Type fieldType, object value)
    {
        if (fieldType == typeof(bool))
            return (bool)value ? "1" : "0";
        if (fieldType == typeof(string))
            return $"'{value}'";
        if (fieldType == typeof(DateTime))
            return $"'{value:yyyy-MM-dd HH:mm:ss}'";
        if (value is SqlSegment sqlSegment)
            return this.GetQuotedValue(sqlSegment.Value);
        return value.ToString();
    }
    public abstract bool TryGetMethodCallSqlFormatter(MethodInfo methodInfo, out MethodCallSqlFormatter formatter);
    protected virtual Func<string, IDbConnection> CreateConnectionDelegate(Type connectionType)
    {
        var constructor = connectionType.GetConstructor(new Type[] { typeof(string) });
        var connStringExpr = Expression.Parameter(typeof(string), "connectionString");
        var instanceExpr = Expression.New(constructor, connStringExpr);
        return Expression.Lambda<Func<string, IDbConnection>>(
             Expression.Convert(instanceExpr, typeof(IDbConnection))
             , connStringExpr).Compile();
    }
    protected virtual Func<string, int, object, IDbDataParameter> CreateParameterDelegate(Type dbTypeType, Type dbParameterType, PropertyInfo dbTypePropertyInfo)
    {
        var constructor = dbParameterType.GetConstructor(new Type[] { typeof(string), typeof(object) });
        var parametersExpr = new ParameterExpression[] {
            Expression.Parameter(typeof(string), "name"),
            Expression.Parameter(typeof(int), "dbType"),
            Expression.Parameter(typeof(object), "value") };

        var returnLabel = Expression.Label(typeof(IDbDataParameter));
        var instanceExpr = Expression.New(constructor, parametersExpr[0], parametersExpr[2]);
        var dbTypeExpr = Expression.Convert(parametersExpr[1], dbTypeType);
        return Expression.Lambda<Func<string, int, object, IDbDataParameter>>(
            Expression.Block(
                Expression.Call(instanceExpr, dbTypePropertyInfo.GetSetMethod(), dbTypeExpr),
                Expression.Return(returnLabel, Expression.Convert(instanceExpr, typeof(IDbDataParameter))),
                Expression.Label(returnLabel, Expression.Constant(null, typeof(IDbDataParameter))))
            , parametersExpr).Compile();
    }
}
