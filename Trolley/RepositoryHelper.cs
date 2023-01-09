using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

class RepositoryHelper
{
    public static void AddParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr,
        Expression parameterNameExpr, Expression parameterValueExpr, int? nativeDbType, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", value);
        var valueExpr = Expression.Convert(parameterValueExpr, typeof(object));
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
        Expression parameterNameExpr, Expression typedParameterExpr, string parameterMemberName, int? nativeDbType, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", nativeDbType, whereObj.Name);
        var valueExpr = Expression.PropertyOrField(typedParameterExpr, parameterMemberName);
        AddParameter(commandExpr, ormProviderExpr, parameterNameExpr, valueExpr, nativeDbType, blockBodies);
    }
}
