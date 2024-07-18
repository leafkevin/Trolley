using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface ISqlVisitor : IDisposable
{
    string DbKey { get; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider MapProvider { get; }
    IShardingProvider ShardingProvider { get; }
    bool IsParameterized { get; set; }
    bool IsSelect { get; }
    bool IsWhere { get; }

    void UseTable(Type entityType, params string[] tableNames);
    void UseTable(Type entityType, Func<string, bool> tableNamePredicate);
    void UseTable(Type entityType, Type masterEntityType, Func<string, string, string, string, string> tableNameGetter);
    void UseTableBy(Type entityType, object field1Value, object field2Value = null);
    void UseTableByRange(Type entityType, object beginFieldValue, object endFieldValue);
    void UseTableByRange(Type entityType, object fieldValue1, object fieldValue2, object fieldValue3);

    SqlSegment VisitAndDeferred(SqlSegment sqlSegment);
    SqlSegment Visit(SqlSegment sqlSegment);
    SqlSegment VisitUnary(SqlSegment sqlSegment);
    SqlSegment VisitBinary(SqlSegment sqlSegment);
    SqlSegment VisitMemberAccess(SqlSegment sqlSegment);
    SqlSegment VisitConstant(SqlSegment sqlSegment);
    SqlSegment VisitMethodCall(SqlSegment sqlSegment);
    SqlSegment VisitParameter(SqlSegment sqlSegment);
    SqlSegment VisitNew(SqlSegment sqlSegment);
    SqlSegment VisitMemberInit(SqlSegment sqlSegment);
    SqlSegment VisitNewArray(SqlSegment sqlSegment);
    SqlSegment VisitIndexExpression(SqlSegment sqlSegment);
    SqlSegment VisitConditional(SqlSegment sqlSegment);
    SqlSegment VisitListInit(SqlSegment sqlSegment);
    SqlSegment VisitTypeIs(SqlSegment sqlSegment);
    SqlSegment Evaluate(SqlSegment sqlSegment);
    object Evaluate(Expression expr);
    T Evaluate<T>(Expression expr);
    string GetQuotedValue(SqlSegment sqlSegment, Type expectType = null, object nativeDbType = null, ITypeHandler typeHandler = null);
    string GetQuotedValue(object elementValue, SqlSegment arraySegment, SqlSegment elementSegmente);
    SqlSegment VisitSqlMethodCall(SqlSegment sqlSegment);
    bool IsStringConcatOperator(SqlSegment sqlSegment, out SqlSegment result);
    string VisitConditionExpr(Expression conditionExpr, out OperationType operationType);
    List<Expression> ConvertFormatToConcatList(Expression[] argsExprs);
    List<Expression> SplitConcatList(Expression[] argsExprs);
    string VisitFromQuery(LambdaExpression lambdaExpr);
    DataTable ToDataTable(Type entityType, IEnumerable entities, List<(MemberMap RefMemberMapper, Func<object, object> ValueGetter)> memberMappers, string tableName = null);
    List<(MemberMap RefMemberMapper, Func<object, object> ValueGetter)> GetRefMemberMappers(Type entityType, EntityMap refEntityMapper, bool isUpdate = false);
    SqlSegment BuildDeferredSqlSegment(MethodCallExpression methodCallExpr, SqlSegment sqlSegment);
    SqlSegment ToEnumString(SqlSegment sqlSegment);
}
