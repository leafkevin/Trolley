using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

public interface ISqlVisitor : IDisposable
{
    string DbKey { get; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider MapProvider { get; }
    bool IsParameterized { get; set; }
    bool IsSelect { get; }
    bool IsWhere { get; }

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
    object EvaluateAndCache(object entity, MemberInfo member);
    //SqlSegment Merge(SqlSegment sqlSegment, SqlSegment rightSegment, object segmentValue);
    //SqlSegment Merge(SqlSegment sqlSegment, SqlSegment args0Segment, SqlSegment args1Segment, object segmentValue);
    //SqlSegment Merge(SqlSegment sqlSegment, SqlSegment rightSegment, object segmentValue, bool isExpression, bool isMethodCall);
    //SqlSegment Merge(SqlSegment sqlSegment, SqlSegment args0Segment, SqlSegment args1Segment, object segmentValue, bool isExpression, bool isMethodCall);
    //SqlSegment Change(SqlSegment sqlSegment);
    //SqlSegment Change(SqlSegment sqlSegment, object segmentValue);
    //SqlSegment Change(SqlSegment sqlSegment, object segmentValue, bool isExpression, bool isMethodCall);



    //TODO:
    string GetParameterizedValue(SqlSegment sqlSegment);
    //SqlSegment Change(SqlSegment sqlSegment, object segmentValue, bool isConstant = true, bool isVariable = false, bool isExpression = false, bool isMethodCall = false);
    //SqlSegment Change(SqlSegment sqlSegment, SqlSegment rightSegment, object segmentValue, bool isConstant = true, bool isVariable = false, bool isExpression = false, bool isMethodCall = false);
    //SqlSegment Change(SqlSegment sqlSegment, SqlSegment leftSegment, SqlSegment rightSegment, object segmentValue, bool isConstant = true, bool isVariable = false, bool isExpression = false, bool isMethodCall = false);




    string GetQuotedValue(SqlSegment sqlSegment);
    string GetQuotedValue(object elementValue, SqlSegment arraySegment);
    SqlSegment VisitSqlMethodCall(SqlSegment sqlSegment);
    bool IsStringConcatOperator(SqlSegment sqlSegment, out SqlSegment result);
    string VisitConditionExpr(Expression conditionExpr);
    List<Expression> ConvertFormatToConcatList(Expression[] argsExprs);
    List<Expression> SplitConcatList(Expression[] argsExprs);
    string VisitFromQuery(LambdaExpression lambdaExpr);
    bool ChangeSameType(SqlSegment leftSegment, SqlSegment rightSegment);
}
