using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

    private static ConcurrentDictionary<int, Action<StringBuilder, object>> createFieldsSqlCache = new();
    private static ConcurrentDictionary<int, object> createValuesSqlParametersCache = new();
    private static ConcurrentDictionary<int, object> createBulkValuesSqlParametersCache = new();

    private static ConcurrentDictionary<int, (bool, string, object, Action<StringBuilder, string>)> deleteCommandInitializerCache = new();
    private static ConcurrentDictionary<int, (bool, string, object, Action<StringBuilder, string>)> deleteMultiCommandInitializerCache = new();
    private static ConcurrentDictionary<int, (bool, string, Action<StringBuilder, string>, Action<IDataParameterCollection, StringBuilder, IOrmProvider, string, object, string>)> deleteBulkCommandInitializerCache = new();

    private static ConcurrentDictionary<int, (string, Action<StringBuilder, string>, object, object)> updateCommandInitializerCache = new();
    private static ConcurrentDictionary<int, (string, Action<StringBuilder, string>, object, object)> updateMultiCommandInitializerCache = new();

    private static ConcurrentDictionary<int, object> updateWithCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> updateMultiWithCommandInitializerCache = new();

    private static ConcurrentDictionary<int, Func<string, string, object, string>> shardingTableNameGetters = new();

    public static void AddValueParameter(IOrmProvider ormProvider, Expression dbParametersExpr, Expression ormProviderExpr, Expression parameterNameExpr,
        Type fieldValueType, Expression parameterValueExpr, MemberMap memberMapper, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        MethodInfo methodInfo = null;
        var fieldValueExpr = parameterValueExpr;
        var addMethodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add));
        var createParameterMethodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });

        if (memberMapper.TypeHandler != null)
        {
            var typeHandlerExpr = Expression.Constant(memberMapper.TypeHandler);
            methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
            if (fieldValueType != typeof(object))
                fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
            fieldValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, fieldValueExpr);
        }
        else
        {
            var targetType = ormProvider.MapDefaultType(memberMapper.NativeDbType);
            var valueGetter = ormProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired);
            if (fieldValueType != typeof(object))
                fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
            fieldValueExpr = Expression.Invoke(Expression.Constant(valueGetter), fieldValueExpr);
        }

        Expression nativeDbTypeExpr = Expression.Constant(memberMapper.NativeDbType);
        if (nativeDbTypeExpr.Type != typeof(object))
            nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));
        var dbParameterExpr = Expression.Call(ormProviderExpr, createParameterMethodInfo, parameterNameExpr, nativeDbTypeExpr, fieldValueExpr);
        //Expression dbParameterExpr = null;

        ////可为null类型，要判断是否=null
        //if (!memberMapper.IsRequired && (fieldValueType.IsNullableType(out _) || !fieldValueType.IsValueType))
        //{
        //    var isNullExpr = Expression.NotEqual(parameterValueExpr, Expression.Constant(null));
        //    var setNullExpr = Expression.Call(ormProviderExpr, createParameterMethodInfo, parameterNameExpr, nativeDbTypeExpr, Expression.Constant(DBNull.Value));
        //    dbParameterExpr = Expression.IfThenElse(isNullExpr, setNullExpr, setParameterExpr);
        //}
        ////不可为null，直接赋值
        //else dbParameterExpr = setParameterExpr;
        blockBodies.Add(Expression.Call(dbParametersExpr, addMethodInfo, dbParameterExpr));
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

    public static string BuildFieldsSqlPart(IOrmProvider ormProvider, EntityMap entityMapper, Type selectType, bool isNeedAs)
    {
        var index = 0;
        var memberInfos = selectType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

        var builder = new StringBuilder();
        foreach (var memberInfo in memberInfos)
        {
            if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                || memberMapper.IsIgnore || memberMapper.IsNavigation
                || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                continue;

            if (index > 0) builder.Append(',');
            builder.Append(ormProvider.GetFieldName(memberMapper.FieldName));
            if (isNeedAs && memberMapper.FieldName != memberMapper.MemberName)
                builder.Append(" AS " + ormProvider.GetFieldName(memberMapper.MemberName));
            index++;
        }
        return builder.ToString();
    }
    public static object BuildWhereSqlParameters(bool isFunc, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type whereObjType, bool isWhereKey, bool hasSuffix, bool isWithKey, bool isOnlyParameters, string whereObjName, string headSql = null)
    {
        object commandInitializer = null;
        var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
        var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
        var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
        var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

        ParameterExpression suffixExpr = null;
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();
        MethodInfo methodInfo = null;
        if (hasSuffix) suffixExpr = Expression.Parameter(typeof(string), "suffix");

        if (isFunc)
        {
            blockParameters.Add(builderExpr);
            var constructorExpr = typeof(StringBuilder).GetConstructor(Type.EmptyTypes);
            blockBodies.Add(Expression.Assign(builderExpr, Expression.New(constructorExpr)));
        }

        var entityMapper = mapProvider.GetEntityMap(entityType);
        var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });

        if (!string.IsNullOrEmpty(headSql))
            blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(headSql)));

        if (typeof(IDictionary<string, object>).IsAssignableFrom(whereObjType))
        {
            var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
            var dictExpr = Expression.Variable(typeof(IDictionary<string, object>), "dict");
            var fieldValueExpr = Expression.Variable(typeof(object), "fieldValue");
            blockParameters.AddRange(new[] { dictExpr, fieldValueExpr, parameterNameExpr });
            blockBodies.Add(Expression.Assign(dictExpr, Expression.Convert(whereObjExpr, typeof(IDictionary<string, object>))));

            var index = 0;
            var concatMethodInfo1 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
            var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });
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
                        myParameterNameExpr = Expression.Call(concatMethodInfo1, myParameterNameExpr, suffixExpr);
                        blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                        myParameterNameExpr = parameterNameExpr;
                    }
                    if (!isOnlyParameters)
                    {
                        if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                        var fieldNameExpr = Expression.Constant($"{ormProvider.GetFieldName(keyMapper.FieldName)}=");
                        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
                    }

                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, myParameterNameExpr));
                    var typedFieldValueExpr = Expression.Convert(fieldValueExpr, keyMapper.MemberType);
                    AddValueParameter(ormProvider, dbParametersExpr, ormProviderExpr, parameterNameExpr, keyMapper.MemberType, typedFieldValueExpr, keyMapper, blockParameters, blockBodies);
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
                ParameterExpression dbFieldValueExpr = null;

                //var index = 0;
                //var enumerator = dict.GetEnumerator();
                //var entityMapper = new EntityMap{ ... };
                var entityMapperExpr = Expression.Constant(entityMapper);
                blockBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
                methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
                blockBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(dictExpr, methodInfo)));

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

                //var parameterName = ormProvider.ParameterPrefix + itemKey + suffix;
                Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix + (isWithKey ? "k" : ""));
                if (hasSuffix)
                    myParameterNameExpr = Expression.Call(concatMethodInfo2, myParameterNameExpr, itemKeyExpr, suffixExpr);
                else myParameterNameExpr = Expression.Call(concatMethodInfo1, myParameterNameExpr, itemKeyExpr);
                loopBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));

                if (!isOnlyParameters)
                {
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
                }
                loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));

                //object dbFieldValue=null;
                //if(memberMapper.TypeHandler!=null)
                //  dbFieldValue = memberMapper.TypeHandler.ToFieldValue(ormProvider, dbFieldValue);
                //else
                //{
                //    var targetType = this.OrmProvider.MapDefaultType(memberMapper.NativeDbType);
                //    var valueGetter = this.OrmProvider.GetParameterValueGetter(sqlSegment.SegmentType, targetType, false);
                //    dbFieldValue = valueGetter.Invoke(dbFieldValue);
                //}
                Expression nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
                var typeHandlerExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler));
                methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
                Expression myFieldValueExpr = fieldValueExpr;
                if (fieldValueExpr.Type != typeof(object))
                    myFieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                var typeHandlerValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, myFieldValueExpr);

                nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));

                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.MapDefaultType));
                var targetTypeExpr = Expression.Call(ormProviderExpr, methodInfo, nativeDbTypeExpr);
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetParameterValueGetter));
                var valueGetterExpr = Expression.Call(ormProviderExpr, methodInfo, Expression.Constant(fieldValueExpr.Type), targetTypeExpr, Expression.Constant(false));
                var valueGetterValueExpr = Expression.Invoke(valueGetterExpr, myFieldValueExpr);

                if (dbFieldValueExpr == null)
                {
                    dbFieldValueExpr = Expression.Variable(typeof(object), "objValue");
                    blockParameters.Add(dbFieldValueExpr);
                }

                var isNotNullExpr = Expression.IsFalse(Expression.Equal(typeHandlerExpr, Expression.Constant(null)));
                var setTypeHandlerValueExpr = Expression.Assign(dbFieldValueExpr, typeHandlerValueExpr);
                var setValueGetterValueExpr = Expression.Assign(dbFieldValueExpr, valueGetterValueExpr);
                blockBodies.Add(Expression.IfThenElse(isNotNullExpr, setTypeHandlerValueExpr, setValueGetterValueExpr));


                //dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue);
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                var dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, dbFieldValueExpr);
                methodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add), new Type[] { typeof(object) });
                blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr));

                //index++;
                loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));

                blockBodies.Add(Expression.Loop(Expression.Block(loopBodies), breakLabel, continueLabel));
            }
        }
        else
        {
            ParameterExpression parameterNameExpr = null;
            ParameterExpression typedWhereObjExpr = null;
            bool isEntityType = false;
            List<MemberInfo> targetMemberInfos = null;
            List<MemberInfo> whereMemberInfos = null;
            if (hasSuffix)
            {
                parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                blockParameters.Add(parameterNameExpr);
            }
            targetMemberInfos = whereObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
            if (whereObjType.IsEntityType(out _))
            {
                isEntityType = true;
                typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");
                blockParameters.Add(typedWhereObjExpr);
                blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));
                if (isWhereKey) whereMemberInfos = entityMapper.KeyMembers.Select(f => f.Member).ToList();
                else whereMemberInfos = targetMemberInfos;
            }
            else
            {
                if (entityMapper.KeyMembers.Count > 1)
                    throw new NotSupportedException($"模型{entityType.FullName}有多个主键字段，不能使用单个值类型{whereObjType.FullName}作为参数");
                whereMemberInfos = entityMapper.KeyMembers.Select(f => f.Member).ToList();
            }

            var index = 0;
            var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
            foreach (var memberInfo in whereMemberInfos)
            {
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                    || memberMapper.IsIgnore || memberMapper.IsNavigation
                    || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                    continue;

                if (isWhereKey && memberMapper.IsKey && isEntityType && !targetMemberInfos.Exists(f => f.Name == memberMapper.MemberName))
                    throw new ArgumentNullException(whereObjName, $"参数类型{whereObjType.FullName}缺少主键字段{memberMapper.MemberName}");
                if (isWhereKey && !memberMapper.IsKey) continue;

                Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix + (isWithKey ? "k" : "") + memberMapper.MemberName);
                if (hasSuffix)
                {
                    myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, suffixExpr);
                    blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                    myParameterNameExpr = parameterNameExpr;
                }

                if (!isOnlyParameters)
                {
                    if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                    var fieldNameExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}=");
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
                }
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, myParameterNameExpr));

                if (isEntityType)
                {
                    var fieldValueExpr = Expression.PropertyOrField(typedWhereObjExpr, memberMapper.MemberName);
                    var fieldValueType = targetMemberInfos.Find(f => f.Name == memberMapper.MemberName).GetMemberType();
                    AddValueParameter(ormProvider, dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueType, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                }
                else
                {
                    var fieldValueExpr = Expression.Convert(whereObjExpr, memberMapper.MemberType);
                    AddValueParameter(ormProvider, dbParametersExpr, ormProviderExpr, myParameterNameExpr, memberMapper.MemberType, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                }
                index++;
            }
        }

        if (isFunc)
        {
            methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
            var returnExpr = Expression.Call(builderExpr, methodInfo);
            var resultLabelExpr = Expression.Label(typeof(string));
            blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
            blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));

            if (hasSuffix) commandInitializer = Expression.Lambda<Func<IDataParameterCollection, IOrmProvider, object, string, string>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, whereObjExpr, suffixExpr).Compile();
            else commandInitializer = Expression.Lambda<Func<IDataParameterCollection, IOrmProvider, object, string>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, whereObjExpr).Compile();
        }
        else
        {
            if (hasSuffix) commandInitializer = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, ormProviderExpr, whereObjExpr, suffixExpr).Compile();
            else commandInitializer = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, IOrmProvider, object>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, ormProviderExpr, whereObjExpr).Compile();
        }
        return commandInitializer;
    }
    public static object BuildSqlParametersPart(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type parametersType, bool isUpdate, bool isFunc, bool isOnlySql, bool isUseKey, bool isWithKey, bool isOnlyParameters, bool hasSuffix, bool isIgnoreKeys, List<string> onlyFieldNames, List<string> ignoreFieldNames, string jointMark, string headSql)
    {
        object commandInitializer = null;
        var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
        var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
        var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
        var parametersExpr = Expression.Parameter(typeof(object), "parameters");

        ParameterExpression suffixExpr = null;
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();
        MethodInfo methodInfo = null;
        if (hasSuffix) suffixExpr = Expression.Parameter(typeof(string), "suffix");
        if (isFunc)
        {
            blockParameters.Add(builderExpr);
            var constructorExpr = typeof(StringBuilder).GetConstructor(Type.EmptyTypes);
            blockBodies.Add(Expression.Assign(builderExpr, Expression.New(constructorExpr)));
        }
        var entityMapper = mapProvider.GetEntityMap(entityType);
        var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
        if (!string.IsNullOrEmpty(headSql))
            blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(headSql)));

        if (typeof(IDictionary<string, object>).IsAssignableFrom(parametersType))
        {
            var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
            var dictExpr = Expression.Variable(typeof(IDictionary<string, object>), "dict");
            var fieldValueExpr = Expression.Variable(typeof(object), "fieldValue");
            blockParameters.AddRange(new[] { dictExpr, fieldValueExpr, parameterNameExpr });
            blockBodies.Add(Expression.Assign(dictExpr, Expression.Convert(parametersExpr, typeof(IDictionary<string, object>))));

            var index = 0;
            var concatMethodInfo1 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
            var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });
            if (isUseKey)
            {
                var tryGetValueMethodInfo = typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.TryGetValue));
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    if (onlyFieldNames != null && !onlyFieldNames.Contains(keyMapper.MemberName))
                        continue;
                    if (ignoreFieldNames != null && ignoreFieldNames.Contains(keyMapper.MemberName))
                        continue;

                    var keyMemberExpr = Expression.Constant(keyMapper.MemberName);
                    var isFalseExpr = Expression.IsFalse(Expression.Call(dictExpr, tryGetValueMethodInfo, keyMemberExpr, fieldValueExpr));
                    var exceptionExpr = Expression.Constant(new Exception($"字典参数缺少主键字段{keyMapper.MemberName}，区分大小写"));
                    blockBodies.Add(Expression.IfThen(isFalseExpr, Expression.Throw(exceptionExpr)));

                    var parameterName = ormProvider.ParameterPrefix + (isWithKey ? "k" : "") + keyMapper.MemberName;
                    Expression myParameterNameExpr = Expression.Constant(parameterName);
                    if (hasSuffix)
                    {
                        myParameterNameExpr = Expression.Call(concatMethodInfo1, myParameterNameExpr, suffixExpr);
                        blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                        myParameterNameExpr = parameterNameExpr;
                    }
                    if (!isOnlyParameters)
                    {
                        if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(jointMark)));
                        var fieldNameExpr = Expression.Constant($"{ormProvider.GetFieldName(keyMapper.FieldName)}=");
                        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
                    }

                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, myParameterNameExpr));
                    if (!isOnlySql)
                    {
                        var typedFieldValueExpr = Expression.Convert(fieldValueExpr, keyMapper.MemberType);
                        AddValueParameter(ormProvider, dbParametersExpr, ormProviderExpr, parameterNameExpr, keyMapper.MemberType, typedFieldValueExpr, keyMapper, blockParameters, blockBodies);
                    }
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
                ParameterExpression dbFieldValueExpr = null;

                //var index = 0;
                //var enumerator = dict.GetEnumerator();
                //var entityMapper = new EntityMap{ ... };
                var entityMapperExpr = Expression.Constant(entityMapper);
                blockBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
                methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
                blockBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(dictExpr, methodInfo)));

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
                loopBodies.Add(Expression.Assign(fieldValueExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Value))));

                //var isContinue = !entityMapper.TryGetMemberMap(itemKey, out var memberMapper)
                //|| memberMapper.IsIgnore || memberMapper.IsNavigation
                methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.TryGetMemberMap));
                Expression isContinueExpr = Expression.IsFalse(Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr, memberMapperExpr));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnore)));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsNavigation)));

                //|| memberMapper.IsRowVersion || memberMapper.IsIgnoreUpdate
                if (isUpdate)
                {
                    isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnoreUpdate)));
                    isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsRowVersion)));
                }

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
                if (isIgnoreKeys)
                {
                    var keyNames = entityMapper.KeyMembers.Select(f => f.MemberName).ToArray();
                    var keyNamesExpr = Expression.Constant(keyNames);
                    methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), new Type[] { typeof(string) });
                    var isFalseExpr = Expression.IsFalse(Expression.Call(methodInfo, keyNamesExpr, itemKeyExpr));
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
                Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix + (isWithKey ? "k" : ""));
                if (hasSuffix)
                    myParameterNameExpr = Expression.Call(concatMethodInfo2, myParameterNameExpr, itemKeyExpr, suffixExpr);
                else myParameterNameExpr = Expression.Call(concatMethodInfo1, myParameterNameExpr, itemKeyExpr);
                loopBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));

                if (!isOnlyParameters)
                {
                    //if(index > 0) builder.Append(" AND ");
                    var greaterThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0));
                    var callExpr = Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(jointMark));
                    loopBodies.Add(Expression.IfThen(greaterThenExpr, callExpr));

                    //builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                    methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetFieldName));
                    Expression fieldNameExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.FieldName));
                    fieldNameExpr = Expression.Call(ormProviderExpr, methodInfo, fieldNameExpr);
                    loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
                    loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant("=")));
                }
                loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));

                if (!isOnlySql)
                {
                    //object dbFieldValue=null;
                    //if(memberMapper.TypeHandler!=null)
                    //  dbFieldValue = memberMapper.TypeHandler.ToFieldValue(ormProvider, dbFieldValue);
                    //else
                    //{
                    //    var targetType = this.OrmProvider.MapDefaultType(memberMapper.NativeDbType);
                    //    var valueGetter = this.OrmProvider.GetParameterValueGetter(sqlSegment.SegmentType, targetType, false);
                    //    dbFieldValue = valueGetter.Invoke(dbFieldValue);
                    //}
                    Expression nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
                    var typeHandlerExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler));
                    methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
                    Expression myFieldValueExpr = fieldValueExpr;
                    if (fieldValueExpr.Type != typeof(object))
                        myFieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                    var typeHandlerValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, myFieldValueExpr);

                    nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));

                    methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.MapDefaultType));
                    var targetTypeExpr = Expression.Call(ormProviderExpr, methodInfo, nativeDbTypeExpr);
                    methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetParameterValueGetter));
                    var valueGetterExpr = Expression.Call(ormProviderExpr, methodInfo, Expression.Constant(fieldValueExpr.Type), targetTypeExpr, Expression.Constant(false));
                    var valueGetterValueExpr = Expression.Invoke(valueGetterExpr, myFieldValueExpr);

                    if (dbFieldValueExpr == null)
                    {
                        dbFieldValueExpr = Expression.Variable(typeof(object), "objValue");
                        blockParameters.Add(dbFieldValueExpr);
                    }

                    var isNotNullExpr = Expression.IsFalse(Expression.Equal(typeHandlerExpr, Expression.Constant(null)));
                    var setTypeHandlerValueExpr = Expression.Assign(dbFieldValueExpr, typeHandlerValueExpr);
                    var setValueGetterValueExpr = Expression.Assign(dbFieldValueExpr, valueGetterValueExpr);
                    blockBodies.Add(Expression.IfThenElse(isNotNullExpr, setTypeHandlerValueExpr, setValueGetterValueExpr));


                    //dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue);
                    methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                    var dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, dbFieldValueExpr);
                    methodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add), new Type[] { typeof(object) });
                    blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr));
                }

                //index++;
                loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));

                blockBodies.Add(Expression.Loop(Expression.Block(loopBodies), breakLabel, continueLabel));
            }
        }
        else
        {
            ParameterExpression parameterNameExpr = null;
            ParameterExpression typedParametersExpr = null;
            bool isEntityType = false;
            List<MemberInfo> targetMemberInfos = null;
            List<MemberInfo> filterMemberInfos = null;
            if (hasSuffix)
            {
                parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                blockParameters.Add(parameterNameExpr);
            }
            targetMemberInfos = parametersType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
            if (parametersType.IsEntityType(out _))
            {
                isEntityType = true;
                typedParametersExpr = Expression.Variable(parametersType, "typedParameters");
                blockParameters.Add(typedParametersExpr);
                blockBodies.Add(Expression.Assign(typedParametersExpr, Expression.Convert(parametersExpr, parametersType)));
                if (isUseKey) filterMemberInfos = entityMapper.KeyMembers.Select(f => f.Member).ToList();
                else filterMemberInfos = targetMemberInfos;
            }
            else filterMemberInfos = entityMapper.KeyMembers.Select(f => f.Member).ToList();

            var index = 0;
            var keyNames = entityMapper.KeyMembers.Select(f => f.MemberName).ToArray();
            var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
            foreach (var memberInfo in filterMemberInfos)
            {
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                    || memberMapper.IsIgnore || memberMapper.IsNavigation
                    || (isUpdate && (memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion))
                    || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                    continue;

                if (isUseKey && memberMapper.IsKey && isEntityType && !targetMemberInfos.Exists(f => f.Name == memberMapper.MemberName))
                    throw new Exception($"参数类型{parametersType.FullName}缺少主键字段{memberMapper.MemberName}");
                if (isUseKey && !memberMapper.IsKey) continue;

                if (onlyFieldNames != null && !onlyFieldNames.Contains(memberInfo.Name))
                    continue;
                if (ignoreFieldNames != null && ignoreFieldNames.Contains(memberInfo.Name))
                    continue;
                if (isIgnoreKeys && keyNames.Contains(memberInfo.Name))
                    continue;

                Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix + (isWithKey ? "k" : "") + memberMapper.MemberName);
                if (hasSuffix)
                {
                    myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, suffixExpr);
                    blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                    myParameterNameExpr = parameterNameExpr;
                }

                if (!isOnlyParameters)
                {
                    if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(jointMark)));
                    var fieldNameExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}=");
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
                }
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, myParameterNameExpr));

                if (!isOnlySql)
                {
                    if (isEntityType)
                    {
                        var fieldValueExpr = Expression.PropertyOrField(typedParametersExpr, memberMapper.MemberName);
                        var fieldValueType = targetMemberInfos.Find(f => f.Name == memberMapper.MemberName).GetMemberType();
                        AddValueParameter(ormProvider, dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueType, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                    }
                    else
                    {
                        var fieldValueExpr = Expression.Convert(parametersExpr, memberMapper.MemberType);
                        AddValueParameter(ormProvider, dbParametersExpr, ormProviderExpr, myParameterNameExpr, memberMapper.MemberType, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                    }
                }
                index++;
            }
        }

        if (isFunc)
        {
            methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
            var returnExpr = Expression.Call(builderExpr, methodInfo);
            var resultLabelExpr = Expression.Label(typeof(string));
            blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
            blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));

            if (isOnlySql)
            {
                if (hasSuffix) commandInitializer = Expression.Lambda<Func<StringBuilder, IOrmProvider, object, string, string>>(
                    Expression.Block(blockParameters, blockBodies), ormProviderExpr, parametersExpr, suffixExpr).Compile();
                else commandInitializer = Expression.Lambda<Func<StringBuilder, IOrmProvider, object, string>>(
                    Expression.Block(blockParameters, blockBodies), ormProviderExpr, parametersExpr).Compile();
            }
            else
            {
                if (hasSuffix) commandInitializer = Expression.Lambda<Func<IDataParameterCollection, IOrmProvider, object, string, string>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, parametersExpr, suffixExpr).Compile();
                else commandInitializer = Expression.Lambda<Func<IDataParameterCollection, IOrmProvider, object, string>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, parametersExpr).Compile();
            }
        }
        else
        {
            if (isOnlySql)
            {
                if (hasSuffix) commandInitializer = Expression.Lambda<Action<StringBuilder, IOrmProvider, object, string>>(
                    Expression.Block(blockParameters, blockBodies), builderExpr, ormProviderExpr, parametersExpr, suffixExpr).Compile();
                else commandInitializer = Expression.Lambda<Action<StringBuilder, IOrmProvider, object>>(
                    Expression.Block(blockParameters, blockBodies), builderExpr, ormProviderExpr, parametersExpr).Compile();
            }
            else
            {
                if (hasSuffix) commandInitializer = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, ormProviderExpr, parametersExpr, suffixExpr).Compile();
                else commandInitializer = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, IOrmProvider, object>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, ormProviderExpr, parametersExpr).Compile();
            }
        }
        return commandInitializer;
    }
    public static object BuildGetSqlParameters(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiple)
    {
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(ormProvider, mapProvider, entityType, whereObjType);
        var commandInitializerCache = isMultiple ? queryMultiGetCommandInitializerCache : queryGetCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            string tableName = ormProvider.GetTableName(entityMapper.TableName);
            var fieldsSql = BuildFieldsSqlPart(ormProvider, entityMapper, entityType, true);
            var headSql = $"SELECT {fieldsSql} FROM {tableName} WHERE ";
            return BuildSqlParametersPart(ormProvider, mapProvider, entityType, whereObjType, false, true, false, true, false, false, isMultiple, false, null, null, " AND ", headSql);
        });
    }
    public static object BuildQueryWhereObjSqlParameters(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiple)
    {
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(ormProvider, mapProvider, entityType, whereObjType);
        var commandInitializerCache = isMultiple ? queryMultiWhereObjCommandInitializerCache : queryWhereObjCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            string tableName = ormProvider.GetTableName(entityMapper.TableName);
            var fieldsSql = BuildFieldsSqlPart(ormProvider, entityMapper, entityType, true);
            var headSql = $"SELECT {fieldsSql} FROM {tableName} WHERE ";
            return BuildSqlParametersPart(ormProvider, mapProvider, entityType, whereObjType, false, true, false, false, false, false, isMultiple, false, null, null, " AND ", headSql);
        });
    }
    public static object BuildExistsSqlParameters(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiple)
    {
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(ormProvider, mapProvider, entityType, whereObjType);
        var commandInitializerCache = isMultiple ? queryMultiExistsCommandInitializerCache : queryExistsCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var headSql = $"SELECT COUNT(1) FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ";
            return BuildWhereSqlParameters(true, ormProvider, mapProvider, entityType, whereObjType, false, isMultiple, false, false, nameof(whereObj), headSql);
        });
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
            var cacheKey = HashCode.Combine(rawSql, parameterType);
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

    public static (string, Action<StringBuilder, string, object>, object) BuildCreateSqlParameters(IOrmProvider ormProvider, IEntityMapProvider mapProvider,
        Type entityType, Type insertObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool hasSuffix, bool isReturnIdentity)
    {
        var entityMapper = mapProvider.GetEntityMap(entityType);
        var tableName = entityMapper.TableName;
        var fieldsSqlPartSetter = BuildCreateFieldsSqlPart(ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames);
        var valuesSqlPartSetter = BuildCreateValuesSqlParametes(ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames, hasSuffix);

        Action<StringBuilder, string, object> headSqlSetter = null;
        object valuesSqlSetter = null;
        headSqlSetter = (builder, tableName, insertObj) =>
        {
            builder.Append($"INSERT INTO {ormProvider.GetFieldName(tableName)} (");
            fieldsSqlPartSetter.Invoke(builder, insertObj);
            builder.Append(')');
        };
        if (hasSuffix)
        {
            var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>;
            Action<IDataParameterCollection, StringBuilder, object, string> typedValuesSqlSetter = (dbParameters, builder, insertObj, suffix) =>
            {
                builder.Append('(');
                typedValuesSqlPartSetter.Invoke(dbParameters, builder, ormProvider, insertObj, suffix);
                builder.Append(')');
            };
            valuesSqlSetter = typedValuesSqlSetter;
        }
        else
        {
            var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object>;
            if (isReturnIdentity)
            {
                Action<IDataParameterCollection, StringBuilder, object> typedValuesSqlSetter = (dbParameters, builder, insertObj) =>
                {
                    builder.Append('(');
                    typedValuesSqlPartSetter.Invoke(dbParameters, builder, ormProvider, insertObj);
                    builder.Append(')');
                    var keyFieldName = ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName);
                    builder.Append(ormProvider.GetIdentitySql(keyFieldName));
                };
                valuesSqlSetter = typedValuesSqlSetter;
            }
            else
            {
                Action<IDataParameterCollection, StringBuilder, object> typedValuesSqlSetter = (dbParameters, builder, insertObj) =>
                {
                    builder.Append('(');
                    typedValuesSqlPartSetter.Invoke(dbParameters, builder, ormProvider, insertObj);
                    builder.Append(')');
                };
                valuesSqlSetter = typedValuesSqlSetter;
            }
        }
        return (tableName, headSqlSetter, valuesSqlSetter);
    }
    public static Action<StringBuilder, object> BuildCreateFieldsSqlPart(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type insertObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames)
    {
        var cacheKey = HashCode.Combine(ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames);
        return createFieldsSqlCache.GetOrAdd(cacheKey, f =>
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
                            || memberMapper.IsIgnore || memberMapper.IsIgnoreInsert
                            || memberMapper.IsNavigation || memberMapper.IsAutoIncrement || memberMapper.IsRowVersion
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
                var blockBodies = new List<Expression>();
                var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                var memberInfos = insertObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

                var index = 0;
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsIgnoreInsert
                        || memberMapper.IsNavigation || memberMapper.IsAutoIncrement || memberMapper.IsRowVersion
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;

                    if (ignoreFieldNames != null && ignoreFieldNames.Contains(memberInfo.Name))
                        continue;
                    if (onlyFieldNames != null && !onlyFieldNames.Contains(memberInfo.Name))
                        continue;

                    if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(",")));
                    var fieldNameExpr = Expression.Constant(ormProvider.GetFieldName(memberMapper.FieldName));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
                    index++;
                }
                commandInitializer = Expression.Lambda<Action<StringBuilder, object>>(Expression.Block(blockBodies), builderExpr, insertObjExpr).Compile();
            }
            return commandInitializer;
        });
    }
    public static object BuildCreateValuesSqlParametes(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type insertObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool hasSuffix)
    {
        var cacheKey = HashCode.Combine(ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames);
        var cache = hasSuffix ? createBulkValuesSqlParametersCache : createValuesSqlParametersCache;
        return cache.GetOrAdd(cacheKey, f =>
        {
            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
            var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var insertObjExpr = Expression.Parameter(typeof(object), "insertObj");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            ParameterExpression suffixExpr = null;
            ParameterExpression dbFieldValueExpr = null;
            if (hasSuffix) suffixExpr = Expression.Parameter(typeof(string), "suffix");
            var entityMapper = mapProvider.GetEntityMap(entityType);

            MethodInfo methodInfo = null;
            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var concatMethodInfo1 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
            var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });

            if (typeof(IDictionary<string, object>).IsAssignableFrom(insertObjType))
            {
                var dictExpr = Expression.Variable(typeof(IDictionary<string, object>), "dict");
                var fieldValueExpr = Expression.Variable(typeof(object), "fieldValue");
                var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                var indexExpr = Expression.Variable(typeof(int), "index");
                var enumeratorExpr = Expression.Variable(typeof(IEnumerator<KeyValuePair<string, object>>), "enumerator");
                var itemKeyExpr = Expression.Variable(typeof(string), "itemKey");
                var memberMapperExpr = Expression.Variable(typeof(MemberMap), "memberMapper");
                var outTypeExpr = Expression.Variable(typeof(Type), "outType");
                blockParameters.AddRange(new[] { dictExpr, fieldValueExpr, parameterNameExpr, indexExpr, enumeratorExpr, itemKeyExpr, memberMapperExpr, outTypeExpr });
                blockBodies.Add(Expression.Assign(dictExpr, Expression.Convert(insertObjExpr, typeof(IDictionary<string, object>))));
                var breakLabel = Expression.Label();
                var continueLabel = Expression.Label();

                //var index = 0;
                //var enumerator = dict.GetEnumerator();
                //var entityMapper = new EntityMap{ ... };
                var entityMapperExpr = Expression.Constant(entityMapper);
                blockBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
                methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
                blockBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(dictExpr, methodInfo)));

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
                loopBodies.Add(Expression.Assign(fieldValueExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Value))));

                //var isContinue = !entityMapper.TryGetMemberMap(itemKey, out var memberMapper)
                //|| memberMapper.IsIgnore || memberMapper.IsIgnoreInsert || memberMapper.IsNavigation
                methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.TryGetMemberMap));
                Expression isContinueExpr = Expression.IsFalse(Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr, memberMapperExpr));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnore)));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnoreInsert)));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsNavigation)));

                //|| memberMapper.IsAutoIncrement || memberMapper.IsRowVersion
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsAutoIncrement)));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsRowVersion)));

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
                loopBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));

                //if(index > 0) builder.Append(",");
                var greaterThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0));
                var callExpr = Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(","));
                loopBodies.Add(Expression.IfThen(greaterThenExpr, callExpr));

                //builder.Append(parameterName);
                loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));


                //object dbFieldValue=null;
                //if(memberMapper.TypeHandler!=null)
                //  dbFieldValue = memberMapper.TypeHandler.ToFieldValue(ormProvider, dbFieldValue);
                //else
                //{
                //    var targetType = this.OrmProvider.MapDefaultType(memberMapper.NativeDbType);
                //    var valueGetter = this.OrmProvider.GetParameterValueGetter(sqlSegment.SegmentType, targetType, false);
                //    dbFieldValue = valueGetter.Invoke(dbFieldValue);
                //}
                Expression nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
                var typeHandlerExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler));
                methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
                Expression myFieldValueExpr = fieldValueExpr;
                if (fieldValueExpr.Type != typeof(object))
                    myFieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                var typeHandlerValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, myFieldValueExpr);

                nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));

                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.MapDefaultType));
                var targetTypeExpr = Expression.Call(ormProviderExpr, methodInfo, nativeDbTypeExpr);
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetParameterValueGetter));
                var valueGetterExpr = Expression.Call(ormProviderExpr, methodInfo, Expression.Constant(fieldValueExpr.Type), targetTypeExpr, Expression.Constant(false));
                var valueGetterValueExpr = Expression.Invoke(valueGetterExpr, myFieldValueExpr);

                if (dbFieldValueExpr == null)
                {
                    dbFieldValueExpr = Expression.Variable(typeof(object), "objValue");
                    blockParameters.Add(dbFieldValueExpr);
                }

                var isNotNullExpr = Expression.IsFalse(Expression.Equal(typeHandlerExpr, Expression.Constant(null)));
                var setTypeHandlerValueExpr = Expression.Assign(dbFieldValueExpr, typeHandlerValueExpr);
                var setValueGetterValueExpr = Expression.Assign(dbFieldValueExpr, valueGetterValueExpr);
                blockBodies.Add(Expression.IfThenElse(isNotNullExpr, setTypeHandlerValueExpr, setValueGetterValueExpr));


                //dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue);
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                var dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, dbFieldValueExpr);
                methodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add), new Type[] { typeof(object) });
                loopBodies.Add(Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr));

                //index++;
                loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));

                blockBodies.Add(Expression.Loop(Expression.Block(loopBodies), breakLabel, continueLabel));
            }
            else
            {
                ParameterExpression parameterNameExpr = null;
                var typedInsertObjExpr = Expression.Variable(insertObjType, "typedInsertObj");
                blockParameters.Add(typedInsertObjExpr);
                if (hasSuffix)
                {
                    parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                    blockParameters.Add(parameterNameExpr);
                }
                blockBodies.Add(Expression.Assign(typedInsertObjExpr, Expression.Convert(insertObjExpr, insertObjType)));
                var memberInfos = insertObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                     .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

                var index = 0;
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsIgnoreInsert
                        || memberMapper.IsNavigation || memberMapper.IsAutoIncrement || memberMapper.IsRowVersion
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;

                    if (ignoreFieldNames != null && ignoreFieldNames.Contains(memberInfo.Name))
                        continue;
                    if (onlyFieldNames != null && !onlyFieldNames.Contains(memberInfo.Name))
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
                    var fieldValueType = memberInfo.GetMemberType();
                    AddValueParameter(ormProvider, dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueType, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                    index++;
                }
            }

            object result = null;
            if (hasSuffix) result = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, ormProviderExpr, insertObjExpr, suffixExpr).Compile();
            else result = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, IOrmProvider, object>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, ormProviderExpr, insertObjExpr).Compile();
            return result;
        });
    }
    public static (string, Action<StringBuilder, string>, object, object) BuildUpdateSqlParameters(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type updateObjType, bool hasSuffix, List<string> onlyFieldNames, List<string> ignoreFieldNames)
    {
        var cacheKey = HashCode.Combine(ormProvider, mapProvider, entityType, updateObjType, hasSuffix, onlyFieldNames, ignoreFieldNames);
        var cache = hasSuffix ? updateMultiCommandInitializerCache : updateCommandInitializerCache;
        return cache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var setSqlParametersSetter = BuildSqlParametersPart(ormProvider, mapProvider, entityType, updateObjType, true, false, false, false, false, false, hasSuffix, true, onlyFieldNames, ignoreFieldNames, ",", null);
            var whereSqlParametersSetter = BuildSqlParametersPart(ormProvider, mapProvider, entityType, updateObjType, false, false, false, true, true, false, hasSuffix, false, null, null, " AND ", null);
            var setSqlSetter = BuildSqlParametersPart(ormProvider, mapProvider, entityType, updateObjType, true, false, true, false, false, false, hasSuffix, true, onlyFieldNames, ignoreFieldNames, ",", null);
            var whereSqlSetter = BuildSqlParametersPart(ormProvider, mapProvider, entityType, updateObjType, false, false, true, true, true, false, hasSuffix, false, null, null, " AND ", null);
            object firstSqlParametersSetter = null, sqlSetter = null;

            string tableName = entityMapper.TableName;
            Action<StringBuilder, string> headSqlSetter = (builder, tableName)
                => builder.Append($"UPDATE {ormProvider.GetTableName(tableName)} SET ");

            if (hasSuffix)
            {
                Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string> typedFirstSqlParametersSetter = null;
                Action<StringBuilder, IOrmProvider, object, string> typedSqlSetter = null;
                var typedSetSqlParametersSetter = setSqlParametersSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>;
                var typedWhereSqlParametersSetter = whereSqlParametersSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>;
                var typedSetSqlSetter = setSqlSetter as Action<StringBuilder, IOrmProvider, object, string>;
                var typedWhereSqlSetter = whereSqlSetter as Action<StringBuilder, IOrmProvider, object, string>;

                typedFirstSqlParametersSetter = (dbParameters, builder, ormProvider, parameters, suffix) =>
                {
                    typedSetSqlParametersSetter.Invoke(dbParameters, builder, ormProvider, parameters, suffix);
                    builder.Append(" WHERE ");
                    typedWhereSqlParametersSetter.Invoke(dbParameters, builder, ormProvider, parameters, suffix);
                };
                typedSqlSetter = (builder, ormProvider, parameters, suffix) =>
                {
                    typedSetSqlSetter.Invoke(builder, ormProvider, parameters, suffix);
                    builder.Append(" WHERE ");
                    typedWhereSqlSetter.Invoke(builder, ormProvider, parameters, suffix);
                };
                firstSqlParametersSetter = typedFirstSqlParametersSetter;
                sqlSetter = typedSqlSetter;
            }
            else
            {
                Action<IDataParameterCollection, StringBuilder, IOrmProvider, object> typedFirstSqlParametersSetter = null;
                Action<StringBuilder, IOrmProvider, object> typedSqlSetter = null;
                var typedSetSqlParametersSetter = setSqlParametersSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object>;
                var typedWhereSqlParametersSetter = whereSqlParametersSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object>;
                var typedSetSqlSetter = setSqlParametersSetter as Action<StringBuilder, IOrmProvider, object>;
                var typedWhereSqlSetter = whereSqlParametersSetter as Action<StringBuilder, IOrmProvider, object>;

                typedFirstSqlParametersSetter = (dbParameters, builder, ormProvider, parameters) =>
                {
                    typedSetSqlParametersSetter.Invoke(dbParameters, builder, ormProvider, parameters);
                    builder.Append(" WHERE ");
                    typedWhereSqlParametersSetter.Invoke(dbParameters, builder, ormProvider, parameters);
                };
                typedSqlSetter = (builder, ormProvider, parameters) =>
                {
                    typedSetSqlSetter.Invoke(builder, ormProvider, parameters);
                    builder.Append(" WHERE ");
                    typedWhereSqlSetter.Invoke(builder, ormProvider, parameters);
                };
                firstSqlParametersSetter = typedFirstSqlParametersSetter;
                sqlSetter = typedSqlSetter;
            }
            return (tableName, headSqlSetter, firstSqlParametersSetter, sqlSetter);
        });
    }

    public static object BuildUpdateSetWithPartSqlParameters(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type updateObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool hasSuffix)
    {
        var cacheKey = HashCode.Combine(ormProvider, mapProvider, entityType, updateObjType, onlyFieldNames, ignoreFieldNames);
        var commandInitializerCache = hasSuffix ? updateMultiWithCommandInitializerCache : updateWithCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var updateFieldsExpr = Expression.Parameter(typeof(List<string>), "updateFields");
            var updateObjExpr = Expression.Parameter(typeof(object), "updateObj");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            ParameterExpression suffixExpr = null;
            if (hasSuffix) suffixExpr = Expression.Parameter(typeof(string), "suffix");
            MethodInfo methodInfo = null;
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var concatMethodInfo1 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
            var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });
            var addMethodInfo = typeof(List<string>).GetMethod(nameof(List<string>.Add));

            if (typeof(IDictionary<string, object>).IsAssignableFrom(updateObjType))
            {
                var dictExpr = Expression.Variable(typeof(IDictionary<string, object>), "dict");
                var fieldValueExpr = Expression.Variable(typeof(object), "fieldValue");
                var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                var indexExpr = Expression.Variable(typeof(int), "index");
                var enumeratorExpr = Expression.Variable(typeof(IEnumerator<KeyValuePair<string, object>>), "enumerator");
                var itemKeyExpr = Expression.Variable(typeof(string), "itemKey");
                var memberMapperExpr = Expression.Variable(typeof(MemberMap), "memberMapper");
                var outTypeExpr = Expression.Variable(typeof(Type), "outType");
                blockParameters.AddRange(new[] { dictExpr, fieldValueExpr, parameterNameExpr, indexExpr, enumeratorExpr, itemKeyExpr, memberMapperExpr, outTypeExpr });
                blockBodies.Add(Expression.Assign(dictExpr, Expression.Convert(updateObjExpr, typeof(IDictionary<string, object>))));
                var breakLabel = Expression.Label();
                var continueLabel = Expression.Label();
                ParameterExpression dbFieldValueExpr = null;

                //var index = 0;
                //var enumerator = dict.GetEnumerator();
                //var entityMapper = new EntityMap{ ... };
                var entityMapperExpr = Expression.Constant(entityMapper);
                blockBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
                methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
                blockBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(dictExpr, methodInfo)));

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
                loopBodies.Add(Expression.Assign(fieldValueExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Value))));

                //var isContinue = !entityMapper.TryGetMemberMap(itemKey, out var memberMapper)
                //|| memberMapper.IsIgnore || memberMapper.IsIgnoreUpdate
                methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.TryGetMemberMap));
                Expression isContinueExpr = Expression.IsFalse(Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr, memberMapperExpr));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnore)));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnoreUpdate)));

                //|| memberMapper.IsNavigation || memberMapper.IsRowVersion
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsNavigation)));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsRowVersion)));

                //|| (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.IsEntityType));
                var memberTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.MemberType));
                var isEntityTypeExpr = Expression.Call(methodInfo, memberTypeExpr, outTypeExpr);
                var isNullExpr = Expression.Equal(Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler)), Expression.Constant(null));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.AndAlso(isEntityTypeExpr, isNullExpr));

                //if (isContinue) continue;
                loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

                Expression fieldNameExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.FieldName));
                var getFieldNameMethodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetFieldName));
                fieldNameExpr = Expression.Call(ormProviderExpr, getFieldNameMethodInfo, fieldNameExpr);

                //var parameterName = ormProvider.ParameterPrefix + itemKey + multiMark;
                Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix);
                if (hasSuffix)
                    myParameterNameExpr = Expression.Call(concatMethodInfo2, myParameterNameExpr, itemKeyExpr, suffixExpr);
                else myParameterNameExpr = Expression.Call(concatMethodInfo1, myParameterNameExpr, itemKeyExpr);
                blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));

                //updateFields.Add(ormProvider.GetFieldName(memberMapper.FieldName) + "=" + parameterName });
                loopBodies.Add(Expression.Call(concatMethodInfo2, fieldNameExpr, Expression.Constant("="), parameterNameExpr));

                //if(!dbParameters.Contains(parameterName))
                //{
                //  object dbFieldValue=null;
                //  if(memberMapper.TypeHandler!=null)
                //      dbFieldValue = memberMapper.TypeHandler.ToFieldValue(ormProvider, dbFieldValue);
                //  else
                //  {
                //      var targetType = this.OrmProvider.MapDefaultType(memberMapper.NativeDbType);
                //      var valueGetter = this.OrmProvider.GetParameterValueGetter(sqlSegment.SegmentType, targetType, false);
                //      dbFieldValue = valueGetter.Invoke(dbFieldValue);
                //  }
                //  dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue);
                //}


                //object dbFieldValue=null;
                //if(memberMapper.TypeHandler!=null)
                //  dbFieldValue = memberMapper.TypeHandler.ToFieldValue(ormProvider, dbFieldValue);
                //else
                //{
                //    var targetType = this.OrmProvider.MapDefaultType(memberMapper.NativeDbType);
                //    var valueGetter = this.OrmProvider.GetParameterValueGetter(sqlSegment.SegmentType, targetType, false);
                //    dbFieldValue = valueGetter.Invoke(dbFieldValue);
                //}
                Expression nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
                var typeHandlerExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler));
                methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
                Expression myFieldValueExpr = fieldValueExpr;
                if (fieldValueExpr.Type != typeof(object))
                    myFieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                var typeHandlerValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, myFieldValueExpr);

                nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));

                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.MapDefaultType));
                var targetTypeExpr = Expression.Call(ormProviderExpr, methodInfo, nativeDbTypeExpr);
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetParameterValueGetter));
                var valueGetterExpr = Expression.Call(ormProviderExpr, methodInfo, Expression.Constant(fieldValueExpr.Type), targetTypeExpr, Expression.Constant(false));
                var valueGetterValueExpr = Expression.Invoke(valueGetterExpr, myFieldValueExpr);

                if (dbFieldValueExpr == null)
                {
                    dbFieldValueExpr = Expression.Variable(typeof(object), "objValue");
                    blockParameters.Add(dbFieldValueExpr);
                }

                var isNotNullExpr = Expression.IsFalse(Expression.Equal(typeHandlerExpr, Expression.Constant(null)));
                var setTypeHandlerValueExpr = Expression.Assign(dbFieldValueExpr, typeHandlerValueExpr);
                var setValueGetterValueExpr = Expression.Assign(dbFieldValueExpr, valueGetterValueExpr);
                blockBodies.Add(Expression.IfThenElse(isNotNullExpr, setTypeHandlerValueExpr, setValueGetterValueExpr));

                //dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue);
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                var dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, dbFieldValueExpr);
                methodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add), new Type[] { typeof(object) });
                var addParameterExpr = Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr);

                methodInfo = typeof(IDataParameterCollection).GetMethod(nameof(IDataParameterCollection.Contains));
                var notContainsExpr = Expression.IsFalse(Expression.Call(dbParametersExpr, methodInfo, parameterNameExpr));
                loopBodies.Add(Expression.IfThen(notContainsExpr, addParameterExpr));

                //index++;
                loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));

                blockBodies.Add(Expression.Loop(Expression.Block(loopBodies), breakLabel, continueLabel));
            }
            else
            {
                ParameterExpression parameterNameExpr = null;
                var typedUpdateObjExpr = Expression.Variable(updateObjType, "typeUpdateObj");
                blockParameters.Add(typedUpdateObjExpr);
                blockBodies.Add(Expression.Assign(typedUpdateObjExpr, Expression.Convert(updateObjExpr, updateObjType)));
                if (hasSuffix)
                {
                    parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                    blockParameters.Add(parameterNameExpr);
                }
                var memberInfos = updateObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

                var index = 0;
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsIgnoreUpdate
                        || memberMapper.IsNavigation || memberMapper.IsRowVersion
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;
                    if (memberMapper.IsKey) continue;
                    if (ignoreFieldNames != null && ignoreFieldNames.Contains(memberInfo.Name))
                        continue;
                    if (onlyFieldNames != null && !onlyFieldNames.Contains(memberInfo.Name))
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + memberMapper.MemberName;
                    Expression myParameterNameExpr = Expression.Constant(parameterName);
                    Expression setFieldExpr = null;
                    if (hasSuffix)
                    {
                        myParameterNameExpr = Expression.Call(concatMethodInfo1, myParameterNameExpr, suffixExpr);
                        blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                        setFieldExpr = Expression.Constant(ormProvider.GetFieldName(memberMapper.FieldName) + "=");
                        setFieldExpr = Expression.Call(concatMethodInfo1, setFieldExpr, parameterNameExpr);
                        myParameterNameExpr = parameterNameExpr;
                    }
                    else setFieldExpr = Expression.Constant(ormProvider.GetFieldName(memberMapper.FieldName) + "=" + parameterName);
                    blockBodies.Add(Expression.Call(updateFieldsExpr, addMethodInfo, setFieldExpr));

                    var fieldValueExpr = Expression.PropertyOrField(typedUpdateObjExpr, memberMapper.MemberName);
                    var fieldVallueType = memberInfo.GetMemberType();
                    AddValueParameter(ormProvider, dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldVallueType, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                    index++;
                }
            }

            object result = null;
            if (hasSuffix) result = Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, List<string>, object, string>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, updateFieldsExpr, updateObjExpr, suffixExpr).Compile();
            else result = Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, List<string>, object>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, updateFieldsExpr, updateObjExpr).Compile();
            return result;
        });
    }
    public static (bool, string, object, Action<StringBuilder, string>) BuildDeleteCommandInitializer(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type whereObjType, bool isBulk, bool hasSuffix)
    {
        var cacheKey = HashCode.Combine(ormProvider, mapProvider, entityType, whereObjType, isBulk);
        var commandInitializerCache = hasSuffix ? deleteMultiCommandInitializerCache : deleteCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var isMultiKeys = entityMapper.KeyMembers.Count > 1;
            Action<StringBuilder, string> sqlSetter = null;
            object whereSqlParametersSetter = null;
            if (isBulk)
            {
                if (isMultiKeys) sqlSetter = (builder, tableName) => builder.Append($"DELETE FROM {ormProvider.GetTableName(tableName)} WHERE ");
                else sqlSetter = (builder, tableName) => builder.Append($"DELETE FROM {ormProvider.GetTableName(tableName)} WHERE {ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)} IN (");
            }
            else sqlSetter = (builder, tableName) => builder.Append($"DELETE FROM {ormProvider.GetTableName(tableName)} WHERE ");
            var isOnlyParameters = isBulk && !isMultiKeys;
            whereSqlParametersSetter = BuildSqlParametersPart(ormProvider, mapProvider, entityType, whereObjType, false, false, false, true, false, isOnlyParameters, hasSuffix, false, null, null, " AND ", null);
            return (isMultiKeys, entityMapper.TableName, whereSqlParametersSetter, sqlSetter);
        });
    }

    public static Dictionary<string, List<object>> SplitShardingParameters(string dbKey, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, Type entityType, IEnumerable parameters)
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
            var tableName = RepositoryHelper.GetShardingTableName(dbKey, mapProvider, shardingProvider, entityType, parameterType, parameter);
            if (!result.TryGetValue(tableName, out var myParameters))
                result.Add(tableName, myParameters = new List<object>());
            myParameters.Add(parameter);
        }
        return result;
    }
    public static string GetShardingTableName(string dbKey, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, Type entityType, Type parameterType, object parameter)
    {
        var entityMapper = mapProvider.GetEntityMap(entityType);
        var tableName = entityMapper.TableName;
        if (TryBuildShardingTableNameGetter(shardingProvider, entityType, parameterType, out var tableNameGetter))
            return tableNameGetter.Invoke(dbKey, tableName, parameter);
        return tableName;
    }
    public static bool TryBuildShardingTableNameGetter(IShardingProvider shardingProvider, Type entityType, Type parameterType, out Func<string, string, object, string> tableNameGetter)
    {
        if (!shardingProvider.TryGetShardingTable(entityType, out var shardingTable))
        {
            tableNameGetter = null;
            return false;
        }
        if (shardingTable.DependOnMembers == null || shardingTable.DependOnMembers.Count == 0)
            throw new NotSupportedException($"实体表{entityType.FullName}有设置分表，但未指定依赖字段，插入数据无法确定分表");

        var cacheKey = HashCode.Combine(entityType, parameterType);
        if (shardingTable.DependOnMembers.Count > 1)
        {
            if (typeof(IDictionary<string, object>).IsAssignableFrom(parameterType))
            {
                tableNameGetter = shardingTableNameGetters.GetOrAdd(cacheKey, f =>
                {
                    return (string dbKey, string origName, object parameter) =>
                    {
                        var dict = parameter as IDictionary<string, object>;
                        if (!dict.TryGetValue(shardingTable.DependOnMembers[0], out var field1Value))
                            throw new MissingMemberException($"实体表{entityType.FullName}已设置分表并依赖成员{shardingTable.DependOnMembers[0]}映射的字段，但当前字典中并不包含key:{shardingTable.DependOnMembers[0]}的键值");
                        if (!dict.TryGetValue(shardingTable.DependOnMembers[1], out var field2Value))
                            throw new MissingMemberException($"实体表{entityType.FullName}已设置分表并依赖成员{shardingTable.DependOnMembers[1]}映射的字段，但当前字典中不包含key:{shardingTable.DependOnMembers[1]}的键值");

                        var tableNameRuleGetter = shardingTable.Rule as Func<string, string, object, object, string>;
                        return tableNameRuleGetter.Invoke(dbKey, origName, field1Value, field2Value);
                    };
                });
            }
            else
            {
                tableNameGetter = shardingTableNameGetters.GetOrAdd(cacheKey, f =>
                {
                    var dbKeyExpr = Expression.Parameter(typeof(string), "dbKey");
                    var origNameExpr = Expression.Parameter(typeof(string), "origName");
                    var parameterObjExpr = Expression.Parameter(typeof(object), "parameterObj");
                    var tableNameRuleGetter = shardingTable.Rule as Func<string, string, object, object, string>;

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
                    var getterExpr = Expression.Constant(tableNameRuleGetter, typeof(Func<string, string, object, object, string>));
                    var bodyExpr = Expression.Invoke(getterExpr, dbKeyExpr, origNameExpr, field1Expr, field2Expr);
                    return Expression.Lambda<Func<string, string, object, string>>(bodyExpr, dbKeyExpr, origNameExpr, parameterObjExpr).Compile();
                });
            }
        }
        else
        {
            if (typeof(IDictionary<string, object>).IsAssignableFrom(parameterType))
            {
                tableNameGetter = shardingTableNameGetters.GetOrAdd(cacheKey, f =>
                {
                    return (string dbKey, string origName, object parameter) =>
                    {
                        var dict = parameter as IDictionary<string, object>;
                        if (!dict.TryGetValue(shardingTable.DependOnMembers[0], out var fieldValue))
                            throw new MissingMemberException($"实体表{entityType.FullName}已设置分表并依赖成员{shardingTable.DependOnMembers[0]}映射的字段，但当前字典中并不包含key:{shardingTable.DependOnMembers[0]}的键值");

                        var tableNameRuleGetter = shardingTable.Rule as Func<string, string, object, string>;
                        return tableNameRuleGetter.Invoke(dbKey, origName, fieldValue);
                    };
                });
            }
            else
            {
                tableNameGetter = shardingTableNameGetters.GetOrAdd(cacheKey, f =>
                {
                    var dbKeyExpr = Expression.Parameter(typeof(string), "dbKey");
                    var origNameExpr = Expression.Parameter(typeof(string), "origName");
                    var parameterObjExpr = Expression.Parameter(typeof(object), "parameterObj");
                    var tableNameRuleGetter = shardingTable.Rule as Func<string, string, object, string>;

                    var members = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                        .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                    if (!members.Exists(f => f.Name == shardingTable.DependOnMembers[0]))
                        throw new MissingMemberException($"实体表{entityType.FullName}已设置分表并依赖成员{shardingTable.DependOnMembers[0]}映射的字段，但当前参数中并不包含{shardingTable.DependOnMembers[0]}成员");

                    var typedParameterObjExpr = Expression.Convert(parameterObjExpr, parameterType);
                    Expression fieldExpr = Expression.PropertyOrField(typedParameterObjExpr, shardingTable.DependOnMembers[0]);
                    if (fieldExpr.Type != typeof(object))
                        fieldExpr = Expression.Convert(fieldExpr, typeof(object));
                    var getterExpr = Expression.Constant(tableNameRuleGetter, typeof(Func<string, string, object, string>));
                    var bodyExpr = Expression.Invoke(getterExpr, dbKeyExpr, origNameExpr, fieldExpr);
                    return Expression.Lambda<Func<string, string, object, string>>(bodyExpr, dbKeyExpr, origNameExpr, parameterObjExpr).Compile();
                });
            }
        }
        return true;
    }
}
