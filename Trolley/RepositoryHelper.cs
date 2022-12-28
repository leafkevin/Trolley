using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

class RepositoryHelper
{
    private static ConcurrentDictionary<int, object> commandInitializerCache = new();
    private static ConcurrentDictionary<int, object> memberGetterCache = new();

    public static void AddParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr,
        Expression typedParameterExpr, Expression parameterNameExpr, string parameterMemberName, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", whereObj.Name);
        Expression whereObjValueExpr = Expression.PropertyOrField(typedParameterExpr, parameterMemberName);
        whereObjValueExpr = Expression.Convert(whereObjValueExpr, typeof(object));
        var methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
        Expression dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, whereObjValueExpr);
        dbParameterExpr = Expression.Convert(dbParameterExpr, typeof(object));

        //command.Parameters.Add(parameter);
        var propertyInfo = typeof(IDbCommand).GetProperty(nameof(IDbCommand.Parameters));
        var parametersExpr = Expression.MakeMemberAccess(commandExpr, propertyInfo);
        methodInfo = typeof(IList).GetMethod(nameof(IDbCommand.Parameters.Add));
        var addParameterExpr = Expression.Call(parametersExpr, methodInfo, dbParameterExpr);
        blockBodies.Add(addParameterExpr);
    }
    public static void AddDbParameter(ParameterExpression dbParametersExpr, ParameterExpression ormProviderExpr,
        Expression typedParameterExpr, Expression parameterNameExpr, string parameterMemberName, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", whereObj.Name);
        Expression whereObjValueExpr = Expression.PropertyOrField(typedParameterExpr, parameterMemberName);
        whereObjValueExpr = Expression.Convert(whereObjValueExpr, typeof(object));
        var methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
        Expression dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, whereObjValueExpr);
        dbParameterExpr = Expression.Convert(dbParameterExpr, typeof(object));

        //dbParameters.Add(parameter);
        methodInfo = typeof(List<IDbDataParameter>).GetMethod(nameof(IDbCommand.Parameters.Add));
        var addParameterExpr = Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr);
        blockBodies.Add(addParameterExpr);
    }
    public static object GetMemberValue(MemberMap memberMapper, object entity)
    {
        var cacheKey = HashCode.Combine(memberMapper.Parent.EntityType, memberMapper.MemberName);
        if (!memberGetterCache.TryGetValue(cacheKey, out var memberGetter))
        {
            var objExpr = Expression.Parameter(typeof(object), "obj");
            var entityExpr = Expression.Convert(objExpr, memberMapper.Parent.EntityType);
            var memberExpr = Expression.PropertyOrField(entityExpr, memberMapper.MemberName);
            var bodyExpr = Expression.Convert(memberExpr, typeof(object));
            memberGetter = Expression.Lambda<Func<object, object>>(bodyExpr, objExpr);
            memberGetterCache.TryAdd(cacheKey, memberGetter);
        }
        var memberValueFunc = (Func<object, object>)memberGetter;
        return memberValueFunc.Invoke(entity);
    }
}
