using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface ISqlVisitor : IDisposable
{
    DbContext DbContext { get; }
    bool IsSelect { get; }

    void UseTable(TableSegment tableSegment, params string[] tableNames);
    void UseTableByRange(TableUsageMode usageMode, TableSegment tableSegment, object[] fieldValues);
    void UseTableMap(TableSegment tableSegment, Func<string, string, string, string> tableNameGetter);
    void UseTableBy(TableUsageMode usageMode, TableSegment tableSegment, params object[] fieldValues);
    void UseTable(TableSegment tableSegment, Func<object, string> tableNameGetter);
    void UseTableSchema(TableSegment tableSegment, string tableSchema);
    void WithTableAliasTrailing(TableSegment tableSegment, string rawSql);


    SqlSegment Visit(SqlSegment sqlSegment);
    string WrapSql(SqlSegment sqlSegment, bool isNeedWrapExpr = false);
    string WrapParameterSql(object value);
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
    string ChangeParameterValue(SqlSegment sqlSegment, Type targetType);
    SqlSegment VisitSqlMethodCall(SqlSegment sqlSegment);
    string VisitConditionExpr(Expression conditionExpr, out OperationType operationType);
    List<Expression> ConvertFormatToConcatList(Expression[] argsExprs);
    List<Expression> SplitConcatList(Expression[] argsExprs);
    SqlSegment VisitDeferredSqlSegment(SqlSegment sqlSegment);
    SqlSegment ToEnumString(SqlSegment sqlSegment);

    DataTable ToDataTable(string tableName, IEnumerable entities, List<MemberMap> memberMappers, List<Func<object, object>> valueGetters);
    (List<MemberMap>, List<Func<object, object>>) GetRefMemberMappers(Type parameterType, EntityMap entityMapper, object parameterSample, bool isUpdate = false);
}