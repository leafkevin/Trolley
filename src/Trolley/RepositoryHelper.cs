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

        Expression addParameterExpr = null;
        if (memberMapper.IsRequired)
        {
            fieldValueExpr = GetTypedFieldValue(fieldValueExpr, memberMapper, blockParameters, blockBodies);
            typedParameterExpr = Expression.Call(ormProviderExpr, createParameterMethodInfo, parameterNameExpr, nativeDbTypeExpr, fieldValueExpr);
            addParameterExpr = Expression.Call(dbParametersExpr, addMethodInfo, typedParameterExpr);
        }
        else
        {
            if (fieldValueType.IsNullableType(out _) || fieldValueType.IsClass)
            {
                if (fieldValueType == typeof(DBNull))
                {
                    fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                    var dbNullExpr = Expression.Call(ormProviderExpr, createParameterMethodInfo, parameterNameExpr, nativeDbTypeExpr, fieldValueExpr);
                    addParameterExpr = Expression.Call(dbParametersExpr, addMethodInfo, dbNullExpr);
                }
                else
                {
                    var equalsExpr = Expression.Equal(parameterValueExpr, Expression.Constant(null));
                    var nullExpr = Expression.Call(ormProviderExpr, createParameterMethodInfo, parameterNameExpr, nativeDbTypeExpr, Expression.Constant(DBNull.Value));
                    var addNullExpr = Expression.Call(dbParametersExpr, addMethodInfo, nullExpr);

                    fieldValueExpr = GetTypedFieldValue(fieldValueExpr, memberMapper, blockParameters, blockBodies);
                    typedParameterExpr = Expression.Call(ormProviderExpr, createParameterMethodInfo, parameterNameExpr, nativeDbTypeExpr, fieldValueExpr);
                    addTypedParameterExpr = Expression.Call(dbParametersExpr, addMethodInfo, typedParameterExpr);

                    addParameterExpr = Expression.IfThenElse(equalsExpr, addNullExpr, addTypedParameterExpr);
                }
            }
            else
            {
                fieldValueExpr = GetTypedFieldValue(fieldValueExpr, memberMapper, blockParameters, blockBodies);
                typedParameterExpr = Expression.Call(ormProviderExpr, createParameterMethodInfo, parameterNameExpr, nativeDbTypeExpr, fieldValueExpr);
                addTypedParameterExpr = Expression.Call(dbParametersExpr, addMethodInfo, typedParameterExpr);

                addParameterExpr = addTypedParameterExpr;
            }
        }
        blockBodies.Add(addParameterExpr);
    }
    private static Expression GetTypedFieldValue(Expression fieldValueExpr, MemberMap memberMapper, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        var fieldValueType = fieldValueExpr.Type;
        var typedFieldValueExpr = fieldValueExpr;
        MethodInfo methodInfo = null;

        if (fieldValueType == typeof(DBNull))
            throw new ArgumentNullException($"表{memberMapper.Parent.TableName}的字段{memberMapper.FieldName}配置为必输字段，不能为DBNull.Value");

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
                            typedFieldValueExpr = Expression.Convert(typedFieldValueExpr, enumUnderlyingType);
                        methodInfo = typeof(Enum).GetMethod(nameof(Enum.ToObject), new Type[] { typeof(Type), enumUnderlyingType });
                        typedFieldValueExpr = Expression.Call(methodInfo, Expression.Constant(underlyingType), typedFieldValueExpr);
                        typedFieldValueExpr = Expression.Convert(typedFieldValueExpr, underlyingType);
                    }
                    //把枚举类型再变成字符串类型
                    typedFieldValueExpr = Expression.Call(typedFieldValueExpr, typeof(Enum).GetMethod(nameof(Enum.ToString), Type.EmptyTypes));
                }
                //数据库类型是数字类型
                else
                {
                    //枚举类型或是数字类型
                    if (fieldValueType.IsEnum)
                        typedFieldValueExpr = Expression.Convert(typedFieldValueExpr, enumUnderlyingType);
                    if (memberMapper.DbDefaultType != enumUnderlyingType)
                        typedFieldValueExpr = Expression.Convert(typedFieldValueExpr, memberMapper.DbDefaultType);
                }
            }
            else if (underlyingType == typeof(Guid))
            {
                if (memberMapper.DbDefaultType == typeof(string))
                    typedFieldValueExpr = Expression.Call(typedFieldValueExpr, typeof(Guid).GetMethod(nameof(Guid.ToString), Type.EmptyTypes));
                else if (memberMapper.DbDefaultType == typeof(byte[]))
                    typedFieldValueExpr = Expression.Call(typedFieldValueExpr, typeof(Guid).GetMethod(nameof(Guid.ToByteArray), Type.EmptyTypes));
            }
            else if (underlyingType == typeof(DateTime))
            {
                if (memberMapper.DbDefaultType == typeof(long))
                    typedFieldValueExpr = Expression.Property(typedFieldValueExpr, nameof(DateTime.Ticks));
                if (memberMapper.DbDefaultType == typeof(string))
                {
                    methodInfo = typeof(DateTime).GetMethod(nameof(DateTime.ToString), new Type[] { typeof(string) });
                    typedFieldValueExpr = Expression.Call(typedFieldValueExpr, methodInfo, Expression.Constant("yyyy-MM-dd HH:mm:ss.fffffff"));
                }
            }
            else if (underlyingType == typeof(DateOnly))
            {
                if (memberMapper.DbDefaultType == typeof(string))
                {
                    methodInfo = typeof(DateTime).GetMethod(nameof(DateTime.ToString), new Type[] { typeof(string) });
                    typedFieldValueExpr = Expression.Call(typedFieldValueExpr, methodInfo, Expression.Constant("yyyy-MM-dd"));
                }
            }
            else if (underlyingType == typeof(TimeSpan))
            {
                if (memberMapper.DbDefaultType == typeof(long))
                    typedFieldValueExpr = Expression.Property(typedFieldValueExpr, nameof(TimeSpan.Ticks));
                if (memberMapper.DbDefaultType == typeof(string))
                {
                    methodInfo = typeof(TimeSpan).GetMethod(nameof(TimeSpan.ToString), new Type[] { typeof(string) });
                    var greaterThanExpr = Expression.GreaterThanOrEqual(typedFieldValueExpr, Expression.Constant(1));
                    var ifExpr = Expression.Call(typedFieldValueExpr, methodInfo, Expression.Constant("d\\.hh\\:mm\\:ss\\.fffffff"));
                    var elseExpr = Expression.Call(typedFieldValueExpr, methodInfo, Expression.Constant("hh\\:mm\\:ss\\.fffffff"));

                    var localVariable = Expression.Variable(typeof(string), $"str{memberMapper.MemberName}");
                    blockParameters.Add(localVariable);
                    var assignIfExpr = Expression.Assign(localVariable, ifExpr);
                    var assignElseExpr = Expression.Assign(localVariable, elseExpr);
                    blockBodies.Add(Expression.IfThenElse(greaterThanExpr, assignIfExpr, assignElseExpr));
                    typedFieldValueExpr = localVariable;
                }
            }
            else if (underlyingType == typeof(TimeOnly))
            {
                if (memberMapper.DbDefaultType == typeof(long))
                    typedFieldValueExpr = Expression.Property(typedFieldValueExpr, nameof(TimeOnly.Ticks));
                if (memberMapper.DbDefaultType == typeof(string))
                {
                    methodInfo = typeof(TimeOnly).GetMethod(nameof(TimeOnly.ToString), new Type[] { typeof(string) });
                    typedFieldValueExpr = Expression.Call(typedFieldValueExpr, methodInfo, Expression.Constant("hh\\:mm\\:ss\\.fffffff"));
                }
            }
            else
            {
                methodInfo = typeof(Convert).GetMethod(nameof(Convert.ChangeType), new Type[] { typeof(Object), typeof(Type) });
                if (fieldValueType != typeof(object))
                    typedFieldValueExpr = Expression.Convert(typedFieldValueExpr, typeof(object));
                typedFieldValueExpr = Expression.Call(methodInfo, typedFieldValueExpr, Expression.Constant(memberMapper.DbDefaultType));
            }
            typedFieldValueExpr = Expression.Convert(typedFieldValueExpr, typeof(object));
        }
        else
        {
            if (fieldValueType != typeof(object))
                typedFieldValueExpr = Expression.Convert(typedFieldValueExpr, typeof(object));
        }
        return typedFieldValueExpr;
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
    public static object BuildWhereSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type whereObjType, bool isWhereKey, bool hasSuffix, bool isWithKey, string whereObjName, string headSql)
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
            var fieldsSql = BuildFieldsSqlPart(ormProvider, entityMapper, entityType, true);
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
            var fieldsSql = BuildFieldsSqlPart(ormProvider, entityMapper, entityType, true);
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

    public static Func<IDataParameterCollection, IOrmProvider, object, string> BuildCreateSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object insertObj, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool isReturnIdentity)
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
    public static (Action<StringBuilder>, Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>) BuildCreateMultiSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object insertObjs, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool isBulk)
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
        var cacheKey = BuildInsertHashKey(dbKey, ormProvider, mapProvider, entityType, insertObjType, onlyFieldNames, ignoreFieldNames);
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
    public static Action<StringBuilder, object> BuildCreateHeadSqlPart(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type insertObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames)
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
    public static object BuildCreateValuesPartSqlParametes(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type insertObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool hasSuffix)
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
                AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
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
    public static object BuildUpdateSetPartSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type updateObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool hasSuffix, bool isInsertOrUpdate = false)
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
                //  ormProvider.AddDbParameter(dbKey, dbParameters, memberMapper, parameterName, fieldValue);
                methodInfo = typeof(IDataParameterCollection).GetMethod(nameof(IDataParameterCollection.Contains));
                var notContainsExpr = Expression.IsFalse(Expression.Call(dbParametersExpr, methodInfo, parameterNameExpr));
                methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.AddDbParameter));
                var addParameterExpr = Expression.Call(methodInfo, ormProviderExpr, Expression.Constant(dbKey),
                     dbParametersExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr);
                loopBodies.Add(Expression.IfThen(notContainsExpr, addParameterExpr));
            }
            else
            {
                //ormProvider.AddDbParameter(dbKey, dbParameters, memberMapper, parameterName, fieldValue);
                methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.AddDbParameter));
                loopBodies.Add(Expression.Call(methodInfo, ormProviderExpr, Expression.Constant(dbKey),
                    dbParametersExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr));
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
                if (isInsertOrUpdate)
                {
                    //if(!dbParameters.Contains(parameterName))
                    //  ormProvider.AddDbParameter(dbKey, dbParameters, memberMapper, parameterName, fieldValue);
                    methodInfo = typeof(IDataParameterCollection).GetMethod(nameof(IDataParameterCollection.Contains));
                    var notContainsExpr = Expression.IsFalse(Expression.Call(dbParametersExpr, methodInfo, myParameterNameExpr));
                    methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.AddDbParameter));
                    if (fieldValueExpr.Type != typeof(object))
                        fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                    var addParameterExpr = Expression.Call(methodInfo, ormProviderExpr, Expression.Constant(dbKey),
                         dbParametersExpr, Expression.Constant(memberMapper), myParameterNameExpr, fieldValueExpr);
                    blockBodies.Add(Expression.IfThen(notContainsExpr, addParameterExpr));
                }
                else AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
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
            var setFieldsMethodInfo = typeof(FieldsSegment).GetProperty(nameof(FieldsSegment.Fields)).GetSetMethod();
            var setValuesMethodInfo = typeof(FieldsSegment).GetProperty(nameof(FieldsSegment.Values)).GetSetMethod();
            var addMethodInfo = typeof(List<FieldsSegment>).GetMethod(nameof(List<FieldsSegment>.Add));

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
                ParameterExpression parameterNameExpr = null;
                var typedUpdateObjExpr = Expression.Parameter(updateObjType, "typeUpdateObj");
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
                    AddValueParameter(dbParametersExpr, ormProviderExpr, myParameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
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

    public static int BuildInsertHashKey(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type insertObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames)
    {
        var hashCode = new HashCode();
        hashCode.Add(dbKey);
        hashCode.Add(ormProvider);
        hashCode.Add(mapProvider);
        hashCode.Add(entityType);
        hashCode.Add(insertObjType);
        AddFieldHashCode(hashCode, onlyFieldNames);
        AddFieldHashCode(hashCode, ignoreFieldNames);
        return hashCode.ToHashCode();
    }
    public static int BuildInsertHashKey(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, Type insertObjType, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool isReturnIdentity)
    {
        var hashCode = new HashCode();
        hashCode.Add(dbKey);
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
