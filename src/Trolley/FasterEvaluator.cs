using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

public static class FasterEvaluator
{
    private readonly static ConcurrentDictionary<int, Func<object, object, object>> binaryFuncCache = new();

    public static object Evaluate(this Expression expression, object target = null)
    {
        return expression switch
        {
            BinaryExpression binaryExpression => binaryExpression.Evaluate(),
            ConstantExpression constantExpression => constantExpression.Value,
            UnaryExpression unaryExpression => unaryExpression.Evaluate(),
            MethodCallExpression methodCallExpression => methodCallExpression.Evaluate(target),
            MemberExpression memberExpression => memberExpression.Evaluate(),
            NewArrayExpression newArrayExpression => newArrayExpression.Evaluate(),
            ListInitExpression listInitExpression => listInitExpression.Evaluate(),
            NewExpression newExpression => newExpression.Evaluate(),
            MemberInitExpression memberInitExpression => memberInitExpression.Evaluate(),
            ConditionalExpression conditionalExpression => conditionalExpression.Evaluate(),
            ParameterExpression parameterExpression => parameterExpression.Evaluate(target),
            DefaultExpression defaultExpression => defaultExpression.Evaluate(),
            _ => Expression.Lambda(expression).Compile().DynamicInvoke()
        };
    }
    public static object Evaluate(this BinaryExpression expression)
    {
        var left = expression.Left.Evaluate();
        var right = expression.Right.Evaluate();
        return expression.Evaluate(left, right);
    }
    public static object Evaluate(this BinaryExpression expression, object left, object right)
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
    public static object Evaluate(this UnaryExpression expression)
    {
        var operand = expression.Operand.Evaluate();
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
    public static object Evaluate(this MethodCallExpression expression, object target)
        => expression.Method.Invoke(target ?? expression.Object?.Evaluate(), expression.Arguments.Select(argExpression => argExpression.Evaluate()).ToArray());
    public static object Evaluate(this MemberExpression expression) => expression.Member.Evaluate(expression.Expression?.Evaluate());
    public static object Evaluate(this NewArrayExpression expression)
    {
        var arrayType = expression.Type.HasElementType ? expression.Type.GetElementType() : expression.Type;
        var array = Array.CreateInstance(arrayType, expression.Expressions.Count);
        for (var i = 0; i < expression.Expressions.Count; i++)
        {
            array.SetValue(expression.Expressions[i].Evaluate(), i);
        }
        return array;
    }
    public static object Evaluate(this ListInitExpression expression)
    {
        var list = RepositoryHelper.CreateInstance(expression.Type);
        foreach (var item in expression.Initializers)
        {
            item.AddMethod.Invoke(list, new[] { item.Arguments.FirstOrDefault().Evaluate() });
        }
        return list;
    }
    public static object Evaluate(this NewExpression expression)
    {
        if (expression.Arguments.Count > 0)
            return RepositoryHelper.CreateInstance(expression.Type, expression.Arguments.Select(f => f.Type).ToArray(),
                expression.Arguments.Select(arg => arg.Evaluate()).ToArray());
        else return RepositoryHelper.CreateInstance(expression.Type);
    }
    public static object Evaluate(this MemberInitExpression expression)
    {
        var instance = expression.NewExpression.Evaluate();
        foreach (var binding in expression.Bindings)
        {
            binding.Member.SetValue(instance, binding.Evaluate());
        }
        return instance;
    }
    public static object Evaluate(this ConditionalExpression expression)
    {
        var test = (bool)expression.Test.Evaluate();
        var trueValue = expression.IfTrue.Evaluate();
        var falseValue = expression.IfFalse.Evaluate();
        return test ? trueValue : falseValue;
    }
    public static object Evaluate(this ParameterExpression expression, object target)
    {
        if (expression.Type.GetConstructors().Any(e => e.GetParameters().Length == 0))
            return RepositoryHelper.CreateInstance(expression.Type);
        return target;
    }
    public static object Evaluate(this DefaultExpression expression) => expression.Type.IsValueType ? RepositoryHelper.CreateInstance(expression.Type) : null;
    public static object Evaluate(this MemberInfo member, object obj, object[] parameters = null, bool isCache = true)
    {
        return member switch
        {
            FieldInfo fieldInfo => EvaluateAndCache(obj, fieldInfo),
            PropertyInfo propertyInfo => EvaluateAndCache(obj, propertyInfo),
            MethodInfo methodInfo => methodInfo.Invoke(obj, parameters),
            _ => throw new NotSupportedException($"不支持的成员访问，只支持字段、属性、方法访问，obj:{obj}")
        };
    }
    public static object Evaluate(this MemberBinding member)
    {
        if (member is MemberAssignment memberAssignment)
            return memberAssignment.Expression.Evaluate();
        return null;
    }
    static void SetValue(this MemberInfo member, object obj, object value)
    {
        if (member is not FieldInfo && member is not PropertyInfo)
            throw new NotSupportedException($"不支持的成员访问，只支持字段、属性访问，obj:{obj}");
        SetValueAndCache(obj, member, value);
    }
    public static object EvaluateAndCache(object entity, MemberInfo memberInfo)
    {
        var memberGetter = RepositoryHelper.GetMemberValueGetter(memberInfo);
        return memberGetter.Invoke(entity);
    }
    public static void SetValueAndCache(object entity, MemberInfo memberInfo, object value)
    {
        var memberSetter = RepositoryHelper.GetMemberValueSetter(memberInfo);
        memberSetter.Invoke(entity, value);
    }
    private static int Compare(object left, object right)
    {
        if (left is IComparable comparable) return comparable.CompareTo(right);
        return Comparer.Default.Compare(left, right);
    }
    private static object Negate(object operand)
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
    private static object Not(object operand)
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