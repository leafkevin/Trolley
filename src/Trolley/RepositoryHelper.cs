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
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public static class RepositoryHelper
{
    private static readonly ConcurrentDictionary<Type, List<MemberInfo>> typeMemberInfos = new();
    private static readonly ConcurrentDictionary<int, Func<object, object>> memberGetterCache = new();
    private static readonly ConcurrentDictionary<int, Action<object, object>> memberSetterCache = new();

    private static readonly ConcurrentDictionary<int, Func<IDictionary<string, object>, string>> shardingTableNameGetters = new();
    private static readonly ConcurrentDictionary<int, Func<object, object[], string>> shardingTableNameBulkGetters = new();

    private static readonly ConcurrentDictionary<int, Action<IDataParameterCollection, IOrmProvider, object>> queryRawSqlCommandInitializerCache = new();

    private static readonly ConcurrentDictionary<int, Func<IDataParameterCollection, DbContext, object, string>> queryByCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<IDataParameterCollection, DbContext, object, string>> queryByIdCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<IDataParameterCollection, DbContext, object, string>> queryByIdsCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<IDataParameterCollection, DbContext, object, string>> existsByCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<IDataParameterCollection, DbContext, object, string>> existsByIdCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<IDataParameterCollection, DbContext, object, string>> existsByIdsCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<IDataParameterCollection, DbContext, object, string>> deleteByCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<IDataParameterCollection, DbContext, object, string>> deleteByIdCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<IDataParameterCollection, DbContext, object, string>> deleteByIdsCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> whereCommandInitializerCache = new();


    private static readonly ConcurrentDictionary<int, object> dictBulkCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> typedCreateCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> typedUpdateCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> typedCreateBulkCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> typedUpdateBulkCommandInitializerCache = new();

    private static readonly ConcurrentDictionary<int, object> createWithCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> createWithBulkCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> updateWithCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> updateWithBulkCommandInitializerCache = new();

    private static readonly ConcurrentDictionary<int, object> createCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> createBulkCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<DbContext, ITheaCommand, IEnumerable, int, CancellationToken, Task<int>>> createBulkAsyncCommandExecutorCache = new();
    private static readonly ConcurrentDictionary<int, Action<StringBuilder, DbContext, object>> createFieldsSqlCache = new();
    private static readonly ConcurrentDictionary<int, object> createValuesSqlParametersCache = new();
    private static readonly ConcurrentDictionary<int, object> createBulkValuesSqlParametersCache = new();

    private static readonly ConcurrentDictionary<int, object> updateCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> updateBulkCommandInitializerCache = new();


    private static readonly ConcurrentDictionary<int, Delegate> readerDeserializerGetters = new();
    private static readonly ConcurrentDictionary<int, Delegate> readerDeserializerAsyncGetters = new();

    private static readonly ConcurrentDictionary<Type, Func<object>> creatorCache = new();
    private static readonly ConcurrentDictionary<int, Func<object[], object>> parameterizedCreatorCache = new();

    /// <summary>
    /// 实体参数使用
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="dbParametersExpr"></param>
    /// <param name="ormProviderExpr"></param>
    /// <param name="isNullable">字典值需要设置为true</param>
    /// <param name="parameterNameExpr"></param>
    /// <param name="fieldValueExpr"></param>
    /// <param name="memberMapper"></param>
    /// <param name="blockBodies"></param>
    private static void AddValueParameter(DbContext dbContext, Expression dbParametersExpr, Expression ormProviderExpr,
        bool isNullable, Expression parameterNameExpr, Expression fieldValueExpr, MemberMap memberMapper, List<Expression> blockBodies)
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
            parameterValueExpr = Expression.Call(typeHandlerExpr, methodInfo, parameterValueExpr);
        }
        else
        {
            var ormProvider = dbContext.OrmProvider;
            var targetType = memberMapper.MappedTargetType;
            var fieldValueType = fieldValueExpr.Type.ToUnderlyingType();
            if (fieldValueType != targetType)
            {
                var valueGetter = ormProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, dbContext);
                if (fieldValueType != typeof(object))
                    parameterValueExpr = Expression.Convert(parameterValueExpr, typeof(object));
                parameterValueExpr = Expression.Invoke(Expression.Constant(valueGetter), parameterValueExpr);
            }
        }

        Expression nativeDbTypeExpr = Expression.Constant(memberMapper.NativeDbType);
        if (nativeDbTypeExpr.Type != typeof(object))
            nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));
        if (isNullable)
        {
            var conditionExpr = Expression.Equal(fieldValueExpr, Expression.Constant(null));
            parameterValueExpr = Expression.Condition(conditionExpr, Expression.Constant(DBNull.Value), parameterValueExpr);
        }
        var dbParameterExpr = Expression.Call(ormProviderExpr, createParameterMethodInfo, parameterNameExpr, nativeDbTypeExpr, parameterValueExpr);
        blockBodies.Add(Expression.Call(dbParametersExpr, addMethodInfo, dbParameterExpr));
    }
    /// <summary>
    /// 字典参数使用
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="dbContextExpr"></param>
    /// <param name="dbParametersExpr"></param>
    /// <param name="ormProviderExpr"></param>
    /// <param name="parameterNameExpr"></param>
    /// <param name="fieldValueExpr"></param>
    /// <param name="memberMapperExpr"></param>
    /// <param name="blockBodies"></param>
    private static void AddValueParameter(DbContext dbContext, Expression dbContextExpr, Expression dbParametersExpr, Expression ormProviderExpr,
        Expression parameterNameExpr, Expression fieldValueExpr, Expression memberMapperExpr, List<Expression> blockBodies)
    {
        var typeHandlerExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler));
        var methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
        Expression parameterValueExpr = fieldValueExpr;
        if (fieldValueExpr.Type != typeof(object))
            parameterValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
        var typeHandlerValueExpr = Expression.Call(typeHandlerExpr, methodInfo, parameterValueExpr);

        var targetTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.MappedTargetType));
        methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetParameterValueGetter));

        var fieldValueTypeExpr = Expression.Call(fieldValueExpr, typeof(object).GetMethod(nameof(object.GetType)));
        var isNullableExpr = Expression.IsFalse(Expression.Property(memberMapperExpr, nameof(MemberMap.IsRequired)));
        var valueGetterExpr = Expression.Call(ormProviderExpr, methodInfo, fieldValueTypeExpr, targetTypeExpr, isNullableExpr, dbContextExpr);
        var valueGetterValueExpr = Expression.Invoke(valueGetterExpr, parameterValueExpr);

        //var fieldValue = itemValue == null ? DBNull.Value : memberMapper.TypeHandler == null? valueGetterValue : typeHandlerValue;
        //dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue);
        var typeHandlerConditionExpr = Expression.Equal(typeHandlerExpr, Expression.Constant(null));
        var typedValueExpr = Expression.Condition(typeHandlerConditionExpr, valueGetterValueExpr, typeHandlerValueExpr);
        var conditionExpr = Expression.Equal(fieldValueExpr, Expression.Constant(null));
        var dbFieldValueExpr = Expression.Condition(conditionExpr, Expression.Constant(DBNull.Value), typedValueExpr);

        methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), [typeof(string), typeof(object), typeof(object)]);
        var nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
        var dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, dbFieldValueExpr);

        methodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add), [typeof(object)]);
        var typeHandlerAddParametersExpr = Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr);
        blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr));
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
    public static string BuildSelectFieldsSqlPart(DbContext dbContext, EntityMap entityMapper, Type parametersType)
    {
        var builder = new StringBuilder();
        var memberInfos = parametersType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.CanWrite()).ToList();

        var index = 0;
        var ormProvider = dbContext.OrmProvider;
        var isCanMapToHandler = dbContext.EntityMapProvider.IsCanMapTo;
        foreach (var memberInfo in memberInfos)
        {
            if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                || memberMapper.IsIgnore || memberMapper.IsNavigation)
                continue;

            if (index > 0) builder.Append(',');
            builder.Append(ormProvider.GetFieldName(memberMapper.FieldName));
            if (!isCanMapToHandler(memberMapper.FieldName, memberInfo.Name))
                builder.Append(" AS " + ormProvider.GetFieldName(memberMapper.MemberName));
            index++;
        }
        return builder.ToString();
    }
    public static Action<IDataParameterCollection, IOrmProvider, object> BuildQueryRawSqlCommandInitializer(IOrmProvider ormProvider, string rawSql, object parameters)
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
                    .Where(f => f.MemberType == MemberTypes.Property || f.MemberType == MemberTypes.Field).ToList();
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

    public static Func<IDataParameterCollection, DbContext, object, string> BuildQueryWhereCommandInitializer(DbContext dbContext, Type entityType, object whereObjs, bool isUseKey, bool isMultiple, bool isBulk)
    {
        Type whereObjType = null;
        bool isDictionary = false;
        bool hasWhere = whereObjs != null;
        if (isBulk)
        {
            object firstWhereObj = null;
            var typedWhereObjs = whereObjs as IEnumerable;
            foreach (var whereObj in typedWhereObjs)
            {
                firstWhereObj = whereObj;
                break;
            }
            if (firstWhereObj is IDictionary<string, object> dict)
            {
                isDictionary = true;
                whereObjType = typeof(IDictionary<string, object>);
            }
            else whereObjType = firstWhereObj.GetType();
        }
        else whereObjType = hasWhere ? whereObjs.GetType() : entityType;

        var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.EntityMapProvider, entityType, whereObjType, isMultiple);
        var commandInitializerCache = isBulk ? queryByIdsCommandInitializerCache : isUseKey ? queryByIdCommandInitializerCache : queryByCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = dbContext.EntityMapProvider.GetEntityMap(entityType);
            var fieldSql = BuildSelectFieldsSqlPart(dbContext, entityMapper, entityType);
            var headSql = $"SELECT {fieldSql}";
            if (!hasWhere)
            {
                var ormProvider = dbContext.OrmProvider;
                headSql += $" FROM {ormProvider.GetTableName(entityMapper.TableName)}";
                return (dbParameters, dbContext, parameters) => headSql;
            }
            var commandInitializer = BuildWhereObjsCommandInitializer(dbContext, entityType, whereObjType, false, isUseKey, false, isMultiple, isBulk, headSql, null);
            if (isDictionary && isBulk)
            {
                var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>, DbContext, object, string>;
                return (dbParameters, dbContext, whereObjs) =>
                {
                    IDictionary<string, object> dict = null;
                    var typedWhereObjs = whereObjs as IEnumerable;
                    foreach (var whereObj in typedWhereObjs)
                    {
                        dict = whereObj as IDictionary<string, object>;
                        break;
                    }
                    var valueSetters = BuildBulkDictKeysValueSetters(dbContext, entityType, dict);
                    return typedCommandInitializer.Invoke(dbParameters, valueSetters, dbContext, whereObjs);
                };
            }
            return commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
        });
    }
    public static Func<IDataParameterCollection, DbContext, object, string> BuildExistsCommandInitializer(DbContext dbContext, Type entityType, object whereObjs, bool isUseKey, bool isMultiple, bool isBulk)
    {
        Type whereObjType = null;
        bool isDictionary = false;
        bool hasWhere = whereObjs != null;
        if (isBulk)
        {
            object firstWhereObj = null;
            var typedWhereObjs = whereObjs as IEnumerable;
            foreach (var whereObj in typedWhereObjs)
            {
                firstWhereObj = whereObj;
                break;
            }
            if (firstWhereObj is IDictionary<string, object> dict)
            {
                isDictionary = true;
                whereObjType = typeof(IDictionary<string, object>);
            }
            else whereObjType = firstWhereObj.GetType();
        }
        else whereObjType = hasWhere ? whereObjs.GetType() : entityType;

        var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.EntityMapProvider, entityType, whereObjType, isMultiple);
        var commandInitializerCache = isBulk ? existsByIdsCommandInitializerCache : isUseKey ? existsByIdCommandInitializerCache : existsByCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = dbContext.EntityMapProvider.GetEntityMap(entityType);
            var fieldSql = BuildSelectFieldsSqlPart(dbContext, entityMapper, entityType);
            var headSql = "SELECT 1";
            if (!hasWhere)
            {
                var ormProvider = dbContext.OrmProvider;
                headSql += $" FROM {ormProvider.GetTableName(entityMapper.TableName)} LIMIT 1";
                return (dbParameters, dbContext, parameters) => headSql;
            }
            var commandInitializer = BuildWhereObjsCommandInitializer(dbContext, entityType, whereObjType, false, isUseKey, false, isMultiple, isBulk, headSql, " LIMIT 1");
            if (isDictionary && isBulk)
            {
                var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>, DbContext, object, string>;
                return (dbParameters, dbContext, whereObjs) =>
                {
                    IDictionary<string, object> dict = null;
                    var typedWhereObjs = whereObjs as IEnumerable;
                    foreach (var whereObj in typedWhereObjs)
                    {
                        dict = whereObj as IDictionary<string, object>;
                        break;
                    }
                    var valueSetters = BuildBulkDictKeysValueSetters(dbContext, entityType, dict);
                    return typedCommandInitializer.Invoke(dbParameters, valueSetters, dbContext, whereObjs);
                };
            }
            return commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
        });
    }
    public static Func<IDataParameterCollection, DbContext, object, string> BuildDeleteCommandInitializer(DbContext dbContext, Type entityType, object whereObjs, bool isUseKey, bool isMultiple, bool isBulk)
    {
        Type whereObjType = null;
        bool isDictionary = false;
        if (isBulk)
        {
            object firstWhereObj = null;
            var typedWhereObjs = whereObjs as IEnumerable;
            foreach (var whereObj in typedWhereObjs)
            {
                firstWhereObj = whereObj;
                break;
            }
            if (firstWhereObj is IDictionary<string, object> dict)
            {
                isDictionary = true;
                whereObjType = typeof(IDictionary<string, object>);
            }
            else whereObjType = firstWhereObj.GetType();
        }
        else whereObjType = whereObjs.GetType();

        var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.EntityMapProvider, entityType, whereObjType, isMultiple);
        var commandInitializerCache = isBulk ? deleteByIdsCommandInitializerCache : isUseKey ? deleteByIdCommandInitializerCache : deleteByCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var commandInitializer = BuildWhereObjsCommandInitializer(dbContext, entityType, whereObjType, false, isUseKey, false, isMultiple, isBulk, "DELETE", null);
            if (isDictionary && isBulk)
            {
                var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>, DbContext, object, string>;
                return (dbParameters, dbContext, whereObjs) =>
                {
                    IDictionary<string, object> dict = null;
                    var typedWhereObjs = whereObjs as IEnumerable;
                    foreach (var whereObj in typedWhereObjs)
                    {
                        dict = whereObj as IDictionary<string, object>;
                        break;
                    }
                    var valueSetters = BuildBulkDictKeysValueSetters(dbContext, entityType, dict);
                    return typedCommandInitializer.Invoke(dbParameters, valueSetters, dbContext, whereObjs);
                };
            }
            return commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
        });
    }
    public static object BuildWhereCommandInitializer(DbContext dbContext, Type entityType, Type whereObjType, bool isFunc, bool isUseKey, bool isWithKey)
    {
        var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.EntityMapProvider, entityType, whereObjType, isFunc, isUseKey, isWithKey);
        return whereCommandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
            var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
            var dbContextExpr = Expression.Parameter(typeof(DbContext), "dbContext");
            var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            if (isFunc) blockParameters.Add(builderExpr);
            AddWhereSqlParameters(dbParametersExpr, builderExpr, dbContextExpr, whereObjExpr, dbContext, entityType, whereObjType, isUseKey, isWithKey, false, false, blockParameters, blockBodies);
            if (isFunc)
            {
                var returnExpr = Expression.Call(builderExpr, typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes));
                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));
                return Expression.Lambda<Func<IDataParameterCollection, DbContext, object, string>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, whereObjExpr).Compile();
            }
            return Expression.Lambda<Action<IDataParameterCollection, StringBuilder, DbContext, object>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, dbContextExpr, whereObjExpr).Compile();
        });
    }
    private static object BuildWhereObjsCommandInitializer(DbContext dbContext, Type entityType, Type whereObjType,
         bool isOnlyWhereSql, bool isUseKey, bool isWithKey, bool isMultiple, bool isBulk, string headSql, string tailSql)
    {
        object commandInitializer = null;
        var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
        var dbContextExpr = Expression.Parameter(typeof(DbContext), "dbContext");
        var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");
        var builderExpr = Expression.Variable(typeof(StringBuilder), "builder");

        ParameterExpression valueSettersExpr = null;
        var blockParameters = new List<ParameterExpression>() { builderExpr };
        var blockBodies = new List<Expression>();

        var constructor = typeof(StringBuilder).GetConstructor(Type.EmptyTypes);
        var newExpr = Expression.New(constructor);
        blockBodies.Add(Expression.Assign(builderExpr, newExpr));

        var ormProvider = dbContext.OrmProvider;
        var entityMapper = dbContext.EntityMapProvider.GetEntityMap(entityType);
        var isMultiKeys = entityMapper.KeyMembers.Count > 1;
        bool isInExpr = isBulk && isUseKey && !isMultiKeys;

        MethodInfo methodInfo = null;
        var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), [typeof(string)]);
        var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)]);
        var dictItemPropertyInfo = typeof(IDictionary<string, object>).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(string)).First();

        bool isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(whereObjType);
        Type valueSettersType = null;
        if (isDictionary && isBulk)
        {
            valueSettersType = typeof(List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>);
            valueSettersExpr = Expression.Parameter(valueSettersType, "valueSetters");
        }

        var typedWhereObjExpr = Expression.Variable(whereObjType, isDictionary ? "dict" : "typedWhereObj");
        blockParameters.Add(typedWhereObjExpr);

        //builder.Append($"{headSql} FROM {ormProvider.GetTableName(tableName)} {tailSql}");
        if (!isOnlyWhereSql)
        {
            string tableName = ormProvider.GetTableName(entityMapper.TableName);
            var fixedHeadSql = $"{headSql} FROM {tableName} WHERE";
            if (isInExpr) fixedHeadSql += $"{ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)} IN (";
            blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(fixedHeadSql)));
        }

        ParameterExpression indexExpr = null;
        ParameterExpression enumeratorExpr = null;
        List<Expression> myBlockBodies = blockBodies;
        List<Expression> loopBodies = null;
        var breakLabel = Expression.Label();

        if (isBulk)
        {
            indexExpr = Expression.Variable(typeof(int), "index");
            enumeratorExpr = Expression.Variable(typeof(IEnumerable), "enumerator");
            loopBodies = new List<Expression>();
            blockParameters.AddRange([indexExpr, enumeratorExpr]);

            //var index = 0;
            //var enumerator = dict.GetEnumerator();
            blockBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
            methodInfo = typeof(IEnumerable).GetMethod(nameof(IEnumerable.GetEnumerator));
            var enumerableExpr = Expression.TypeAs(whereObjExpr, typeof(IEnumerable));
            blockBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(enumerableExpr, methodInfo)));

            //if(!enumerator.MoveNext())
            //  break;
            methodInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
            var ifFalseExpr = Expression.IsFalse(Expression.Call(enumeratorExpr, methodInfo));
            loopBodies.Add(Expression.IfThen(ifFalseExpr, Expression.Break(breakLabel)));

            //var typedWhereObj = enumerator.Current as TType;
            var currentExpr = Expression.Property(enumeratorExpr, nameof(IEnumerator.Current));
            loopBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(currentExpr, whereObjType)));

            // if (index > 0) builder.Append(jointMark);
            var greaterThanExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0));
            var jointMark = isMultiKeys ? " OR " : ",";
            var addJointMarkExpr = Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(jointMark));
            loopBodies.Add(Expression.IfThen(greaterThanExpr, addJointMarkExpr));
            myBlockBodies = loopBodies;
        }
        else blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));

        if (isDictionary && isUseKey && !isBulk || !isDictionary)
        {
            Dictionary<string, MemberInfo> targetMemberInfos = null;
            bool isEntityType = false;
            if (!isDictionary)
            {
                isEntityType = whereObjType.IsEntityType(out _);
                if (isEntityType) targetMemberInfos = whereObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property || f.MemberType == MemberTypes.Field).ToDictionary(f => f.Name.ToLower(), f => f);
                else if (!(isUseKey && entityMapper.KeyMembers.Count == 1))
                    throw new NotSupportedException("不支持非单主键字段的业务场景");
            }

            var index = 0;
            var hasSuffix = isMultiple || isBulk;
            var ormProviderExpr = Expression.Variable(typeof(IOrmProvider), "ormProvider");
            blockParameters.Add(ormProviderExpr);
            blockBodies.Add(Expression.Assign(ormProviderExpr, Expression.Property(dbContextExpr, nameof(DbContext.OrmProvider))));

            var filterMemberMappers = isUseKey ? entityMapper.KeyMembers : entityMapper.MemberMaps;
            foreach (var memberMapper in filterMemberMappers)
            {
                ParameterExpression itemKeyExpr = null;
                MemberInfo targetMemberInfo = null;
                if (memberMapper.IsIgnore || memberMapper.IsNavigation)
                    continue;

                var lowerMemberName = memberMapper.MemberName.ToLower();
                if (isDictionary)
                {
                    //if(!dict.TryGetKeyIgnoreCase(targetMemberInfo.Name.ToLower(),out var itemKey))
                    //  throw new KeyNotFoundException($"字典参数中{parametersType.FullName}缺少Key:{memberMapper.MemberName}的成员");
                    itemKeyExpr = Expression.Variable(typeof(string), $"{memberMapper.MemberName.ToCamel()}ItemKey");
                    blockParameters.Add(itemKeyExpr);
                    var lowerMemberNameExpr = Expression.Constant(lowerMemberName);
                    methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.TryGetKeyIgnoreCase));
                    var isContainsKeyExpr = Expression.Call(methodInfo, typedWhereObjExpr, lowerMemberNameExpr, itemKeyExpr);
                    var exception = new KeyNotFoundException($"字典参数中{whereObjType.FullName}缺少Key:{memberMapper.MemberName}的成员");
                    myBlockBodies.Add(Expression.IfThen(Expression.IsFalse(isContainsKeyExpr), Expression.Throw(Expression.Constant(exception))));
                }
                else
                {
                    if (!targetMemberInfos.TryGetValue(lowerMemberName, out targetMemberInfo))
                    {
                        if (!isUseKey) continue;
                        throw new KeyNotFoundException($"参数类型{whereObjType.FullName}缺少{memberMapper.MemberName}的成员");
                    }
                }

                var memberNameExpr = Expression.Constant(memberMapper.MemberName);
                Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix + (isWithKey ? "k" : "") + memberMapper.MemberName);
                if (hasSuffix)
                {
                    var parameterNameExpr = Expression.Variable(typeof(string), memberMapper.MemberName.ToCamel() + "Name");
                    blockParameters.Add(parameterNameExpr);

                    Expression suffixExpr = Expression.Property(dbParametersExpr, nameof(IDataParameterCollection.Count));
                    suffixExpr = Expression.Call(suffixExpr, typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes));
                    myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, suffixExpr);
                    myBlockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
                    myParameterNameExpr = parameterNameExpr;
                }

                Expression contentExpr = null;
                if (isInExpr) contentExpr = myParameterNameExpr;
                else
                {
                    contentExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}=");
                    contentExpr = Expression.Call(concatMethodInfo, contentExpr, myParameterNameExpr);
                }
                if (index > 0) myBlockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                myBlockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, contentExpr));

                Expression fieldValueExpr = null;
                if (isDictionary) fieldValueExpr = Expression.Property(typedWhereObjExpr, dictItemPropertyInfo, itemKeyExpr);
                else if (isEntityType) fieldValueExpr = Expression.PropertyOrField(typedWhereObjExpr, targetMemberInfo.Name);
                else fieldValueExpr = whereObjExpr;
                AddValueParameter(dbContext, dbParametersExpr, ormProviderExpr, isDictionary, myParameterNameExpr, fieldValueExpr, memberMapper, myBlockBodies);
                index++;
            }
            if (index <= 0) throw new Exception($"没有找到where条件语句");
        }
        else if (isBulk)
        {
            var loopIndexExpr = Expression.Variable(typeof(int), "loopIndex");
            var countExpr = Expression.Variable(typeof(int), "count");
            var myLoopBodies = new List<Expression>();
            blockParameters.AddRange([loopIndexExpr, countExpr]);
            var myBreakLabel = Expression.Label();

            //var loopIndex = 0;
            //var count = valueSetters.Count;
            loopBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
            loopBodies.Add(Expression.Assign(countExpr, Expression.Property(valueSettersExpr, "Count")));

            //if(loopIndex>=count)
            //  break;
            var greaterThanExpr = Expression.GreaterThanOrEqual(loopIndexExpr, countExpr);
            myLoopBodies.Add(Expression.IfThen(greaterThanExpr, Expression.Break(myBreakLabel)));

            //var valueSetter = valueSetters[loopIndex];
            //var suffix = dbParameters.Count.ToString();
            //valueSetter.Invoke(dbParameters, builder, dict, suffix);
            //loopIndex++;
            Expression suffixExpr = Expression.Property(dbParametersExpr, nameof(IDataParameterCollection.Count));
            suffixExpr = Expression.Call(suffixExpr, typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes));
            var itemProperyInfo = valueSettersType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(int)).First();
            var valueSetterExpr = Expression.Property(valueSettersExpr, itemProperyInfo, loopIndexExpr);

            myLoopBodies.Add(Expression.Invoke(valueSetterExpr, dbParametersExpr, builderExpr, typedWhereObjExpr, suffixExpr));
            myLoopBodies.Add(Expression.AddAssign(loopIndexExpr, Expression.Constant(1)));
            loopBodies.Add(Expression.Loop(Expression.Block(myLoopBodies), myBreakLabel));
        }
        else
        {
            indexExpr = Expression.Variable(typeof(int), "index");
            enumeratorExpr = Expression.Variable(typeof(IEnumerator<KeyValuePair<string, object>>), "enumerator");
            var itemKeyExpr = Expression.Variable(typeof(string), "itemKey");
            var itemValueExpr = Expression.Variable(typeof(object), "itemValue");
            var memberMapperExpr = Expression.Variable(typeof(MemberMap), "memberMapper");
            var entityMapperExpr = Expression.Constant(entityMapper);
            var ormProviderExpr = Expression.Variable(typeof(IOrmProvider), "ormProvider");

            blockParameters.AddRange([ormProviderExpr, indexExpr, enumeratorExpr, itemKeyExpr, itemValueExpr, memberMapperExpr]);
            blockBodies.Add(Expression.Assign(ormProviderExpr, Expression.Property(dbContextExpr, nameof(DbContext.OrmProvider))));
            var continueLabel = Expression.Label();

            //var index = 0;
            //var enumerator = dict.GetEnumerator();
            blockBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
            methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
            blockBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(typedWhereObjExpr, methodInfo)));

            //if(!enumerator.MoveNext())
            //  break;
            loopBodies = new List<Expression>();
            methodInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
            var ifFalseExpr = Expression.IsFalse(Expression.Call(enumeratorExpr, methodInfo));
            loopBodies.Add(Expression.IfThen(ifFalseExpr, Expression.Break(breakLabel)));

            //var itemKey = enumerator.Current.Key;
            //var fieldValue = enumerator.Current.Value;          
            var currentExpr = Expression.Property(enumeratorExpr, nameof(IEnumerator.Current));
            loopBodies.Add(Expression.Assign(itemKeyExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Key))));

            //if(!entityMapper.TryGetMemberMap(itemKey, out var memberMapper)
            //  || memberMapper.IsIgnore || memberMapper.IsNavigation) continue;
            methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.TryGetMemberMap));
            Expression isContinueExpr = Expression.IsFalse(Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr));
            isContinueExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnore));
            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsNavigation)));
            loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

            //if(index > 0) builder.Append(" AND ");
            if (isMultiKeys)
            {
                var greaterThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0));
                var appendExpr = Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND "));
                loopBodies.Add(Expression.IfThen(greaterThenExpr, appendExpr));
            }
            var parameterNameExpr = Expression.Variable(typeof(string));
            blockParameters.Add(parameterNameExpr);

            //var parameterName = $"{ormProvider.ParameterPrefix}{(isWithKey ? "k" : "")}{memberMapper.MemberName}";
            Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix + (isWithKey ? "k" : ""));
            var memberNameExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.MemberName));
            if (isMultiple)
            {
                //var parameterName = $"{ormProvider.ParameterPrefix}{(isWithKey ? "k" : "")}{memberMapper.MemberName}{dbParameters.Count}";
                methodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string), typeof(string)]);
                Expression suffixExpr = Expression.Property(dbParametersExpr, nameof(IDataParameterCollection.Count));
                suffixExpr = Expression.Call(suffixExpr, typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes));
                myParameterNameExpr = Expression.Call(methodInfo, myParameterNameExpr, memberNameExpr, suffixExpr);
            }
            else myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, memberNameExpr);
            loopBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));

            //builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}=@ParameterName");
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetFieldName));
            Expression fieldNameExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.FieldName));
            fieldNameExpr = Expression.Call(ormProviderExpr, methodInfo, fieldNameExpr);
            methodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string), typeof(string)]);
            var sqlExpr = Expression.Call(concatMethodInfo, fieldNameExpr, Expression.Constant("="), parameterNameExpr);
            loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, sqlExpr));
            AddValueParameter(dbContext, dbContextExpr, dbParametersExpr, ormProviderExpr, parameterNameExpr, currentExpr, memberMapperExpr, loopBodies);
        }

        if (isBulk)
        {
            //index++;
            loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));
            blockBodies.Add(Expression.Loop(Expression.Block(loopBodies), breakLabel));
            if (isInExpr) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(")")));
        }
        if (!string.IsNullOrEmpty(tailSql))
            blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(tailSql)));

        methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
        var returnExpr = Expression.Call(builderExpr, methodInfo);
        var resultLabelExpr = Expression.Label(typeof(string));
        blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));

        if (isDictionary && isBulk) commandInitializer = Expression.Lambda<Func<IDataParameterCollection, List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>, DbContext, object, string>>(
            Expression.Block(blockParameters, blockBodies), dbParametersExpr, valueSettersExpr, dbContextExpr, whereObjExpr).Compile();
        else commandInitializer = Expression.Lambda<Func<IDataParameterCollection, DbContext, object, string>>(
            Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, whereObjExpr).Compile();
        return commandInitializer;
    }
    private static List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>> BuildBulkDictKeysValueSetters(DbContext dbContext, Type entityType, IDictionary<string, object> dict)
    {
        var valueSetters = new List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>();
        int index = 0;
        var entityMapper = dbContext.EntityMapProvider.GetEntityMap(entityType);
        var ormProvider = dbContext.OrmProvider;
        foreach (var memberMapper in entityMapper.KeyMembers)
        {
            if (!dict.TryGetKeyIgnoreCase(memberMapper.MemberName.ToLower(), out var itemKey))
                throw new ArgumentException($"字典参数whereObjs缺少实体表{entityMapper.EntityType.FullName}主键成员{memberMapper.MemberName}");

            Func<IDictionary<string, object>, object> valueGetter = null;
            if (memberMapper.TypeHandler != null)
                valueGetter = insertObj => memberMapper.TypeHandler.ToFieldValue(insertObj[itemKey]);
            else
            {
                var targetType = memberMapper.MappedTargetType;
                var fieldValueType = dict[itemKey].GetType();
                if (fieldValueType.ToUnderlyingType() != targetType)
                {
                    var myValueGetter = ormProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, dbContext);
                    valueGetter = insertObj => myValueGetter.Invoke(insertObj[itemKey]);
                }
                else valueGetter = insertObj => insertObj[itemKey];
            }
            Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string> valueSetter = null;
            if (index > 0)
            {
                valueSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    var fieldValue = valueGetter.Invoke(insertObj);
                    var parameterName = $"{ormProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                    builder.Append(parameterName);
                    dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                };
            }
            else
            {
                valueSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    var fieldValue = valueGetter.Invoke(insertObj);
                    var parameterName = $"{ormProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                    builder.Append(',');
                    builder.Append(parameterName);
                    dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                };
            }
            valueSetters.Add(valueSetter);
            index++;
        }
        return valueSetters;
    }


    public static object BuildTypedCommandInitializer(DbContext dbContext, Type entityType, Type parameterType, int commandType, bool isFunc, bool isSplitSharding, bool hasIdentity, List<string> onlyFields, List<string> ignoreFields)
    {
        var hasOnlyFields = onlyFields != null && onlyFields.Count > 0;
        var hasIgnoreFields = ignoreFields != null && ignoreFields.Count > 0;
        var onlyFieldsKey = hasOnlyFields ? string.Join("-", onlyFields) : "";
        var ignoreFieldsKey = hasIgnoreFields ? string.Join("-", ignoreFields) : "";
        var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.EntityMapProvider, entityType, parameterType, isSplitSharding, hasIdentity, onlyFieldsKey, ignoreFieldsKey);
        var commandInitializerCache = commandType == 1 ? createWithCommandInitializerCache : updateWithCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
            var fieldBuilderExpr = Expression.Variable(typeof(StringBuilder), "fieldBuilder");
            var dbContextExpr = Expression.Parameter(typeof(DbContext), "dbContext");
            var parameterExpr = Expression.Parameter(typeof(object), "parameter");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            var typedParameterExpr = Expression.Variable(parameterType, "typedParameterObj");
            var ormProviderExpr = Expression.Variable(typeof(IOrmProvider), "ormProvider");
            blockParameters.AddRange([typedParameterExpr, ormProviderExpr]);
            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
            blockBodies.Add(Expression.Assign(ormProviderExpr, Expression.Property(dbContextExpr, nameof(DbContext.OrmProvider))));

            ParameterExpression valueBuilderExpr = null;
            ParameterExpression shardingValuesExpr = null;

            List<string> shardingMembers = null;
            var ormProvider = dbContext.OrmProvider;
            var entityMapper = dbContext.EntityMapProvider.GetEntityMap(entityType);

            MethodInfo methodInfo = null;
            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), [typeof(string)]);
            var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)]);
            var dictItemPropertyInfo = typeof(IDictionary<string, object>).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(string)).First();

            if (commandType == 1)
            {
                if (isFunc)
                {
                    valueBuilderExpr = Expression.Variable(typeof(StringBuilder), "valueBuilder");
                    blockParameters.AddRange([fieldBuilderExpr, valueBuilderExpr]);
                    var constructor = typeof(StringBuilder).GetConstructor(Type.EmptyTypes);
                    blockBodies.Add(Expression.Assign(fieldBuilderExpr, Expression.New(constructor)));
                    blockBodies.Add(Expression.Assign(valueBuilderExpr, Expression.New(constructor)));

                    var headSql = $"INSERT INTO {ormProvider.GetTableName(entityMapper.TableName)} (";
                    blockBodies.Add(Expression.Call(fieldBuilderExpr, appendMethodInfo, Expression.Constant(headSql)));
                    blockBodies.Add(Expression.Call(valueBuilderExpr, appendMethodInfo, Expression.Constant(") VALUES (")));
                }
                else valueBuilderExpr = Expression.Parameter(typeof(StringBuilder), "valueBuilder");
            }
            else if (isFunc)
            {
                blockParameters.Add(valueBuilderExpr);
                var constructor = typeof(StringBuilder).GetConstructor(Type.EmptyTypes);
                blockBodies.Add(Expression.Assign(valueBuilderExpr, Expression.New(constructor)));

                var headSql = $"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ";
                blockBodies.Add(Expression.Call(fieldBuilderExpr, appendMethodInfo, Expression.Constant(headSql)));
                blockBodies.Add(Expression.Call(valueBuilderExpr, appendMethodInfo, Expression.Constant(" WHERE ")));
            }

            if (isSplitSharding)
            {
                shardingValuesExpr = Expression.Parameter(typeof(IDictionary<string, object>), "shardingValues");
                if (!dbContext.TableShardingProvider.TryGetTableSharding(entityType, out var tableShardingInfo))
                    throw new InvalidOperationException($"实体表{tableShardingInfo.EntityType.FullName}未配置分表信息，原表名：{entityMapper.TableName}");
                shardingMembers = tableShardingInfo.DependOnMembers;
            }
            int index = 0, whereIndex = 0;
            var targetMemberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => (f.MemberType == MemberTypes.Property || f.MemberType == MemberTypes.Field)).ToList();

            foreach (var memberMapper in entityMapper.MemberMaps)
            {
                if (!targetMemberInfos.TryFind(memberMapper.MemberName, out var memberInfo))
                    continue;

                var parameterValueExpr = Expression.PropertyOrField(typedParameterExpr, memberInfo.Name);
                var parameterName = ormProvider.ParameterPrefix + memberMapper.MemberName;
                var parameterNameExpr = Expression.Constant(parameterName);
                var memberNameExpr = Expression.Constant(memberMapper.MemberName);
                var memberType = memberInfo.GetMemberType();
                var lowerMemberNameExpr = Expression.Constant(memberMapper.MemberName.ToLower());
                Expression memberValueExpr = null;

                //shardingValues[memberMapper.MemberName] = memberValue;
                if (isSplitSharding && shardingMembers.Contains(memberMapper.MemberName))
                {
                    memberValueExpr = parameterValueExpr;
                    if (memberType != memberMapper.UnderlyingType && memberType.ToUnderlyingType() != memberMapper.UnderlyingType)
                    {
                        var valueGetter = ormProvider.GetParameterValueGetter(memberType, memberMapper.UnderlyingType, !memberMapper.IsRequired, dbContext);
                        memberValueExpr = Expression.Invoke(Expression.Constant(valueGetter), parameterValueExpr);
                    }
                    methodInfo = dictItemPropertyInfo.GetSetMethod();
                    blockBodies.Add(Expression.Call(shardingValuesExpr, methodInfo, memberNameExpr, memberValueExpr));
                }
                if (memberMapper.IsAutoIncrement || memberMapper.IsIgnore
                    || memberMapper.IsNavigation || memberMapper.IsRowVersion
                    || (commandType == 1 && memberMapper.IsIgnoreInsert)
                    || (commandType == 2 && memberMapper.IsIgnoreUpdate))
                    continue;

                if (hasOnlyFields && !onlyFields.Contains(memberMapper.MemberName.ToLower()))
                    continue;
                if (hasIgnoreFields && ignoreFields.Contains(memberMapper.MemberName.ToLower()))
                    continue;

                if (commandType == 1)
                {
                    var addExpr1 = Expression.Call(fieldBuilderExpr, appendMethodInfo, Expression.Constant(","));
                    var addExpr2 = Expression.Call(valueBuilderExpr, appendMethodInfo, Expression.Constant(","));
                    if (index > 0) blockBodies.AddRange([addExpr1, addExpr2]);
                    else
                    {
                        var lengthExpr = Expression.Property(fieldBuilderExpr, nameof(StringBuilder.Length));
                        var greaterExpr = Expression.GreaterThan(lengthExpr, Expression.Constant(0));
                        blockBodies.Add(Expression.IfThen(greaterExpr, Expression.Block([addExpr1, addExpr2])));
                    }
                    var fieldNameExpr = Expression.Constant(ormProvider.GetFieldName(memberMapper.FieldName));
                    blockBodies.Add(Expression.Call(fieldBuilderExpr, appendMethodInfo, fieldNameExpr));
                    blockBodies.Add(Expression.Call(valueBuilderExpr, appendMethodInfo, parameterNameExpr));
                    index++;
                }
                else
                {
                    var setSqlExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                    if (isFunc)
                    {
                        if (memberMapper.IsKey)
                        {
                            if (whereIndex > 0) blockBodies.Add(Expression.Call(valueBuilderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                            blockBodies.Add(Expression.Call(valueBuilderExpr, appendMethodInfo, setSqlExpr));
                            whereIndex++;
                        }
                        else
                        {
                            if (index > 0) blockBodies.Add(Expression.Call(fieldBuilderExpr, appendMethodInfo, Expression.Constant(",")));
                            blockBodies.Add(Expression.Call(fieldBuilderExpr, appendMethodInfo, setSqlExpr));
                            index++;
                        }
                    }
                    else
                    {
                        var addExpr = Expression.Call(fieldBuilderExpr, appendMethodInfo, Expression.Constant(","));
                        if (index > 0) blockBodies.Add(addExpr);
                        else
                        {
                            var lengthExpr = Expression.Property(fieldBuilderExpr, nameof(StringBuilder.Length));
                            var greaterExpr = Expression.GreaterThan(lengthExpr, Expression.Constant(0));
                            blockBodies.Add(Expression.IfThen(greaterExpr, addExpr));
                        }
                        blockBodies.Add(Expression.Call(fieldBuilderExpr, appendMethodInfo, setSqlExpr));
                        index++;
                    }
                }
                var fieldValueExpr = Expression.Variable(typeof(object), $"{memberMapper.MemberName.ToCamel()}Value");
                blockParameters.Add(fieldValueExpr);

                if (memberValueExpr == null)
                {
                    if (memberMapper.TypeHandler != null)
                    {
                        var typeHandlerMethodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
                        var typeHandlerExpr = Expression.Constant(memberMapper.TypeHandler);
                        memberValueExpr = Expression.Call(typeHandlerExpr, typeHandlerMethodInfo, parameterValueExpr);
                    }
                    else
                    {
                        if (memberType.ToUnderlyingType() == memberMapper.MappedTargetType)
                            memberValueExpr = parameterValueExpr;
                        else
                        {
                            var valueGetter = ormProvider.GetParameterValueGetter(memberType, memberMapper.MappedTargetType, !memberMapper.IsRequired, dbContext);
                            memberValueExpr = Expression.Invoke(Expression.Constant(valueGetter), parameterValueExpr);
                        }
                    }
                }
                blockBodies.Add(Expression.Assign(fieldValueExpr, memberValueExpr));

                Expression nativeDbTypeExpr = Expression.Constant(memberMapper.NativeDbType);
                if (nativeDbTypeExpr.Type != typeof(object))
                    nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), [typeof(string), typeof(object), typeof(object)]);
                var dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, fieldValueExpr);
                methodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add));
                blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr));
            }
            if (index <= 0) throw new Exception($"没有找到{(commandType == 1 ? "插入" : "更新")}语句");

            if (isFunc)
            {
                methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
                if (commandType == 1)
                {
                    blockBodies.Add(Expression.Call(valueBuilderExpr, appendMethodInfo, Expression.Constant(")")));
                    if (hasIdentity)
                    {
                        var keyFieldName = ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName);
                        var tailSql = ormProvider.GetIdentitySql(ormProvider.GetFieldName(keyFieldName));
                        blockBodies.Add(Expression.Call(valueBuilderExpr, appendMethodInfo, Expression.Constant(tailSql)));
                    }
                    blockBodies.Add(Expression.Call(fieldBuilderExpr, appendMethodInfo, Expression.Call(valueBuilderExpr, methodInfo)));
                }
                else blockBodies.Add(Expression.Call(fieldBuilderExpr, appendMethodInfo, Expression.Call(valueBuilderExpr, methodInfo)));

                var returnExpr = Expression.Call(fieldBuilderExpr, methodInfo);
                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));
            }

            if (isSplitSharding)
            {
                if (commandType == 1) return Expression.Lambda<Action<IDataParameterCollection, StringBuilder, StringBuilder, IDictionary<string, object>, DbContext, object>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, fieldBuilderExpr, valueBuilderExpr, shardingValuesExpr, dbContextExpr, parameterExpr).Compile();
                else return Expression.Lambda<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, DbContext, object>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, fieldBuilderExpr, shardingValuesExpr, dbContextExpr, parameterExpr).Compile();
            }
            else
            {
                if (isFunc) return Expression.Lambda<Func<IDataParameterCollection, DbContext, object, string>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, parameterExpr).Compile();
                if (commandType == 1) return Expression.Lambda<Action<IDataParameterCollection, StringBuilder, StringBuilder, DbContext, object>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, fieldBuilderExpr, valueBuilderExpr, dbContextExpr, parameterExpr).Compile();
                else return Expression.Lambda<Action<IDataParameterCollection, StringBuilder, DbContext, object>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, fieldBuilderExpr, dbContextExpr, parameterExpr).Compile();
            }
        });
    }
    public static object BuildDictBulkCommandInitializer(DbContext dbContext, Type entityType, int commandType, bool hasOnlyFields, bool hasIgnoreFields)
    {
        var cacheKey = GetCacheKey(commandType, hasOnlyFields, hasIgnoreFields);
        return dictBulkCommandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var dbContextExpr = Expression.Parameter(typeof(DbContext), "dbContext");
            var dictExpr = Expression.Parameter(typeof(IDictionary<string, object>), "dict");
            var valueSettersExpr = Expression.Parameter(typeof(List<Action<IDataParameterCollection, StringBuilder, object, string>>), "valueSetters");

            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            ParameterExpression onlyFieldsExpr = null;
            ParameterExpression ignoreFieldsExpr = null;
            ParameterExpression keySettersExpr = null;
            ParameterExpression fieldBuilderExpr = null;

            var ormProviderExpr = Expression.Variable(typeof(IOrmProvider), "ormProvider");
            var enumeratorExpr = Expression.Variable(typeof(IEnumerator<KeyValuePair<string, object>>), "enumerator");
            var itemKeyExpr = Expression.Variable(typeof(string), "itemKey");
            var itemValueExpr = Expression.Variable(typeof(object), "itemValue");
            var entityMapperExpr = Expression.Variable(typeof(EntityMap), "entityMapper");
            var memberMapperExpr = Expression.Variable(typeof(MemberMap), "memberMapper");
            var indexExpr = Expression.Variable(typeof(int), "index");
            blockParameters.AddRange([ormProviderExpr, enumeratorExpr, itemKeyExpr, itemValueExpr, entityMapperExpr, memberMapperExpr, indexExpr]);

            var hasFilterFields = hasOnlyFields || hasIgnoreFields;
            if (hasFilterFields)
            {
                onlyFieldsExpr = Expression.Parameter(typeof(List<string>), "onlyFields");
                ignoreFieldsExpr = Expression.Parameter(typeof(List<string>), "ignoreFields");
            }
            if (commandType == 1)
            {
                fieldBuilderExpr = Expression.Variable(typeof(StringBuilder), "fieldBuilder");
                blockParameters.Add(fieldBuilderExpr);
                var constructor = typeof(StringBuilder).GetConstructor([typeof(string)]);
                var newExpr = Expression.New(constructor, Expression.Constant("("));
                blockBodies.Add(Expression.Assign(fieldBuilderExpr, newExpr));
            }
            else keySettersExpr = Expression.Parameter(typeof(List<Action<object, string>>), "keySetters");

            MethodInfo methodInfo = null;
            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), [typeof(string)]);
            var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)]);
            var concat2MethodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string), typeof(string)]);
            var dictItemPropertyInfo = typeof(IDictionary<string, object>).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(string)).First();

            methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.GetEntityMap), [typeof(EntityMapProvider), typeof(Type)]);
            var entityMapProviderExpr = Expression.Property(dbContextExpr, nameof(DbContext.EntityMapProvider));
            blockBodies.Add(Expression.Assign(ormProviderExpr, Expression.Property(dbContextExpr, nameof(DbContext.OrmProvider))));
            blockBodies.Add(Expression.Assign(entityMapperExpr, Expression.Call(methodInfo, entityMapProviderExpr, Expression.Constant(entityType))));
            blockBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));

            var breakLabel = Expression.Label();
            var continueLabel = Expression.Label();

            //var enumerator = dict.GetEnumerator();
            methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
            blockBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(dictExpr, methodInfo)));

            //if(!enumerator.MoveNext())
            //  break;
            var loopBodies = new List<Expression>();
            methodInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
            var ifFalseExpr = Expression.IsFalse(Expression.Call(enumeratorExpr, methodInfo));
            loopBodies.Add(Expression.IfThen(ifFalseExpr, Expression.Break(breakLabel)));

            //var itemKey = enumerator.Current.Key;
            var currentExpr = Expression.Property(enumeratorExpr, nameof(IEnumerator.Current));
            loopBodies.Add(Expression.Assign(itemKeyExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Key))));

            //if(!entityMapper.TryGetMemberMap(itemKey, out var memberMapper)) continue;
            methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.TryGetMemberMap));
            Expression isContinueExpr = Expression.IsFalse(Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr, memberMapperExpr));
            loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

            //if(memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsRowVersion)
            isContinueExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnore));
            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsNavigation)));
            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsRowVersion)));

            //|| memberMapper.IsIgnoreInsert || memberMapper.IsAutoIncrement
            if (commandType == 1)
            {
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnoreInsert)));
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsAutoIncrement)));
            }
            //|| memberMapper.IsIgnoreUpdate
            if (commandType == 2)
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnoreUpdate)));

            Expression lowerMemberNameExpr = null;
            var toLowerMethodInfo = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes);
            if (hasOnlyFields && hasIgnoreFields)
            {
                var lowerItemValueExpr = Expression.Parameter(typeof(string), "lowerMemberName");
                blockParameters.Add(lowerItemValueExpr);
                lowerMemberNameExpr = lowerItemValueExpr;
                loopBodies.Add(Expression.Assign(lowerMemberNameExpr, Expression.Call(itemKeyExpr, toLowerMethodInfo)));
            }
            else lowerMemberNameExpr = Expression.Call(itemKeyExpr, toLowerMethodInfo);

            //|| !onlyFields.Constains(itemKey.ToLower())
            methodInfo = typeof(List<string>).GetMethod(nameof(List<string>.Contains), [typeof(string)]);
            if (hasOnlyFields)
            {
                var isFalseExpr = Expression.IsFalse(Expression.Call(methodInfo, onlyFieldsExpr, lowerMemberNameExpr));
                isContinueExpr = Expression.OrElse(isContinueExpr, isFalseExpr);
            }
            //|| ignoreFields.Constains(itemKey.ToLower()) 
            if (hasIgnoreFields)
                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Call(methodInfo, ignoreFieldsExpr, lowerMemberNameExpr));
            loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));


            //生成SQL语句部分
            //builder.Append(ormProvider.GetFieldName(memberMapper.FieldName));
            Expression fieldNameExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.FieldName));
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetFieldName));
            fieldNameExpr = Expression.Call(ormProviderExpr, methodInfo, fieldNameExpr);

            var greaterThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0));
            if (commandType == 1)
            {
                var addExpr = Expression.Call(fieldBuilderExpr, appendMethodInfo, Expression.Constant(","));
                loopBodies.Add(Expression.IfThen(greaterThenExpr, addExpr));
                loopBodies.Add(Expression.Call(fieldBuilderExpr, appendMethodInfo, fieldNameExpr));
            }

            //生成valueGetter委托
            var vfParameterExpr = Expression.Parameter(typeof(object), "f");

            var typeHandlerExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.TypeHandler));
            var notNullExpr = Expression.NotEqual(typeHandlerExpr, Expression.Constant(null));
            methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
            var typeHandleValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, vfParameterExpr);

            methodInfo = typeof(object).GetMethod(nameof(object.GetType));
            var itemValueTypeExpr = Expression.Call(vfParameterExpr, methodInfo);
            var targetTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.MappedTargetType));

            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetParameterValueGetter));
            var myValueGetterExpr = Expression.Call(ormProviderExpr, methodInfo, itemValueTypeExpr, targetTypeExpr, Expression.Constant(false), dbContextExpr);
            var myTypedValueExpr = Expression.Invoke(myValueGetterExpr, vfParameterExpr);

            var dbNullExpr = Expression.Constant(DBNull.Value);
            var isNullValueExpr = Expression.Equal(vfParameterExpr, Expression.Constant(null));
            var typedValueExpr = Expression.Condition(isNullValueExpr, dbNullExpr, myTypedValueExpr, typeof(object));
            var bodyExpr = Expression.Condition(notNullExpr, typeHandleValueExpr, typedValueExpr);
            var valueGetterExpr = Expression.Lambda<Func<object, object>>(bodyExpr, vfParameterExpr);

            //生成Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string> valueSetter委托

            var valueSetterType = typeof(Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>);
            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
            var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
            var typedParameterExpr = Expression.Parameter(typeof(IDictionary<string, object>), "typedParameter");
            var suffixExpr = Expression.Parameter(typeof(string), "suffix");
            var valueSettersParameters = new List<ParameterExpression>();
            var valueSettersBodies = new List<Expression>();
            var valueSettersBodies2 = new List<Expression>();
            valueSettersBodies2.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(",")));

            var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
            var fieldValueExpr = Expression.Variable(typeof(object), "fieldValue");
            valueSettersParameters.AddRange([parameterNameExpr, fieldValueExpr]);

            var parameterPrefixExpr = Expression.Property(ormProviderExpr, nameof(IOrmProvider.ParameterPrefix));
            var memberNameExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.MemberName));
            var myParameterNameExpr = Expression.Call(concat2MethodInfo, parameterPrefixExpr, memberNameExpr, suffixExpr);
            valueSettersBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));

            if (commandType == 1)
                valueSettersBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
            else
            {
                var setSqlExpr = Expression.Call(concat2MethodInfo, fieldNameExpr, Expression.Constant("="), parameterNameExpr);
                valueSettersBodies.Add(Expression.Call(builderExpr, appendMethodInfo, setSqlExpr));
            }

            Expression myFieldValueExpr = Expression.Property(typedParameterExpr, dictItemPropertyInfo, itemKeyExpr);
            valueSettersBodies.Add(Expression.Assign(fieldValueExpr, Expression.Invoke(valueGetterExpr, myFieldValueExpr)));

            Expression nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
            if (nativeDbTypeExpr.Type != typeof(object))
                nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), [typeof(string), typeof(object), typeof(object)]);
            var dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, fieldValueExpr);
            methodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add));
            valueSettersBodies.Add(Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr));
            valueSettersBodies2.AddRange(valueSettersBodies);

            var valueSetterExpr = Expression.Lambda(Expression.Block(valueSettersParameters, valueSettersBodies),
                dbParametersExpr, builderExpr, typedParameterExpr, suffixExpr);
            var valueSetterExpr2 = Expression.Lambda(Expression.Block(valueSettersParameters, valueSettersBodies2),
                dbParametersExpr, builderExpr, typedParameterExpr, suffixExpr);

            methodInfo = valueSettersExpr.Type.GetMethod("Add", [valueSetterType]);
            greaterThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0));
            var setValueSetterExpr = Expression.Call(valueSettersExpr, methodInfo, valueSetterExpr);
            var setValueSetterExpr2 = Expression.Call(valueSettersExpr, methodInfo, valueSetterExpr2);
            loopBodies.Add(Expression.IfThenElse(greaterThenExpr, setValueSetterExpr, setValueSetterExpr2));
            loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));

            if (commandType == 1)
            {
                methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
                var returnExpr = Expression.Call(fieldBuilderExpr, methodInfo);
                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));
            }
            if (hasFilterFields)
            {
                if (commandType == 1) return Expression.Lambda<Func<IDataParameterCollection, List<Action<IDataParameterCollection, StringBuilder, object, string>>, DbContext, List<string>, List<string>, IDictionary<string, object>, string>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, valueSettersExpr, dbContextExpr, onlyFieldsExpr, ignoreFieldsExpr, dictExpr).Compile();
                else return Expression.Lambda<Action<IDataParameterCollection, List<Action<IDataParameterCollection, StringBuilder, object, string>>, DbContext, List<string>, List<string>, IDictionary<string, object>>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, valueSettersExpr, dbContextExpr, onlyFieldsExpr, ignoreFieldsExpr, dictExpr).Compile();
            }
            else
            {
                if (commandType == 1) return Expression.Lambda<Func<IDataParameterCollection, List<Action<IDataParameterCollection, StringBuilder, object, string>>, DbContext, IDictionary<string, object>, string>>(
                   Expression.Block(blockParameters, blockBodies), dbParametersExpr, valueSettersExpr, dbContextExpr, dictExpr).Compile();
                else return Expression.Lambda<Action<IDataParameterCollection, List<Action<IDataParameterCollection, StringBuilder, object, string>>, DbContext, IDictionary<string, object>>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, valueSettersExpr, dbContextExpr, dictExpr).Compile();
            }
        });
    }
    public static object BuildTypedBulkCommandInitializer(DbContext dbContext, Type entityType, Type parameterType, int commandType, List<string> onlyFields, List<string> ignoreFields)
    {
        var hasOnlyFields = onlyFields != null && onlyFields.Count > 0;
        var hasIgnoreFields = ignoreFields != null && ignoreFields.Count > 0;
        var onlyFieldsKey = hasOnlyFields ? string.Join("-", onlyFields) : "";
        var ignoreFieldsKey = hasIgnoreFields ? string.Join("-", ignoreFields) : "";
        var hasFilterFields = hasOnlyFields || hasIgnoreFields;
        var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.EntityMapProvider, entityType, parameterType, onlyFieldsKey, ignoreFieldsKey);
        var commandInitializerCache = commandType == 1 ? createWithCommandInitializerCache : updateWithCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
            var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
            var dbContextExpr = Expression.Parameter(typeof(DbContext), "dbContext");
            var parameterExpr = Expression.Parameter(typeof(object), "parameter");
            var suffixExpr = Expression.Parameter(typeof(string), "suffix");
            var headSqlExpr = Expression.Parameter(typeof(string), "headSql");
            var tailSqlExpr = Expression.Parameter(typeof(string), "tailSql");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            var typedParameterExpr = Expression.Variable(parameterType, "typedParameterObj");
            var ormProviderExpr = Expression.Variable(typeof(IOrmProvider), "ormProvider");
            blockParameters.AddRange([typedParameterExpr, ormProviderExpr]);
            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
            blockBodies.Add(Expression.Assign(ormProviderExpr, Expression.Property(dbContextExpr, nameof(DbContext.OrmProvider))));

            var targetMemberInfos = new List<MemberInfo>();
            var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property || f.MemberType == MemberTypes.Field).ToList();
            var ormProvider = dbContext.OrmProvider;
            var entityMapper = dbContext.EntityMapProvider.GetEntityMap(entityType);
            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), [typeof(string)]);
            var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)]);

            foreach (var memberMapper in entityMapper.MemberMaps)
            {
                var memberInfo = memberInfos.Find(f => string.Equals(f.Name, memberMapper.MemberName, StringComparison.OrdinalIgnoreCase));
                if (memberInfo == null) continue;
                if (hasOnlyFields && !onlyFields.Contains(memberMapper.MemberName))
                    continue;
                if (hasIgnoreFields && ignoreFields.Contains(memberMapper.MemberName))
                    continue;
                targetMemberInfos.Add(memberInfo);
            }
            string headSql = null;
            if (commandType == 1)
            {
                int index = 0;
                var builder = new StringBuilder();
                foreach (var memberInfo in targetMemberInfos)
                {
                    var memberMapper = entityMapper.GetMemberMap(memberInfo.Name);
                    if (index > 0) builder.Append(',');
                    builder.Append(ormProvider.GetFieldName(memberMapper.FieldName));
                    index++;
                }
                headSql = builder.ToString();
            }

            AddTypedCommandInitializer(dbContext, entityType, parameterType, commandType, targetMemberInfos,
                dbContextExpr, dbParametersExpr, builderExpr, ormProviderExpr, typedParameterExpr, suffixExpr, blockParameters, blockBodies);
            blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, tailSqlExpr));

            if (commandType == 1) return (headSql, Expression.Lambda<Action<IDataParameterCollection, StringBuilder, DbContext, string, string, object, string>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, dbContextExpr, headSqlExpr, tailSqlExpr, parameterExpr, suffixExpr).Compile());
            else return Expression.Lambda<Action<IDataParameterCollection, StringBuilder, DbContext, string, string, string, object, string>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, dbContextExpr, headSqlExpr, tailSqlExpr, parameterExpr, suffixExpr).Compile();
        });
    }
    public static void AddTypedCommandInitializer(DbContext dbContext, Type entityType, Type parameterType, int commandType, List<MemberInfo> targetMemberInfos, Expression dbContextExpr,
        Expression dbParametersExpr, Expression builderExpr, Expression ormProviderExpr, Expression typedParameterExpr, Expression suffixExpr, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        var ormProvider = dbContext.OrmProvider;
        var entityMapper = dbContext.EntityMapProvider.GetEntityMap(entityType);
        ParameterExpression whereExpr = null;
        MethodInfo methodInfo = null;
        var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), [typeof(string)]);
        var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)]);

        if (commandType == 2)
        {
            whereExpr = Expression.Variable(typeof(StringBuilder), "whereBuilder");
            blockParameters.Add(whereExpr);
            var constructor = typeof(StringBuilder).GetConstructor([typeof(string)]);
            var newExpr = Expression.New(constructor, Expression.Constant(" WHERE "));
            blockBodies.Add(Expression.Assign(whereExpr, newExpr));
        }

        var index = 0;
        foreach (var memberMapper in entityMapper.MemberMaps)
        {
            if (!targetMemberInfos.TryFind(memberMapper.MemberName, out var memberInfo))
                continue;

            if (memberMapper.IsAutoIncrement || memberMapper.IsIgnore
                || memberMapper.IsNavigation || memberMapper.IsRowVersion
                || (commandType == 1 && memberMapper.IsIgnoreInsert)
                || (commandType == 2 && memberMapper.IsIgnoreUpdate))
                continue;

            var parameterName = ormProvider.ParameterPrefix + memberMapper.MemberName;
            var parameterNameExpr = Expression.Variable(typeof(string), $"parameterName{index}");
            blockParameters.Add(parameterNameExpr);
            var myParameterNameExpr = Expression.Call(concatMethodInfo, Expression.Constant(parameterName), suffixExpr);
            blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));

            if (commandType == 1)
            {
                if (index > 0)
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(",")));
                var fieldNameExpr = Expression.Constant(ormProvider.GetFieldName(memberMapper.FieldName));
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
            }
            else
            {
                var setPartExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}=");
                if (memberMapper.IsKey)
                {
                    if (index > 0) blockBodies.Add(Expression.Call(whereExpr, appendMethodInfo, Expression.Constant(" AND ")));
                    blockBodies.Add(Expression.Call(whereExpr, appendMethodInfo, setPartExpr));
                }
                else
                {
                    if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(",")));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, setPartExpr));
                }
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
            }

            var fieldValueExpr = Expression.Variable(typeof(object), $"{memberMapper.MemberName.ToCamel()}Value");
            blockParameters.Add(fieldValueExpr);

            Expression memberValueExpr = null;
            var parameterValueExpr = Expression.PropertyOrField(typedParameterExpr, memberInfo.Name);
            if (memberMapper.TypeHandler != null)
            {
                var typeHandlerMethodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.ToFieldValue));
                var typeHandlerExpr = Expression.Constant(memberMapper.TypeHandler);
                memberValueExpr = Expression.Call(typeHandlerExpr, typeHandlerMethodInfo, parameterValueExpr);
            }
            else
            {
                var memberType = memberInfo.GetMemberType();
                if (memberType.ToUnderlyingType() != memberMapper.MappedTargetType)
                {
                    var valueGetter = ormProvider.GetParameterValueGetter(memberType, memberMapper.MappedTargetType, !memberMapper.IsRequired, dbContext);
                    memberValueExpr = Expression.Invoke(Expression.Constant(valueGetter), parameterValueExpr);
                }
                else memberValueExpr = parameterValueExpr;
            }
            blockBodies.Add(Expression.Assign(fieldValueExpr, memberValueExpr));

            Expression nativeDbTypeExpr = Expression.Constant(memberMapper.NativeDbType);
            if (nativeDbTypeExpr.Type != typeof(object))
                nativeDbTypeExpr = Expression.Convert(nativeDbTypeExpr, typeof(object));
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), [typeof(string), typeof(object), typeof(object)]);
            var dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, fieldValueExpr);
            methodInfo = typeof(IList).GetMethod(nameof(IDataParameterCollection.Add));
            blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr));
            index++;
        }
        if (index <= 0) throw new Exception($"没有找到{(commandType == 1 ? "插入" : "更新")}语句");

        if (commandType == 2)
        {
            methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
            blockBodies.Add(Expression.Call(builderExpr, methodInfo, Expression.Call(whereExpr, methodInfo)));
        }
    }



    //private static void AddFieldsSqlParameters(ParameterExpression dbParametersExpr, ParameterExpression builderExpr, ParameterExpression dbContextExpr, ParameterExpression whereObjExpr, Expression headSqlExpr, DbContext dbContext,
    //    Type entityType, Type parametersType, int commandType, int sqlType, int keyType, bool isFunc, bool isBulk, bool isUpdateRowVersion, bool hasOnlyFields, bool hasIgnoreFields, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    //{
    //    //commandType 1:Insert, 3:Insert Update Set 4:Update Set
    //    //sqlType 0:None 1:Sql And Parameters 2:Only Sql 3:Only Parameters
    //    //keyType 0:None 1:Use Keys 2:Ignore Keys     

    //    ParameterExpression suffixExpr = null;
    //    ParameterExpression ormProviderExpr = null;
    //    ParameterExpression parameterNameExpr = null;
    //    ParameterExpression typedParametersExpr = null;

    //    bool isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(parametersType);
    //    var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), [typeof(string)]);
    //    var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)]);
    //    MethodInfo methodInfo = null;

    //    if (commandType > 1 && sqlType != 2)
    //        dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");

    //    if (isDictionary || commandType > 1 && sqlType != 2)
    //    {
    //        if (isDictionary) parametersType = typeof(IDictionary<string, object>);
    //        ormProviderExpr = Expression.Variable(typeof(IOrmProvider), "ormProvider");
    //        typedParametersExpr = Expression.Variable(parametersType, isDictionary ? "dict" : "typedParameters");
    //        blockParameters.AddRange([ormProviderExpr, typedParametersExpr]);
    //        blockBodies.Add(Expression.Assign(ormProviderExpr, Expression.Property(dbContextExpr, nameof(DbContext.OrmProvider))));
    //        blockBodies.Add(Expression.Assign(typedParametersExpr, Expression.Convert(parametersExpr, parametersType)));
    //    }
    //    if (isFunc)
    //    {
    //        builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
    //        blockParameters.Add(builderExpr);
    //        var constructorInfo = typeof(StringBuilder).GetConstructor(Type.EmptyTypes);
    //        blockBodies.Add(Expression.Assign(builderExpr, Expression.New(constructorInfo)));
    //    }
    //    else if (sqlType != 3) builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
    //    if (commandType > 1 && isBulk)
    //    {
    //        suffixExpr = Expression.Parameter(typeof(string), "suffix");
    //        parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
    //        blockParameters.Add(parameterNameExpr);
    //    }

    //    if (sqlType != 3 && !string.IsNullOrEmpty(headSql))
    //        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(headSql)));

    //    ParameterExpression entityMapperExpr = null;
    //    ParameterExpression memberMapperExpr = null;
    //    MethodInfo containsKeyMethodInfo = null;
    //    PropertyInfo dictItemPropertyInfo = null;
    //    var ormProvider = dbContext.OrmProvider;

    //    if (isDictionary)
    //    {
    //        entityMapperExpr = Expression.Variable(typeof(EntityMap), "entityMapper");
    //        memberMapperExpr = Expression.Variable(typeof(MemberMap), "memberMapper");
    //        blockParameters.AddRange([entityMapperExpr, memberMapperExpr]);

    //        containsKeyMethodInfo = typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.ContainsKey));
    //        dictItemPropertyInfo = typeof(IDictionary<string, object>).GetProperties(BindingFlags.Instance | BindingFlags.Public)
    //            .Where(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(string)).First();

    //        var mapProviderExpr = Expression.Property(dbContextExpr, nameof(DbContext.MapProvider));
    //        methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.GetEntityMap), [typeof(EntityMapProvider), typeof(Type)]);
    //        blockBodies.Add(Expression.Assign(entityMapperExpr, Expression.Call(methodInfo, mapProviderExpr, Expression.Constant(entityType))));
    //    }

    //    if (isDictionary && keyType != 1)
    //    {
    //        var indexExpr = Expression.Variable(typeof(int), "index");
    //        var enumeratorExpr = Expression.Variable(typeof(IEnumerator<KeyValuePair<string, object>>), "enumerator");
    //        var itemKeyExpr = Expression.Variable(typeof(string), "itemKey");
    //        var itemValueExpr = Expression.Variable(typeof(object), "itemValue");
    //        var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string), typeof(string)]);

    //        blockParameters.AddRange([indexExpr, enumeratorExpr, itemKeyExpr, itemValueExpr]);
    //        var breakLabel = Expression.Label();
    //        var continueLabel = Expression.Label();

    //        //var index = 0;
    //        //var enumerator = dict.GetEnumerator();
    //        blockBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
    //        methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
    //        blockBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(typedParametersExpr, methodInfo)));

    //        //if(!enumerator.MoveNext())
    //        //  break;
    //        var loopBodies = new List<Expression>();
    //        methodInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
    //        var ifFalseExpr = Expression.IsFalse(Expression.Call(enumeratorExpr, methodInfo));
    //        loopBodies.Add(Expression.IfThen(ifFalseExpr, Expression.Break(breakLabel)));

    //        //var itemKey = enumerator.Current.Key;
    //        //var fieldValue = enumerator.Current.Value;          
    //        var currentExpr = Expression.Property(enumeratorExpr, nameof(IEnumerator.Current));
    //        loopBodies.Add(Expression.Assign(itemKeyExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Key))));

    //        //if(!entityMapper.ContainsMemberMap(itemKey)) continue;
    //        methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.ContainsMemberMap));
    //        Expression isContinueExpr = Expression.IsFalse(Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr));
    //        loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

    //        //var memberMapper = entityMapper.GetMemberMap(itemKey);
    //        methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.GetMemberMap));
    //        loopBodies.Add(Expression.Assign(memberMapperExpr, Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr)));
    //        //|| memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsKey
    //        isContinueExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnore));
    //        isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsNavigation)));
    //        if (keyType == 2)
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsKey)));

    //        //|| memberMapper.IsIgnoreInsert || memberMapper.IsAutoIncrement
    //        if (commandType < 3)
    //        {
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnoreInsert)));
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsAutoIncrement)));
    //        }
    //        //|| memberMapper.IsIgnoreUpdate
    //        if (commandType > 2)
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnoreUpdate)));
    //        //|| memberMapper.IsRowVersion
    //        if (!isUpdateRowVersion)
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsRowVersion)));

    //        var lowerItemKeyExpr = Expression.Call(itemKeyExpr, typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes));
    //        //|| !onlyFields.Constains(itemKey.ToLower())
    //        if (onlyFieldNames != null)
    //        {
    //            var initExprs = onlyFieldNames.Select(f => Expression.Constant(f, typeof(string)));
    //            var onlyFieldsExpr = Expression.NewArrayInit(typeof(string), initExprs);
    //            methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), [typeof(string)]);
    //            var isFalseExpr = Expression.IsFalse(Expression.Call(methodInfo, onlyFieldsExpr, lowerItemKeyExpr));
    //            isContinueExpr = Expression.OrElse(isContinueExpr, isFalseExpr);
    //        }
    //        //|| ignoreFields.Constains(itemKey.ToLower()) 
    //        if (ignoreFieldNames != null)
    //        {
    //            var initExprs = ignoreFieldNames.Select(f => Expression.Constant(f, typeof(string)));
    //            var ignoreFieldsExpr = Expression.NewArrayInit(typeof(string), initExprs);
    //            methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), [typeof(string)]);
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Call(methodInfo, ignoreFieldsExpr, lowerItemKeyExpr));
    //        }
    //        loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

    //        //var parameterName = ormProvider.ParameterPrefix + memberMapper.MemberName + suffix;
    //        Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix);
    //        if (commandType > 1)
    //        {
    //            var memberNameExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.MemberName));
    //            if (isBulk)
    //            {
    //                myParameterNameExpr = Expression.Call(concatMethodInfo2, myParameterNameExpr, memberNameExpr, suffixExpr);
    //                loopBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
    //                myParameterNameExpr = parameterNameExpr;
    //            }
    //            else myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, memberNameExpr);
    //        }
    //        //生成SQL
    //        if (sqlType < 3)
    //        {
    //            //if(index > 0) builder.Append(",");
    //            var greaterThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0));
    //            var callExpr = Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(","));
    //            loopBodies.Add(Expression.IfThen(greaterThenExpr, callExpr));

    //            //builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)");
    //            //builder.Append(parameterName);
    //            //builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");

    //            Expression contentExpr = null;
    //            Expression fieldNameExpr = null;

    //            if (commandType == 2) contentExpr = myParameterNameExpr;
    //            else
    //            {
    //                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetFieldName));
    //                fieldNameExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.FieldName));
    //                fieldNameExpr = Expression.Call(ormProviderExpr, methodInfo, fieldNameExpr);

    //                if (commandType == 1) contentExpr = fieldNameExpr;
    //                else contentExpr = Expression.Call(concatMethodInfo2, fieldNameExpr, Expression.Constant("="), myParameterNameExpr);
    //            }
    //            loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, contentExpr));
    //        }
    //        //生成参数
    //        if (commandType > 1 && sqlType != 2)
    //        {
    //            loopBodies.Add(Expression.Assign(itemValueExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Value))));
    //            AddValueParameter(dbContext, dbContextExpr, dbParametersExpr, ormProviderExpr, myParameterNameExpr, itemValueExpr, memberMapperExpr, loopBodies);
    //        }

    //        //index++;
    //        loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));
    //        blockBodies.Add(Expression.Loop(Expression.Block(loopBodies), breakLabel, continueLabel));
    //    }
    //    else
    //    {
    //        var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);
    //        var filterMemberMaps = keyType == 1 ? entityMapper.KeyMembers : entityMapper.MemberMaps;
    //        Dictionary<string, MemberInfo> targetMemberInfos = null;

    //        if (!isDictionary)
    //        {
    //            targetMemberInfos = parametersType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
    //                .Where(f => f.MemberType == MemberTypes.Property || f.MemberType == MemberTypes.Field)
    //                .ToDictionary(f => f.Name.ToLower(), f => f);
    //        }
    //        var index = 0;
    //        foreach (var memberMapper in filterMemberMaps)
    //        {
    //            ParameterExpression valueTupleExpr = null;
    //            MemberInfo targetMemberInfo = null;
    //            var lowerMemberName = memberMapper.MemberName.ToLower();
    //            if (keyType == 1 && isDictionary)
    //            {
    //                //var tuple = dict.ContainsLowerKey(targetMemberInfo.Name.ToLower());
    //                //if(!tuple.Item1)
    //                //  throw new KeyNotFoundException($"字典参数中{parametersType.FullName}缺少Key:{memberMapper.MemberName}的成员");
    //                valueTupleExpr = Expression.Variable(typeof(ValueTuple<bool, object>), $"{memberMapper.MemberName.ToCamel()}Tuple");
    //                blockParameters.Add(valueTupleExpr);
    //                var lowerMemberNameExpr = Expression.Constant(lowerMemberName);
    //                methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.TryGetValueIgnoreCase));
    //                var containsLowerKeyExpr = Expression.Call(methodInfo, typedParametersExpr, lowerMemberNameExpr);
    //                blockBodies.Add(Expression.Assign(valueTupleExpr, containsLowerKeyExpr));
    //                var exception = new KeyNotFoundException($"字典参数中{parametersType.FullName}缺少Key:{memberMapper.MemberName}的成员");
    //                var isContainsKeyExpr = Expression.Field(valueTupleExpr, "Item1");
    //                blockBodies.Add(Expression.IfThen(Expression.IsFalse(isContainsKeyExpr), Expression.Throw(Expression.Constant(exception))));
    //            }
    //            //忽略大小写
    //            else if (!targetMemberInfos.TryGetValue(lowerMemberName, out targetMemberInfo))
    //            {
    //                if (keyType == 1) throw new KeyNotFoundException($"参数类型{parametersType.FullName}缺少{memberMapper.MemberName}的成员");
    //                else continue;
    //            }

    //            if (memberMapper.IsIgnore || memberMapper.IsNavigation || (keyType == 2 && memberMapper.IsKey))
    //                continue;
    //            if (onlyFieldNames != null && !onlyFieldNames.Contains(lowerMemberName))
    //                continue;
    //            if (ignoreFieldNames != null && ignoreFieldNames.Contains(lowerMemberName))
    //                continue;
    //            if (!isUpdateRowVersion && memberMapper.IsRowVersion)
    //                continue;
    //            //Insert
    //            if (commandType < 3 && (memberMapper.IsIgnoreInsert || memberMapper.IsAutoIncrement))
    //                continue;
    //            //Update
    //            if (commandType > 2 && memberMapper.IsIgnoreUpdate)
    //                continue;

    //            var parameterName = ormProvider.ParameterPrefix + (commandType == 3 ? "p" : "") + memberMapper.MemberName;
    //            Expression myParameterNameExpr = Expression.Constant(parameterName);
    //            if (commandType > 1 && isBulk)
    //            {
    //                myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, suffixExpr);
    //                blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
    //                myParameterNameExpr = parameterNameExpr;
    //            }
    //            //生成SQL
    //            if (sqlType != 3)
    //            {
    //                if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(",")));
    //                Expression contentExpr = null;
    //                switch (commandType)
    //                {
    //                    case 1: contentExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}"); break;
    //                    case 2: contentExpr = myParameterNameExpr; break;
    //                    case 3:
    //                    case 4:
    //                        contentExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}=");
    //                        contentExpr = Expression.Call(concatMethodInfo, contentExpr, myParameterNameExpr);
    //                        break;
    //                }
    //                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, contentExpr));
    //            }
    //            //生成参数
    //            if (commandType > 1 && sqlType != 2)
    //            {
    //                if (isDictionary)
    //                {
    //                    var fieldValueExpr = Expression.Field(valueTupleExpr, "Item2");
    //                    AddValueParameter(dbContext, dbContextExpr, dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueExpr, memberMapperExpr, blockBodies);
    //                }
    //                else
    //                {
    //                    var fieldValueType = targetMemberInfo.GetMemberType();
    //                    Expression fieldValueExpr = Expression.PropertyOrField(typedParametersExpr, memberMapper.MemberName);
    //                    AddValueParameter(dbContext, dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueType, fieldValueExpr, memberMapper, blockBodies);
    //                }
    //            }
    //            index++;
    //        }
    //        if (index <= 0)
    //            throw new Exception($"没有找到{(commandType == 4 ? "更新" : "插入")}语句");
    //    }

    //    if (sqlType != 3 && !string.IsNullOrEmpty(tailSql))
    //        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(tailSql)));

    //    if (isFunc)
    //    {
    //        methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
    //        var returnExpr = Expression.Call(builderExpr, methodInfo);
    //        var resultLabelExpr = Expression.Label(typeof(string));
    //        blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
    //        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));

    //        if (commandType == 1) return Expression.Lambda<Func<DbContext, object, string>>(
    //            Expression.Block(blockParameters, blockBodies), dbContextExpr, parametersExpr).Compile();
    //        else
    //        {
    //            if (sqlType == 2)
    //            {
    //                if (isBulk) return Expression.Lambda<Func<DbContext, object, string, string>>(
    //                    Expression.Block(blockParameters, blockBodies), dbContextExpr, parametersExpr, suffixExpr).Compile();
    //                else return Expression.Lambda<Func<DbContext, object, string>>(
    //                    Expression.Block(blockParameters, blockBodies), dbContextExpr, parametersExpr).Compile();
    //            }
    //            else
    //            {
    //                if (isBulk) return Expression.Lambda<Func<IDataParameterCollection, DbContext, object, string, string>>(
    //                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, parametersExpr, suffixExpr).Compile();
    //                else return Expression.Lambda<Func<IDataParameterCollection, DbContext, object, string>>(
    //                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, parametersExpr).Compile();
    //            }
    //        }
    //    }
    //    else
    //    {
    //        if (commandType == 1) return Expression.Lambda<Action<StringBuilder, DbContext, object>>(
    //            Expression.Block(blockParameters, blockBodies), builderExpr, dbContextExpr, parametersExpr).Compile();
    //        else
    //        {
    //            switch (sqlType)
    //            {
    //                case 1:
    //                    if (isBulk) return Expression.Lambda<Action<IDataParameterCollection, StringBuilder, DbContext, object, string>>(
    //                        Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, dbContextExpr, parametersExpr, suffixExpr).Compile();
    //                    else return Expression.Lambda<Action<IDataParameterCollection, StringBuilder, DbContext, object>>(
    //                        Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, dbContextExpr, parametersExpr).Compile();
    //                case 2:
    //                    if (isBulk) return Expression.Lambda<Action<StringBuilder, DbContext, object, string>>(
    //                        Expression.Block(blockParameters, blockBodies), builderExpr, dbContextExpr, parametersExpr, suffixExpr).Compile();
    //                    else return Expression.Lambda<Action<StringBuilder, DbContext, object>>(
    //                        Expression.Block(blockParameters, blockBodies), builderExpr, dbContextExpr, parametersExpr).Compile();
    //                case 3:
    //                    if (isBulk) return Expression.Lambda<Action<IDataParameterCollection, DbContext, object, string>>(
    //                        Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, parametersExpr, suffixExpr).Compile();
    //                    else return Expression.Lambda<Action<IDataParameterCollection, DbContext, object>>(
    //                        Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, parametersExpr).Compile();
    //                default: throw new NotSupportedException("不支持的场景");
    //            }
    //        }
    //    }
    //}

    //public static Action<IDataParameterCollection, StringBuilder, StringBuilder, DbContext, IOrmProvider, EntityMap, List<string>, List<string>, IDictionary<string, object>>
    //    BuildCreateDictCommandInitializer(bool hasOnlyFields, bool hasIgnoreFields)
    //{
    //    var cacheKey = GetCacheKey(hasOnlyFields, hasIgnoreFields);
    //    return createDictCommandInitializerCache.GetOrAdd(cacheKey, _ =>
    //    {
    //        Action<IDataParameterCollection, StringBuilder, StringBuilder, DbContext, IOrmProvider, EntityMap, List<string>, List<string>, IDictionary<string, object>> commandInitializer = null;
    //        if (hasOnlyFields && hasIgnoreFields) commandInitializer = (dbParameters, fieldBuilder, valueBuilder, dbContext, ormProvider, entityMapper, lowerOnlyFields, lowerIgnoreFields, dict) =>
    //        {
    //            int index = 0;
    //            foreach (var itemKey in dict.Keys)
    //            {
    //                if (!entityMapper.TryGetMemberMap(itemKey, out var memberMapper))
    //                    continue;
    //                if (memberMapper.IsKey || memberMapper.IsAutoIncrement || memberMapper.IsIgnore
    //                    || memberMapper.IsIgnoreInsert || memberMapper.IsNavigation || memberMapper.IsRowVersion)
    //                    continue;
    //                if (!lowerOnlyFields.Contains(itemKey.ToLower())) continue;
    //                if (lowerIgnoreFields.Contains(itemKey.ToLower())) continue;

    //                if (index > 0) fieldBuilder.Append(',');
    //                fieldBuilder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}");

    //                object itemValue = null;
    //                if (memberMapper.TypeHandler != null)
    //                    itemValue = memberMapper.TypeHandler.ToFieldValue(dict[itemKey]);
    //                else
    //                {
    //                    var targetType = ormProvider.MapDefaultType(memberMapper);
    //                    itemValue = dict[itemKey];
    //                    var itemValueType = itemValue.GetType();
    //                    itemValue = ormProvider.GetParameterValueGetter(itemValueType, targetType, !memberMapper.IsRequired, dbContext);
    //                }
    //                if (index > 0) valueBuilder.Append(',');
    //                var parameterName = $"{ormProvider.ParameterPrefix}{memberMapper.MemberName}";
    //                valueBuilder.Append(parameterName);
    //                dbParameters.Add(ormProvider.CreateParameter(parameterName, itemValue));
    //                index++;
    //            }
    //        };
    //        else if (hasOnlyFields && !hasIgnoreFields) commandInitializer = (dbParameters, fieldBuilder, valueBuilder, dbContext, ormProvider, entityMapper, lowerOnlyFields, lowerIgnoreFields, dict) =>
    //        {
    //            int index = 0;
    //            foreach (var itemKey in dict.Keys)
    //            {
    //                if (!entityMapper.TryGetMemberMap(itemKey, out var memberMapper))
    //                    continue;
    //                if (memberMapper.IsKey || memberMapper.IsAutoIncrement || memberMapper.IsIgnore
    //                    || memberMapper.IsIgnoreInsert || memberMapper.IsNavigation || memberMapper.IsRowVersion)
    //                    continue;
    //                if (!lowerOnlyFields.Contains(itemKey.ToLower())) continue;

    //                if (index > 0) fieldBuilder.Append(',');
    //                fieldBuilder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}");

    //                object itemValue = null;
    //                if (memberMapper.TypeHandler != null)
    //                    itemValue = memberMapper.TypeHandler.ToFieldValue(dict[itemKey]);
    //                else
    //                {
    //                    var targetType = ormProvider.MapDefaultType(memberMapper);
    //                    itemValue = dict[itemKey];
    //                    var itemValueType = itemValue.GetType();
    //                    itemValue = ormProvider.GetParameterValueGetter(itemValueType, targetType, !memberMapper.IsRequired, dbContext);
    //                }
    //                if (index > 0) valueBuilder.Append(',');
    //                var parameterName = $"{ormProvider.ParameterPrefix}{memberMapper.MemberName}";
    //                valueBuilder.Append(parameterName);
    //                dbParameters.Add(ormProvider.CreateParameter(parameterName, itemValue));
    //                index++;
    //            }
    //        };
    //        else if (!hasOnlyFields && hasIgnoreFields) commandInitializer = (dbParameters, fieldBuilder, valueBuilder, dbContext, ormProvider, entityMapper, lowerOnlyFields, lowerIgnoreFields, dict) =>
    //        {
    //            int index = 0;
    //            foreach (var itemKey in dict.Keys)
    //            {
    //                if (!entityMapper.TryGetMemberMap(itemKey, out var memberMapper))
    //                    continue;
    //                if (memberMapper.IsKey || memberMapper.IsAutoIncrement || memberMapper.IsIgnore
    //                    || memberMapper.IsIgnoreInsert || memberMapper.IsNavigation || memberMapper.IsRowVersion)
    //                    continue;
    //                if (lowerIgnoreFields.Contains(itemKey.ToLower())) continue;

    //                if (index > 0) fieldBuilder.Append(',');
    //                fieldBuilder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}");

    //                object itemValue = null;
    //                if (memberMapper.TypeHandler != null)
    //                    itemValue = memberMapper.TypeHandler.ToFieldValue(dict[itemKey]);
    //                else
    //                {
    //                    var targetType = ormProvider.MapDefaultType(memberMapper);
    //                    itemValue = dict[itemKey];
    //                    var itemValueType = itemValue.GetType();
    //                    itemValue = ormProvider.GetParameterValueGetter(itemValueType, targetType, !memberMapper.IsRequired, dbContext);
    //                }
    //                if (index > 0) valueBuilder.Append(',');
    //                var parameterName = $"{ormProvider.ParameterPrefix}{memberMapper.MemberName}";
    //                valueBuilder.Append(parameterName);
    //                dbParameters.Add(ormProvider.CreateParameter(parameterName, itemValue));
    //                index++;
    //            }
    //        };
    //        else commandInitializer = (dbParameters, fieldBuilder, valueBuilder, dbContext, ormProvider, entityMapper, lowerOnlyFields, lowerIgnoreFields, dict) =>
    //        {
    //            int index = 0;
    //            foreach (var itemKey in dict.Keys)
    //            {
    //                if (!entityMapper.TryGetMemberMap(itemKey, out var memberMapper))
    //                    continue;
    //                if (memberMapper.IsKey || memberMapper.IsAutoIncrement || memberMapper.IsIgnore
    //                    || memberMapper.IsIgnoreInsert || memberMapper.IsNavigation || memberMapper.IsRowVersion)
    //                    continue;

    //                if (index > 0) fieldBuilder.Append(',');
    //                fieldBuilder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}");

    //                object itemValue = null;
    //                if (memberMapper.TypeHandler != null)
    //                    itemValue = memberMapper.TypeHandler.ToFieldValue(dict[itemKey]);
    //                else
    //                {
    //                    var targetType = ormProvider.MapDefaultType(memberMapper);
    //                    itemValue = dict[itemKey];
    //                    var itemValueType = itemValue.GetType();
    //                    itemValue = ormProvider.GetParameterValueGetter(itemValueType, targetType, !memberMapper.IsRequired, dbContext);
    //                }
    //                if (index > 0) valueBuilder.Append(',');
    //                var parameterName = $"{ormProvider.ParameterPrefix}{memberMapper.MemberName}";
    //                valueBuilder.Append(parameterName);
    //                dbParameters.Add(ormProvider.CreateParameter(parameterName, itemValue));
    //                index++;
    //            }
    //        };
    //        return commandInitializer;
    //    });
    //}
    //public static Action<IDataParameterCollection, StringBuilder, DbContext, Type, List<string>, List<string>, object>
    //    BuildCreateBulkDictCommandInitializer(bool hasOnlyFields, bool hasIgnoreFields)
    //{
    //    var cacheKey = GetCacheKey(hasOnlyFields, hasIgnoreFields);
    //    Action<IDataParameterCollection, StringBuilder, IOrmProvider, List<(string, string, Func<object, object>)>, string, IEnumerable> loopCommandInitializer = null;
    //    loopCommandInitializer = (dbParameters, builder, ormProvider, valueFields, fixedSql, insertObjs) =>
    //    {
    //        int iLoopIndex = 0;
    //        builder.Append(fixedSql);
    //        foreach (var insertObj in insertObjs)
    //        {
    //            if (iLoopIndex > 0) builder.Append(',');

    //            var index = 0;
    //            builder.Append('(');
    //            foreach ((var itemKey, var parameterName, var valueGetter) in valueFields)
    //            {
    //                if (index > 0) builder.Append(',');
    //                var myParameterName = parameterName + index.ToString();
    //                builder.Append(myParameterName);
    //                var typedInsertObj = insertObj as IDictionary<string, object>;
    //                var itemValue = valueGetter.Invoke(typedInsertObj[itemKey]);
    //                dbParameters.Add(ormProvider.CreateParameter(myParameterName, itemValue));
    //                index++;
    //            }
    //            builder.Append(')');
    //            iLoopIndex++;
    //        }
    //    };
    //    return createDictBulkCommandInitializerCache.GetOrAdd(cacheKey, _ =>
    //    {
    //        Action<IDataParameterCollection, StringBuilder, DbContext, Type, List<string>, List<string>, object> commandInitializer = null;
    //        if (hasOnlyFields && hasIgnoreFields) commandInitializer = (dbParameters, builder, dbContext, entityType, lowerOnlyFields, lowerIgnoreFields, parameters) =>
    //        {
    //            int index = 0;
    //            var ormProvider = dbContext.OrmProvider;
    //            var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);
    //            var valueFields = new List<(string, string, Func<object, object>)>();
    //            var insertObjs = parameters as IEnumerable;
    //            IDictionary<string, object> dict = null;
    //            foreach (var insertObj in insertObjs)
    //            {
    //                dict = insertObj as IDictionary<string, object>;
    //                break;
    //            }
    //            foreach (var itemKey in dict.Keys)
    //            {
    //                if (!entityMapper.TryGetMemberMap(itemKey, out var memberMapper))
    //                    continue;
    //                if (memberMapper.IsKey || memberMapper.IsAutoIncrement || memberMapper.IsIgnore
    //                    || memberMapper.IsIgnoreInsert || memberMapper.IsNavigation || memberMapper.IsRowVersion)
    //                    continue;
    //                if (!lowerOnlyFields.Contains(itemKey.ToLower())) continue;
    //                if (lowerIgnoreFields.Contains(itemKey.ToLower())) continue;

    //                if (index > 0) builder.Append(',');
    //                builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}");

    //                Func<object, object> valueGetter = null;
    //                if (memberMapper.TypeHandler != null)
    //                    valueGetter = itemValue => memberMapper.TypeHandler.ToFieldValue(itemValue);
    //                else
    //                {
    //                    object itemValue = null;
    //                    var targetType = ormProvider.MapDefaultType(memberMapper);
    //                    itemValue = dict[itemKey];
    //                    var itemValueType = itemValue.GetType();
    //                    valueGetter = ormProvider.GetParameterValueGetter(itemValueType, targetType, !memberMapper.IsRequired, dbContext);
    //                }
    //                valueFields.Add((itemKey, $"{ormProvider.ParameterPrefix}{memberMapper.MemberName}", valueGetter));
    //                index++;
    //            }
    //            builder.Append(") VALUES ");
    //            var fixedSql = builder.ToString();
    //            builder.Clear();
    //            loopCommandInitializer.Invoke(dbParameters, builder, ormProvider, valueFields, fixedSql, insertObjs);
    //        };
    //        else if (hasOnlyFields && !hasIgnoreFields) commandInitializer = (dbParameters, builder, dbContext, entityType, lowerOnlyFields, lowerIgnoreFields, parameters) =>
    //        {
    //            int index = 0;
    //            var ormProvider = dbContext.OrmProvider;
    //            var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);
    //            var valueFields = new List<(string, string, Func<object, object>)>();
    //            var insertObjs = parameters as IEnumerable;
    //            IDictionary<string, object> dict = null;
    //            foreach (var insertObj in insertObjs)
    //            {
    //                dict = insertObj as IDictionary<string, object>;
    //                break;
    //            }
    //            foreach (var itemKey in dict.Keys)
    //            {
    //                if (!entityMapper.TryGetMemberMap(itemKey, out var memberMapper))
    //                    continue;
    //                if (memberMapper.IsKey || memberMapper.IsAutoIncrement || memberMapper.IsIgnore
    //                    || memberMapper.IsIgnoreInsert || memberMapper.IsNavigation || memberMapper.IsRowVersion)
    //                    continue;
    //                if (!lowerOnlyFields.Contains(itemKey.ToLower())) continue;

    //                if (index > 0) builder.Append(',');
    //                builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}");

    //                Func<object, object> valueGetter = null;
    //                if (memberMapper.TypeHandler != null)
    //                    valueGetter = itemValue => memberMapper.TypeHandler.ToFieldValue(itemValue);
    //                else
    //                {
    //                    object itemValue = null;
    //                    var targetType = ormProvider.MapDefaultType(memberMapper);
    //                    itemValue = dict[itemKey];
    //                    var itemValueType = itemValue.GetType();
    //                    valueGetter = ormProvider.GetParameterValueGetter(itemValueType, targetType, !memberMapper.IsRequired, dbContext);
    //                }
    //                valueFields.Add((itemKey, $"{ormProvider.ParameterPrefix}{memberMapper.MemberName}", valueGetter));
    //                index++;
    //            }
    //            builder.Append(") VALUES ");
    //            var fixedSql = builder.ToString();
    //            builder.Clear();
    //            loopCommandInitializer.Invoke(dbParameters, builder, ormProvider, valueFields, fixedSql, insertObjs);
    //        };
    //        else if (!hasOnlyFields && hasIgnoreFields) commandInitializer = (dbParameters, builder, dbContext, entityType, lowerOnlyFields, lowerIgnoreFields, parameters) =>
    //        {
    //            int index = 0;
    //            var ormProvider = dbContext.OrmProvider;
    //            var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);
    //            var valueFields = new List<(string, string, Func<object, object>)>();
    //            var insertObjs = parameters as IEnumerable;
    //            IDictionary<string, object> dict = null;
    //            foreach (var insertObj in insertObjs)
    //            {
    //                dict = insertObj as IDictionary<string, object>;
    //                break;
    //            }
    //            foreach (var itemKey in dict.Keys)
    //            {
    //                if (!entityMapper.TryGetMemberMap(itemKey, out var memberMapper))
    //                    continue;
    //                if (memberMapper.IsKey || memberMapper.IsAutoIncrement || memberMapper.IsIgnore
    //                    || memberMapper.IsIgnoreInsert || memberMapper.IsNavigation || memberMapper.IsRowVersion)
    //                    continue;
    //                if (lowerIgnoreFields.Contains(itemKey.ToLower())) continue;

    //                if (index > 0) builder.Append(',');
    //                builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}");

    //                Func<object, object> valueGetter = null;
    //                if (memberMapper.TypeHandler != null)
    //                    valueGetter = itemValue => memberMapper.TypeHandler.ToFieldValue(itemValue);
    //                else
    //                {
    //                    object itemValue = null;
    //                    var targetType = ormProvider.MapDefaultType(memberMapper);
    //                    itemValue = dict[itemKey];
    //                    var itemValueType = itemValue.GetType();
    //                    valueGetter = ormProvider.GetParameterValueGetter(itemValueType, targetType, !memberMapper.IsRequired, dbContext);
    //                }
    //                valueFields.Add((itemKey, $"{ormProvider.ParameterPrefix}{memberMapper.MemberName}", valueGetter));
    //                index++;
    //            }
    //            builder.Append(") VALUES ");
    //            var fixedSql = builder.ToString();
    //            builder.Clear();
    //            loopCommandInitializer.Invoke(dbParameters, builder, ormProvider, valueFields, fixedSql, insertObjs);
    //        };
    //        else commandInitializer = (dbParameters, builder, dbContext, entityType, lowerOnlyFields, lowerIgnoreFields, parameters) =>
    //        {
    //            int index = 0;
    //            var ormProvider = dbContext.OrmProvider;
    //            var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);
    //            var valueFields = new List<(string, string, Func<object, object>)>();
    //            var insertObjs = parameters as IEnumerable;
    //            IDictionary<string, object> dict = null;
    //            foreach (var insertObj in insertObjs)
    //            {
    //                dict = insertObj as IDictionary<string, object>;
    //                break;
    //            }
    //            foreach (var itemKey in dict.Keys)
    //            {
    //                if (!entityMapper.TryGetMemberMap(itemKey, out var memberMapper))
    //                    continue;
    //                if (memberMapper.IsKey || memberMapper.IsAutoIncrement || memberMapper.IsIgnore
    //                    || memberMapper.IsIgnoreInsert || memberMapper.IsNavigation || memberMapper.IsRowVersion)
    //                    continue;

    //                if (index > 0) builder.Append(',');
    //                builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}");

    //                Func<object, object> valueGetter = null;
    //                if (memberMapper.TypeHandler != null)
    //                    valueGetter = itemValue => memberMapper.TypeHandler.ToFieldValue(itemValue);
    //                else
    //                {
    //                    object itemValue = null;
    //                    var targetType = ormProvider.MapDefaultType(memberMapper);
    //                    itemValue = dict[itemKey];
    //                    var itemValueType = itemValue.GetType();
    //                    valueGetter = ormProvider.GetParameterValueGetter(itemValueType, targetType, !memberMapper.IsRequired, dbContext);
    //                }
    //                valueFields.Add((itemKey, $"{ormProvider.ParameterPrefix}{memberMapper.MemberName}", valueGetter));
    //                index++;
    //            }
    //            builder.Append(") VALUES ");
    //            var fixedSql = builder.ToString();
    //            builder.Clear();
    //            loopCommandInitializer.Invoke(dbParameters, builder, ormProvider, valueFields, fixedSql, insertObjs);
    //        };
    //        return commandInitializer;
    //    });
    //}
    //public static Func<IDataParameterCollection, DbContext, List<(string, string, Func<IDictionary<string, object>, string, object>)>, string, string, object, int> CreateDictionaryBulkLoopBodis()
    //{
    //    builder.Append(") VALUES ");
    //    var firstSql = builder.ToString();
    //    var loopFixedSql = hasFixedField ? $"({fiexedValueSql}" : "(";
    //    var fixedParameters = new List<IDbDataParameter>();
    //    var enumerable = parameters as IEnumerable;
    //    var suffix = isBulk ? index.ToString() : "";
    //    int iLoopIndex = 0, count = 0, bulkCount = 500;
    //    ITheaCommand command = null;
    //    var dbParameters = new TheaDbParameterCollection();

    //    Func<IDataParameterCollection, DbContext, List<(string, string, Func<IDictionary<string, object>, string, object>)>, string, string, object, int> result = null;
    //    result = (dbParameters, dbContext, valueFields, firstFieldSql, firstValueSql, parameters) =>
    //    {
    //        var insertObjs = parameters as IEnumerable;
    //        var builder = new StringBuilder(firstFieldSql);
    //        int iLoopIndex = 0, count = 0, bulkCount = 500;
    //        var ormProvider = dbContext.OrmProvider;

    //        foreach (var insertObj in insertObjs)
    //        {
    //            var dict = insertObj as IDictionary<string, object>;
    //            if (iLoopIndex > 0) builder.Append(",");
    //            builder.Append(loopFixedSql);
    //            var index = 0;
    //            foreach ((var itemKey, var parameterPrefix, var valueGetter) in valueFields)
    //            {
    //                if (index > 0) builder.Append(",");
    //                builder.Append(parameterPrefix);
    //                builder.Append(suffix);
    //                var fieldValue = valueGetter.Invoke(dict, itemKey);
    //                dbParameters.Add(ormProvider.CreateParameter($"{parameterPrefix}{dbParameters.Count}", fieldValue));
    //                index++;
    //            }
    //            builder.Append(")");
    //            iLoopIndex++;

    //            if (iLoopIndex > bulkCount)
    //            {
    //                command.CommandText = builder.ToString();
    //                count += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
    //                builder.Clear();
    //                command.Parameters.Clear();
    //                builder.Append(firstFieldSql);
    //                builder.Append(firstValueSql);
    //                fixedParameters.ForEach(f => dbParameters.Add(f));
    //                iLoopIndex = 0;
    //            }
    //        }
    //        if (iLoopIndex > 0)
    //        {
    //            command.CommandText = builder.ToString();
    //            count += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
    //            builder.Clear();
    //            command.Parameters.Clear();
    //        }
    //    };
    //}

    //public static object BuildFieldsSqlParametersPart(DbContext dbContext, Type entityType, Type parametersType, int commandType, int sqlType, int keyType, bool isFunc, bool isBulk, bool isUpdateRowVersion, List<string> onlyFieldNames, List<string> ignoreFieldNames, string headSql = null, string tailSql = null)
    //{
    //    //commandType 1:Insert Field, 2:Insert Value, 3:Insert Update Set 4:Update Set
    //    //sqlType 1:Sql And Parameters 2:Only Sql 3:Only Parameters
    //    //keyType 0:None 1:Use Keys 2:Ignore Keys      
    //    var dbContextExpr = Expression.Parameter(typeof(DbContext), "dbContext");
    //    var parametersExpr = Expression.Parameter(typeof(object), "parameters");

    //    ParameterExpression dbParametersExpr = null;
    //    ParameterExpression builderExpr = null;
    //    ParameterExpression suffixExpr = null;
    //    ParameterExpression ormProviderExpr = null;
    //    ParameterExpression parameterNameExpr = null;
    //    ParameterExpression typedParametersExpr = null;
    //    var blockParameters = new List<ParameterExpression>();
    //    var blockBodies = new List<Expression>();

    //    bool isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(parametersType);
    //    var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), [typeof(string)]);
    //    var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)]);
    //    MethodInfo methodInfo = null;

    //    if (commandType > 1 && sqlType != 2)
    //        dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");

    //    if (isDictionary || commandType > 1 && sqlType != 2)
    //    {
    //        if (isDictionary) parametersType = typeof(IDictionary<string, object>);
    //        ormProviderExpr = Expression.Variable(typeof(IOrmProvider), "ormProvider");
    //        typedParametersExpr = Expression.Variable(parametersType, isDictionary ? "dict" : "typedParameters");
    //        blockParameters.AddRange([ormProviderExpr, typedParametersExpr]);
    //        blockBodies.Add(Expression.Assign(ormProviderExpr, Expression.Property(dbContextExpr, nameof(DbContext.OrmProvider))));
    //        blockBodies.Add(Expression.Assign(typedParametersExpr, Expression.Convert(parametersExpr, parametersType)));
    //    }
    //    if (isFunc)
    //    {
    //        builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
    //        blockParameters.Add(builderExpr);
    //        var constructorInfo = typeof(StringBuilder).GetConstructor(Type.EmptyTypes);
    //        blockBodies.Add(Expression.Assign(builderExpr, Expression.New(constructorInfo)));
    //    }
    //    else if (sqlType != 3) builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
    //    if (commandType > 1 && isBulk)
    //    {
    //        suffixExpr = Expression.Parameter(typeof(string), "suffix");
    //        parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
    //        blockParameters.Add(parameterNameExpr);
    //    }

    //    if (sqlType != 3 && !string.IsNullOrEmpty(headSql))
    //        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(headSql)));

    //    ParameterExpression entityMapperExpr = null;
    //    ParameterExpression memberMapperExpr = null;
    //    MethodInfo containsKeyMethodInfo = null;
    //    PropertyInfo dictItemPropertyInfo = null;
    //    var ormProvider = dbContext.OrmProvider;

    //    if (isDictionary)
    //    {
    //        entityMapperExpr = Expression.Variable(typeof(EntityMap), "entityMapper");
    //        memberMapperExpr = Expression.Variable(typeof(MemberMap), "memberMapper");
    //        blockParameters.AddRange([entityMapperExpr, memberMapperExpr]);

    //        containsKeyMethodInfo = typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.ContainsKey));
    //        dictItemPropertyInfo = typeof(IDictionary<string, object>).GetProperties(BindingFlags.Instance | BindingFlags.Public)
    //            .Where(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(string)).First();

    //        var mapProviderExpr = Expression.Property(dbContextExpr, nameof(DbContext.EntityMapProvider));
    //        methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.GetEntityMap), [typeof(EntityMapProvider), typeof(Type)]);
    //        blockBodies.Add(Expression.Assign(entityMapperExpr, Expression.Call(methodInfo, mapProviderExpr, Expression.Constant(entityType))));
    //    }

    //    if (isDictionary && keyType != 1)
    //    {
    //        var indexExpr = Expression.Variable(typeof(int), "index");
    //        var enumeratorExpr = Expression.Variable(typeof(IEnumerator<KeyValuePair<string, object>>), "enumerator");
    //        var itemKeyExpr = Expression.Variable(typeof(string), "itemKey");
    //        var itemValueExpr = Expression.Variable(typeof(object), "itemValue");
    //        var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string), typeof(string)]);

    //        blockParameters.AddRange([indexExpr, enumeratorExpr, itemKeyExpr, itemValueExpr]);
    //        var breakLabel = Expression.Label();
    //        var continueLabel = Expression.Label();

    //        //var index = 0;
    //        //var enumerator = dict.GetEnumerator();
    //        blockBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
    //        methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
    //        blockBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(typedParametersExpr, methodInfo)));

    //        //if(!enumerator.MoveNext())
    //        //  break;
    //        var loopBodies = new List<Expression>();
    //        methodInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
    //        var ifFalseExpr = Expression.IsFalse(Expression.Call(enumeratorExpr, methodInfo));
    //        loopBodies.Add(Expression.IfThen(ifFalseExpr, Expression.Break(breakLabel)));

    //        //var itemKey = enumerator.Current.Key;
    //        //var fieldValue = enumerator.Current.Value;          
    //        var currentExpr = Expression.Property(enumeratorExpr, nameof(IEnumerator.Current));
    //        loopBodies.Add(Expression.Assign(itemKeyExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Key))));

    //        //if(!entityMapper.ContainsMemberMap(itemKey)) continue;
    //        methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.ContainsMemberMap));
    //        Expression isContinueExpr = Expression.IsFalse(Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr));
    //        loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

    //        //var memberMapper = entityMapper.GetMemberMap(itemKey);
    //        methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.GetMemberMap));
    //        loopBodies.Add(Expression.Assign(memberMapperExpr, Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr)));
    //        //|| memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsKey
    //        isContinueExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnore));
    //        isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsNavigation)));
    //        if (keyType == 2)
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsKey)));

    //        //|| memberMapper.IsIgnoreInsert || memberMapper.IsAutoIncrement
    //        if (commandType < 3)
    //        {
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnoreInsert)));
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsAutoIncrement)));
    //        }
    //        //|| memberMapper.IsIgnoreUpdate
    //        if (commandType > 2)
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnoreUpdate)));
    //        //|| memberMapper.IsRowVersion
    //        if (!isUpdateRowVersion)
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsRowVersion)));

    //        var lowerItemKeyExpr = Expression.Call(itemKeyExpr, typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes));
    //        //|| !onlyFields.Constains(itemKey.ToLower())
    //        if (onlyFieldNames != null)
    //        {
    //            var initExprs = onlyFieldNames.Select(f => Expression.Constant(f, typeof(string)));
    //            var onlyFieldsExpr = Expression.NewArrayInit(typeof(string), initExprs);
    //            methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), [typeof(string)]);
    //            var isFalseExpr = Expression.IsFalse(Expression.Call(methodInfo, onlyFieldsExpr, lowerItemKeyExpr));
    //            isContinueExpr = Expression.OrElse(isContinueExpr, isFalseExpr);
    //        }
    //        //|| ignoreFields.Constains(itemKey.ToLower()) 
    //        if (ignoreFieldNames != null)
    //        {
    //            var initExprs = ignoreFieldNames.Select(f => Expression.Constant(f, typeof(string)));
    //            var ignoreFieldsExpr = Expression.NewArrayInit(typeof(string), initExprs);
    //            methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), [typeof(string)]);
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Call(methodInfo, ignoreFieldsExpr, lowerItemKeyExpr));
    //        }
    //        loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

    //        //var parameterName = ormProvider.ParameterPrefix + memberMapper.MemberName + suffix;
    //        Expression myParameterNameExpr = Expression.Constant(ormProvider.ParameterPrefix);
    //        if (commandType > 1)
    //        {
    //            var memberNameExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.MemberName));
    //            if (isBulk)
    //            {
    //                myParameterNameExpr = Expression.Call(concatMethodInfo2, myParameterNameExpr, memberNameExpr, suffixExpr);
    //                loopBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
    //                myParameterNameExpr = parameterNameExpr;
    //            }
    //            else myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, memberNameExpr);
    //        }
    //        //生成SQL
    //        if (sqlType < 3)
    //        {
    //            //if(index > 0) builder.Append(",");
    //            var greaterThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0));
    //            var callExpr = Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(","));
    //            loopBodies.Add(Expression.IfThen(greaterThenExpr, callExpr));

    //            //builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)");
    //            //builder.Append(parameterName);
    //            //builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");

    //            Expression contentExpr = null;
    //            Expression fieldNameExpr = null;

    //            if (commandType == 2) contentExpr = myParameterNameExpr;
    //            else
    //            {
    //                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetFieldName));
    //                fieldNameExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.FieldName));
    //                fieldNameExpr = Expression.Call(ormProviderExpr, methodInfo, fieldNameExpr);

    //                if (commandType == 1) contentExpr = fieldNameExpr;
    //                else contentExpr = Expression.Call(concatMethodInfo2, fieldNameExpr, Expression.Constant("="), myParameterNameExpr);
    //            }
    //            loopBodies.Add(Expression.Call(builderExpr, appendMethodInfo, contentExpr));
    //        }
    //        //生成参数
    //        if (commandType > 1 && sqlType != 2)
    //        {
    //            loopBodies.Add(Expression.Assign(itemValueExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Value))));
    //            AddValueParameter(dbContext, dbContextExpr, dbParametersExpr, ormProviderExpr, myParameterNameExpr, itemValueExpr, memberMapperExpr, loopBodies);
    //        }

    //        //index++;
    //        loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));
    //        blockBodies.Add(Expression.Loop(Expression.Block(loopBodies), breakLabel, continueLabel));
    //    }
    //    else
    //    {
    //        var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);
    //        var filterMemberMaps = keyType == 1 ? entityMapper.KeyMembers : entityMapper.MemberMaps;
    //        Dictionary<string, MemberInfo> targetMemberInfos = null;

    //        if (!isDictionary)
    //        {
    //            targetMemberInfos = parametersType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
    //                .Where(f => f.MemberType == MemberTypes.Property || f.MemberType == MemberTypes.Field)
    //                .ToDictionary(f => f.Name.ToLower(), f => f);
    //        }
    //        var index = 0;
    //        foreach (var memberMapper in filterMemberMaps)
    //        {
    //            ParameterExpression valueTupleExpr = null;
    //            MemberInfo targetMemberInfo = null;
    //            var lowerMemberName = memberMapper.MemberName.ToLower();
    //            if (keyType == 1 && isDictionary)
    //            {
    //                //var tuple = dict.ContainsLowerKey(targetMemberInfo.Name.ToLower());
    //                //if(!tuple.Item1)
    //                //  throw new KeyNotFoundException($"字典参数中{parametersType.FullName}缺少Key:{memberMapper.MemberName}的成员");
    //                valueTupleExpr = Expression.Variable(typeof(ValueTuple<bool, object>), $"{memberMapper.MemberName.ToCamel()}Tuple");
    //                blockParameters.Add(valueTupleExpr);
    //                var lowerMemberNameExpr = Expression.Constant(lowerMemberName);
    //                methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.TryGetValueIgnoreCase));
    //                var containsLowerKeyExpr = Expression.Call(methodInfo, typedParametersExpr, lowerMemberNameExpr);
    //                blockBodies.Add(Expression.Assign(valueTupleExpr, containsLowerKeyExpr));
    //                var exception = new KeyNotFoundException($"字典参数中{parametersType.FullName}缺少Key:{memberMapper.MemberName}的成员");
    //                var isContainsKeyExpr = Expression.Field(valueTupleExpr, "Item1");
    //                blockBodies.Add(Expression.IfThen(Expression.IsFalse(isContainsKeyExpr), Expression.Throw(Expression.Constant(exception))));
    //            }
    //            //忽略大小写
    //            else if (!targetMemberInfos.TryGetValue(lowerMemberName, out targetMemberInfo))
    //            {
    //                if (keyType == 1) throw new KeyNotFoundException($"参数类型{parametersType.FullName}缺少{memberMapper.MemberName}的成员");
    //                else continue;
    //            }

    //            if (memberMapper.IsIgnore || memberMapper.IsNavigation || (keyType == 2 && memberMapper.IsKey))
    //                continue;
    //            if (onlyFieldNames != null && !onlyFieldNames.Contains(lowerMemberName))
    //                continue;
    //            if (ignoreFieldNames != null && ignoreFieldNames.Contains(lowerMemberName))
    //                continue;
    //            if (!isUpdateRowVersion && memberMapper.IsRowVersion)
    //                continue;
    //            //Insert
    //            if (commandType < 3 && (memberMapper.IsIgnoreInsert || memberMapper.IsAutoIncrement))
    //                continue;
    //            //Update
    //            if (commandType > 2 && memberMapper.IsIgnoreUpdate)
    //                continue;

    //            var parameterName = ormProvider.ParameterPrefix + (commandType == 3 ? "p" : "") + memberMapper.MemberName;
    //            Expression myParameterNameExpr = Expression.Constant(parameterName);
    //            if (commandType > 1 && isBulk)
    //            {
    //                myParameterNameExpr = Expression.Call(concatMethodInfo, myParameterNameExpr, suffixExpr);
    //                blockBodies.Add(Expression.Assign(parameterNameExpr, myParameterNameExpr));
    //                myParameterNameExpr = parameterNameExpr;
    //            }
    //            //生成SQL
    //            if (sqlType != 3)
    //            {
    //                if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(",")));
    //                Expression contentExpr = null;
    //                switch (commandType)
    //                {
    //                    case 1: contentExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}"); break;
    //                    case 2: contentExpr = myParameterNameExpr; break;
    //                    case 3:
    //                    case 4:
    //                        contentExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}=");
    //                        contentExpr = Expression.Call(concatMethodInfo, contentExpr, myParameterNameExpr);
    //                        break;
    //                }
    //                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, contentExpr));
    //            }
    //            //生成参数
    //            if (commandType > 1 && sqlType != 2)
    //            {
    //                if (isDictionary)
    //                {
    //                    var fieldValueExpr = Expression.Field(valueTupleExpr, "Item2");
    //                    AddValueParameter(dbContext, dbContextExpr, dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueExpr, memberMapperExpr, blockBodies);
    //                }
    //                else
    //                {
    //                    var fieldValueType = targetMemberInfo.GetMemberType();
    //                    Expression fieldValueExpr = Expression.PropertyOrField(typedParametersExpr, memberMapper.MemberName);
    //                    AddValueParameter(dbContext, dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueType, fieldValueExpr, memberMapper, blockBodies);
    //                }
    //            }
    //            index++;
    //        }
    //        if (index <= 0)
    //            throw new Exception($"没有找到{(commandType == 4 ? "更新" : "插入")}语句");
    //    }

    //    if (sqlType != 3 && !string.IsNullOrEmpty(tailSql))
    //        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(tailSql)));

    //    if (isFunc)
    //    {
    //        methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
    //        var returnExpr = Expression.Call(builderExpr, methodInfo);
    //        var resultLabelExpr = Expression.Label(typeof(string));
    //        blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
    //        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));

    //        if (commandType == 1) return Expression.Lambda<Func<DbContext, object, string>>(
    //            Expression.Block(blockParameters, blockBodies), dbContextExpr, parametersExpr).Compile();
    //        else
    //        {
    //            if (sqlType == 2)
    //            {
    //                if (isBulk) return Expression.Lambda<Func<DbContext, object, string, string>>(
    //                    Expression.Block(blockParameters, blockBodies), dbContextExpr, parametersExpr, suffixExpr).Compile();
    //                else return Expression.Lambda<Func<DbContext, object, string>>(
    //                    Expression.Block(blockParameters, blockBodies), dbContextExpr, parametersExpr).Compile();
    //            }
    //            else
    //            {
    //                if (isBulk) return Expression.Lambda<Func<IDataParameterCollection, DbContext, object, string, string>>(
    //                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, parametersExpr, suffixExpr).Compile();
    //                else return Expression.Lambda<Func<IDataParameterCollection, DbContext, object, string>>(
    //                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, parametersExpr).Compile();
    //            }
    //        }
    //    }
    //    else
    //    {
    //        if (commandType == 1) return Expression.Lambda<Action<StringBuilder, DbContext, object>>(
    //            Expression.Block(blockParameters, blockBodies), builderExpr, dbContextExpr, parametersExpr).Compile();
    //        else
    //        {
    //            switch (sqlType)
    //            {
    //                case 1:
    //                    if (isBulk) return Expression.Lambda<Action<IDataParameterCollection, StringBuilder, DbContext, object, string>>(
    //                        Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, dbContextExpr, parametersExpr, suffixExpr).Compile();
    //                    else return Expression.Lambda<Action<IDataParameterCollection, StringBuilder, DbContext, object>>(
    //                        Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, dbContextExpr, parametersExpr).Compile();
    //                case 2:
    //                    if (isBulk) return Expression.Lambda<Action<StringBuilder, DbContext, object, string>>(
    //                        Expression.Block(blockParameters, blockBodies), builderExpr, dbContextExpr, parametersExpr, suffixExpr).Compile();
    //                    else return Expression.Lambda<Action<StringBuilder, DbContext, object>>(
    //                        Expression.Block(blockParameters, blockBodies), builderExpr, dbContextExpr, parametersExpr).Compile();
    //                case 3:
    //                    if (isBulk) return Expression.Lambda<Action<IDataParameterCollection, DbContext, object, string>>(
    //                        Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, parametersExpr, suffixExpr).Compile();
    //                    else return Expression.Lambda<Action<IDataParameterCollection, DbContext, object>>(
    //                        Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, parametersExpr).Compile();
    //                default: throw new NotSupportedException("不支持的场景");
    //            }
    //        }
    //    }
    //}


    //public static Action<DbContext, ITheaCommand, object> BuildCreateCommandInitializer(DbContext dbContext, Type entityType, object insertObj, bool isReturnIdentity)
    //{
    //    var insertObjType = insertObj.GetType();
    //    var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.MapProvider, entityType, insertObjType, isReturnIdentity);
    //    return createCommandInitializerCache.GetOrAdd(cacheKey, f =>
    //    {
    //        var ormProvider = dbContext.OrmProvider;
    //        var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);
    //        var tableName = entityMapper.TableName;

    //        var tailSql = ")";
    //        if (isReturnIdentity)
    //        {
    //            var keyField = entityMapper.KeyMembers[0].FieldName;
    //            tailSql += ormProvider.GetIdentitySql(ormProvider.GetFieldName(keyField));
    //        }
    //        var fieldsSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 1, 2, 0, true, false, false, null, null, " (", ") VALUES ")
    //            as Func<DbContext, object, string>;
    //        var valuesSqlSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 2, 2, 0, true, false, false, null, null, "(", tailSql)
    //            as Func<DbContext, object, string>;
    //        var valuesParametersSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 2, 3, 0, false, false, false, null, null)
    //            as Action<IDataParameterCollection, DbContext, object>;
    //        var sql = $"INSERT INTO {ormProvider.GetTableName(tableName)}" + fieldsSetter.Invoke(dbContext, insertObj) + valuesSqlSetter.Invoke(dbContext, insertObj);
    //        return (dbContext, command, insertObjs) =>
    //        {
    //            command.CommandText = sql;
    //            valuesParametersSetter.Invoke(command.Parameters, dbContext, insertObjs);
    //        };
    //    });
    //}
    //public static Func<DbContext, ITheaCommand, IEnumerable, int, int> BuildCreateBulkCommandExecutor(DbContext dbContext, Type entityType, IEnumerable insertObjs)
    //{
    //    object firstInsertObj = null;
    //    Type insertObjType = null;
    //    foreach (var insertObj in insertObjs)
    //    {
    //        firstInsertObj = insertObj;
    //        insertObjType = insertObj.GetType();
    //        break;
    //    }
    //    var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.MapProvider, entityType, insertObjType);
    //    return createBulkCommandExecutorCache.GetOrAdd(cacheKey, f =>
    //    {
    //        var ormProvider = dbContext.OrmProvider;
    //        var fieldsSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 1, 2, 0, true, false, false, null, null, "(", ") VALUES ")
    //            as Func<DbContext, object, string>;
    //        var valuesSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 2, 1, 0, false, true, false, null, null, "(", ")")
    //            as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
    //        var fieldsSql = fieldsSetter.Invoke(dbContext, firstInsertObj);

    //        int Execute(DbContext dbContext, ITheaCommand command, string tableName, IEnumerable insertObjs, int bulkCount)
    //        {
    //            int count = 0, index = 0;
    //            var builder = new StringBuilder($"INSERT INTO {tableName}{fieldsSql} ");
    //            foreach (var insertObj in insertObjs)
    //            {
    //                if (index > 0) builder.Append(',');

    //                valuesSetter.Invoke(command.Parameters, builder, dbContext, insertObj, index.ToString());
    //                index++;

    //                if (index >= bulkCount)
    //                {
    //                    command.CommandText = builder.ToString();
    //                    count += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
    //                    builder.Clear();
    //                    command.Parameters.Clear();
    //                    builder.Append($"INSERT INTO {tableName}{fieldsSql}");
    //                    index = 0;
    //                }
    //            }
    //            if (index > 0)
    //            {
    //                command.CommandText = builder.ToString();
    //                count += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
    //                builder.Clear();
    //                command.Parameters.Clear();
    //            }
    //            return count;
    //        }

    //        Func<DbContext, ITheaCommand, IEnumerable, int, int> commandExecutor = null;
    //        var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);
    //        if (dbContext.ShardingProvider != null && dbContext.ShardingProvider.TryGetTableSharding(entityType, out var tableShardingInfo))
    //        {
    //            Func<object, string> jsonHandler = obj => $"{dbContext.JsonTypeHandler.ToFieldValue(obj)}";
    //            commandExecutor = (dbContext, command, insertObjs, bulkCount) =>
    //            {
    //                int count = 0;
    //                var tabledInsertObjs = SplitShardingParameters(tableShardingInfo, entityMapper, insertObjType, insertObjs, firstInsertObj, jsonHandler);
    //                var tableName = ormProvider.GetTableName(entityMapper.TableName);
    //                foreach (var tabledInsertObj in tabledInsertObjs)
    //                {
    //                    count += Execute(dbContext, command, tabledInsertObj.Key, tabledInsertObj.Value, bulkCount);
    //                }
    //                return count;
    //            };
    //        }
    //        else
    //        {
    //            var tableName = ormProvider.GetTableName(entityMapper.TableName);
    //            commandExecutor = (dbContext, command, insertObjs, bulkCount) => Execute(dbContext, command, tableName, insertObjs, bulkCount);
    //        }
    //        return commandExecutor;
    //    });
    //}
    //public static Func<DbContext, ITheaCommand, IEnumerable, int, CancellationToken, Task<int>> BuildCreateBulkAsyncCommandExecutor(DbContext dbContext, Type entityType, IEnumerable insertObjs)
    //{
    //    object firstInsertObj = null;
    //    Type insertObjType = null;
    //    foreach (var insertObj in insertObjs)
    //    {
    //        firstInsertObj = insertObj;
    //        insertObjType = insertObj.GetType();
    //        break;
    //    }
    //    var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.MapProvider, entityType, insertObjType);
    //    return createBulkAsyncCommandExecutorCache.GetOrAdd(cacheKey, f =>
    //    {
    //        var ormProvider = dbContext.OrmProvider;
    //        var fieldsSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 1, 2, 0, true, false, false, null, null, "(", ") VALUES ")
    //            as Func<DbContext, object, string>;
    //        var valuesSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 2, 1, 0, false, true, false, null, null, "(", ")")
    //            as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
    //        var fieldsSql = fieldsSetter.Invoke(dbContext, firstInsertObj);

    //        async Task<int> Execute(DbContext dbContext, ITheaCommand command, string tableName, IEnumerable insertObjs, int bulkCount, CancellationToken cancellationToken)
    //        {
    //            int count = 0, index = 0;
    //            var builder = new StringBuilder($"INSERT INTO {tableName}{fieldsSql} ");
    //            foreach (var insertObj in insertObjs)
    //            {
    //                if (index > 0) builder.Append(',');
    //                valuesSetter.Invoke(command.Parameters, builder, dbContext, insertObj, index.ToString());
    //                index++;

    //                if (index >= bulkCount)
    //                {
    //                    command.CommandText = builder.ToString();
    //                    count += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
    //                    builder.Clear();
    //                    command.Parameters.Clear();
    //                    builder.Append($"INSERT INTO {tableName}{fieldsSql}");
    //                    index = 0;
    //                }
    //            }
    //            if (index > 0)
    //            {
    //                command.CommandText = builder.ToString();
    //                count += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
    //                builder.Clear();
    //                command.Parameters.Clear();
    //            }
    //            return count;
    //        }

    //        Func<DbContext, ITheaCommand, IEnumerable, int, CancellationToken, Task<int>> commandExecutor = null;
    //        var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);
    //        if (dbContext.ShardingProvider != null && dbContext.ShardingProvider.TryGetTableSharding(entityType, out var tableShardingInfo))
    //        {
    //            Func<object, string> jsonHandler = obj => $"{dbContext.JsonTypeHandler.ToFieldValue(obj)}";
    //            commandExecutor = async (dbContext, command, insertObjs, bulkCount, cancellationToken) =>
    //            {
    //                int count = 0;
    //                var tabledInsertObjs = SplitShardingParameters(tableShardingInfo, entityMapper, insertObjType, insertObjs, firstInsertObj, jsonHandler);
    //                var tableName = ormProvider.GetTableName(entityMapper.TableName);
    //                foreach (var tabledInsertObj in tabledInsertObjs)
    //                {
    //                    count += await Execute(dbContext, command, tabledInsertObj.Key, tabledInsertObj.Value, bulkCount, cancellationToken);
    //                }
    //                return count;
    //            };
    //        }
    //        else
    //        {
    //            var tableName = ormProvider.GetTableName(entityMapper.TableName);
    //            commandExecutor = (dbContext, command, insertObjs, bulkCount, cancellationToken) => Execute(dbContext, command, tableName, insertObjs, bulkCount, cancellationToken);
    //        }
    //        return commandExecutor;
    //    });
    //}
    //public static Action<StringBuilder, DbContext, object> BuildCreateFieldsSqlPart(DbContext dbContext, Type entityType, Type insertObjType, bool isUpdateRowVersion, List<string> onlyFieldNames, List<string> ignoreFieldNames)
    //{
    //    var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.MapProvider, entityType, insertObjType, GetHashCode(onlyFieldNames), GetHashCode(ignoreFieldNames));
    //    return createFieldsSqlCache.GetOrAdd(cacheKey, f =>
    //    {
    //        Action<StringBuilder, DbContext, object> fieldsSetter = null;
    //        if (typeof(IDictionary<string, object>).IsAssignableFrom(insertObjType))
    //        {
    //            fieldsSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 1, 2, 0, false, false, isUpdateRowVersion, onlyFieldNames, ignoreFieldNames)
    //                as Action<StringBuilder, DbContext, object>;
    //        }
    //        else
    //        {
    //            var fieldsSqlGetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 1, 2, 0, true, false, isUpdateRowVersion, onlyFieldNames, ignoreFieldNames)
    //                as Func<DbContext, object, string>;
    //            var fieldsSql = fieldsSqlGetter.Invoke(dbContext, null);
    //            fieldsSetter = (builder, dbContext, insertObj) => builder.Append(fieldsSql);
    //        }
    //        return fieldsSetter;
    //    });
    //}
    //public static object BuildCreateValuesSqlPart(DbContext dbContext, Type entityType, Type insertObjType, bool isBulk, bool isUpdateRowVersion, List<string> onlyFieldNames, List<string> ignoreFieldNames)
    //{
    //    var ormProvider = dbContext.OrmProvider;
    //    var cacheKey = GetCacheKey(ormProvider.OrmProviderType, dbContext.MapProvider, entityType, insertObjType, GetHashCode(onlyFieldNames), GetHashCode(ignoreFieldNames));
    //    var cache = isBulk ? createBulkValuesSqlParametersCache : createValuesSqlParametersCache;
    //    return cache.GetOrAdd(cacheKey, f =>
    //    {
    //        var isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(insertObjType);
    //        if (!isDictionary && !isBulk)
    //        {
    //            var valuesSqlSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 2, 2, 0, true, isBulk, isUpdateRowVersion, onlyFieldNames, ignoreFieldNames)
    //                as Func<DbContext, object, string>;
    //            var valuesParametersSetter = BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 2, 3, 0, false, isBulk, isUpdateRowVersion, onlyFieldNames, ignoreFieldNames)
    //                as Action<IDataParameterCollection, DbContext, object>;
    //            var sql = valuesSqlSetter.Invoke(dbContext, null);
    //            Action<IDataParameterCollection, StringBuilder, DbContext, object> valuesSetter = (dbParameters, builder, dbContext, insertObj) =>
    //            {
    //                builder.Append(sql);
    //                valuesParametersSetter.Invoke(dbParameters, dbContext, insertObj);
    //            };
    //            return valuesSetter;
    //        }
    //        return BuildFieldsSqlParametersPart(dbContext, entityType, insertObjType, 2, 1, 0, false, isBulk, isUpdateRowVersion, onlyFieldNames, ignoreFieldNames);
    //    });
    //}

    //public static object BuildUpdateCommandInitializer(DbContext dbContext, Type entityType, Type updateObjType, bool isBulk)
    //{
    //    var ormProvider = dbContext.OrmProvider;
    //    var cacheKey = GetCacheKey(ormProvider.OrmProviderType, dbContext.EntityMapProvider, entityType, updateObjType, isBulk);
    //    var cache = isBulk ? updateBulkCommandInitializerCache : updateCommandInitializerCache;
    //    return cache.GetOrAdd(cacheKey, f =>
    //    {
    //        var entityMapper = dbContext.EntityMapProvider.GetEntityMap(entityType);
    //        var headSql = $"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ";
    //        var isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(updateObjType);
    //        object commandInitializer = null;
    //        if (isBulk)
    //        {
    //            var fieldsSqlSetter = BuildFieldsSqlParametersPart(dbContext, entityType, updateObjType, 4, 1, 2, false, isBulk, isUpdateRowVersion, null, null, headSql) as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
    //            var whereSqlSetter = BuildWhereSqlParametersPart(dbContext, entityType, updateObjType, 1, false, true, true, isBulk, " WHERE ") as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
    //            Action<IDataParameterCollection, StringBuilder, DbContext, object, string> typedCommandInitializer = (dbParameters, builder, dbContext, updateObj, suffix) =>
    //            {
    //                fieldsSqlSetter.Invoke(dbParameters, builder, dbContext, updateObj, suffix);
    //                whereSqlSetter.Invoke(dbParameters, builder, dbContext, updateObj, suffix);
    //            };
    //            commandInitializer = typedCommandInitializer;
    //        }
    //        else
    //        {
    //            Func<IDataParameterCollection, DbContext, object, string> typedCommandInitializer = null;
    //            if (isDictionary)
    //            {
    //                var fieldsSqlSetter = BuildFieldsSqlParametersPart(dbContext, entityType, updateObjType, 4, 1, 2, false, isBulk, isUpdateRowVersion, null, null, headSql) as Action<IDataParameterCollection, StringBuilder, DbContext, object>;
    //                var whereSqlSetter = BuildWhereSqlParametersPart(dbContext, entityType, updateObjType, 1, false, true, true, isBulk, " WHERE ") as Action<IDataParameterCollection, StringBuilder, DbContext, object>;
    //                typedCommandInitializer = (dbParameters, dbContext, updateObj) =>
    //                {
    //                    var builder = new StringBuilder();
    //                    fieldsSqlSetter.Invoke(dbParameters, builder, dbContext, updateObj);
    //                    whereSqlSetter.Invoke(dbParameters, builder, dbContext, updateObj);
    //                    return builder.ToString();
    //                };
    //            }
    //            else
    //            {
    //                var fieldsSqlSetter = BuildFieldsSqlParametersPart(dbContext, entityType, updateObjType, 4, 2, 2, true, false, isUpdateRowVersion, null, null, headSql) as Func<DbContext, object, string>;
    //                var fieldsParameterSetter = BuildFieldsSqlParametersPart(dbContext, entityType, updateObjType, 4, 3, 0, false, false, isUpdateRowVersion, null, null, headSql) as Action<IDataParameterCollection, DbContext, object>;
    //                var whereSqlSetter = BuildWhereSqlParametersPart(dbContext, entityType, updateObjType, 2, true, true, true, isBulk, " WHERE ") as Func<DbContext, object, string>;
    //                var whereParameterSetter = BuildWhereSqlParametersPart(dbContext, entityType, updateObjType, 3, false, true, true, isBulk) as Action<IDataParameterCollection, DbContext, object>;
    //                var sql = fieldsSqlSetter.Invoke(dbContext, null) + whereSqlSetter.Invoke(dbContext, null);
    //                typedCommandInitializer = (dbParameters, dbContext, updateObj) =>
    //                {
    //                    fieldsParameterSetter.Invoke(dbParameters, dbContext, updateObj);
    //                    whereParameterSetter.Invoke(dbParameters, dbContext, updateObj);
    //                    return sql;
    //                };
    //            }
    //            commandInitializer = typedCommandInitializer;
    //        }
    //        return commandInitializer;
    //    });
    //}
    //public static object BuildUpdateSetWithSqlParametersPart(DbContext dbContext, Type entityType, Type updateObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool isUpdateRowVersion)
    //{
    //    //单个对象，有可能会有join操作，会有添加别名的可能，多命令执行时会有suffix情况
    //    //Bulk场景，反而没有别名，也没有suffix情况
    //    var ormProvider = dbContext.OrmProvider;
    //    var mapProvider = dbContext.MapProvider;
    //    var cacheKey = GetCacheKey(ormProvider.OrmProviderType, dbContext.MapProvider, entityType, updateObjType, GetHashCode(onlyFieldNames), GetHashCode(ignoreFieldNames));
    //    return updateWithCommandInitializerCache.GetOrAdd(cacheKey, f =>
    //    {
    //        var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
    //        var dbContextExpr = Expression.Parameter(typeof(DbContext), "dbContext");
    //        var updateFieldsExpr = Expression.Parameter(typeof(List<string>), "updateFields");
    //        var updateObjExpr = Expression.Parameter(typeof(object), "updateObj");
    //        var blockParameters = new List<ParameterExpression>();
    //        var blockBodies = new List<Expression>();

    //        bool isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(updateObjType);
    //        if (isDictionary) updateObjType = typeof(IDictionary<string, object>);
    //        var typedUpdateObjExpr = Expression.Variable(updateObjType, isDictionary ? "dict" : "typedUpdateObj");
    //        var ormProviderExpr = Expression.Variable(typeof(IOrmProvider), "ormProvider");
    //        blockParameters.AddRange([ormProviderExpr, typedUpdateObjExpr]);
    //        blockBodies.Add(Expression.Assign(ormProviderExpr, Expression.Property(dbContextExpr, nameof(DbContext.OrmProvider))));
    //        blockBodies.Add(Expression.Assign(typedUpdateObjExpr, Expression.Convert(updateObjExpr, updateObjType)));
    //        var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)]);
    //        var addMethodInfo = typeof(List<string>).GetMethod(nameof(List<string>.Add), [typeof(string)]);

    //        if (isDictionary)
    //        {
    //            var entityMapperExpr = Expression.Variable(typeof(EntityMap), "entityMapper");
    //            var memberMapperExpr = Expression.Variable(typeof(MemberMap), "memberMapper");
    //            var containsKeyMethodInfo = typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.ContainsKey));
    //            var dictItemPropertyInfo = typeof(IDictionary<string, object>).GetProperties(BindingFlags.Instance | BindingFlags.Public)
    //                .Where(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(string)).First();
    //            blockParameters.AddRange([entityMapperExpr, memberMapperExpr]);
    //            var mapProviderExpr = Expression.Property(dbContextExpr, nameof(DbContext.MapProvider));
    //            var methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.GetEntityMap), [typeof(EntityMapProvider), typeof(Type)]);
    //            blockBodies.Add(Expression.Assign(entityMapperExpr, Expression.Call(methodInfo, mapProviderExpr, Expression.Constant(entityType))));

    //            var indexExpr = Expression.Variable(typeof(int), "index");
    //            var enumeratorExpr = Expression.Variable(typeof(IEnumerator<KeyValuePair<string, object>>), "enumerator");
    //            var itemKeyExpr = Expression.Variable(typeof(string), "itemKey");
    //            var itemValueExpr = Expression.Variable(typeof(object), "itemValue");
    //            var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string), typeof(string)]);

    //            blockParameters.AddRange([indexExpr, enumeratorExpr, itemKeyExpr, itemValueExpr]);
    //            var breakLabel = Expression.Label();
    //            var continueLabel = Expression.Label();

    //            //var index = 0;
    //            //var enumerator = dict.GetEnumerator();
    //            blockBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0)));
    //            methodInfo = typeof(IEnumerable<KeyValuePair<string, object>>).GetMethod("GetEnumerator");
    //            blockBodies.Add(Expression.Assign(enumeratorExpr, Expression.Call(typedUpdateObjExpr, methodInfo)));

    //            //if(!enumerator.MoveNext())
    //            //  break;
    //            var loopBodies = new List<Expression>();
    //            methodInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
    //            var ifFalseExpr = Expression.IsFalse(Expression.Call(enumeratorExpr, methodInfo));
    //            loopBodies.Add(Expression.IfThen(ifFalseExpr, Expression.Break(breakLabel)));

    //            //var itemKey = enumerator.Current.Key.ToLower();
    //            //var lowerItemKey = itemKey.ToLower();
    //            //var fieldValue = enumerator.Current.Value;          
    //            var currentExpr = Expression.Property(enumeratorExpr, nameof(IEnumerator.Current));
    //            var myItemKeyExpr = Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Key));
    //            loopBodies.Add(Expression.Assign(itemKeyExpr, myItemKeyExpr));

    //            //if(!entityMapper.ContainsMemberMap(itemKey)) continue;
    //            methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.ContainsMemberMap));
    //            Expression isContinueExpr = Expression.IsFalse(Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr));
    //            loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

    //            //var memberMapper = entityMapper.GetMemberMap(itemKey);
    //            methodInfo = typeof(EntityMap).GetMethod(nameof(EntityMap.GetMemberMap));
    //            loopBodies.Add(Expression.Assign(memberMapperExpr, Expression.Call(entityMapperExpr, methodInfo, itemKeyExpr)));
    //            //|| memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsKey
    //            isContinueExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnore));
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsNavigation)));
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsKey)));
    //            //|| memberMapper.IsIgnoreUpdate
    //            isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsIgnoreUpdate)));
    //            //|| memberMapper.IsRowVersion
    //            if (!isUpdateRowVersion)
    //                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Property(memberMapperExpr, nameof(MemberMap.IsRowVersion)));

    //            methodInfo = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes);
    //            var lowerItemKeyExpr = Expression.Call(myItemKeyExpr, methodInfo);
    //            //|| !onlyFields.Constains(itemKey)
    //            if (onlyFieldNames != null)
    //            {
    //                var initExprs = onlyFieldNames.Select(f => Expression.Constant(f, typeof(string)));
    //                var onlyFieldsExpr = Expression.NewArrayInit(typeof(string), initExprs);
    //                methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), [typeof(string)]);
    //                var isFalseExpr = Expression.IsFalse(Expression.Call(methodInfo, onlyFieldsExpr, lowerItemKeyExpr));
    //                isContinueExpr = Expression.OrElse(isContinueExpr, isFalseExpr);
    //            }
    //            //|| ignoreFields.Constains(itemKey) 
    //            if (ignoreFieldNames != null)
    //            {
    //                var initExprs = ignoreFieldNames.Select(f => Expression.Constant(f, typeof(string)));
    //                var ignoreFieldsExpr = Expression.NewArrayInit(typeof(string), initExprs);
    //                methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Contains), [typeof(string)]);
    //                isContinueExpr = Expression.OrElse(isContinueExpr, Expression.Call(methodInfo, ignoreFieldsExpr, lowerItemKeyExpr));
    //            }
    //            loopBodies.Add(Expression.IfThen(isContinueExpr, Expression.Continue(continueLabel)));

    //            //var parameterName = ormProvider.ParameterPrefix + itemKey + suffix;
    //            var myParameterNameExpr = Expression.Call(concatMethodInfo, Expression.Constant(ormProvider.ParameterPrefix), itemKeyExpr);

    //            //updateFields.Add($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
    //            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetFieldName));
    //            Expression fieldNameExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.FieldName));
    //            fieldNameExpr = Expression.Call(ormProviderExpr, methodInfo, fieldNameExpr);
    //            var contentExpr = Expression.Call(concatMethodInfo2, fieldNameExpr, Expression.Constant("="), myParameterNameExpr);

    //            loopBodies.Add(Expression.Call(updateFieldsExpr, addMethodInfo, contentExpr));
    //            loopBodies.Add(Expression.Assign(itemValueExpr, Expression.Property(currentExpr, nameof(KeyValuePair<string, object>.Value))));
    //            AddValueParameter(dbContext, dbContextExpr, dbParametersExpr, ormProviderExpr, myParameterNameExpr, itemValueExpr, memberMapperExpr, loopBodies);

    //            //index++;
    //            loopBodies.Add(Expression.AddAssign(indexExpr, Expression.Constant(1)));
    //            blockBodies.Add(Expression.Loop(Expression.Block(loopBodies), breakLabel, continueLabel));
    //        }
    //        else
    //        {
    //            var entityMapper = mapProvider.GetEntityMap(entityType);
    //            var targetMemberInfos = updateObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
    //                .Where(f => f.MemberType == MemberTypes.Property || f.MemberType == MemberTypes.Field)
    //                .ToDictionary(f => f.Name.ToLower(), f => f);

    //            var index = 0;
    //            foreach (var memberMapper in entityMapper.MemberMaps)
    //            {
    //                MemberInfo targetMemberInfo = null;
    //                var lowerMemberName = memberMapper.MemberName.ToLower();
    //                if (!isDictionary && !targetMemberInfos.TryGetValue(lowerMemberName, out targetMemberInfo))
    //                    continue;
    //                if (memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsIgnoreUpdate || memberMapper.IsKey)
    //                    continue;
    //                if (onlyFieldNames != null && !onlyFieldNames.Contains(lowerMemberName))
    //                    continue;
    //                if (ignoreFieldNames != null && ignoreFieldNames.Contains(lowerMemberName))
    //                    continue;
    //                if (!isUpdateRowVersion && memberMapper.IsRowVersion)
    //                    continue;

    //                var parameterName = ormProvider.ParameterPrefix + memberMapper.MemberName;
    //                Expression myParameterNameExpr = Expression.Constant(parameterName);
    //                var setFieldExpr = Expression.Constant(ormProvider.GetFieldName(memberMapper.FieldName) + "=" + parameterName);
    //                blockBodies.Add(Expression.Call(updateFieldsExpr, addMethodInfo, setFieldExpr));

    //                var fieldValueType = targetMemberInfo.GetMemberType();
    //                var fieldValueExpr = Expression.PropertyOrField(typedUpdateObjExpr, targetMemberInfo.Name);
    //                AddValueParameter(dbContext, dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueType, fieldValueExpr, memberMapper, blockBodies);
    //                index++;
    //            }
    //            if (index <= 0)
    //                throw new Exception("没有找到可以更新的字段");
    //        }

    //        return Expression.Lambda<Action<IDataParameterCollection, DbContext, List<string>, object>>(
    //            Expression.Block(blockParameters, blockBodies), dbParametersExpr, dbContextExpr, updateFieldsExpr, updateObjExpr).Compile();
    //    });
    //}
    //public static (Action<IDataParameterCollection, StringBuilder, DbContext, object, string>, Action<StringBuilder, DbContext, object, string>)
    //    BuildUpdateBulkSetWithSqlParametersPart(DbContext dbContext, Type entityType, Type updateObjType, bool isUpdateRowVersion, List<string> onlyFieldNames, List<string> ignoreFieldNames)
    //{
    //    var ormProvider = dbContext.OrmProvider;
    //    var mapProvider = dbContext.MapProvider;
    //    var cacheKey = GetCacheKey(ormProvider.OrmProviderType, dbContext.MapProvider, entityType, updateObjType, GetHashCode(onlyFieldNames), GetHashCode(ignoreFieldNames));
    //    return updateBulkWithCommandInitializerCache.GetOrAdd(cacheKey, f =>
    //    {
    //        //TODO: 这里可以优化
    //        var fieldsSetter = BuildFieldsSqlParametersPart(dbContext, entityType, updateObjType, 4, 1, 2, false, true, isUpdateRowVersion, onlyFieldNames, ignoreFieldNames) as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
    //        var whereSetter = BuildWhereSqlParametersPart(dbContext, entityType, updateObjType, 1, false, true, true, false, " WHERE ") as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
    //        Action<IDataParameterCollection, StringBuilder, DbContext, object, string> firstSqlSetter = (dbParameters, builder, dbContext, updateObj, suffix) =>
    //        {
    //            fieldsSetter.Invoke(dbParameters, builder, dbContext, updateObj, suffix);
    //            whereSetter.Invoke(dbParameters, builder, dbContext, updateObj, suffix);
    //        };
    //        var fieldsSqlSetter = BuildFieldsSqlParametersPart(dbContext, entityType, updateObjType, 4, 2, 2, false, true, isUpdateRowVersion, onlyFieldNames, ignoreFieldNames) as Action<StringBuilder, DbContext, object, string>;
    //        var whereSqlSetter = BuildWhereSqlParametersPart(dbContext, entityType, updateObjType, 2, false, true, true, false, " WHERE ") as Action<StringBuilder, DbContext, object, string>;
    //        Action<StringBuilder, DbContext, object, string> shardingSqlSetter = (builder, dbContext, updateObj, suffix) =>
    //        {
    //            fieldsSqlSetter.Invoke(builder, dbContext, updateObj, suffix);
    //            whereSqlSetter.Invoke(builder, dbContext, updateObj, suffix);
    //        };
    //        return (firstSqlSetter, shardingSqlSetter);
    //    });
    //}

    public static List<MemberInfo> GetMembers(Type entityType)
    {
        return typeMemberInfos.GetOrAdd(entityType, f => entityType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.MemberType == MemberTypes.Property || f.MemberType == MemberTypes.Field).ToList());
    }
    public static Func<object, object[], string> BuildBulkShardingTableNameGetter(DbContext dbContext, Type entityType, Type parameterType)
    {
        var entityMapProvider = dbContext.EntityMapProvider;
        var cacheKey = RepositoryHelper.GetCacheKey(entityMapProvider, dbContext.TableShardingProvider, entityType, parameterType);
        return shardingTableNameBulkGetters.GetOrAdd(cacheKey, f =>
        {
            //字典尽力不要使用此方法，性能较差
            if (typeof(IDictionary<string, object>).IsAssignableFrom(parameterType))
                throw new NotSupportedException("使用字典类型参数时，请使用.UseTable<TParameter>(Func<string, TParameter, string> tableNameGetter)方法性能高");

            if (!dbContext.TableShardingProvider.TryGetTableSharding(entityType, out var tableShardingInfo))
                throw new Exception($"实体表{entityType.FullName}未配置分表信息");

            var parameterExpr = Expression.Parameter(typeof(object), "f");
            var otherParametersExpr = Expression.Parameter(typeof(object[]), "others");
            var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
            var blockParameters = new List<Expression>() { typedParameterExpr };
            var blockBodies = new List<Expression>();

            var entityMapper = entityMapProvider.GetEntityMap(entityType);
            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
            var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property || f.MemberType == MemberTypes.Field).ToList();

            var index = 0;
            var ruleParameterExprs = new List<Expression>();
            foreach (var memberName in tableShardingInfo.DependOnMembers)
            {
                var memberInfo = memberInfos.Find(f => f.Name == memberName);
                if (memberInfo != null)
                {
                    var memberMapper = entityMapper.GetMemberMap(memberName);
                    var memberType = memberInfo.GetMemberType();
                    var memberValueExpr = Expression.PropertyOrField(typedParameterExpr, memberName);
                    //这里假设参数值与实体成员类型一致，或是对获取分表名无影响的类型                    
                    ruleParameterExprs.Add(memberValueExpr);
                }
                else
                {
                    ruleParameterExprs.Add(Expression.ArrayIndex(otherParametersExpr, Expression.Constant(index)));
                    index++;
                }
            }
            var ruleExpr = Expression.Constant(tableShardingInfo.Rule);
            var orgNameExpr = Expression.Constant(entityMapper.TableName);
            var ruleParametersExpr = Expression.NewArrayInit(typeof(object[]), ruleParameterExprs);
            var bodyExpr = Expression.Invoke(ruleExpr, [orgNameExpr, ruleParametersExpr]);
            return Expression.Lambda<Func<object, object[], string>>(bodyExpr, parameterExpr, otherParametersExpr).Compile();
        });
    }
    public static object CreateInstance(Type targetType)
    {
        var creator = creatorCache.GetOrAdd(targetType, f =>
        {
            var constructor = f.GetConstructor(Type.EmptyTypes);
            return Expression.Lambda<Func<object>>(Expression.New(constructor)).Compile();
        });
        return creator.Invoke();
    }
    public static object CreateInstance(Type targetType, Type[] parameterTypes, params object[] parameters)
    {
        var keyParameterTypes = new List<object> { targetType, "args" };
        keyParameterTypes.AddRange(parameterTypes);
        var cacheKey = GetCacheKey(keyParameterTypes.ToArray());
        var creator = parameterizedCreatorCache.GetOrAdd(cacheKey, f =>
        {
            var parametersExprs = Expression.Parameter(typeof(object[]), "parameters");
            var constructor = targetType.GetConstructor(parameterTypes);
            var argsExprs = new List<Expression>();
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                var type = parameterTypes[i];
                if (type == typeof(object))
                    argsExprs.Add(Expression.ArrayIndex(parametersExprs, Expression.Constant(i)));
                else
                {
                    var paramExpr = Expression.ArrayIndex(parametersExprs, Expression.Constant(i));
                    argsExprs.Add(Expression.Convert(paramExpr, type));
                }
            }
            Expression bodyExpr = Expression.New(constructor, argsExprs);
            if (targetType != typeof(object))
                bodyExpr = Expression.Convert(bodyExpr, typeof(object));
            return Expression.Lambda<Func<object[], object>>(bodyExpr, parametersExprs).Compile();
        });
        return creator.Invoke(parameters);
    }
    public static object ReadList(Type entityType, ITheaDataReader reader, DbContext dbContext)
    {
        var cacheKey = GetCacheKey(entityType, dbContext.OrmProvider.OrmProviderType);
        var typedReaderDeserializer = readerDeserializerGetters.GetOrAdd(cacheKey, f =>
        {
            var readerExpr = Expression.Parameter(typeof(ITheaDataReader), "reader");
            var dbContextExpr = Expression.Parameter(typeof(DbContext), "dbContext");
            var blockBodies = new List<Expression>();
            var methodInfo = typeof(RepositoryHelper).GetMethod(nameof(ReadTypedList));
            methodInfo = methodInfo.MakeGenericMethod(entityType);
            var targetType = typeof(List<>).MakeGenericType(entityType);
            var resultLabelExpr = Expression.Label(targetType);
            blockBodies.Add(Expression.Return(resultLabelExpr, Expression.Call(methodInfo, readerExpr, dbContextExpr)));
            blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(targetType)));
            var delegateType = typeof(Func<,,>).MakeGenericType(typeof(ITheaDataReader), typeof(DbContext), targetType);
            return Expression.Lambda(delegateType, Expression.Block(blockBodies), readerExpr, dbContextExpr).Compile();
        });
        return typedReaderDeserializer.DynamicInvoke(reader, dbContext);
    }
    public static Task<object> ReadListAsync(Type entityType, ITheaDataReader reader, DbContext dbContext, CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(entityType, dbContext.OrmProvider.OrmProviderType);
        var typedReaderDeserializer = readerDeserializerAsyncGetters.GetOrAdd(cacheKey, f =>
        {
            var readerExpr = Expression.Parameter(typeof(ITheaDataReader), "reader");
            var dbContextExpr = Expression.Parameter(typeof(DbContext), "dbContext");
            var cancellationTokenExpr = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
            var blockBodies = new List<Expression>();
            var methodInfo = typeof(RepositoryHelper).GetMethod(nameof(ReadTypedListAsync));
            methodInfo = methodInfo.MakeGenericMethod(entityType);
            var listType = typeof(List<>).MakeGenericType(entityType);
            var targetType = typeof(Task<>).MakeGenericType(listType);
            var resultLabelExpr = Expression.Label(targetType);
            blockBodies.Add(Expression.Return(resultLabelExpr, Expression.Call(methodInfo, readerExpr, dbContextExpr, cancellationTokenExpr)));
            blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(targetType)));
            var delegateType = typeof(Func<,,,>).MakeGenericType(typeof(ITheaDataReader), typeof(DbContext), typeof(CancellationToken), targetType);
            return Expression.Lambda(delegateType, Expression.Block(blockBodies), readerExpr, dbContextExpr, cancellationTokenExpr).Compile();
        });
        return (Task<object>)typedReaderDeserializer.DynamicInvoke(reader, cancellationToken);
    }
    public static List<TEntity> ReadTypedList<TEntity>(this ITheaDataReader reader, DbContext dbContext)
    {
        var result = new List<TEntity>();
        var entityType = typeof(TEntity);
        var deserializer = reader.GetReaderDeserializer(entityType, dbContext);
        while (reader.Read())
            result.Add((TEntity)deserializer.Invoke(reader));
        return result;
    }
    public static async Task<List<TEntity>> ReadTypedListAsync<TEntity>(this ITheaDataReader reader, DbContext dbContext, CancellationToken cancellationToken)
    {
        var result = new List<TEntity>();
        var entityType = typeof(TEntity);
        var deserializer = reader.GetReaderDeserializer(entityType, dbContext);
        while (await reader.ReadAsync(cancellationToken))
            result.Add((TEntity)deserializer.Invoke(reader));
        return result;
    }

    public static Func<object, object> GetMemberValueGetter(MemberInfo memberInfo)
    {
        var entityType = memberInfo.DeclaringType;
        var cacheKey = RepositoryHelper.GetCacheKey(entityType, memberInfo);
        return memberGetterCache.GetOrAdd(cacheKey, f =>
        {
            Expression valueExpr;
            var objExpr = Expression.Parameter(typeof(object), "obj");
            if (memberInfo is FieldInfo fieldInfo)
            {
                if (fieldInfo.IsStatic) valueExpr = Expression.Field(null, fieldInfo);
                else
                {
                    var typedObjExpr = Expression.Convert(objExpr, entityType);
                    valueExpr = Expression.Field(typedObjExpr, fieldInfo);
                }
            }
            else if (memberInfo is PropertyInfo propertyInfo)
            {
                var methodInfo = propertyInfo.GetGetMethod();
                if (methodInfo.IsStatic) valueExpr = Expression.Call(methodInfo);
                else
                {
                    var typedObjExpr = Expression.Convert(objExpr, entityType);
                    valueExpr = Expression.Call(typedObjExpr, methodInfo);
                }
            }
            else throw new NotSupportedException("不支持的成员访问");
            if (valueExpr.Type != typeof(object))
                valueExpr = Expression.Convert(valueExpr, typeof(object));
            return Expression.Lambda<Func<object, object>>(valueExpr, objExpr).Compile();
        });
    }
    public static Action<object, object> GetMemberValueSetter(MemberInfo memberInfo)
    {
        var type = memberInfo.DeclaringType;
        var cacheKey = RepositoryHelper.GetCacheKey(type, memberInfo);
        return memberSetterCache.GetOrAdd(cacheKey, f =>
        {
            Expression bodyExpr = null;
            var objExpr = Expression.Parameter(typeof(object), "obj");
            var valueExpr = Expression.Parameter(typeof(object), "value");
            if (memberInfo is FieldInfo fieldInfo)
            {
                var typedValueExpr = Expression.Convert(valueExpr, fieldInfo.FieldType);
                if (fieldInfo.IsStatic)
                    bodyExpr = Expression.Assign(Expression.Field(null, fieldInfo), typedValueExpr);
                else
                {
                    var typedObjExpr = Expression.Convert(objExpr, type);
                    bodyExpr = Expression.Assign(Expression.Field(typedObjExpr, fieldInfo), typedValueExpr);
                }
            }
            else if (memberInfo is PropertyInfo propertyInfo)
            {
                var methodInfo = propertyInfo.GetSetMethod();
                var typedValueExpr = Expression.Convert(valueExpr, propertyInfo.PropertyType);
                if (methodInfo.IsStatic)
                    bodyExpr = Expression.Call(methodInfo, typedValueExpr);
                else
                {
                    var typedObjExpr = Expression.Convert(objExpr, type);
                    bodyExpr = Expression.Call(typedObjExpr, methodInfo, typedValueExpr);
                }
            }
            else throw new NotSupportedException("不支持的成员访问");
            return Expression.Lambda<Action<object, object>>(bodyExpr, objExpr, valueExpr).Compile();
        });
    }
    public static bool TryGetMemberGetter(Type entityType, string memberName, object targetSample, out Func<object, object> memberGetter)
    {
        var isContains = false;
        memberGetter = null;
        if (targetSample is IDictionary<string, object> dict)
        {
            foreach (var dictKey in dict.Keys)
            {
                if (dictKey.ToLower() != memberName)
                    continue;
                isContains = true;
                memberGetter = f =>
                {
                    var myDict = f as IDictionary<string, object>;
                    return myDict[dictKey];
                };
                break;
            }
        }

        var memberInfos = entityType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.MemberType == MemberTypes.Property || f.MemberType == MemberTypes.Field).ToList();
        foreach (var memberInfo in memberInfos)
        {
            if (memberInfo.Name.ToLower() != memberName)
                continue;
            isContains = true;
            memberGetter = f => FasterEvaluator.EvaluateAndCache(f, memberInfo);
            break;
        }
        return isContains;
    }
    public static Dictionary<string, List<object>> SplitShardingParameters(TableShardingInfo tableShardingInfo, EntityMap entityMapper, Type insertObjType, IEnumerable insertObjs, object insertObjSample, Func<object, string> jsonHandler)
    {
        var result = new Dictionary<string, List<object>>();
        var origTableName = entityMapper.TableName;

        //根据依赖的字段值执行分表规则委托获取分表名
        if (tableShardingInfo.DependOnMembers == null || tableShardingInfo.DependOnMembers.Count == 0)
            throw new InvalidOperationException($"实体表{tableShardingInfo.EntityType.FullName}已设置分表，但未指定分表名，也未指定依赖的成员，无法确定分表，原表名：{origTableName}");

        Func<object, string> tableNameGetter = null;
        if (tableShardingInfo.DependOnMembers.Count > 1)
        {
            var fieldValueGetters = new List<Func<object, object>>();
            foreach (var memberName in tableShardingInfo.DependOnMembers)
            {
                if (!TryGetMemberGetter(insertObjType, memberName.ToLower(), insertObjSample, out var memberGetter))
                    throw new InvalidOperationException($"实体表{tableShardingInfo.EntityType.FullName}已设置分表，依赖的成员{memberName}在插入对象类型{insertObjType.FullName}中不存在，无法确定分表，原表名：{origTableName}");
                fieldValueGetters.Add(memberGetter);
            }
            tableNameGetter = insertObj =>
            {
                var fieldValus = new List<object>();
                foreach (var fieldValueGetter in fieldValueGetters)
                    fieldValus.Add(fieldValueGetter.Invoke(insertObj));
                return tableShardingInfo.Rule.Invoke(origTableName, fieldValus.ToArray()) as string;
            };
        }
        else
        {
            var memberName = tableShardingInfo.DependOnMembers[0].ToLower();
            if (!TryGetMemberGetter(insertObjType, memberName.ToLower(), insertObjSample, out var memberGetter))
                throw new InvalidOperationException($"实体表{tableShardingInfo.EntityType.FullName}已设置分表，依赖的成员{memberName}在插入对象类型{insertObjType.FullName}中不存在，无法确定分表，原表名：{origTableName}");
            tableNameGetter = insertObj => tableShardingInfo.Rule.Invoke(origTableName, [memberGetter.Invoke(insertObj)]);
        }

        foreach (var insertObj in insertObjs)
        {
            var tableName = tableNameGetter.Invoke(insertObj);
            if (string.IsNullOrEmpty(tableName))
                throw new InvalidOperationException($"分表规则无法获取分表名，原表名：{origTableName}，当前参数：{jsonHandler(insertObj)}");
            if (!result.TryGetValue(tableName, out var myParameters))
                result.Add(tableName, myParameters = new List<object>());
            myParameters.Add(insertObj);
        }
        return result;
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

    public static int GetCacheKey(object parameter)
    {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return HashCode.Combine(parameter);
#else
        int hashCode = 17;
        unchecked
        {
            hashCode = hashCode * 23 + parameter.GetHashCode();
        }
        return hashCode;
#endif
    }
    public static int GetCacheKey(object parameter1, object parameter2)
    {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return HashCode.Combine(parameter1, parameter2);
#else
        int hashCode = 17;
        unchecked
        {
            hashCode = hashCode * 23 + parameter1.GetHashCode();
            hashCode = hashCode * 23 + parameter2.GetHashCode();
        }
        return hashCode;
#endif
    }
    public static int GetCacheKey(object parameter1, object parameter2, object parameter3)
    {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return HashCode.Combine(parameter1, parameter2, parameter3);
#else
        int hashCode = 17;
        unchecked
        {
            hashCode = hashCode * 23 + parameter1.GetHashCode();
            hashCode = hashCode * 23 + parameter2.GetHashCode();
            hashCode = hashCode * 23 + parameter3.GetHashCode();
        }
        return hashCode;
#endif
    }
    public static int GetCacheKey(object parameter1, object parameter2, object parameter3, object parameter4)
    {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return HashCode.Combine(parameter1, parameter2, parameter3, parameter4);
#else
        int hashCode = 17;
        unchecked
        {
            hashCode = hashCode * 23 + parameter1.GetHashCode();
            hashCode = hashCode * 23 + parameter2.GetHashCode();
            hashCode = hashCode * 23 + parameter3.GetHashCode();
            hashCode = hashCode * 23 + parameter4.GetHashCode();
        }
        return hashCode;
#endif
    }
    public static int GetCacheKey(object parameter1, object parameter2, object parameter3, object parameter4, object parameter5)
    {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return HashCode.Combine(parameter1, parameter2, parameter3, parameter4, parameter5);
#else
        int hashCode = 17;
        unchecked
        {
            hashCode = hashCode * 23 + parameter1.GetHashCode();
            hashCode = hashCode * 23 + parameter2.GetHashCode();
            hashCode = hashCode * 23 + parameter3.GetHashCode();
            hashCode = hashCode * 23 + parameter4.GetHashCode();
            hashCode = hashCode * 23 + parameter5.GetHashCode();
        }
        return hashCode;
#endif
    }
    public static int GetCacheKey(object parameter1, object parameter2, object parameter3, object parameter4, object parameter5, object parameter6)
    {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return HashCode.Combine(parameter1, parameter2, parameter3, parameter4, parameter5, parameter6);
#else
        int hashCode = 17;
        unchecked
        {
            hashCode = hashCode * 23 + parameter1.GetHashCode();
            hashCode = hashCode * 23 + parameter2.GetHashCode();
            hashCode = hashCode * 23 + parameter3.GetHashCode();
            hashCode = hashCode * 23 + parameter4.GetHashCode();
            hashCode = hashCode * 23 + parameter5.GetHashCode();
            hashCode = hashCode * 23 + parameter6.GetHashCode();
        }
        return hashCode;
#endif
    }
    public static int GetCacheKey(object parameter1, object parameter2, object parameter3, object parameter4, object parameter5, object parameter6, object parameter7)
    {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return HashCode.Combine(parameter1, parameter2, parameter3, parameter4, parameter5, parameter6, parameter7);
#else
        int hashCode = 17;
        unchecked
        {
            hashCode = hashCode * 23 + parameter1.GetHashCode();
            hashCode = hashCode * 23 + parameter2.GetHashCode();
            hashCode = hashCode * 23 + parameter3.GetHashCode();
            hashCode = hashCode * 23 + parameter4.GetHashCode();
            hashCode = hashCode * 23 + parameter5.GetHashCode();
            hashCode = hashCode * 23 + parameter6.GetHashCode();
            hashCode = hashCode * 23 + parameter7.GetHashCode();
        }
        return hashCode;
#endif
    }
    public static int GetCacheKey(object parameter1, object parameter2, object parameter3, object parameter4, object parameter5, object parameter6, object parameter7, object parameter8)
    {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return HashCode.Combine(parameter1, parameter2, parameter3, parameter4, parameter5, parameter6, parameter7, parameter8);
#else
        int hashCode = 17;
        unchecked
        {
            hashCode = hashCode * 23 + parameter1.GetHashCode();
            hashCode = hashCode * 23 + parameter2.GetHashCode();
            hashCode = hashCode * 23 + parameter3.GetHashCode();
            hashCode = hashCode * 23 + parameter4.GetHashCode();
            hashCode = hashCode * 23 + parameter5.GetHashCode();
            hashCode = hashCode * 23 + parameter6.GetHashCode();
            hashCode = hashCode * 23 + parameter7.GetHashCode();
            hashCode = hashCode * 23 + parameter8.GetHashCode();
        }
        return hashCode;
#endif
    }
    private static object GetHashValue(List<string> strValues)
    {
        if (strValues == null || strValues.Count == 0)
            return 0;
        return string.Join("-", strValues);
    }
    public static Func<ITheaDataReader, object> CreateReaderValueTupleDeserializer(Type entityType, DbContext dbContext, ITheaDataReader reader)
    {
        var readerExpr = Expression.Parameter(typeof(ITheaDataReader), "reader");
        var ormProviderExpr = Expression.Constant(dbContext.OrmProvider);
        var index = 0;
        var target = NewBuildInfo(entityType);
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();
        while (index < reader.FieldCount)
        {
            //使用原始SQL才有可能SQL中的字段名与成员名不一致，或是没有加 AS成员名
            var fieldType = reader.GetFieldType(index);
            var memberInfo = entityType.GetMember($"Item{index + 1}")[0];
            var memberType = memberInfo.GetMemberType();
            var readerValueExpr = GetReaderValue(dbContext, ormProviderExpr, readerExpr,
                Expression.Constant(index), memberType, fieldType, null, blockParameters, blockBodies);
            target.Arguments.Add(readerValueExpr);
            index++;
        }
        var resultLabelExpr = Expression.Label(typeof(object));
        Expression returnExpr = Expression.New(target.Constructor, target.Arguments);
        returnExpr = Expression.Convert(returnExpr, typeof(object));

        blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(object))));
        return Expression.Lambda<Func<ITheaDataReader, object>>(Expression.Block(blockParameters, blockBodies), readerExpr).Compile();
    }
    public static Func<ITheaDataReader, object> CreateReaderEntityDeserializer(Type entityType, DbContext dbContext, ITheaDataReader reader)
    {
        var readerExpr = Expression.Parameter(typeof(ITheaDataReader), "reader");
        var ormProviderExpr = Expression.Constant(dbContext.OrmProvider);
        var memberInfos = entityType.GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(f => f.CanWrite()).ToList();
        var entityMapProvider = dbContext.EntityMapProvider;
        var hasMapper = entityMapProvider.TryGetEntityMap(entityType, out var entityMapper);
        var index = 0;
        var target = NewBuildInfo(entityType);
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();

        while (index < reader.FieldCount)
        {
            var memberName = reader.GetName(index);
            //使用原始SQL才有可能SQL中的字段名与成员名不一致，或是没有加 AS成员名
            MemberInfo memberInfo = null;
            ITypeHandler typeHandler = null;
            if (hasMapper && entityMapper.TryGetMemberMap(memberName, out var memberMapper))
            {
                memberInfo = memberMapper.Member;
                typeHandler = memberMapper.TypeHandler;
            }
            else if (!entityMapProvider.TryMapMember(memberName, memberInfos, out memberInfo))
                throw new Exception($"SQL中字段{memberName}映射不到模型{entityType.FullName}任何栏位,或者没有添加AS子句");

            var fieldType = reader.GetFieldType(index);
            var readerValueExpr = GetReaderValue(dbContext, ormProviderExpr, readerExpr, Expression.Constant(index),
                memberInfo.GetMemberType(), fieldType, typeHandler, blockParameters, blockBodies);

            if (!target.IsDefault)
                target.Arguments.Add(readerValueExpr);
            else if (memberInfo.CanWrite())
                target.Bindings.Add(Expression.Bind(memberInfo, readerValueExpr));
            index++;
        }
        var resultLabelExpr = Expression.Label(typeof(object));
        Expression returnExpr;
        if (target.IsDefault) returnExpr = Expression.MemberInit(Expression.New(target.Constructor), target.Bindings);
        else returnExpr = Expression.New(target.Constructor, target.Arguments);
        returnExpr = Expression.Convert(returnExpr, typeof(object));

        blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(object))));
        return Expression.Lambda<Func<ITheaDataReader, object>>(Expression.Block(blockParameters, blockBodies), readerExpr).Compile();
    }
    public static Func<ITheaDataReader, object> CreateReaderDeferredValueDeserializer(Type valueType, DbContext dbContext, ITheaDataReader reader, List<SqlFieldSegment> readerFields)
    {
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();
        var readerExpr = Expression.Parameter(typeof(ITheaDataReader), "reader");
        var ormProviderExpr = Expression.Constant(dbContext.OrmProvider);

        var readerField = readerFields[0];
        var visitor = new ReplaceMemberVisitor();
        var bodyExpr = visitor.Visit(readerField.DeferredExpression);

        var fieldType = reader.GetFieldType(0);
        //延迟的方法调用，有字段值作为方法参数就读取，没有什么也不做
        var childReaderField = readerField.Fields[0];
        var readerValueExpr = GetReaderValue(dbContext, ormProviderExpr, readerExpr, Expression.Constant(0),
            childReaderField.SegmentType, fieldType, childReaderField.TypeHandler, blockParameters, blockBodies);
        var executeExpr = Expression.Invoke(Expression.Lambda(bodyExpr, visitor.NewParameters), readerValueExpr);

        var resultLabelExpr = Expression.Label(typeof(object));
        var returnExpr = Expression.Convert(executeExpr, typeof(object));
        blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(object))));
        return Expression.Lambda<Func<ITheaDataReader, object>>(Expression.Block(blockParameters, blockBodies), readerExpr).Compile();
    }
    public static Func<ITheaDataReader, object> CreateReaderEntityDeserializer(Type entityType, DbContext dbContext, ITheaDataReader reader, List<SqlFieldSegment> readerFields)
    {
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();
        var readerExpr = Expression.Parameter(typeof(ITheaDataReader), "reader");
        var ormProviderExpr = Expression.Constant(dbContext.OrmProvider);

        //IDataReader的索引，readerFields的索引
        int index = 0, readerIndex = 0;
        var root = NewBuildInfo(entityType);
        var current = root;
        var parent = root;
        var readerBuilders = new Dictionary<SqlFieldSegment, EntityBuildInfo>();
        var deferredBuilds = new Stack<EntityBuildInfo>();

        if (readerFields.Count == 1 && readerFields[0].FieldType == SqlFieldType.RawSql)
        {
            var memberInfos = entityType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.CanWrite()).ToList();

            if (!root.IsDefault)
                throw new NotSupportedException($"不支持使用原始SQL创建没有默认构造函数的实体，实体类型:{entityType.FullName}");

            while (index < reader.FieldCount)
            {
                var fieldName = reader.GetName(index);
                if (!memberInfos.TryFind(fieldName, out var memberInfo))
                    continue;
                if (!memberInfo.CanWrite()) continue;
                Expression readerValueExpr = null;
                var fieldType = reader.GetFieldType(index);

                var indexExpr = Expression.Constant(index);
                readerValueExpr = GetReaderValue(dbContext, ormProviderExpr, readerExpr, indexExpr, memberInfo.GetMemberType(), fieldType, null, blockParameters, blockBodies);
                if (!root.IsDefault)
                    root.Arguments.Add(readerValueExpr);
                else if (memberInfo.CanWrite())
                    root.Bindings.Add(Expression.Bind(memberInfo, readerValueExpr));
                index++;
            }
        }
        else
        {
            while (readerIndex < readerFields.Count)
            {
                var readerField = readerFields[readerIndex];
                //readerFields个数与IDataReader返回的Field个数不一致的场景，readerFields是根据Type类型来生成的，
                //SQL语句也不是生成的，通常是原始SQL，才会走到此处
                if (index >= reader.FieldCount && readerField.FieldType != SqlFieldType.IncludeRef)
                {
                    var readerValueExpr = Expression.Default(readerField.TargetMember.GetMemberType());
                    if (!root.IsDefault) root.Arguments.Add(readerValueExpr);
                    readerIndex++;
                    continue;
                }
                if (readerField.FieldType == SqlFieldType.Field)
                {
                    var fieldType = reader.GetFieldType(index);
                    var readerValueExpr = GetReaderValue(dbContext, ormProviderExpr, readerExpr, Expression.Constant(index),
                        readerField.SegmentType, fieldType, readerField.TypeHandler, blockParameters, blockBodies);
                    if (!root.IsDefault) root.Arguments.Add(readerValueExpr);
                    else if (readerField.TargetMember.CanWrite()) root.Bindings.Add(Expression.Bind(readerField.TargetMember, readerValueExpr));
                    index++;
                }
                else
                {
                    Expression readerValueExpr = null;
                    SqlFieldSegment childReaderField = null;
                    var childIndex = 0;
                    var endIndex = index;
                    //当无参数的Deferred函数调用，ReaderFields的值为null，也没有从数据库读取字段，count=0
                    if (readerField.Fields != null)
                        endIndex += readerField.Fields.Count;

                    //支持延迟方法调用、属性访问，一切均可延迟，但必须最后调用Deferred()方法
                    if (readerField.FieldType == SqlFieldType.DeferredFields)
                    {
                        if (readerField.SegmentType.IsEntityType(out _))
                        {
                            current = NewBuildInfo(readerField.SegmentType, readerField.TargetMember, parent);
                            readerBuilders.Add(readerField, current);
                        }
                        Expression bodyExpr = readerField.DeferredExpression;
                        //$"{f.OrderNo} : {f.TotalAmount.ToString("C")}"
                        //f.TotalAmount.ToString("C")
                        //"TotalAmount: " + (f.Price * f.Quantity).ToString("C")
                        //this.DeferredInvoke(f.Price, f.Quantity)
                        //new DateTimeOffset(DateTime.SpecifyKind(f.DateTimeField, DateTimeKind.Local)).UtcDateTime.Deferred()
                        Expression executeExpr = null;
                        if (readerField.Fields != null && readerField.Fields.Count > 0)
                        {
                            var visitor = new ReplaceMemberVisitor();
                            bodyExpr = visitor.Visit(readerField.DeferredExpression);
                            var argsExprs = new List<Expression>();
                            while (index < endIndex)
                            {
                                var fieldType = reader.GetFieldType(index);
                                //延迟的方法调用，有字段值作为方法参数就读取，没有什么也不做
                                childReaderField = readerField.Fields[childIndex];
                                readerValueExpr = GetReaderValue(dbContext, ormProviderExpr, readerExpr, Expression.Constant(index),
                                    childReaderField.SegmentType, fieldType, childReaderField.TypeHandler, blockParameters, blockBodies);
                                argsExprs.Add(readerValueExpr);
                                childIndex++;
                                index++;
                            }
                            executeExpr = Expression.Invoke(Expression.Lambda(bodyExpr, visitor.NewParameters), argsExprs);
                        }
                        else executeExpr = Expression.Invoke(Expression.Lambda(bodyExpr));
                        //把延迟方法调用委托当作参数传进来，这样缓存才有效，相同key，不同的延迟方法
                        if (!current.IsDefault) current.Arguments.Add(executeExpr);
                        else if (readerField.TargetMember.CanWrite()) current.Bindings.Add(Expression.Bind(readerField.TargetMember, executeExpr));
                    }
                    else if (readerField.FieldType == SqlFieldType.IncludeRef)
                    {
                        //Include导航属性引用不能单独Select，前面一定有Parameter访问
                        //Include导航属性引用单独处理，先设置默认值，在整个实体初始化完后，再设置具体值，初始化Action在成员访问的时候，已经构建好了
                        var refReaderField = readerField.Value as SqlFieldSegment;
                        var instanceExpr = readerBuilders[refReaderField].InstanceExpr;
                        //此处生成的副本，从新new的一个对象
                        if (!parent.IsDefault) parent.Arguments.Add(instanceExpr);
                        else if (readerField.TargetMember.CanWrite()) parent.Bindings.Add(Expression.Bind(readerField.TargetMember, instanceExpr));
                        readerIndex++;
                        continue;
                    }
                    else
                    {
                        //默认是目标类型，并且也只有第一个ReaderField才是目标类型
                        if (!readerField.IsTargetType)
                        {
                            if (readerField.Parent != null)
                                parent = readerBuilders[readerField.Parent];
                            else parent = root;
                            current = NewBuildInfo(readerField.SegmentType, readerField.TargetMember, parent);
                        }
                        while (index < endIndex)
                        {
                            var fieldType = reader.GetFieldType(index);
                            childReaderField = readerField.Fields[childIndex];
                            readerValueExpr = GetReaderValue(dbContext, ormProviderExpr, readerExpr, Expression.Constant(index),
                                childReaderField.SegmentType, fieldType, childReaderField.TypeHandler, blockParameters, blockBodies);

                            if (!current.IsDefault) current.Arguments.Add(readerValueExpr);
                            else if (childReaderField.TargetMember.CanWrite()) current.Bindings.Add(Expression.Bind(childReaderField.TargetMember, readerValueExpr));
                            childIndex++;
                            index++;
                        }

                        //有include对象
                        if (readerField.HasNextInclude)
                        {
                            deferredBuilds.Push(current);
                            readerBuilders.Add(readerField, current);
                        }
                        else
                        {
                            do
                            {
                                //创建子对象，并赋值给父对象的属性,直到Select语句
                                Expression instanceExpr = null;
                                if (current.IsDefault)
                                    instanceExpr = Expression.MemberInit(Expression.New(current.Constructor), current.Bindings);
                                else instanceExpr = Expression.New(current.Constructor, current.Arguments);
                                current.InstanceExpr = instanceExpr;
                                //赋值给父对象的属性
                                if (current.Parent == null)
                                    break;
                                if (!current.Parent.IsDefault) current.Parent.Arguments.Add(instanceExpr);
                                else if (current.FromMember.CanWrite()) current.Parent.Bindings.Add(Expression.Bind(current.FromMember, instanceExpr));
                            }
                            while (deferredBuilds.TryPop(out current));
                        }
                    }
                }
                readerIndex++;
            }
        }

        var resultLabelExpr = Expression.Label(typeof(object));
        Expression returnExpr = null;
        if (root.IsDefault)
            returnExpr = Expression.MemberInit(Expression.New(root.Constructor), root.Bindings);
        else returnExpr = Expression.New(root.Constructor, root.Arguments);
        returnExpr = Expression.Convert(returnExpr, typeof(object));

        blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(object))));
        return Expression.Lambda<Func<ITheaDataReader, object>>(Expression.Block(blockParameters, blockBodies), readerExpr).Compile();
    }
    public static Expression GetReaderValue(DbContext dbContext, Expression ormProviderExpr, ParameterExpression readerExpr, Expression indexExpr, Type targetType, Type fieldType, ITypeHandler typeHandler, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        var methodInfo = typeof(ITheaDataReader).GetMethod(nameof(ITheaDataReader.GetValue), [typeof(int)]);
        var readerValueExpr = AssignLocalParameter(typeof(object), Expression.Call(readerExpr, methodInfo, indexExpr), blockParameters, blockBodies);
        var isNullable = targetType.IsNullableType(out var underlyingType);
        Expression targetValueExpr = null;
        if (typeHandler != null)
        {
            methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.Parse), [typeof(IOrmProvider), typeof(Type), typeof(object)]);
            var typeHandlerExpr = Expression.Constant(typeHandler);
            var underlyingTypeExpr = Expression.Constant(underlyingType);
            targetValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, underlyingTypeExpr, readerValueExpr);
        }
        else
        {
            var valueGetter = dbContext.OrmProvider.GetReaderValueGetter(targetType, fieldType, dbContext);
            targetValueExpr = Expression.Invoke(Expression.Constant(valueGetter), readerValueExpr);
        }
        blockBodies.Add(Expression.Assign(readerValueExpr, targetValueExpr));
        return Expression.Convert(readerValueExpr, targetType);
    }
    private static EntityBuildInfo NewBuildInfo(Type targetType, MemberInfo fromMember = null, EntityBuildInfo parent = null)
    {
        bool isDefaultCtor = false;
        List<MemberBinding> bindings = null;
        List<Expression> ctorArguments = null;

        var ctor = targetType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        if (ctor != null)
        {
            bindings = new List<MemberBinding>();
            isDefaultCtor = true;
        }
        else
        {
            ctor = targetType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(f => f.IsPublic ? 0 : (f.IsPrivate ? 2 : 1)).First();
            ctorArguments = new List<Expression>();
        }
        return new EntityBuildInfo
        {
            IsDefault = isDefaultCtor,
            Constructor = ctor,
            Bindings = bindings,
            Arguments = ctorArguments,
            FromMember = fromMember,
            Parent = parent
        };
    }
    private static ParameterExpression AssignLocalParameter(Type type, Expression valueExpr, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        var objLocalExpr = Expression.Variable(type, $"local{blockParameters.Count}");
        blockParameters.Add(objLocalExpr);
        blockBodies.Add(Expression.Assign(objLocalExpr, valueExpr));
        return objLocalExpr;
    }
    class EntityBuildInfo
    {
        public bool IsDefault { get; set; }
        public ConstructorInfo Constructor { get; set; }
        public List<MemberBinding> Bindings { get; set; }
        public List<Expression> Arguments { get; set; }
        public MemberInfo FromMember { get; set; }
        public EntityBuildInfo Parent { get; set; }
        public Expression InstanceExpr { get; set; }
    }
    struct BulkCommandParametersCache
    {
        public Func<object, object> ValueGetter { get; set; }
        public Action<IDataParameterCollection, StringBuilder, object, string> ParametersSetter { get; set; }
    }
}