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

    private static ConcurrentDictionary<int, Func<IDataParameterCollection, IOrmProvider, object, string>> createSqlParametersCache = new();
    private static ConcurrentDictionary<int, (Action<StringBuilder, object>, Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>)> createMultiSqlParametersCache = new();

    private static ConcurrentDictionary<int, object> deleteCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> deleteMultiCommandInitializerCache = new();
    private static ConcurrentDictionary<int, (bool, string, Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>)> deleteBulkCommandInitializerCache = new();

    private static ConcurrentDictionary<int, Func<IDataParameterCollection, IOrmProvider, object, string>> updateCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>> updateMultiCommandInitializerCache = new();

    private static ConcurrentDictionary<int, object> updateWithCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> updateMultiWithCommandInitializerCache = new();

    public static void AddValueParameter(Expression dbParametersExpr, Expression ormProviderExpr, Expression parameterNameExpr, Type fieldValueType,
        Expression parameterValueExpr, MemberMap memberMapper, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        MethodInfo methodInfo = null;
        Expression typedParameterExpr = null;
        var fieldValueExpr = parameterValueExpr;
        var addMethodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add));
        var createParameterMethodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
        Expression nativeDbTypeExpr = Expression.Constant(memberMapper.NativeDbType);
        if (nativeDbTypeExpr.Type != typeof(object))
            nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));
        if (memberMapper.TypeHandler == null)
            throw new Exception($"{memberMapper.Parent.EntityType.FullName}类成员{memberMapper.MemberName}TypeHandler不能为null");

        var typeHandlerExpr = Expression.Constant(memberMapper.TypeHandler);
        methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
        if (fieldValueType != typeof(object))
            fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
        fieldValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, Expression.Constant(memberMapper.UnderlyingType), fieldValueExpr);
        typedParameterExpr = Expression.Call(ormProviderExpr, createParameterMethodInfo, parameterNameExpr, nativeDbTypeExpr, fieldValueExpr);
        blockBodies.Add(Expression.Call(dbParametersExpr, addMethodInfo, typedParameterExpr));
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

    public static string BuildFieldsSqlPart(IOrmProvider ormProvider, EntityMap entityMapper, Type selectType, bool isSelect)
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
            if (isSelect && memberMapper.FieldName != memberMapper.MemberName)
                builder.Append(" AS " + ormProvider.GetFieldName(memberMapper.MemberName));
            index++;
        }
        return builder.ToString();
    }
    public static object BuildWhereSqlParameters(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type whereObjType, bool isWhereKey, bool hasSuffix, bool isWithKey, string whereObjName, string headSql)
    {
        object commandInitializer = null;
        var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
        var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
        var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

        ParameterExpression suffixExpr = null;
        var builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();

        if (hasSuffix) suffixExpr = Expression.Parameter(typeof(string), "suffix");
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

                    if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                    var fieldNameExpr = Expression.Constant($"{ormProvider.GetFieldName(keyMapper.FieldName)}=");
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, myParameterNameExpr));
                    var typedFieldValueExpr = Expression.Convert(fieldValueExpr, keyMapper.MemberType);
                    AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, keyMapper.MemberType, typedFieldValueExpr, keyMapper, blockParameters, blockBodies);
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

                //var dbFieldValue = memberMapper.TypeHandler.ToFieldValue(ormProvider, memberMapper.UnderlyingType, fieldValue);
                var underlyingTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.UnderlyingType));
                Expression nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
                var typeHandlerExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler));
                methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
                Expression myFieldValueExpr = fieldValueExpr;
                if (fieldValueExpr.Type != typeof(object))
                    myFieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                var dbFieldValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, underlyingTypeExpr, myFieldValueExpr);

                //dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue);
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));
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

                if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                var fieldNameExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}=");
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, myParameterNameExpr));

                if (isEntityType)
                {
                    var fieldValueExpr = Expression.PropertyOrField(typedWhereObjExpr, memberMapper.MemberName);
                    var fieldValueType = targetMemberInfos.Find(f => f.Name == memberMapper.MemberName).GetMemberType();
                    AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueType, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                }
                else
                {
                    var fieldValueExpr = Expression.Convert(whereObjExpr, memberMapper.MemberType);
                    AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, memberMapper.MemberType, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                }
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

    public static object BuildGetSqlParameters(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiple)
    {
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(ormProvider, mapProvider, entityType, whereObjType);
        var commandInitializerCache = isMultiple ? queryMultiGetCommandInitializerCache : queryGetCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var fieldsSql = BuildFieldsSqlPart(ormProvider, entityMapper, entityType, true);
            var headSql = $"SELECT {fieldsSql} FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ";
            return BuildWhereSqlParameters(ormProvider, mapProvider, entityType, whereObjType, true, isMultiple, false, nameof(whereObj), headSql);
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
            var fieldsSql = BuildFieldsSqlPart(ormProvider, entityMapper, entityType, true);
            var headSql = $"SELECT {fieldsSql} FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ";
            return BuildWhereSqlParameters(ormProvider, mapProvider, entityType, whereObjType, false, isMultiple, false, nameof(whereObj), headSql);
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
            return BuildWhereSqlParameters(ormProvider, mapProvider, entityType, whereObjType, false, isMultiple, false, nameof(whereObj), headSql);
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

    public static Func<IDataParameterCollection, IOrmProvider, object, string> BuildCreateSqlParameters(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object insertObj, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool isReturnIdentity)
    {
        var insertObjType = insertObj.GetType();
        var cacheKey = HashCode.Combine(ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames, isReturnIdentity);
        return createSqlParametersCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var headSqlSetter = BuildCreateHeadSqlPart(ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames);
            var valuesSqlParameters = BuildCreateValuesPartSqlParametes(ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames, false);
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
    public static (Action<StringBuilder>, Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>) BuildCreateMultiSqlParameters(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object insertObjs, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool isBulk)
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
        var insertObjType = insertObj.GetType();
        var cacheKey = BuildInsertHashKey(ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames);
        (var headSqlParameterSetter, var commandInitializer) = createMultiSqlParametersCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var headSqlPartSetter = BuildCreateHeadSqlPart(ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames);
            Action<StringBuilder, object> headSqlParameterSetter = (builder, parameter) =>
            {
                builder.Append($"INSERT INTO {ormProvider.GetFieldName(entityMapper.TableName)} (");
                headSqlPartSetter.Invoke(builder, parameter);
                builder.Append(") VALUES ");
            };

            var valuesSqlParameters = BuildCreateValuesPartSqlParametes(ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames, true);
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
    public static Action<StringBuilder, object> BuildCreateHeadSqlPart(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type insertObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames)
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

                if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(",")));
                var fieldNameExpr = Expression.Constant(ormProvider.GetFieldName(memberMapper.FieldName));
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
                index++;
            }
            commandInitializer = Expression.Lambda<Action<StringBuilder, object>>(Expression.Block(blockBodies), builderExpr, insertObjExpr).Compile();
        }
        return commandInitializer;
    }
    public static object BuildCreateValuesPartSqlParametes(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type insertObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool hasSuffix)
    {
        var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
        var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
        var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
        var insertObjExpr = Expression.Parameter(typeof(object), "insertObj");
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();

        ParameterExpression suffixExpr = null;
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
            loopBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));

            //if(index > 0) builder.Append(",");
            var greaterThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0));
            var callExpr = Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(","));
            loopBodies.Add(Expression.IfThen(greaterThenExpr, callExpr));

            //builder.Append(parameterName);
            loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));

            //var dbFieldValue = memberMapper.TypeHandler.ToFieldValue(ormProvider, memberMapper.UnderlyingType, fieldValue);
            var underlyingTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.UnderlyingType));
            Expression nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
            var typeHandlerExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler));
            methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
            var dbFieldValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, underlyingTypeExpr, fieldValueExpr);

            //dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue);
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
            nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));
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
                    || memberMapper.IsIgnore || memberMapper.IsNavigation
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
                AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueType, fieldValueExpr, memberMapper, blockParameters, blockBodies);
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
    public static Func<IDataParameterCollection, IOrmProvider, object, string> BuildUpdateSqlParameters(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object updateObj, List<string> onlyFieldNames, List<string> ignoreFieldNames)
    {
        var updateObjType = updateObj.GetType();
        var cacheKey = HashCode.Combine(ormProvider, mapProvider, entityType, updateObjType, onlyFieldNames, ignoreFieldNames);
        return updateCommandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var setCommandInitializer = BuildUpdateSetPartSqlParameters(ormProvider, mapProvider, entityType, updateObjType, onlyFieldNames, ignoreFieldNames, false);
            var whereCommandInitializer = BuildWhereSqlParameters(ormProvider, mapProvider, entityType, updateObjType, true, false, true, nameof(updateObj), null);
            Func<IDataParameterCollection, IOrmProvider, object, string> commandInitializer;
            var typeSetCommandInitializer = setCommandInitializer as Action<IDataParameterCollection, IOrmProvider, StringBuilder, object>;
            var typeWhereCommandInitializer = whereCommandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;

            commandInitializer = (dbParameters, ormProvider, updateObj) =>
            {
                var builder = new StringBuilder($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
                typeSetCommandInitializer.Invoke(dbParameters, ormProvider, builder, updateObj);
                builder.Append(" WHERE ");
                builder.Append(typeWhereCommandInitializer.Invoke(dbParameters, ormProvider, updateObj));
                return builder.ToString();
            };
            return commandInitializer;
        });
    }
    public static Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string> BuildUpdateMultiSqlParameters(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object updateObjs, List<string> onlyFieldNames, List<string> ignoreFieldNames)
    {
        var entities = updateObjs as IEnumerable;
        object updateObj = null;
        foreach (var entity in entities)
        {
            updateObj = entity;
            break;
        }
        var updateObjType = updateObj.GetType();
        var cacheKey = HashCode.Combine(ormProvider, mapProvider, entityType, updateObjType, onlyFieldNames, ignoreFieldNames);
        return updateMultiCommandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var setCommandInitializer = BuildUpdateSetPartSqlParameters(ormProvider, mapProvider, entityType, updateObjType, onlyFieldNames, ignoreFieldNames, true);
            var whereCommandInitializer = BuildWhereSqlParameters(ormProvider, mapProvider, entityType, updateObjType, true, true, true, nameof(updateObj), null);
            Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string> commandInitializer;
            var typeSetCommandInitializer = setCommandInitializer as Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>;
            var typeWhereCommandInitializer = whereCommandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string, string>;
            commandInitializer = (dbParameters, ormProvider, builder, updateObj, suffix) =>
            {
                builder.Append($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
                typeSetCommandInitializer.Invoke(dbParameters, ormProvider, builder, updateObj, suffix);
                builder.Append(" WHERE ");
                builder.Append(typeWhereCommandInitializer.Invoke(dbParameters, ormProvider, updateObj, suffix));
            };
            return commandInitializer;
        });
    }
    public static object BuildUpdateSetPartSqlParameters(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type updateObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool hasSuffix, bool isInsertOrUpdate = false)
    {
        var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
        var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
        var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
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

            //var parameterName = ormProvider.ParameterPrefix + itemKey + suffix;
            Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix);
            if (hasSuffix)
                myParameterNameExpr = Expression.Call(concatMethodInfo2, myParameterNameExpr, itemKeyExpr, suffixExpr);
            else myParameterNameExpr = Expression.Call(concatMethodInfo1, myParameterNameExpr, itemKeyExpr);
            loopBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));

            //if(index > 0) builder.Append(",");
            var greaterThanExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0));
            if (isInsertOrUpdate)
            {
                var lengthExpr = Expression.Property(builderExpr, nameof(StringBuilder.Length));
                greaterThanExpr = Expression.GreaterThan(lengthExpr, Expression.Constant(0));
            }
            var callExpr = Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(","));
            loopBodies.Add(Expression.IfThen(greaterThanExpr, callExpr));

            //builder.Append($"{ormProider.GetFieldName(itemKey)}={parameterName}");
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetFieldName));
            var fieldNameExpr = Expression.Call(ormProviderExpr, methodInfo, itemKeyExpr);
            loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
            loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant("=")));
            loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));

            if (isInsertOrUpdate)
            {
                //if(!dbParameters.Contains(parameterName))
                //{
                //  var dbFieldValue = memberMapper.TypeHandler.ToFieldValue(ormProvider, memberMapper.UnderlyingType, fieldValue);
                //  dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue);
                //}

                //var dbFieldValue = memberMapper.TypeHandler.ToFieldValue(ormProvider, memberMapper.UnderlyingType, fieldValue);
                var underlyingTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.UnderlyingType));
                Expression nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
                var typeHandlerExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler));
                methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
                Expression myFieldValueExpr = fieldValueExpr;
                if (fieldValueExpr.Type != typeof(object))
                    myFieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                var dbFieldValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, underlyingTypeExpr, myFieldValueExpr);

                //dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue);
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));
                var dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, dbFieldValueExpr);
                methodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add), new Type[] { typeof(object) });
                var addParameterExpr = Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr);

                methodInfo = typeof(IDataParameterCollection).GetMethod(nameof(IDataParameterCollection.Contains));
                var notContainsExpr = Expression.IsFalse(Expression.Call(dbParametersExpr, methodInfo, parameterNameExpr));
                loopBodies.Add(Expression.IfThen(notContainsExpr, addParameterExpr));
            }
            else
            {
                //var dbFieldValue = memberMapper.TypeHandler.ToFieldValue(ormProvider, memberMapper.UnderlyingType, fieldValue);
                var underlyingTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.UnderlyingType));
                Expression nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
                var typeHandlerExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler));
                methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
                Expression myFieldValueExpr = fieldValueExpr;
                if (fieldValueExpr.Type != typeof(object))
                    myFieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                var dbFieldValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, underlyingTypeExpr, myFieldValueExpr);

                //dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue);
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));
                var dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, dbFieldValueExpr);
                methodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add), new Type[] { typeof(object) });
                loopBodies.Add(Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr));
            }
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
                    || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsKey
                    || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                    continue;

                if (ignoreFieldNames != null && ignoreFieldNames.Contains(memberInfo.Name))
                    continue;
                if (onlyFieldNames != null && !onlyFieldNames.Contains(memberInfo.Name))
                    continue;

                if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(",")));
                else if (isInsertOrUpdate)
                {
                    var lengthExpr = Expression.Property(builderExpr, nameof(StringBuilder.Length));
                    var greaterThanExpr = Expression.GreaterThan(lengthExpr, Expression.Constant(0));
                    var addCommaExpr = Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(","));
                    blockBodies.Add(Expression.IfThen(greaterThanExpr, addCommaExpr));
                }
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

                Expression fieldValueExpr = Expression.PropertyOrField(typedUpdateObjExpr, memberMapper.MemberName);
                var fieldValueType = memberInfo.GetMemberType();
                if (isInsertOrUpdate)
                {
                    //if(!dbParameters.Contains(parameterName))
                    //{
                    //  var dbFieldValue = memberMapper.TypeHandler.ToFieldValue(ormProvider, fieldVallueType, fieldValue);
                    //  dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue);
                    //}

                    //var dbFieldValue = memberMapper.TypeHandler.ToFieldValue(ormProvider, fieldVallueType, fieldValue);
                    var underlyingTypeExpr = Expression.Constant(fieldValueType);
                    Expression nativeDbTypeExpr = Expression.Constant(memberMapper.NativeDbType);
                    var typeHandlerExpr = Expression.Constant(memberMapper.TypeHandler);
                    methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
                    Expression myFieldValueExpr = fieldValueExpr;
                    if (fieldValueExpr.Type != typeof(object))
                        myFieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                    var dbFieldValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, underlyingTypeExpr, myFieldValueExpr);

                    //dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue);
                    methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                    nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));
                    var dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, myParameterNameExpr, nativeDbTypeExpr, dbFieldValueExpr);
                    methodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add), new Type[] { typeof(object) });
                    var addParameterExpr = Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr);

                    methodInfo = typeof(IDataParameterCollection).GetMethod(nameof(IDataParameterCollection.Contains));
                    var notContainsExpr = Expression.IsFalse(Expression.Call(dbParametersExpr, methodInfo, myParameterNameExpr));
                    blockBodies.Add(Expression.IfThen(notContainsExpr, addParameterExpr));
                }
                else AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueType, fieldValueExpr, memberMapper, blockParameters, blockBodies);
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
                //  var dbFieldValue = memberMapper.TypeHandler.ToFieldValue(ormProvider, memberMapper.UnderlyingType, fieldValue);
                //  dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue);
                //}

                //var dbFieldValue = memberMapper.TypeHandler.ToFieldValue(ormProvider, memberMapper.UnderlyingType, fieldValue);
                var underlyingTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.UnderlyingType));
                Expression nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
                var typeHandlerExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler));
                methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
                Expression myFieldValueExpr = fieldValueExpr;
                if (fieldValueExpr.Type != typeof(object))
                    myFieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                var dbFieldValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, underlyingTypeExpr, myFieldValueExpr);

                //dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue);
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));
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
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
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
                    AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldVallueType, fieldValueExpr, memberMapper, blockParameters, blockBodies);
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
    public static object BuildDeleteCommandInitializer(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool hasSuffix)
    {
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(ormProvider, mapProvider, entityType, whereObjType);
        var commandInitializerCache = hasSuffix ? deleteMultiCommandInitializerCache : deleteCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var headSql = $"DELETE FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ";
            return BuildWhereSqlParameters(ormProvider, mapProvider, entityType, whereObjType, true, hasSuffix, false, "whereObj", headSql);
        });
    }
    public static (bool, string, Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>)
        BuildDeleteBulkCommandInitializer(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObjs)
    {
        object whereObj = null;
        var enumerable = whereObjs as IEnumerable;
        foreach (var item in enumerable)
        {
            whereObj = item;
            break;
        }
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(ormProvider, mapProvider, entityType, whereObjType);
        return deleteBulkCommandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
            var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");
            var suffixExpr = Expression.Parameter(typeof(string), "suffix");

            ParameterExpression dictExpr = null;
            ParameterExpression typedWhereObjExpr = null;
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            var fieldValueExpr = Expression.Variable(typeof(object), "fieldValue");
            var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
            blockParameters.AddRange(new[] { fieldValueExpr, parameterNameExpr });
            bool isEntityType = false;
            List<MemberInfo> memberInfos = null;
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var isMultiKeys = entityMapper.KeyMembers.Count > 1;
            bool isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(whereObjType);

            if (isDictionary)
            {
                dictExpr = Expression.Variable(typeof(IDictionary<string, object>), "dict");
                blockParameters.Add(dictExpr);
            }
            else
            {
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
            var index = 0;
            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
            var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });
            var tryGetValueMethodInfo = typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.TryGetValue));

            string headSql = null;
            if (isMultiKeys)
            {
                var bulkHeadSql = $"DELETE FROM {ormProvider.GetFieldName(entityMapper.TableName)} WHERE ";
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(bulkHeadSql)));
            }
            else headSql = $"DELETE FROM {ormProvider.GetFieldName(entityMapper.TableName)} WHERE {ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)} IN (";

            foreach (var keyMapper in entityMapper.KeyMembers)
            {
                Expression myFieldValueExpr = null;
                var keyMemberExpr = Expression.Constant(keyMapper.MemberName);
                var fieldVallueType = keyMapper.MemberType;
                if (isDictionary)
                {
                    var isFalseExpr = Expression.IsFalse(Expression.Call(dictExpr, tryGetValueMethodInfo, keyMemberExpr, fieldValueExpr));
                    var exceptionExpr = Expression.Constant(new ArgumentNullException(nameof(whereObjs), $"字典参数缺少主键字段{keyMapper.MemberName}"));
                    blockBodies.Add(Expression.IfThen(isFalseExpr, Expression.Throw(exceptionExpr, typeof(ArgumentNullException))));
                    myFieldValueExpr = Expression.Convert(fieldValueExpr, keyMapper.MemberType);
                }
                else
                {
                    if (isEntityType)
                    {
                        if (!memberInfos.Exists(f => f.Name == keyMapper.MemberName))
                            throw new ArgumentNullException("whereObj", $"参数类型{whereObjType.FullName}缺少主键字段{keyMapper.MemberName}");
                        myFieldValueExpr = Expression.PropertyOrField(typedWhereObjExpr, keyMapper.MemberName);
                        fieldVallueType = myFieldValueExpr.Type;
                    }
                    else myFieldValueExpr = Expression.Convert(whereObjExpr, keyMapper.MemberType);
                }

                var parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName;
                var myParameterNameExpr = Expression.Call(concatMethodInfo, Expression.Constant(parameterName), suffixExpr);
                blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));

                if (isMultiKeys)
                {
                    if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                    var assignExpr = Expression.Constant($"{ormProvider.GetFieldName(keyMapper.FieldName)}=");
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, assignExpr));
                }
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
                AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldVallueType, myFieldValueExpr, keyMapper, blockParameters, blockBodies);
                index++;
            }
            var whereSqlParameters = Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, builderExpr, whereObjExpr, suffixExpr).Compile();

            return (isMultiKeys, headSql, whereSqlParameters);
        });
    }
    public static int BuildInsertHashKey(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type insertObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames)
    {
        var hashCode = new HashCode();
        hashCode.Add(ormProvider);
        hashCode.Add(mapProvider);
        hashCode.Add(entityType);
        hashCode.Add(insertObjType);
        AddFieldHashCode(hashCode, onlyFieldNames);
        AddFieldHashCode(hashCode, ignoreFieldNames);
        return hashCode.ToHashCode();
    }
    public static int BuildInsertHashKey(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type insertObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool isReturnIdentity)
    {
        var hashCode = new HashCode();
        hashCode.Add(ormProvider);
        hashCode.Add(mapProvider);
        hashCode.Add(entityType);
        hashCode.Add(insertObjType);
        AddFieldHashCode(hashCode, onlyFieldNames);
        AddFieldHashCode(hashCode, ignoreFieldNames);
        hashCode.Add(isReturnIdentity);
        return hashCode.ToHashCode();
    }
    private static void AddFieldHashCode(HashCode hashCode, List<string> fieldNames)
    {
        if (fieldNames == null)
        {
            hashCode.Add(0);
            return;
        }
        hashCode.Add(fieldNames.Count);
        foreach (var fieldName in fieldNames)
            hashCode.Add(fieldName);
    }
}
