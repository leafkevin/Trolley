using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

class RepositoryHelper
{
    public static void AddParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr, Expression parameterNameExpr, Expression parameterValueExpr,
        bool isNullable, int? nativeDbType, Dictionary<Type, ParameterExpression> localExprs, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", value);
        //if(value==null)objLocal=DBNull.Value;
        //else objLocal=value;
        //var parameter = ormProvider.CreateParameter("@Parameter", objLocal);
        Expression valueExpr = null;
        if (isNullable)
        {
            if (!localExprs.TryGetValue(typeof(object), out var objLocalExpr))
            {
                objLocalExpr = Expression.Variable(typeof(object), "objLocal");
                blockParameters.Add(objLocalExpr);
                localExprs.TryAdd(typeof(object), objLocalExpr);
            }
            var isNullExpr = Expression.Equal(parameterValueExpr, Expression.Constant(null));
            var assignNullExpr = Expression.Assign(objLocalExpr, Expression.Constant(DBNull.Value));
            var assignValueExpr = Expression.Assign(objLocalExpr, Expression.Convert(parameterValueExpr, typeof(object)));
            blockBodies.Add(Expression.IfThenElse(isNullExpr, assignNullExpr, assignValueExpr));
            valueExpr = objLocalExpr;
        }
        else valueExpr = Expression.Convert(parameterValueExpr, typeof(object));

        MethodInfo methodInfo = null;
        Expression dbParameterExpr = null;
        if (nativeDbType.HasValue)
        {
            var dbTypeExpr = Expression.Constant(nativeDbType.Value, typeof(int));
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(int), typeof(object) });
            dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, dbTypeExpr, valueExpr);
        }
        else
        {
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
            dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, valueExpr);
        }
        dbParameterExpr = Expression.Convert(dbParameterExpr, typeof(object));

        //command.Parameters.Add(parameter);
        var propertyInfo = typeof(IDbCommand).GetProperty(nameof(IDbCommand.Parameters));
        var parametersExpr = Expression.MakeMemberAccess(commandExpr, propertyInfo);
        methodInfo = typeof(IList).GetMethod(nameof(IDbCommand.Parameters.Add));
        var addParameterExpr = Expression.Call(parametersExpr, methodInfo, dbParameterExpr);
        blockBodies.Add(addParameterExpr);
    }
    public static void AddParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr,
        Expression parameterNameExpr, Expression parameterValueExpr, int? nativeDbType, List<Expression> blockBodies)
        => AddParameter(commandExpr, ormProviderExpr, parameterNameExpr, parameterValueExpr, false, nativeDbType, null, null, blockBodies);
    public static void AddParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr, Expression parameterNameExpr, Expression typedParameterExpr,
        bool isNullable, int? nativeDbType, MemberMap memberMapper, Dictionary<Type, ParameterExpression> localParameters, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", nativeDbType, whereObj.Name);
        var valueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
        if (memberMapper.TypeHandler != null)
        {
            var typeHandlerExpr = Expression.Constant(memberMapper.TypeHandler);
            var objValueExpr = Expression.Convert(valueExpr, typeof(object));
            if (!localParameters.TryGetValue(typeof(IDbDataParameter), out var dbParameterExpr))
            {
                dbParameterExpr = Expression.Variable(typeof(IDbDataParameter), "dbParameter");
                blockParameters.Add(dbParameterExpr);
                localParameters.TryAdd(typeof(IDbDataParameter), dbParameterExpr);
            }
            MethodInfo methodInfo = null;
            if (nativeDbType.HasValue)
            {
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(int), typeof(object) });
                var newParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, Expression.Constant(nativeDbType.Value), objValueExpr);
                blockBodies.Add(Expression.Assign(dbParameterExpr, newParameterExpr));
            }
            else
            {
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
                var newParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, objValueExpr);
                blockBodies.Add(Expression.Assign(dbParameterExpr, newParameterExpr));
            }
            methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.SetValue), new Type[] { typeof(IOrmProvider), typeof(IDbDataParameter), typeof(object) });
            blockBodies.Add(Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, dbParameterExpr, objValueExpr));

            //command.Parameters.Add(parameter);
            var propertyInfo = typeof(IDbCommand).GetProperty(nameof(IDbCommand.Parameters));
            var parametersExpr = Expression.MakeMemberAccess(commandExpr, propertyInfo);
            methodInfo = typeof(IList).GetMethod(nameof(IDbCommand.Parameters.Add));
            var addParameterExpr = Expression.Call(parametersExpr, methodInfo, dbParameterExpr);
            blockBodies.Add(addParameterExpr);
        }
        else AddParameter(commandExpr, ormProviderExpr, parameterNameExpr, valueExpr, isNullable, nativeDbType, localParameters, blockParameters, blockBodies);
    }
    public static void AddParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr,
        Expression parameterNameExpr, Expression typedParameterExpr, MemberMap memberMapper, List<Expression> blockBodies)
    {
        //没有TypeHandler处理,主要是主键
        //var parameter = ormProvider.CreateParameter("@Parameter", nativeDbType, whereObj.Name);
        var valueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
        AddParameter(commandExpr, ormProviderExpr, parameterNameExpr, valueExpr, false, memberMapper.NativeDbType, null, null, blockBodies);
    }
}