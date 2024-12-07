using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class RepositoryHelper
{
    private static readonly ConcurrentDictionary<int, object> queryWhereObjCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> queryBulkWhereObjCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> queryMultiWhereObjCommandInitializerCache = new();

    private static readonly ConcurrentDictionary<int, object> queryWhereObjByKeyCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> queryBulkWhereObjByKeyCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> queryMultiWhereObjByKeyCommandInitializerCache = new();

    private static readonly ConcurrentDictionary<int, object> queryExistsCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> queryBulkExistsCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> queryMultiExistsCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, Action<IDataParameterCollection, IOrmProvider, object>> queryRawSqlCommandInitializerCache = new();

    private static readonly ConcurrentDictionary<int, object> fieldsSqlParameterPartCache = new();

    private static readonly ConcurrentDictionary<int, Action<DbContext, ITheaCommand, object>> createCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<DbContext, ITheaCommand, IEnumerable, int, int>> createBulkCommandExecutorCache = new();
    private static readonly ConcurrentDictionary<int, Func<DbContext, ITheaCommand, IEnumerable, int, CancellationToken, Task<int>>> createBulkAsyncCommandExecutorCache = new();
    private static readonly ConcurrentDictionary<int, Action<StringBuilder, DbContext, object>> createFieldsSqlCache = new();
    private static readonly ConcurrentDictionary<int, object> createValuesSqlParametersCache = new();
    private static readonly ConcurrentDictionary<int, object> createBulkValuesSqlParametersCache = new();

    private static readonly ConcurrentDictionary<int, (bool, string, Action<StringBuilder, string>, object)> deleteCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, (bool, string, Action<StringBuilder, string>, object)> deleteMultiCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, (bool, string, Action<StringBuilder, string>, object)> deleteBulkCommandInitializerCache = new();

    private static readonly ConcurrentDictionary<int, object> updateCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> updateMultiCommandInitializerCache = new();

    private static readonly ConcurrentDictionary<int, object> updateWithCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> updateMultiWithCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, (Action<IDataParameterCollection, StringBuilder, DbContext, object, string>, Action<StringBuilder, DbContext, object, string>)> updateBulkWithCommandInitializerCache = new();

    private static readonly ConcurrentDictionary<int, Func<string, object, string>> shardingTableNameGetters = new();
    private static readonly ConcurrentDictionary<Type, Func<object>> typedListGetters = new();
    private static readonly ConcurrentDictionary<Type, Func<object, object>> typedInitListGetters = new();
    private static readonly ConcurrentDictionary<Type, Func<object, object>> typedCollectionGetters = new();
    private static readonly ConcurrentDictionary<Type, Func<object, object>> toArrayGetters = new();

    public static void AddValueParameter(DbContext dbContext, Expression dbParametersExpr, Expression ormProviderExpr,
        Expression parameterNameExpr, Type fieldValueType, Expression fieldValueExpr, MemberMap memberMapper, List<Expression> blockBodies)
    {
        MethodInfo methodInfo = null;
        var parameterValueExpr = fieldValueExpr;
        var addMethodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add));
        var createParameterMethodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), [typeof(string), typeof(object), typeof(object)]);

        if (memberMapper.TypeHandler != null)
        {
            var typeHandlerExpr = Expression.Constant(memberMapper.TypeHandler);
            methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
            if (fieldValueExpr.Type != typeof(object))
                parameterValueExpr = Expression.Convert(parameterValueExpr, typeof(object));
            parameterValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, parameterValueExpr);
        }
        else
        {
            var ormProvider = dbContext.OrmProvider;
            //数据库类型
            var targetType = ormProvider.MapDefaultType(memberMapper);
            var valueGetter = ormProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, dbContext.Options);
            if (fieldValueExpr.Type != typeof(object))
                parameterValueExpr = Expression.Convert(parameterValueExpr, typeof(object));
            parameterValueExpr = Expression.Invoke(Expression.Constant(valueGetter), parameterValueExpr);
        }

        Expression nativeDbTypeExpr = Expression.Constant(memberMapper.NativeDbType);
        if (nativeDbTypeExpr.Type != typeof(object))
            nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));
        var dbParameterExpr = Expression.Call(ormProviderExpr, createParameterMethodInfo, parameterNameExpr, nativeDbTypeExpr, parameterValueExpr);
        blockBodies.Add(Expression.Call(dbParametersExpr, addMethodInfo, dbParameterExpr));
    }
    public static void AddValueParameter(DbContext dbContext, Expression dbParametersExpr, Expression ormProviderExpr,
        Expression parameterNameExpr, Expression fieldValueExpr, Expression memberMapperExpr, List<Expression> blockBodies)
    {
        var typeHandlerExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler));
        var methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
        Expression boxedFieldValueExpr = fieldValueExpr;
        if (fieldValueExpr.Type != typeof(object))
            boxedFieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
        var typeHandlerValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, boxedFieldValueExpr);

        methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.MapDefaultType), [typeof(MemberMap)]);
        var targetTypeExpr = Expression.Call(ormProviderExpr, methodInfo, memberMapperExpr);
        methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetParameterValueGetter));

        var optionsExpr = Expression.Constant(dbContext.Options);
        var fieldValueTypeExpr = Expression.Call(fieldValueExpr, typeof(object).GetMethod(nameof(object.GetType)));
        var valueGetterExpr = Expression.Call(ormProviderExpr, methodInfo, fieldValueTypeExpr, targetTypeExpr, Expression.Constant(false), optionsExpr);
        var valueGetterValueExpr = Expression.Invoke(valueGetterExpr, boxedFieldValueExpr);

        //dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue);
        methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), [typeof(string), typeof(object), typeof(object)]);
        var nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
        var typeHandlerDbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, typeHandlerValueExpr);
        var valueGetterDbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, valueGetterValueExpr);

        methodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add), [typeof(object)]);
        var typeHandlerAddParametersExpr = Expression.Call(dbParametersExpr, methodInfo, typeHandlerDbParameterExpr);
        var valueGetterAddParametersExpr = Expression.Call(dbParametersExpr, methodInfo, valueGetterDbParameterExpr);

        var isNotNullExpr = Expression.IsFalse(Expression.Equal(typeHandlerExpr, Expression.Constant(null)));
        blockBodies.Add(Expression.IfThenElse(isNotNullExpr, typeHandlerAddParametersExpr, valueGetterAddParametersExpr));
    }
    public static void AddValueParameter(Expression dbParametersExpr, Expression ormProviderExpr, Expression parameterNameExpr, Expression parameterValueExpr, List<Expression> blockBodies)
    {
        var fieldValueExpr = parameterValueExpr;
        var fieldValueType = parameterValueExpr.Type;
        var addMethodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add));
        bool isNullableType = fieldValueType.IsNullableType(out var underlyingType);
        if (underlyingType.IsEnumType(out _, out var enumUnderlyingType))
            fieldValueExpr = Expression.Convert(fieldValueExpr, enumUnderlyingType);
        if (fieldValueExpr.Type != typeof(object))
            fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));

        var methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), [typeof(string), typeof(object)]);
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
    public static string BuildSelectFieldsSqlPart(IOrmProvider ormProvider, EntityMap entityMapper, Type parametersType)
    {
        var builder = new StringBuilder();
        var memberInfos = parametersType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
           .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

        var index = 0;
        foreach (var memberInfo in memberInfos)
        {
            if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                || memberMapper.IsIgnore || memberMapper.IsNavigation)
                continue;

            if (index > 0) builder.Append(',');
            builder.Append(ormProvider.GetFieldName(memberMapper.FieldName));
            if (memberMapper.FieldName != memberMapper.MemberName)
                builder.Append(" AS " + ormProvider.GetFieldName(memberMapper.MemberName));
            index++;
        }
        return builder.ToString();
    }
    //public static (bool, object) BuildSqlParametersPart(DbContext dbContext, Type entityType, Type parametersType, bool isUpdate, bool isFunc, bool isOnlySql, bool isUseKey, bool isWithKey, bool isOnlyParameters, bool hasSuffix, bool isIgnoreKeys, List<string> onlyFieldNames, List<string> ignoreFieldNames, string jointMark, string headSql)
    //{
    //    object commandInitializer = null;
    //    var ormProvider = dbContext.OrmProvider;
    //    var mapProvider = dbContext.MapProvider;
    //    var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
    //    var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
    //    var dbContextExpr = Expression.Parameter(typeof(DbContext), "dbContext");
    //    var parametersExpr = Expression.Parameter(typeof(object), "parameters");

    //    ParameterExpression entityMapperExpr = null;
    //    ParameterExpression suffixExpr = null;
    //    var ormProviderExpr = Expression.Variable(typeof(IOrmProvider), "ormProvider");

    //    var blockParameters = new List<ParameterExpression>();
    //    var blockBodies = new List<Expression>();
    //    MethodInfo methodInfo = null;
    //    var entityMapper = mapProvider.GetEntityMap(entityType);
    //    if (hasSuffix) suffixExpr = Expression.Parameter(typeof(string), "suffix");
    //    if (isFunc)
    //    {
    //        blockParameters.Add(builderExpr);
    //        var constructorExpr = typeof(StringBuilder).GetConstructor(Type.EmptyTypes);
    //        blockBodies.Add(Expression.Assign(builderExpr, Expression.New(constructorExpr)));
    //    }
    //    var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), [typeof(string)]);
    //    if (!string.IsNullOrEmpty(headSql))
    //        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(headSql)));
    //    blockParameters.Add(ormProviderExpr);
    //    blockBodies.Add(Expression.Assign(ormProviderExpr, Expression.Property(dbContextExpr, nameof(DbContext.OrmProvider))));

    //    bool isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(parametersType);
    //    if (isDictionary)
    //    {
    //        entityMapperExpr = Expression.Parameter(typeof(EntityMap), "entityMapper");
    //        var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
    //        var dictExpr = Expression.Variable(typeof(IDictionary<string, object>), "dict");
    //        var fieldValueExpr = Expression.Variable(typeof(object), "fieldValue");
    //        blockParameters.AddRange([dictExpr, fieldValueExpr, parameterNameExpr]);
    //        blockBodies.Add(Expression.Assign(dictExpr, Expression.Convert(parametersExpr, typeof(IDictionary<string, object>))));

    //        var index = 0;
    //        var concatMethodInfo1 = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)]);
    //        var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string), typeof(string)]);
    //        if (isUseKey)
    //        {
    //            var tryGetValueMethodInfo = typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.TryGetValue));
    //            foreach (var keyMapper in entityMapper.KeyMembers)
    //            {
    //                if (onlyFieldNames != null && !onlyFieldNames.Contains(keyMapper.MemberName))
    //                    continue;
    //                if (ignoreFieldNames != null && ignoreFieldNames.Contains(keyMapper.MemberName))
    //                    continue;

    //                var keyMemberExpr = Expression.Constant(keyMapper.MemberName);
    //                var hasNotKeyExpr = Expression.IsFalse(Expression.Call(dictExpr, containsKeyMethodInfo, keyMemberExpr));
    //                var exceptionExpr = Expression.Constant(new Exception($"字典参数缺少主键字段{keyMapper.MemberName}，区分大小写"));
    //                blockBodies.Add(Expression.IfThen(hasNotKeyExpr, Expression.Throw(exceptionExpr)));
    //                blockBodies.Add(Expression.Assign(fieldValueExpr, Expression.Property(dictExpr, dictItemPropertyInfo)));

    //                var parameterName = ormProvider.ParameterPrefix + (isWithKey ? "k" : "") + keyMapper.MemberName;
    //                Expression myParameterNameExpr = Expression.Constant(parameterName);
    //                if (hasSuffix)
    //                {
    //                    myParameterNameExpr = Expression.Call(concatMethodInfo1, myParameterNameExpr, suffixExpr);
    //                    blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
    //                    myParameterNameExpr = parameterNameExpr;
    //                }
    //                if (!isOnlyParameters)
    //                {
    //                    if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(jointMark)));
    //                    var fieldNameExpr = Expression.Constant($"{ormProvider.GetFieldName(keyMapper.FieldName)}=");
    //                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
    //                }

    //                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, myParameterNameExpr));
    //                if (!isOnlySql)
    //                {
    //                    var typedFieldValueExpr = Expression.Convert(fieldValueExpr, keyMapper.MemberType);
    //                    AddValueParameter(dbContext, dbParametersExpr, ormProviderExpr, parameterNameExpr, keyMapper.MemberType, typedFieldValueExpr, keyMapper, blockParameters, blockBodies);
    //                }
    //                index++;
    //            }
    //        }
    //        else
    //        {
    //            var indexExpr = Expression.Variable(typeof(int), "index");
    //            var enumeratorExpr = Expression.Variable(typeof(IEnumerable<KeyValuePair<string, object>>), "enumerator");
    //            var itemKeyExpr = Expression.Variable(typeof(string), "itemKey");
    //            var memberMapperExpr = Expression.Variable(typeof(MemberMap), "memberMapper");
    //            var outTypeExpr = Expression.Variable(typeof(Type), "outType");
    //            blockParameters.AddRange(new[] { indexExpr, enumeratorExpr, itemKeyExpr, fieldValueExpr, memberMapperExpr, outTypeExpr });
    //            var breakLabel = Expression.Label();
    //            var continueLabel = Expression.Label();
    //            ParameterExpression dbFieldValueExpr = null;

    //            //var index = 0;
    //            //var enumerator = dict.GetEnumerator();
    //            blockBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
    //            methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
    //            blockBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(dictExpr, methodInfo)));

    //            //if(!enumerator.MoveNext())
    //            //  break;
    //            var loopBodies = new List<Expression>();
    //            methodInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
    //            var ifFalseExpr = Expression.IsFalse(Expression.Call(enumeratorExpr, methodInfo));
    //            loopBodies.Add(Expression.IfThen(ifFalseExpr, Expression.Break(breakLabel)));

    //            //var itemKey = enumerator.Current.Key;
    //            //var fieldValue = enumerator.Current.Value;          
    //            var currentExpr = Expression.Property(enumeratorExpr, nameof(IEnumerator.Current));
    //            loopBodies.Add(Expression.Assign(itemKeyExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Key))));
    //            loopBodies.Add(Expression.Assign(fieldValueExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Value))));

    //            //var isContinue = !entityMapper.TryGetMemberMap(itemKey, out var memberMapper)
    //            //|| memberMapper.IsIgnore || memberMapper.IsNavigation
    //            methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.TryGetMemberMap));
    //            Expression isContinueExpr = Expression.IsFalse(Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr, memberMapperExpr));
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnore)));
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsNavigation)));

    //            //|| memberMapper.IsRowVersion || memberMapper.IsIgnoreUpdate
    //            if (isUpdate)
    //            {
    //                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnoreUpdate)));
    //                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsRowVersion)));
    //            }

    //            //|| ignoreFields.Constains(itemKey) || !onlyFields.Constains(itemKey)
    //            if (ignoreFieldNames != null)
    //            {
    //                var ignoreFieldsExpr = Expression.Constant(ignoreFieldNames);
    //                methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), [typeof(string)]);
    //                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Call(methodInfo, ignoreFieldsExpr, itemKeyExpr));
    //            }
    //            if (onlyFieldNames != null)
    //            {
    //                var onlyFieldsExpr = Expression.Constant(onlyFieldNames);
    //                methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), [typeof(string)]);
    //                var isFalseExpr = Expression.IsFalse(Expression.Call(methodInfo, onlyFieldsExpr, itemKeyExpr));
    //                isContinueExpr = Expression.OrElse(isContinueExpr, isFalseExpr);
    //            }
    //            if (isIgnoreKeys)
    //            {
    //                var keyNames = entityMapper.KeyMembers.Select(f => f.MemberName).ToArray();
    //                var keyNamesExpr = Expression.Constant(keyNames);
    //                methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), [typeof(string)]);
    //                var isFalseExpr = Expression.IsFalse(Expression.Call(methodInfo, keyNamesExpr, itemKeyExpr));
    //                isContinueExpr = Expression.OrElse(isContinueExpr, isFalseExpr);
    //            }

    //            //|| (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
    //            //methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.IsEntityType));
    //            //var memberTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.MemberType));
    //            //var isEntityTypeExpr = Expression.Call(methodInfo, memberTypeExpr, outTypeExpr);
    //            //var isNullExpr = Expression.Equal(Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler)), Expression.Constant(null));
    //            //isContinueExpr = Expression.OrElse(isContinueExpr, Expression.AndAlso(isEntityTypeExpr, isNullExpr));
    //            //if(isContinue)continue;
    //            loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

    //            //var parameterName = ormProvider.ParameterPrefix + itemKey + suffix;
    //            Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix + (isWithKey ? "k" : ""));
    //            if (hasSuffix)
    //                myParameterNameExpr = Expression.Call(concatMethodInfo2, myParameterNameExpr, itemKeyExpr, suffixExpr);
    //            else myParameterNameExpr = Expression.Call(concatMethodInfo1, myParameterNameExpr, itemKeyExpr);
    //            loopBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));

    //            if (!isOnlyParameters)
    //            {
    //                //if(index > 0) builder.Append(" AND ");
    //                var greaterThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0));
    //                var callExpr = Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(jointMark));
    //                loopBodies.Add(Expression.IfThen(greaterThenExpr, callExpr));

    //                //builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
    //                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetFieldName));
    //                Expression fieldNameExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.FieldName));
    //                fieldNameExpr = Expression.Call(ormProviderExpr, methodInfo, fieldNameExpr);
    //                loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
    //                loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant("=")));
    //            }
    //            loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));

    //            if (!isOnlySql)
    //            {
    //                //object dbFieldValue=null;
    //                //if(memberMapper.TypeHandler!=null)
    //                //  dbFieldValue = memberMapper.TypeHandler.ToFieldValue(ormProvider, dbFieldValue);
    //                //else
    //                //{
    //                //    var targetType = this.OrmProvider.MapDefaultType(memberMapper);
    //                //    var valueGetter = this.OrmProvider.GetParameterValueGetter(sqlSegment.SegmentType, targetType, false, dbContext.Options);
    //                //    dbFieldValue = valueGetter.Invoke(dbFieldValue);
    //                //}
    //                var typeHandlerExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler));
    //                methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
    //                Expression myFieldValueExpr = fieldValueExpr;
    //                if (fieldValueExpr.Type != typeof(object))
    //                    myFieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
    //                var typeHandlerValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, myFieldValueExpr);

    //                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.MapDefaultType), [typeof(MemberMap)]);
    //                var targetTypeExpr = Expression.Call(ormProviderExpr, methodInfo, memberMapperExpr);
    //                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetParameterValueGetter));

    //                var fieldValueTypeExpr = Expression.Call(fieldValueExpr, typeof(object).GetMethod(nameof(object.GetType)));
    //                var optionsExpr = Expression.Property(dbContextExpr, nameof(DbContext.Options));
    //                var valueGetterExpr = Expression.Call(ormProviderExpr, methodInfo, fieldValueTypeExpr, targetTypeExpr, Expression.Constant(false), optionsExpr);
    //                var valueGetterValueExpr = Expression.Invoke(valueGetterExpr, myFieldValueExpr);

    //                if (dbFieldValueExpr == null)
    //                {
    //                    dbFieldValueExpr = Expression.Variable(typeof(object), "objValue");
    //                    blockParameters.Add(dbFieldValueExpr);
    //                }

    //                var isNotNullExpr = Expression.IsFalse(Expression.Equal(typeHandlerExpr, Expression.Constant(null)));
    //                var setTypeHandlerValueExpr = Expression.Assign(dbFieldValueExpr, typeHandlerValueExpr);
    //                var setValueGetterValueExpr = Expression.Assign(dbFieldValueExpr, valueGetterValueExpr);
    //                loopBodies.Add(Expression.IfThenElse(isNotNullExpr, setTypeHandlerValueExpr, setValueGetterValueExpr));

    //                //dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue);
    //                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), [typeof(string), typeof(object), typeof(object)]);
    //                var nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
    //                var dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, dbFieldValueExpr);
    //                methodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add), [typeof(object)]);
    //                blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr));
    //            }

    //            //index++;
    //            loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));

    //            blockBodies.Add(Expression.Loop(Expression.Block(loopBodies), breakLabel, continueLabel));
    //        }
    //    }
    //    else
    //    {
    //        ParameterExpression parameterNameExpr = null;
    //        ParameterExpression typedParametersExpr = null;
    //        bool isEntityType = false;
    //        List<MemberInfo> targetMemberInfos = null;
    //        List<MemberInfo> filterMemberInfos = null;
    //        if (hasSuffix)
    //        {
    //            parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
    //            blockParameters.Add(parameterNameExpr);
    //        }

    //        if (parametersType.IsEntityType(out _))
    //        {
    //            isEntityType = true;
    //            typedParametersExpr = Expression.Variable(parametersType, "typedParameters");
    //            blockParameters.Add(typedParametersExpr);
    //            blockBodies.Add(Expression.Assign(typedParametersExpr, Expression.Convert(parametersExpr, parametersType)));

    //            targetMemberInfos = parametersType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
    //                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
    //            if (isUseKey) filterMemberInfos = entityMapper.KeyMembers.Select(f => f.Member).ToList();
    //            else filterMemberInfos = targetMemberInfos;
    //        }
    //        else filterMemberInfos = entityMapper.KeyMembers.Select(f => f.Member).ToList();

    //        var index = 0;
    //        var keyNames = entityMapper.KeyMembers.Select(f => f.MemberName).ToArray();
    //        var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)]);
    //        foreach (var memberInfo in filterMemberInfos)
    //        {
    //            if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
    //                || memberMapper.IsIgnore || memberMapper.IsNavigation
    //                || (isUpdate && (memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)))
    //                continue;

    //            if (isUseKey && memberMapper.IsKey && isEntityType && !targetMemberInfos.Exists(f => f.Name == memberMapper.MemberName))
    //                throw new Exception($"参数类型{parametersType.FullName}缺少主键字段{memberMapper.MemberName}");
    //            if (isUseKey && !memberMapper.IsKey) continue;

    //            if (onlyFieldNames != null && !onlyFieldNames.Contains(memberInfo.Name))
    //                continue;
    //            if (ignoreFieldNames != null && ignoreFieldNames.Contains(memberInfo.Name))
    //                continue;
    //            if (isIgnoreKeys && keyNames.Contains(memberInfo.Name))
    //                continue;

    //            Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix + (isWithKey ? "k" : "") + memberMapper.MemberName);
    //            if (hasSuffix)
    //            {
    //                myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, suffixExpr);
    //                blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
    //                myParameterNameExpr = parameterNameExpr;
    //            }

    //            if (!isOnlyParameters)
    //            {
    //                if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(jointMark)));
    //                var fieldNameExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}=");
    //                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
    //            }
    //            blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, myParameterNameExpr));
    //            //isOnlySql: 只返回生成SQL, 不处理参数
    //            if (!isOnlySql)
    //            {
    //                if (isEntityType)
    //                {
    //                    var fieldValueExpr = Expression.PropertyOrField(typedParametersExpr, memberMapper.MemberName);
    //                    var fieldValueType = targetMemberInfos.Find(f => f.Name == memberMapper.MemberName).GetMemberType();
    //                    AddValueParameter(dbContext, dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueType, fieldValueExpr, memberMapper, blockBodies);
    //                }
    //                else
    //                {
    //                    var fieldValueExpr = Expression.Convert(parametersExpr, memberMapper.MemberType);
    //                    AddValueParameter(dbContext, dbParametersExpr, ormProviderExpr, myParameterNameExpr, memberMapper.MemberType, fieldValueExpr, memberMapper, blockBodies);
    //                }
    //            }
    //            index++;
    //        }
    //        if (index <= 0)
    //            throw new Exception("没有找到where条件或是忽略了字段大小写，无法识别到字段名称");
    //    }

    //    //isFunc通常是查询的where场景
    //    if (isFunc)
    //    {
    //        methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
    //        var returnExpr = Expression.Call(builderExpr, methodInfo);
    //        var resultLabelExpr = Expression.Label(typeof(string));
    //        blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
    //        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));

    //        if (isDictionary)
    //        {
    //            if (hasSuffix) commandInitializer = Expression.Lambda<Func<IDataParameterCollection, DbContext, EntityMap, object, string, string>>(
    //                Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, entityMapperExpr, parametersExpr, suffixExpr).Compile();
    //            else commandInitializer = Expression.Lambda<Func<IDataParameterCollection, DbContext, EntityMap, object, string>>(
    //                Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, entityMapperExpr, parametersExpr).Compile();
    //        }
    //        else
    //        {
    //            if (hasSuffix) commandInitializer = Expression.Lambda<Func<IDataParameterCollection, DbContext, object, string, string>>(
    //                Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, parametersExpr, suffixExpr).Compile();
    //            else commandInitializer = Expression.Lambda<Func<IDataParameterCollection, DbContext, object, string>>(
    //                Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, parametersExpr).Compile();
    //        }
    //    }
    //    else
    //    {
    //        //isOnlySql是Insert or update的update场景，参数在insert的时候已经设置过了
    //        if (isOnlySql)
    //        {
    //            if (isDictionary)
    //            {
    //                if (hasSuffix) commandInitializer = Expression.Lambda<Action<StringBuilder, DbContext, EntityMap, object, string>>(
    //                    Expression.Block(blockParameters, blockBodies), builderExpr, dbContextExpr, entityMapperExpr, parametersExpr, suffixExpr).Compile();
    //                else commandInitializer = Expression.Lambda<Action<StringBuilder, DbContext, EntityMap, object>>(
    //                    Expression.Block(blockParameters, blockBodies), builderExpr, dbContextExpr, entityMapperExpr, parametersExpr).Compile();
    //            }
    //            else
    //            {
    //                if (hasSuffix) commandInitializer = Expression.Lambda<Action<StringBuilder, DbContext, object, string>>(
    //                    Expression.Block(blockParameters, blockBodies), builderExpr, dbContextExpr, parametersExpr, suffixExpr).Compile();
    //                else commandInitializer = Expression.Lambda<Action<StringBuilder, DbContext, object>>(
    //                    Expression.Block(blockParameters, blockBodies), builderExpr, dbContextExpr, parametersExpr).Compile();
    //            }
    //        }
    //        else
    //        {
    //            if (isDictionary)
    //            {
    //                if (hasSuffix) commandInitializer = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, DbContext, EntityMap, object, string>>(
    //                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, dbContextExpr, entityMapperExpr, parametersExpr, suffixExpr).Compile();
    //                else commandInitializer = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, DbContext, EntityMap, object>>(
    //                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, dbContextExpr, entityMapperExpr, parametersExpr).Compile();
    //            }
    //            else
    //            {
    //                if (hasSuffix) commandInitializer = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, DbContext, object, string>>(
    //                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, dbContextExpr, parametersExpr, suffixExpr).Compile();
    //                else commandInitializer = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, DbContext, object>>(
    //                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, dbContextExpr, parametersExpr).Compile();
    //            }
    //        }
    //    }
    //    return (isDictionary, commandInitializer);
    //}

    public static object BuildFieldsSqlParametersPart(DbContext dbContext, Type entityType, Type parametersType, int commandType, int sqlType, int keyType, bool isFunc, bool hasSuffix, bool isUpdateRowVersion, List<string> onlyFieldNames, List<string> ignoreFieldNames, string headSql = null, string tailSql = null)
    {
        //commandType 1:Insert Field, 2:Insert Value, 3:Insert Update Set 4:Update Set
        //sqlType 0:None 1:Sql And Parameters 2:Only Sql 3:Only Parameters
        //keyType 0:None 1:Use Keys 2:Ignore Keys      
        var dbContextExpr = Expression.Parameter(typeof(DbContext), "dbContext");
        var parametersExpr = Expression.Parameter(typeof(object), "parameters");

        ParameterExpression dbParametersExpr = null;
        ParameterExpression builderExpr = null;
        ParameterExpression suffixExpr = null;
        ParameterExpression ormProviderExpr = null;
        ParameterExpression parameterNameExpr = null;
        ParameterExpression typedParametersExpr = null;
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();

        bool isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(parametersType);
        var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), [typeof(string)]);
        var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)]);
        MethodInfo methodInfo = null;

        if (commandType > 1 && sqlType != 2)
            dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");

        if (isDictionary || commandType > 1 && sqlType != 2)
        {
            if (isDictionary) parametersType = typeof(IDictionary<string, object>);
            ormProviderExpr = Expression.Variable(typeof(IOrmProvider), "ormProvider");
            typedParametersExpr = Expression.Variable(parametersType, isDictionary ? "dict" : "typedParameters");
            blockParameters.AddRange([ormProviderExpr, typedParametersExpr]);
            blockBodies.Add(Expression.Assign(ormProviderExpr, Expression.Property(dbContextExpr, nameof(DbContext.OrmProvider))));
            blockBodies.Add(Expression.Assign(typedParametersExpr, Expression.Convert(parametersExpr, parametersType)));
        }
        if (sqlType != 3)
        {
            if (isFunc)
            {
                builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
                blockParameters.Add(builderExpr);
                var constructorInfo = typeof(StringBuilder).GetConstructor(Type.EmptyTypes);
                blockBodies.Add(Expression.Assign(builderExpr, Expression.New(constructorInfo)));
            }
            else builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
        }
        if (commandType > 1 && hasSuffix)
        {
            suffixExpr = Expression.Parameter(typeof(string), "suffix");
            parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
            blockParameters.Add(parameterNameExpr);
        }

        if (sqlType != 3 && !string.IsNullOrEmpty(headSql))
            blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(headSql)));

        ParameterExpression entityMapperExpr = null;
        ParameterExpression memberMapperExpr = null;
        MethodInfo containsKeyMethodInfo = null;
        PropertyInfo dictItemPropertyInfo = null;
        var ormProvider = dbContext.OrmProvider;

        if (isDictionary)
        {
            entityMapperExpr = Expression.Variable(typeof(EntityMap), "entityMapper");
            memberMapperExpr = Expression.Variable(typeof(MemberMap), "memberMapper");
            blockParameters.AddRange([entityMapperExpr, memberMapperExpr]);

            containsKeyMethodInfo = typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.ContainsKey));
            dictItemPropertyInfo = typeof(IDictionary<string, object>).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(string)).First();

            var mapProviderExpr = Expression.Property(dbContextExpr, nameof(DbContext.MapProvider));
            methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.GetEntityMap), [typeof(EntityMapProvider), typeof(Type)]);
            blockBodies.Add(Expression.Assign(entityMapperExpr, Expression.Call(methodInfo, mapProviderExpr, Expression.Constant(entityType))));
        }

        if (isDictionary && keyType != 1)
        {
            var indexExpr = Expression.Variable(typeof(int), "index");
            var enumeratorExpr = Expression.Variable(typeof(IEnumerator<KeyValuePair<string, object>>), "enumerator");
            var itemKeyExpr = Expression.Variable(typeof(string), "itemKey");
            var itemValueExpr = Expression.Variable(typeof(object), "itemValue");
            var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string), typeof(string)]);

            blockParameters.AddRange([indexExpr, enumeratorExpr, itemKeyExpr, itemValueExpr]);
            var breakLabel = Expression.Label();
            var continueLabel = Expression.Label();

            //var index = 0;
            //var enumerator = dict.GetEnumerator();
            blockBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
            methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
            blockBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(typedParametersExpr, methodInfo)));

            //if(!enumerator.MoveNext())
            //  break;
            var loopBodies = new List<Expression>();
            methodInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
            var ifFalseExpr = Expression.IsFalse(Expression.Call(enumeratorExpr, methodInfo));
            loopBodies.Add(Expression.IfThen(ifFalseExpr, Expression.Break(breakLabel)));

            //var itemKey = enumerator.Current.Key;
            //var fieldValue = enumerator.Current.Value;          
            var currentExpr = Expression.Property(enumeratorExpr, nameof(IEnumerator.Current));
            loopBodies.Add(Expression.Assign(itemKeyExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Key))));

            //if(!entityMapper.ContainsMemberMap(itemKey)) continue;          
            methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.ContainsMemberMap));
            Expression isContinueExpr = Expression.IsFalse(Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr));
            loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

            //var memberMapper = entityMapper.GetMemberMap(itemKey);
            methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.GetMemberMap));
            loopBodies.Add(Expression.Assign(memberMapperExpr, Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr)));
            //|| memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsKey
            isContinueExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnore));
            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsNavigation)));
            if (keyType == 2)
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsKey)));

            //|| memberMapper.IsIgnoreInsert || memberMapper.IsAutoIncrement
            if (commandType < 3)
            {
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnoreInsert)));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsAutoIncrement)));
            }
            //|| memberMapper.IsIgnoreUpdate
            if (commandType > 2)
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnoreUpdate)));
            //|| memberMapper.IsRowVersion
            if (!isUpdateRowVersion)
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsRowVersion)));

            //|| !onlyFields.Constains(itemKey)
            if (onlyFieldNames != null)
            {
                var initExprs = onlyFieldNames.Select(f => Expression.Constant(f, typeof(string)));
                var onlyFieldsExpr = Expression.NewArrayInit(typeof(string), initExprs);
                methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), [typeof(string)]);
                var isFalseExpr = Expression.IsFalse(Expression.Call(methodInfo, onlyFieldsExpr, itemKeyExpr));
                isContinueExpr = Expression.OrElse(isContinueExpr, isFalseExpr);
            }
            //|| ignoreFields.Constains(itemKey) 
            if (ignoreFieldNames != null)
            {
                var initExprs = ignoreFieldNames.Select(f => Expression.Constant(f, typeof(string)));
                var ignoreFieldsExpr = Expression.NewArrayInit(typeof(string), initExprs);
                methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), [typeof(string)]);
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Call(methodInfo, ignoreFieldsExpr, itemKeyExpr));
            }
            loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

            //var parameterName = ormProvider.ParameterPrefix + itemKey + suffix;
            Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix);
            if (commandType > 1)
            {
                if (hasSuffix)
                {
                    myParameterNameExpr = Expression.Call(concatMethodInfo2, myParameterNameExpr, itemKeyExpr, suffixExpr);
                    loopBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                    myParameterNameExpr = parameterNameExpr;
                }
                else myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, itemKeyExpr);
            }
            //生成SQL
            if (sqlType < 3)
            {
                //if(index > 0) builder.Append(",");
                var greaterThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0));
                var callExpr = Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(","));
                loopBodies.Add(Expression.IfThen(greaterThenExpr, callExpr));

                //builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)");
                //builder.Append(parameterName);
                //builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");

                Expression contentExpr = null;
                Expression fieldNameExpr = null;

                if (commandType == 2) contentExpr = myParameterNameExpr;
                else
                {
                    methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetFieldName));
                    fieldNameExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.FieldName));
                    fieldNameExpr = Expression.Call(ormProviderExpr, methodInfo, fieldNameExpr);

                    if (commandType == 1) contentExpr = fieldNameExpr;
                    else contentExpr = Expression.Call(concatMethodInfo2, fieldNameExpr, Expression.Constant("="), myParameterNameExpr);
                }
                loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, contentExpr));
            }
            //生成参数
            if (commandType > 1 && sqlType != 2)
            {
                loopBodies.Add(Expression.Assign(itemValueExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Value))));
                AddValueParameter(dbContext, dbParametersExpr, ormProviderExpr, myParameterNameExpr, itemValueExpr, memberMapperExpr, loopBodies);
            }

            //index++;
            loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));
            blockBodies.Add(Expression.Loop(Expression.Block(loopBodies), breakLabel, continueLabel));
        }
        else
        {
            var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);
            var filterMemberMaps = keyType == 1 ? entityMapper.KeyMembers : entityMapper.MemberMaps;
            var targetMemberInfos = parametersType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

            var index = 0;
            foreach (var memberMapper in filterMemberMaps)
            {
                if (!isDictionary && !targetMemberInfos.Exists(f => f.Name == memberMapper.MemberName))
                    continue;
                if (memberMapper.IsIgnore || memberMapper.IsNavigation)
                    continue;
                if (onlyFieldNames != null && !onlyFieldNames.Contains(memberMapper.MemberName))
                    continue;
                if (ignoreFieldNames != null && ignoreFieldNames.Contains(memberMapper.MemberName))
                    continue;
                if (!isUpdateRowVersion && memberMapper.IsRowVersion)
                    continue;
                //Insert
                if (commandType < 3 && (memberMapper.IsIgnoreInsert || memberMapper.IsAutoIncrement))
                    continue;
                //Update
                if (commandType > 2 && memberMapper.IsIgnoreUpdate)
                    continue;

                if (keyType == 1)
                {
                    if (isDictionary)
                    {
                        var isContainsKeyExpr = Expression.Call(typedParametersExpr, containsKeyMethodInfo, Expression.Constant(memberMapper.MemberName));
                        var exception = new KeyNotFoundException($"字典参数中{parametersType.FullName}缺少Key:{memberMapper.MemberName}的成员");
                        blockBodies.Add(Expression.IfThen(Expression.IsFalse(isContainsKeyExpr), Expression.Throw(Expression.Constant(exception))));
                    }
                    else throw new KeyNotFoundException($"参数类型{parametersType.FullName}缺少{memberMapper.MemberName}的成员");
                }

                var parameterName = ormProvider.ParameterPrefix + (commandType == 3 ? "p" : "") + memberMapper.MemberName;
                Expression myParameterNameExpr = Expression.Constant(parameterName);
                if (commandType > 1 && hasSuffix)
                {
                    myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, suffixExpr);
                    blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                    myParameterNameExpr = parameterNameExpr;
                }
                //生成SQL
                if (sqlType != 3)
                {
                    if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(",")));
                    Expression contentExpr = null;
                    switch (commandType)
                    {
                        case 1: contentExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}"); break;
                        case 2: contentExpr = myParameterNameExpr; break;
                        case 3:
                        case 4:
                            contentExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}=");
                            contentExpr = Expression.Call(concatMethodInfo, contentExpr, myParameterNameExpr);
                            break;
                    }
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, contentExpr));
                }
                //生成参数
                if (commandType > 1 && sqlType != 2)
                {
                    if (isDictionary)
                    {
                        var memberNameExpr = Expression.Constant(memberMapper.MemberName);
                        var fieldValueExpr = Expression.Property(typedParametersExpr, dictItemPropertyInfo, memberNameExpr);
                        AddValueParameter(dbContext, dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueExpr, memberMapperExpr, blockBodies);
                    }
                    else
                    {
                        var fieldValueType = targetMemberInfos.Find(f => f.Name == memberMapper.MemberName).GetMemberType();
                        Expression fieldValueExpr = Expression.PropertyOrField(typedParametersExpr, memberMapper.MemberName);
                        if (fieldValueType != typeof(object))
                            fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                        AddValueParameter(dbContext, dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueType, fieldValueExpr, memberMapper, blockBodies);
                    }
                }
                index++;
            }
            if (index <= 0)
                throw new Exception("没有找到where条件或是忽略了字段大小写，无法识别到字段名称");
        }

        if (sqlType != 3 && !string.IsNullOrEmpty(tailSql))
            blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(tailSql)));

        if (isFunc)
        {
            methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
            var returnExpr = Expression.Call(builderExpr, methodInfo);
            var resultLabelExpr = Expression.Label(typeof(string));
            blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
            blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));

            if (commandType == 1) return Expression.Lambda<Func<DbContext, object, string>>(
                Expression.Block(blockParameters, blockBodies), dbContextExpr, parametersExpr).Compile();
            else
            {
                if (sqlType == 2)
                {
                    if (hasSuffix) return Expression.Lambda<Func<DbContext, object, string, string>>(
                        Expression.Block(blockParameters, blockBodies), dbContextExpr, parametersExpr, suffixExpr).Compile();
                    else return Expression.Lambda<Func<DbContext, object, string>>(
                        Expression.Block(blockParameters, blockBodies), dbContextExpr, parametersExpr).Compile();
                }
                else
                {
                    if (hasSuffix) return Expression.Lambda<Func<IDataParameterCollection, DbContext, object, string, string>>(
                        Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, parametersExpr, suffixExpr).Compile();
                    else return Expression.Lambda<Func<IDataParameterCollection, DbContext, object, string>>(
                        Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, parametersExpr).Compile();
                }
            }
        }
        else
        {
            if (commandType == 1) return Expression.Lambda<Action<StringBuilder, DbContext, object>>(
                Expression.Block(blockParameters, blockBodies), builderExpr, dbContextExpr, parametersExpr).Compile();
            else
            {
                switch (sqlType)
                {
                    case 1:
                        if (hasSuffix) return Expression.Lambda<Action<IDataParameterCollection, StringBuilder, DbContext, object, string>>(
                            Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, dbContextExpr, parametersExpr, suffixExpr).Compile();
                        else return Expression.Lambda<Action<IDataParameterCollection, StringBuilder, DbContext, object>>(
                            Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, dbContextExpr, parametersExpr).Compile();
                    case 2:
                        if (hasSuffix) return Expression.Lambda<Action<StringBuilder, DbContext, object, string>>(
                            Expression.Block(blockParameters, blockBodies), builderExpr, dbContextExpr, parametersExpr, suffixExpr).Compile();
                        else return Expression.Lambda<Action<StringBuilder, DbContext, object>>(
                            Expression.Block(blockParameters, blockBodies), builderExpr, dbContextExpr, parametersExpr).Compile();
                    case 3:
                        if (hasSuffix) return Expression.Lambda<Action<IDataParameterCollection, DbContext, object, string>>(
                            Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, parametersExpr, suffixExpr).Compile();
                        else return Expression.Lambda<Action<IDataParameterCollection, DbContext, object>>(
                            Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, parametersExpr).Compile();
                    default: throw new NotSupportedException("不支持的场景");
                }
            }
        }
    }
    public static object BuildWhereSqlParametersPart(DbContext dbContext, Type entityType, Type whereObjType, int sqlType, bool isFunc, bool isUseKey, bool isWithKey, bool isInExpr, bool isMultiple, bool isBulk, string headSql = null)
    {
        //sqlType 0:None 1:Sql And Parameters 2:Only Sql 3:Only Parameters
        var dbContextExpr = Expression.Parameter(typeof(DbContext), "dbContext");
        var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

        ParameterExpression dbParametersExpr = null;
        ParameterExpression builderExpr = null;
        ParameterExpression ormProviderExpr = null;
        ParameterExpression suffixExpr = null;
        ParameterExpression parameterNameExpr = null;
        ParameterExpression typedWhereObjExpr = null;
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();

        var isEntityType = whereObjType.IsEntityType(out _);
        bool isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(whereObjType);
        var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), [typeof(string)]);
        var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)]);
        MethodInfo methodInfo = null;

        if (sqlType != 2)
            dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");

        if (isDictionary || sqlType != 2)
        {
            if (isDictionary) whereObjType = typeof(IDictionary<string, object>);
            ormProviderExpr = Expression.Variable(typeof(IOrmProvider), "ormProvider");
            blockParameters.Add(ormProviderExpr);
            blockBodies.Add(Expression.Assign(ormProviderExpr, Expression.Property(dbContextExpr, nameof(DbContext.OrmProvider))));

            if (isDictionary || isEntityType)
            {
                typedWhereObjExpr = Expression.Variable(whereObjType, isDictionary ? "dict" : "typedWhereObj");
                blockParameters.Add(typedWhereObjExpr);
                blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));
            }
        }
        if (isFunc)
        {
            builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
            blockParameters.Add(builderExpr);
            var constructorInfo = typeof(StringBuilder).GetConstructor(Type.EmptyTypes);
            blockBodies.Add(Expression.Assign(builderExpr, Expression.New(constructorInfo)));
        }
        else builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");

        var hasSuffix = isMultiple || isBulk;
        if (hasSuffix)
        {
            suffixExpr = Expression.Parameter(typeof(string), "suffix");
            parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
            blockParameters.Add(parameterNameExpr);
        }

        if (sqlType != 3 && !string.IsNullOrEmpty(headSql))
            blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(headSql)));

        ParameterExpression entityMapperExpr = null;
        ParameterExpression memberMapperExpr = null;
        MethodInfo containsKeyMethodInfo = null;
        PropertyInfo dictItemPropertyInfo = null;
        List<MemberInfo> targetMemberInfos = null;
        var ormProvider = dbContext.OrmProvider;
        var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);

        if (isDictionary)
        {
            containsKeyMethodInfo = typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.ContainsKey));
            dictItemPropertyInfo = typeof(IDictionary<string, object>).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(string)).First();

            if (sqlType != 2)
            {
                entityMapperExpr = Expression.Variable(typeof(EntityMap), "entityMapper");
                memberMapperExpr = Expression.Variable(typeof(MemberMap), "memberMapper");
                blockParameters.AddRange([entityMapperExpr, memberMapperExpr]);
                var mapProviderExpr = Expression.Property(dbContextExpr, nameof(DbContext.MapProvider));
                methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.GetEntityMap), [typeof(EntityMapProvider), typeof(Type)]);
                blockBodies.Add(Expression.Assign(entityMapperExpr, Expression.Call(methodInfo, mapProviderExpr, Expression.Constant(entityType))));
            }
        }
        else
        {
            if (isEntityType)
            {
                targetMemberInfos = whereObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
            }
            else if (!isUseKey || entityMapper.KeyMembers.Count != 1)
                throw new NotSupportedException("不支持非单主键字段的业务场景");
        }

        if (isDictionary && !isUseKey)
        {
            var indexExpr = Expression.Variable(typeof(int), "index");
            var enumeratorExpr = Expression.Variable(typeof(IEnumerator<KeyValuePair<string, object>>), "enumerator");
            var itemKeyExpr = Expression.Variable(typeof(string), "itemKey");
            var itemValueExpr = Expression.Variable(typeof(object), "itemValue");
            var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string), typeof(string)]);

            blockParameters.AddRange([indexExpr, enumeratorExpr, itemKeyExpr, itemValueExpr]);
            var breakLabel = Expression.Label();
            var continueLabel = Expression.Label();

            //var index = 0;
            //var enumerator = dict.GetEnumerator();
            blockBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
            methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
            blockBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(typedWhereObjExpr, methodInfo)));

            //if(!enumerator.MoveNext())
            //  break;
            var loopBodies = new List<Expression>();
            methodInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
            var ifFalseExpr = Expression.IsFalse(Expression.Call(enumeratorExpr, methodInfo));
            loopBodies.Add(Expression.IfThen(ifFalseExpr, Expression.Break(breakLabel)));

            //var itemKey = enumerator.Current.Key;
            //var fieldValue = enumerator.Current.Value;          
            var currentExpr = Expression.Property(enumeratorExpr, nameof(IEnumerator.Current));
            loopBodies.Add(Expression.Assign(itemKeyExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Key))));

            //if(!entityMapper.ContainsMemberMap(itemKey)) continue;          
            methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.ContainsMemberMap));
            Expression isContinueExpr = Expression.IsFalse(Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr));
            loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

            //var memberMapper = entityMapper.GetMemberMap(itemKey);
            methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.GetMemberMap));
            loopBodies.Add(Expression.Assign(memberMapperExpr, Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr)));
            //|| memberMapper.IsIgnore || memberMapper.IsNavigation
            isContinueExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnore));
            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsNavigation)));
            loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

            //var parameterName = ormProvider.ParameterPrefix + itemKey + suffix;
            Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix + (isWithKey ? "k" : ""));
            if (hasSuffix)
            {
                myParameterNameExpr = Expression.Call(concatMethodInfo2, myParameterNameExpr, itemKeyExpr, suffixExpr);
                loopBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                myParameterNameExpr = parameterNameExpr;
            }
            else myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, itemKeyExpr);

            //生成SQL
            if (sqlType < 3)
            {
                //if(index > 0) builder.Append(" AND ");
                var greaterThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0));
                var callExpr = Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND "));
                loopBodies.Add(Expression.IfThen(greaterThenExpr, callExpr));

                //builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetFieldName));
                Expression contentExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.FieldName));
                contentExpr = Expression.Call(ormProviderExpr, methodInfo, contentExpr);
                contentExpr = Expression.Call(concatMethodInfo2, contentExpr, Expression.Constant("="), myParameterNameExpr);
                loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, contentExpr));
            }
            //生成参数
            if (sqlType != 2)
            {
                loopBodies.Add(Expression.Assign(itemValueExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Value))));
                AddValueParameter(dbContext, dbParametersExpr, ormProviderExpr, myParameterNameExpr, itemValueExpr, memberMapperExpr, loopBodies);
            }

            //index++;
            loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));
            blockBodies.Add(Expression.Loop(Expression.Block(loopBodies), breakLabel, continueLabel));
        }
        else
        {
            var index = 0;
            var filterMemberMappers = isUseKey ? entityMapper.KeyMembers : entityMapper.MemberMaps;
            foreach (var memberMapper in filterMemberMappers)
            {
                if (memberMapper.IsIgnore || memberMapper.IsNavigation)
                    continue;

                var memberNameExpr = Expression.Constant(memberMapper.MemberName);
                if (isUseKey)
                {
                    if (isDictionary)
                    {
                        var isContainsKeyExpr = Expression.Call(typedWhereObjExpr, containsKeyMethodInfo, Expression.Constant(memberMapper.MemberName));
                        var exception = new KeyNotFoundException($"字典参数中{whereObjType.FullName}缺少Key:{memberMapper.MemberName}的成员");
                        blockBodies.Add(Expression.IfThen(Expression.IsFalse(isContainsKeyExpr), Expression.Throw(Expression.Constant(exception))));
                    }
                    if (isEntityType && !targetMemberInfos.Exists(f => f.Name == memberMapper.MemberName))
                        throw new KeyNotFoundException($"参数类型{whereObjType.FullName}缺少{memberMapper.MemberName}的成员");
                }
                else if (!targetMemberInfos.Exists(f => f.Name == memberMapper.MemberName))
                    continue;

                Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix + (isWithKey ? "k" : "") + memberMapper.MemberName);
                if (hasSuffix)
                {
                    myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, suffixExpr);
                    blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                    myParameterNameExpr = parameterNameExpr;
                }
                if (sqlType != 3)
                {
                    Expression contentExpr = null;
                    if (isInExpr) contentExpr = myParameterNameExpr;
                    else
                    {
                        contentExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}=");
                        contentExpr = Expression.Call(concatMethodInfo, contentExpr, myParameterNameExpr);
                    }
                    if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, contentExpr));
                }
                if (sqlType != 2)
                {
                    if (isDictionary)
                    {
                        methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.GetMemberMap));
                        blockBodies.Add(Expression.Assign(memberMapperExpr, Expression.Call(entityMapperExpr, methodInfo, memberNameExpr)));
                        var fieldValueExpr = Expression.Property(typedWhereObjExpr, dictItemPropertyInfo, memberNameExpr);
                        AddValueParameter(dbContext, dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueExpr, memberMapperExpr, blockBodies);
                    }
                    else
                    {
                        if (isEntityType)
                        {
                            var fieldValueType = targetMemberInfos.Find(f => f.Name == memberMapper.MemberName).GetMemberType();
                            Expression fieldValueExpr = Expression.PropertyOrField(typedWhereObjExpr, memberMapper.MemberName);
                            if (fieldValueType != typeof(object))
                                fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                            AddValueParameter(dbContext, dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueType, fieldValueExpr, memberMapper, blockBodies);
                        }
                        else AddValueParameter(dbContext, dbParametersExpr, ormProviderExpr, myParameterNameExpr, whereObjType, whereObjExpr, memberMapper, blockBodies);
                    }
                }
                index++;
            }

            if (index <= 0)
                throw new Exception("没有找到where条件或是忽略了字段大小写，无法识别到字段名称");
        }

        if (isFunc)
        {
            methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
            var returnExpr = Expression.Call(builderExpr, methodInfo);
            var resultLabelExpr = Expression.Label(typeof(string));
            blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
            blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));

            if (sqlType == 2)
            {
                if (hasSuffix) return Expression.Lambda<Func<DbContext, object, string, string>>(
                    Expression.Block(blockParameters, blockBodies), dbContextExpr, whereObjExpr, suffixExpr).Compile();
                else return Expression.Lambda<Func<DbContext, object, string>>(
                    Expression.Block(blockParameters, blockBodies), dbContextExpr, whereObjExpr).Compile();
            }
            else
            {
                if (hasSuffix) return Expression.Lambda<Func<IDataParameterCollection, DbContext, object, string, string>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, whereObjExpr, suffixExpr).Compile();
                else return Expression.Lambda<Func<IDataParameterCollection, DbContext, object, string>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, whereObjExpr).Compile();
            }
        }
        else
        {
            switch (sqlType)
            {
                case 1:
                    if (hasSuffix) return Expression.Lambda<Action<IDataParameterCollection, StringBuilder, DbContext, object, string>>(
                        Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, dbContextExpr, whereObjExpr, suffixExpr).Compile();
                    else return Expression.Lambda<Action<IDataParameterCollection, StringBuilder, DbContext, object>>(
                        Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, dbContextExpr, whereObjExpr).Compile();
                case 2:
                    if (hasSuffix) return Expression.Lambda<Action<StringBuilder, DbContext, object, string>>(
                        Expression.Block(blockParameters, blockBodies), builderExpr, dbContextExpr, whereObjExpr, suffixExpr).Compile();
                    else return Expression.Lambda<Action<StringBuilder, DbContext, object>>(
                        Expression.Block(blockParameters, blockBodies), builderExpr, dbContextExpr, whereObjExpr).Compile();
                case 3:
                    if (hasSuffix) return Expression.Lambda<Action<IDataParameterCollection, DbContext, object, string>>(
                        Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, whereObjExpr, suffixExpr).Compile();
                    else return Expression.Lambda<Action<IDataParameterCollection, DbContext, object>>(
                        Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, whereObjExpr).Compile();
                default: throw new NotSupportedException("不支持的场景");
            }
        }
    }

    private static object BuildQueryWhereSqlParameters(DbContext dbContext, Type entityType, Type whereObjType, bool isExists, bool isUseKey, bool isMultiple, bool isBulk)
    {
        var ormProvider = dbContext.OrmProvider;
        var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);
        string tableName = ormProvider.GetTableName(entityMapper.TableName);
        string fieldsSql = null;
        if (isExists) fieldsSql = "COUNT(1)";
        else fieldsSql = BuildSelectFieldsSqlPart(ormProvider, entityMapper, entityType);

        var headSql = $"SELECT {fieldsSql} FROM {tableName} WHERE ";
        if (isBulk)
        {
            if (isUseKey && entityMapper.KeyMembers.Count == 1)
            {
                headSql += $"{ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)} IN (";
                return (true, headSql, BuildWhereSqlParametersPart(dbContext, entityType, whereObjType, 1, false, isUseKey, false, true, isMultiple, isBulk, headSql));
            }
            else return (false, headSql, BuildWhereSqlParametersPart(dbContext, entityType, whereObjType, 1, false, isUseKey, false, false, isMultiple, isBulk, headSql));
        }
        return BuildWhereSqlParametersPart(dbContext, entityType, whereObjType, 1, true, isUseKey, false, false, isMultiple, isBulk, headSql);
    }
    public static object BuildQueryWhereObjSqlParameters(DbContext dbContext, Type entityType, Type whereObjType, object whereObjs, bool isMultiple, bool isBulk)
    {
        if (isBulk)
        {
            var parameters = whereObjs as IEnumerable;
            foreach (var parameter in parameters)
            {
                whereObjType = parameter.GetType();
                break;
            }
        }
        var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.MapProvider, entityType, whereObjType);
        var commandInitializerCache = isBulk ? queryBulkWhereObjCommandInitializerCache : isMultiple ? queryMultiWhereObjCommandInitializerCache : queryWhereObjCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, BuildQueryWhereSqlParameters(dbContext, entityType, whereObjType, false, false, isMultiple, isBulk));
    }
    public static object BuildQueryWhereObjByKeySqlParameters(DbContext dbContext, Type entityType, object whereObjs, bool isMultiple, bool isBulk)
    {
        Type whereObjType = null;
        if (isBulk)
        {
            var parameters = whereObjs as IEnumerable;
            foreach (var parameter in parameters)
            {
                whereObjType = parameter.GetType();
                break;
            }
        }
        else whereObjType = whereObjs.GetType();
        var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.MapProvider, entityType, whereObjType);
        var commandInitializerCache = isBulk ? queryBulkWhereObjByKeyCommandInitializerCache : isMultiple ? queryMultiWhereObjByKeyCommandInitializerCache : queryWhereObjByKeyCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, BuildQueryWhereSqlParameters(dbContext, entityType, whereObjType, false, true, isMultiple, isBulk));
    }
    public static object BuildExistsSqlParameters(DbContext dbContext, Type entityType, Type whereObjType, object whereObjs, bool isMultiple, bool isBulk)
    {
        if (isBulk)
        {
            var parameters = whereObjs as IEnumerable;
            foreach (var parameter in parameters)
            {
                whereObjType = parameter.GetType();
                break;
            }
        }
        var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.MapProvider, entityType, whereObjType);
        var commandInitializerCache = isBulk ? queryBulkExistsCommandInitializerCache : isMultiple ? queryMultiExistsCommandInitializerCache : queryExistsCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, BuildQueryWhereSqlParameters(dbContext, entityType, whereObjType, true, false, isMultiple, isBulk));
    }
    public static Action<IDataParameterCollection, IOrmProvider, object> BuildQueryRawSqlParameters(IOrmProvider ormProvider, string rawSql, object parameters)
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
            var cacheKey = GetCacheKey(rawSql, parameterType);
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

    public static Action<DbContext, ITheaCommand, object> BuildCreateCommandInitializer(DbContext dbContext, Type entityType, object insertObj, bool isReturnIdentity)
    {
        var insertObjType = insertObj.GetType();
        var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.MapProvider, entityType, insertObjType, isReturnIdentity);
        return createCommandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var ormProvider = dbContext.OrmProvider;
            var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);
            var tableName = entityMapper.TableName;

            var tailSql = ")";
            if (isReturnIdentity)
            {
                var keyField = entityMapper.KeyMembers[0].FieldName;
                tailSql += ormProvider.GetIdentitySql(ormProvider.GetFieldName(keyField));
            }
            bool isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(insertObjType);
            if (isDictionary || dbContext.ShardingProvider != null && dbContext.ShardingProvider.TryGetTableSharding(entityType, out _))
            {
                var fieldsSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 1, 2, 0, false, false, false, null, null, "(", ") VALUES ")
                    as Action<StringBuilder, DbContext, object>;
                var valuesSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 2, 1, 0, false, false, false, null, null, "(", tailSql)
                    as Action<IDataParameterCollection, StringBuilder, DbContext, object>;

                if (dbContext.ShardingProvider != null && dbContext.ShardingProvider.TryGetTableSharding(entityType, out _))
                {
                    return (dbContext, command, insertObjs) =>
                    {
                        var myTableName = dbContext.GetShardingTableName(entityType, insertObjType, insertObjs);
                        var builder = new StringBuilder();
                        builder.Append($"INSERT INTO {ormProvider.GetTableName(myTableName)}");
                        fieldsSetter.Invoke(builder, dbContext, insertObjs);
                        valuesSetter.Invoke(command.Parameters, builder, dbContext, insertObjs);
                        command.CommandText = builder.ToString();
                        builder.Clear();
                    };
                }
                else
                {
                    return (dbContext, command, insertObjs) =>
                    {
                        var builder = new StringBuilder();
                        builder.Append($"INSERT INTO {ormProvider.GetTableName(tableName)}");
                        fieldsSetter.Invoke(builder, dbContext, insertObjs);
                        valuesSetter.Invoke(command.Parameters, builder, dbContext, insertObjs);
                        command.CommandText = builder.ToString();
                        builder.Clear();
                    };
                }
            }
            else
            {
                var fieldsSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 1, 2, 0, true, false, false, null, null, " (", ") VALUES ")
                    as Func<DbContext, object, string>;
                var valuesSqlSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 2, 2, 0, true, false, false, null, null, "(", tailSql)
                    as Func<DbContext, object, string>;
                var valuesParametersSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 2, 3, 0, false, false, false, null, null)
                    as Action<IDataParameterCollection, DbContext, object>;
                var sql = $"INSERT INTO {ormProvider.GetTableName(tableName)}" + fieldsSetter.Invoke(dbContext, null) + valuesSqlSetter.Invoke(dbContext, null);
                return (dbContext, command, insertObjs) =>
                {
                    command.CommandText = sql;
                    valuesParametersSetter.Invoke(command.Parameters, dbContext, insertObjs);
                };
            }
        });
    }
    public static Func<DbContext, ITheaCommand, IEnumerable, int, int> BuildCreateBulkCommandExecutor(DbContext dbContext, Type entityType, IEnumerable insertObjs)
    {
        object firstInsertObj = null;
        Type insertObjType = null;
        foreach (var insertObj in insertObjs)
        {
            firstInsertObj = insertObj;
            insertObjType = insertObj.GetType();
            break;
        }
        var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.MapProvider, entityType, insertObjType);
        return createBulkCommandExecutorCache.GetOrAdd(cacheKey, f =>
        {
            var ormProvider = dbContext.OrmProvider;
            var fieldsSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 1, 2, 0, true, false, false, null, null, "(", ") VALUES ")
                as Func<DbContext, object, string>;
            var valuesSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 2, 1, 0, false, true, false, null, null, "(", ")")
                as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            var fieldsSql = fieldsSetter.Invoke(dbContext, firstInsertObj);

            int Execute(DbContext dbContext, ITheaCommand command, string tableName, IEnumerable insertObjs, int bulkCount)
            {
                int count = 0, index = 0;
                var builder = new StringBuilder($"INSERT INTO {ormProvider.GetTableName(tableName)}{fieldsSql} ");
                foreach (var insertObj in insertObjs)
                {
                    if (index > 0) builder.Append(',');
                    valuesSetter.Invoke(command.Parameters, builder, dbContext, insertObj, index.ToString());
                    if (index >= bulkCount)
                    {
                        command.CommandText = builder.ToString();
                        count += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                        builder.Clear();
                        command.Parameters.Clear();
                        builder.Append($"INSERT INTO {ormProvider.GetTableName(tableName)}{fieldsSql}");
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    command.CommandText = builder.ToString();
                    count += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                    builder.Clear();
                    command.Parameters.Clear();
                }
                return count;
            }

            Func<DbContext, ITheaCommand, IEnumerable, int, int> commandExecutor = null;
            if (dbContext.ShardingProvider != null && dbContext.ShardingProvider.TryGetTableSharding(entityType, out _))
            {
                commandExecutor = (dbContext, command, insertObjs, bulkCount) =>
                {
                    int count = 0;
                    var tabledInsertObjs = dbContext.SplitShardingParameters(entityType, insertObjs);
                    foreach (var tabledInsertObj in tabledInsertObjs)
                    {
                        count += Execute(dbContext, command, tabledInsertObj.Key, tabledInsertObj.Value, bulkCount);
                    }
                    return count;
                };
            }
            else
            {
                var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);
                var tableName = entityMapper.TableName;
                commandExecutor = (dbContext, command, insertObjs, bulkCount) => Execute(dbContext, command, tableName, insertObjs, bulkCount);
            }
            return commandExecutor;
        });
    }
    public static Func<DbContext, ITheaCommand, IEnumerable, int, CancellationToken, Task<int>> BuildCreateBulkAsyncCommandExecutor(DbContext dbContext, Type entityType, IEnumerable insertObjs)
    {
        object firstInsertObj = null;
        Type insertObjType = null;
        foreach (var insertObj in insertObjs)
        {
            firstInsertObj = insertObj;
            insertObjType = insertObj.GetType();
            break;
        }
        var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.MapProvider, entityType, insertObjType);
        return createBulkAsyncCommandExecutorCache.GetOrAdd(cacheKey, f =>
        {
            var ormProvider = dbContext.OrmProvider;
            var fieldsSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 1, 2, 0, true, false, false, null, null, "(", ") VALUES ")
                as Func<DbContext, object, string>;
            var valuesSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 2, 1, 0, false, true, false, null, null, "(", ")")
                as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            var fieldsSql = fieldsSetter.Invoke(dbContext, firstInsertObj);

            async Task<int> Execute(DbContext dbContext, ITheaCommand command, string tableName, IEnumerable insertObjs, int bulkCount, CancellationToken cancellationToken)
            {
                int count = 0, index = 0;
                var builder = new StringBuilder($"INSERT INTO {ormProvider.GetTableName(tableName)}{fieldsSql} ");
                foreach (var insertObj in insertObjs)
                {
                    if (index > 0) builder.Append(',');
                    valuesSetter.Invoke(command.Parameters, builder, dbContext, insertObj, index.ToString());
                    if (index >= bulkCount)
                    {
                        command.CommandText = builder.ToString();
                        count += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                        builder.Clear();
                        command.Parameters.Clear();
                        builder.Append($"INSERT INTO {ormProvider.GetTableName(tableName)}{fieldsSql}");
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    command.CommandText = builder.ToString();
                    count += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                    builder.Clear();
                    command.Parameters.Clear();
                }
                return count;
            }

            Func<DbContext, ITheaCommand, IEnumerable, int, CancellationToken, Task<int>> commandExecutor = null;
            if (dbContext.ShardingProvider != null && dbContext.ShardingProvider.TryGetTableSharding(entityType, out _))
            {
                commandExecutor = async (dbContext, command, insertObjs, bulkCount, cancellationToken) =>
                {
                    int count = 0;
                    var tabledInsertObjs = dbContext.SplitShardingParameters(entityType, insertObjs);
                    foreach (var tabledInsertObj in tabledInsertObjs)
                    {
                        count += await Execute(dbContext, command, tabledInsertObj.Key, tabledInsertObj.Value, bulkCount, cancellationToken);
                    }
                    return count;
                };
            }
            else
            {
                var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);
                var tableName = entityMapper.TableName;
                commandExecutor = async (dbContext, command, insertObjs, bulkCount, cancellationToken) => await Execute(dbContext, command, tableName, insertObjs, bulkCount, cancellationToken);
            }
            return commandExecutor;
        });
    }
    public static Action<StringBuilder, DbContext, object> BuildCreateFieldsSqlPart(DbContext dbContext, Type entityType, Type insertObjType, bool isUpdateRowVersion, List<string> onlyFieldNames, List<string> ignoreFieldNames)
    {
        var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.MapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames);
        return createFieldsSqlCache.GetOrAdd(cacheKey, f =>
        {
            Action<StringBuilder, DbContext, object> fieldsSetter = null;
            if (typeof(IDictionary<string, object>).IsAssignableFrom(insertObjType))
            {
                fieldsSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 1, 2, 0, false, false, isUpdateRowVersion, onlyFieldNames, ignoreFieldNames)
                    as Action<StringBuilder, DbContext, object>;
            }
            else
            {
                var fieldsSqlGetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 1, 2, 0, true, false, isUpdateRowVersion, onlyFieldNames, ignoreFieldNames)
                    as Func<DbContext, object, string>;
                var fieldsSql = fieldsSqlGetter.Invoke(dbContext, null);
                fieldsSetter = (builder, dbContext, insertObj) => builder.Append(fieldsSql);
            }
            return fieldsSetter;
        });
    }
    public static object BuildCreateValuesSqlPart(DbContext dbContext, Type entityType, Type insertObjType, bool hasSuffix, bool isUpdateRowVersion, List<string> onlyFieldNames, List<string> ignoreFieldNames)
    {
        var ormProvider = dbContext.OrmProvider;
        var cacheKey = GetCacheKey(ormProvider.OrmProviderType, dbContext.MapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames);
        var cache = hasSuffix ? createBulkValuesSqlParametersCache : createValuesSqlParametersCache;
        return cache.GetOrAdd(cacheKey, f =>
        {
            var isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(insertObjType);
            if (!isDictionary && !hasSuffix)
            {
                var valuesSqlSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 2, 2, 0, true, hasSuffix, isUpdateRowVersion, onlyFieldNames, ignoreFieldNames)
                    as Func<DbContext, object, string>;
                var valuesParametersSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 2, 3, 0, false, hasSuffix, isUpdateRowVersion, onlyFieldNames, ignoreFieldNames)
                    as Action<IDataParameterCollection, DbContext, object>;
                var sql = valuesSqlSetter.Invoke(dbContext, null);
                Action<IDataParameterCollection, StringBuilder, DbContext, object> valuesSetter = (dbParameters, builder, dbContext, insertObj) =>
                {
                    builder.Append(sql);
                    valuesParametersSetter.Invoke(dbParameters, dbContext, insertObj);
                };
                return valuesSetter;
            }
            return BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 2, 1, 0, false, hasSuffix, isUpdateRowVersion, onlyFieldNames, ignoreFieldNames);
        });
    }

    public static object BuildUpdateCommandInitializer(DbContext dbContext, Type entityType, Type updateObjType, bool isBulk, bool isUpdateRowVersion)
    {
        var ormProvider = dbContext.OrmProvider;
        var cacheKey = GetCacheKey(ormProvider.OrmProviderType, dbContext.MapProvider, entityType, updateObjType, isBulk);
        var cache = isBulk ? updateMultiCommandInitializerCache : updateCommandInitializerCache;
        return cache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);
            var headSql = $"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ";
            var isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(updateObjType);
            object commandInitializer = null;
            if (isDictionary || isBulk)
            {
                var fieldsSqlSetter = BuildFieldsSqlParametersPart(dbContext, entityType, updateObjType, 4, 1, 3, false, isBulk, isUpdateRowVersion, null, null, headSql) as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
                var whereSqlSetter = BuildWhereSqlParametersPart(dbContext, entityType, updateObjType, 1, false, true, true, false, false, isBulk, " WHERE ") as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
                Action<IDataParameterCollection, StringBuilder, DbContext, object, string> typedCommandInitializer = (dbParameters, builder, dbContext, updateObj, suffix) =>
                {
                    fieldsSqlSetter.Invoke(dbParameters, builder, dbContext, updateObj, suffix);
                    whereSqlSetter.Invoke(dbParameters, builder, dbContext, updateObj, suffix);
                };
                commandInitializer = typedCommandInitializer;
            }
            else
            {
                var fieldsSqlSetter = BuildFieldsSqlParametersPart(dbContext, entityType, updateObjType, 4, 2, 3, true, false, isUpdateRowVersion, null, null, headSql) as Func<DbContext, object, string>;
                var fieldsParameterSetter = BuildFieldsSqlParametersPart(dbContext, entityType, updateObjType, 4, 3, 0, true, false, isUpdateRowVersion, null, null, headSql) as Action<IDataParameterCollection, DbContext, object>;
                var whereSqlSetter = BuildWhereSqlParametersPart(dbContext, entityType, updateObjType, 2, true, true, true, false, false, isBulk, " WHERE ") as Func<DbContext, object, string>;
                var whereParameterSetter = BuildWhereSqlParametersPart(dbContext, entityType, updateObjType, 3, false, true, true, false, false, isBulk) as Action<IDataParameterCollection, DbContext, object>;
                var sql = fieldsSqlSetter.Invoke(dbContext, null) + whereSqlSetter.Invoke(dbContext, null);
                Func<IDataParameterCollection, DbContext, object, string> typedCommandInitializer = (dbParameters, dbContext, updateObj) =>
                {
                    fieldsParameterSetter.Invoke(dbParameters, dbContext, updateObj);
                    whereParameterSetter.Invoke(dbParameters, dbContext, updateObj);
                    return sql;
                };
                commandInitializer = typedCommandInitializer;
            }
            return commandInitializer;
        });
    }
    public static object BuildUpdateSetWithSqlParametersPart(DbContext dbContext, Type entityType, Type updateObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool isMultiple, bool isUpdateRowVersion)
    {
        //单个对象，有可能会有join操作，会有添加别名的可能，多命令执行时会有suffix情况
        //Bulk场景，反而没有别名，也没有suffix情况
        var ormProvider = dbContext.OrmProvider;
        var mapProvider = dbContext.MapProvider;
        var cacheKey = GetCacheKey(ormProvider.OrmProviderType, dbContext.MapProvider, entityType, updateObjType, onlyFieldNames, ignoreFieldNames);
        var commandInitializerCache = isMultiple ? updateMultiWithCommandInitializerCache : updateWithCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
            var dbContextExpr = Expression.Parameter(typeof(DbContext), "dbContext");
            var updateFieldsExpr = Expression.Parameter(typeof(List<string>), "updateFields");
            var updateObjExpr = Expression.Parameter(typeof(object), "updateObj");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            ParameterExpression parameterNameExpr = null;
            ParameterExpression suffixExpr = null;

            bool isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(updateObjType);
            if (isDictionary) updateObjType = typeof(IDictionary<string, object>);
            var typedUpdateObjExpr = Expression.Variable(updateObjType, isDictionary ? "dict" : "typedUpdateObj");
            var ormProviderExpr = Expression.Variable(typeof(IOrmProvider), "ormProvider");
            blockParameters.AddRange([ormProviderExpr, typedUpdateObjExpr]);
            blockBodies.Add(Expression.Assign(ormProviderExpr, Expression.Property(dbContextExpr, nameof(DbContext.OrmProvider))));
            blockBodies.Add(Expression.Assign(typedUpdateObjExpr, Expression.Convert(updateObjExpr, updateObjType)));
            var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)]);
            var addMethodInfo = typeof(List<string>).GetMethod(nameof(List<string>.Add), [typeof(string)]);

            if (isMultiple)
            {
                suffixExpr = Expression.Variable(typeof(string), "suffix");
                parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                blockParameters.AddRange([parameterNameExpr, suffixExpr]);
            }
            if (isDictionary)
            {
                var entityMapperExpr = Expression.Variable(typeof(EntityMap), "entityMapper");
                var memberMapperExpr = Expression.Variable(typeof(MemberMap), "memberMapper");
                var containsKeyMethodInfo = typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.ContainsKey));
                var dictItemPropertyInfo = typeof(IDictionary<string, object>).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(string)).First();
                blockParameters.AddRange([entityMapperExpr, memberMapperExpr]);
                var mapProviderExpr = Expression.Property(dbContextExpr, nameof(DbContext.MapProvider));
                var methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.GetEntityMap), [typeof(EntityMapProvider), typeof(Type)]);
                blockBodies.Add(Expression.Assign(entityMapperExpr, Expression.Call(methodInfo, mapProviderExpr, Expression.Constant(entityType))));

                var indexExpr = Expression.Variable(typeof(int), "index");
                var enumeratorExpr = Expression.Variable(typeof(IEnumerator<KeyValuePair<string, object>>), "enumerator");
                var itemKeyExpr = Expression.Variable(typeof(string), "itemKey");
                var itemValueExpr = Expression.Variable(typeof(object), "itemValue");
                var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string), typeof(string)]);

                blockParameters.AddRange([indexExpr, enumeratorExpr, itemKeyExpr, itemValueExpr, memberMapperExpr]);
                var breakLabel = Expression.Label();
                var continueLabel = Expression.Label();

                //var index = 0;
                //var enumerator = dict.GetEnumerator();
                blockBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
                methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
                blockBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(typedUpdateObjExpr, methodInfo)));

                //if(!enumerator.MoveNext())
                //  break;
                var loopBodies = new List<Expression>();
                methodInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
                var ifFalseExpr = Expression.IsFalse(Expression.Call(enumeratorExpr, methodInfo));
                loopBodies.Add(Expression.IfThen(ifFalseExpr, Expression.Break(breakLabel)));

                //var itemKey = enumerator.Current.Key;
                //var fieldValue = enumerator.Current.Value;          
                var currentExpr = Expression.Property(enumeratorExpr, nameof(IEnumerator.Current));
                loopBodies.Add(Expression.Assign(itemKeyExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Key))));

                //if(!entityMapper.ContainsMemberMap(itemKey)) continue;          
                methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.ContainsMemberMap));
                Expression isContinueExpr = Expression.IsFalse(Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr));
                loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

                //var memberMapper = entityMapper.GetMemberMap(itemKey);
                methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.GetMemberMap));
                loopBodies.Add(Expression.Assign(memberMapperExpr, Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr)));
                //|| memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsKey
                isContinueExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnore));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsNavigation)));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsKey)));
                //|| memberMapper.IsIgnoreUpdate
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnoreUpdate)));
                //|| memberMapper.IsRowVersion
                if (!isUpdateRowVersion)
                    isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsRowVersion)));

                //|| !onlyFields.Constains(itemKey)
                if (onlyFieldNames != null)
                {
                    var initExprs = onlyFieldNames.Select(f => Expression.Constant(f, typeof(string)));
                    var onlyFieldsExpr = Expression.NewArrayInit(typeof(string), initExprs);
                    methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), [typeof(string)]);
                    var isFalseExpr = Expression.IsFalse(Expression.Call(methodInfo, onlyFieldsExpr, itemKeyExpr));
                    isContinueExpr = Expression.OrElse(isContinueExpr, isFalseExpr);
                }
                //|| ignoreFields.Constains(itemKey) 
                if (ignoreFieldNames != null)
                {
                    var initExprs = ignoreFieldNames.Select(f => Expression.Constant(f, typeof(string)));
                    var ignoreFieldsExpr = Expression.NewArrayInit(typeof(string), initExprs);
                    methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), [typeof(string)]);
                    isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Call(methodInfo, ignoreFieldsExpr, itemKeyExpr));
                }
                loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

                //var parameterName = ormProvider.ParameterPrefix + itemKey + suffix;
                Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix);
                if (isMultiple)
                {
                    myParameterNameExpr = Expression.Call(concatMethodInfo2, myParameterNameExpr, itemKeyExpr, suffixExpr);
                    loopBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                    myParameterNameExpr = parameterNameExpr;
                }
                else myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, itemKeyExpr);

                //updateFields.Add($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetFieldName));
                Expression fieldNameExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.FieldName));
                fieldNameExpr = Expression.Call(ormProviderExpr, methodInfo, fieldNameExpr);
                var contentExpr = Expression.Call(concatMethodInfo2, fieldNameExpr, Expression.Constant("="), myParameterNameExpr);

                loopBodies.Add(Expression.Call(updateFieldsExpr, addMethodInfo, contentExpr));
                loopBodies.Add(Expression.Assign(itemValueExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Value))));
                AddValueParameter(dbContext, dbParametersExpr, ormProviderExpr, myParameterNameExpr, itemValueExpr, memberMapperExpr, loopBodies);

                //index++;
                loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));
                blockBodies.Add(Expression.Loop(Expression.Block(loopBodies), breakLabel, continueLabel));
            }
            else
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var targetMemberInfos = updateObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

                var index = 0;
                foreach (var memberMapper in entityMapper.MemberMaps)
                {
                    if (!isDictionary && !targetMemberInfos.Exists(f => f.Name == memberMapper.MemberName))
                        continue;
                    if (memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsIgnoreUpdate || memberMapper.IsKey)
                        continue;
                    if (onlyFieldNames != null && !onlyFieldNames.Contains(memberMapper.MemberName))
                        continue;
                    if (ignoreFieldNames != null && ignoreFieldNames.Contains(memberMapper.MemberName))
                        continue;
                    if (!isUpdateRowVersion && memberMapper.IsRowVersion)
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + memberMapper.MemberName;
                    Expression myParameterNameExpr = Expression.Constant(parameterName);
                    Expression setFieldExpr = null;
                    if (isMultiple)
                    {
                        myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, suffixExpr);
                        blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                        setFieldExpr = Expression.Constant(ormProvider.GetFieldName(memberMapper.FieldName) + "=");
                        setFieldExpr = Expression.Call(concatMethodInfo, setFieldExpr, parameterNameExpr);
                        myParameterNameExpr = parameterNameExpr;
                    }
                    else setFieldExpr = Expression.Constant(ormProvider.GetFieldName(memberMapper.FieldName) + "=" + parameterName);
                    blockBodies.Add(Expression.Call(updateFieldsExpr, addMethodInfo, setFieldExpr));

                    var fieldValueType = targetMemberInfos.Find(f => f.Name == memberMapper.MemberName).GetMemberType();
                    var fieldValueExpr = Expression.PropertyOrField(typedUpdateObjExpr, memberMapper.MemberName);
                    AddValueParameter(dbContext, dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueType, fieldValueExpr, memberMapper, blockBodies);
                    index++;
                }
                if (index <= 0)
                    throw new Exception("没有找到更新字段或是忽略了字段大小写，无法识别到字段名称");
            }

            object result = null;
            if (isMultiple) result = Expression.Lambda<Action<IDataParameterCollection, DbContext, List<string>, object, string>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, updateFieldsExpr, updateObjExpr, suffixExpr).Compile();
            else result = Expression.Lambda<Action<IDataParameterCollection, DbContext, List<string>, object>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, updateFieldsExpr, updateObjExpr).Compile();
            return result;
        });
    }
    public static (Action<IDataParameterCollection, StringBuilder, DbContext, object, string>, Action<StringBuilder, DbContext, object, string>)
        BuildUpdateBulkSetWithSqlParametersPart(DbContext dbContext, Type entityType, Type updateObjType, bool isMultiple, bool isUpdateRowVersion, List<string> onlyFieldNames, List<string> ignoreFieldNames)
    {
        var ormProvider = dbContext.OrmProvider;
        var mapProvider = dbContext.MapProvider;
        var cacheKey = GetCacheKey(ormProvider.OrmProviderType, dbContext.MapProvider, entityType, updateObjType, onlyFieldNames, ignoreFieldNames);
        return updateBulkWithCommandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var fieldsSetter = BuildFieldsSqlParametersPart(dbContext, entityType, updateObjType, 4, 2, 3, false, true, isUpdateRowVersion, onlyFieldNames, ignoreFieldNames) as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            var whereSetter = BuildWhereSqlParametersPart(dbContext, entityType, updateObjType, 1, false, true, true, false, isMultiple, true, " WHERE ") as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            Action<IDataParameterCollection, StringBuilder, DbContext, object, string> firstSqlSetter = (dbParameters, builder, dbContext, updateObj, suffix) =>
            {
                fieldsSetter.Invoke(dbParameters, builder, dbContext, updateObj, suffix);
                whereSetter.Invoke(dbParameters, builder, dbContext, updateObj, suffix);
            };
            var fieldsSqlSetter = BuildFieldsSqlParametersPart(dbContext, entityType, updateObjType, 4, 2, 1, false, true, isUpdateRowVersion, onlyFieldNames, ignoreFieldNames) as Action<StringBuilder, DbContext, object, string>;
            var whereSqlSetter = BuildWhereSqlParametersPart(dbContext, entityType, updateObjType, 1, false, true, true, false, isMultiple, true, " WHERE ") as Action<StringBuilder, DbContext, object, string>;
            Action<StringBuilder, DbContext, object, string> shardingSqlSetter = (builder, dbContext, updateObj, suffix) =>
            {
                fieldsSqlSetter.Invoke(builder, dbContext, updateObj, suffix);
                whereSqlSetter.Invoke(builder, dbContext, updateObj, suffix);
            };
            return (firstSqlSetter, shardingSqlSetter);
        });
    }

    public static (bool, string, Action<StringBuilder, string>, object) BuildDeleteCommandInitializer(DbContext dbContext, Type entityType, Type whereObjType, bool isMultiple, bool isBulk)
    {
        var ormProvider = dbContext.OrmProvider;
        var mapProvider = dbContext.MapProvider;
        var cacheKey = GetCacheKey(ormProvider.OrmProviderType, mapProvider, entityType, whereObjType, isBulk);
        var commandInitializerCache = isBulk ? deleteBulkCommandInitializerCache : isMultiple ? deleteMultiCommandInitializerCache : deleteCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var isMultiKeys = entityMapper.KeyMembers.Count > 1;
            Action<StringBuilder, string> headSqlSetter = null;
            if (isBulk)
            {
                if (isMultiKeys) headSqlSetter = (builder, tableName) => builder.Append($"DELETE FROM {ormProvider.GetTableName(tableName)} WHERE ");
                else headSqlSetter = (builder, tableName) => builder.Append($"DELETE FROM {ormProvider.GetTableName(tableName)} WHERE {ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)} IN (");
            }
            else headSqlSetter = (builder, tableName) => builder.Append($"DELETE FROM {ormProvider.GetTableName(tableName)} WHERE ");
            bool isInExpr = isBulk && !isMultiKeys;
            var whereSqlParametersSetter = BuildWhereSqlParametersPart(dbContext, entityType, whereObjType, 1, false, true, false, isInExpr, isMultiple, isBulk);
            return (isMultiKeys, entityMapper.TableName, headSqlSetter, whereSqlParametersSetter);
        });
    }
    public static Dictionary<string, List<object>> SplitShardingParameters(IEntityMapProvider mapProvider, ITableShardingProvider shardingProvider, Type entityType, IEnumerable parameters)
    {
        var result = new Dictionary<string, List<object>>();
        Type parameterType = null;
        foreach (var parameter in parameters)
        {
            parameterType = parameter.GetType();
            break;
        }
        foreach (var parameter in parameters)
        {
            var tableName = RepositoryHelper.GetShardingTableName(mapProvider, shardingProvider, entityType, parameterType, parameter);
            if (!result.TryGetValue(tableName, out var myParameters))
                result.Add(tableName, myParameters = new List<object>());
            myParameters.Add(parameter);
        }
        return result;
    }
    public static string GetShardingTableName(IEntityMapProvider mapProvider, ITableShardingProvider shardingProvider, Type entityType, Type parameterType, object parameter)
    {
        var entityMapper = mapProvider.GetEntityMap(entityType);
        var tableName = entityMapper.TableName;
        if (TryBuildShardingTableNameGetter(shardingProvider, entityType, parameterType, out var tableNameGetter))
            return tableNameGetter.Invoke(tableName, parameter);
        return tableName;
    }
    public static bool TryBuildShardingTableNameGetter(ITableShardingProvider shardingProvider, Type entityType, Type parameterType, out Func<string, object, string> tableNameGetter)
    {
        if (shardingProvider == null || !shardingProvider.TryGetTableSharding(entityType, out var shardingTable))
        {
            tableNameGetter = null;
            return false;
        }
        if (shardingTable.DependOnMembers == null || shardingTable.DependOnMembers.Count == 0)
            throw new NotSupportedException($"实体表{entityType.FullName}有设置分表，但未指定依赖字段，插入数据无法确定分表");

        var cacheKey = GetCacheKey(entityType, parameterType);
        if (shardingTable.DependOnMembers.Count > 1)
        {
            if (typeof(IDictionary<string, object>).IsAssignableFrom(parameterType))
            {
                tableNameGetter = shardingTableNameGetters.GetOrAdd(cacheKey, f =>
                {
                    return (string origName, object parameter) =>
                    {
                        var dict = parameter as IDictionary<string, object>;
                        if (!dict.TryGetValue(shardingTable.DependOnMembers[0], out var field1Value))
                            throw new MissingMemberException($"实体表{entityType.FullName}已设置分表并依赖成员{shardingTable.DependOnMembers[0]}映射的字段，但当前字典中并不包含key:{shardingTable.DependOnMembers[0]}的键值");
                        if (!dict.TryGetValue(shardingTable.DependOnMembers[1], out var field2Value))
                            throw new MissingMemberException($"实体表{entityType.FullName}已设置分表并依赖成员{shardingTable.DependOnMembers[1]}映射的字段，但当前字典中不包含key:{shardingTable.DependOnMembers[1]}的键值");

                        var tableNameRuleGetter = shardingTable.Rule as Func<string, object, object, string>;
                        return tableNameRuleGetter.Invoke(origName, field1Value, field2Value);
                    };
                });
            }
            else
            {
                tableNameGetter = shardingTableNameGetters.GetOrAdd(cacheKey, f =>
                {
                    var origNameExpr = Expression.Parameter(typeof(string), "origName");
                    var parameterObjExpr = Expression.Parameter(typeof(object), "parameterObj");
                    var tableNameRuleGetter = shardingTable.Rule as Func<string, object, object, string>;

                    var members = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                        .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                    if (!members.Exists(f => f.Name == shardingTable.DependOnMembers[0]))
                        throw new MissingMemberException($"实体表{entityType.FullName}已设置分表并依赖成员{shardingTable.DependOnMembers[0]}映射的字段，但当前参数中并不包含{shardingTable.DependOnMembers[0]}成员");
                    if (!members.Exists(f => f.Name == shardingTable.DependOnMembers[1]))
                        throw new MissingMemberException($"实体表{entityType.FullName}已设置分表并依赖成员{shardingTable.DependOnMembers[1]}映射的字段，但当前参数中并不包含{shardingTable.DependOnMembers[1]}成员");

                    var typedParameterObjExpr = Expression.Convert(parameterObjExpr, parameterType);
                    Expression field1Expr = Expression.PropertyOrField(typedParameterObjExpr, shardingTable.DependOnMembers[0]);
                    if (field1Expr.Type != typeof(object))
                        field1Expr = Expression.Convert(field1Expr, typeof(object));
                    Expression field2Expr = Expression.PropertyOrField(typedParameterObjExpr, shardingTable.DependOnMembers[1]);
                    if (field2Expr.Type != typeof(object))
                        field2Expr = Expression.Convert(field2Expr, typeof(object));
                    var getterExpr = Expression.Constant(tableNameRuleGetter, typeof(Func<string, object, object, string>));
                    var bodyExpr = Expression.Invoke(getterExpr, origNameExpr, field1Expr, field2Expr);
                    return Expression.Lambda<Func<string, object, string>>(bodyExpr, origNameExpr, parameterObjExpr).Compile();
                });
            }
        }
        else
        {
            if (typeof(IDictionary<string, object>).IsAssignableFrom(parameterType))
            {
                tableNameGetter = shardingTableNameGetters.GetOrAdd(cacheKey, f =>
                {
                    return (string origName, object parameter) =>
                    {
                        var dict = parameter as IDictionary<string, object>;
                        if (!dict.TryGetValue(shardingTable.DependOnMembers[0], out var fieldValue))
                            throw new MissingMemberException($"实体表{entityType.FullName}已设置分表并依赖成员{shardingTable.DependOnMembers[0]}映射的字段，但当前字典中并不包含key:{shardingTable.DependOnMembers[0]}的键值");

                        var tableNameRuleGetter = shardingTable.Rule as Func<string, object, string>;
                        return tableNameRuleGetter.Invoke(origName, fieldValue);
                    };
                });
            }
            else
            {
                tableNameGetter = shardingTableNameGetters.GetOrAdd(cacheKey, f =>
                {
                    var origNameExpr = Expression.Parameter(typeof(string), "origName");
                    var parameterObjExpr = Expression.Parameter(typeof(object), "parameterObj");
                    var tableNameRuleGetter = shardingTable.Rule as Func<string, object, string>;

                    var members = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                        .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                    if (!members.Exists(f => f.Name == shardingTable.DependOnMembers[0]))
                        throw new MissingMemberException($"实体表{entityType.FullName}已设置分表并依赖成员{shardingTable.DependOnMembers[0]}映射的字段，但当前参数中并不包含{shardingTable.DependOnMembers[0]}成员");

                    var typedParameterObjExpr = Expression.Convert(parameterObjExpr, parameterType);
                    Expression fieldExpr = Expression.PropertyOrField(typedParameterObjExpr, shardingTable.DependOnMembers[0]);
                    if (fieldExpr.Type != typeof(object))
                        fieldExpr = Expression.Convert(fieldExpr, typeof(object));
                    var getterExpr = Expression.Constant(tableNameRuleGetter, typeof(Func<string, object, string>));
                    var bodyExpr = Expression.Invoke(getterExpr, origNameExpr, fieldExpr);
                    return Expression.Lambda<Func<string, object, string>>(bodyExpr, origNameExpr, parameterObjExpr).Compile();
                });
            }
        }
        return true;
    }
    public static void BuildDictWhereSqlParameters(IDataParameterCollection dbParameters, StringBuilder builder, DbContext dbContext, IDictionary<string, object> dict)
    {
        EntityMap entityMapper = null;
        var ormProvider = dbContext.OrmProvider;
        foreach (var item in dict)
        {
            if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                || memberMapper.IsIgnore || memberMapper.IsNavigation)
                continue;
            builder.Append($"{memberMapper.FieldName}={item.Value}");
            var valueGetter = ormProvider.GetParameterValueGetter(item.Value.GetType(), memberMapper.UnderlyingType, !memberMapper.IsRequired, dbContext.Options);
            valueGetter.Invoke(item.Value);
        }
    }
    public static object CreateListInstance(Type elementType)
    {
        var typedListGetter = typedListGetters.GetOrAdd(elementType, f =>
        {
            var listType = typeof(List<>).MakeGenericType(elementType);
            var bodyExpr = Expression.New(listType.GetConstructor(Type.EmptyTypes));
            return Expression.Lambda<Func<object>>(bodyExpr).Compile();
        });
        return typedListGetter.Invoke();
    }
    public static object CreateListInstance(Type elementType, object parameters)
    {
        var typedListGetter = typedInitListGetters.GetOrAdd(elementType, f =>
        {
            var collectionExpr = Expression.Parameter(typeof(object), "collection");
            var parametersType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var typedCollectionExpr = Expression.Convert(collectionExpr, parametersType);
            var listType = typeof(List<>).MakeGenericType(elementType);
            var bodyExpr = Expression.New(listType.GetConstructor([parametersType]), typedCollectionExpr);
            return Expression.Lambda<Func<object, object>>(bodyExpr, collectionExpr).Compile();
        });
        return typedListGetter.Invoke(parameters);
    }
    public static object CreateCollectionInstance(Type elementType, object parameters)
    {
        var typedCollectionGetter = typedCollectionGetters.GetOrAdd(elementType, f =>
        {
            var listExpr = Expression.Parameter(typeof(object), "collection");
            var parametersType = typeof(IList<>).MakeGenericType(elementType);
            var typedListExpr = Expression.Convert(listExpr, parametersType);
            var collectionType = typeof(Collection<>).MakeGenericType(elementType);
            var bodyExpr = Expression.New(collectionType.GetConstructor([parametersType]), typedListExpr);
            return Expression.Lambda<Func<object, object>>(bodyExpr, listExpr).Compile();
        });
        return typedCollectionGetter.Invoke(parameters);
    }
    public static object ToArray(Type elementType, object parameters)
    {
        var toArrayGetter = toArrayGetters.GetOrAdd(elementType, f =>
        {
            var enumerableExpr = Expression.Parameter(typeof(object), "enumerable");
            var parametersType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var typedEnumerableExpr = Expression.Convert(enumerableExpr, parametersType);
            var methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray), [parametersType]);
            var bodyExpr = Expression.Call(methodInfo, typedEnumerableExpr);
            return Expression.Lambda<Func<object>>(bodyExpr, enumerableExpr).Compile();
        });
        return toArrayGetter.Invoke(parameters);
    }
    public static DateTime ToUtcTime(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Local)
            return dateTime.ToUniversalTime();
        return dateTime;
    }
    public static DateTimeOffset ToUtcTime(DateTimeOffset dateTimeOffset)
    {
        if (dateTimeOffset.DateTime.Kind == DateTimeKind.Local)
            return dateTimeOffset.ToUniversalTime();
        return dateTimeOffset;
    }
    public static DateTime ToLocalTime(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
            return dateTime.ToLocalTime();
        return dateTime;
    }
    public static DateTimeOffset ToLocalTime(DateTimeOffset dateTimeOffset)
    {
        if (dateTimeOffset.DateTime.Kind == DateTimeKind.Utc)
            return dateTimeOffset.ToLocalTime();
        return dateTimeOffset;
    }
    public static int GetCacheKey(params object[] parameters)
    {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        var hashCode = new HashCode();
        foreach (var parameter in parameters)
        {
            hashCode.Add(parameter);
        }
        return hashCode.ToHashCode();
#else
        int hashCode = 17;
        unchecked
        {
            foreach (var parameter in parameters)
            {
                hashCode = hashCode * 23 + (parameter?.GetHashCode() ?? 0);
            }
        }
        return hashCode;
#endif
    }
}