using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

public static class FasterEvaluator
{
    public static object Evaluate(this Expression expression, object target = null)
    {
        return expression switch
        {
            //BinaryExpression binaryExpression => binaryExpression.Evaluate(),
            ConstantExpression constantExpression => constantExpression.Evaluate(),
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
    public static object Evaluate(this BinaryExpression expression) => expression.Right.Evaluate();
    public static object Evaluate(this ConstantExpression expression) => expression.Value;
    public static object Evaluate(this UnaryExpression expression) => expression.Operand.Evaluate();
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
        //throw new InvalidExpressionException($"The default constructor for expression '{expression}' is not found.");
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
}
