﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public delegate SqlSegment MemberAccessSqlFormatter(ISqlVisitor visitor, SqlSegment target);
public delegate SqlSegment MethodCallSqlFormatter(ISqlVisitor visitor, SqlSegment target, Stack<DeferredExpr> DeferredExprs, params SqlSegment[] arguments);
public enum DatabaseType
{
    MySql = 1,
    SqlServer = 2,
    Oracle = 3,
    Postgresql = 4
}
public interface IOrmProvider
{
    DatabaseType DatabaseType { get; }
    string ParameterPrefix { get; }
    string SelectIdentitySql { get; }
    Type NativeDbTypeType { get; }
    IDbConnection CreateConnection(string connectionString);
    IDbDataParameter CreateParameter(string parameterName, object value);
    IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value);
    IQueryVisitor NewQueryVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p");
    ICreateVisitor NewCreateVisitor(string dbKey, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p");
    IUpdateVisitor NewUpdateVisitor(string dbKey, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p");
    IDeleteVisitor NewDeleteVisitor(string dbKey, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p");
    string GetTableName(string entityName);
    string GetFieldName(string propertyName);
    string GetPagingTemplate(int? skip, int? limit, string orderBy = null);
    object GetNativeDbType(Type type);
    Type MapDefaultType(object nativeDbType);
    string CastTo(Type type, object value);
    string GetQuotedValue(Type fieldType, object value);
    object ToFieldValue(object fieldValue, object nativeDbType);
    string GetBinaryOperator(ExpressionType nodeType);
    bool TryGetMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter);
    bool TryGetMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
}