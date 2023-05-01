using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

class RepositoryHelper
{
    private static ConcurrentDictionary<Type, Func<object, DbType>> getDbTypeDelegates = new();
    public static void AddParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr, Expression parameterNameExpr, Expression parameterValueExpr, bool isExpectNullable,
        object nativeDbType, IOrmProvider ormProvider, Dictionary<string, int> localParameters, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", value);
        //if(value==null)objLocal=DBNull.Value;
        //else objLocal=value;
        //var parameter = ormProvider.CreateParameter("@Parameter", objLocal);
        Expression valueExpr = parameterValueExpr;
        Expression nativeDbTypeExpr = null;
        MethodInfo methodInfo = null;

        Expression dbParameterExpr = null;
        if (nativeDbType != null)
        {
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.ToFieldValue), new Type[] { typeof(object), typeof(object) });
            nativeDbTypeExpr = Expression.Convert(Expression.Constant(nativeDbType), typeof(object));
            valueExpr = Expression.Convert(valueExpr, typeof(object));
            valueExpr = Expression.Call(ormProviderExpr, methodInfo, valueExpr, nativeDbTypeExpr);
        }

        //INSERT场景
        if (isExpectNullable)
        {
            //object localValue;
            //if(gender == null)
            //  localValue = DBNull.Value;
            //else localValue = (object)gender.Value;
            var isNullExpr = Expression.Equal(parameterValueExpr, Expression.Constant(null));
            var objLocalExpr = DefineLocalParameter("objLocal", typeof(object), localParameters, blockParameters);
            var assignNullExpr = Expression.Assign(objLocalExpr, Expression.Constant(DBNull.Value));
            var assignValueExpr = Expression.Assign(objLocalExpr, valueExpr);
            blockBodies.Add(Expression.IfThenElse(isNullExpr, assignNullExpr, assignValueExpr));
            valueExpr = objLocalExpr;
        }
        if (nativeDbType != null)
        {
            //var dbParameter = ormProvider.CreateParameter("@Name", SqlDbType.VarChar);
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
            dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, valueExpr);
        }
        else
        {
            //var dbParameter = ormProvider.CreateParameter("@Name", "kevin");
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
            valueExpr = Expression.Convert(valueExpr, typeof(object));
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
    /// <summary>
    /// Where条件，参数是单纯的值类型主键
    /// </summary>
    /// <param name="commandExpr"></param>
    /// <param name="ormProviderExpr"></param>
    /// <param name="parameterNameExpr"></param>
    /// <param name="parameterValueExpr"></param>
    /// <param name="nativeDbType"></param>
    /// <param name="blockBodies"></param>
    public static void AddParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr,
        Expression parameterNameExpr, Expression parameterValueExpr, object nativeDbType, IOrmProvider ormProvider, List<Expression> blockBodies)
        => AddParameter(commandExpr, ormProviderExpr, parameterNameExpr, parameterValueExpr, false, nativeDbType, ormProvider, null, null, blockBodies);
    /// <summary>
    /// Where条件，参数是实体的成员，值有可能是null，有TypeHandler处理
    /// 批量操作可能有为null的参数，插入有可能有为null的参数，其他场景都是不能有为null的参数
    /// </summary>
    /// <param name="commandExpr"></param>
    /// <param name="ormProviderExpr"></param>
    /// <param name="parameterNameExpr"></param>
    /// <param name="typedParameterExpr"></param>
    /// <param name="isExpectNullable"></param>
    /// <param name="nativeDbType"></param>
    /// <param name="memberMapper"></param>
    /// <param name="localParameters"></param>
    /// <param name="blockParameters"></param>
    /// <param name="blockBodies"></param>
    public static void AddParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr, Expression parameterNameExpr, Expression typedParameterExpr, bool isExpectNullable,
        object nativeDbType, MemberMap memberMapper, IOrmProvider ormProvider, Dictionary<string, int> localParameters, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        //只有插入场景有可能有为null的参数，其他场景都期望参数是不可能为null，如果为null,将会报错
        //如果字段想要更新为null，请使用明确的表达式Update，如：repository.Update<Order>().Set(f => new { BuyerId = DBNull.Value }).Where(a => a.BuyerId == 1).Execute();

        //var parameter = ormProvider.CreateParameter("@Parameter", nativeDbType, whereObj.Name);
        var parameterValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
        Expression valueExpr = parameterValueExpr;
        MethodInfo methodInfo = null;

        if (memberMapper.TypeHandler != null)
        {
            //var dbParameter = ormProvider.CreateParameter("@Name", SqlDbType.VarChar);
            var dbParameterExpr = DefineLocalParameter("dbParameter", typeof(IDbDataParameter), localParameters, blockParameters);
            if (nativeDbType != null)
            {
                var nativeDbTypeExpr = Expression.Convert(Expression.Constant(nativeDbType), typeof(object));
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                var newParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, valueExpr);
                blockBodies.Add(Expression.Assign(dbParameterExpr, newParameterExpr));
            }
            else
            {
                //var dbParameter = ormProvider.CreateParameter("@Name", "kevin");
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
                valueExpr = Expression.Convert(valueExpr, typeof(object));
                var newParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, valueExpr);
                blockBodies.Add(Expression.Assign(dbParameterExpr, newParameterExpr));
            }

            //typeHandler.SetValue(ormProvider, dbParameter, value);
            var typeHandlerExpr = Expression.Constant(memberMapper.TypeHandler);
            methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.SetValue), new Type[] { typeof(IOrmProvider), typeof(IDbDataParameter), typeof(object) });
            blockBodies.Add(Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, dbParameterExpr, valueExpr));

            //command.Parameters.Add(dbParameter);
            var propertyInfo = typeof(IDbCommand).GetProperty(nameof(IDbCommand.Parameters));
            var parametersExpr = Expression.MakeMemberAccess(commandExpr, propertyInfo);
            methodInfo = typeof(IList).GetMethod(nameof(IDbCommand.Parameters.Add));
            var addParameterExpr = Expression.Call(parametersExpr, methodInfo, dbParameterExpr);
            blockBodies.Add(addParameterExpr);
        }
        else AddParameter(commandExpr, ormProviderExpr, parameterNameExpr, valueExpr, isExpectNullable, nativeDbType, ormProvider, localParameters, blockParameters, blockBodies);
    }
    /// <summary>
    /// SQL中使用的参数，没有TypeHandler处理，值不可能为null
    /// </summary>
    /// <param name="commandExpr"></param>
    /// <param name="ormProviderExpr"></param>
    /// <param name="parameterNameExpr"></param>
    /// <param name="typedParameterExpr"></param>
    /// <param name="memberMapper"></param>
    /// <param name="blockBodies"></param>
    public static void AddParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr,
        Expression parameterNameExpr, Expression typedParameterExpr, MemberMap memberMapper, IOrmProvider ormProvider, List<Expression> blockBodies)
    {
        //单纯SQL,没有TypeHandler处理
        //var parameter = ormProvider.CreateParameter("@Parameter", nativeDbType, whereObj.Name);
        var valueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
        AddParameter(commandExpr, ormProviderExpr, parameterNameExpr, valueExpr, false, memberMapper.NativeDbType, ormProvider, null, null, blockBodies);
    }
    private static ParameterExpression DefineLocalParameter(string namePrefix, Type localVariableType, Dictionary<string, int> localParameters, List<ParameterExpression> blockParameters)
    {
        if (!localParameters.TryGetValue(namePrefix, out var index))
            index = 0;
        localParameters[namePrefix] = index + 1;
        var objLocalExpr = Expression.Variable(localVariableType, $"{namePrefix}{index + 1}");
        blockParameters.Add(objLocalExpr);
        return objLocalExpr;
    }
}