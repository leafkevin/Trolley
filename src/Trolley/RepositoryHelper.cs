﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;

namespace Trolley;

public class RepositoryHelper
{
    private static ConcurrentDictionary<int, object> queryGetCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> queryMultiGetCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> queryWhereObjCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> queryMultiWhereObjCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> queryExistsCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> queryMultiExistsCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Action<IDataParameterCollection, IOrmProvider, object>> queryRawSqlCommandInitializerCache = new();

    private static ConcurrentDictionary<int, Func<IDataParameterCollection, IOrmProvider, object, string>> createSqlParametersCache = new();
    private static ConcurrentDictionary<int, (Action<StringBuilder, object>, Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>)> createMultiSqlParametersCache = new();

    //private static ConcurrentDictionary<int, Func<IDataParameterCollection, IOrmProvider, object, string>> createCommandInitializerCache = new();
    //private static ConcurrentDictionary<int, object> createWithByCommandInitializerCache = new();
    //private static ConcurrentDictionary<int, object> createMultiWithByCommandInitializerCache = new();

    private static ConcurrentDictionary<int, object> deleteCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> deleteMultiCommandInitializerCache = new();

    private static ConcurrentDictionary<int, object> deleteBulkCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> deleteMultiBulkCommandInitializerCache = new();


    private static ConcurrentDictionary<int, (string, object)> createBulkCommandInitializerCache = new();
    private static ConcurrentDictionary<int, (string, object)> createMultiBulkCommandInitializerCache = new();


    private static ConcurrentDictionary<int, Func<IDataParameterCollection, IOrmProvider, object, string>> updateCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>> updateMultiCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> updateBulkCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> updateMultiBulkCommandInitializerCache = new();

    private static ConcurrentDictionary<int, object> updateWithCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> updateMultiWithCommandInitializerCache = new();


    private static ConcurrentDictionary<int, object> whereWithKeysCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> mutilWhereWithKeysCommandInitializerCache = new();

    public static void AddValueParameter(Expression dbParametersExpr, Expression ormProviderExpr, Expression parameterNameExpr,
        Expression parameterValueExpr, MemberMap memberMapper, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        MethodInfo methodInfo = null;
        Expression typedParameterExpr = null;
        var fieldValueExpr = parameterValueExpr;
        var fieldValueType = parameterValueExpr.Type;
        var addMethodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add));
        var createParameterMethodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
        Expression nativeDbTypeExpr = Expression.Constant(memberMapper.NativeDbType);
        if (nativeDbTypeExpr.Type != typeof(object))
            nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));
        Expression addTypedParameterExpr = null;

        if (memberMapper.TypeHandler != null)
        {
            var typeHandlerExpr = Expression.Constant(memberMapper.TypeHandler);
            methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
            if (fieldValueExpr.Type != typeof(object))
                fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
            fieldValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, fieldValueExpr);
            typedParameterExpr = Expression.Call(ormProviderExpr, createParameterMethodInfo, parameterNameExpr, nativeDbTypeExpr, fieldValueExpr);
            addTypedParameterExpr = Expression.Call(dbParametersExpr, addMethodInfo, typedParameterExpr);
            if (!memberMapper.IsRequired)
            {
                var equalsExpr = Expression.Equal(fieldValueExpr, Expression.Constant(null));
                var nullExpr = Expression.Call(ormProviderExpr, createParameterMethodInfo, parameterNameExpr, nativeDbTypeExpr, Expression.Constant(DBNull.Value));
                var addNullExpr = Expression.Call(dbParametersExpr, addMethodInfo, nullExpr);
                blockBodies.Add(Expression.IfThenElse(equalsExpr, addNullExpr, addTypedParameterExpr));
            }
            else blockBodies.Add(Expression.Call(dbParametersExpr, addMethodInfo, addTypedParameterExpr));
            return;
        }

        //数据库类型和当前值类型不一致
        if (memberMapper.DbDefaultType != fieldValueType)
        {
            bool isNullableType = memberMapper.MemberType.IsNullableType(out var underlyingType);
            if (underlyingType.IsEnumType(out _, out var enumUnderlyingType))
            {
                //数据库类型是字符串
                if (memberMapper.DbDefaultType == typeof(string))
                {
                    //枚举类型或是数字类型
                    if (!fieldValueType.IsEnum)
                    {
                        //数字类型，可以转换为对应的enumUnderlyingType
                        if (fieldValueType != enumUnderlyingType)
                            fieldValueExpr = Expression.Convert(fieldValueExpr, enumUnderlyingType);
                        methodInfo = typeof(Enum).GetMethod(nameof(Enum.ToObject), new Type[] { typeof(Type), enumUnderlyingType });
                        fieldValueExpr = Expression.Call(methodInfo, Expression.Constant(underlyingType), fieldValueExpr);
                        fieldValueExpr = Expression.Convert(fieldValueExpr, underlyingType);
                    }
                    //把枚举类型再变成字符串类型
                    fieldValueExpr = Expression.Call(fieldValueExpr, typeof(Enum).GetMethod(nameof(Enum.ToString), Type.EmptyTypes));
                }
                //数据库类型是数字类型
                else
                {
                    //枚举类型或是数字类型
                    if (fieldValueType.IsEnum)
                        fieldValueExpr = Expression.Convert(fieldValueExpr, enumUnderlyingType);
                    if (memberMapper.DbDefaultType != enumUnderlyingType)
                        fieldValueExpr = Expression.Convert(fieldValueExpr, memberMapper.DbDefaultType);
                }
            }
            else if (underlyingType == typeof(Guid))
            {
                if (memberMapper.DbDefaultType == typeof(string))
                    fieldValueExpr = Expression.Call(fieldValueExpr, typeof(Guid).GetMethod(nameof(Guid.ToString), Type.EmptyTypes));
                else if (memberMapper.DbDefaultType == typeof(byte[]))
                    fieldValueExpr = Expression.Call(fieldValueExpr, typeof(Guid).GetMethod(nameof(Guid.ToByteArray), Type.EmptyTypes));
            }
            else if (underlyingType == typeof(DateTime))
            {
                if (memberMapper.DbDefaultType == typeof(long))
                    fieldValueExpr = Expression.Property(fieldValueExpr, nameof(DateTime.Ticks));
                if (memberMapper.DbDefaultType == typeof(string))
                {
                    methodInfo = typeof(DateTime).GetMethod(nameof(DateTime.ToString), new Type[] { typeof(string) });
                    fieldValueExpr = Expression.Call(fieldValueExpr, methodInfo, Expression.Constant("yyyy-MM-dd HH:mm:ss.fffffff"));
                }
            }
            else if (underlyingType == typeof(DateOnly))
            {
                if (memberMapper.DbDefaultType == typeof(string))
                {
                    methodInfo = typeof(DateTime).GetMethod(nameof(DateTime.ToString), new Type[] { typeof(string) });
                    fieldValueExpr = Expression.Call(fieldValueExpr, methodInfo, Expression.Constant("yyyy-MM-dd"));
                }
            }
            else if (underlyingType == typeof(TimeSpan))
            {
                if (memberMapper.DbDefaultType == typeof(long))
                    fieldValueExpr = Expression.Property(fieldValueExpr, nameof(TimeSpan.Ticks));
                if (memberMapper.DbDefaultType == typeof(string))
                {
                    methodInfo = typeof(TimeSpan).GetMethod(nameof(TimeSpan.ToString), new Type[] { typeof(string) });
                    var greaterThanExpr = Expression.GreaterThanOrEqual(fieldValueExpr, Expression.Constant(1));
                    var ifExpr = Expression.Call(fieldValueExpr, methodInfo, Expression.Constant("d\\.hh\\:mm\\:ss\\.fffffff"));
                    var elseExpr = Expression.Call(fieldValueExpr, methodInfo, Expression.Constant("hh\\:mm\\:ss\\.fffffff"));

                    var localVariable = Expression.Variable(typeof(string), $"str{memberMapper.MemberName}");
                    blockParameters.Add(localVariable);
                    var assignIfExpr = Expression.Assign(localVariable, ifExpr);
                    var assignElseExpr = Expression.Assign(localVariable, elseExpr);
                    blockBodies.Add(Expression.IfThenElse(greaterThanExpr, assignIfExpr, assignElseExpr));
                    fieldValueExpr = localVariable;
                }
            }
            else if (underlyingType == typeof(TimeOnly))
            {
                if (memberMapper.DbDefaultType == typeof(long))
                    fieldValueExpr = Expression.Property(fieldValueExpr, nameof(TimeOnly.Ticks));
                if (memberMapper.DbDefaultType == typeof(string))
                {
                    methodInfo = typeof(TimeOnly).GetMethod(nameof(TimeOnly.ToString), new Type[] { typeof(string) });
                    fieldValueExpr = Expression.Call(fieldValueExpr, methodInfo, Expression.Constant("hh\\:mm\\:ss\\.fffffff"));
                }
            }
            else
            {
                methodInfo = typeof(Convert).GetMethod(nameof(Convert.ChangeType), new Type[] { typeof(Object), typeof(Type) });
                if (parameterValueExpr.Type != typeof(object))
                    fieldValueExpr = Expression.Convert(parameterValueExpr, typeof(object));
                fieldValueExpr = Expression.Call(methodInfo, fieldValueExpr, Expression.Constant(memberMapper.DbDefaultType));
            }
        }

        if (fieldValueExpr.Type != typeof(object))
            fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
        typedParameterExpr = Expression.Call(ormProviderExpr, createParameterMethodInfo, parameterNameExpr, nativeDbTypeExpr, fieldValueExpr);
        addTypedParameterExpr = Expression.Call(dbParametersExpr, addMethodInfo, typedParameterExpr);

        Expression addParameterExpr = null;
        if (!memberMapper.IsRequired && parameterValueExpr.Type.IsNullableType(out _))
        {
            var equalsExpr = Expression.Equal(parameterValueExpr, Expression.Constant(null));
            var nullExpr = Expression.Call(ormProviderExpr, createParameterMethodInfo, parameterNameExpr, nativeDbTypeExpr, Expression.Constant(DBNull.Value));
            var addNullExpr = Expression.Call(dbParametersExpr, addMethodInfo, nullExpr);
            addParameterExpr = Expression.IfThenElse(equalsExpr, addNullExpr, addTypedParameterExpr);
        }
        else addParameterExpr = addTypedParameterExpr;
        blockBodies.Add(addParameterExpr);
    }
    public static void AddValueParameter(Expression dbParametersExpr, Expression ormProviderExpr,
        Expression parameterNameExpr, Expression parameterValueExpr, List<Expression> blockBodies)
    {
        var fieldValueExpr = parameterValueExpr;
        var fieldValueType = parameterValueExpr.Type;
        var addMethodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add));
        bool isNullableType = fieldValueType.IsNullableType(out var underlyingType);
        if (underlyingType.IsEnumType(out _, out var enumUnderlyingType))
            fieldValueExpr = Expression.Convert(fieldValueExpr, enumUnderlyingType);
        if (fieldValueExpr.Type != typeof(object))
            fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));

        var methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
        var typedParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, fieldValueExpr);
        Expression addParameterExpr = Expression.Call(dbParametersExpr, addMethodInfo, typedParameterExpr);

        if (isNullableType)
        {
            var equalsExpr = Expression.Equal(parameterValueExpr, Expression.Constant(null));
            var nullExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, Expression.Constant(DBNull.Value));
            var addNullExpr = Expression.Call(dbParametersExpr, addMethodInfo, nullExpr);
            addParameterExpr = Expression.IfThenElse(equalsExpr, addNullExpr, addParameterExpr);
        }
        blockBodies.Add(addParameterExpr);
    }

    //public static Action<IDataParameterCollection, IOrmProvider, string, object> BuildAddDbParameters(string dbKey, IOrmProvider ormProvider, MemberMap memberMapper, object fieldVallue)
    //{
    //    var fieldVallueType = fieldVallue.GetType();
    //    var cacheKey = HashCode.Combine(dbKey, ormProvider, memberMapper.Parent.EntityType, memberMapper, fieldVallueType);
    //    return addDbParametersCache.GetOrAdd(cacheKey, f =>
    //    {
    //        var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
    //        var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
    //        var parameterNameExpr = Expression.Parameter(typeof(string), "parameterName");
    //        var fieldValueExpr = Expression.Parameter(typeof(object), "fieldValue");

    //        var typedFieldValueExpr = Expression.Variable(fieldVallueType, "typedFieldValue");
    //        var blockParameters = new List<ParameterExpression> { typedFieldValueExpr };
    //        var blockBodies = new List<Expression>();
    //        blockBodies.Add(Expression.Assign(typedFieldValueExpr, Expression.Convert(fieldValueExpr, fieldVallueType)));

    //        RepositoryHelper.AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
    //        return Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, string, object>>(
    //            Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr).Compile();
    //    });
    //}
    public static string BuildFieldsSqlPart(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type selectType, bool isSelect)
    {
        var index = 0;
        var entityMapper = mapProvider.GetEntityMap(entityType);
        var memberInfos = selectType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

        var builder = new StringBuilder();
        foreach (var memberInfo in memberInfos)
        {
            if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
                || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                continue;

            if (index > 0) builder.Append(',');
            builder.Append(ormProvider.GetFieldName(memberMapper.FieldName));
            if (isSelect && memberMapper.FieldName != memberMapper.MemberName)
                builder.Append(" AS " + ormProvider.GetFieldName(memberMapper.MemberName));
            index++;
        }
        return builder.ToString();
    }
    public static object BuildWhereSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type whereObjType, bool isWhereKey, bool hasSuffix, bool isWithKey, string whereObjName, string headSql)
    {
        object commandInitializer = null;
        var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
        var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
        var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

        ParameterExpression suffixExpr = null;
        ParameterExpression parameterNameExpr = null;
        var builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();

        if (hasSuffix)
        {
            suffixExpr = Expression.Parameter(typeof(string), "suffix");
            parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
            blockParameters.Add(parameterNameExpr);
        }
        blockParameters.Add(builderExpr);
        var constructorExpr = typeof(StringBuilder).GetConstructor(Type.EmptyTypes);
        blockBodies.Add(Expression.Assign(builderExpr, Expression.New(constructorExpr)));

        MethodInfo methodInfo = null;
        var entityMapper = mapProvider.GetEntityMap(entityType);
        var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
        if (!string.IsNullOrEmpty(headSql))
            blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(headSql)));
        if (typeof(IDictionary<string, object>).IsAssignableFrom(whereObjType))
        {
            var dictExpr = Expression.Variable(typeof(IDictionary<string, object>), "dict");
            var fieldValueExpr = Expression.Variable(typeof(object), "fieldValue");
            blockParameters.AddRange(new[] { dictExpr, fieldValueExpr });
            blockBodies.Add(Expression.Assign(dictExpr, Expression.Convert(whereObjExpr, typeof(IDictionary<string, object>))));

            var index = 0;
            var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
            if (isWhereKey)
            {
                var tryGetValueMethodInfo = typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.TryGetValue));
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    var keyMemberExpr = Expression.Constant(keyMapper.MemberName);
                    var isFalseExpr = Expression.IsFalse(Expression.Call(dictExpr, tryGetValueMethodInfo, keyMemberExpr, fieldValueExpr));
                    var exceptionExpr = Expression.Constant(new ArgumentNullException(whereObjName, $"字典参数缺少主键字段{keyMapper.MemberName}"));
                    blockBodies.Add(Expression.IfThen(isFalseExpr, Expression.Throw(exceptionExpr, typeof(ArgumentNullException))));

                    var parameterName = ormProvider.ParameterPrefix + (isWithKey ? "k" : "") + keyMapper.MemberName;
                    Expression myParameterNameExpr = Expression.Constant(parameterName);
                    if (hasSuffix)
                    {
                        myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, suffixExpr);
                        blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                        myParameterNameExpr = parameterNameExpr;
                    }

                    if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                    var fieldNameExpr = Expression.Constant($"{ormProvider.GetFieldName(keyMapper.FieldName)}=");
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, myParameterNameExpr));
                    AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, keyMapper, blockParameters, blockBodies);
                    index++;
                }
            }
            else
            {
                var indexExpr = Expression.Variable(typeof(int), "index");
                var enumeratorExpr = Expression.Variable(typeof(IEnumerable<KeyValuePair<string, object>>), "enumerator");
                var itemKeyExpr = Expression.Variable(typeof(string), "itemKey");
                var memberMapperExpr = Expression.Variable(typeof(MemberMap), "memberMapper");
                var outTypeExpr = Expression.Variable(typeof(Type), "outType");
                blockParameters.AddRange(new[] { indexExpr, enumeratorExpr, itemKeyExpr, fieldValueExpr, memberMapperExpr, outTypeExpr });
                var breakLabel = Expression.Label();
                var continueLabel = Expression.Label();

                //if(!enumerator.MoveNext())
                //  break;
                var loopBodies = new List<Expression>();
                methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
                loopBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(dictExpr, methodInfo)));
                methodInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
                var ifFalseExpr = Expression.IsFalse(Expression.Call(enumeratorExpr, methodInfo));
                var ifThenExpr = Expression.IfThen(ifFalseExpr, Expression.Break(breakLabel));

                //var entityMapper = new EntityMap{ ... };
                //var itemKey = enumerator.Current.Key;
                //var fieldValue = enumerator.Current.Value;
                var entityMapperExpr = Expression.Constant(entityMapper);
                var currentExpr = Expression.Property(enumeratorExpr, nameof(IEnumerator.Current));
                loopBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
                loopBodies.Add(Expression.Assign(itemKeyExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Key))));
                loopBodies.Add(Expression.Assign(fieldValueExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Value))));

                //var isContinue = !entityMapper.TryGetMemberMap(itemKey, out var memberMapper)
                //|| memberMapper.IsIgnore || memberMapper.IsNavigation
                methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.TryGetMemberMap));
                Expression isContinueExpr = Expression.IsFalse(Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr, memberMapperExpr));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnore)));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsNavigation)));

                //|| (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.IsEntityType));
                var memberTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.MemberType));
                var isEntityTypeExpr = Expression.Call(methodInfo, memberTypeExpr, outTypeExpr);
                var isNullExpr = Expression.Equal(Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler)), Expression.Constant(null));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.AndAlso(isEntityTypeExpr, isNullExpr));
                //if(isContinue)continue;
                loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

                //var parameterName = ormProvider.ParameterPrefix + itemKey + multiMark;
                Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix + (isWithKey ? "k" : ""));
                myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, itemKeyExpr);
                if (hasSuffix)
                {
                    myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, suffixExpr);
                    loopBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                    myParameterNameExpr = parameterNameExpr;
                }

                //if(index > 0) builder.Append(" AND ");
                var greaterThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0));
                var callExpr = Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND "));
                loopBodies.Add(Expression.IfThen(greaterThenExpr, callExpr));

                //builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetFieldName));
                Expression fieldNameExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.FieldName));
                fieldNameExpr = Expression.Call(ormProviderExpr, methodInfo, fieldNameExpr);
                loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
                loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant("=")));
                loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));

                //ormProvider.AddDbParameter(dbKey, dbParameters, memberMapper, parameterName, fieldValue);
                methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.AddDbParameter));
                loopBodies.Add(Expression.Call(methodInfo, ormProviderExpr, Expression.Constant(dbKey),
                    dbParametersExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr));

                //index++;
                loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));

                blockBodies.Add(Expression.Loop(Expression.Block(loopBodies), breakLabel, continueLabel));
            }
        }
        else
        {
            ParameterExpression typedWhereObjExpr = null;
            bool isEntityType = false;
            List<MemberInfo> memberInfos = null;

            if (whereObjType.IsEntityType(out _))
            {
                isEntityType = true;
                typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");
                blockParameters.Add(typedWhereObjExpr);
                blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));
                memberInfos = whereObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
            }
            else
            {
                if (entityMapper.KeyMembers.Count > 1)
                    throw new NotSupportedException($"模型{entityType.FullName}有多个主键字段，不能使用单个值类型{whereObjType.FullName}作为参数");
                memberInfos = entityMapper.KeyMembers.Select(f => f.Member).ToList();
            }

            var index = 0;
            var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
            foreach (var memberInfo in memberInfos)
            {
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                    || memberMapper.IsIgnore || memberMapper.IsNavigation
                    || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                    continue;

                if (isWhereKey && memberMapper.IsKey && isEntityType && !memberInfos.Exists(f => f.Name == memberMapper.MemberName))
                    throw new ArgumentNullException(whereObjName, $"参数类型{whereObjType.FullName}缺少主键字段{memberMapper.MemberName}");

                Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix + (isWithKey ? "k" : "") + memberMapper.MemberName);
                if (hasSuffix)
                {
                    myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, suffixExpr);
                    blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                    myParameterNameExpr = parameterNameExpr;
                }

                if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                var fieldNameExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}=");
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, myParameterNameExpr));

                if (isEntityType)
                {
                    var fieldValueExpr = Expression.PropertyOrField(typedWhereObjExpr, memberMapper.MemberName);
                    AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                }
                //var fieldValueExpr = Expression.Convert(whereObjExpr, memberMapper.MemberType);
                else AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, whereObjExpr, memberMapper, blockParameters, blockBodies);
                index++;
            }
        }

        methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
        var returnExpr = Expression.Call(builderExpr, methodInfo);
        var resultLabelExpr = Expression.Label(typeof(string));
        blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));

        if (hasSuffix) commandInitializer = Expression.Lambda<Func<IDataParameterCollection, IOrmProvider, object, string, string>>(
            Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, whereObjExpr, suffixExpr).Compile();
        else commandInitializer = Expression.Lambda<Func<IDataParameterCollection, IOrmProvider, object, string>>(
            Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, whereObjExpr).Compile();
        return commandInitializer;
    }

    public static object BuildGetSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiple)
    {
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, whereObjType);
        var commandInitializerCache = isMultiple ? queryMultiGetCommandInitializerCache : queryGetCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var fieldsSql = BuildFieldsSqlPart(ormProvider, mapProvider, entityType, entityType, true);
            var headSql = $"SELECT {fieldsSql} FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ";
            return BuildWhereSqlParameters(dbKey, ormProvider, mapProvider, entityType, whereObjType, true, isMultiple, false, nameof(whereObj), headSql);
        });
    }
    public static object BuildQueryWhereObjSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiple)
    {
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, whereObjType);
        var commandInitializerCache = isMultiple ? queryMultiWhereObjCommandInitializerCache : queryWhereObjCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var fieldsSql = BuildFieldsSqlPart(ormProvider, mapProvider, entityType, entityType, true);
            var headSql = $"SELECT {fieldsSql} FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ";
            return BuildWhereSqlParameters(dbKey, ormProvider, mapProvider, entityType, whereObjType, false, isMultiple, false, nameof(whereObj), headSql);
        });
    }
    public static object BuildExistsSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiple)
    {
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, whereObjType);
        var commandInitializerCache = isMultiple ? queryMultiExistsCommandInitializerCache : queryExistsCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var headSql = $"SELECT COUNT(1) FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ";
            return BuildWhereSqlParameters(dbKey, ormProvider, mapProvider, entityType, whereObjType, true, isMultiple, false, nameof(whereObj), headSql);
        });
    }
    public static Action<IDataParameterCollection, IOrmProvider, object> BuildQueryRawSqlParameters(string dbKey, IOrmProvider ormProvider, string rawSql, object parameters)
    {
        Action<IDataParameterCollection, IOrmProvider, object> commandInitializer = null;
        if (parameters is IDictionary<string, object>)
        {
            commandInitializer = (dbParameters, ormProvider, parameter) =>
            {
                var dict = parameter as IDictionary<string, object>;
                foreach (var item in dict)
                {
                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                        continue;
                    var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                    dbParameters.Add(dbParameter);
                }
            };
        }
        else
        {
            var parameterType = parameters.GetType();
            var cacheKey = HashCode.Combine(dbKey, rawSql, parameterType);
            commandInitializer = queryRawSqlCommandInitializerCache.GetOrAdd(cacheKey, f =>
            {
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
                foreach (var memberInfo in memberInfos)
                {
                    var parameterName = ormProvider.ParameterPrefix + memberInfo.Name;
                    if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                        continue;
                    var parameterNameExpr = Expression.Constant(parameterName);
                    var fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberInfo.Name);
                    AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, blockBodies);
                }
                return Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, object>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, parameterExpr).Compile();
            });
        }
        return commandInitializer;
    }

    public static Func<IDataParameterCollection, IOrmProvider, object, string> BuildCreateSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object insertObj, string[] onlyFieldNames, string[] ignoreFieldNames, bool isReturnIdentity)
    {
        var insertObjType = insertObj.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames, isReturnIdentity);
        return createSqlParametersCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var headSqlSetter = BuildCreateHeadSqlPart(dbKey, ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames);
            var valuesSqlParameters = BuildCreateValuesPartSqlParametes(dbKey, ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames, false);
            Func<IDataParameterCollection, IOrmProvider, object, string> commandInitializer = null;
            commandInitializer = (dbParameters, ormProvider, insertObj) =>
            {
                var builder = new StringBuilder($"INSERT INTO {ormProvider.GetFieldName(entityMapper.TableName)} (");
                headSqlSetter.Invoke(builder, insertObj);
                builder.Append(") VALUES (");
                var typedValuesSqlParameters = valuesSqlParameters as Action<IDataParameterCollection, IOrmProvider, StringBuilder, object>;
                typedValuesSqlParameters.Invoke(dbParameters, ormProvider, builder, insertObj);
                builder.Append(')');
                if (isReturnIdentity) builder.Append(ormProvider.GetIdentitySql(entityType));
                return builder.ToString();
            };
            return commandInitializer;
        });
    }
    public static (Action<StringBuilder>, Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>) BuildCreateMultiSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object insertObjs, string[] onlyFieldNames, string[] ignoreFieldNames, bool isBulk)
    {
        object insertObj = insertObjs;
        if (isBulk)
        {
            var entities = insertObjs as IEnumerable;
            foreach (var entity in entities)
            {
                insertObj = entity;
                break;
            }
        }
        var insertObjType = insertObjs.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames);
        (var headSqlParameterSetter, var commandInitializer) = createMultiSqlParametersCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var headSqlPartSetter = BuildCreateHeadSqlPart(dbKey, ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames);
            Action<StringBuilder, object> headSqlParameterSetter = (builder, parameter) =>
            {
                builder.Append($"INSERT INTO {ormProvider.GetFieldName(entityMapper.TableName)} (");
                headSqlPartSetter.Invoke(builder, parameter);
                builder.Append(") VALUES ");
            };

            var valuesSqlParameters = BuildCreateValuesPartSqlParametes(dbKey, ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames, true);
            Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string> commandInitializer = null;
            commandInitializer = (dbParameters, ormProvider, builder, insertObj, suffix) =>
            {
                builder.Append('(');
                var typedValuesSqlParameters = valuesSqlParameters as Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>;
                typedValuesSqlParameters.Invoke(dbParameters, ormProvider, builder, insertObj, suffix);
                builder.Append(')');
            };
            return (headSqlParameterSetter, commandInitializer);
        });
        Action<StringBuilder> headSqlSetter = builder => headSqlParameterSetter.Invoke(builder, insertObj);
        return (headSqlSetter, commandInitializer);
    }
    public static Action<StringBuilder, object> BuildCreateHeadSqlPart(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type insertObjType, string[] onlyFieldNames, string[] ignoreFieldNames)
    {
        Action<StringBuilder, object> commandInitializer = null;
        var entityMapper = mapProvider.GetEntityMap(entityType);
        if (typeof(IDictionary<string, object>).IsAssignableFrom(insertObjType))
        {
            commandInitializer = (builder, insertObj) =>
            {
                int index = 0;
                var dict = insertObj as IDictionary<string, object>;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;

                    if (ignoreFieldNames != null && ignoreFieldNames.Contains(item.Key))
                        continue;
                    if (onlyFieldNames != null && !onlyFieldNames.Contains(item.Key))
                        continue;

                    if (index > 0) builder.Append(',');
                    builder.Append(ormProvider.GetFieldName(memberMapper.FieldName));
                    index++;
                }
            };
        }
        else
        {
            var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
            var insertObjExpr = Expression.Parameter(typeof(object), "insertObj");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var memberInfos = insertObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

            var index = 0;
            foreach (var memberInfo in memberInfos)
            {
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                    || memberMapper.IsIgnore || memberMapper.IsNavigation
                    || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                    continue;

                if (ignoreFieldNames != null && ignoreFieldNames.Contains(memberInfo.Name))
                    continue;
                if (onlyFieldNames != null && !onlyFieldNames.Contains(memberInfo.Name))
                    continue;

                if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(',')));
                var fieldNameExpr = Expression.Constant(ormProvider.GetFieldName(memberMapper.FieldName));
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
                index++;
            }
            commandInitializer = Expression.Lambda<Action<StringBuilder, object>>(
                Expression.Block(blockParameters, blockBodies), builderExpr, insertObjExpr).Compile();
        }
        return commandInitializer;
    }
    public static object BuildCreateValuesPartSqlParametes(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type insertObjType, string[] onlyFieldNames, string[] ignoreFieldNames, bool hasSuffix)
    {
        var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
        var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
        var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
        var insertObjExpr = Expression.Parameter(typeof(object), "insertObj");
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();

        ParameterExpression suffixExpr = null;
        ParameterExpression parameterNameExpr = null;
        if (hasSuffix)
        {
            suffixExpr = Expression.Parameter(typeof(string), "suffix");
            parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
            blockParameters.Add(parameterNameExpr);
        }
        MethodInfo methodInfo = null;
        var entityMapper = mapProvider.GetEntityMap(entityType);
        var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
        var concatMethodInfo1 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
        var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });

        if (typeof(IDictionary<string, object>).IsAssignableFrom(insertObjType))
        {
            var dictExpr = Expression.Variable(typeof(IDictionary<string, object>), "dict");
            var fieldValueExpr = Expression.Variable(typeof(object), "fieldValue");
            blockParameters.AddRange(new[] { dictExpr, fieldValueExpr });
            blockBodies.Add(Expression.Assign(dictExpr, Expression.Convert(insertObjExpr, typeof(IDictionary<string, object>))));

            var indexExpr = Expression.Variable(typeof(int), "index");
            var enumeratorExpr = Expression.Variable(typeof(IEnumerable<KeyValuePair<string, object>>), "enumerator");
            var itemKeyExpr = Expression.Variable(typeof(string), "itemKey");
            var memberMapperExpr = Expression.Variable(typeof(MemberMap), "memberMapper");
            var outTypeExpr = Expression.Variable(typeof(Type), "outType");
            blockParameters.AddRange(new[] { indexExpr, enumeratorExpr, itemKeyExpr, fieldValueExpr, memberMapperExpr, outTypeExpr });
            var breakLabel = Expression.Label();
            var continueLabel = Expression.Label();

            //if(!enumerator.MoveNext())
            //  break;
            var loopBodies = new List<Expression>();
            methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
            loopBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(dictExpr, methodInfo)));
            methodInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
            var ifFalseExpr = Expression.IsFalse(Expression.Call(enumeratorExpr, methodInfo));
            var ifThenExpr = Expression.IfThen(ifFalseExpr, Expression.Break(breakLabel));

            //var entityMapper = new EntityMap{ ... };
            //var itemKey = enumerator.Current.Key;
            //var fieldValue = enumerator.Current.Value;
            var entityMapperExpr = Expression.Constant(entityMapper);
            var currentExpr = Expression.Property(enumeratorExpr, nameof(IEnumerator.Current));
            loopBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
            loopBodies.Add(Expression.Assign(itemKeyExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Key))));
            loopBodies.Add(Expression.Assign(fieldValueExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Value))));

            //var isContinue = !entityMapper.TryGetMemberMap(itemKey, out var memberMapper)
            //|| memberMapper.IsIgnore || memberMapper.IsNavigation
            methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.TryGetMemberMap));
            Expression isContinueExpr = Expression.IsFalse(Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr, memberMapperExpr));
            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnore)));
            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsNavigation)));

            //|| ignoreFields.Constains(itemKey) || !onlyFields.Constains(itemKey)
            if (ignoreFieldNames != null)
            {
                var ignoreFieldsExpr = Expression.Constant(ignoreFieldNames);
                methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), new Type[] { typeof(string) });
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Call(methodInfo, ignoreFieldsExpr, itemKeyExpr));
            }
            if (onlyFieldNames != null)
            {
                var onlyFieldsExpr = Expression.Constant(onlyFieldNames);
                methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), new Type[] { typeof(string) });
                var isFalseExpr = Expression.IsFalse(Expression.Call(methodInfo, onlyFieldsExpr, itemKeyExpr));
                isContinueExpr = Expression.OrElse(isContinueExpr, isFalseExpr);
            }

            //|| (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
            methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.IsEntityType));
            var memberTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.MemberType));
            var isEntityTypeExpr = Expression.Call(methodInfo, memberTypeExpr, outTypeExpr);
            var isNullExpr = Expression.Equal(Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler)), Expression.Constant(null));
            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.AndAlso(isEntityTypeExpr, isNullExpr));
            //if(isContinue)continue;
            loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

            //var parameterName = ormProvider.ParameterPrefix + itemKey + suffix;
            Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix);
            if (hasSuffix)
                myParameterNameExpr = Expression.Call(concatMethodInfo2, myParameterNameExpr, itemKeyExpr, suffixExpr);
            else myParameterNameExpr = Expression.Call(concatMethodInfo1, myParameterNameExpr, itemKeyExpr);
            blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));

            //if(index > 0) builder.Append(",");
            var greaterThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0));
            var callExpr = Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(","));
            loopBodies.Add(Expression.IfThen(greaterThenExpr, callExpr));

            //builder.Append(parameterName);
            loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));

            //ormProvider.AddDbParameter(dbKey, dbParameters, memberMapper, parameterName, fieldValue);
            methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.AddDbParameter));
            loopBodies.Add(Expression.Call(methodInfo, ormProviderExpr, Expression.Constant(dbKey),
                dbParametersExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr));

            //index++;
            loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));

            blockBodies.Add(Expression.Loop(Expression.Block(loopBodies), breakLabel, continueLabel));
        }
        else
        {
            var typedInsertObjExpr = Expression.Parameter(insertObjType, "typedInsertObj");
            blockBodies.Add(Expression.Assign(typedInsertObjExpr, Expression.Convert(insertObjExpr, insertObjType)));
            var memberInfos = insertObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                 .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

            var index = 0;
            foreach (var memberInfo in memberInfos)
            {
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                    || memberMapper.IsIgnore || memberMapper.IsNavigation
                    || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                    continue;

                if (ignoreFieldNames != null && ignoreFieldNames.Contains((memberInfo.Name)))
                    continue;
                if (onlyFieldNames != null && !onlyFieldNames.Contains((memberInfo.Name)))
                    continue;

                if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(",")));

                Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix + memberMapper.MemberName);
                if (hasSuffix)
                {
                    myParameterNameExpr = Expression.Call(concatMethodInfo1, myParameterNameExpr, suffixExpr);
                    blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                    myParameterNameExpr = parameterNameExpr;
                }
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, myParameterNameExpr));

                var fieldValueExpr = Expression.PropertyOrField(typedInsertObjExpr, memberMapper.MemberName);
                AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                index++;
            }
        }

        object result = null;
        if (hasSuffix) result = Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>>(
            Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, builderExpr, insertObjExpr, suffixExpr).Compile();
        else result = Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, StringBuilder, object>>(
            Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, builderExpr, insertObjExpr).Compile();
        return result;
    }
    //public static object BuildCreateWithByCommandInitializer(ISqlVisitor sqlVisitor, Type entityType, object insertObj, bool isMultiple)
    //{
    //    object commandInitializer = null;
    //    if (insertObj is IDictionary<string, object>)
    //    {
    //        if (isMultiple)
    //        {
    //            Action<IDataParameterCollection, StringBuilder, StringBuilder, object, string> typedCommandInitializer = null;
    //            typedCommandInitializer = (dbParameters, fieldsBuilder, valuesBuilder, insertObj, multiMark) =>
    //            {
    //                int index = 0;
    //                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
    //                var dict = insertObj as IDictionary<string, object>;
    //                foreach (var item in dict)
    //                {
    //                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
    //                        || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
    //                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
    //                        continue;

    //                    if (index > 0)
    //                    {
    //                        fieldsBuilder.Append(',');
    //                        valuesBuilder.Append(',');
    //                    }
    //                    var parameterName = sqlVisitor.OrmProvider.ParameterPrefix + multiMark + memberMapper.MemberName;
    //                    fieldsBuilder.Append(sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName));
    //                    valuesBuilder.Append(parameterName);
    //                    sqlVisitor.OrmProvider.AddDbParameter(sqlVisitor.DbKey, dbParameters, memberMapper, parameterName, item.Value);
    //                    index++;
    //                }
    //            };
    //            commandInitializer = typedCommandInitializer;
    //        }
    //        else
    //        {
    //            Action<IDataParameterCollection, StringBuilder, StringBuilder, object> typedCommandInitializer = null;
    //            typedCommandInitializer = (dbParameters, fieldsBuilder, valuesBuilder, insertObj) =>
    //            {
    //                int index = 0;
    //                var dict = insertObj as IDictionary<string, object>;
    //                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
    //                foreach (var item in dict)
    //                {
    //                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
    //                        || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
    //                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
    //                        continue;

    //                    if (index > 0)
    //                    {
    //                        fieldsBuilder.Append(',');
    //                        valuesBuilder.Append(',');
    //                    }
    //                    var parameterName = sqlVisitor.OrmProvider.ParameterPrefix + memberMapper.MemberName;
    //                    fieldsBuilder.Append(sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName));
    //                    valuesBuilder.Append(parameterName);
    //                    sqlVisitor.OrmProvider.AddDbParameter(sqlVisitor.DbKey, dbParameters, memberMapper, parameterName, item.Value);
    //                    index++;
    //                }
    //            };
    //            commandInitializer = typedCommandInitializer;
    //        }
    //    }
    //    else
    //    {
    //        var parameterType = insertObj.GetType();
    //        if (!parameterType.IsEntityType(out _))
    //            throw new NotSupportedException("方法WithBy<TInsertObject>(TInsertObject insertObj)只支持类对象参数，不支持基础类型参数");

    //        var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
    //        var dbParametersInitializerCache = isMultiple ? createMultiWithByCommandInitializerCache : createWithByCommandInitializerCache;
    //        commandInitializer = dbParametersInitializerCache.GetOrAdd(cacheKey, f =>
    //        {
    //            var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
    //            var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
    //                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
    //            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
    //            var parameterExpr = Expression.Parameter(typeof(object), "parameter");
    //            var fieldsBuilderExpr = Expression.Parameter(typeof(StringBuilder), "fieldsBuilder");
    //            var valueBuilderExpr = Expression.Parameter(typeof(StringBuilder), "valueBuilder");
    //            ParameterExpression multiMarkExpr = null;
    //            ParameterExpression parameterNameExpr = null;
    //            var blockParameters = new List<ParameterExpression>();
    //            var blockBodies = new List<Expression>();

    //            if (isMultiple)
    //            {
    //                multiMarkExpr = Expression.Parameter(typeof(string), "multiMark");
    //                parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
    //                blockParameters.Add(parameterNameExpr);
    //            }
    //            var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
    //            blockParameters.Add(typedParameterExpr);
    //            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
    //            var ormProviderExpr = Expression.Constant(sqlVisitor.OrmProvider);

    //            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
    //            var addMethodInfo = typeof(IList).GetMethod(nameof(IList.Add));
    //            var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });

    //            int index = 0;
    //            foreach (var memberInfo in memberInfos)
    //            {
    //                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
    //                    || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
    //                    || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
    //                    continue;

    //                if (index > 0)
    //                {
    //                    blockBodies.Add(Expression.Call(fieldsBuilderExpr, appendMethodInfo, Expression.Constant(",")));
    //                    blockBodies.Add(Expression.Call(valueBuilderExpr, appendMethodInfo, Expression.Constant(",")));
    //                }

    //                var fieldName = sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName);
    //                blockBodies.Add(Expression.Call(fieldsBuilderExpr, appendMethodInfo, Expression.Constant(fieldName)));

    //                Expression myParameterNameExpr = Expression.Constant(sqlVisitor.OrmProvider.ParameterPrefix + memberMapper.MemberName);
    //                if (isMultiple)
    //                {
    //                    myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, multiMarkExpr);
    //                    blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
    //                    myParameterNameExpr = parameterNameExpr;
    //                }

    //                blockBodies.Add(Expression.Call(valueBuilderExpr, appendMethodInfo, myParameterNameExpr));
    //                var fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
    //                AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
    //                index++;
    //            }
    //            if (isMultiple) commandInitializer = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, StringBuilder, object, string>>(
    //                Expression.Block(blockParameters, blockBodies), dbParametersExpr, fieldsBuilderExpr, valueBuilderExpr, parameterExpr, multiMarkExpr).Compile();
    //            else commandInitializer = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, StringBuilder, object>>(
    //                Expression.Block(blockParameters, blockBodies), dbParametersExpr, fieldsBuilderExpr, valueBuilderExpr, parameterExpr).Compile();
    //            return commandInitializer;
    //        });
    //    }
    //    return commandInitializer;
    //}
    //public static object BuildCreateWithBulkCommandInitializer(ISqlVisitor sqlVisitor, Type entityType, object insertObjs, bool isMultiple, out string headSql)
    //{
    //    var entities = insertObjs as IEnumerable;
    //    headSql = null;
    //    object parameter = null, commandInitializer = null;
    //    foreach (var entity in entities)
    //    {
    //        parameter = entity;
    //        break;
    //    }
    //    if (parameter is IDictionary<string, object> dict)
    //    {
    //        int index = 0;
    //        var builder = new StringBuilder();
    //        var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
    //        foreach (var item in dict)
    //        {
    //            if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
    //                || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
    //                || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
    //                continue;

    //            if (index > 0) builder.Append(',');
    //            builder.Append(sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName));
    //            index++;
    //        }
    //        headSql = builder.ToString();
    //        if (isMultiple)
    //        {
    //            Action<IDataParameterCollection, StringBuilder, object, string, int> typedCommandInitializer = null;
    //            typedCommandInitializer = (dbParameters, builder, insertObj, multiMark, bulkIndex) =>
    //            {
    //                int index = 0;
    //                var dict = insertObj as IDictionary<string, object>;
    //                foreach (var item in dict)
    //                {
    //                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
    //                        || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
    //                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
    //                        continue;

    //                    if (index > 0) builder.Append(',');
    //                    var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}{item.Key}{multiMark}{bulkIndex}";
    //                    builder.Append(parameterName);
    //                    sqlVisitor.OrmProvider.AddDbParameter(sqlVisitor.DbKey, dbParameters, memberMapper, parameterName, item.Value);
    //                    index++;
    //                }
    //            };
    //            commandInitializer = typedCommandInitializer;
    //        }
    //        else
    //        {
    //            Action<IDataParameterCollection, StringBuilder, object, int> typedCommandInitializer = null;
    //            typedCommandInitializer = (dbParameters, builder, insertObj, bulkIndex) =>
    //            {
    //                int index = 0;
    //                var dict = insertObj as IDictionary<string, object>;
    //                foreach (var item in dict)
    //                {
    //                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
    //                        || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
    //                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
    //                        continue;

    //                    if (index > 0) builder.Append(',');
    //                    var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}{item.Key}{bulkIndex}";
    //                    builder.Append(parameterName);
    //                    sqlVisitor.OrmProvider.AddDbParameter(sqlVisitor.DbKey, dbParameters, memberMapper, parameterName, item.Value);
    //                    index++;
    //                }
    //            };
    //            commandInitializer = typedCommandInitializer;
    //        }
    //    }
    //    else
    //    {
    //        var parameterType = parameter.GetType();
    //        if (!parameterType.IsEntityType(out _))
    //            throw new NotSupportedException("只支持类对象，不支持基础类型");

    //        var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
    //        var dbParametersInitializerCache = isMultiple ? createMultiBulkCommandInitializerCache : createBulkCommandInitializerCache;
    //        (headSql, commandInitializer) = dbParametersInitializerCache.GetOrAdd(cacheKey, f =>
    //        {
    //            var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
    //            var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
    //                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
    //            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
    //            var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
    //            var insertObjExpr = Expression.Parameter(typeof(object), "insertObj");
    //            var bulkIndexExpr = Expression.Parameter(typeof(int), "bulkIndex");

    //            ParameterExpression multiMarkExpr = null;
    //            var blockParameters = new List<ParameterExpression>();
    //            var blockBodies = new List<Expression>();
    //            var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
    //            var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
    //            var suffixExpr = Expression.Variable(typeof(string), "suffix");
    //            blockParameters.AddRange(new[] { typedParameterExpr, parameterNameExpr, suffixExpr });
    //            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(insertObjExpr, parameterType)));
    //            var ormProviderExpr = Expression.Constant(sqlVisitor.OrmProvider);

    //            var toStringMethodInfo = typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes);
    //            var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
    //            Expression mySuffixExpr = Expression.Call(bulkIndexExpr, toStringMethodInfo);
    //            if (isMultiple)
    //            {
    //                multiMarkExpr = Expression.Parameter(typeof(string), "multiMark");
    //                mySuffixExpr = Expression.Call(concatMethodInfo, multiMarkExpr, suffixExpr);
    //            }
    //            blockBodies.Add(Expression.Assign(suffixExpr, mySuffixExpr));

    //            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });

    //            var index = 0;
    //            foreach (var memberInfo in memberInfos)
    //            {
    //                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
    //                    || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
    //                    || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
    //                    continue;

    //                if (index > 0)
    //                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(",")));

    //                var myParameterNameExpr = Expression.Constant(sqlVisitor.OrmProvider.ParameterPrefix + memberMapper.MemberName);
    //                blockBodies.Add(Expression.Assign(parameterNameExpr, Expression.Call(concatMethodInfo, myParameterNameExpr, suffixExpr)));
    //                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));

    //                var fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
    //                AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
    //                index++;
    //            }

    //            object result = null;
    //            if (isMultiple) result = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, object, string, int>>(
    //                Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, insertObjExpr, multiMarkExpr, multiMarkExpr, bulkIndexExpr).Compile();
    //            else result = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, object, int>>(
    //                Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, insertObjExpr, bulkIndexExpr).Compile();

    //            var headSql = BuildFieldsSqlPart(sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType, false);
    //            return (headSql, result);
    //        });
    //    }
    //    return commandInitializer;
    //}


    public static Func<IDataParameterCollection, IOrmProvider, object, string> BuildUpdateSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object updateObj, List<string> onlyFieldNames, List<string> ignoreFieldNames)
    {
        var updateObjType = updateObj.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, updateObjType, onlyFieldNames, ignoreFieldNames);
        return updateCommandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var setCommandInitializer = BuildUpdateSetPartSqlParameters(dbKey, ormProvider, mapProvider, entityType, updateObjType, onlyFieldNames, ignoreFieldNames, false);
            var whereCommandInitializer = BuildWhereSqlParameters(dbKey, ormProvider, mapProvider, entityType, updateObjType, true, false, true, nameof(updateObj), null);
            Func<IDataParameterCollection, IOrmProvider, object, string> commandInitializer;
            var typeSetCommandInitializer = setCommandInitializer as Action<IDataParameterCollection, IOrmProvider, StringBuilder, object>;
            var typeWhereCommandInitializer = whereCommandInitializer as Action<IDataParameterCollection, IOrmProvider, StringBuilder, object>;
            commandInitializer = (dbParameters, ormProvider, updateObj) =>
            {
                var builder = new StringBuilder($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
                typeSetCommandInitializer.Invoke(dbParameters, ormProvider, builder, updateObj);
                builder.Append(" WHERE ");
                typeWhereCommandInitializer.Invoke(dbParameters, ormProvider, builder, updateObj);
                return builder.ToString();
            };
            return commandInitializer;
        });
    }
    public static Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string> BuildUpdateMultiSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object updateObjs, List<string> onlyFieldNames, List<string> ignoreFieldNames)
    {
        var entities = updateObjs as IEnumerable;
        object updateObj = null;
        foreach (var entity in entities)
        {
            updateObj = entity;
            break;
        }
        var updateObjType = updateObj.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, updateObjType, onlyFieldNames, ignoreFieldNames);
        return updateMultiCommandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var setCommandInitializer = BuildUpdateSetPartSqlParameters(dbKey, ormProvider, mapProvider, entityType, updateObjType, onlyFieldNames, ignoreFieldNames, true);
            var whereCommandInitializer = BuildWhereSqlParameters(dbKey, ormProvider, mapProvider, entityType, updateObjType, true, true, true, nameof(updateObj), null);
            Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string> commandInitializer;
            var typeSetCommandInitializer = setCommandInitializer as Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>;
            var typeWhereCommandInitializer = whereCommandInitializer as Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>;
            commandInitializer = (dbParameters, ormProvider, builder, updateObj, suffix) =>
            {
                builder.Append($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
                typeSetCommandInitializer.Invoke(dbParameters, ormProvider, builder, updateObj, suffix);
                builder.Append(" WHERE ");
                typeWhereCommandInitializer.Invoke(dbParameters, ormProvider, builder, updateObj, suffix);
            };
            return commandInitializer;
        });
    }
    public static object BuildUpdateSetPartSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type updateObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool hasSuffix)
    {
        var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
        var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
        var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
        var updateObjExpr = Expression.Parameter(typeof(object), "updateObj");
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();

        ParameterExpression suffixExpr = null;
        ParameterExpression parameterNameExpr = null;
        if (hasSuffix)
        {
            suffixExpr = Expression.Parameter(typeof(string), "suffix");
            parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
            blockParameters.Add(parameterNameExpr);
        }
        MethodInfo methodInfo = null;
        var entityMapper = mapProvider.GetEntityMap(entityType);
        var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
        var concatMethodInfo1 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
        var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });

        if (typeof(IDictionary<string, object>).IsAssignableFrom(updateObjType))
        {
            var dictExpr = Expression.Variable(typeof(IDictionary<string, object>), "dict");
            var fieldValueExpr = Expression.Variable(typeof(object), "fieldValue");
            blockParameters.AddRange(new[] { dictExpr, fieldValueExpr });
            blockBodies.Add(Expression.Assign(dictExpr, Expression.Convert(updateObjExpr, typeof(IDictionary<string, object>))));

            var indexExpr = Expression.Variable(typeof(int), "index");
            var enumeratorExpr = Expression.Variable(typeof(IEnumerable<KeyValuePair<string, object>>), "enumerator");
            var itemKeyExpr = Expression.Variable(typeof(string), "itemKey");
            var memberMapperExpr = Expression.Variable(typeof(MemberMap), "memberMapper");
            var outTypeExpr = Expression.Variable(typeof(Type), "outType");
            blockParameters.AddRange(new[] { indexExpr, enumeratorExpr, itemKeyExpr, fieldValueExpr, memberMapperExpr, outTypeExpr });
            var breakLabel = Expression.Label();
            var continueLabel = Expression.Label();

            //if(!enumerator.MoveNext())
            //  break;
            var loopBodies = new List<Expression>();
            methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
            loopBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(dictExpr, methodInfo)));
            methodInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
            var ifFalseExpr = Expression.IsFalse(Expression.Call(enumeratorExpr, methodInfo));
            var ifThenExpr = Expression.IfThen(ifFalseExpr, Expression.Break(breakLabel));

            //var entityMapper = new EntityMap{ ... };
            //var itemKey = enumerator.Current.Key;
            //var fieldValue = enumerator.Current.Value;
            var entityMapperExpr = Expression.Constant(entityMapper);
            var currentExpr = Expression.Property(enumeratorExpr, nameof(IEnumerator.Current));
            loopBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
            loopBodies.Add(Expression.Assign(itemKeyExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Key))));
            loopBodies.Add(Expression.Assign(fieldValueExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Value))));

            //var isContinue = !entityMapper.TryGetMemberMap(itemKey, out var memberMapper)
            //|| memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsKey
            methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.TryGetMemberMap));
            Expression isContinueExpr = Expression.IsFalse(Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr, memberMapperExpr));
            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnore)));
            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsNavigation)));
            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsKey)));

            //|| ignoreFields.Constains(itemKey) || !onlyFields.Constains(itemKey)
            if (ignoreFieldNames != null)
            {
                var ignoreFieldsExpr = Expression.Constant(ignoreFieldNames);
                methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), new Type[] { typeof(string) });
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Call(methodInfo, ignoreFieldsExpr, itemKeyExpr));
            }
            if (onlyFieldNames != null)
            {
                var onlyFieldsExpr = Expression.Constant(onlyFieldNames);
                methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), new Type[] { typeof(string) });
                var isFalseExpr = Expression.IsFalse(Expression.Call(methodInfo, onlyFieldsExpr, itemKeyExpr));
                isContinueExpr = Expression.OrElse(isContinueExpr, isFalseExpr);
            }

            //|| (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
            methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.IsEntityType));
            var memberTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.MemberType));
            var isEntityTypeExpr = Expression.Call(methodInfo, memberTypeExpr, outTypeExpr);
            var isNullExpr = Expression.Equal(Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler)), Expression.Constant(null));
            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.AndAlso(isEntityTypeExpr, isNullExpr));
            //if(isContinue)continue;
            loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

            //var parameterName = ormProvider.ParameterPrefix + itemKey + multiMark;
            Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix);
            if (hasSuffix)
                myParameterNameExpr = Expression.Call(concatMethodInfo2, myParameterNameExpr, itemKeyExpr, suffixExpr);
            else myParameterNameExpr = Expression.Call(concatMethodInfo1, myParameterNameExpr, itemKeyExpr);
            blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));

            //if(index > 0) builder.Append(",");
            var greaterThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0));
            var callExpr = Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(","));
            loopBodies.Add(Expression.IfThen(greaterThenExpr, callExpr));

            //builder.Append($"{ormProider.GetFieldName(itemKey)}={parameterName}");
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetFieldName));
            var fieldNameExpr = Expression.Call(ormProviderExpr, methodInfo, itemKeyExpr);
            loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
            loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant("=")));
            loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));

            //ormProvider.AddDbParameter(dbKey, dbParameters, memberMapper, parameterName, fieldValue);
            methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.AddDbParameter));
            loopBodies.Add(Expression.Call(methodInfo, ormProviderExpr, Expression.Constant(dbKey),
                dbParametersExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr));

            //index++;
            loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));

            blockBodies.Add(Expression.Loop(Expression.Block(loopBodies), breakLabel, continueLabel));
        }
        else
        {
            var typedUpdateObjExpr = Expression.Parameter(updateObjType, "typeUpdateObj");
            blockBodies.Add(Expression.Assign(typedUpdateObjExpr, Expression.Convert(updateObjExpr, updateObjType)));
            var memberInfos = updateObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

            var index = 0;
            foreach (var memberInfo in memberInfos)
            {
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                    || memberMapper.IsIgnore || memberMapper.IsNavigation
                    || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                    continue;

                if (ignoreFieldNames != null && ignoreFieldNames.Contains((memberInfo.Name)))
                    continue;
                if (onlyFieldNames != null && !onlyFieldNames.Contains((memberInfo.Name)))
                    continue;

                if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(",")));

                Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix + memberMapper.MemberName);
                if (hasSuffix)
                {
                    myParameterNameExpr = Expression.Call(concatMethodInfo1, myParameterNameExpr, suffixExpr);
                    blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                    myParameterNameExpr = parameterNameExpr;
                }
                var fieldNameExpr = Expression.Constant(ormProvider.GetFieldName(memberMapper.MemberName) + "=");
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, myParameterNameExpr));

                var fieldValueExpr = Expression.PropertyOrField(typedUpdateObjExpr, memberMapper.MemberName);
                AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                index++;
            }
        }

        object result = null;
        if (hasSuffix) result = Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>>(
            Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, builderExpr, updateObjExpr, suffixExpr).Compile();
        else result = Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, StringBuilder, object>>(
            Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, builderExpr, updateObjExpr).Compile();
        return result;
    }
    public static object BuildUpdateSetWithPartSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type updateObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool hasSuffix)
    {
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, updateObjType, onlyFieldNames, ignoreFieldNames);
        var commandInitializerCache = hasSuffix ? updateMultiWithCommandInitializerCache : updateWithCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var updateFieldsExpr = Expression.Parameter(typeof(List<FieldsSegment>), "updateFields");
            var whereFieldsExpr = Expression.Parameter(typeof(List<FieldsSegment>), "whereFields");
            var updateObjExpr = Expression.Parameter(typeof(object), "updateObj");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            ParameterExpression suffixExpr = null;
            ParameterExpression parameterNameExpr = null;
            if (hasSuffix)
            {
                suffixExpr = Expression.Parameter(typeof(string), "suffix");
                parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                blockParameters.Add(parameterNameExpr);
            }
            MethodInfo methodInfo = null;
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var concatMethodInfo1 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
            var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });
            var setFieldsMethodInfo = typeof(FieldsSegment).GetProperty(nameof(FieldsSegment.Fields)).GetSetMethod();
            var setValuesMethodInfo = typeof(FieldsSegment).GetProperty(nameof(FieldsSegment.Values)).GetSetMethod();
            var addMethodInfo = typeof(List<FieldsSegment>).GetMethod(nameof(List<FieldsSegment>.Add));

            if (typeof(IDictionary<string, object>).IsAssignableFrom(updateObjType))
            {
                var dictExpr = Expression.Variable(typeof(IDictionary<string, object>), "dict");
                var fieldValueExpr = Expression.Variable(typeof(object), "fieldValue");
                blockParameters.AddRange(new[] { dictExpr, fieldValueExpr });
                blockBodies.Add(Expression.Assign(dictExpr, Expression.Convert(updateObjExpr, typeof(IDictionary<string, object>))));

                var indexExpr = Expression.Variable(typeof(int), "index");
                var enumeratorExpr = Expression.Variable(typeof(IEnumerable<KeyValuePair<string, object>>), "enumerator");
                var itemKeyExpr = Expression.Variable(typeof(string), "itemKey");
                var memberMapperExpr = Expression.Variable(typeof(MemberMap), "memberMapper");
                var outTypeExpr = Expression.Variable(typeof(Type), "outType");
                blockParameters.AddRange(new[] { indexExpr, enumeratorExpr, itemKeyExpr, fieldValueExpr, memberMapperExpr, outTypeExpr });
                var breakLabel = Expression.Label();
                var continueLabel = Expression.Label();

                //if(!enumerator.MoveNext())
                //  break;
                var loopBodies = new List<Expression>();
                methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
                loopBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(dictExpr, methodInfo)));
                methodInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
                var ifFalseExpr = Expression.IsFalse(Expression.Call(enumeratorExpr, methodInfo));
                var ifThenExpr = Expression.IfThen(ifFalseExpr, Expression.Break(breakLabel));

                //var entityMapper = new EntityMap{ ... };
                //var itemKey = enumerator.Current.Key;
                //var fieldValue = enumerator.Current.Value;
                var entityMapperExpr = Expression.Constant(entityMapper);
                var currentExpr = Expression.Property(enumeratorExpr, nameof(IEnumerator.Current));
                loopBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
                loopBodies.Add(Expression.Assign(itemKeyExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Key))));
                loopBodies.Add(Expression.Assign(fieldValueExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Value))));

                //var isContinue = !entityMapper.TryGetMemberMap(itemKey, out var memberMapper)
                //|| memberMapper.IsIgnore || memberMapper.IsNavigation
                methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.TryGetMemberMap));
                Expression isContinueExpr = Expression.IsFalse(Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr, memberMapperExpr));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnore)));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsNavigation)));
                //|| memberMapper.IsKey
                //if (!isWhereKey) isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsKey)));

                //|| ignoreFields.Constains(itemKey) || !onlyFields.Constains(itemKey)
                if (ignoreFieldNames != null)
                {
                    var ignoreFieldsExpr = Expression.Constant(ignoreFieldNames);
                    methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), new Type[] { typeof(string) });
                    isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Call(methodInfo, ignoreFieldsExpr, itemKeyExpr));
                }
                if (onlyFieldNames != null)
                {
                    var onlyFieldsExpr = Expression.Constant(onlyFieldNames);
                    methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), new Type[] { typeof(string) });
                    var isFalseExpr = Expression.IsFalse(Expression.Call(methodInfo, onlyFieldsExpr, itemKeyExpr));
                    isContinueExpr = Expression.OrElse(isContinueExpr, isFalseExpr);
                }

                //|| (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.IsEntityType));
                var memberTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.MemberType));
                var isEntityTypeExpr = Expression.Call(methodInfo, memberTypeExpr, outTypeExpr);
                var isNullExpr = Expression.Equal(Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler)), Expression.Constant(null));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.AndAlso(isEntityTypeExpr, isNullExpr));
                //if(isContinue)continue;
                loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

                var fieldNameExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.FieldName));
                var getFieldNameMethodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetFieldName));

                //var parameterName = ormProvider.ParameterPrefix + itemKey + multiMark;
                Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix);
                if (hasSuffix)
                    myParameterNameExpr = Expression.Call(concatMethodInfo2, myParameterNameExpr, itemKeyExpr, suffixExpr);
                else myParameterNameExpr = Expression.Call(concatMethodInfo1, myParameterNameExpr, itemKeyExpr);
                blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));

                //if (isWhereKey)
                //{
                //    //var fieldsSegment = new FieldsSegment { Fields = ormProvider.GetFieldName(memberMapper.FieldName), Values = parameterName }
                //    //if(memberMapper.IsKey) whereFields.Add(fieldsSegment);
                //    //else updateFields.Add(fieldsSegment);
                //    var fieldsSegmentExpr = Expression.Variable(typeof(FieldsSegment));
                //    blockParameters.Add(fieldsSegmentExpr);
                //    var fieldsNameExpr = Expression.Call(ormProviderExpr, getFieldNameMethodInfo, fieldNameExpr);
                //    loopBodies.Add(Expression.Call(fieldsSegmentExpr, setFieldsMethodInfo, fieldsNameExpr));
                //    loopBodies.Add(Expression.Call(fieldsSegmentExpr, setValuesMethodInfo, parameterNameExpr));
                //    var ifAddExpr = Expression.Call(whereFieldsExpr, addMethodInfo, fieldsSegmentExpr);
                //    var elseAddExpr = Expression.Call(updateFieldsExpr, addMethodInfo, fieldsSegmentExpr);

                //    var isKeyExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.IsKey));
                //    loopBodies.Add(Expression.IfThenElse(isKeyExpr, ifAddExpr, elseAddExpr));
                //}
                //else
                //{
                //updateFields.Add(new FieldsSegment { Fields = ormProvider.GetFieldName(memberMapper.FieldName), Values = parameterName });
                var fieldsExpr = Expression.Call(ormProviderExpr, methodInfo, fieldNameExpr);
                var fieldsSegmentExpr = Expression.Variable(typeof(FieldsSegment));
                blockParameters.Add(fieldsSegmentExpr);

                var fieldsNameExpr = Expression.Call(ormProviderExpr, getFieldNameMethodInfo, fieldNameExpr);
                loopBodies.Add(Expression.Call(fieldsSegmentExpr, setFieldsMethodInfo, fieldsNameExpr));
                loopBodies.Add(Expression.Call(fieldsSegmentExpr, setValuesMethodInfo, parameterNameExpr));
                loopBodies.Add(Expression.Call(updateFieldsExpr, addMethodInfo, fieldsSegmentExpr));
                //}

                //ormProvider.AddDbParameter(dbKey, dbParameters, memberMapper, parameterName, fieldValue);
                methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.AddDbParameter));
                loopBodies.Add(Expression.Call(methodInfo, ormProviderExpr, Expression.Constant(dbKey),
                    dbParametersExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr));

                //index++;
                loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));

                blockBodies.Add(Expression.Loop(Expression.Block(loopBodies), breakLabel, continueLabel));
            }
            else
            {
                var typedUpdateObjExpr = Expression.Parameter(updateObjType, "typeUpdateObj");
                blockBodies.Add(Expression.Assign(typedUpdateObjExpr, Expression.Convert(updateObjExpr, updateObjType)));
                var memberInfos = updateObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

                var index = 0;
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;
                    //if (!isWhereKey && memberMapper.IsKey) continue;
                    if (ignoreFieldNames != null && ignoreFieldNames.Contains((memberInfo.Name)))
                        continue;
                    if (onlyFieldNames != null && !onlyFieldNames.Contains((memberInfo.Name)))
                        continue;

                    if (index > 0) blockBodies.Add(Expression.Call(updateFieldsExpr, appendMethodInfo, Expression.Constant(",")));

                    var parameterName = ormProvider.ParameterPrefix + memberMapper.MemberName;
                    Expression myParameterNameExpr = Expression.Constant(parameterName);
                    Expression fieldsSegmentExpr = null;
                    if (hasSuffix)
                    {
                        myParameterNameExpr = Expression.Call(concatMethodInfo1, myParameterNameExpr, suffixExpr);
                        blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                        myParameterNameExpr = parameterNameExpr;

                        fieldsSegmentExpr = Expression.Variable(typeof(FieldsSegment));
                        var fieldNameExpr = Expression.Constant(ormProvider.GetFieldName(memberMapper.FieldName));
                        blockBodies.Add(Expression.Call(fieldsSegmentExpr, setFieldsMethodInfo, fieldNameExpr));
                        blockBodies.Add(Expression.Call(fieldsSegmentExpr, setValuesMethodInfo, parameterNameExpr));
                    }
                    else fieldsSegmentExpr = Expression.Constant(new FieldsSegment
                    {
                        Fields = ormProvider.GetFieldName(memberMapper.FieldName),
                        Values = parameterName
                    });
                    //if (isWhereKey && memberMapper.IsKey)
                    //    blockBodies.Add(Expression.Call(whereFieldsExpr, addMethodInfo, fieldsSegmentExpr));
                    //else
                    blockBodies.Add(Expression.Call(updateFieldsExpr, addMethodInfo, fieldsSegmentExpr));

                    var fieldValueExpr = Expression.PropertyOrField(typedUpdateObjExpr, memberMapper.MemberName);
                    AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                    index++;
                }
            }

            object result = null;
            if (hasSuffix) result = Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, List<FieldsSegment>, List<FieldsSegment>, object, string>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, updateFieldsExpr, whereFieldsExpr, updateObjExpr, suffixExpr).Compile();
            else result = Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, List<FieldsSegment>, List<FieldsSegment>, object>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, updateFieldsExpr, whereFieldsExpr, updateObjExpr).Compile();
            return result;
        });
    }
    //public static object BuildUpdateWithParameters(ISqlVisitor sqlVisitor, Type entityType, object updateObj, bool isWhere, bool isMultiExecute, bool isAnonymousParameter = false)
    //{
    //    object commandInitializer = null;
    //    if (updateObj is IDictionary<string, object>)
    //    {
    //        if (isMultiExecute)
    //        {
    //            Action<IDataParameterCollection, StringBuilder, object, string> typedCommandInitializer = null;
    //            if (isWhere) typedCommandInitializer = (dbParameters, builder, updateObj, multiMark) =>
    //            {
    //                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
    //                var dict = updateObj as IDictionary<string, object>;
    //                foreach (var item in dict)
    //                {
    //                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
    //                        || memberMapper.IsIgnore || memberMapper.IsNavigation
    //                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
    //                        continue;

    //                    var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}k{item.Key}{multiMark}";
    //                    if (builder.Length > 0) builder.Append(',');
    //                    builder.Append($"{sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
    //                    sqlVisitor.OrmProvider.AddDbParameter(sqlVisitor.DbKey, dbParameters, memberMapper, parameterName, item.Value);
    //                }
    //            };
    //            else typedCommandInitializer = (dbParameters, builder, updateObj, multiMark) =>
    //            {
    //                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
    //                var dict = updateObj as IDictionary<string, object>;
    //                foreach (var item in dict)
    //                {
    //                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
    //                        || memberMapper.IsIgnore || memberMapper.IsNavigation
    //                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
    //                        continue;

    //                    var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}{item.Key}{multiMark}";
    //                    if (builder.Length > 0) builder.Append(',');
    //                    builder.Append($"{sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
    //                    sqlVisitor.OrmProvider.AddDbParameter(sqlVisitor.DbKey, dbParameters, memberMapper, parameterName, item.Value);
    //                }
    //            };
    //            commandInitializer = typedCommandInitializer;
    //        }
    //        else
    //        {
    //            Action<IDataParameterCollection, StringBuilder, object> typedCommandInitializer = null;
    //            if (isWhere) typedCommandInitializer = (dbParameters, builder, updateObj) =>
    //            {
    //                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
    //                var dict = updateObj as IDictionary<string, object>;
    //                foreach (var item in dict)
    //                {
    //                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
    //                        || memberMapper.IsIgnore || memberMapper.IsNavigation
    //                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
    //                        continue;

    //                    var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}k{item.Key}";
    //                    if (builder.Length > 0) builder.Append(',');
    //                    builder.Append($"{sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
    //                    sqlVisitor.OrmProvider.AddDbParameter(sqlVisitor.DbKey, dbParameters, memberMapper, parameterName, item.Value);
    //                }
    //            };
    //            else typedCommandInitializer = (dbParameters, builder, updateObj) =>
    //            {
    //                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
    //                var dict = updateObj as IDictionary<string, object>;
    //                foreach (var item in dict)
    //                {
    //                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
    //                        || memberMapper.IsIgnore || memberMapper.IsNavigation
    //                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
    //                        continue;

    //                    var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}{item.Key}";
    //                    if (builder.Length > 0) builder.Append(',');
    //                    builder.Append($"{sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
    //                    sqlVisitor.OrmProvider.AddDbParameter(sqlVisitor.DbKey, dbParameters, memberMapper, parameterName, item.Value);
    //                }
    //            };
    //            commandInitializer = typedCommandInitializer;
    //        }
    //    }
    //    else
    //    {
    //        var parameterType = updateObj.GetType();
    //        var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
    //        var dbParametersInitializerCache = isMultiExecute ? updateMultiSetFieldsCommandInitializerCache : updateSetFieldsCommandInitializerCache;
    //        if (!dbParametersInitializerCache.TryGetValue(cacheKey, out commandInitializer))
    //        {
    //            var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
    //            var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
    //                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
    //            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
    //            var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
    //            var parameterExpr = Expression.Parameter(typeof(object), "parameter");
    //            ParameterExpression multiMarkExpr = null;
    //            if (isMultiExecute) multiMarkExpr = Expression.Parameter(typeof(string), "multiMark");

    //            var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
    //            var blockParameters = new List<ParameterExpression>();
    //            var blockBodies = new List<Expression>();

    //            blockParameters.Add(typedParameterExpr);
    //            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
    //            var ormProviderExpr = Expression.Constant(sqlVisitor.OrmProvider);

    //            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
    //            var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
    //            foreach (var memberInfo in memberInfos)
    //            {
    //                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
    //                    || memberMapper.IsIgnore || memberMapper.IsNavigation
    //                    || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
    //                    continue;

    //                Expression parameterNameExpr = null;
    //                var memberMapperExpr = Expression.Constant(memberMapper);
    //                if (isWhere) parameterNameExpr = Expression.Constant($"{sqlVisitor.OrmProvider.ParameterPrefix}k{memberInfo.Name}");
    //                else parameterNameExpr = Expression.Constant($"{sqlVisitor.OrmProvider.ParameterPrefix}{memberInfo.Name}");
    //                if (isMultiExecute) parameterNameExpr = Expression.Call(concatMethodInfo, parameterNameExpr, multiMarkExpr);

    //                var fieldNameExpr = Expression.Constant(sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName) + "=");
    //                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
    //                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
    //                var fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
    //                AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
    //            }

    //            if (isMultiExecute) commandInitializer = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, object, string>>(
    //                Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, parameterExpr, multiMarkExpr).Compile();
    //            else commandInitializer = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, object>>(
    //                Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, parameterExpr).Compile();
    //            dbParametersInitializerCache.TryAdd(cacheKey, commandInitializer);
    //        }
    //    }
    //    return commandInitializer;
    //}


    public static object BuildDeleteCommandInitializer(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiple)
    {
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, whereObjType);
        var commandInitializerCache = isMultiple ? deleteMultiCommandInitializerCache : deleteCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var headSql = $"DELETE FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ";
            return BuildWhereSqlParameters(dbKey, ormProvider, mapProvider, entityType, whereObjType, true, isMultiple, false, "whereObj", headSql);
        });
    }
    public static (bool, string, object) BuildDeleteBulkCommandInitializer(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObjs, bool isMultiple)
    {
        object whereObj = null;
        var enumerable = whereObjs as IEnumerable;
        foreach (var item in enumerable)
        {
            whereObj = item;
            break;
        }
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, whereObjType);
        var entityMapper = mapProvider.GetEntityMap(entityType);
        var isMultiKeys = entityMapper.KeyMembers.Count > 1;
        var commandInitializerCache = isMultiple ? deleteMultiBulkCommandInitializerCache : deleteBulkCommandInitializerCache;
        var commandInitializer = commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var bulkHeadSql = $"DELETE FROM {ormProvider.GetFieldName(entityMapper.TableName)} WHERE ";
            return BuildBulkWhereKeySqlParameters(dbKey, ormProvider, mapProvider, entityType, whereObj, isMultiple, bulkHeadSql);
        });
        string headSql = null;
        if (!isMultiKeys) headSql = $"DELETE FROM {ormProvider.GetFieldName(entityMapper.TableName)} WHERE {ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)} IN (";
        return (isMultiKeys, headSql, commandInitializer);
    }
    public static object BuildBulkWhereKeySqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereKeys, bool isMultiple, string bulkHeadSql)
    {
        object commandInitializer = null;
        var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
        var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
        var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
        var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");
        var bulkIndexExpr = Expression.Parameter(typeof(int), "bulkIndex");

        ParameterExpression multiMarkExpr = null;
        ParameterExpression dictExpr = null;
        ParameterExpression typedWhereObjExpr = null;
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();

        var fieldValueExpr = Expression.Variable(typeof(object), "fieldValue");
        var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
        blockParameters.AddRange(new[] { fieldValueExpr, parameterNameExpr });
        var methodInfo = typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes);
        var strBulkIndexExpr = Expression.Call(bulkIndexExpr, methodInfo);
        bool isEntityType = false;
        Type whereObjType = null;
        List<MemberInfo> memberInfos = null;
        var entityMapper = mapProvider.GetEntityMap(entityType);
        var isMultiKeys = entityMapper.KeyMembers.Count > 1;
        var isDictionary = whereKeys is IDictionary<string, object>;

        if (isDictionary)
        {
            dictExpr = Expression.Variable(typeof(IDictionary<string, object>), "dict");
            blockParameters.Add(dictExpr);
        }
        else
        {
            whereObjType = whereKeys.GetType();
            if (whereObjType.IsEntityType(out _))
            {
                isEntityType = true;
                typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");
                blockParameters.Add(typedWhereObjExpr);
                blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));
                memberInfos = whereObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                   .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
            }
            else
            {
                if (entityMapper.KeyMembers.Count > 1)
                    throw new NotSupportedException($"模型{entityType.FullName}有多个主键字段，不能使用单个值类型{whereObjType.FullName}作为参数");
            }
        }
        if (isMultiple)
        {
            multiMarkExpr = Expression.Parameter(typeof(string), "multiMark");
            blockParameters.Add(parameterNameExpr);
        }
        var index = 0;
        var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
        var concatMethodInfo1 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
        var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });
        var tryGetValueMethodInfo = typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.TryGetValue));

        foreach (var keyMapper in entityMapper.KeyMembers)
        {
            Expression myFieldValueExpr = null;
            var keyMemberExpr = Expression.Constant(keyMapper.MemberName);
            if (isDictionary)
            {
                var isFalseExpr = Expression.IsFalse(Expression.Call(dictExpr, tryGetValueMethodInfo, keyMemberExpr, fieldValueExpr));
                var exceptionExpr = Expression.Constant(new ArgumentNullException(nameof(whereKeys), $"字典参数缺少主键字段{keyMapper.MemberName}"));
                blockBodies.Add(Expression.IfThen(isFalseExpr, Expression.Throw(exceptionExpr, typeof(ArgumentNullException))));
                myFieldValueExpr = fieldValueExpr;
            }
            else
            {
                if (isEntityType)
                {
                    if (!memberInfos.Exists(f => f.Name == keyMapper.MemberName))
                        throw new ArgumentNullException("whereObj", $"参数类型{whereObjType.FullName}缺少主键字段{keyMapper.MemberName}");
                    myFieldValueExpr = Expression.PropertyOrField(typedWhereObjExpr, keyMapper.MemberName);
                }
                else myFieldValueExpr = whereObjExpr;
            }

            var parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName;
            Expression myParameterNameExpr = Expression.Constant(parameterName);
            if (isMultiple)
                myParameterNameExpr = Expression.Call(concatMethodInfo2, myParameterNameExpr, multiMarkExpr, strBulkIndexExpr);
            else myParameterNameExpr = Expression.Call(concatMethodInfo1, myParameterNameExpr, strBulkIndexExpr);
            blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));

            if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
            if (isMultiKeys)
            {
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(bulkHeadSql)));
                var assignExpr = Expression.Constant($"{ormProvider.GetFieldName(keyMapper.FieldName)}=");
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, assignExpr));
            }
            blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
            AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, myFieldValueExpr, keyMapper, blockParameters, blockBodies);
            index++;
        }
        if (isMultiple) commandInitializer = Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string, int>>(
            Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, builderExpr, whereObjExpr, multiMarkExpr, bulkIndexExpr).Compile();
        else commandInitializer = Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, int>>(
            Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, builderExpr, whereObjExpr, bulkIndexExpr).Compile();
        return commandInitializer;
    }
}