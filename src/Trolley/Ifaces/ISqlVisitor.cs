using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace Trolley;

public interface ISqlVisitor : IDisposable
{
    DbContext DbContext { get; }
    bool IsSelect { get; }
    bool IsWhere { get; }

    void UseTable(TableShardingUsageMode usageMode, bool isIncludeMany, params string[] tableNames);
    void UseTableByRange(TableShardingUsageMode usageMode, bool isIncludeMany, object[] fieldValues);
    void UseTableMap(TableShardingUsageMode usageMode, bool isIncludeMany, Func<string, string, string, string> tableNameGetter);
    void UseTableBy(TableShardingUsageMode usageMode, bool isIncludeMany, params object[] fieldValues);
    void UseTableByOthers(TableShardingUsageMode usageMode, bool isIncludeMany, params object[] otherFieldValues);
    void UseTableSchema(bool isIncludeMany, string tableSchema);

    SqlFieldSegment VisitAndDeferred(SqlFieldSegment sqlSegment);
    SqlFieldSegment Visit(SqlFieldSegment sqlSegment);
    SqlFieldSegment VisitUnary(SqlFieldSegment sqlSegment);
    SqlFieldSegment VisitBinary(SqlFieldSegment sqlSegment);
    SqlFieldSegment VisitMemberAccess(SqlFieldSegment sqlSegment);
    SqlFieldSegment VisitConstant(SqlFieldSegment sqlSegment);
    SqlFieldSegment VisitMethodCall(SqlFieldSegment sqlSegment);
    SqlFieldSegment VisitParameter(SqlFieldSegment sqlSegment);
    SqlFieldSegment VisitNew(SqlFieldSegment sqlSegment);
    SqlFieldSegment VisitMemberInit(SqlFieldSegment sqlSegment);
    SqlFieldSegment VisitNewArray(SqlFieldSegment sqlSegment);
    SqlFieldSegment VisitIndexExpression(SqlFieldSegment sqlSegment);
    SqlFieldSegment VisitConditional(SqlFieldSegment sqlSegment);
    SqlFieldSegment VisitListInit(SqlFieldSegment sqlSegment);
    SqlFieldSegment VisitTypeIs(SqlFieldSegment sqlSegment);
    SqlFieldSegment Evaluate(SqlFieldSegment sqlSegment);
    object Evaluate(Expression expr);
    T Evaluate<T>(Expression expr);
    string GetQuotedValue(SqlFieldSegment sqlSegment, bool isNeedExprWrap = false);
    TValue GetQuotedValue<TValue>(SqlFieldSegment sqlSegment);
    string GetQuotedValue(object elementValue, SqlFieldSegment arraySegment, SqlFieldSegment elementSegmente);
    string ChangeParameterValue(SqlFieldSegment sqlSegment, Type targetType);
    SqlFieldSegment VisitSqlMethodCall(SqlFieldSegment sqlSegment);
    bool IsStringConcatOperator(SqlFieldSegment sqlSegment, out SqlFieldSegment result);
    string VisitConditionExpr(Expression conditionExpr, out OperationType operationType);
    List<Expression> ConvertFormatToConcatList(Expression[] argsExprs);
    List<Expression> SplitConcatList(Expression[] argsExprs);
    DataTable ToDataTable(Type entityType, IEnumerable entities, List<(MemberMap, Func<object, object>)> memberMappers, string tableName = null);
    List<(MemberMap, Func<object, object>)> GetRefMemberMappers(Type entityType, EntityMap refEntityMapper, bool isUpdate = false);
    SqlFieldSegment BuildDeferredSqlSegment(MethodCallExpression methodCallExpr, SqlFieldSegment sqlSegment);
    SqlFieldSegment ToEnumString(SqlFieldSegment sqlSegment);
}