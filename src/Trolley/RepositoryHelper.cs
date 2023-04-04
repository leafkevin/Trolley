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
        MethodInfo methodInfo = null;

        //int? age = 25;
        //age.Value;
        if (valueExpr.Type.IsNullableType(out var underlyingType))
            valueExpr = Expression.Property(valueExpr, "Value");

        if (nativeDbType != null)
        {
            var defaultType = ormProvider.MapDefaultType(nativeDbType);
            if (defaultType != underlyingType)
            {
                //Gender? gender = Gender.Male;
                //(int)gender.Value;
                if (underlyingType.IsEnumType(out _, out var enumUnderlyingType))
                {
                    if (defaultType == typeof(string))
                    {
                        methodInfo = typeof(Enum).GetMethod(nameof(Enum.GetName), new Type[] { typeof(Type), typeof(object) });
                        var convertExpr = Expression.Convert(valueExpr, typeof(object));
                        valueExpr = Expression.Call(methodInfo, Expression.Constant(underlyingType), convertExpr);
                    }
                    else if (defaultType == typeof(byte) || defaultType == typeof(sbyte) || defaultType == typeof(short)
                          || defaultType == typeof(ushort) || defaultType == typeof(int) || defaultType == typeof(uint)
                          || defaultType == typeof(long) || defaultType == typeof(ulong))
                        valueExpr = Expression.Convert(valueExpr, enumUnderlyingType);
                    else throw new NotSupportedException($"不支持的NativeDbType类型,MemberType:{underlyingType.FullName},NativeDbType:{nativeDbType}");
                }
                else if (underlyingType == typeof(Guid))
                {
                    if (defaultType == typeof(string))
                        valueExpr = Expression.Call(valueExpr, typeof(Guid).GetMethod(nameof(Guid.ToString), Type.EmptyTypes));
                    else if (defaultType == typeof(byte[]))
                        valueExpr = Expression.Call(valueExpr, typeof(Guid).GetMethod(nameof(Guid.ToByteArray), Type.EmptyTypes));
                    else throw new NotSupportedException($"不支持的NativeDbType类型,MemberType:{underlyingType.FullName},NativeDbType:{nativeDbType}");
                }
                else if (underlyingType == typeof(TimeSpan) || underlyingType == typeof(TimeOnly))
                {
                    if (defaultType == typeof(long))
                        valueExpr = Expression.Property(valueExpr, "Ticks");
                }
                else
                {
                    var typeCode = Type.GetTypeCode(defaultType);
                    string toTypeMethod = null;
                    switch (typeCode)
                    {
                        case TypeCode.Boolean: toTypeMethod = nameof(Convert.ToBoolean); break;
                        case TypeCode.Char: toTypeMethod = nameof(Convert.ToChar); break;
                        case TypeCode.Byte: toTypeMethod = nameof(Convert.ToByte); break;
                        case TypeCode.SByte: toTypeMethod = nameof(Convert.ToSByte); break;
                        case TypeCode.Int16: toTypeMethod = nameof(Convert.ToInt16); break;
                        case TypeCode.UInt16: toTypeMethod = nameof(Convert.ToUInt16); break;
                        case TypeCode.Int32: toTypeMethod = nameof(Convert.ToInt32); break;
                        case TypeCode.UInt32: toTypeMethod = nameof(Convert.ToUInt32); break;
                        case TypeCode.Int64: toTypeMethod = nameof(Convert.ToInt64); break;
                        case TypeCode.UInt64: toTypeMethod = nameof(Convert.ToUInt64); break;
                        case TypeCode.Single: toTypeMethod = nameof(Convert.ToSingle); break;
                        case TypeCode.Double: toTypeMethod = nameof(Convert.ToDouble); break;
                        case TypeCode.Decimal: toTypeMethod = nameof(Convert.ToDecimal); break;
                        case TypeCode.DateTime: toTypeMethod = nameof(Convert.ToDateTime); break;
                        case TypeCode.String: toTypeMethod = nameof(Convert.ToString); break;
                    }
                    if (!string.IsNullOrEmpty(toTypeMethod))
                    {
                        methodInfo = typeof(Convert).GetMethod(toTypeMethod, new Type[] { underlyingType });
                        valueExpr = Expression.Call(methodInfo, valueExpr);
                    }
                    else valueExpr = Expression.Convert(valueExpr, defaultType);
                }
            }
        }

        valueExpr = Expression.Convert(valueExpr, typeof(object));

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

        Expression dbParameterExpr = null;
        if (nativeDbType != null)
        {
            //var dbParameter = ormProvider.CreateParameter("@Name", SqlDbType.VarChar);
            var dbTypeExpr = Expression.Constant(nativeDbType, typeof(object));
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
            dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, dbTypeExpr, valueExpr);
        }
        else
        {
            //var dbParameter = ormProvider.CreateParameter("@Name", "kevin");
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
                var dbTypeExpr = Expression.Constant(nativeDbType, typeof(object));
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                var callExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, dbTypeExpr, valueExpr);
                blockBodies.Add(Expression.Assign(dbParameterExpr, callExpr));
            }
            else
            {
                //var dbParameter = ormProvider.CreateParameter("@Name", "kevin");
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
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