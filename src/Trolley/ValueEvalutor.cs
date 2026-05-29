using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

public static class ValueEvalutor
{
    private static readonly ConcurrentDictionary<int, Func<object, object, object>> binaryFuncCache = new();

    public static T Evaluate<T>(this Expression expression)
    {
        var objValue = Evaluate(expression);
        if (objValue == null) return default;
        return (T)objValue;
    }
    public static object Evaluate(this Expression expression, object target = null)
    {
        return expression switch
        {
            BinaryExpression binaryExpression => Evaluate(binaryExpression),
            ConstantExpression constantExpression => constantExpression.Value,
            UnaryExpression unaryExpression => Evaluate(unaryExpression),
            MethodCallExpression methodCallExpression => Evaluate(methodCallExpression, target),
            MemberExpression memberExpression => Evaluate(memberExpression),
            IndexExpression indexExpression => Evaluate(indexExpression),
            NewArrayExpression newArrayExpression => Evaluate(newArrayExpression),
            ListInitExpression listInitExpression => Evaluate(listInitExpression),
            NewExpression newExpression => Evaluate(newExpression),
            MemberInitExpression memberInitExpression => Evaluate(memberInitExpression),
            ConditionalExpression conditionalExpression => Evaluate(conditionalExpression),
            ParameterExpression parameterExpression => Evaluate(parameterExpression, target),
            DefaultExpression defaultExpression => Evaluate(defaultExpression),
            _ => Expression.Lambda(expression).Compile().DynamicInvoke()
        };
    }
    public static object Evaluate(BinaryExpression expression)
    {
        var left = Evaluate(expression.Left);
        var right = Evaluate(expression.Right);
        return Evaluate(expression, left, right);
    }
    public static object Evaluate(BinaryExpression expression, object left, object right)
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
    public static object Evaluate(UnaryExpression expression)
    {
        var operand = Evaluate(expression.Operand);
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
    public static object Evaluate(MethodCallExpression expression, object target)
    {
        var myTarget = target;
        if (expression.Object != null)
            myTarget = Evaluate(expression.Object);
        var parameters = expression.Arguments.Select(arg => Evaluate(arg)).ToArray();
        return expression.Method.Invoke(myTarget, parameters);
    }
    public static object Evaluate(MemberExpression expression)
        => Evaluate(expression.Member, Evaluate(expression.Expression));
    public static object Evaluate(IndexExpression expression)
    {
        var target = Evaluate(expression.Object);
        var arguments = expression.Arguments.Select(arg => Evaluate(arg)).ToArray();
        return Evaluate(expression.Indexer, target, arguments);
    }
    public static object Evaluate(NewArrayExpression expression)
    {
        var arrayType = expression.Type.HasElementType ? expression.Type.GetElementType() : expression.Type;
        var array = Array.CreateInstance(arrayType, expression.Expressions.Count);
        for (var i = 0; i < expression.Expressions.Count; i++)
            array.SetValue(Evaluate(expression.Expressions[i]), i);
        return array;
    }
    public static object Evaluate(ListInitExpression expression)
    {
        var list = RepositoryHelper.CreateInstance(expression.Type);
        foreach (var item in expression.Initializers)
            item.AddMethod.Invoke(list, [Evaluate(item.Arguments.FirstOrDefault())]);
        return list;
    }
    public static object Evaluate(NewExpression expression)
    {
        if (expression.Arguments.Count > 0)
            return RepositoryHelper.CreateInstance(expression.Type, expression.Arguments.Select(f => f.Type).ToArray(),
                expression.Arguments.Select(arg => Evaluate(arg)).ToArray());
        else return RepositoryHelper.CreateInstance(expression.Type);
    }
    public static object Evaluate(MemberInitExpression expression)
    {
        var instance = Evaluate(expression.NewExpression);
        foreach (var binding in expression.Bindings)
            SetValue(binding.Member, instance, Evaluate(binding));
        return instance;
    }
    public static object Evaluate(ConditionalExpression expression)
    {
        var test = (bool)Evaluate(expression.Test);
        var trueValue = Evaluate(expression.IfTrue);
        var falseValue = Evaluate(expression.IfFalse);
        return test ? trueValue : falseValue;
    }
    public static object Evaluate(ParameterExpression expression, object target)
    {
        if (expression.Type.GetConstructors().Any(e => e.GetParameters().Length == 0))
            return RepositoryHelper.CreateInstance(expression.Type);
        return target;
    }
    public static object Evaluate(DefaultExpression expression) => expression.Type.IsValueType ? Activator.CreateInstance(expression.Type) : null;
    public static object Evaluate(MemberInfo member, object obj, object[] parameters = null, bool isCache = true)
    {
        return member switch
        {
            FieldInfo fieldInfo => EvaluateAndCache(obj, fieldInfo),
            PropertyInfo propertyInfo => EvaluateAndCache(obj, propertyInfo, parameters),
            MethodInfo methodInfo => methodInfo.Invoke(obj, parameters),
            _ => throw new NotSupportedException($"不支持的成员访问，只支持字段、属性、方法访问，obj:{obj}")
        };
    }
    public static object Evaluate(MemberBinding member)
    {
        if (member is MemberAssignment memberAssignment)
            return Evaluate(memberAssignment.Expression);
        return null;
    }
    public static void SetValue(MemberInfo member, object obj, object value)
    {
        if (member is not FieldInfo && member is not PropertyInfo)
            throw new NotSupportedException($"不支持的成员访问，只支持字段、属性访问，obj:{obj}");
        SetValueAndCache(obj, member, value);
    }
    public static object EvaluateAndCache(object entity, MemberInfo memberInfo, object[] parameters = null)
    {
        var memberGetter = RepositoryHelper.GetMemberValueGetter(memberInfo);
        return memberGetter.Invoke(entity, parameters);
    }
    public static void SetValueAndCache(object entity, MemberInfo memberInfo, object value)
    {
        var memberSetter = RepositoryHelper.GetMemberValueSetter(memberInfo);
        memberSetter.Invoke(entity, value);
    }
    public static int Compare(object left, object right)
    {
        if (left is IComparable comparable) return comparable.CompareTo(right);
        return Comparer.Default.Compare(left, right);
    }
    public static object Negate(object operand)
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
    public static object Not(object operand)
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