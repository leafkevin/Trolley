using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

public static class FasterEvaluator
{
    private static ConcurrentDictionary<int, Func<object, object>> memberGetterCache = new();
    private static ConcurrentDictionary<int, Action<object, object>> memberSetterCache = new();

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
        var list = Activator.CreateInstance(expression.Type);
        foreach (var item in expression.Initializers)
        {
            item.AddMethod.Invoke(list, new[] { item.Arguments.FirstOrDefault().Evaluate() });
        }
        return list;
    }
    public static object Evaluate(this NewExpression expression)
    {
        if (expression.Arguments.Count > 0)
            return Activator.CreateInstance(expression.Type, expression.Arguments.Select(arg => arg.Evaluate()).ToArray());
        else return Activator.CreateInstance(expression.Type);
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
            return Activator.CreateInstance(expression.Type);
        return target;
        //throw new InvalidExpressionException($"The default constructor for expression '{expression}' is not found.");
    }
    public static object Evaluate(this DefaultExpression expression) => expression.Type.IsValueType ? Activator.CreateInstance(expression.Type) : null;
    public static object Evaluate(this MemberInfo member, object obj, object[] parameters = null)
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
        var type = memberInfo.DeclaringType;
        var cacheKey = HashCode.Combine(type, memberInfo);
        var memberGetter = memberGetterCache.GetOrAdd(cacheKey, f =>
        {
            Expression valueExpr;
            var objExpr = Expression.Parameter(typeof(object), "obj");
            if (memberInfo is FieldInfo fieldInfo)
            {
                if (fieldInfo.IsStatic) valueExpr = Expression.Field(null, fieldInfo);
                else
                {
                    var typedObjExpr = Expression.Convert(objExpr, type);
                    valueExpr = Expression.Field(typedObjExpr, fieldInfo);
                }
            }
            else if (memberInfo is PropertyInfo propertyInfo)
            {
                var methodInfo = propertyInfo.GetGetMethod();
                if (methodInfo.IsStatic) valueExpr = Expression.Call(methodInfo);
                else
                {
                    var typedObjExpr = Expression.Convert(objExpr, type);
                    valueExpr = Expression.Call(typedObjExpr, methodInfo);
                }
            }
            else throw new NotSupportedException("不支持的成员访问");
            if (valueExpr.Type != typeof(object))
                valueExpr = Expression.Convert(valueExpr, typeof(object));
            return Expression.Lambda<Func<object, object>>(valueExpr, objExpr).Compile();
        });
        return memberGetter.Invoke(entity);
    }
    public static void SetValueAndCache(object entity, MemberInfo memberInfo, object value)
    {
        var type = memberInfo.DeclaringType;
        var cacheKey = HashCode.Combine(type, memberInfo);
        var memberSetter = memberSetterCache.GetOrAdd(cacheKey, f =>
        {
            Expression bodyExpr = null;
            var objExpr = Expression.Parameter(typeof(object), "obj");
            var valueExpr = Expression.Parameter(typeof(object), "value");
            if (memberInfo is FieldInfo fieldInfo)
            {
                var typedValueExpr = Expression.Convert(valueExpr, fieldInfo.FieldType);
                if (fieldInfo.IsStatic)
                    bodyExpr = Expression.Assign(Expression.Field(null, fieldInfo), typedValueExpr);
                else
                {
                    var typedObjExpr = Expression.Convert(objExpr, type);
                    bodyExpr = Expression.Assign(Expression.Field(typedObjExpr, fieldInfo), typedValueExpr);
                }
            }
            else if (memberInfo is PropertyInfo propertyInfo)
            {
                var methodInfo = propertyInfo.GetSetMethod();
                var typedValueExpr = Expression.Convert(valueExpr, propertyInfo.PropertyType);
                if (methodInfo.IsStatic)
                    bodyExpr = Expression.Call(methodInfo, typedValueExpr);
                else
                {
                    var typedObjExpr = Expression.Convert(objExpr, type);
                    bodyExpr = Expression.Call(typedObjExpr, methodInfo, typedValueExpr);
                }
            }
            else throw new NotSupportedException("不支持的成员访问");
            return Expression.Lambda<Action<object, object>>(bodyExpr, objExpr, valueExpr).Compile();
        });
        memberSetter.Invoke(entity, value);
    }
}
