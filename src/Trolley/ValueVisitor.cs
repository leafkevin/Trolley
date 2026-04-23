using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

public class ValueVisitor
{
    private readonly ConcurrentDictionary<int, Func<object, object, object>> binaryFuncCache = new();
    public SqlType SqlType { get; private set; }

    public static SqlSegment Evaluate(Expression expression, object target = null)
    {
        var visitor = new ValueVisitor();
        var objValue = visitor.Visit(expression, target);
        if (objValue == null) return SqlSegment.Null;
        return new SqlSegment { SqlType = visitor.SqlType, Value = objValue };
    }
    public static SqlSegment Evaluate(Expression expression, SqlType sqlType)
    {
        var visitor = new ValueVisitor();
        var objValue = visitor.Visit(expression);
        if (objValue == null) return SqlSegment.Null;
        return new SqlSegment { SqlType = sqlType, Value = objValue };
    }
    public static T Evaluate<T>(Expression expression)
    {
        var visitor = new ValueVisitor();
        var objValue = visitor.Visit(expression);
        if (objValue == null) return default;
        return (T)objValue;
    }
    public static object EvaluateValue(Expression expression)
    {
        var visitor = new ValueVisitor();
        return visitor.Visit(expression);
    }
    public object Visit(Expression expression, object target = null)
    {
        return expression switch
        {
            BinaryExpression binaryExpression => this.Visit(binaryExpression),
            ConstantExpression constantExpression => constantExpression.Value,
            UnaryExpression unaryExpression => this.Visit(unaryExpression),
            MethodCallExpression methodCallExpression => this.Visit(methodCallExpression, target),
            MemberExpression memberExpression => this.Visit(memberExpression),
            IndexExpression indexExpression => this.Visit(indexExpression),
            NewArrayExpression newArrayExpression => this.Visit(newArrayExpression),
            ListInitExpression listInitExpression => this.Visit(listInitExpression),
            NewExpression newExpression => this.Visit(newExpression),
            MemberInitExpression memberInitExpression => this.Visit(memberInitExpression),
            ConditionalExpression conditionalExpression => this.Visit(conditionalExpression),
            ParameterExpression parameterExpression => this.Visit(parameterExpression, target),
            DefaultExpression defaultExpression => this.Visit(defaultExpression),
            _ => Expression.Lambda(expression).Compile().DynamicInvoke()
        };
    }
    public object Visit(BinaryExpression expression)
    {
        var left = this.Visit(expression.Left);
        var right = this.Visit(expression.Right);
        return this.Visit(expression, left, right);
    }
    public object Visit(BinaryExpression expression, object left, object right)
    {
        switch (expression.NodeType)
        {
            case ExpressionType.AndAlso: return (bool)left && (bool)right;
            case ExpressionType.OrElse: return (bool)left || (bool)right;
            case ExpressionType.Equal: return object.Equals(left, right);
            case ExpressionType.NotEqual: return !object.Equals(left, right);
            case ExpressionType.LessThan: return left != null && right != null && Compare(left, right) < 0;
            case ExpressionType.LessThanOrEqual: return left != null && right != null && Compare(left, right) <= 0;
            case ExpressionType.GreaterThan: return left != null && right != null && Compare(left, right) > 0;
            case ExpressionType.GreaterThanOrEqual: return left != null && right != null && Compare(left, right) >= 0;
            case ExpressionType.Coalesce: return left ?? right;
            case ExpressionType.ArrayIndex: return ((Array)left).GetValue(Convert.ToInt32(right));
            default:
                var hashKey = HashCode.Combine(expression.Left.Type, expression.Right.Type, expression.NodeType);
                var func = binaryFuncCache.GetOrAdd(hashKey, k =>
                {
                    var pLeft = Expression.Parameter(typeof(object), "left");
                    var pRight = Expression.Parameter(typeof(object), "right");
                    var leftExpr = Expression.Convert(pLeft, expression.Left.Type);
                    var rightExpr = Expression.Convert(pRight, expression.Right.Type);

                    Expression bodyExpr = null;
                    if (expression.Left.Type == typeof(string) || expression.Right.Type == typeof(string))
                    {
                        var methodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)]);
                        bodyExpr = Expression.Call(methodInfo, leftExpr, rightExpr);
                    }
                    else bodyExpr = Expression.MakeBinary(expression.NodeType, leftExpr, rightExpr);
                    bodyExpr = Expression.Convert(bodyExpr, typeof(object));
                    return Expression.Lambda<Func<object, object, object>>(bodyExpr, pLeft, pRight).Compile();
                });
                return func(left, right);
        }
    }
    public object Visit(UnaryExpression expression)
    {
        var operand = this.Visit(expression.Operand);
        return expression.NodeType switch
        {
            ExpressionType.Not => Not(operand),
            ExpressionType.Convert => operand != null ? Convert.ChangeType(operand, expression.Type) : null,
            ExpressionType.ConvertChecked => operand != null ? Convert.ChangeType(operand, expression.Type) : null,
            ExpressionType.Negate => Negate(operand),
            ExpressionType.NegateChecked => Negate(operand),
            ExpressionType.ArrayLength => ((Array)operand).Length,
            ExpressionType.TypeAs => operand != null && expression.Type.IsInstanceOfType(operand) ? operand : null,
            _ => Expression.Lambda(expression).Compile().DynamicInvoke()
        };
    }
    public object Visit(MethodCallExpression expression, object target)
    {
        var myTarget = target;
        if (expression.Object != null)
            myTarget = this.Visit(expression.Object);
        var parameters = expression.Arguments.Select(arg => this.Visit(arg)).ToArray();
        return expression.Method.Invoke(myTarget, parameters);
    }
    public object Visit(MemberExpression expression)
    {
        this.SqlType = SqlType.Variable;
        return this.Visit(expression.Member, this.Visit(expression.Expression));
    }
    public object Visit(IndexExpression expression)
    {
        var target = this.Visit(expression.Object);
        var arguments = expression.Arguments.Select(arg => this.Visit(arg)).ToArray();
        return this.Visit(expression.Indexer, target, arguments);
    }
    public object Visit(NewArrayExpression expression)
    {
        var arrayType = expression.Type.HasElementType ? expression.Type.GetElementType() : expression.Type;
        var array = Array.CreateInstance(arrayType, expression.Expressions.Count);
        for (var i = 0; i < expression.Expressions.Count; i++)
            array.SetValue(this.Visit(expression.Expressions[i]), i);
        return array;
    }
    public object Visit(ListInitExpression expression)
    {
        var list = RepositoryHelper.CreateInstance(expression.Type);
        foreach (var item in expression.Initializers)
            item.AddMethod.Invoke(list, [this.Visit(item.Arguments.FirstOrDefault())]);
        return list;
    }
    public object Visit(NewExpression expression)
    {
        if (expression.Arguments.Count > 0)
            return RepositoryHelper.CreateInstance(expression.Type, expression.Arguments.Select(f => f.Type).ToArray(),
                expression.Arguments.Select(arg => this.Visit(arg)).ToArray());
        else return RepositoryHelper.CreateInstance(expression.Type);
    }
    public object Visit(MemberInitExpression expression)
    {
        var instance = this.Visit(expression.NewExpression);
        foreach (var binding in expression.Bindings)
            this.SetValue(binding.Member, instance, this.Visit(binding));
        return instance;
    }
    public object Visit(ConditionalExpression expression)
    {
        var test = (bool)this.Visit(expression.Test);
        var trueValue = this.Visit(expression.IfTrue);
        var falseValue = this.Visit(expression.IfFalse);
        return test ? trueValue : falseValue;
    }
    public object Visit(ParameterExpression expression, object target)
    {
        if (expression.Type.GetConstructors().Any(e => e.GetParameters().Length == 0))
            return RepositoryHelper.CreateInstance(expression.Type);
        return target;
    }
    public object Visit(DefaultExpression expression) => expression.Type.IsValueType ? Activator.CreateInstance(expression.Type) : null;
    public object Visit(MemberInfo member, object obj, object[] parameters = null, bool isCache = true)
    {
        return member switch
        {
            FieldInfo fieldInfo => VisitAndCache(obj, fieldInfo),
            PropertyInfo propertyInfo => VisitAndCache(obj, propertyInfo, parameters),
            MethodInfo methodInfo => methodInfo.Invoke(obj, parameters),
            _ => throw new NotSupportedException($"不支持的成员访问，只支持字段、属性、方法访问，obj:{obj}")
        };
    }
    public object Visit(MemberBinding member)
    {
        if (member is MemberAssignment memberAssignment)
            return this.Visit(memberAssignment.Expression);
        return null;
    }
    public void SetValue(MemberInfo member, object obj, object value)
    {
        if (member is not FieldInfo && member is not PropertyInfo)
            throw new NotSupportedException($"不支持的成员访问，只支持字段、属性访问，obj:{obj}");
        SetValueAndCache(obj, member, value);
    }
    public object VisitAndCache(object entity, MemberInfo memberInfo, object[] parameters = null)
    {
        var memberGetter = RepositoryHelper.GetMemberValueGetter(memberInfo);
        return memberGetter.Invoke(entity, parameters);
    }
    public void SetValueAndCache(object entity, MemberInfo memberInfo, object value)
    {
        var memberSetter = RepositoryHelper.GetMemberValueSetter(memberInfo);
        memberSetter.Invoke(entity, value);
    }
    private int Compare(object left, object right)
    {
        if (left is IComparable comparable) return comparable.CompareTo(right);
        return Comparer.Default.Compare(left, right);
    }
    private object Negate(object operand)
    {
        if (operand == null) return null;
        else if (operand is int i) return -i;
        else if (operand is double d) return -d;
        else if (operand is decimal de) return -de;
        else if (operand is long l) return -l;
        else if (operand is byte b) return -b;
        else if (operand is sbyte sb) return -sb;
        else if (operand is short s) return -s;
        else if (operand is ushort us) return -us;
        else if (operand is uint ui) return -ui;
        return -(dynamic)operand;
    }
    public object Not(object operand)
    {
        if (operand == null) return null;
        switch (operand)
        {
            case bool b: return !b;
            case int i: return ~i;
            case uint ui: return ~ui;
            case long l: return ~l;
            case ulong ul: return ~ul;
            case sbyte sb: return ~sb;
            case byte b: return ~b;
            case short s: return ~s;
            case ushort us: return ~us;
            default: return ~((dynamic)operand);
        }
    }
}