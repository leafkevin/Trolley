using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Trolley;

public delegate string MemberAccessSqlFormatter(object target);
public delegate string MethodCallSqlFormatter(object target, Stack<DeferredExpr> deferredExprs, params object[] arguments);
public enum DatabaseType
{
    MySql = 1,
    MsSql = 2,
    Oracle = 3,
    Postgresql = 4
}
public interface IOrmProvider
{
    DatabaseType DatabaseType { get; }
    string ParameterPrefix { get; }
    string SelectIdentitySql { get; }
    bool IsSupportArrayParameter { get; }
    IDbConnection CreateConnection(string connectionString);
    IDbDataParameter CreateParameter(string parameterName, object value);
    string GetTableName(string entityName);
    string GetFieldName(string propertyName);
    string GetPagingTemplate(int skip, int? limit, string orderBy = null);
    int GetNativeDbType(Type type);
    string CastTo(Type type);
    string GetQuotedValue(Type fieldType, object value);
    bool TryGetMemberAccessSqlFormatter(MemberInfo memberInfo, out MemberAccessSqlFormatter formatter);
    bool TryGetMethodCallSqlFormatter(MethodInfo methodInfo, out MethodCallSqlFormatter formatter);
}