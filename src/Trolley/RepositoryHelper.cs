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
    private static readonly ConcurrentDictionary<int, Func<IDataParameterCollection, DbContext, object, string>> whereByCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<IDataParameterCollection, DbContext, object, string>> whereByIdCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<IDataParameterCollection, DbContext, object, string>> whereByIdsCommandInitializerCache = new();

    private static readonly ConcurrentDictionary<int, object> createWithCommandInitializerCache = new();
    private static readonly ConcurrentDictionary<int, object> updateWithCommandInitializerCache = new();

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

    public static Func<IDataParameterCollection, DbContext, object, string> BuildWhereCommandInitializer(DbContext dbContext, Type entityType, object whereObjs, int commandType, bool isUseKey, bool isMultiple, bool isBulk)
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
        bool hasWhere = whereObjs != null;
        whereObjType ??= entityType;

        var cacheKey = GetCacheKey(dbContext.OrmProvider.OrmProviderType, dbContext.EntityMapProvider, entityType, whereObjType, isMultiple);
        var commandInitializerCache = commandType switch
        {
            1 => isBulk ? queryByIdsCommandInitializerCache : isUseKey ? queryByIdCommandInitializerCache : queryByCommandInitializerCache,
            2 => isBulk ? existsByIdsCommandInitializerCache : isUseKey ? existsByIdCommandInitializerCache : existsByCommandInitializerCache,
            3 => isBulk ? deleteByIdsCommandInitializerCache : isUseKey ? deleteByIdCommandInitializerCache : deleteByCommandInitializerCache,
            4 => isBulk ? whereByIdsCommandInitializerCache : isUseKey ? whereByIdCommandInitializerCache : whereByCommandInitializerCache,
            _ => throw new NotSupportedException($"不支持的命令类型:{commandType}"),
        };
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            string headSql = null, tailSql = null;
            var entityMapper = dbContext.EntityMapProvider.GetEntityMap(entityType);
            switch (commandType)
            {
                case 1:
                    var fieldSql = BuildSelectFieldsSqlPart(dbContext, entityMapper, entityType);
                    headSql = $"SELECT {fieldSql}";
                    break;
                case 2:
                    headSql = "SELECT 1";
                    tailSql = " LIMIT 1";
                    break;
                case 3:
                    headSql = "DELETE";
                    break;
            }
            if (!hasWhere && commandType < 4)
            {
                var ormProvider = dbContext.OrmProvider;
                headSql += $" FROM {ormProvider.GetTableName(entityMapper.TableName)}";
                if (commandType == 2) headSql += tailSql;
                return (dbParameters, dbContext, parameters) => headSql;
            }
            var commandInitializer = BuildWhereObjsCommandInitializer(dbContext, entityType, whereObjType, isUseKey, false, isMultiple, isBulk, headSql, tailSql);
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
    private static object BuildWhereObjsCommandInitializer(DbContext dbContext, Type entityType,
        Type whereObjType, bool isUseKey, bool isWithKey, bool isMultiple, bool isBulk, string headSql, string tailSql)
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

        string fixedHeadSql = null;
        if (!string.IsNullOrEmpty(headSql))
        {
            string tableName = ormProvider.GetTableName(entityMapper.TableName);
            fixedHeadSql = $"{headSql} FROM {tableName} WHERE";
        }
        if (isInExpr) fixedHeadSql += $"{ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)} IN (";
        if (!string.IsNullOrEmpty(fixedHeadSql))
            blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(fixedHeadSql)));

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
}