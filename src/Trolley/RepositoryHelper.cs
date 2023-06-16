using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

class RepositoryHelper
{
    public static void AddKeyValueParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr, Expression parameterNameExpr,
        Expression parameterValueExpr, MemberMap memberMapper, IOrmProvider ormProvider, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", value);
        //if(value==null)objLocal=DBNull.Value;
        //else objLocal=value;
        //var parameter = ormProvider.CreateParameter("@Parameter", objLocal);
        Expression valueExpr = parameterValueExpr;
        Expression memberMapperExpr = null;
        MethodInfo methodInfo = null;

        Expression dbParameterExpr = null;
        if (memberMapper != null)
        {
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.ToFieldValue), new Type[] { typeof(MemberMap), typeof(object) });
            memberMapperExpr = Expression.Constant(memberMapper);
            valueExpr = Expression.Call(ormProviderExpr, methodInfo, memberMapperExpr, valueExpr);

            if (memberMapper.NativeDbType != null)
            {
                //var dbParameter = ormProvider.CreateParameter("@Name", SqlDbType.VarChar, "kevin");
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                var nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
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
        }
        //command.Parameters.Add(parameter);
        var propertyInfo = typeof(IDbCommand).GetProperty(nameof(IDbCommand.Parameters));
        var parametersExpr = Expression.MakeMemberAccess(commandExpr, propertyInfo);
        methodInfo = typeof(IList).GetMethod(nameof(IDbCommand.Parameters.Add));
        var addParameterExpr = Expression.Call(parametersExpr, methodInfo, dbParameterExpr);
        blockBodies.Add(addParameterExpr);
    }
    public static void AddKeyMemberParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr, Expression parameterNameExpr,
        Expression typedParameterExpr, MemberMap memberMapper, IOrmProvider ormProvider, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", value);
        Expression valueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
        Expression memberMapperExpr = null;
        MethodInfo methodInfo = null;

        Expression dbParameterExpr = null;
        if (memberMapper != null)
        {
            //var objValue = ormProvider.ToFieldValue(memberMapper, "kevin");
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.ToFieldValue), new Type[] { typeof(MemberMap), typeof(object) });
            memberMapperExpr = Expression.Constant(memberMapper);
            if (valueExpr.Type != typeof(object))
                valueExpr = Expression.Convert(valueExpr, typeof(object));
            valueExpr = Expression.Call(ormProviderExpr, methodInfo, memberMapperExpr, valueExpr);

            if (memberMapper.NativeDbType != null)
            {
                //var dbParameter = ormProvider.CreateParameter("@Name", SqlDbType.VarChar, objValue);
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                var nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
                dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, valueExpr);
            }
            else
            {
                //var dbParameter = ormProvider.CreateParameter("@Name", objValue);
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
                valueExpr = Expression.Convert(valueExpr, typeof(object));
                dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, valueExpr);
            }
            dbParameterExpr = Expression.Convert(dbParameterExpr, typeof(object));
        }
        //command.Parameters.Add(parameter);
        var propertyInfo = typeof(IDbCommand).GetProperty(nameof(IDbCommand.Parameters));
        var parametersExpr = Expression.MakeMemberAccess(commandExpr, propertyInfo);
        methodInfo = typeof(IList).GetMethod(nameof(IDbCommand.Parameters.Add));
        var addParameterExpr = Expression.Call(parametersExpr, methodInfo, dbParameterExpr);
        blockBodies.Add(addParameterExpr);
    }
    public static void AddMemberParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr, Expression parameterNameExpr,
        Expression typedParameterExpr, MemberMap memberMapper, IOrmProvider ormProvider, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", value);
        //if(value==null)objLocal=DBNull.Value;
        //else objLocal=value;
        //var parameter = ormProvider.CreateParameter("@Parameter", objLocal);
        var memberValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
        Expression valueExpr = memberValueExpr;
        Expression memberMapperExpr = null;
        MethodInfo methodInfo = null;

        Expression dbParameterExpr = null;
        if (memberMapper != null)
        {
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.ToFieldValue), new Type[] { typeof(MemberMap), typeof(object) });
            memberMapperExpr = Expression.Constant(memberMapper);
            if (valueExpr.Type != typeof(object))
                valueExpr = Expression.Convert(valueExpr, typeof(object));
            valueExpr = Expression.Call(ormProviderExpr, methodInfo, memberMapperExpr, valueExpr);

            if (memberMapper.NativeDbType != null)
            {
                //var dbParameter = ormProvider.CreateParameter("@Name", SqlDbType.VarChar, "kevin");
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                var nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
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
        }
        //command.Parameters.Add(parameter);
        var propertyInfo = typeof(IDbCommand).GetProperty(nameof(IDbCommand.Parameters));
        var parametersExpr = Expression.MakeMemberAccess(commandExpr, propertyInfo);
        methodInfo = typeof(IList).GetMethod(nameof(IDbCommand.Parameters.Add));
        var addParameterExpr = Expression.Call(parametersExpr, methodInfo, dbParameterExpr);
        blockBodies.Add(addParameterExpr);
    }
    public static void AddMemberParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr, Expression parameterNameExpr, Expression typedParameterExpr, bool isNullable,
        MemberMap memberMapper, IOrmProvider ormProvider, Dictionary<string, int> localParameters, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", value);
        //if(value==null)objLocal=DBNull.Value;
        //else objLocal=value;
        //var parameter = ormProvider.CreateParameter("@Parameter", objLocal);
        var memberValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
        Expression valueExpr = memberValueExpr;
        Expression memberMapperExpr = null;
        MethodInfo methodInfo = null;

        Expression dbParameterExpr = null;
        if (memberMapper != null)
        {
            if (memberMapper.TypeHandler != null)
            {
                dbParameterExpr = DefineLocalParameter("dbParameter", typeof(IDbDataParameter), localParameters, blockParameters);
                if (valueExpr.Type != typeof(object))
                    valueExpr = Expression.Convert(valueExpr, typeof(object));
                if (memberMapper.NativeDbType != null)
                {
                    //var dbParameter = ormProvider.CreateParameter("@Name", SqlDbType.VarChar, value);
                    var nativeDbTypeExpr = Expression.Convert(Expression.Constant(memberMapper.NativeDbType), typeof(object));
                    methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                    var newParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, valueExpr);
                    blockBodies.Add(Expression.Assign(dbParameterExpr, newParameterExpr));
                }
                else
                {
                    //var dbParameter = ormProvider.CreateParameter("@Name", value);
                    methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
                    var newParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, valueExpr);
                    blockBodies.Add(Expression.Assign(dbParameterExpr, newParameterExpr));
                }

                //typeHandler.SetValue(ormProvider, dbParameter, value);
                var typeHandlerExpr = Expression.Constant(memberMapper.TypeHandler);
                methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.SetValue), new Type[] { typeof(IOrmProvider), typeof(IDbDataParameter), typeof(object) });
                Expression assignValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, dbParameterExpr, valueExpr);

                //INSERT场景
                if (isNullable && memberMapper.MemberType.IsNullableType(out _)
                    || memberMapper.MemberType == typeof(string) || memberMapper.MemberType.IsEntityType())
                {
                    //var dbParameter = ormProvider.CreateParameter("@Name", SqlDbType.VarChar, value);
                    //if(gender == null)
                    //  dbParameter.Value = DBNull.Value;
                    //else typeHandler.SetValue(ormProvider, dbParameter, value);
                    var isNullExpr = Expression.Equal(valueExpr, Expression.Constant(null));
                    methodInfo = typeof(IDataParameter).GetProperty(nameof(IDataParameter.Value)).GetSetMethod();
                    var assignNullExpr = Expression.Call(dbParameterExpr, methodInfo, Expression.Constant(DBNull.Value));
                    assignValueExpr = Expression.IfThenElse(isNullExpr, assignNullExpr, assignValueExpr);
                }
                blockBodies.Add(assignValueExpr);
            }
            else
            {
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.ToFieldValue), new Type[] { typeof(MemberMap), typeof(object) });
                memberMapperExpr = Expression.Constant(memberMapper);
                if (valueExpr.Type != typeof(object))
                    valueExpr = Expression.Convert(valueExpr, typeof(object));
                valueExpr = Expression.Call(ormProviderExpr, methodInfo, memberMapperExpr, valueExpr);

                //INSERT场景                
                if (isNullable && memberMapper.MemberType.IsNullableType(out _)
                    || memberMapper.MemberType == typeof(string) || memberMapper.MemberType.IsEntityType())
                {
                    //object localValue;
                    //if(gender == null)
                    //  localValue = DBNull.Value;
                    //else localValue = (object)gender.Value;
                    var isNullExpr = Expression.Equal(valueExpr, Expression.Constant(null));
                    var objLocalExpr = DefineLocalParameter("objLocal", typeof(object), localParameters, blockParameters);
                    var assignNullExpr = Expression.Assign(objLocalExpr, Expression.Constant(DBNull.Value));
                    var assignValueExpr = Expression.Assign(objLocalExpr, valueExpr);
                    blockBodies.Add(Expression.IfThenElse(isNullExpr, assignNullExpr, assignValueExpr));
                    valueExpr = objLocalExpr;
                }
                if (memberMapper.NativeDbType != null)
                {
                    //var dbParameter = ormProvider.CreateParameter("@Name", SqlDbType.VarChar, "kevin");
                    methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                    var nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
                    dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, valueExpr);
                }
                else
                {
                    //var dbParameter = ormProvider.CreateParameter("@Name", "kevin");
                    methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
                    valueExpr = Expression.Convert(valueExpr, typeof(object));
                    dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, valueExpr);
                }
            }
            dbParameterExpr = Expression.Convert(dbParameterExpr, typeof(object));
        }
        //command.Parameters.Add(parameter);
        var propertyInfo = typeof(IDbCommand).GetProperty(nameof(IDbCommand.Parameters));
        var parametersExpr = Expression.MakeMemberAccess(commandExpr, propertyInfo);
        methodInfo = typeof(IList).GetMethod(nameof(IDbCommand.Parameters.Add));
        var addParameterExpr = Expression.Call(parametersExpr, methodInfo, dbParameterExpr);
        blockBodies.Add(addParameterExpr);
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