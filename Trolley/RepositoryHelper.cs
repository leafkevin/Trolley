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

    public static void AddWhereParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr,
       Expression typedWhereObjExpr, Expression parameterNameExpr, string whereObjMemberName, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", whereObj.Name);
        Expression whereObjValueExpr = Expression.PropertyOrField(typedWhereObjExpr, whereObjMemberName);
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
}
