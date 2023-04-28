using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

public class SqlVisitor : ISqlVisitor
{
    protected internal readonly string dbKey;
    protected internal readonly IOrmProvider ormProvider;
    protected internal char tableAsStart = 'a';
    protected readonly IEntityMapProvider mapProvider;

    protected string parameterPrefix = "p";
    protected bool isParameterized;
    protected bool isTarget;
    protected List<TableSegment> tables;
    protected Dictionary<string, TableSegment> tableAlias;
    protected List<IDbDataParameter> dbParameters;
    protected List<ReaderField> readerFields;
    protected bool isNeedAlias = false;
    protected bool isSelect = false;
    protected bool isWhere = false;
    protected bool isFromQuery = false;

    public SqlVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
    {
        this.dbKey = dbKey;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.isParameterized = isParameterized;
        this.tableAsStart = tableAsStart;
        this.parameterPrefix = parameterPrefix;
    }
    public virtual SqlSegment VisitAndDeferred(SqlSegment sqlSegment)
    {
        sqlSegment = this.Visit(sqlSegment);
        if (!sqlSegment.HasDeferred)
            return sqlSegment;

        //处理HasValue !逻辑取反操作，这种情况下是一元操作
        int notIndex = 0;
        SqlSegment deferredSegment = null;
        var operationTypes = new OperationType[] { OperationType.Equal, OperationType.Not };

        while (sqlSegment.TryPop(operationTypes, out var deferredExpr))
        {
            switch (deferredExpr.OperationType)
            {
                case OperationType.Equal:
                    deferredSegment = deferredExpr.Value as SqlSegment;
                    break;
                case OperationType.Not:
                    notIndex++;
                    break;
            }
        }
        if (deferredSegment == null)
            deferredSegment = SqlSegment.True;

        string strOperator = null;
        if (notIndex % 2 > 0)
            strOperator = deferredSegment == SqlSegment.Null ? "IS NOT" : "<>";
        else strOperator = deferredSegment == SqlSegment.Null ? "IS" : "=";
        sqlSegment.IsExpression = true;

        //在SELECT语句中，两边加个()，生成的SQL更优雅，在外层会添加
        if (this.isSelect) sqlSegment.IsNeedParentheses = true;
        return sqlSegment.Change($"{sqlSegment} {strOperator} {this.GetQuotedValue(deferredSegment)}", false, true);
    }
    public virtual SqlSegment Visit(SqlSegment sqlSegment)
    {
        SqlSegment result = null;
        if (sqlSegment.Expression == null)
            throw new ArgumentNullException("sqlSegment.Expression");

        switch (sqlSegment.Expression.NodeType)
        {
            case ExpressionType.Lambda:
                var lambdaExpr = sqlSegment.Expression as LambdaExpression;
                result = this.Visit(sqlSegment.Next(lambdaExpr.Body));
                break;
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
            default: throw new ArgumentNullException($"不支持的表达式操作，{sqlSegment.Expression}");
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
                    //SELECT/WHERE语句，都会有Defer处理，在最外层再计算bool值
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                    return this.Visit(sqlSegment.Next(unaryExpr.Operand));
                }
                return sqlSegment.ChangeValue($"~{this.Visit(sqlSegment)}");
            case ExpressionType.Convert:
                if (unaryExpr.Method != null)
                {
                    if (unaryExpr.Operand.IsParameter(out _))
                        return this.Visit(sqlSegment.Next(unaryExpr.Operand));
                    return this.Evaluate(sqlSegment);
                }
                //string.Concat,string.Format,string.Join等方法，参数都是object，
                //最终变成string，字段访问、表达式需要强制转换,如：a.IntField + 5, b.Field等，常量不需要强转
                sqlSegment = this.Visit(sqlSegment.Next(unaryExpr.Operand));
                if (sqlSegment.TargetType != null && sqlSegment.Type != sqlSegment.TargetType && sqlSegment.HasField && !sqlSegment.IsExpression)
                {
                    sqlSegment.TableSegment.Mapper ??= this.mapProvider.GetEntityMap(sqlSegment.TableSegment.EntityType);
                    var memberMapper = sqlSegment.TableSegment.Mapper.GetMemberMap(sqlSegment.FromMember.Name);
                    if (this.ormProvider.MapDefaultType(memberMapper.NativeDbType) != sqlSegment.TargetType)
                        sqlSegment.Value = this.ormProvider.CastTo(sqlSegment.TargetType, this.GetQuotedValue(sqlSegment));
                    if (sqlSegment.Type != sqlSegment.TargetType)
                        sqlSegment.Type = sqlSegment.TargetType;
                }
                return sqlSegment;
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
                if (this.IsStringConcatOperator(sqlSegment, out var operatorSegment))
                    return operatorSegment;
                if (this.IsDateTimeOperator(sqlSegment, out operatorSegment))
                    return operatorSegment;
                if (this.IsTimeSpanOperator(sqlSegment, out operatorSegment))
                    return operatorSegment;

                var leftSegment = this.Visit(sqlSegment.Next(binaryExpr.Left));
                var rightSegment = this.Visit(new SqlSegment
                {
                    Expression = binaryExpr.Right,
                    ExpectType = leftSegment.ExpectType,
                    TargetType = leftSegment.TargetType
                });

                //计算数组访问，a??bb
                if (leftSegment.IsConstantValue && rightSegment.IsConstantValue)
                    return this.Evaluate(sqlSegment.Next(binaryExpr));

                if (!leftSegment.HasField && rightSegment.HasField)
                {
                    this.Swap(ref leftSegment, ref rightSegment);
                    if (leftSegment.ExpectType != null && leftSegment.ExpectType.IsEnum && leftSegment.TargetType != null)
                    {
                        rightSegment = this.Visit(new SqlSegment
                        {
                            Expression = binaryExpr.Right,
                            ExpectType = leftSegment.ExpectType,
                            TargetType = leftSegment.TargetType
                        });
                    }
                }

                //bool类型的表达式，这里不做解析，到where、having、joinOn子句中去解析并展开合并
                if (binaryExpr.NodeType == ExpressionType.Equal || binaryExpr.NodeType == ExpressionType.NotEqual)
                {
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
                //if (binaryExpr.NodeType == ExpressionType.ArrayIndex)
                //    throw new NotSupportedException("不支持的数组访问，只支持常量访问");

                leftSegment.Merge(rightSegment);
                if (leftSegment.Type != rightSegment.Type)
                    leftSegment.Type = rightSegment.Type;
                var operators = this.ormProvider.GetBinaryOperator(binaryExpr.NodeType);
                if (binaryExpr.NodeType == ExpressionType.Coalesce)
                    return leftSegment.Change($"{operators}({this.GetQuotedValue(leftSegment)},{this.GetQuotedValue(rightSegment)})", false, true);
                return leftSegment.Change($"{this.GetQuotedValue(leftSegment)}{operators}{this.GetQuotedValue(rightSegment)}", false, true);
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

        sqlSegment.Value = constantExpr.Value;
        sqlSegment.IsConstantValue = true;
        //.NET 枚举类型有时候会解析错误，解析成对应的数值类型，如：a.Gender ?? Gender.Male == Gender.Male
        //如果枚举类型对应的数据库类型是字符串，就会有问题，需要把数字变为枚举，再把枚举的名字入库。
        return this.ConvertTo(sqlSegment);
    }
    public virtual SqlSegment VisitMethodCall(SqlSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        if (methodCallExpr.Method.DeclaringType == typeof(Sql)
            || typeof(IAggregateSelect).IsAssignableFrom(methodCallExpr.Method.DeclaringType))
            return this.VisitSqlMethodCall(sqlSegment);

        if (!this.ormProvider.TryGetMethodCallSqlFormatter(methodCallExpr, out var formatter))
            throw new NotSupportedException($"不支持的方法访问，或是{this.ormProvider.GetType().FullName}未实现此方法{methodCallExpr.Method.Name}");

        int newStartIndex = 1;
        SqlSegment target = null;
        if (methodCallExpr.Object != null)
        {
            target = sqlSegment.Next(methodCallExpr.Object);
            newStartIndex = 0;
        }

        SqlSegment[] arguments = null;
        if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count > 0)
        {
            var argumentSegments = new List<SqlSegment>();
            //如果target为null，第一个参数，直接使用现有对象sqlSegment
            if (newStartIndex > 0) argumentSegments.Add(sqlSegment.Next(methodCallExpr.Arguments[0]));
            for (int i = newStartIndex; i < methodCallExpr.Arguments.Count; i++)
            {
                argumentSegments.Add(new SqlSegment { Expression = methodCallExpr.Arguments[i] });
            }
            arguments = argumentSegments.ToArray();
        }
        return formatter.Invoke(this, target, sqlSegment.DeferredExprs, arguments);
    }
    public virtual SqlSegment VisitParameter(SqlSegment sqlSegment)
    {
        var parameterExpr = sqlSegment.Expression as ParameterExpression;

        if (this.isFromQuery)
            throw new NotSupportedException($"FROM子查询中不支持实体类型成员MemberAccess表达式访问，只支持基础字段访问访问,{parameterExpr}");

        var fromSegment = this.tableAlias[parameterExpr.Name];
        //参数访问的是模型，通常是成员访问或是SELECT语句的实体访问，成员访问已经处理了参数访问，不会走到此方法
        //参数访问通常都是SELECT语句的实体访问
        if (!this.isSelect) throw new NotSupportedException($"不支持的参数表达式访问，{parameterExpr}");

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
        bool isConstantValue = true;
        foreach (var elementExpr in newArrayExpr.Expressions)
        {
            var elementSegment = new SqlSegment { Expression = elementExpr };
            elementSegment = this.VisitAndDeferred(elementSegment);
            if (!elementSegment.IsConstantValue)
                isConstantValue = false;
            sqlSegment.Merge(elementSegment);
            result.Add(elementSegment);
        }
        return sqlSegment.Change(result, isConstantValue);
    }
    public virtual SqlSegment VisitIndexExpression(SqlSegment sqlSegment)
    {
        if (sqlSegment.Expression.IsParameter(out _))
            throw new NotSupportedException("索引表达式不支持Parameter访问操作");
        return this.Evaluate(sqlSegment);
    }
    public virtual SqlSegment VisitConditional(SqlSegment sqlSegment)
    {
        var conditionalExpr = sqlSegment.Expression as ConditionalExpression;
        sqlSegment = this.VisitAndDeferred(sqlSegment.Next(conditionalExpr.Test));
        var ifTrueSegment = this.Visit(new SqlSegment { Expression = conditionalExpr.IfTrue });
        var ifFalseSegment = this.Visit(new SqlSegment { Expression = conditionalExpr.IfFalse });
        sqlSegment.Merge(ifTrueSegment);
        sqlSegment.Merge(ifFalseSegment);
        return sqlSegment.Change($"(CASE WHEN {sqlSegment} THEN {this.GetQuotedValue(ifTrueSegment)} ELSE {this.GetQuotedValue(ifFalseSegment)} END)", false, true);
    }
    public virtual SqlSegment VisitListInit(SqlSegment sqlSegment)
    {
        sqlSegment.IsArray = true;
        var listExpr = sqlSegment.Expression as ListInitExpression;
        var result = new List<SqlSegment>();
        bool isConstantValue = true;
        foreach (var elementInit in listExpr.Initializers)
        {
            if (elementInit.Arguments.Count == 0)
                continue;
            var elementSegment = new SqlSegment { Expression = elementInit.Arguments[0] };
            elementSegment = this.VisitAndDeferred(elementSegment);
            if (!elementSegment.IsConstantValue)
                isConstantValue = false;
            result.Add(elementSegment);
        }
        return sqlSegment.Change(result, isConstantValue);
    }
    public virtual SqlSegment VisitTypeIs(SqlSegment sqlSegment)
    {
        var binaryExpr = sqlSegment.Expression as TypeBinaryExpression;
        if (!binaryExpr.Expression.IsParameter(out _))
            return this.Evaluate(sqlSegment);
        if (binaryExpr.TypeOperand == typeof(DBNull))
        {
            sqlSegment.Push(new DeferredExpr
            {
                OperationType = OperationType.Equal,
                Value = SqlSegment.Null
            });
            return this.Visit(sqlSegment.Next(binaryExpr.Expression));
        }
        throw new NotSupportedException($"不支持的表达式操作，{sqlSegment.Expression}");
    }
    public virtual List<SqlSegment> VisitLogicBinaryExpr(Expression conditionExpr)
    {
        Func<Expression, bool> isConditionExpr = f => f.NodeType == ExpressionType.AndAlso || f.NodeType == ExpressionType.OrElse;

        int deep = 0;
        var lastOperationType = OperationType.None;
        var deferredExprs = new Stack<Expression>();
        var completedSegements = new List<SqlSegment>();
        var binaryExpr = conditionExpr as BinaryExpression;

        while (binaryExpr != null)
        {
            if (isConditionExpr(binaryExpr.Left))
            {
                deferredExprs.Push(binaryExpr.Right);
                binaryExpr = binaryExpr.Left as BinaryExpression;
                continue;
            }
            var operationType = binaryExpr.NodeType == ExpressionType.AndAlso ? OperationType.And : OperationType.Or;
            if (lastOperationType == OperationType.None)
                lastOperationType = operationType;
            if (operationType != lastOperationType)
            {
                lastOperationType = operationType;
                deep++;
            }
            var leftSegment = this.CreateConditionSegment(binaryExpr.Left);
            leftSegment.OperationType = operationType;
            leftSegment.Deep = deep;
            completedSegements.Add(leftSegment);

            if (isConditionExpr(binaryExpr.Right))
            {
                binaryExpr = binaryExpr.Right as BinaryExpression;
                continue;
            }
            var rightSegment = this.CreateConditionSegment(binaryExpr.Right);
            rightSegment.OperationType = operationType;
            rightSegment.Deep = deep;
            completedSegements.Add(rightSegment);

            bool isCompleted = true;
            while (deferredExprs.TryPop(out var deferredExpr))
            {
                if (isConditionExpr(deferredExpr))
                {
                    binaryExpr = deferredExpr as BinaryExpression;
                    isCompleted = false;
                    break;
                }
                var deferredSegment = this.CreateConditionSegment(deferredExpr);
                deferredSegment.OperationType = operationType;
                deferredSegment.Deep = deep;
                completedSegements.Add(deferredSegment);
            }
            if (isCompleted) break;
        }
        return completedSegements;
    }
    public virtual SqlSegment Evaluate(SqlSegment sqlSegment)
    {
        var lambdaExpr = Expression.Lambda(sqlSegment.Expression);
        var objValue = lambdaExpr.Compile().DynamicInvoke();
        if (objValue == null)
            return SqlSegment.Null;

        return sqlSegment.Change(objValue);
    }
    public virtual T Evaluate<T>(Expression expr)
    {
        var lambdaExpr = Expression.Lambda(expr);
        var objValue = lambdaExpr.Compile().DynamicInvoke();
        if (objValue == null)
            return default;
        return (T)objValue;
    }
    public virtual SqlSegment VisitSqlMethodCall(SqlSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        LambdaExpression lambdaExpr = null;
        List<ParameterExpression> visitedParameters = null;
        switch (methodCallExpr.Method.Name)
        {
            case "FlattenTo"://通常在最外层的SELECT中转为其他类型
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count >= 1)
                {
                    Expression flattenExpr = null;
                    var targetType = methodCallExpr.Method.ReturnType;
                    var sourceType = methodCallExpr.Arguments[0].Type;
                    if (methodCallExpr.Arguments.Count > 1)
                    {
                        lambdaExpr = this.EnsureLambda(methodCallExpr.Arguments[1]);
                        flattenExpr = lambdaExpr;
                        if (lambdaExpr.Body.GetParameters(out visitedParameters))
                            flattenExpr = Expression.Lambda(lambdaExpr.Body, visitedParameters);
                    }
                    var readerFields = this.FlattenFieldsTo(targetType, flattenExpr);
                    sqlSegment.ChangeValue(readerFields);
                }
                break;
            case "ToParameter":
                sqlSegment.IsParameterized = true;
                sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                sqlSegment.IsParameterized = false;
                break;
            case "In":
                var elementType = methodCallExpr.Method.GetGenericArguments()[0];
                var type = methodCallExpr.Arguments[1].Type;
                if (type.IsArray || typeof(IEnumerable<>).MakeGenericType(elementType).IsAssignableFrom(type))
                {
                    sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[1]));
                    if (sqlSegment == SqlSegment.Null)
                        return sqlSegment.Change("1=0", false, true);
                    var sqlSegments = sqlSegment.Value as List<SqlSegment>;
                    sqlSegments.ForEach(f => f.Value = this.ormProvider.GetQuotedValue(f));
                    sqlSegment.ChangeValue(string.Join(',', sqlSegments));
                }
                else
                {
                    lambdaExpr = methodCallExpr.Arguments[1] as LambdaExpression;
                    var sql = this.VisitFromQuery(lambdaExpr, out var isNeedAlias);
                    sqlSegment.Change(sql, false);
                    if (isNeedAlias) this.isNeedAlias = true;
                }
                var fieldSegment = this.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                sqlSegment.Change($"{fieldSegment} IN ({sqlSegment})", false, true);
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
                sqlSegment.Change($"EXISTS({existsSql})", false, true);
                break;
            case "Count":
            case "LongCount":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");

                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"COUNT({sqlSegment})", false, true);
                }
                else sqlSegment.Change("COUNT(1)", false, true);
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
                    sqlSegment.Change($"COUNT(DISTINCT {sqlSegment})", false, true);
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
                    sqlSegment.Change($"SUM({sqlSegment})", false, true);
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
                    sqlSegment.Change($"AVG({sqlSegment})", false, true);
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
                    sqlSegment.Change($"MAX({sqlSegment})", false, true);
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
                    sqlSegment.Change($"MIN({sqlSegment})", false, true);
                }
                if (this.isSelect || this.isWhere)
                    this.tables.FindAll(f => f.IsMaster).ForEach(f => f.IsUsed = true);
                break;
        }
        return sqlSegment;
    }
    public virtual bool IsStringConcatOperator(SqlSegment sqlSegment, out SqlSegment result)
    {
        var binaryExpr = sqlSegment.Expression as BinaryExpression;
        if (binaryExpr.NodeType == ExpressionType.Add && (binaryExpr.Left.Type == typeof(string) || binaryExpr.Right.Type == typeof(string)))
        {
            //先打开所有要拼接的部分，最后再拼接
            var concatSegments = this.SplitConcatList(sqlSegment.Expression);

            //调用拼接方法Concat,每个数据库Provider都实现了这个方法
            var methodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(object[]) });
            var parameters = Expression.NewArrayInit(typeof(object), concatSegments.Select(f => f.Expression));
            var methodCallExpr = Expression.Call(methodInfo, parameters);
            sqlSegment.Expression = methodCallExpr;
            this.ormProvider.TryGetMethodCallSqlFormatter(methodCallExpr, out var formater);
            //返回的SQL表达式中直接拼接好
            result = formater.Invoke(this, null, null, concatSegments);
            return true;
        }
        result = null;
        return false;
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
        return this.VisitAndDeferred(this.CreateConditionSegment(conditionExpr)).ToString();
    }
    public virtual List<SqlSegment> ConvertFormatToConcatList(SqlSegment[] argsSegments)
    {
        var format = this.Evaluate<string>(argsSegments[0].Expression);
        int index = 1, formatIndex = 0;
        var parameters = new List<SqlSegment>();
        for (int i = 1; i < argsSegments.Length; i++)
        {
            switch (argsSegments[i].Expression.NodeType)
            {
                case ExpressionType.ListInit:
                    var listExpr = argsSegments[i].Expression as ListInitExpression;
                    foreach (var elementInit in listExpr.Initializers)
                    {
                        if (elementInit.Arguments.Count == 0)
                            continue;
                        parameters.Add(new SqlSegment { Expression = elementInit.Arguments[0] });
                    }
                    break;
                case ExpressionType.NewArrayBounds:
                case ExpressionType.NewArrayInit:
                    var newArrayExpr = argsSegments[i].Expression as NewArrayExpression;
                    foreach (var elementExpr in newArrayExpr.Expressions)
                    {
                        parameters.Add(new SqlSegment { Expression = elementExpr });
                    }
                    break;
                default: parameters.Add(argsSegments[i]); break;
            }
        }
        index = 0;
        var result = new List<SqlSegment>();
        while (formatIndex < format.Length)
        {
            var nextIndex = format.IndexOf('{', formatIndex);
            if (nextIndex > formatIndex)
            {
                var constValue = format.Substring(formatIndex, nextIndex - formatIndex);
                var constExpr = Expression.Constant(constValue);
                result.Add(new SqlSegment { Expression = constExpr, Value = constValue, IsConstantValue = true });
            }
            result.AddRange(this.SplitConcatList(parameters[index].Expression));
            index++;
            formatIndex = format.IndexOf('}', nextIndex + 2) + 1;
        }
        return result;
    }
    public virtual List<SqlSegment> SplitConcatList(SqlSegment[] argsSegments)
    {
        var completedExprs = new List<SqlSegment>();
        var deferredExprs = new Stack<SqlSegment>();
        Func<Expression, bool> isConcatBinary = f =>
        {
            if (f is BinaryExpression binaryExpr && binaryExpr.NodeType == ExpressionType.Add && binaryExpr.Type == typeof(string)
                && (binaryExpr.Left.Type == typeof(string) || binaryExpr.Right.Type == typeof(string)))
                return true;
            if (f is MethodCallExpression callExpr && callExpr.Method.Name == "Concat")
                return true;
            return false;
        };
        SqlSegment nextSegment = null;
        for (int i = argsSegments.Length - 1; i > 0; i--)
        {
            deferredExprs.Push(argsSegments[i]);
        }
        nextSegment = argsSegments[0];
        while (true)
        {
            if (isConcatBinary(nextSegment.Expression))
            {
                //字符串连接+
                if (nextSegment.Expression is BinaryExpression binaryExpr)
                {
                    if (isConcatBinary(binaryExpr.Left))
                    {
                        deferredExprs.Push(nextSegment.Next(binaryExpr.Right));
                        nextSegment = new SqlSegment { Expression = binaryExpr.Left };
                        continue;
                    }
                    completedExprs.Add(nextSegment);
                    if (isConcatBinary(binaryExpr.Right))
                    {
                        nextSegment = new SqlSegment { Expression = binaryExpr.Right };
                        continue;
                    }
                    completedExprs.Add(new SqlSegment { Expression = binaryExpr.Right });
                    if (!deferredExprs.TryPop(out nextSegment))
                        break;
                    continue;
                }
                else
                {
                    var callExpr = nextSegment.Expression as MethodCallExpression;
                    for (int i = callExpr.Arguments.Count - 1; i > 0; i--)
                    {
                        deferredExprs.Push(new SqlSegment { Expression = callExpr.Arguments[i] });
                    }
                    nextSegment.Next(callExpr.Arguments[0]);
                    continue;
                }
            }
            completedExprs.Add(nextSegment);
            if (!deferredExprs.TryPop(out nextSegment))
                break;
        }
        return completedExprs;
    }
    public virtual SqlSegment[] SplitConcatList(Expression concatExpr)
    {
        var completedExprs = new List<SqlSegment>();
        var deferredExprs = new Stack<Expression>();
        Func<Expression, bool> isConcatBinary = f =>
        {
            if (f is BinaryExpression binaryExpr && binaryExpr.NodeType == ExpressionType.Add && binaryExpr.Type == typeof(string)
                && (binaryExpr.Left.Type == typeof(string) || binaryExpr.Right.Type == typeof(string)))
                return true;
            if (f is MethodCallExpression callExpr && callExpr.Method.Name == "Concat")
                return true;
            return false;
        };
        var nextExpr = concatExpr;
        while (true)
        {
            if (isConcatBinary(nextExpr))
            {
                //字符串连接+
                if (nextExpr is BinaryExpression binaryExpr)
                {
                    if (isConcatBinary(binaryExpr.Left))
                    {
                        deferredExprs.Push(binaryExpr.Right);
                        nextExpr = binaryExpr.Left;
                        continue;
                    }
                    completedExprs.Add(new SqlSegment { Expression = binaryExpr.Left, TargetType = typeof(string) });
                    if (isConcatBinary(binaryExpr.Right))
                    {
                        nextExpr = binaryExpr.Right;
                        continue;
                    }
                    completedExprs.Add(new SqlSegment { Expression = binaryExpr.Right, TargetType = typeof(string) });
                    if (!deferredExprs.TryPop(out nextExpr))
                        break;
                    continue;
                }
                else
                {
                    //Concat方法
                    var callExpr = nextExpr as MethodCallExpression;
                    for (int i = callExpr.Arguments.Count - 1; i > 0; i--)
                    {
                        deferredExprs.Push(callExpr.Arguments[i]);
                    }
                    nextExpr = callExpr.Arguments[0];
                    continue;
                }
            }
            completedExprs.Add(new SqlSegment { Expression = nextExpr, TargetType = typeof(string) });
            if (!deferredExprs.TryPop(out nextExpr))
                break;
        }
        return completedExprs.ToArray();
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

        //数组，直接返回@p0,@p1,@p2,@p3或是@Name0,@Name1,@Name2,@Name3
        if (sqlSegment.Value is IEnumerable objValues && objValues is not string)
        {
            var paramPrefix = this.ormProvider.ParameterPrefix + this.parameterPrefix;
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
            var parameterName = this.ormProvider.ParameterPrefix + this.parameterPrefix + this.dbParameters.Count.ToString();
            this.dbParameters.Add(this.ormProvider.CreateParameter(parameterName, sqlSegment.Value));
            return sqlSegment.Change(parameterName, false);
        }
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
        var queryVisitor = new QueryVisitor(this.dbKey, this.ormProvider, this.mapProvider, this.isParameterized, tableAsStart, parameterPrefix);
        queryVisitor.isNeedAlias = this.isNeedAlias;
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
    public SqlSegment ConvertTo(SqlSegment sqlSegment)
    {
        if (sqlSegment.TargetType != null && sqlSegment.TargetType != sqlSegment.Type)
        {
            if (sqlSegment.ExpectType != null)
            {
                var currentType = sqlSegment.Value.GetType();
                if (sqlSegment.ExpectType != currentType)
                {
                    if (sqlSegment.ExpectType.IsEnumType(out var enumType, out _))
                        sqlSegment.Value = Enum.ToObject(enumType, sqlSegment.Value);
                    else sqlSegment.Value = Convert.ChangeType(sqlSegment.Value, sqlSegment.ExpectType);
                    sqlSegment.Type = sqlSegment.ExpectType;
                }
            }

            if (sqlSegment.TargetType == typeof(string))
                sqlSegment.Value = sqlSegment.Value.ToString();
            else sqlSegment.Value = Convert.ChangeType(sqlSegment.Value, sqlSegment.TargetType);
            sqlSegment.Type = sqlSegment.TargetType;
        }
        return sqlSegment;
    }
    public List<ReaderField> FlattenFieldsTo(Type targetType, Expression toTargetExpr = null)
    {
        List<ReaderField> initReaderFields = null;
        if (targetType == null)
            throw new ArgumentNullException(nameof(targetType));

        //通过表达式设置的字段
        if (toTargetExpr != null)
        {
            var lambdaExpr = toTargetExpr as LambdaExpression;
            initReaderFields = this.ConstructorFieldsTo(lambdaExpr);
        }

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
                    //成员访问，最多是Include子表访问或是临时表访问，如：x.Grouping分组，
                    //可能会有多个子表或是临时表
                    if (sqlSegment.MemberType == ReaderFieldType.Entity
                        || sqlSegment.MemberType == ReaderFieldType.AnonymousObject)
                        result = sqlSegment.Value as List<ReaderField>;
                    else
                    {
                        result.Add(new ReaderField
                        {
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
    /// <summary>
    /// 展开当前表tableSegment的所有字段，tableSegment是实体表(主表或Include表)
    /// repository.From(f => f.From<Order, OrderDetail, User>()
    ///         .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id)
    ///         .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
    ///         .Having((x, a, b, c) => c.Age > 20 && x.Sum(b.Amount) > 500)
    ///         .Select((x, a, b, c) => new { x.Grouping.OrderId, x.Grouping.BuyerId, TotalAmount = x.Sum(b.Amount) }))
    ///     .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
    ///     .InnerJoin<Order>((a, b, c) => a.OrderId == c.Id)
    ///     .Select((a, b, c) => new { a.OrderId, a.BuyerId, Buyer = b, Order = c, a.TotalAmount })
    ///     .ToSql(out _);
    /// </summary>
    /// <param name="tableSegment"></param>
    /// <returns></returns>
    public List<ReaderField> FlattenTableFields(TableSegment tableSegment)
    {
        var targetFields = new List<ReaderField>();
        tableSegment.Mapper ??= this.mapProvider.GetEntityMap(tableSegment.EntityType);
        foreach (var memberMapper in tableSegment.Mapper.MemberMaps)
        {
            if (memberMapper.IsIgnore || memberMapper.IsNavigation
                || (memberMapper.MemberType.IsEntityType() && memberMapper.TypeHandler == null))
                continue;
            targetFields.Add(new ReaderField
            {
                FromMember = memberMapper.Member,
                TableSegment = tableSegment,
                FieldType = ReaderFieldType.Field,
                //加表别名
                Body = this.GetFieldName(tableSegment, memberMapper.FieldName)
            });
        }
        return targetFields;
    }
    /// <summary>
    /// 根据表、字段获取Field表达式,SELECT和WHERE语句都可以使用
    /// 有表且有别名，将生成a.Field，常量访问没有表则就是字段访问Field
    /// </summary>
    /// <param name="tableSegment">表</param> 
    /// <param name="fieldName">字段名称或是表达式</param>
    /// <returns></returns>
    public string GetFieldName(TableSegment tableSegment, string fieldName)
    {
        fieldName = this.ormProvider.GetFieldName(fieldName);
        if (tableSegment != null && !string.IsNullOrEmpty(tableSegment.AliasName) && (this.isNeedAlias || tableSegment.IsNeedAlais))
            fieldName = tableSegment.AliasName + "." + fieldName;
        return fieldName;
    }
    private string GetQuotedValue(SqlSegment sqlSegment)
        => this.ormProvider.GetQuotedValue(sqlSegment);
    private bool IsDateTimeOperator(SqlSegment sqlSegment, out SqlSegment result)
    {
        var binaryExpr = sqlSegment.Expression as BinaryExpression;
        if (binaryExpr.Left.Type == typeof(DateTime) && binaryExpr.Right.Type == typeof(TimeSpan) && binaryExpr.NodeType == ExpressionType.Add)
        {
            var methodInfo = typeof(DateTime).GetMethod(nameof(DateTime.Add), new Type[] { binaryExpr.Right.Type });
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, binaryExpr.Right);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
        if (binaryExpr.Left.Type == typeof(DateTime) && binaryExpr.Right.Type == typeof(TimeSpan) && binaryExpr.NodeType == ExpressionType.Subtract)
        {
            var methodInfo = typeof(DateTime).GetMethod(nameof(DateTime.Subtract), new Type[] { binaryExpr.Right.Type });
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, binaryExpr.Right);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
        result = null;
        return false;
    }
    private bool IsTimeSpanOperator(SqlSegment sqlSegment, out SqlSegment result)
    {
        var binaryExpr = sqlSegment.Expression as BinaryExpression;
        if (binaryExpr.Left.Type == typeof(TimeSpan) && binaryExpr.Right.Type == typeof(TimeSpan) && binaryExpr.NodeType == ExpressionType.Add)
        {
            var methodInfo = typeof(TimeSpan).GetMethod(nameof(TimeSpan.Add), new Type[] { binaryExpr.Right.Type });
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, binaryExpr.Right);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
        if (binaryExpr.Left.Type == typeof(TimeSpan) && binaryExpr.Right.Type == typeof(TimeSpan) && binaryExpr.NodeType == ExpressionType.Subtract)
        {
            var methodInfo = typeof(TimeSpan).GetMethod(nameof(TimeSpan.Subtract), new Type[] { binaryExpr.Right.Type });
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, binaryExpr.Right);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
        if (binaryExpr.Left.Type == typeof(TimeSpan) && binaryExpr.NodeType == ExpressionType.Multiply)
        {
            var rightExpr = binaryExpr.Right;
            if (binaryExpr.Right.Type != typeof(double))
                rightExpr = Expression.Convert(rightExpr, typeof(double));
            var methodInfo = typeof(TimeSpan).GetMethod(nameof(TimeSpan.Multiply), new Type[] { typeof(double) });
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, rightExpr);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
        if (binaryExpr.Left.Type == typeof(TimeSpan) && binaryExpr.NodeType == ExpressionType.Divide)
        {
            Type divideType = null;
            if (binaryExpr.Right.Type == typeof(TimeSpan))
                divideType = typeof(TimeSpan);
            else divideType = typeof(double);
            var methodInfo = typeof(TimeSpan).GetMethod(nameof(TimeSpan.Divide), new Type[] { divideType });
            var rightExpr = binaryExpr.Right;
            if (divideType == typeof(double) && binaryExpr.Right.Type != typeof(double))
                rightExpr = Expression.Convert(rightExpr, typeof(double));
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, rightExpr);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
        result = null;
        return false;
    }
    private SqlSegment CreateConditionSegment(Expression conditionExpr)
    {
        var sqlSegment = new SqlSegment { Expression = conditionExpr };
        if (conditionExpr.NodeType == ExpressionType.MemberAccess && conditionExpr.Type == typeof(bool))
        {
            sqlSegment.DeferredExprs ??= new();
            sqlSegment.DeferredExprs.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlSegment.True });
        }
        return sqlSegment;
    }
    private void Swap<T>(ref T left, ref T right)
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
