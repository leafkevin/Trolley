using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public interface ISqlVisitor
{
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
    List<SqlSegment> VisitLogicBinaryExpr(Expression conditionExpr);
    SqlSegment Evaluate(SqlSegment sqlSegment);
    T Evaluate<T>(Expression expr);
    SqlSegment VisitSqlMethodCall(SqlSegment sqlSegment);
    bool IsStringConcatOperator(SqlSegment sqlSegment, out SqlSegment result);
    string VisitConditionExpr(Expression conditionExpr);
    List<SqlSegment> ConvertFormatToConcatList(SqlSegment[] argsSegments);
    List<SqlSegment> SplitConcatList(SqlSegment[] argsSegments);
    SqlSegment[] SplitConcatList(Expression concatExpr);
    List<ReaderField> AddTableRecursiveReaderFields(int readerIndex, TableSegment fromSegment);
    SqlSegment ToParameter(SqlSegment sqlSegment);
    string VisitFromQuery(LambdaExpression lambdaExpr, out bool isNeedAlias);
}
