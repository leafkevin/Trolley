using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface ISqlVisitor
{
    bool IsParameterized { get; set; }
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
    SqlSegment Merge(SqlSegment sqlSegment, SqlSegment rightSegment, object segmentValue);
    SqlSegment Merge(SqlSegment sqlSegment, SqlSegment args0Segment, SqlSegment args1Segment, object segmentValue);
    SqlSegment Merge(SqlSegment sqlSegment, SqlSegment rightSegment, object segmentValue, bool isExpression, bool isMethodCall);
    SqlSegment Merge(SqlSegment sqlSegment, SqlSegment args0Segment, SqlSegment args1Segment, object segmentValue, bool isExpression, bool isMethodCall);
    SqlSegment Change(SqlSegment sqlSegment);
    SqlSegment Change(SqlSegment sqlSegment, object segmentValue);
    SqlSegment Change(SqlSegment sqlSegment, object segmentValue, bool isExpression, bool isMethodCall);
    string GetQuotedValue(object fieldValue, MemberMap memberMapper = null, bool? isVariable = null, int? index = null, string nullValue = "NULL");
    string GetQuotedValue(SqlSegment sqlSegment, int? index = null, string nullValue = "NULL");
    string GetQuotedValue(object elementValue, SqlSegment arraySegment, int? index = null, string nullValue = "NULL");
    IDbDataParameter CreateParameter(MemberMap memberMapper, string parameterName, object fieldValue);
    SqlSegment VisitSqlMethodCall(SqlSegment sqlSegment);
    bool IsStringConcatOperator(SqlSegment sqlSegment, out SqlSegment result);
    string VisitConditionExpr(Expression conditionExpr);
    List<Expression> ConvertFormatToConcatList(Expression[] argsExprs);
    List<Expression> SplitConcatList(Expression[] argsExprs);
    List<ReaderField> AddTableRecursiveReaderFields(int readerIndex, TableSegment fromSegment);
    string VisitFromQuery(LambdaExpression lambdaExpr, out bool isNeedAlias);
}
