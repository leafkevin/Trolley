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
    private static ConcurrentDictionary<int, Action<IDataParameterCollection, IOrmProvider, string, object>> addDbParametersCache = new();

    private static ConcurrentDictionary<int, object> queryGetDbParametersInitializerCache = new();
    private static ConcurrentDictionary<int, object> queryMultiGetDbParametersInitializerCache = new();
    private static ConcurrentDictionary<int, object> queryWhereObjDbParametersInitializerCache = new();
    private static ConcurrentDictionary<int, object> queryMultiWhereObjDbParametersInitializerCache = new();
    private static ConcurrentDictionary<int, object> queryExistsDbParametersInitializerCache = new();
    private static ConcurrentDictionary<int, object> queryMultiExistsDbParametersInitializerCache = new();
    private static ConcurrentDictionary<int, Action<IDataParameterCollection, IOrmProvider, object>> queryRawSqlDbParametersInitializerCache = new();

    private static ConcurrentDictionary<int, object> createWithByDbParametersInitializerCache = new();
    private static ConcurrentDictionary<int, object> createMultiWithByDbParametersInitializerCache = new();

    private static ConcurrentDictionary<int, object> deleteDbParametersInitializerCache = new();
    private static ConcurrentDictionary<int, object> deleteBulkDbParametersInitializerCache = new();






    private static ConcurrentDictionary<int, (string, object)> createBulkDbParametersInitializerCache = new();
    private static ConcurrentDictionary<int, (string, object)> createMultiBulkDbParametersInitializerCache = new();


    private static ConcurrentDictionary<int, Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, object, string>> updateDbParametersInitializerCache = new();
    private static ConcurrentDictionary<int, object> updateMultiDbParametersInitializerCache = new();

    private static ConcurrentDictionary<int, Action<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, object, int>> updateBulkDbParametersInitializerCache = new();
    private static ConcurrentDictionary<int, object> updateSetFieldsDbParametersInitializerCache = new();
    private static ConcurrentDictionary<int, object> updateMultiSetFieldsDbParametersInitializerCache = new();
    private static ConcurrentDictionary<int, object> updateWhereWithDbParametersInitializerCache = new();
    private static ConcurrentDictionary<int, object> updateMultiWhereWithDbParametersInitializerCache = new();




    private static ConcurrentDictionary<int, object> whereWithKeysDbParametersInitializerCache = new();
    private static ConcurrentDictionary<int, object> mutilWhereWithKeysDbParametersInitializerCache = new();

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
    //public static void AddMemberParameter(ParameterExpression commandExpr, Expression ormProviderExpr, Expression parameterNameExpr,
    //    Expression typedParameterExpr, MemberMap memberMapper, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    //{
    //    var dbParametersExpr = Expression.Property(dbParametersExpr, nameof(IDbCommand.Parameters));
    //    var fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
    //    AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
    //}
    public static Action<IDataParameterCollection, IOrmProvider, string, object> BuildAddDbParameters(string dbKey, IOrmProvider ormProvider, MemberMap memberMapper, object fieldVallue)
    {
        var fieldVallueType = fieldVallue.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, memberMapper.Parent.EntityType, memberMapper, fieldVallueType);
        return addDbParametersCache.GetOrAdd(cacheKey, f =>
        {
            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var parameterNameExpr = Expression.Parameter(typeof(string), "parameterName");
            var fieldValueExpr = Expression.Parameter(typeof(object), "fieldValue");

            var typedFieldValueExpr = Expression.Variable(fieldVallueType, "typedFieldValue");
            var blockParameters = new List<ParameterExpression> { typedFieldValueExpr };
            var blockBodies = new List<Expression>();
            blockBodies.Add(Expression.Assign(typedFieldValueExpr, Expression.Convert(fieldValueExpr, fieldVallueType)));

            RepositoryHelper.AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
            return Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, string, object>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr).Compile();
        });
    }
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
    public static object BuildWhereSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiQuery)
    {
        object dbParametersInitializer = null;
        if (whereObj is IDictionary<string, object>)
        {
            if (isMultiQuery)
            {
                Action<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, string, object> typedDbParametersInitializer = null;
                typedDbParametersInitializer = (dbParameters, ormProvider, mapProvider, builder, multiMark, parameter) =>
                {
                    int index = 0;
                    var entityMapper = mapProvider.GetEntityMap(entityType);
                    var dict = parameter as IDictionary<string, object>;

                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        var parameterName = ormProvider.ParameterPrefix + multiMark + item.Key;
                        if (index > 0) builder.Append(" AND ");
                        builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        var addDbParametersDelegate = BuildAddDbParameters(dbKey, ormProvider, memberMapper, item.Value);
                        addDbParametersDelegate.Invoke(dbParameters, ormProvider, parameterName, item.Value);
                        index++;
                    }
                };
                dbParametersInitializer = typedDbParametersInitializer;
            }
            else
            {
                Action<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, object> typedDbParametersInitializer = null;
                typedDbParametersInitializer = (dbParameters, ormProvider, mapProvider, builder, parameter) =>
                {
                    int index = 0;
                    var entityMapper = mapProvider.GetEntityMap(entityType);
                    var dict = parameter as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        if (index > 0) builder.Append(" AND ");
                        var parameterName = ormProvider.ParameterPrefix + item.Key;
                        builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        var addDbParametersDelegate = BuildAddDbParameters(dbKey, ormProvider, memberMapper, item.Value);
                        addDbParametersDelegate.Invoke(dbParameters, ormProvider, parameterName, item.Value);
                        index++;
                    }
                };
                dbParametersInitializer = typedDbParametersInitializer;
            }
        }
        else
        {
            var whereObjType = whereObj.GetType();
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var memberInfos = whereObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var mapProviderExpr = Expression.Parameter(typeof(IEntityMapProvider), "mapProvider");
            var builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
            var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

            ParameterExpression multiMarkExpr = null;
            ParameterExpression parameterNameExpr = null;
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            if (isMultiQuery)
            {
                multiMarkExpr = Expression.Parameter(typeof(string), "multiMark");
                parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                blockParameters.Add(parameterNameExpr);
                var constructor = typeof(StringBuilder).GetConstructor(Type.EmptyTypes);
                blockBodies.Add(Expression.Assign(builderExpr, Expression.New(constructor)));
            }

            var typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");
            blockParameters.Add(typedWhereObjExpr);
            blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));

            var index = 0;
            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });
            foreach (var memberInfo in memberInfos)
            {
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                    || memberMapper.IsIgnore || memberMapper.IsNavigation
                    || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                    continue;

                if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                Expression myParameterNameExpr = null;
                if (isMultiQuery)
                {
                    var concatExpr = Expression.Call(concatMethodInfo,
                        Expression.Constant(ormProvider.ParameterPrefix), multiMarkExpr, Expression.Constant(memberMapper.MemberName));
                    blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));
                    myParameterNameExpr = parameterNameExpr;

                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo,
                        Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}=")));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
                }
                else
                {
                    var parameterName = $"{ormProvider.ParameterPrefix}{memberMapper.MemberName}";
                    myParameterNameExpr = Expression.Constant(parameterName);
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo,
                        Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}")));
                }
                var fieldValueExpr = Expression.PropertyOrField(typedWhereObjExpr, memberMapper.MemberName);
                AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                index++;
            }
            if (isMultiQuery) dbParametersInitializer = Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, string, object>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, mapProviderExpr, builderExpr, multiMarkExpr, whereObjExpr).Compile();
            else dbParametersInitializer = Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, object>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, mapProviderExpr, builderExpr, whereObjExpr).Compile();
        }
        return dbParametersInitializer;
    }
    public static object BuildWhereKeySqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiQuery)
    {
        object dbParametersInitializer = null;
        if (whereObj is IDictionary<string, object>)
        {
            if (isMultiQuery)
            {
                Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, string, object, string> typedDbParametersInitializer = null;
                typedDbParametersInitializer = (dbParameters, ormProvider, mapProvider, builder, multiMark, parameter) =>
                {
                    var index = 0;
                    var entityMapper = mapProvider.GetEntityMap(entityType);
                    var dict = parameter as IDictionary<string, object>;
                    foreach (var keyMapper in entityMapper.KeyMembers)
                    {
                        if (!dict.TryGetValue(keyMapper.MemberName, out var fieldValue))
                            throw new ArgumentNullException(nameof(whereObj), $"字典参数缺少主键字段{keyMapper.MemberName}");

                        if (index > 0) builder.Append(" AND ");
                        var parameterName = $"{ormProvider.ParameterPrefix}{multiMark}{keyMapper.MemberName}";
                        builder.Append($"{ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");
                        var addDbParametersDelegate = BuildAddDbParameters(dbKey, ormProvider, keyMapper, fieldValue);
                        addDbParametersDelegate.Invoke(dbParameters, ormProvider, parameterName, fieldValue);
                        index++;
                    }
                    return builder.ToString();
                };
                dbParametersInitializer = typedDbParametersInitializer;
            }
            else
            {
                Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, object, string> typedDbParametersInitializer = null;
                typedDbParametersInitializer = (dbParameters, ormProvider, mapProvider, builder, parameter) =>
                {
                    var index = 0;
                    var entityMapper = mapProvider.GetEntityMap(entityType);
                    var dict = parameter as IDictionary<string, object>;
                    foreach (var keyMapper in entityMapper.KeyMembers)
                    {
                        if (!dict.TryGetValue(keyMapper.MemberName, out var fieldValue))
                            throw new ArgumentNullException("whereObj", $"字典参数缺少主键字段{keyMapper.MemberName}");

                        if (index > 0) builder.Append(" AND ");
                        var parameterName = $"{ormProvider.ParameterPrefix}{keyMapper.MemberName}";
                        builder.Append($"{ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");
                        var addDbParametersDelegate = BuildAddDbParameters(dbKey, ormProvider, keyMapper, fieldValue);
                        addDbParametersDelegate.Invoke(dbParameters, ormProvider, parameterName, fieldValue);
                        index++;
                    }
                    return builder.ToString();
                };
                dbParametersInitializer = typedDbParametersInitializer;
            }
        }
        else
        {
            var whereObjType = whereObj.GetType();
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var mapProviderExpr = Expression.Parameter(typeof(IEntityMapProvider), "mapProvider");
            var builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
            var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

            ParameterExpression multiMarkExpr = null;
            ParameterExpression typedWhereObjExpr = null;
            ParameterExpression parameterNameExpr = null;
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            bool isEntityType = false;
            List<MemberInfo> memberInfos = null;

            if (isMultiQuery)
            {
                multiMarkExpr = Expression.Parameter(typeof(string), "multiMark");
                parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                blockParameters.Add(parameterNameExpr);
            }
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

            var index = 0;
            var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });
            var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            foreach (var keyMapper in entityMapper.KeyMembers)
            {
                if (isEntityType && !memberInfos.Exists(f => f.Name == keyMapper.MemberName))
                    throw new ArgumentNullException("whereObj", $"参数类型{whereObjType.FullName}缺少主键字段{keyMapper.MemberName}");

                if (index > 0)
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                Expression myParameterNameExpr = null;
                if (isMultiQuery)
                {
                    var concatExpr = Expression.Call(concatMethodInfo,
                        Expression.Constant(ormProvider.ParameterPrefix), multiMarkExpr, Expression.Constant(keyMapper.MemberName));
                    blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant($"{ormProvider.GetFieldName(keyMapper.FieldName)}=")));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
                    myParameterNameExpr = parameterNameExpr;
                }
                else
                {
                    var parameterName = $"{ormProvider.ParameterPrefix}{keyMapper.MemberName}";
                    myParameterNameExpr = Expression.Constant(parameterName);
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant($"{ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}")));
                }
                if (isEntityType)
                {
                    var fieldValueExpr = Expression.PropertyOrField(typedWhereObjExpr, keyMapper.MemberName);
                    AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueExpr, keyMapper, blockParameters, blockBodies);
                }
                else AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, whereObjExpr, keyMapper, blockParameters, blockBodies);
                index++;
            }

            Expression returnExpr = null;
            if (isMultiQuery)
            {
                var methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
                returnExpr = Expression.Call(builderExpr, methodInfo);
            }
            var resultLabelExpr = Expression.Label(typeof(string));
            blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
            blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));

            if (isMultiQuery) dbParametersInitializer = Expression.Lambda<Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, string, object, string>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, mapProviderExpr, builderExpr, multiMarkExpr, whereObjExpr).Compile();
            else dbParametersInitializer = Expression.Lambda<Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, object, string>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, mapProviderExpr, builderExpr, whereObjExpr).Compile();
        }
        return dbParametersInitializer;
    }
    public static object BuildBulkWhereSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiExecute)
    {
        object dbParametersInitializer = null;
        if (whereObj is IDictionary<string, object>)
        {
            if (isMultiExecute)
            {
                Action<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, object, string, int> typedDbParametersInitializer = null;
                typedDbParametersInitializer = (dbParameters, ormProvider, mapProvider, builder, parameter, multiMark, bulkIndex) =>
                {
                    int index = 0;
                    var entityMapper = mapProvider.GetEntityMap(entityType);
                    var dict = parameter as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        var parameterName = $"{ormProvider.ParameterPrefix}{multiMark}{item.Key}{bulkIndex}";
                        if (index > 0) builder.Append(" AND ");
                        builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        var addDbParametersDelegate = BuildAddDbParameters(dbKey, ormProvider, memberMapper, item.Value);
                        addDbParametersDelegate.Invoke(dbParameters, ormProvider, parameterName, item.Value);
                        index++;
                    }
                };
                dbParametersInitializer = typedDbParametersInitializer;
            }
            else
            {
                Action<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, object, int> typedDbParametersInitializer = null;
                typedDbParametersInitializer = (dbParameters, ormProvider, mapProvider, builder, parameter, bulkIndex) =>
                {
                    int index = 0;
                    var entityMapper = mapProvider.GetEntityMap(entityType);
                    var dict = parameter as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        if (index > 0) builder.Append(" AND ");
                        var parameterName = $"{ormProvider.ParameterPrefix}{item.Key}{bulkIndex}";
                        builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        var addDbParametersDelegate = BuildAddDbParameters(dbKey, ormProvider, memberMapper, item.Value);
                        addDbParametersDelegate.Invoke(dbParameters, ormProvider, parameterName, item.Value);
                        index++;
                    }
                };
                dbParametersInitializer = typedDbParametersInitializer;
            }
        }
        else
        {
            var whereObjType = whereObj.GetType();
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var mapProviderExpr = Expression.Parameter(typeof(IEntityMapProvider), "mapProvider");
            var builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
            var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");
            var bulkIndexExpr = Expression.Parameter(typeof(int), "bulkIndex");

            ParameterExpression multiMarkExpr = null;
            ParameterExpression typedWhereObjExpr = null;
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            bool isEntityType = false;
            List<MemberInfo> memberInfos = null;

            if (isMultiExecute) multiMarkExpr = Expression.Parameter(typeof(string), "multiMark");
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

            var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
            var strIndexExpr = Expression.Variable(typeof(string), "strIndex");
            blockParameters.AddRange(new[] { parameterNameExpr, strIndexExpr });
            var toStringMethodInfo = typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes);
            blockBodies.Add(Expression.Assign(strIndexExpr, Expression.Call(bulkIndexExpr, toStringMethodInfo)));

            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
            var index = 0;
            foreach (var keyMapper in entityMapper.KeyMembers)
            {
                if (isEntityType && !memberInfos.Exists(f => f.Name == keyMapper.MemberName))
                    throw new ArgumentNullException("whereObj", $"参数类型{whereObjType.FullName}缺少主键字段{keyMapper.MemberName}");

                if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                var concatExpr = Expression.Constant(ormProvider.ParameterPrefix + keyMapper.MemberName);
                Expression suffixExpr = strIndexExpr;
                if (isMultiExecute) suffixExpr = Expression.Call(concatMethodInfo, multiMarkExpr, strIndexExpr);
                blockBodies.Add(Expression.Assign(parameterNameExpr, Expression.Call(concatMethodInfo, concatExpr, suffixExpr)));

                if (index > 0)
                {
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo,
                        Expression.Constant($"{ormProvider.GetFieldName(keyMapper.FieldName)}=")));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
                    var fieldValueExpr = Expression.PropertyOrField(typedWhereObjExpr, keyMapper.MemberName);
                    AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, keyMapper, blockParameters, blockBodies);
                }
                else
                {
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
                    AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, whereObjExpr, keyMapper, blockParameters, blockBodies);
                }
                index++;
            }
            if (isMultiExecute) dbParametersInitializer = Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, string, object, int>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, mapProviderExpr, builderExpr, multiMarkExpr, whereObjExpr, bulkIndexExpr).Compile();
            else dbParametersInitializer = Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, object, int>>(
                Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, mapProviderExpr, builderExpr, whereObjExpr, bulkIndexExpr).Compile();
        }
        return dbParametersInitializer;
    }


    public static object BuildGetSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiQuery)
    {
        object dbParametersInitializer = null;
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, whereObjType);
        if (isMultiQuery)
        {
            dbParametersInitializer = queryGetDbParametersInitializerCache.GetOrAdd(cacheKey, f =>
            {
                Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, string, object, string> typedDbParametersInitializer = null;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                typedDbParametersInitializer = (dbParameters, ormProvider, mapProvider, multiMark, parameter) =>
                {
                    var whereDbParametersInitializer = BuildWhereKeySqlParameters(dbKey, ormProvider, mapProvider, entityType, whereObj, false);
                    var typedDbParametersInitializer = whereDbParametersInitializer as Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, string, object, string>;
                    var builder = new StringBuilder("SELECT ");
                    builder.Append(BuildFieldsSqlPart(ormProvider, mapProvider, entityType, entityType, true));
                    builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                    return typedDbParametersInitializer.Invoke(dbParameters, ormProvider, mapProvider, builder, multiMark, parameter);
                };
                return typedDbParametersInitializer;
            });
        }
        else
        {
            dbParametersInitializer = queryGetDbParametersInitializerCache.GetOrAdd(cacheKey, f =>
            {
                Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, object, string> typedDbParametersInitializer = null;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                typedDbParametersInitializer = (dbParameters, ormProvider, mapProvider, parameter) =>
                {
                    var whereDbParametersInitializer = BuildWhereKeySqlParameters(dbKey, ormProvider, mapProvider, entityType, whereObj, false);
                    var typedDbParametersInitializer = whereDbParametersInitializer as Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, object, string>;
                    var builder = new StringBuilder("SELECT ");
                    builder.Append(BuildFieldsSqlPart(ormProvider, mapProvider, entityType, entityType, true));
                    builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                    return typedDbParametersInitializer.Invoke(dbParameters, ormProvider, mapProvider, builder, parameter);
                };
                return typedDbParametersInitializer;
            });
        }
        return dbParametersInitializer;
    }
    public static object BuildQueryWhereObjSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiQuery)
    {
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, whereObjType);
        var dbParametersInitializerCache = isMultiQuery ? queryMultiWhereObjDbParametersInitializerCache : queryWhereObjDbParametersInitializerCache;
        return dbParametersInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var whereDbParametersInitializer = BuildWhereSqlParameters(dbKey, ormProvider, mapProvider, entityType, whereObj, isMultiQuery);
            object result = null;
            if (isMultiQuery)
            {
                Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, string, object, string> dbParametersInitializer;
                var typedDbParametersInitializer = whereDbParametersInitializer as Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, string, object, string>;
                dbParametersInitializer = (dbParameters, ormProvider, mapProvider, multiMark, whereObj) =>
                {
                    var builder = new StringBuilder("SELECT ");
                    builder.Append(BuildFieldsSqlPart(ormProvider, mapProvider, entityType, entityType, true));
                    builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                    builder.Append(typedDbParametersInitializer.Invoke(dbParameters, ormProvider, mapProvider, builder, multiMark, whereObj));
                    return builder.ToString();
                };
                result = dbParametersInitializer;
            }
            else
            {
                Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, object, string> dbParametersInitializer;
                var typedDbParametersInitializer = whereDbParametersInitializer as Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, object, string>;
                dbParametersInitializer = (dbParameters, ormProvider, mapProvider, whereObj) =>
                {
                    var builder = new StringBuilder("SELECT ");
                    builder.Append(BuildFieldsSqlPart(ormProvider, mapProvider, entityType, entityType, true));
                    builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                    builder.Append(typedDbParametersInitializer.Invoke(dbParameters, ormProvider, mapProvider, builder, whereObj));
                    return builder.ToString();
                };
                result = dbParametersInitializer;
            }
            return result;
        });
    }
    public static object BuildExistsSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiQuery)
    {
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, whereObjType);
        var dbParametersInitializerCache = isMultiQuery ? queryMultiExistsDbParametersInitializerCache : queryExistsDbParametersInitializerCache;
        return dbParametersInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var whereDbParametersInitializer = BuildWhereSqlParameters(dbKey, ormProvider, mapProvider, entityType, whereObj, isMultiQuery);
            object result = null;
            if (isMultiQuery)
            {
                Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, string, object, string> dbParametersInitializer;
                var typedDbParametersInitializer = whereDbParametersInitializer as Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, string, object, string>;
                dbParametersInitializer = (dbParameters, ormProvider, mapProvider, multiMark, whereObj) =>
                {
                    var builder = new StringBuilder($"SELECT COUNT(1) FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                    builder.Append(typedDbParametersInitializer.Invoke(dbParameters, ormProvider, mapProvider, builder, multiMark, whereObj));
                    return builder.ToString();
                };
                result = dbParametersInitializer;
            }
            else
            {
                Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, object, string> dbParametersInitializer;
                var typedDbParametersInitializer = whereDbParametersInitializer as Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, object, string>;
                dbParametersInitializer = (dbParameters, ormProvider, mapProvider, whereObj) =>
                {
                    var builder = new StringBuilder($"SELECT COUNT(1) FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                    builder.Append(typedDbParametersInitializer.Invoke(dbParameters, ormProvider, mapProvider, builder, whereObj));
                    return builder.ToString();
                };
                result = dbParametersInitializer;
            }
            return result;
        });
    }
    public static Action<IDataParameterCollection, IOrmProvider, object> BuildQueryRawSqlParameters(string dbKey, IOrmProvider ormProvider, string rawSql, object parameters)
    {
        Action<IDataParameterCollection, IOrmProvider, object> dbParametersInitializer = null;
        if (parameters is IDictionary<string, object>)
        {
            dbParametersInitializer = (dbParameters, ormProvider, parameter) =>
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
            dbParametersInitializer = queryRawSqlDbParametersInitializerCache.GetOrAdd(cacheKey, f =>
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
        return dbParametersInitializer;
    }

    public static object BuildCreateWithBiesDbParametersInitializer(ISqlVisitor sqlVisitor, Type entityType, object insertObj, bool isMultiExecute)
    {
        object dbParametersInitializer = null;
        if (insertObj is IDictionary<string, object>)
        {
            if (isMultiExecute)
            {
                Action<IDataParameterCollection, StringBuilder, StringBuilder, string, object> typedDbParametersInitializer = null;
                typedDbParametersInitializer = (dbParameters, insertBuilder, valuesBuilder, multiMark, insertObj) =>
                {
                    int index = 0;
                    var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                    var dict = insertObj as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        if (index > 0)
                        {
                            insertBuilder.Append(',');
                            valuesBuilder.Append(',');
                        }
                        var parameterName = sqlVisitor.OrmProvider.ParameterPrefix + multiMark + memberMapper.MemberName;
                        insertBuilder.Append(sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName));
                        valuesBuilder.Append(parameterName);
                        var addDbParametersDelegate = BuildAddDbParameters(sqlVisitor.DbKey, sqlVisitor.OrmProvider, memberMapper, item.Value);
                        addDbParametersDelegate.Invoke(dbParameters, sqlVisitor.OrmProvider, parameterName, item.Value);
                        index++;
                    }
                };
                dbParametersInitializer = typedDbParametersInitializer;
            }
            else
            {
                Action<IDataParameterCollection, StringBuilder, StringBuilder, object> typedDbParametersInitializer = null;
                typedDbParametersInitializer = (dbParameters, insertBuilder, valuesBuilder, insertObj) =>
                {
                    int index = 0;
                    var dict = insertObj as IDictionary<string, object>;
                    var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        if (index > 0)
                        {
                            insertBuilder.Append(',');
                            valuesBuilder.Append(',');
                        }
                        var parameterName = sqlVisitor.OrmProvider.ParameterPrefix + memberMapper.MemberName;
                        insertBuilder.Append(sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName));
                        valuesBuilder.Append(parameterName);
                        var addDbParametersDelegate = BuildAddDbParameters(sqlVisitor.DbKey, sqlVisitor.OrmProvider, memberMapper, item.Value);
                        addDbParametersDelegate.Invoke(dbParameters, sqlVisitor.OrmProvider, parameterName, item.Value);
                        index++;
                    }
                };
                dbParametersInitializer = typedDbParametersInitializer;
            }
        }
        else
        {
            var parameterType = insertObj.GetType();
            if (!parameterType.IsEntityType(out _))
                throw new NotSupportedException("只支持类对象，不支持基础类型");

            var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
            var dbParametersInitializerCache = isMultiExecute ? createMultiWithByDbParametersInitializerCache : createWithByDbParametersInitializerCache;
            dbParametersInitializer = dbParametersInitializerCache.GetOrAdd(cacheKey, f =>
            {
                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");
                var fieldsBuilderExpr = Expression.Parameter(typeof(StringBuilder), "fieldsBuilder");
                var valueBuilderExpr = Expression.Parameter(typeof(StringBuilder), "valueBuilder");
                ParameterExpression multiMarkExpr = null;
                ParameterExpression parameterNameExpr = null;
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();

                if (isMultiExecute)
                {
                    multiMarkExpr = Expression.Parameter(typeof(string), "multiMark");
                    parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                    blockParameters.Add(parameterNameExpr);
                }
                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
                var ormProviderExpr = Expression.Constant(sqlVisitor.OrmProvider);

                var appendMethodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
                var appendMethodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                var addMethodInfo = typeof(IList).GetMethod(nameof(IList.Add));
                var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });

                int index = 0;
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;

                    if (index > 0)
                    {
                        blockBodies.Add(Expression.Call(fieldsBuilderExpr, appendMethodInfo1, Expression.Constant(',')));
                        blockBodies.Add(Expression.Call(valueBuilderExpr, appendMethodInfo1, Expression.Constant(',')));
                    }

                    var fieldName = sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName);
                    blockBodies.Add(Expression.Call(fieldsBuilderExpr, appendMethodInfo2, Expression.Constant(fieldName)));

                    Expression myParameterNameExpr = null;
                    if (isMultiExecute)
                    {
                        var parameterPrefixExpr = Expression.Constant(sqlVisitor.OrmProvider.ParameterPrefix);
                        var concatExpr = Expression.Call(concatMethodInfo, parameterPrefixExpr,
                            multiMarkExpr, Expression.Constant(memberMapper.MemberName));
                        blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));
                        myParameterNameExpr = parameterNameExpr;
                    }
                    else myParameterNameExpr = Expression.Constant(sqlVisitor.OrmProvider.ParameterPrefix + memberMapper.MemberName);

                    blockBodies.Add(Expression.Call(valueBuilderExpr, appendMethodInfo2, myParameterNameExpr));
                    var fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
                    AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                    index++;
                }
                if (isMultiExecute) dbParametersInitializer = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, StringBuilder, string, object>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, fieldsBuilderExpr, valueBuilderExpr, multiMarkExpr, parameterExpr).Compile();
                else dbParametersInitializer = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, StringBuilder, object>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, fieldsBuilderExpr, valueBuilderExpr, parameterExpr).Compile();
                return dbParametersInitializer;
            });
        }
        return dbParametersInitializer;
    }
    public static object BuildCreateWithBulkDbParametersInitializer(ISqlVisitor sqlVisitor, Type entityType, object insertObjs, bool isMultiExecute, out string headSql)
    {
        var entities = insertObjs as IEnumerable;
        headSql = null;
        object parameter = null, dbParametersInitializer = null;
        foreach (var entity in entities)
        {
            parameter = entity;
            break;
        }
        if (parameter is IDictionary<string, object> dict)
        {
            int index = 0;
            var builder = new StringBuilder();
            var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
            foreach (var item in dict)
            {
                if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                    || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
                    || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName));
                index++;
            }
            headSql = builder.ToString();
            if (isMultiExecute)
            {
                Action<IDataParameterCollection, StringBuilder, string, object, int> typedDbParametersInitializer = null;
                typedDbParametersInitializer = (dbParameters, builder, multiMark, insertObj, bulkIndex) =>
                {
                    int index = 0;
                    var dict = insertObj as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        if (index > 0) builder.Append(',');
                        var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}{multiMark}{item.Key}{bulkIndex}";
                        builder.Append(parameterName);
                        var addDbParametersDelegate = BuildAddDbParameters(sqlVisitor.DbKey, sqlVisitor.OrmProvider, memberMapper, item.Value);
                        addDbParametersDelegate.Invoke(dbParameters, sqlVisitor.OrmProvider, parameterName, item.Value);
                        index++;
                    }
                };
                dbParametersInitializer = typedDbParametersInitializer;
            }
            else
            {
                Action<IDataParameterCollection, StringBuilder, object, int> typedDbParametersInitializer = null;
                typedDbParametersInitializer = (dbParameters, builder, insertObj, bulkIndex) =>
                {
                    int index = 0;
                    var dict = insertObj as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        if (index > 0) builder.Append(',');
                        var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}{item.Key}{bulkIndex}";
                        builder.Append(parameterName);
                        var addDbParametersDelegate = BuildAddDbParameters(sqlVisitor.DbKey, sqlVisitor.OrmProvider, memberMapper, item.Value);
                        addDbParametersDelegate.Invoke(dbParameters, sqlVisitor.OrmProvider, parameterName, item.Value);
                        index++;
                    }
                };
                dbParametersInitializer = typedDbParametersInitializer;
            }
        }
        else
        {
            var parameterType = parameter.GetType();
            if (!parameterType.IsEntityType(out _))
                throw new NotSupportedException("只支持类对象，不支持基础类型");

            var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
            var dbParametersInitializerCache = isMultiExecute ? createMultiBulkDbParametersInitializerCache : createBulkDbParametersInitializerCache;
            (headSql, dbParametersInitializer) = dbParametersInitializerCache.GetOrAdd(cacheKey, f =>
              {
                  var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                  var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                      .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                  var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
                  var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
                  var insertObjExpr = Expression.Parameter(typeof(object), "insertObj");
                  var bulkIndexExpr = Expression.Parameter(typeof(int), "bulkIndex");

                  ParameterExpression multiMarkExpr = null;
                  var blockParameters = new List<ParameterExpression>();
                  var blockBodies = new List<Expression>();
                  var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                  var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                  var suffixExpr = Expression.Variable(typeof(string), "suffix");
                  blockParameters.AddRange(new[] { typedParameterExpr, suffixExpr, parameterNameExpr });
                  blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(insertObjExpr, parameterType)));
                  var ormProviderExpr = Expression.Constant(sqlVisitor.OrmProvider);

                  var toStringMethodInfo = typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes);
                  var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
                  var toStringExpr = Expression.Call(bulkIndexExpr, toStringMethodInfo);
                  if (isMultiExecute)
                  {
                      multiMarkExpr = Expression.Parameter(typeof(string), "multiMark");
                      var callExpr = Expression.Call(concatMethodInfo, multiMarkExpr, toStringExpr);
                      blockBodies.Add(Expression.Assign(suffixExpr, callExpr));
                  }
                  else blockBodies.Add(Expression.Assign(suffixExpr, toStringExpr));

                  var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                  var concatMethodInfo1 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) });
                  var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });

                  var index = 0;
                  foreach (var memberInfo in memberInfos)
                  {
                      if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                          || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
                          || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                          continue;

                      if (index > 0)
                          blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(",")));

                      var parameterPrefixExpr = Expression.Constant(sqlVisitor.OrmProvider.ParameterPrefix + memberMapper.MemberName);
                      blockBodies.Add(Expression.Assign(parameterNameExpr, Expression.Call(concatMethodInfo, parameterPrefixExpr, suffixExpr)));
                      blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
                      var fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
                      AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                      index++;
                  }

                  object result = null;
                  if (isMultiExecute) result = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, string, object, int>>(
                      Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, multiMarkExpr, insertObjExpr, multiMarkExpr, bulkIndexExpr).Compile();
                  else result = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, object, int>>(
                      Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, insertObjExpr, bulkIndexExpr).Compile();

                  var headSql = BuildFieldsSqlPart(sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType, false);
                  return (headSql, result);
              });
        }
        return dbParametersInitializer;
    }

    public static Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, object, string> BuildUpdateSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object updateObj)
    {
        Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, object, string> dbParametersInitializer = null;
        if (updateObj is IDictionary<string, object> dict)
        {
            dbParametersInitializer = (dbParameters, ormProvider, mapProvider, parameter) =>
            {
                int fieldsIndex = 0, whereIndex = 0;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var fieldsBuilder = new StringBuilder($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
                var whereBuilder = new StringBuilder(" WHERE ");
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;

                    string parameterName = null;
                    if (memberMapper.IsKey)
                    {
                        parameterName = $"{ormProvider.ParameterPrefix}k{item.Key}";
                        if (fieldsIndex > 0) fieldsBuilder.Append(',');
                        fieldsBuilder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        fieldsIndex++;
                    }
                    else
                    {
                        parameterName = $"{ormProvider.ParameterPrefix}{item.Key}";
                        if (whereIndex > 0) fieldsBuilder.Append(" AND ");
                        whereBuilder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        whereIndex++;
                    }
                    var addDbParametersDelegate = BuildAddDbParameters(dbKey, ormProvider, memberMapper, item.Value);
                    addDbParametersDelegate.Invoke(dbParameters, ormProvider, parameterName, item.Value);
                }
                return fieldsBuilder.Append(whereBuilder).ToString();
            };
        }
        else
        {
            var updateObjType = updateObj.GetType();
            var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, updateObjType);
            dbParametersInitializer = updateDbParametersInitializerCache.GetOrAdd(cacheKey, f =>
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var memberInfos = updateObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var mapProviderExpr = Expression.Parameter(typeof(IEntityMapProvider), "mapProvider");
                var updateObjExpr = Expression.Parameter(typeof(object), "updateObj");

                var typedUpdateObjExpr = Expression.Variable(updateObjType, "typedUpdateObj");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedUpdateObjExpr);
                blockBodies.Add(Expression.Assign(typedUpdateObjExpr, Expression.Convert(updateObjExpr, updateObjType)));

                int index = 0;
                var builder = new StringBuilder($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;
                    if (memberMapper.IsKey) continue;

                    Expression myParameterNameExpr = null;
                    if (index > 0) builder.Append(',');
                    var parameterName = ormProvider.ParameterPrefix + memberInfo.Name;
                    var parameterNameExpr = Expression.Constant(parameterName);
                    builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");

                    var fieldValueExpr = Expression.PropertyOrField(typedUpdateObjExpr, memberMapper.MemberName);
                    AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                    index++;
                }
                index = 0;
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    if (index > 0) builder.Append(" WHERE ");
                    var parameterName = $"{ormProvider.ParameterPrefix}k{keyMapper.MemberName}";
                    var parameterNameExpr = Expression.Constant(parameterName);
                    builder.Append($"{ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");
                    var fieldValueExpr = Expression.PropertyOrField(typedUpdateObjExpr, keyMapper.MemberName);
                    AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, keyMapper, blockParameters, blockBodies);
                    index++;
                }

                var returnExpr = Expression.Constant(builder.ToString());
                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));

                return Expression.Lambda<Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, object, string>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, mapProviderExpr, updateObjExpr).Compile();
            });
        }
        return dbParametersInitializer;
    }
    public static Action<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, object, int> BuildUpdateBulkSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object parameters)
    {
        Action<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, object, int> dbParametersInitializer = null;
        var entities = parameters as IEnumerable;
        object parameter = null;
        foreach (var entity in entities)
        {
            parameter = entity;
            break;
        }
        if (parameter is IDictionary<string, object> dict)
        {
            dbParametersInitializer = (dbParameters, ormProvider, mapProvider, builder, parameter, index) =>
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var whereBuilder = new StringBuilder();
                var keyDbParameters = new List<IDbDataParameter>();
                builder.Append($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;

                    var addDbParametersDelegate = BuildAddDbParameters(dbKey, ormProvider, memberMapper, item.Value);
                    if (memberMapper.IsKey)
                    {
                        var parameterName = $"{ormProvider.ParameterPrefix}k{item.Key}{index}";
                        whereBuilder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        addDbParametersDelegate.Invoke(dbParameters, ormProvider, parameterName, item.Value);
                    }
                    else
                    {
                        var parameterName = $"{ormProvider.ParameterPrefix}{item.Key}{index}";
                        builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        addDbParametersDelegate.Invoke(dbParameters, ormProvider, parameterName, item.Value);
                    }
                }
                builder.Append(whereBuilder);
                keyDbParameters.ForEach(f => dbParameters.Add(f));
            };
        }
        else
        {
            var updateObjType = parameter.GetType();
            var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, updateObjType);
            var objDbParametersInitializer = updateBulkDbParametersInitializerCache.GetOrAdd(cacheKey, f =>
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var memberInfos = updateObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var mapProviderExpr = Expression.Parameter(typeof(IEntityMapProvider), "mapProvider");
                var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
                var updateObjExpr = Expression.Parameter(typeof(object), "updateObj");
                var bulkIndexExpr = Expression.Parameter(typeof(int), "bulkIndex");

                var typedUpdateObjExpr = Expression.Variable(updateObjType, "typedUpdateObj");
                var strIndexExpr = Expression.Variable(typeof(string), "strIndex");
                var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.AddRange(new[] { typedUpdateObjExpr, strIndexExpr, parameterNameExpr });

                var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
                var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                var methodInfo = typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes);
                var updateHeadExpr = Expression.Constant($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
                blockBodies.Add(Expression.Assign(typedUpdateObjExpr, Expression.Convert(updateObjExpr, updateObjType)));
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, updateHeadExpr));
                blockBodies.Add(Expression.Assign(strIndexExpr, Expression.Call(bulkIndexExpr, methodInfo)));

                int index = 0;
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                       || memberMapper.IsIgnore || memberMapper.IsNavigation
                       || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;
                    if (memberMapper.IsKey) continue;

                    if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(",")));
                    var parameterName = $"{ormProvider.ParameterPrefix}{memberInfo.Name}";
                    var concatExpr = Expression.Call(concatMethodInfo, Expression.Constant(parameterName), strIndexExpr);
                    blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));

                    var setExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}=");
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, setExpr));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));

                    var fieldValueExpr = Expression.PropertyOrField(typedUpdateObjExpr, memberMapper.MemberName);
                    AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                    index++;
                }
                var whereBuilderExpr = Expression.Variable(typeof(StringBuilder), "whereBuilder");
                blockParameters.Add(whereBuilderExpr);
                var constructor = typeof(StringBuilder).GetConstructor(new Type[] { typeof(string) });
                blockBodies.Add(Expression.Assign(whereBuilderExpr, Expression.New(constructor, Expression.Constant(" WHERE "))));

                index = 0;
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    if (!memberInfos.Exists(f => f.Name == keyMapper.MemberName))
                        throw new ArgumentNullException($"参数类型{updateObjType.FullName}缺少主键字段{keyMapper.MemberName}");

                    if (index > 0) blockBodies.Add(Expression.Call(whereBuilderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                    var parameterName = $"{ormProvider.ParameterPrefix}k{keyMapper.MemberName}";
                    var callExpr = Expression.Call(concatMethodInfo, Expression.Constant(parameterName), strIndexExpr);
                    blockBodies.Add(Expression.Assign(parameterNameExpr, callExpr));
                    var setExpr = Expression.Constant($"{ormProvider.GetFieldName(keyMapper.FieldName)}=");
                    blockBodies.Add(Expression.Call(whereBuilderExpr, appendMethodInfo, setExpr));
                    blockBodies.Add(Expression.Call(whereBuilderExpr, appendMethodInfo, parameterNameExpr));

                    var fieldValueExpr = Expression.PropertyOrField(typedUpdateObjExpr, keyMapper.MemberName);
                    AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, keyMapper, blockParameters, blockBodies);
                    index++;
                }
                methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(StringBuilder) });
                blockBodies.Add(Expression.Call(builderExpr, methodInfo, whereBuilderExpr));
                return Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, object, int>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, mapProviderExpr, builderExpr, updateObjExpr, bulkIndexExpr).Compile();
            });
        }
        return dbParametersInitializer;
    }





    public static object BuildUpdateWithParameters(ISqlVisitor sqlVisitor, Type entityType, object updateObj, bool isWhere, bool isMultiExecute, bool isAnonymousParameter = false)
    {
        object dbParametersInitializer = null;
        if (updateObj is IDictionary<string, object>)
        {
            if (isMultiExecute)
            {
                Action<IDataParameterCollection, StringBuilder, object, string> typedDbParametersInitializer = null;
                if (isWhere) typedDbParametersInitializer = (dbParameters, builder, updateObj, multiMark) =>
                {
                    var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                    var dict = updateObj as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}k{item.Key}{multiMark}";
                        if (builder.Length > 0) builder.Append(',');
                        builder.Append($"{sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        var addDbParametersDelegate = BuildAddDbParameters(sqlVisitor.DbKey, sqlVisitor.OrmProvider, memberMapper, item.Value);
                        addDbParametersDelegate.Invoke(dbParameters, sqlVisitor.OrmProvider, parameterName, item.Value);
                    }
                };
                else typedDbParametersInitializer = (dbParameters, builder, updateObj, multiMark) =>
                {
                    var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                    var dict = updateObj as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}{item.Key}{multiMark}";
                        if (builder.Length > 0) builder.Append(',');
                        builder.Append($"{sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        var addDbParametersDelegate = BuildAddDbParameters(sqlVisitor.DbKey, sqlVisitor.OrmProvider, memberMapper, item.Value);
                        addDbParametersDelegate.Invoke(dbParameters, sqlVisitor.OrmProvider, parameterName, item.Value);
                    }
                };
                dbParametersInitializer = typedDbParametersInitializer;
            }
            else
            {
                Action<IDataParameterCollection, StringBuilder, object> typedDbParametersInitializer = null;
                if (isWhere) typedDbParametersInitializer = (dbParameters, builder, updateObj) =>
                {
                    var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                    var dict = updateObj as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}k{item.Key}";
                        if (builder.Length > 0) builder.Append(',');
                        builder.Append($"{sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        var addDbParametersDelegate = BuildAddDbParameters(sqlVisitor.DbKey, sqlVisitor.OrmProvider, memberMapper, item.Value);
                        addDbParametersDelegate.Invoke(dbParameters, sqlVisitor.OrmProvider, parameterName, item.Value);
                    }
                };
                else typedDbParametersInitializer = (dbParameters, builder, updateObj) =>
                {
                    var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                    var dict = updateObj as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}{item.Key}";
                        if (builder.Length > 0) builder.Append(',');
                        builder.Append($"{sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        var addDbParametersDelegate = BuildAddDbParameters(sqlVisitor.DbKey, sqlVisitor.OrmProvider, memberMapper, item.Value);
                        addDbParametersDelegate.Invoke(dbParameters, sqlVisitor.OrmProvider, parameterName, item.Value);
                    }
                };
                dbParametersInitializer = typedDbParametersInitializer;
            }
        }
        else
        {
            var parameterType = updateObj.GetType();
            var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
            var dbParametersInitializerCache = isMultiExecute ? updateMultiSetFieldsDbParametersInitializerCache : updateSetFieldsDbParametersInitializerCache;
            if (!dbParametersInitializerCache.TryGetValue(cacheKey, out dbParametersInitializer))
            {
                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
                var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");
                ParameterExpression multiMarkExpr = null;
                if (isMultiExecute) multiMarkExpr = Expression.Parameter(typeof(string), "multiMark");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();

                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
                var ormProviderExpr = Expression.Constant(sqlVisitor.OrmProvider);

                var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;

                    Expression parameterNameExpr = null;
                    var memberMapperExpr = Expression.Constant(memberMapper);
                    if (isWhere) parameterNameExpr = Expression.Constant($"{sqlVisitor.OrmProvider.ParameterPrefix}k{memberInfo.Name}");
                    else parameterNameExpr = Expression.Constant($"{sqlVisitor.OrmProvider.ParameterPrefix}{memberInfo.Name}");
                    if (isMultiExecute) parameterNameExpr = Expression.Call(concatMethodInfo, parameterNameExpr, multiMarkExpr);

                    var fieldNameExpr = Expression.Constant(sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName) + "=");
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, fieldNameExpr));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
                    var fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
                    AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                }

                if (isMultiExecute) dbParametersInitializer = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, object, string>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, parameterExpr, multiMarkExpr).Compile();
                else dbParametersInitializer = Expression.Lambda<Action<IDataParameterCollection, StringBuilder, object>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, builderExpr, parameterExpr).Compile();
                dbParametersInitializerCache.TryAdd(cacheKey, dbParametersInitializer);
            }
        }
        return dbParametersInitializer;
    }

    public static object BuildDeleteDbParametersInitializer(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiExecute)
    {
        object dbParametersInitializer = null;
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, whereObjType);
        if (isMultiExecute)
        {
            dbParametersInitializer = queryGetDbParametersInitializerCache.GetOrAdd(cacheKey, f =>
            {
                Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, string, object, string> typedDbParametersInitializer = null;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                typedDbParametersInitializer = (command, ormProvider, mapProvider, multiMark, parameter) =>
                {
                    var whereDbParametersInitializer = BuildWhereKeySqlParameters(dbKey, ormProvider, mapProvider, entityType, whereObj, false);
                    var typedDbParametersInitializer = whereDbParametersInitializer as Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, string, object, string>;
                    var builder = new StringBuilder($"DELETE FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                    return typedDbParametersInitializer.Invoke(command, ormProvider, mapProvider, builder, multiMark, parameter);
                };
                return typedDbParametersInitializer;
            });
        }
        else
        {
            dbParametersInitializer = queryGetDbParametersInitializerCache.GetOrAdd(cacheKey, f =>
            {
                Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, object, string> typedDbParametersInitializer = null;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                typedDbParametersInitializer = (command, ormProvider, mapProvider, parameter) =>
                {
                    var whereDbParametersInitializer = BuildWhereKeySqlParameters(dbKey, ormProvider, mapProvider, entityType, whereObj, false);
                    var typedDbParametersInitializer = whereDbParametersInitializer as Func<IDataParameterCollection, IOrmProvider, IEntityMapProvider, StringBuilder, object, string>;
                    var builder = new StringBuilder($"DELETE FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                    return typedDbParametersInitializer.Invoke(command, ormProvider, mapProvider, builder, parameter);
                };
                return typedDbParametersInitializer;
            });
        }
        return dbParametersInitializer;
    }

    public static object BuildWhereWithKeysSqlParameters(ISqlVisitor sqlVisitor, Type entityType, object parameters, bool isMultiExecute)
    {
        object dbParametersInitializer = null;
        if (parameters is IDictionary<string, object>)
        {
            if (isMultiExecute)
            {
                Func<IDataParameterCollection, object, int, string> typedDbParametersInitializer = null;
                typedDbParametersInitializer = (dbParameters, whereObj, commandIndex) =>
                {
                    int index = 0;
                    var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                    var dict = whereObj as IDictionary<string, object>;
                    var builder = new StringBuilder();
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        if (index > 0) builder.Append(" AND ");
                        var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}k{item.Key}_m{commandIndex}";
                        builder.Append(sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName));
                        var addDbParametersDelegate = BuildAddDbParameters(sqlVisitor.DbKey, sqlVisitor.OrmProvider, memberMapper, item.Value);
                        addDbParametersDelegate.Invoke(dbParameters, sqlVisitor.OrmProvider, parameterName, item.Value);
                        index++;
                    }
                    return builder.ToString();
                };
                dbParametersInitializer = typedDbParametersInitializer;
            }
            else
            {
                Func<IDataParameterCollection, object, string> typedDbParametersInitializer = null;
                typedDbParametersInitializer = (dbParameters, whereObj) =>
                {
                    int index = 0;
                    var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                    var builder = new StringBuilder();
                    var dict = whereObj as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        if (index > 0) builder.Append(" AND ");
                        var parameterName = sqlVisitor.OrmProvider.ParameterPrefix + "k" + item.Key;
                        builder.Append(sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName));
                        var addDbParametersDelegate = BuildAddDbParameters(sqlVisitor.DbKey, sqlVisitor.OrmProvider, memberMapper, item.Value);
                        addDbParametersDelegate.Invoke(dbParameters, sqlVisitor.OrmProvider, parameterName, item.Value);
                        index++;
                    }
                    return builder.ToString();
                };
                dbParametersInitializer = typedDbParametersInitializer;
            }
        }
        else
        {
            var parameterType = parameters.GetType();
            if (!parameterType.IsEntityType(out _))
                throw new NotSupportedException("只支持类对象，不支持基础类型");

            var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
            var whereWithCache = isMultiExecute ? mutilWhereWithKeysDbParametersInitializerCache : whereWithKeysDbParametersInitializerCache;
            if (!whereWithCache.TryGetValue(cacheKey, out dbParametersInitializer))
            {
                int columnIndex = 0;
                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");
                ParameterExpression builderExpr = null;
                ParameterExpression commandIndexExpr = null;
                if (isMultiExecute)
                {
                    builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
                    commandIndexExpr = Expression.Parameter(typeof(int), "commandIndex");
                }

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
                var ormProviderExpr = Expression.Constant(sqlVisitor.OrmProvider);

                var addMethodInfo = typeof(IList).GetMethod(nameof(IList.Add));
                var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
                var toStringMethodInfo = typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes);
                var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });

                StringBuilder builder = null;
                if (isMultiExecute)
                {
                    blockParameters.Add(builderExpr);
                    var newExpr = Expression.New(typeof(StringBuilder).GetConstructor(Type.EmptyTypes));
                    blockBodies.Add(Expression.Assign(builderExpr, newExpr));
                }
                else builder = new StringBuilder();
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.IsAutoIncrement
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;

                    if (columnIndex > 0)
                    {
                        if (isMultiExecute)
                            blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                        else builder.Append(" AND ");
                    }
                    var fieldName = sqlVisitor.OrmProvider.GetFieldName(memberMapper.FieldName);
                    var parameterName = sqlVisitor.OrmProvider.ParameterPrefix + "k" + memberMapper.MemberName;
                    var memberMapperExpr = Expression.Constant(memberMapper);
                    Expression parameterNameExpr = null;
                    if (isMultiExecute)
                    {
                        parameterName += $"_m";
                        var toStringExpr = Expression.Call(commandIndexExpr, toStringMethodInfo);
                        parameterNameExpr = Expression.Call(concatMethodInfo, Expression.Constant(parameterName), toStringExpr);
                        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant($"{fieldName}=")));
                        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
                    }
                    else
                    {
                        parameterNameExpr = Expression.Constant(parameterName);
                        builder.Append($"{fieldName}={parameterName}");
                    }
                    var fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
                    AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
                    columnIndex++;
                }
                var methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
                Expression returnExpr = isMultiExecute ? Expression.Call(builderExpr, methodInfo) : Expression.Constant(builder.ToString());
                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                if (isMultiExecute) dbParametersInitializer = Expression.Lambda<Action<IDataParameterCollection, object, int>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, parameterExpr, commandIndexExpr).Compile();
                else dbParametersInitializer = Expression.Lambda<Func<IDataParameterCollection, object, string>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, parameterExpr).Compile();
                whereWithCache.TryAdd(cacheKey, dbParametersInitializer);
            }
        }
        return dbParametersInitializer;
    }
}