using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Trolley;

abstract class SqlVisitor
{
    private readonly IOrmDbFactory dbFactory;
    private readonly IOrmProvider ormProvider;
    private List<IDbDataParameter> dbParameters;
    private string parameterPrefix;

    private List<TableSegment> tables = new();
    private Dictionary<string, TableSegment> tableAlias = new();
    public SqlVisitor(IOrmDbFactory dbFactory, IOrmProvider ormProvider, string parameterPrefix = "p")
    {
        this.dbFactory = dbFactory;
        this.ormProvider = ormProvider;
        this.parameterPrefix = parameterPrefix;
    }

    protected virtual SqlSegment VisitAndDeferred(SqlSegment sqlSegment)
      => this.VisitBooleanDeferred(this.Visit(sqlSegment));
    protected virtual SqlSegment Visit(SqlSegment sqlSegment)
    {
        if (sqlSegment == SqlSegment.None)
            return SqlSegment.None;

        SqlSegment result = null;
        while (sqlSegment.Expression != null)
        {
            switch (sqlSegment.Expression.NodeType)
            {
                case ExpressionType.Lambda:
                    var lambdaExpr = sqlSegment.Expression as LambdaExpression;
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
                    //default: return nextExpr.ToString();
            }
            return result;
        }
        return result;
    }
    protected virtual SqlSegment VisitUnary(SqlSegment sqlSegment)
    {
        var unaryExpr = sqlSegment.Expression as UnaryExpression;
        switch (unaryExpr.NodeType)
        {
            case ExpressionType.Not:
                if (unaryExpr.Type == typeof(bool))
                {
                    if (unaryExpr.Operand.IsParameter(out _))
                    {
                        //目前只有Not，一元/二元 bool类型才有延时处理,到参数表达式再统一处理
                        sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                        return this.Visit(sqlSegment.Next(unaryExpr.Operand));
                    }
                    //TODO:测试Value赋值
                    return sqlSegment.Next(unaryExpr.Operand, $"NOT ({this.Visit(sqlSegment)})");
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
    protected virtual SqlSegment VisitBinary(SqlSegment sqlSegment)
    {
        var binaryExpr = sqlSegment.Expression as BinaryExpression;
        switch (binaryExpr.NodeType)
        {
            //And/Or，已经在Where/Having中单独处理了
            //case ExpressionType.AndAlso:
            //case ExpressionType.OrElse:
            //var operationType = binaryExpr.NodeType == ExpressionType.AndAlso ?
            //         OperationType.And : OperationType.Or;
            //if (sqlSegment.OperationType == OperationType.None)
            //    sqlSegment.OperationType = operationType;
            //if (sqlSegment.OperationType != operationType)
            //{
            //    sqlSegment.Push(new DeferredExpr
            //    {
            //        OperationType = sqlSegment.OperationType,
            //        Value = sqlSegment.Value
            //    });
            //}
            //sqlSegment.Push(new DeferredExpr
            //{
            //    OperationType = operationType,
            //    Value = binaryExpr.Right
            //});
            //return sqlSegment.Next(binaryExpr.Left);


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
                if (binaryExpr.NodeType == ExpressionType.Add && binaryExpr.Left.Type == typeof(string) && binaryExpr.Right.Type == typeof(string))
                    return sqlSegment.Change(this.VisitConcatAndDeferred(binaryExpr));

                var leftSegment = this.Visit(sqlSegment.Next(binaryExpr.Left));
                var rightSegment = this.Visit(sqlSegment.Next(binaryExpr.Right));
                var operators = this.GetOperator(binaryExpr.NodeType);

                if (binaryExpr.NodeType == ExpressionType.Modulo || binaryExpr.NodeType == ExpressionType.Coalesce)
                    return leftSegment.Change($"{operators}({leftSegment},{rightSegment})");
                if (binaryExpr.NodeType == ExpressionType.Equal || binaryExpr.NodeType == ExpressionType.NotEqual)
                {
                    if (!leftSegment.HasField && rightSegment.HasField)
                        this.Swap(ref leftSegment, ref rightSegment);

                    //处理!(a.IsEnabled==true)情况,bool类型，最外层再做defer处理
                    if (binaryExpr.Left.Type == typeof(bool) && leftSegment.HasField && !rightSegment.HasField)
                    {
                        if (!(bool)rightSegment.Value)
                            leftSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                        if (binaryExpr.NodeType == ExpressionType.NotEqual)
                            leftSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                        return sqlSegment.Merge(leftSegment);
                    }
                    //处理a.UserName!=null情况
                    if (leftSegment == SqlSegment.Null)
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

                return leftSegment.Merge(rightSegment, $"{this.GetSqlValue(leftSegment)}{operators}{this.GetSqlValue(rightSegment)}");
        }
        return sqlSegment;
    }
    protected virtual SqlSegment VisitMemberAccess(SqlSegment sqlSegment)
    {
        var memberExpr = sqlSegment.Expression as MemberExpression;
        if (memberExpr.Expression != null)
        {
            //Where(f=>... && !f.OrderId.HasValue && ...)
            //Where(f=>... f.OrderId.Value==10 && ...)
            //Select(f=>... ,f.OrderId.HasValue  ...)
            //Select(f=>... ,f.OrderId.Value==10  ...)
            if (Nullable.GetUnderlyingType(memberExpr.Member.DeclaringType) != null)
            {
                if (memberExpr.Member.Name == nameof(Nullable<bool>.HasValue))
                {
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlSegment.Null });
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                    return sqlSegment.Next(memberExpr.Expression);
                }
                else if (memberExpr.Member.Name == nameof(Nullable<bool>.Value))
                    return sqlSegment.Next(memberExpr.Expression);
                else throw new ArgumentException($"不支持的MemberAccess操作，表达式'{memberExpr}'返回值不是boolean类型");
            }

            //各种类型值的属性访问，如：DateTime,TimeSpan,String.Length,List.Count,
            if (this.ormProvider.TryGetMemberAccessSqlFormatter(memberExpr.Member, out var formatter))
            {
                //Where(f=>... && f.OrderNo.Length==10 && ...)
                //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
                var targetSegment = this.Visit(sqlSegment.Next(memberExpr.Expression));
                return sqlSegment.Change(formatter.Invoke(targetSegment));
            }

            if (memberExpr.IsParameter(out var parameterName))
            {
                //Where(f=>... && f.Amount>5 && ...)
                //Include(f=>f.Buyer); 或是 IncludeMany(f=>f.Orders)
                //Select(f=>new {f.OrderId, ...})
                //Where(f=>f.Order.Id>10)
                //Include(f=>f.Order.Buyer)
                //Select(f=>new {f.Order.OrderId, ...})
                //GroupBy(f=>new {f.Order.OrderId, ...})
                //GroupBy(f=>f.Order.OrderId)
                //OrderBy(f=>new {f.Order.OrderId, ...})
                //OrderBy(f=>f.Order.OrderId)
                string path = null;
                TableSegment tableSegment = null;

                if (memberExpr.Type.IsEntityType())
                {
                    path = memberExpr.Expression.ToString();
                    var fromSegment = this.FindTableSegment(parameterName, path);
                    if (fromSegment == null)
                        throw new Exception($"使用导航属性前，要先使用Include方法包含进来，访问路径:{path}");
                    if (fromSegment.Mapper == null)
                        fromSegment.Mapper = this.dbFactory.GetEntityMap(fromSegment.EntityType);

                    var vavigationMapper = fromSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                    if (!vavigationMapper.IsNavigation)
                        throw new Exception($"类{tableSegment.EntityType.FullName}属性{memberExpr.Member.Name}未配置为导航属性");

                    path = memberExpr.ToString();
                    tableSegment = this.FindTableSegment(parameterName, path);
                    if (tableSegment == null)
                        throw new Exception($"使用导航属性前，要先使用Include方法包含进来，访问路径:{path}");

                    var memberSegments = this.GetTableFields(tableSegment, 1, tableSegment.FromMember);
                    return new SqlSegment
                    {
                        IsCompleted = true,
                        HasField = true,
                        TableSegment = tableSegment,
                        Value = memberSegments
                    };
                }
                else
                {
                    path = memberExpr.Expression.ToString();
                    tableSegment = this.FindTableSegment(parameterName, path);
                    if (tableSegment == null)
                        throw new Exception($"使用导航属性前，要先使用Include方法包含进来，访问路径:{path}");
                    if (tableSegment.Mapper == null)
                        tableSegment.Mapper = this.dbFactory.GetEntityMap(tableSegment.EntityType);

                    var memberMapper = tableSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                    var fieldName = this.ormProvider.GetFieldName(memberMapper.FieldName);
                    if (sqlSegment.HasDeferred)
                    {
                        return this.VisitBooleanDeferred(new SqlSegment
                        {
                            IsCompleted = true,
                            Expression = null,
                            HasField = true,
                            TableSegment = tableSegment,
                            MemberMapper = memberMapper,
                            OperationType = sqlSegment.OperationType,
                            DeferredExprs = sqlSegment.DeferredExprs,
                            Value = $"{tableSegment.AliasName}.{fieldName}"
                        });
                    }
                    return new SqlSegment
                    {
                        IsCompleted = true,
                        Expression = null,
                        HasField = true,
                        OperationType = sqlSegment.OperationType,
                        TableSegment = tableSegment,
                        MemberMapper = memberMapper,
                        Value = $"{tableSegment.AliasName}.{fieldName}"
                    };
                }
            }
        }
        //访问局部变量或是成员变量，当作常量处理,直接计算，如果是字符串变成参数@p
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId)); orderIds
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        return this.EvaluateAndParameter(sqlSegment);
    }
    protected virtual SqlSegment VisitConstant(SqlSegment sqlSegment)
    {
        var constantExpr = sqlSegment.Expression as ConstantExpression;
        if (constantExpr.Value == null)
            return SqlSegment.Null;
        return sqlSegment.Change(constantExpr.Value);
    }
    protected virtual SqlSegment VisitMethodCall(SqlSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        if (!this.ormProvider.TryGetMethodCallSqlFormatter(methodCallExpr.Method, out var formatter))
            throw new Exception($"不支持的方法访问，或是IOrmProvider未实现此方法{methodCallExpr.Method.Name}");

        object target = null;
        object[] args = null;
        //如果方法对象是聚合查询，不做任何处理
        if (methodCallExpr.Object?.Type != typeof(IAggregateSelect))
            target = this.Visit(sqlSegment.Next(methodCallExpr.Object));

        if (methodCallExpr.Object?.Type == typeof(IAggregateSelect))
        {
            if (methodCallExpr.Arguments.Count > 1)
                throw new Exception("聚合查询Count，暂时不支持多个参数");

            var argumentExpr = methodCallExpr.Arguments[0];
            //访问1:N关系的导航属性，做Count操作，当作取子表数据COUNT,如：a.Count(b.Details)
            if (methodCallExpr.Method.IsGenericMethod
               && methodCallExpr.Method.Name.Contains("Count")
               && argumentExpr.NodeType == ExpressionType.MemberAccess
               && argumentExpr.Type.GenericTypeArguments[0].IsEntityType())
                throw new Exception($"聚合查询方法{methodCallExpr.Method.Name}，参数必须是字段，不能是导航属性实体，或者使用无参数聚合函数");
        }
        //TODO:字符串连接单独特殊处理
        if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count > 0)
        {
            var arguments = new List<object>();
            foreach (var argumentExpr in methodCallExpr.Arguments)
            {
                arguments.Add(this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr }));
            }
            args = arguments.ToArray();
        }
        var result = formatter.Invoke(target, sqlSegment.DeferredExprs, args);
        sqlSegment.IsMethodCall = true;
        return sqlSegment.Change(result);
    }
    protected virtual SqlSegment VisitParameter(SqlSegment sqlSegment)
    {
        var parameterExpr = sqlSegment.Expression as ParameterExpression;
        if (typeof(IAggregateSelect).IsAssignableFrom(parameterExpr.Type))
            return new SqlSegment { Value = parameterExpr.Name };
        if (typeof(IWhereSql).IsAssignableFrom(parameterExpr.Type))
            return new SqlSegment { Value = parameterExpr.Name };

        //处理Include表字段
        var fromSegment = this.tableAlias[parameterExpr.Name];
        //参数的FromMeber没有设置，在Select的时候设置
        var memberSegments = this.GetTableFields(fromSegment);
        return new SqlSegment
        {
            HasField = true,
            IsCompleted = true,
            TableSegment = fromSegment,
            Expression = parameterExpr,
            Value = memberSegments
        };
    }
    protected abstract SqlSegment VisitNew(SqlSegment sqlSegment);
    protected abstract SqlSegment VisitMemberInit(SqlSegment sqlSegment);
    protected virtual SqlSegment VisitNewArray(SqlSegment sqlSegment)
    {
        var newArrayExpr = sqlSegment.Expression as NewArrayExpression;
        var result = new List<object>();
        foreach (var elementExpr in newArrayExpr.Expressions)
        {
            result.Add(this.Visit(new SqlSegment { Expression = elementExpr }));
        }
        return sqlSegment.Change(result);
    }
    protected virtual SqlSegment VisitIndexExpression(SqlSegment sqlSegment)
    {
        var indexExpr = sqlSegment.Expression as IndexExpression;
        var argExpr = indexExpr.Arguments[0];
        var objIndex = argExpr is ConstantExpression constant
            ? constant.Value : this.Evaluate(sqlSegment.Next(argExpr)).Value;
        var index = (int)Convert.ChangeType(objIndex, typeof(int));
        var objTarget = this.Evaluate(sqlSegment.Next(indexExpr.Object)).Value;
        if (objTarget is List<object> objList)
            return sqlSegment.Change(objList[index]);
        throw new NotImplementedException("不支持的表达式: " + indexExpr);
    }
    protected virtual SqlSegment VisitConditional(SqlSegment sqlSegment)
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
    protected virtual SqlSegment VisitBooleanDeferred(SqlSegment fieldSegment)
    {
        if (!fieldSegment.HasDeferred)
            return fieldSegment;

        //处理HasValue !逻辑取反操作，这种情况下是一元操作
        int notIndex = 0;
        var sqlSegment = SqlSegment.None;
        var operationTypes = new OperationType[] { OperationType.Equal, OperationType.Not };

        while (fieldSegment.TryPop(operationTypes, out var deferredExpr))
        {
            if (deferredExpr.OperationType == OperationType.Equal)
            {
                sqlSegment = deferredExpr.Value as SqlSegment;
                break;
            }
            if (deferredExpr.OperationType == OperationType.Not)
                notIndex++;
        }
        if (sqlSegment == SqlSegment.None)
            sqlSegment = SqlSegment.True;

        string strOperator = null;
        if (notIndex % 2 > 0)
            strOperator = sqlSegment == SqlSegment.Null ? "IS NOT" : "<>";
        else strOperator = sqlSegment == SqlSegment.Null ? "IS" : "=";

        return fieldSegment.Merge(sqlSegment, $"{fieldSegment} {strOperator} {this.GetSqlValue(sqlSegment)}");
    }
    protected virtual string VisitConcatAndDeferred(Expression concatExpr)
    {
        var concatSegments = this.VisitConcatExpr(concatExpr);
        var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(object[]) });
        this.ormProvider.TryGetMethodCallSqlFormatter(concatMethodInfo, out var formater);
        for (int i = 0; i < concatSegments.Count; i++)
        {
            concatSegments[i] = this.Visit(concatSegments[i]);
        }
        return formater.Invoke(null, null, concatSegments);
    }
    protected virtual List<SqlSegment> VisitConcatExpr(Expression concatExpr)
    {
        var deferredExprs = new Stack<SqlSegment>();
        var completedSegements = new List<SqlSegment>();
        var binaryExpr = concatExpr as BinaryExpression;
        Func<Expression, bool> isConcatBinary = f =>
        {
            if (f is BinaryExpression binaryExpr && binaryExpr.NodeType == ExpressionType.Add
                && binaryExpr.Left.Type == typeof(string) && binaryExpr.Right.Type == typeof(string))
                return true;
            return false;
        };

        while (binaryExpr != null)
        {
            var isLeftLeaf = isConcatBinary(binaryExpr.Left);
            var isRightLeaf = isConcatBinary(binaryExpr.Right);
            if (isLeftLeaf)
            {
                completedSegements.Add(new SqlSegment
                {
                    OperationType = OperationType.Concat,
                    Expression = binaryExpr.Left
                });
            }
            if (isRightLeaf)
            {
                deferredExprs.Push(new SqlSegment
                {
                    OperationType = OperationType.Concat,
                    Expression = binaryExpr.Right
                });
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
    protected virtual string VisitConditionExpr(Expression conditionExpr)
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
            return builder.ToString();
        }
        return this.VisitAndDeferred(new SqlSegment { Expression = conditionExpr }).ToString();
    }
    protected virtual List<SqlSegment> VisitLogicBinaryExpr(Expression expr)
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
                deep++;

            var isLeftLeaf = isLogicBinary(binaryExpr.Left);
            var isRightLeaf = isLogicBinary(binaryExpr.Right);
            if (isLeftLeaf)
            {
                var leftSegment = new SqlSegment
                {
                    OperationType = operationType,
                    Expression = binaryExpr.Left,
                    Deep = deep
                };
                if (binaryExpr.Left.NodeType == ExpressionType.MemberAccess)
                    leftSegment.DeferredExprs.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlSegment.True });
                completedSegements.Add(leftSegment);
            }
            if (isRightLeaf)
            {
                var rightSegment = new SqlSegment
                {
                    OperationType = operationType,
                    Expression = binaryExpr.Right,
                    Deep = deep
                };
                if (binaryExpr.Right.NodeType == ExpressionType.MemberAccess)
                    rightSegment.DeferredExprs.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlSegment.True });
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
    protected virtual string GetSqlValue(SqlSegment sqlSegment)
    {
        if (sqlSegment == SqlSegment.Null || sqlSegment.HasField || sqlSegment.IsParameter)
            return sqlSegment.ToString();
        return this.ormProvider.GetQuotedValue(sqlSegment.Value);
    }
    protected virtual SqlSegment Evaluate(SqlSegment sqlSegment)
    {
        var member = Expression.Convert(sqlSegment.Expression, typeof(object));
        var lambda = Expression.Lambda<Func<object>>(member);
        var getter = lambda.Compile();
        var objValue = getter();
        if (objValue == null)
            return SqlSegment.Null;
        return sqlSegment.Change(objValue);
    }
    protected virtual SqlSegment EvaluateAndParameter(SqlSegment sqlSegment)
    {
        var member = Expression.Convert(sqlSegment.Expression, typeof(object));
        var lambda = Expression.Lambda<Func<object>>(member);
        var getter = lambda.Compile();
        var objValue = getter();
        if (objValue == null)
            return SqlSegment.Null;

        //只有字符串会变成参数，有可能sql注入
        if (sqlSegment.Expression.Type == typeof(string))
        {
            if (this.dbParameters == null)
                this.dbParameters = new List<IDbDataParameter>();
            string parameterName = null;
            if (!string.IsNullOrEmpty(sqlSegment.ParameterName)) parameterName = sqlSegment.ParameterName;
            else parameterName = $"{this.ormProvider.ParameterPrefix}{this.parameterPrefix}{this.dbParameters.Count + 1}";
            this.dbParameters.Add(this.ormProvider.CreateParameter(parameterName, objValue));
            return sqlSegment.Change(parameterName);
        }
        return sqlSegment.Change(objValue);
    }
    protected virtual void Swap<T>(ref T left, ref T right)
    {
        var temp = right;
        right = left;
        left = temp;
    }
    protected virtual string GetOperator(ExpressionType exprType)
    {
        switch (exprType)
        {
            case ExpressionType.Equal: return "=";
            case ExpressionType.NotEqual: return "<>";
            case ExpressionType.GreaterThan: return ">";
            case ExpressionType.GreaterThanOrEqual: return ">=";
            case ExpressionType.LessThan: return "<";
            case ExpressionType.LessThanOrEqual: return "<=";
            case ExpressionType.AndAlso: return "AND";
            case ExpressionType.OrElse: return "OR";
            case ExpressionType.Add: return "+";
            case ExpressionType.Subtract: return "-";
            case ExpressionType.Multiply: return "*";
            case ExpressionType.Divide: return "/";
            case ExpressionType.Modulo: return "MOD";
            case ExpressionType.Coalesce: return "COALESCE";
            case ExpressionType.And: return "&";
            case ExpressionType.Or: return "|";
            case ExpressionType.ExclusiveOr: return "^";
            case ExpressionType.LeftShift: return "<<";
            case ExpressionType.RightShift: return ">>";
            default: return exprType.ToString();
        }
    }
    private TableSegment FindTableSegment(string parameterName, string path)
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
    private List<MemberSegment> GetTableFields(TableSegment fromSegment, int tableIndex = 1, MemberInfo fromMember = null)
    {
        var memberSegments = new List<MemberSegment>();
        this.AddTableFields(fromSegment, memberSegments, tableIndex, fromMember);
        var fromTables = new List<TableSegment>() { fromSegment };
        foreach (var tableSegment in this.tables)
        {
            if (tableSegment == fromSegment) continue;
            if (fromTables.Contains(tableSegment.IncludedFrom))
            {
                tableIndex++;
                fromTables.Add(tableSegment);
                this.AddTableFields(tableSegment, memberSegments, tableIndex, tableSegment.FromMember);
            }
        }
        return memberSegments;
    }
    private void AddTableFields(TableSegment fromSegment, List<MemberSegment> memberSegments, int tableIndex, MemberInfo fromMember)
    {
        var memberMappers = fromSegment.Mapper.GetMembers();
        foreach (var memberInfo in memberMappers)
        {
            if (memberInfo.GetMemberType().IsEntityType())
                continue;
            var memberMapper = fromSegment.Mapper.GetMemberMap(memberInfo.Name);
            memberSegments.Add(new MemberSegment
            {
                //此处FromIndex,FromMember,Body不做处理，不知道是否作为Select内容
                FromMember = fromMember ?? memberInfo,
                MemberMapper = memberMapper,
                TableSegment = fromSegment,
                Body = $"{fromSegment.AliasName}.{this.ormProvider.GetFieldName(memberMapper.FieldName)}",
                IsNeedAlias = false,
                TableIndex = tableIndex
            });
        }
    }
}
