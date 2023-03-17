using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

class SqlVisitor
{
    protected internal char tableAsStart = 'a';
    protected internal string parameterPrefix;
    protected internal readonly string dbKey;
    protected internal readonly Type ormProviderType;
    protected internal readonly IOrmProvider ormProvider;
    protected internal readonly IEntityMapProvider mapProvider;
    protected internal bool isTarget;

    protected List<TableSegment> tables;
    protected Dictionary<string, TableSegment> tableAlias;
    protected List<IDbDataParameter> dbParameters;
    protected List<ReaderField> readerFields;
    protected bool isNeedAlias = false;
    protected bool isSelect = false;
    protected bool isWhere = false;

    public SqlVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, char tableAsStart = 'a', string parameterPrefix = "p")
    {
        this.dbKey = dbKey;
        this.ormProviderType = ormProvider.GetType();
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.tableAsStart = tableAsStart;
        this.parameterPrefix = parameterPrefix;
    }
    public QueryVisitor ToQueryVisitor(char tableAsStart = 'a', string parameterPrefix = "p")
    {
        var visitor = new QueryVisitor(this.dbKey, this.ormProvider, this.mapProvider, tableAsStart, parameterPrefix);
        visitor.isNeedAlias = this.isNeedAlias;
        return visitor;
    }
    public virtual SqlSegment VisitAndDeferred(SqlSegment sqlSegment)
        => this.VisitBooleanDeferred(this.Visit(sqlSegment));
    public virtual SqlSegment Visit(SqlSegment sqlSegment)
    {
        SqlSegment result = null;
        while (sqlSegment.Expression != null)
        {
            switch (sqlSegment.Expression.NodeType)
            {
                case ExpressionType.Lambda:
                    var lambdaExpr = sqlSegment.Expression as LambdaExpression;
                    if (this.IsFromQuery(lambdaExpr))
                        return sqlSegment.Change(this.VisitFromQuery(lambdaExpr, out _), false);
                    sqlSegment.Expression = lambdaExpr.Body;
                    continue;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    result = this.VisitUnary(sqlSegment);
                    break;
                case ExpressionType.MemberAccess:
                    result = this.VisitMemberAccess(sqlSegment);
                    break;
                case ExpressionType.Constant:
                    result = this.VisitConstant(sqlSegment);
                    break;
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    result = this.VisitBinary(sqlSegment);
                    break;
                case ExpressionType.Parameter:
                    result = this.VisitParameter(sqlSegment);
                    break;
                case ExpressionType.Call:
                    result = this.VisitMethodCall(sqlSegment);
                    break;
                case ExpressionType.New:
                    result = this.VisitNew(sqlSegment);
                    break;
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    result = this.VisitNewArray(sqlSegment);
                    break;
                case ExpressionType.MemberInit:
                    result = this.VisitMemberInit(sqlSegment);
                    break;
                case ExpressionType.Index:
                    result = this.VisitIndexExpression(sqlSegment);
                    break;
                case ExpressionType.Conditional:
                    result = this.VisitConditional(sqlSegment);
                    break;
                case ExpressionType.ListInit:
                    result = this.VisitListInit(sqlSegment);
                    break;
                case ExpressionType.TypeIs:
                    result = this.VisitTypeIs(sqlSegment);
                    break;
            }
            return result;
        }
        return result;
    }
    public virtual SqlSegment VisitUnary(SqlSegment sqlSegment)
    {
        var unaryExpr = sqlSegment.Expression as UnaryExpression;
        switch (unaryExpr.NodeType)
        {
            case ExpressionType.Not:
                if (unaryExpr.Type == typeof(bool))
                {
                    //目前只有Not，一元/二元 bool类型才有延时处理,到参数表达式再统一处理
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                    return this.Visit(sqlSegment.Next(unaryExpr.Operand));
                    //if (unaryExpr.Operand.IsParameter(out _))
                    //{
                    //    //目前只有Not，一元/二元 bool类型才有延时处理,到参数表达式再统一处理
                    //    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                    //    return this.Visit(sqlSegment.Next(unaryExpr.Operand));
                    //}
                    ////TODO:测试Value赋值
                    //return sqlSegment.Change($"NOT ({this.VisitAndDeferred(sqlSegment.Next(unaryExpr.Operand))})");
                }
                return sqlSegment.Change($"~{this.Visit(sqlSegment)}");
            case ExpressionType.Convert:
                if (unaryExpr.Method != null)
                {
                    //TODO:测试类型转换
                    if (unaryExpr.Operand.IsParameter(out _))
                        return this.Visit(sqlSegment.Next(unaryExpr.Operand));
                    return this.EvaluateAndParameter(sqlSegment);
                }
                break;
        }
        return this.Visit(sqlSegment.Next(unaryExpr.Operand));
    }
    public virtual SqlSegment VisitBinary(SqlSegment sqlSegment)
    {
        var binaryExpr = sqlSegment.Expression as BinaryExpression;
        switch (binaryExpr.NodeType)
        {
            //And/Or，已经在Where/Having中单独处理了
            case ExpressionType.Add:
            case ExpressionType.AddChecked:
            case ExpressionType.Subtract:
            case ExpressionType.SubtractChecked:
            case ExpressionType.Multiply:
            case ExpressionType.MultiplyChecked:
            case ExpressionType.Divide:
            case ExpressionType.Modulo:
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
            case ExpressionType.Equal:
            case ExpressionType.NotEqual:
            case ExpressionType.Coalesce:
            case ExpressionType.ArrayIndex:
            case ExpressionType.And:
            case ExpressionType.Or:
            case ExpressionType.ExclusiveOr:
            case ExpressionType.RightShift:
            case ExpressionType.LeftShift:
                //字符串连接单独处理
                if (binaryExpr.NodeType == ExpressionType.Add && (binaryExpr.Left.Type == typeof(string) || binaryExpr.Right.Type == typeof(string)))
                    return this.VisitConcatAndDeferred(sqlSegment);
                if (binaryExpr.NodeType == ExpressionType.Subtract && binaryExpr.Left.Type == typeof(DateTime))
                {
                    var methodInfo = typeof(DateTime).GetMethod(nameof(DateTime.Subtract), new Type[] { binaryExpr.Type });
                    var subtractExpr = Expression.Call(binaryExpr.Left, methodInfo, binaryExpr.Right);
                    return this.VisitMethodCall(sqlSegment.Next(subtractExpr));
                }

                var leftSegment = this.Visit(sqlSegment.Next(binaryExpr.Left));
                var rightSegment = this.Visit(new SqlSegment { Expression = binaryExpr.Right });

                //bool类型的表达式，这里不做解析，到where、having、joinOn子句中去解析并展开合并
                if (binaryExpr.NodeType == ExpressionType.Equal || binaryExpr.NodeType == ExpressionType.NotEqual)
                {
                    if (!leftSegment.HasField && rightSegment.HasField)
                        this.Swap(ref leftSegment, ref rightSegment);

                    //处理!(a.IsEnabled==true)情况,bool类型，最外层再做defer处理
                    if (binaryExpr.Left.Type == typeof(bool) && leftSegment.HasField && !rightSegment.HasField)
                    {
                        leftSegment.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlSegment.True });
                        if (!(bool)rightSegment.Value)
                            leftSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                        if (binaryExpr.NodeType == ExpressionType.NotEqual)
                            leftSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                        return leftSegment;
                    }
                    //处理a.UserName!=null情况
                    if (leftSegment == SqlSegment.Null && rightSegment != SqlSegment.Null)
                        this.Swap(ref leftSegment, ref rightSegment);

                    if (rightSegment == SqlSegment.Null)
                    {
                        leftSegment.Push(new DeferredExpr
                        {
                            OperationType = OperationType.Equal,
                            Value = SqlSegment.Null
                        });
                        if (binaryExpr.NodeType == ExpressionType.NotEqual)
                            leftSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                        return leftSegment;
                    }
                }
                var operators = this.ormProvider.GetBinaryOperator(binaryExpr.NodeType);
                if (binaryExpr.NodeType == ExpressionType.Coalesce)
                    return leftSegment.Change($"{operators}({leftSegment},{rightSegment})", false);

                return leftSegment.Change($"{this.GetSqlValue(leftSegment)}{operators}{this.GetSqlValue(rightSegment)}", false);
        }
        return sqlSegment;
    }
    public virtual SqlSegment VisitMemberAccess(SqlSegment sqlSegment)
    {
        throw new NotImplementedException();
    }
    public virtual SqlSegment VisitConstant(SqlSegment sqlSegment)
    {
        var constantExpr = sqlSegment.Expression as ConstantExpression;
        if (constantExpr.Value == null)
            return SqlSegment.Null;

        return sqlSegment.Change(constantExpr.Value);
    }
    public virtual SqlSegment VisitMethodCall(SqlSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        if (methodCallExpr.Method.DeclaringType == typeof(Sql)
            || typeof(IAggregateSelect).IsAssignableFrom(methodCallExpr.Method.DeclaringType))
            return this.VisitSqlMethodCall(sqlSegment);

        if (!this.ormProvider.TryGetMethodCallSqlFormatter(sqlSegment, methodCallExpr.Method, out var formatter))
            throw new Exception($"不支持的方法访问，或是{this.ormProvider.GetType().FullName}未实现此方法{methodCallExpr.Method.Name}");

        SqlSegment target = null;
        object[] args = null;
        bool isConstantValue = false;
        //如果方法对象是聚合查询，不做任何处理
        if (methodCallExpr.Object != null)
        {
            target = this.Visit(new SqlSegment { Expression = methodCallExpr.Object });
            if (target.IsConstantValue) isConstantValue = true;
        }

        //字符串连接单独特殊处理
        if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count > 0)
        {
            var arguments = new List<object>();
            foreach (var argumentExpr in methodCallExpr.Arguments)
            {
                var argumentSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                arguments.Add(argumentSegment);
                if (argumentSegment.IsConstantValue) isConstantValue = true;
            }
            args = arguments.ToArray();
        }
        var result = formatter.Invoke(target, sqlSegment.DeferredExprs, args);

        //ToString 原来的值通常都是常量，再以常量值返回
        if (methodCallExpr.Method.Name == "ToString" && isConstantValue)
            return sqlSegment.Change(result);
        sqlSegment.IsExpression = true;
        return sqlSegment.Change(result, false);
    }
    public virtual SqlSegment VisitParameter(SqlSegment sqlSegment)
    {
        var parameterExpr = sqlSegment.Expression as ParameterExpression;
        var fromSegment = this.tableAlias[parameterExpr.Name];
        var readerFields = this.AddTableRecursiveReaderFields(sqlSegment.ReaderIndex, fromSegment);
        return sqlSegment.Change(readerFields, false);
    }
    public virtual SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        throw new NotImplementedException();
    }
    public virtual SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        throw new NotImplementedException();
    }
    public virtual SqlSegment VisitNewArray(SqlSegment sqlSegment)
    {
        sqlSegment.IsArray = true;
        var newArrayExpr = sqlSegment.Expression as NewArrayExpression;
        var result = new List<SqlSegment>();
        bool isConstantValue = false;
        foreach (var elementExpr in newArrayExpr.Expressions)
        {
            var elementSegment = new SqlSegment { Expression = elementExpr };
            elementSegment = this.Visit(elementSegment);
            if (elementSegment.IsConstantValue)
                isConstantValue = true;
            result.Add(elementSegment);
        }
        return sqlSegment.Change(result, isConstantValue);
    }
    public virtual SqlSegment VisitIndexExpression(SqlSegment sqlSegment)
    {
        var indexExpr = sqlSegment.Expression as IndexExpression;
        var argExpr = indexExpr.Arguments[0];
        var objIndex = argExpr is ConstantExpression constant
            ? constant.Value : this.Evaluate(sqlSegment.Next(argExpr)).Value;
        var index = (int)Convert.ChangeType(objIndex, typeof(int));
        var objTarget = this.Evaluate(sqlSegment.Next(indexExpr.Object)).Value;
        if (objTarget is List<object> objList)
            return sqlSegment.Change(objList[index]);
        throw new NotSupportedException("不支持的表达式: " + indexExpr);
    }
    public virtual SqlSegment VisitConditional(SqlSegment sqlSegment)
    {
        var conditionalExpr = sqlSegment.Expression as ConditionalExpression;
        sqlSegment = this.Visit(sqlSegment.Next(conditionalExpr.Test));
        var ifTrue = this.Visit(new SqlSegment { Expression = conditionalExpr.IfTrue });
        var ifFalse = this.Visit(new SqlSegment { Expression = conditionalExpr.IfFalse });
        if (sqlSegment.HasField && conditionalExpr.Test.Type == typeof(bool))
        {
            sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlSegment.True });
            sqlSegment = this.VisitBooleanDeferred(sqlSegment);
            return sqlSegment.Change($"CASE WHEN {sqlSegment} THEN {this.GetSqlValue(ifTrue)} ELSE {this.GetSqlValue(ifFalse)} END");
        }
        if (sqlSegment.Value is bool)
        {
            if ((bool)sqlSegment.Value)
                return sqlSegment.Change($"CASE WHEN {this.GetSqlValue(ifTrue)} THEN {this.ormProvider.GetQuotedValue(true)} ELSE {this.ormProvider.GetQuotedValue(false)} END");
            else return sqlSegment.Change($"CASE WHEN {this.GetSqlValue(ifFalse)} THEN {this.ormProvider.GetQuotedValue(true)} ELSE {this.ormProvider.GetQuotedValue(false)} END");
        }
        return sqlSegment;
    }
    public virtual SqlSegment VisitListInit(SqlSegment sqlSegment)
    {
        sqlSegment.IsArray = true;
        var listExpr = sqlSegment.Expression as ListInitExpression;
        var result = new List<SqlSegment>();
        bool isConstantValue = false;
        foreach (var elementInit in listExpr.Initializers)
        {
            if (elementInit.Arguments.Count == 0)
                continue;
            var elementSegment = new SqlSegment { Expression = elementInit.Arguments[0] };
            elementSegment = this.VisitAndDeferred(elementSegment);
            if (elementSegment.IsConstantValue)
                isConstantValue = true;
            result.Add(elementSegment);
        }
        return sqlSegment.Change(result, isConstantValue);
    }
    public virtual SqlSegment VisitTypeIs(SqlSegment sqlSegment)
    {
        var binaryExpr = sqlSegment.Expression as TypeBinaryExpression;
        sqlSegment = this.Visit(sqlSegment.Next(binaryExpr.Expression));
        if (binaryExpr.TypeOperand == typeof(DBNull))
        {
            sqlSegment.Push(new DeferredExpr
            {
                OperationType = OperationType.Equal,
                Value = SqlSegment.Null
            });
        }
        return sqlSegment;
    }
    public virtual SqlSegment VisitBooleanDeferred(SqlSegment fieldSegment)
    {
        if (!fieldSegment.HasDeferred)
            return fieldSegment;

        //处理HasValue !逻辑取反操作，这种情况下是一元操作
        int notIndex = 0;
        SqlSegment sqlSegment = null;
        var operationTypes = new OperationType[] { OperationType.Equal, OperationType.Not };

        while (fieldSegment.TryPop(operationTypes, out var deferredExpr))
        {
            switch (deferredExpr.OperationType)
            {
                case OperationType.Equal:
                    sqlSegment = deferredExpr.Value as SqlSegment;
                    break;
                case OperationType.Not:
                    notIndex++;
                    break;
            }
        }
        if (sqlSegment == null)
            sqlSegment = SqlSegment.True;

        string strOperator = null;
        if (notIndex % 2 > 0)
            strOperator = sqlSegment == SqlSegment.Null ? "IS NOT" : "<>";
        else strOperator = sqlSegment == SqlSegment.Null ? "IS" : "=";
        fieldSegment.IsExpression = true;
        if (this.isSelect) fieldSegment.IsNeedParentheses = true;
        return fieldSegment.Change($"{fieldSegment} {strOperator} {this.GetSqlValue(sqlSegment)}", false);
    }
    public virtual SqlSegment VisitConcatAndDeferred(SqlSegment sqlSegment)
    {
        var concatSegments = this.VisitConcatExpr(sqlSegment.Expression);
        bool isConstantValue = true;
        foreach (var segment in concatSegments)
        {
            if (!segment.IsConstantValue)
            {
                isConstantValue = false;
                break;
            }
        }
        if (isConstantValue)
            return sqlSegment.Change(string.Concat(concatSegments));

        var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(object[]) });
        this.ormProvider.TryGetMethodCallSqlFormatter(sqlSegment, concatMethodInfo, out var formater);
        sqlSegment.IsExpression = true;
        if (this.isSelect)
        {
            switch (this.ormProvider.DatabaseType)
            {
                case DatabaseType.SqlServer:
                case DatabaseType.Postgresql:
                case DatabaseType.Oracle:
                    sqlSegment.IsNeedParentheses = true;
                    break;
            }
        }
        return sqlSegment.Change(formater.Invoke(null, null, concatSegments), false);
    }
    public virtual List<SqlSegment> VisitConcatExpr(Expression concatExpr)
    {
        Func<Expression, bool> isAddBinary = f =>
        {
            if (f is BinaryExpression binaryExpr && binaryExpr.NodeType == ExpressionType.Add && binaryExpr.Type == typeof(string)
                && (binaryExpr.Left.Type == typeof(string) || binaryExpr.Right.Type == typeof(string)))
                return true;
            return false;
        };
        Func<Expression, bool> isConcatCall = f =>
        {
            if (f is MethodCallExpression callExpr && callExpr.Method.Name == "Concat")
                return true;
            return false;
        };
        List<Expression> completedExprs = null;
        if (isAddBinary(concatExpr))
            completedExprs = this.FlattenAddConcatParameters(concatExpr);
        if (isConcatCall(concatExpr))
            completedExprs = this.FlattenConcatCallParameters(concatExpr);
        var result = new List<SqlSegment>();
        foreach (var completedExpr in completedExprs)
        {
            result.Add(this.VisitAndDeferred(new SqlSegment { Expression = completedExpr }));
        }
        return result;
    }
    public virtual List<Expression> FlattenAddConcatParameters(Expression concatExpr)
    {
        var completedExprs = new List<Expression>();
        var deferredExprs = new Stack<Expression>();
        Func<Expression, bool> isAddBinary = f =>
        {
            if (f is BinaryExpression binaryExpr && binaryExpr.NodeType == ExpressionType.Add && binaryExpr.Type == typeof(string)
                && (binaryExpr.Left.Type == typeof(string) || binaryExpr.Right.Type == typeof(string)))
                return true;
            return false;
        };
        Func<Expression, bool> isConcatCall = f =>
        {
            if (f is MethodCallExpression callExpr && callExpr.Method.Name == "Concat")
                return true;
            return false;
        };
        var nextExpr = concatExpr;
        while (true)
        {
            if (nextExpr is BinaryExpression binaryExpr)
            {
                var isLeftBinary = isAddBinary(binaryExpr.Left);
                if (isLeftBinary)
                {
                    deferredExprs.Push(binaryExpr.Right);
                    nextExpr = binaryExpr.Left as BinaryExpression;
                    continue;
                }
                if (isConcatCall(binaryExpr.Left))
                    completedExprs.AddRange(this.FlattenConcatCallParameters(binaryExpr.Left));
                else completedExprs.Add(binaryExpr.Left);

                var isRightBinary = isAddBinary(binaryExpr.Right);
                if (isRightBinary)
                {
                    nextExpr = binaryExpr.Right as BinaryExpression;
                    continue;
                }

                if (isConcatCall(binaryExpr.Right))
                    completedExprs.AddRange(this.FlattenConcatCallParameters(binaryExpr.Right));
                else completedExprs.Add(binaryExpr.Right);

                if (deferredExprs.TryPop(out var expr))
                    nextExpr = expr;
                else break;
            }
            else
            {
                completedExprs.Add(nextExpr);
                if (deferredExprs.TryPop(out var expr))
                    nextExpr = expr;
                else break;
            }
        }
        return completedExprs;
    }
    public virtual List<Expression> FlattenConcatCallParameters(Expression expr)
    {
        var concatExpr = expr as MethodCallExpression;
        var completedExprs = new List<Expression>();
        Func<Expression, bool> isAddBinary = f =>
        {
            if (f is BinaryExpression binaryExpr && binaryExpr.NodeType == ExpressionType.Add && binaryExpr.Type == typeof(string)
                && (binaryExpr.Left.Type == typeof(string) || binaryExpr.Right.Type == typeof(string)))
                return true;
            return false;
        };
        foreach (var augumentExpr in concatExpr.Arguments)
        {
            if (isAddBinary(augumentExpr))
                completedExprs.AddRange(this.FlattenAddConcatParameters(augumentExpr));
            else if (augumentExpr is MethodCallExpression callExpr && callExpr.Method.Name == "Concat")
                completedExprs.AddRange(this.FlattenConcatCallParameters(callExpr));
            else completedExprs.Add(augumentExpr);
        }
        return completedExprs;
    }
    public virtual List<SqlSegment> VisitLogicBinaryExpr(Expression expr)
    {
        Func<Expression, bool> isLogicBinary = f => f.NodeType != ExpressionType.AndAlso && f.NodeType != ExpressionType.OrElse;

        int deep = 0;
        var lastOperationType = OperationType.None;
        var deferredExprs = new Stack<SqlSegment>();
        var completedSegements = new List<SqlSegment>();
        var binaryExpr = expr as BinaryExpression;

        while (binaryExpr != null)
        {
            var operationType = binaryExpr.NodeType == ExpressionType.AndAlso ? OperationType.And : OperationType.Or;
            if (lastOperationType == OperationType.None)
                lastOperationType = operationType;
            if (operationType != lastOperationType)
            {
                lastOperationType = operationType;
                deep++;
            }

            var isLeftLeaf = isLogicBinary(binaryExpr.Left);
            var isRightLeaf = isLogicBinary(binaryExpr.Right);
            if (isLeftLeaf)
            {
                var leftSegment = this.EnsureConditionSegment(binaryExpr.Left);
                leftSegment.OperationType = operationType;
                leftSegment.Deep = deep;
                completedSegements.Add(leftSegment);
            }
            if (isRightLeaf)
            {
                var rightSegment = this.EnsureConditionSegment(binaryExpr.Right);
                rightSegment.OperationType = operationType;
                rightSegment.Deep = deep;
                deferredExprs.Push(rightSegment);
            }
            Expression nextExpr = null;
            if (isLeftLeaf && isRightLeaf) break;
            if (isLeftLeaf && !isRightLeaf)
                nextExpr = binaryExpr.Right;
            if (!isLeftLeaf && isRightLeaf)
                nextExpr = binaryExpr.Left;

            binaryExpr = nextExpr as BinaryExpression;
        }
        while (deferredExprs.TryPop(out var sqlSegment))
        {
            completedSegements.Add(sqlSegment);
        }
        return completedSegements;
    }
    public virtual SqlSegment Evaluate(SqlSegment sqlSegment)
    {
        var member = Expression.Convert(sqlSegment.Expression, typeof(object));
        var lambda = Expression.Lambda<Func<object>>(member);
        var getter = lambda.Compile();
        var objValue = getter();
        if (objValue == null)
            return SqlSegment.Null;
        return sqlSegment.Change(objValue);
    }
    public virtual T Evaluate<T>(Expression expr)
    {
        var lambda = Expression.Lambda<Func<T>>(expr);
        var getter = lambda.Compile();
        var objValue = getter();
        if (objValue == null)
            return default;
        return objValue;
    }
    public virtual SqlSegment EvaluateAndParameter(SqlSegment sqlSegment)
    {
        var member = Expression.Convert(sqlSegment.Expression, typeof(object));
        var lambda = Expression.Lambda<Func<object>>(member);
        var getter = lambda.Compile();
        var objValue = getter();
        if (objValue == null)
            return SqlSegment.Null;

        sqlSegment.Change(objValue);
        //只有字符串会变成参数，有可能sql注入
        var type = sqlSegment.Expression.Type;
        if (type == typeof(string) || this.IsEnumerableString(type))
            return this.ToParameter(sqlSegment);
        return sqlSegment;
    }
    public virtual SqlSegment VisitSqlMethodCall(SqlSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        sqlSegment.IsExpression = true;
        sqlSegment.IsConstantValue = false;
        LambdaExpression lambdaExpr = null;
        List<ParameterExpression> visitedParameters = null;
        switch (methodCallExpr.Method.Name)
        {
            case "ToFlatten":
                var returnType = methodCallExpr.Method.ReturnType;
                lambdaExpr = this.EnsureLambda(methodCallExpr.Arguments[1]);
                Expression flattenExpr = null;
                var sourceType = methodCallExpr.Arguments[0].Type;
                if (lambdaExpr.Body.GetParameters(out visitedParameters))
                    flattenExpr = Expression.Lambda(lambdaExpr.Body, visitedParameters);
                var readerFields = this.FlattenFieldsTo(returnType, flattenExpr);
                sqlSegment.Change(readerFields);
                break;
            case "In":
                var elementType = methodCallExpr.Method.GetGenericArguments()[0];
                if (methodCallExpr.Arguments[1].Type.IsArray || typeof(IEnumerable<>).MakeGenericType(elementType).IsAssignableFrom(methodCallExpr.Arguments[1].Type))
                {
                    sqlSegment = this.Evaluate(sqlSegment.Next(methodCallExpr.Arguments[1]));
                    if (sqlSegment == SqlSegment.Null)
                        return sqlSegment.Change("0=1", false);
                    sqlSegment = this.ToParameter(sqlSegment);
                }
                else
                {
                    lambdaExpr = methodCallExpr.Arguments[1] as LambdaExpression;
                    var sql = this.VisitFromQuery(lambdaExpr, out bool isNeedAlias);
                    sqlSegment.Change(sql, false);
                    if (isNeedAlias) this.isNeedAlias = true;
                }
                var fieldSegment = this.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                sqlSegment.Change($"{fieldSegment} IN ({sqlSegment})");
                break;
            case "Exists":
                lambdaExpr = this.EnsureLambda(methodCallExpr.Arguments[0]);
                this.isNeedAlias = true;
                string existsSql = null;
                var subTableTypes = methodCallExpr.Method.GetGenericArguments();
                List<IDbDataParameter> parameters = null;
                if (subTableTypes != null && subTableTypes.Length > 0)
                {
                    var removeIndices = new List<int>();
                    var builder = new StringBuilder("SELECT * FROM ");
                    int index = 0;
                    foreach (var subTableType in subTableTypes)
                    {
                        var subTableMapper = this.mapProvider.GetEntityMap(subTableType);
                        var aliasName = lambdaExpr.Parameters[index].Name;
                        var tableSegment = new TableSegment
                        {
                            EntityType = subTableType,
                            AliasName = aliasName
                        };
                        removeIndices.Add(this.tables.Count);
                        this.tables.Add(tableSegment);
                        this.tableAlias.Add(aliasName, tableSegment);
                        if (index > 0) builder.Append(',');
                        builder.Append(this.ormProvider.GetTableName(subTableMapper.TableName));
                        builder.Append($" {tableSegment.AliasName}");
                        index++;
                    }
                    builder.Append(" WHERE ");
                    builder.Append(this.VisitConditionExpr(lambdaExpr.Body));
                    removeIndices.Reverse();
                    removeIndices.ForEach(f => this.tables.RemoveAt(f));
                    existsSql = builder.ToString();
                }
                else existsSql = this.VisitFromQuery(lambdaExpr, out _);
                if (parameters != null && parameters.Count > 0)
                    this.dbParameters.AddRange(parameters);
                sqlSegment.Change($"EXISTS({existsSql})", false);
                break;
            case "Count":
            case "LongCount":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");

                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"COUNT({sqlSegment})", false);
                }
                else sqlSegment.Change("COUNT(1)", false);
                if (this.isSelect || this.isWhere)
                    this.tables.FindAll(f => f.IsMaster).ForEach(f => f.IsUsed = true);
                break;
            case "CountDistinct":
            case "LongCountDistinct":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");

                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"COUNT(DISTINCT {sqlSegment})", false);
                }
                if (this.isSelect || this.isWhere)
                    this.tables.FindAll(f => f.IsMaster).ForEach(f => f.IsUsed = true);
                break;
            case "Sum":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");
                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"SUM({sqlSegment})", false);
                }
                if (this.isSelect || this.isWhere)
                    this.tables.FindAll(f => f.IsMaster).ForEach(f => f.IsUsed = true);
                break;
            case "Avg":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");
                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"AVG({sqlSegment})", false);
                }
                if (this.isSelect || this.isWhere)
                    this.tables.FindAll(f => f.IsMaster).ForEach(f => f.IsUsed = true);
                break;
            case "Max":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");
                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"MAX({sqlSegment})", false);
                }
                if (this.isSelect || this.isWhere)
                    this.tables.FindAll(f => f.IsMaster).ForEach(f => f.IsUsed = true);
                break;
            case "Min":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");
                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"MIN({sqlSegment})", false);
                }
                if (this.isSelect || this.isWhere)
                    this.tables.FindAll(f => f.IsMaster).ForEach(f => f.IsUsed = true);
                break;
        }
        return sqlSegment;
    }
    public virtual string VisitConditionExpr(Expression conditionExpr)
    {
        if (conditionExpr.NodeType == ExpressionType.AndAlso || conditionExpr.NodeType == ExpressionType.OrElse)
        {
            int lastDeep = 0;
            var builder = new StringBuilder();
            var sqlSegments = this.VisitLogicBinaryExpr(conditionExpr);

            for (int i = 0; i < sqlSegments.Count; i++)
            {
                var sqlSegment = this.VisitAndDeferred(sqlSegments[i]);
                var separator = sqlSegment.OperationType == OperationType.And ? " AND " : " OR ";
                if (i > 0)
                {
                    if (sqlSegment.Deep < lastDeep)
                    {
                        var loopTimes = lastDeep - sqlSegment.Deep;
                        for (int j = 0; j < loopTimes; j++)
                        {
                            builder.Append(')');
                        }
                    }
                    builder.Append(separator);
                }
                if (i == 0)
                {
                    if (sqlSegment.Deep > 0)
                    {
                        for (int j = 0; j < sqlSegment.Deep; j++)
                        {
                            builder.Append('(');
                        }
                    }
                }
                else
                {
                    if (sqlSegment.Deep > lastDeep)
                    {
                        var loopTimes = sqlSegment.Deep - lastDeep;
                        for (int j = 0; j < loopTimes; j++)
                        {
                            builder.Append('(');
                        }
                    }
                }
                builder.Append(sqlSegment.ToString());
                lastDeep = sqlSegment.Deep;
            }
            if (lastDeep > 0)
            {
                while (lastDeep > 0)
                {
                    builder.Append(')');
                    lastDeep--;
                }
            }
            return builder.ToString();
        }
        return this.VisitAndDeferred(this.EnsureConditionSegment(conditionExpr)).ToString();
    }
    public virtual string GetSqlValue(SqlSegment sqlSegment)
    {
        if (sqlSegment == SqlSegment.Null || !sqlSegment.IsConstantValue)
            return sqlSegment.ToString();
        return this.ormProvider.GetQuotedValue(sqlSegment.Value);
    }
    public virtual TableSegment FindTableSegment(string parameterName, string path)
    {
        var index = path.IndexOf(".");
        if (index > 0)
        {
            var rootTableSegment = this.tableAlias[parameterName];
            path = path.Replace(parameterName + ".", rootTableSegment.AliasName + ".");
            return this.tables.Find(f => f.Path == path);
        }
        else return this.tableAlias[parameterName];
    }
    public virtual List<ReaderField> AddTableRecursiveReaderFields(int readerIndex, TableSegment fromSegment)
    {
        var readerFields = new List<ReaderField>();
        fromSegment.Mapper ??= this.mapProvider.GetEntityMap(fromSegment.EntityType);
        var lastReaderField = new ReaderField
        {
            Index = readerIndex,
            FieldType = ReaderFieldType.Entity,
            TableSegment = fromSegment,
            ReaderFields = this.FlattenTableFields(fromSegment)
            //最外层Select对象的成员，位于顶层, FromMember暂时不设置值，到Select时候去设置 
        };
        readerFields.Add(lastReaderField);
        this.AddIncludeTables(lastReaderField, readerFields);
        return readerFields;
    }
    public virtual SqlSegment ToParameter(SqlSegment sqlSegment)
    {
        if (sqlSegment == SqlSegment.Null)
            return SqlSegment.Null;

        sqlSegment.IsParameter = true;
        this.dbParameters ??= new();
        if (sqlSegment.Value is IEnumerable objValues && sqlSegment.Value.GetType() != typeof(string))
        {
            string paramPrefix = null;
            if (!string.IsNullOrEmpty(sqlSegment.ParameterName))
                paramPrefix = sqlSegment.ParameterName;
            else paramPrefix = this.ormProvider.ParameterPrefix + this.parameterPrefix;
            int index = 0;
            var builder = new StringBuilder();
            foreach (var objValue in objValues)
            {
                if (index > 0) builder.Append(',');
                var parameterName = paramPrefix + this.dbParameters.Count.ToString();
                builder.Append(parameterName);
                this.dbParameters.Add(this.ormProvider.CreateParameter(parameterName, objValue));
                index++;
            }
            return sqlSegment.Change(builder.ToString(), false);
        }
        else
        {
            string parameterName = null;
            if (!string.IsNullOrEmpty(sqlSegment.ParameterName))
                parameterName = sqlSegment.ParameterName;
            else parameterName = this.ormProvider.ParameterPrefix + this.parameterPrefix + this.dbParameters.Count.ToString();
            this.dbParameters.Add(this.ormProvider.CreateParameter(parameterName, sqlSegment.Value));
            return sqlSegment.Change(parameterName, false);
        }
    }
    public virtual bool IsFromQuery(LambdaExpression lambdaExpr)
    {
        if (!lambdaExpr.Type.IsGenericType)
            return false;
        var genericArguments = lambdaExpr.Type.GetGenericArguments();
        if (genericArguments == null || genericArguments.Length < 2)
            return false;
        if (genericArguments[0] != typeof(IFromQuery))
            return false;
        var queryType = genericArguments.Last();
        if (!queryType.IsGenericType) return false;
        var queryGenericArguments = queryType.GetGenericArguments();
        if (queryGenericArguments == null || queryGenericArguments.Length < 1)
            return false;

        if (!typeof(IFromQuery<>).MakeGenericType(queryGenericArguments[0]).IsAssignableFrom(queryType))
            return false;
        return true;
    }
    public virtual string VisitFromQuery(LambdaExpression lambdaExpr, out bool isNeedAlias)
    {
        var currentExpr = lambdaExpr.Body;
        var callStack = new Stack<MethodCallExpression>();
        while (true)
        {
            if (currentExpr.NodeType == ExpressionType.Parameter)
                break;

            if (currentExpr is MethodCallExpression callExpr)
            {
                callStack.Push(callExpr);
                currentExpr = callExpr.Object;
            }
        }
        var queryVisitor = this.ToQueryVisitor();
        while (callStack.TryPop(out var callExpr))
        {
            var genericArguments = callExpr.Method.GetGenericArguments();
            LambdaExpression lambdaArgsExpr = null;
            switch (callExpr.Method.Name)
            {
                case "From":
                case "Union":
                case "UnionAll":
                    queryVisitor.From(this.Evaluate<char>(callExpr.Arguments[0]), genericArguments);
                    break;
                case "InnerJoin":
                case "LeftJoin":
                case "RightJoin":
                    var joinType = callExpr.Method.Name switch
                    {
                        "LeftJoin" => "LEFT JOIN",
                        "RightJoin" => "RIGHT JOIN",
                        _ => "INNER JOIN"
                    };
                    queryVisitor.isNeedAlias = true;
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    if (lambdaArgsExpr.Body.GetParameters(out var visitedParameters))
                    {
                        foreach (var tableAlias in this.tableAlias.Keys)
                        {
                            if (visitedParameters.Exists(f => f.Name == tableAlias))
                                queryVisitor.AddTable(this.tableAlias[tableAlias]);
                        }
                        lambdaArgsExpr = Expression.Lambda(lambdaArgsExpr.Body, visitedParameters);
                    }
                    queryVisitor.Join(joinType, genericArguments[0], lambdaArgsExpr);
                    break;
                case "Where":
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.InitTableAlias(lambdaArgsExpr);
                    if (lambdaArgsExpr.Body.GetParameters(out visitedParameters))
                    {
                        queryVisitor.isNeedAlias = true;
                        foreach (var tableAlias in this.tableAlias.Keys)
                        {
                            if (visitedParameters.Exists(f => f.Name == tableAlias))
                            {
                                var tableSegment = this.tableAlias[tableAlias];
                                queryVisitor.tableAlias.Add(tableAlias, tableSegment);
                            }
                        }
                        lambdaArgsExpr = Expression.Lambda(lambdaArgsExpr.Body, visitedParameters);
                    }
                    queryVisitor.Where(lambdaArgsExpr, false);
                    break;
                case "And":
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[1]);
                    if (this.Evaluate<bool>(callExpr.Arguments[0]))
                        queryVisitor.And(lambdaArgsExpr);
                    break;
                case "GroupBy":
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.GroupBy(lambdaArgsExpr);
                    break;
                case "Having":
                    if (callExpr.Arguments.Count > 1 && this.Evaluate<bool>(callExpr.Arguments[0]))
                    {
                        lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[1]);
                        queryVisitor.Having(lambdaArgsExpr);
                    }
                    else
                    {
                        lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                        queryVisitor.Having(lambdaArgsExpr);
                    }
                    break;
                case "OrderBy":
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.OrderBy("ASC", lambdaArgsExpr);
                    break;
                case "OrderByDescending":
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.OrderBy("DESC", lambdaArgsExpr);
                    break;
                case "Select":
                case "SelectAggregate":
                    if (callExpr.Arguments[0].NodeType == ExpressionType.Constant)
                        queryVisitor.Select(this.Evaluate<string>(callExpr.Arguments[0]));
                    else
                    {
                        lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                        queryVisitor.Select(null, lambdaArgsExpr);
                    }
                    break;
                case "Distinct":
                    queryVisitor.Distinct();
                    break;
            }
        }
        var result = queryVisitor.BuildSql(out var dbDataParameters, out _);
        if (dbDataParameters != null && dbDataParameters.Count > 0)
        {
            this.dbParameters ??= new();
            this.dbParameters.AddRange(dbDataParameters);
        }
        isNeedAlias = queryVisitor.isNeedAlias;
        return result;
    }
    public List<ReaderField> FlattenFieldsTo(Type entityType, Expression toTargetExpr)
    {
        var targetType = entityType;
        List<ReaderField> initReaderFields = null;
        if (toTargetExpr == null)
            throw new ArgumentNullException(nameof(toTargetExpr));
        var lambdaExpr = toTargetExpr as LambdaExpression;
        //临时添加虚拟表
        this.tableAlias.Clear();
        this.tableAlias.Add(lambdaExpr.Parameters[0].Name, new TableSegment
        {
            TableType = TableType.MapTable,
            ReaderFields = this.readerFields
        });
        initReaderFields = this.ConstructorFieldsTo(lambdaExpr);
        targetType = lambdaExpr.ReturnType;


        var targetMembers = targetType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

        int index = 0;
        var targetFields = new List<ReaderField>();
        foreach (var memberInfo in targetMembers)
        {
            //相同名称属性只取第一个名称相等的对应字段
            ReaderField readerField = null;
            if (initReaderFields != null)
                readerField = initReaderFields.Find(f => f.FromMember.Name == memberInfo.Name);
            readerField ??= this.readerFields.Find(f => f.FromMember.Name == memberInfo.Name);
            //没有赋值的成员，默认值
            if (readerField == null)
                continue;
            readerField.FromMember = memberInfo;
            readerField.Index = index;
            targetFields.Add(readerField);
            index++;
        }
        return targetFields;
    }
    public List<ReaderField> ConstructorFieldsTo(LambdaExpression toTargetExpr)
    {
        var result = new List<ReaderField>();
        if (toTargetExpr != null)
        {
            var sqlSegment = new SqlSegment { Expression = toTargetExpr.Body };
            switch (toTargetExpr.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    sqlSegment = this.VisitMemberAccess(sqlSegment);
                    if (sqlSegment.MemberType == ReaderFieldType.Entity
                        || sqlSegment.MemberType == ReaderFieldType.AnonymousObject)
                        result = sqlSegment.Value as List<ReaderField>;
                    else
                    {
                        result.Add(new ReaderField
                        {
                            Index = 0,
                            TableSegment = sqlSegment.TableSegment,
                            FieldType = sqlSegment.MemberType,
                            FromMember = sqlSegment.FromMember,
                            Body = sqlSegment.Value.ToString()
                        });
                    }
                    break;
                case ExpressionType.New:
                    sqlSegment = this.VisitNew(sqlSegment);
                    result = sqlSegment.Value as List<ReaderField>;
                    break;
                case ExpressionType.MemberInit:
                    sqlSegment = this.VisitMemberInit(sqlSegment);
                    result = sqlSegment.Value as List<ReaderField>;
                    break;
                case ExpressionType.Parameter:
                    sqlSegment = this.VisitParameter(sqlSegment);
                    result = sqlSegment.Value as List<ReaderField>;
                    if (this.isSelect) this.isTarget = true;
                    break;
                default:
                    //单个字段，方法调用
                    sqlSegment = this.VisitAndDeferred(sqlSegment);
                    if (sqlSegment.Value is List<ReaderField> readerFields)
                        result.AddRange(readerFields);
                    else
                    {
                        result.Add(new ReaderField
                        {
                            Index = 0,
                            TableSegment = sqlSegment.TableSegment,
                            FromMember = sqlSegment.FromMember,
                            Body = sqlSegment.Value.ToString()
                        });
                    }
                    break;
            }
        }
        return result;
    }
    public void FlattenReaderFields(List<ReaderField> readerFields, List<ReaderField> result)
    {
        foreach (var readerField in readerFields)
        {
            if (readerField.FieldType == ReaderFieldType.Entity)
                result.AddRange(readerField.ReaderFields);
            else if (readerField.FieldType == ReaderFieldType.AnonymousObject)
            {
                if (readerField.ReaderFields != null && readerField.ReaderFields.Count > 0)
                    this.FlattenReaderFields(readerField.ReaderFields, result);
            }
            else result.Add(readerField);
        }
    }
    public List<ReaderField> FlattenTableFields(TableSegment tableSegment)
    {
        var targetFields = new List<ReaderField>();
        if (tableSegment.TableType == TableType.MapTable)
        {
            var memberInfos = tableSegment.EntityType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
               .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
            foreach (var memberInfo in memberInfos)
            {
                targetFields.Add(new ReaderField
                {
                    FromMember = memberInfo,
                    FieldType = ReaderFieldType.Field,
                    Body = this.GetFieldName(tableSegment, memberInfo.Name)
                });
            }
        }
        else
        {
            tableSegment.Mapper ??= this.mapProvider.GetEntityMap(tableSegment.EntityType);
            foreach (var memberMapper in tableSegment.Mapper.MemberMaps)
            {
                if (memberMapper.IsIgnore || memberMapper.IsNavigation
                    || (memberMapper.MemberType.IsEntityType() && memberMapper.TypeHandler == null))
                    continue;
                targetFields.Add(new ReaderField
                {
                    FromMember = memberMapper.Member,
                    FieldType = ReaderFieldType.Field,
                    Body = this.GetFieldName(tableSegment, memberMapper.FieldName)
                });
            }
        }
        return targetFields;
    }
    public ReaderField FindReaderField(MemberExpression memberExpr, List<ReaderField> readerFields)
    {
        var currentExpr = memberExpr;
        var visitedStack = new Stack<string>();
        while (true)
        {
            if (currentExpr == null || currentExpr.NodeType == ExpressionType.Parameter)
                break;
            visitedStack.Push(currentExpr.Member.Name);
            currentExpr = currentExpr.Expression as MemberExpression;
        }
        List<ReaderField> currentFields = readerFields;
        ReaderField readerField = null;
        while (visitedStack.TryPop(out var memberName))
        {
            readerField = currentFields.Find(f => f.FromMember.Name == memberName);
            if (readerField.ReaderFields != null && readerField.ReaderFields.Count > 0)
                currentFields = readerField.ReaderFields;
        }
        return readerField;
    }
    public string GetFieldName(TableSegment tableSegment, string fieldName)
    {
        fieldName = this.ormProvider.GetFieldName(fieldName);
        if (this.isNeedAlias && !string.IsNullOrEmpty(tableSegment.AliasName))
            return tableSegment.AliasName + "." + fieldName;
        return fieldName;
    }
    public string GetFieldAliasName(string fieldName)
    {
        if (this.ormProvider.DatabaseType == DatabaseType.Postgresql)
            return this.ormProvider.GetFieldName(fieldName);
        return fieldName;
    }
    private SqlSegment EnsureConditionSegment(Expression conditionExpr)
    {
        var result = new SqlSegment { Expression = conditionExpr };
        if (conditionExpr.NodeType == ExpressionType.MemberAccess && conditionExpr.Type == typeof(bool))
        {
            result.DeferredExprs ??= new();
            result.DeferredExprs.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = true });
        }
        return result;
    }
    public void Swap<T>(ref T left, ref T right)
    {
        var temp = right;
        right = left;
        left = temp;
    }
    private void AddIncludeTables(ReaderField lastReaderField, List<ReaderField> readerFields)
    {
        var includedSegments = this.tables.FindAll(f => !f.IsMaster && f.FromTable == lastReaderField.TableSegment);
        if (includedSegments != null && includedSegments.Count > 0)
        {
            lastReaderField.HasNextInclude = true;
            foreach (var includedSegment in includedSegments)
            {
                var readerField = new ReaderField
                {
                    Index = readerFields.Count,
                    FieldType = ReaderFieldType.Entity,
                    TableSegment = includedSegment,
                    FromMember = includedSegment.FromMember.Member,
                    ParentIndex = lastReaderField.Index,
                    ReaderFields = this.FlattenTableFields(includedSegment)
                };
                //主表使用，include子表也将使用
                includedSegment.IsUsed = lastReaderField.TableSegment.IsUsed;
                readerFields.Add(readerField);
                if (this.tables.Exists(f => !f.IsMaster && f.FromTable == includedSegment))
                    this.AddIncludeTables(readerField, readerFields);
            }
        }
    }
    private bool IsEnumerableString(Type type)
    {
        if (!typeof(IEnumerable).IsAssignableFrom(type))
            return false;
        var genericArguments = type.GetGenericArguments();
        if (genericArguments == null || genericArguments.Length <= 0)
            return false;
        if (genericArguments[0] == typeof(string))
            return true;
        return false;
    }
    private LambdaExpression EnsureLambda(Expression expr)
    {
        if (expr.NodeType == ExpressionType.Lambda)
            return expr as LambdaExpression;
        var currentExpr = expr;
        while (true)
        {
            if (currentExpr.NodeType == ExpressionType.Lambda)
                break;

            if (currentExpr is UnaryExpression unaryExpr)
                currentExpr = unaryExpr.Operand;
        }
        return currentExpr as LambdaExpression;
    }
}
