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
    private static ConcurrentDictionary<int, Action<IDbCommand, IOrmProvider, object>> queryRawSqlCommandInitializerCache = new();

    private static ConcurrentDictionary<int, object> createWithByCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> createMultiWithByCommandInitializerCache = new();

    private static ConcurrentDictionary<int, object> deleteCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> deleteBulkCommandInitializerCache = new();






    private static ConcurrentDictionary<int, object> createBulkCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> createMultiBulkCommandInitializerCache = new();


    private static ConcurrentDictionary<int, object> updateCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> updateMultiCommandInitializerCache = new();

    private static ConcurrentDictionary<int, Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, object, int>> updateBulkCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> updateSetFieldsCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> updateMultiSetFieldsCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> updateWhereWithCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> updateMultiWhereWithCommandInitializerCache = new();




    private static ConcurrentDictionary<int, object> whereWithKeysCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> mutilWhereWithKeysCommandInitializerCache = new();

    public static void AddValueParameter(ParameterExpression commandExpr, Expression ormProviderExpr, Expression parameterNameExpr,
        Expression parameterValueExpr, MemberMap memberMapper, List<Expression> blockBodies)
    {
        MethodInfo methodInfo = null;
        Expression dbParameterExpr = null;
        var fieldValueExpr = parameterValueExpr;
        if (parameterValueExpr.Type != typeof(object))
            fieldValueExpr = Expression.Convert(parameterValueExpr, typeof(object));
        var memberMapperExpr = Expression.Constant(memberMapper);
        methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.CreateParameter));
        dbParameterExpr = Expression.Call(methodInfo, ormProviderExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr);
        var parametersExpr = Expression.Property(commandExpr, nameof(IDbCommand.Parameters));
        methodInfo = typeof(IList).GetMethod(nameof(IDbCommand.Parameters.Add));
        var addParameterExpr = Expression.Call(parametersExpr, methodInfo, dbParameterExpr);
        blockBodies.Add(addParameterExpr);
    }
    public static void AddValueParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr, Expression parameterNameExpr,
        Expression parameterValueExpr, List<Expression> blockBodies)
    {
        Expression dbParameterExpr = null;
        var fieldValueExpr = parameterValueExpr;
        if (parameterValueExpr.Type != typeof(object))
            fieldValueExpr = Expression.Convert(parameterValueExpr, typeof(object));

        //var dbParameter = ormProvider.CreateParameter("@Name", objValue);
        var methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
        dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, fieldValueExpr);

        var parametersExpr = Expression.Property(commandExpr, nameof(IDbCommand.Parameters));
        methodInfo = typeof(IList).GetMethod(nameof(IDbCommand.Parameters.Add));
        var addParameterExpr = Expression.Call(parametersExpr, methodInfo, dbParameterExpr);
        blockBodies.Add(addParameterExpr);
    }
    public static void AddMemberParameter(ParameterExpression commandExpr, Expression ormProviderExpr, Expression parameterNameExpr,
        Expression typedParameterExpr, MemberMap memberMapper, List<Expression> blockBodies)
    {
        var fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
        AddValueParameter(commandExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, memberMapper, blockBodies);
    }
    public static void AddMemberParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr, Expression parameterNameExpr,
        Expression typedParameterExpr, string memberName, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", value);
        var fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberName);
        AddValueParameter(commandExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, blockBodies);
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
            if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var propMapper)
                || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                continue;

            if (index > 0) builder.Append(',');
            builder.Append(ormProvider.GetFieldName(propMapper.FieldName));
            if (isSelect && propMapper.FieldName != propMapper.MemberName)
                builder.Append(" AS " + ormProvider.GetFieldName(propMapper.MemberName));
            index++;
        }
        return builder.ToString();
    }
    public static object BuildWhereSqlParameters(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiQuery)
    {
        object commandInitializer = null;
        if (whereObj is IDictionary<string, object>)
        {
            if (isMultiQuery)
            {
                Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, string, object> dictCommandInitializer = null;
                dictCommandInitializer = (command, ormProvider, mapProvider, builder, prefix, parameter) =>
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

                        var parameterName = ormProvider.ParameterPrefix + prefix + item.Key;
                        if (index > 0) builder.Append(" AND ");
                        builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        command.Parameters.Add(ormProvider.CreateParameter(memberMapper, parameterName, item.Value));
                        index++;
                    }
                };
                commandInitializer = dictCommandInitializer;
            }
            else
            {
                Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, object> dictCommandInitializer = null;
                dictCommandInitializer = (command, ormProvider, mapProvider, builder, parameter) =>
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
                        command.Parameters.Add(ormProvider.CreateParameter(memberMapper, parameterName, item.Value));
                        index++;
                    }
                };
                commandInitializer = dictCommandInitializer;
            }
        }
        else
        {
            var whereObjType = whereObj.GetType();
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var memberInfos = whereObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var mapProviderExpr = Expression.Parameter(typeof(IEntityMapProvider), "mapProvider");
            var builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
            var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

            ParameterExpression prefixExpr = null;
            ParameterExpression parameterNameExpr = null;
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            if (isMultiQuery)
            {
                prefixExpr = Expression.Parameter(typeof(string), "prefix");
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
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var propMapper)
                    || propMapper.IsIgnore || propMapper.IsNavigation
                    || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                    continue;

                if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                Expression myParameterNameExpr = null;
                if (isMultiQuery)
                {
                    var concatExpr = Expression.Call(concatMethodInfo,
                        Expression.Constant(ormProvider.ParameterPrefix), prefixExpr, Expression.Constant(propMapper.MemberName));
                    blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));
                    myParameterNameExpr = parameterNameExpr;

                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo,
                        Expression.Constant($"{ormProvider.GetFieldName(propMapper.FieldName)}=")));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
                }
                else
                {
                    var parameterName = $"{ormProvider.ParameterPrefix}{propMapper.MemberName}";
                    myParameterNameExpr = Expression.Constant(parameterName);
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo,
                        Expression.Constant($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}")));
                }
                AddMemberParameter(commandExpr, ormProviderExpr, myParameterNameExpr, typedWhereObjExpr, propMapper, blockBodies);
                index++;
            }
            if (isMultiQuery) commandInitializer = Expression.Lambda<Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, string, object>>(
                Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, mapProviderExpr, builderExpr, prefixExpr, whereObjExpr).Compile();
            else commandInitializer = Expression.Lambda<Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, object>>(
                Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, mapProviderExpr, builderExpr, whereObjExpr).Compile();
        }
        return commandInitializer;
    }
    public static object BuildWhereKeySqlParameters(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiQuery)
    {
        object commandInitializer = null;
        if (whereObj is IDictionary<string, object>)
        {
            if (isMultiQuery)
            {
                Func<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, string, object, string> typedCommandInitializer = null;
                typedCommandInitializer = (command, ormProvider, mapProvider, builder, prefix, parameter) =>
                {
                    var index = 0;
                    var entityMapper = mapProvider.GetEntityMap(entityType);
                    var dict = parameter as IDictionary<string, object>;
                    foreach (var keyMapper in entityMapper.KeyMembers)
                    {
                        if (!dict.TryGetValue(keyMapper.MemberName, out var fieldValue))
                            throw new ArgumentNullException(nameof(whereObj), $"字典参数缺少主键字段{keyMapper.MemberName}");

                        if (index > 0) builder.Append(" AND ");
                        var parameterName = $"{ormProvider.ParameterPrefix}{prefix}{keyMapper.MemberName}";
                        builder.Append($"{ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");
                        command.Parameters.Add(ormProvider.CreateParameter(keyMapper, parameterName, fieldValue));
                        index++;
                    }
                    return builder.ToString();
                };
                commandInitializer = typedCommandInitializer;
            }
            else
            {
                Func<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, object, string> typedCommandInitializer = null;
                typedCommandInitializer = (command, ormProvider, mapProvider, builder, parameter) =>
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
                        command.Parameters.Add(ormProvider.CreateParameter(keyMapper, parameterName, fieldValue));
                        index++;
                    }
                    return builder.ToString();
                };
                commandInitializer = typedCommandInitializer;
            }
        }
        else
        {
            var whereObjType = whereObj.GetType();
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var mapProviderExpr = Expression.Parameter(typeof(IEntityMapProvider), "mapProvider");
            var builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
            var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

            ParameterExpression prefixExpr = null;
            ParameterExpression typedWhereObjExpr = null;
            ParameterExpression parameterNameExpr = null;
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            bool isEntityType = false;
            List<MemberInfo> memberInfos = null;

            if (isMultiQuery)
            {
                prefixExpr = Expression.Parameter(typeof(string), "prefix");
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
                        Expression.Constant(ormProvider.ParameterPrefix), prefixExpr, Expression.Constant(keyMapper.MemberName));
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
                    AddMemberParameter(commandExpr, ormProviderExpr, myParameterNameExpr, typedWhereObjExpr, keyMapper, blockBodies);
                else AddValueParameter(commandExpr, ormProviderExpr, myParameterNameExpr, whereObjExpr, keyMapper, blockBodies);
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

            if (isMultiQuery) commandInitializer = Expression.Lambda<Func<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, string, object, string>>(
                Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, mapProviderExpr, builderExpr, prefixExpr, whereObjExpr).Compile();
            else commandInitializer = Expression.Lambda<Func<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, object, string>>(
                Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, mapProviderExpr, builderExpr, whereObjExpr).Compile();
        }
        return commandInitializer;
    }
    public static object BuildBulkWhereSqlParameters(IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiExecute)
    {
        object commandInitializer = null;
        if (whereObj is IDictionary<string, object>)
        {
            if (isMultiExecute)
            {
                Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, string, object, int> dictCommandInitializer = null;
                dictCommandInitializer = (command, ormProvider, mapProvider, builder, prefix, parameter, bulkIndex) =>
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

                        var parameterName = $"{ormProvider.ParameterPrefix}{prefix}{item.Key}{bulkIndex}";
                        if (index > 0) builder.Append(" AND ");
                        builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        command.Parameters.Add(ormProvider.CreateParameter(memberMapper, parameterName, item.Value));
                        index++;
                    }
                };
                commandInitializer = dictCommandInitializer;
            }
            else
            {
                Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, object, int> dictCommandInitializer = null;
                dictCommandInitializer = (command, ormProvider, mapProvider, builder, parameter, bulkIndex) =>
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
                        command.Parameters.Add(ormProvider.CreateParameter(memberMapper, parameterName, item.Value));
                        index++;
                    }
                };
                commandInitializer = dictCommandInitializer;
            }
        }
        else
        {
            var whereObjType = whereObj.GetType();
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var mapProviderExpr = Expression.Parameter(typeof(IEntityMapProvider), "mapProvider");
            var builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
            var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");
            var bulkIndexExpr = Expression.Parameter(typeof(int), "bulkIndex");

            ParameterExpression prefixExpr = null;
            ParameterExpression typedWhereObjExpr = null;
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            bool isEntityType = false;
            List<MemberInfo> memberInfos = null;

            if (isMultiExecute)
                prefixExpr = Expression.Parameter(typeof(string), "prefix");
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

            bool isMulitKeys = entityMapper.KeyMembers.Count > 1;
            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var concatMethodInfo1 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) });
            var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });

            var index = 0;
            foreach (var keyMapper in entityMapper.KeyMembers)
            {
                if (isEntityType && !memberInfos.Exists(f => f.Name == keyMapper.MemberName))
                    throw new ArgumentNullException("whereObj", $"参数类型{whereObjType.FullName}缺少主键字段{keyMapper.MemberName}");

                if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo,
                    Expression.Constant(isMulitKeys ? " AND " : ",")));
                Expression concatExpr = null;
                if (isMultiExecute) concatExpr = Expression.Call(concatMethodInfo1,
                    Expression.Constant(ormProvider.ParameterPrefix), prefixExpr, Expression.Constant(keyMapper.MemberName), strIndexExpr);
                else concatExpr = Expression.Call(concatMethodInfo2,
                    Expression.Constant($"{ormProvider.ParameterPrefix}{keyMapper.MemberName}"), strIndexExpr);
                blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));

                if (isMulitKeys)
                {
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo,
                        Expression.Constant($"{ormProvider.GetFieldName(keyMapper.FieldName)}=")));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
                    AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, keyMapper, blockBodies);
                }
                else
                {
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
                    AddValueParameter(commandExpr, ormProviderExpr, parameterNameExpr, whereObjExpr, keyMapper, blockBodies);
                }
                index++;
            }
            if (isMultiExecute) commandInitializer = Expression.Lambda<Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, string, object, int>>(
                Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, mapProviderExpr, builderExpr, prefixExpr, whereObjExpr, bulkIndexExpr).Compile();
            else commandInitializer = Expression.Lambda<Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, object, int>>(
                Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, mapProviderExpr, builderExpr, whereObjExpr, bulkIndexExpr).Compile();
        }
        return commandInitializer;
    }


    public static object BuildGetSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiQuery)
    {
        object commandInitializer = null;
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, whereObjType);
        if (isMultiQuery)
        {
            commandInitializer = queryGetCommandInitializerCache.GetOrAdd(cacheKey, f =>
            {
                Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string> typedCommandInitializer = null;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                typedCommandInitializer = (command, ormProvider, mapProvider, prefix, parameter) =>
                {
                    var whereCommandInitializer = BuildWhereKeySqlParameters(ormProvider, mapProvider, entityType, whereObj, false);
                    var typedWhereCommandInitializer = whereCommandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, string, object, string>;
                    var builder = new StringBuilder("SELECT ");
                    builder.Append(BuildFieldsSqlPart(ormProvider, mapProvider, entityType, entityType, true));
                    builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                    return typedWhereCommandInitializer.Invoke(command, ormProvider, mapProvider, builder, prefix, parameter);
                };
                return typedCommandInitializer;
            });
        }
        else
        {
            commandInitializer = queryGetCommandInitializerCache.GetOrAdd(cacheKey, f =>
            {
                Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string> typedCommandInitializer = null;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                typedCommandInitializer = (command, ormProvider, mapProvider, parameter) =>
                {
                    var whereCommandInitializer = BuildWhereKeySqlParameters(ormProvider, mapProvider, entityType, whereObj, false);
                    var typedWhereCommandInitializer = whereCommandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, object, string>;
                    var builder = new StringBuilder("SELECT ");
                    builder.Append(BuildFieldsSqlPart(ormProvider, mapProvider, entityType, entityType, true));
                    builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                    return typedWhereCommandInitializer.Invoke(command, ormProvider, mapProvider, builder, parameter);
                };
                return typedCommandInitializer;
            });
        }
        return commandInitializer;
    }
    public static object BuildQueryWhereObjSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiQuery)
    {
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, whereObjType);
        var commandInitializerCache = isMultiQuery ? queryMultiWhereObjCommandInitializerCache : queryWhereObjCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var whereCommandInitializer = BuildWhereSqlParameters(ormProvider, mapProvider, entityType, whereObj, isMultiQuery);
            object result = null;
            if (isMultiQuery)
            {
                Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string> commandInitializer;
                var typedWhereCommandInitializer = whereCommandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, string, object, string>;
                commandInitializer = (command, ormProvider, mapProvider, prefix, whereObj) =>
                {
                    var builder = new StringBuilder("SELECT ");
                    builder.Append(BuildFieldsSqlPart(ormProvider, mapProvider, entityType, entityType, true));
                    builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                    builder.Append(typedWhereCommandInitializer.Invoke(command, ormProvider, mapProvider, builder, prefix, whereObj));
                    return builder.ToString();
                };
                result = commandInitializer;
            }
            else
            {
                Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string> commandInitializer;
                var typedWhereCommandInitializer = whereCommandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, object, string>;
                commandInitializer = (command, ormProvider, mapProvider, whereObj) =>
                {
                    var builder = new StringBuilder("SELECT ");
                    builder.Append(BuildFieldsSqlPart(ormProvider, mapProvider, entityType, entityType, true));
                    builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                    builder.Append(typedWhereCommandInitializer.Invoke(command, ormProvider, mapProvider, builder, whereObj));
                    return builder.ToString();
                };
                result = commandInitializer;
            }
            return result;
        });
    }
    public static object BuildExistsSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiQuery)
    {
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, whereObjType);
        var commandInitializerCache = isMultiQuery ? queryMultiExistsCommandInitializerCache : queryExistsCommandInitializerCache;
        return commandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var whereCommandInitializer = BuildWhereSqlParameters(ormProvider, mapProvider, entityType, whereObj, isMultiQuery);
            object result = null;
            if (isMultiQuery)
            {
                Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string> commandInitializer;
                var typedWhereCommandInitializer = whereCommandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, string, object, string>;
                commandInitializer = (command, ormProvider, mapProvider, prefix, whereObj) =>
                {
                    var builder = new StringBuilder($"SELECT COUNT(1) FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                    builder.Append(typedWhereCommandInitializer.Invoke(command, ormProvider, mapProvider, builder, prefix, whereObj));
                    return builder.ToString();
                };
                result = commandInitializer;
            }
            else
            {
                Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string> commandInitializer;
                var typedWhereCommandInitializer = whereCommandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, object, string>;
                commandInitializer = (command, ormProvider, mapProvider, whereObj) =>
                {
                    var builder = new StringBuilder($"SELECT COUNT(1) FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                    builder.Append(typedWhereCommandInitializer.Invoke(command, ormProvider, mapProvider, builder, whereObj));
                    return builder.ToString();
                };
                result = commandInitializer;
            }
            return result;
        });
    }
    public static Action<IDbCommand, IOrmProvider, object> BuildQueryRawSqlParameters(string dbKey, IOrmProvider ormProvider, string rawSql, object parameters)
    {
        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
        if (parameters is IDictionary<string, object> dict)
        {
            Action<IDbCommand, IOrmProvider, IDictionary<string, object>> dictCommandInitializer = null;
            dictCommandInitializer = (command, ormProvider, dict) =>
            {
                foreach (var item in dict)
                {
                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                        continue;
                    var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                    command.Parameters.Add(dbParameter);
                }
            };
            commandInitializer = (command, ormProvider, parameters)
                => dictCommandInitializer.Invoke(command, ormProvider, dict);
        }
        else
        {
            var parameterType = parameters.GetType();
            var cacheKey = HashCode.Combine(dbKey, rawSql, parameterType);
            if (!queryRawSqlCommandInitializerCache.TryGetValue(cacheKey, out commandInitializer))
            {
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
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
                    AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, memberInfo.Name, blockBodies);
                }
                commandInitializer = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
                queryRawSqlCommandInitializerCache.TryAdd(cacheKey, commandInitializer);
            }
        }
        return commandInitializer;
    }

    public static object BuildCreateWithBiesCommandInitializer(ISqlVisitor sqlVisitor, Type entityType, object insertObj, bool isMultiExecute)
    {
        object commandInitializer = null;
        if (insertObj is IDictionary<string, object>)
        {
            if (isMultiExecute)
            {
                Action<IDbCommand, StringBuilder, StringBuilder, string, object> dictCommandInitializer = null;
                dictCommandInitializer = (command, insertBuilder, valuesBuilder, prefix, insertObj) =>
                {
                    int index = 0;
                    var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                    var dict = insertObj as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                            || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                            || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                            continue;

                        if (index > 0)
                        {
                            insertBuilder.Append(',');
                            valuesBuilder.Append(',');
                        }
                        var parameterName = sqlVisitor.OrmProvider.ParameterPrefix + prefix + propMapper.MemberName;
                        insertBuilder.Append(sqlVisitor.OrmProvider.GetFieldName(propMapper.FieldName));
                        valuesBuilder.Append(parameterName);
                        command.Parameters.Add(sqlVisitor.OrmProvider.CreateParameter(propMapper, parameterName, item.Value));
                        index++;
                    }
                };
                commandInitializer = dictCommandInitializer;
            }
            else
            {
                Action<IDbCommand, StringBuilder, StringBuilder, object> dictCommandInitializer = null;
                dictCommandInitializer = (command, insertBuilder, valuesBuilder, insertObj) =>
                {
                    int index = 0;
                    var dict = insertObj as IDictionary<string, object>;
                    var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                            || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                            || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                            continue;

                        if (index > 0)
                        {
                            insertBuilder.Append(',');
                            valuesBuilder.Append(',');
                        }
                        var parameterName = sqlVisitor.OrmProvider.ParameterPrefix + propMapper.MemberName;
                        insertBuilder.Append(sqlVisitor.OrmProvider.GetFieldName(propMapper.FieldName));
                        valuesBuilder.Append(parameterName);
                        command.Parameters.Add(sqlVisitor.OrmProvider.CreateParameter(propMapper, parameterName, item.Value));
                        index++;
                    }
                };
                commandInitializer = dictCommandInitializer;
            }
        }
        else
        {
            var parameterType = insertObj.GetType();
            if (!parameterType.IsEntityType(out _))
                throw new NotSupportedException("只支持类对象，不支持基础类型");

            var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
            var commandInitializerCache = isMultiExecute ? createMultiWithByCommandInitializerCache : createWithByCommandInitializerCache;
            commandInitializer = commandInitializerCache.GetOrAdd(cacheKey, f =>
            {
                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");
                var fieldsBuilderExpr = Expression.Parameter(typeof(StringBuilder), "fieldsBuilder");
                var valueBuilderExpr = Expression.Parameter(typeof(StringBuilder), "valueBuilder");
                ParameterExpression prefixExpr = null;
                ParameterExpression parameterNameExpr = null;
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();

                if (isMultiExecute)
                {
                    prefixExpr = Expression.Parameter(typeof(string), "prefix");
                    parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                    blockParameters.Add(parameterNameExpr);
                }
                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                var dbParametersExpr = Expression.Property(commandExpr, nameof(IDbCommand.Parameters));
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
                            prefixExpr, Expression.Constant(memberMapper.MemberName));
                        blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));
                        myParameterNameExpr = parameterNameExpr;
                    }
                    else myParameterNameExpr = Expression.Constant(sqlVisitor.OrmProvider.ParameterPrefix + memberMapper.MemberName);

                    blockBodies.Add(Expression.Call(valueBuilderExpr, appendMethodInfo2, myParameterNameExpr));
                    AddMemberParameter(commandExpr, ormProviderExpr, myParameterNameExpr, typedParameterExpr, memberMapper, blockBodies);
                    index++;
                }
                if (isMultiExecute) commandInitializer = Expression.Lambda<Action<IDbCommand, StringBuilder, StringBuilder, string, object>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, fieldsBuilderExpr, valueBuilderExpr, prefixExpr, parameterExpr).Compile();
                else commandInitializer = Expression.Lambda<Action<IDbCommand, StringBuilder, StringBuilder, object>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, fieldsBuilderExpr, valueBuilderExpr, parameterExpr).Compile();
                return commandInitializer;
            });
        }
        return commandInitializer;
    }
    public static object BuildCreateWithBulkCommandInitializer(ISqlVisitor sqlVisitor, Type entityType, object insertObjs, bool isMultiExecute, out string headSql)
    {
        var entities = insertObjs as IEnumerable;
        headSql = null;
        object parameter = null, commandInitializer = null;
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
                if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                    || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                    || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(sqlVisitor.OrmProvider.GetFieldName(propMapper.FieldName));
                index++;
            }
            headSql = builder.ToString();
            if (isMultiExecute)
            {
                Action<IDbCommand, StringBuilder, string, object, int> dictCommandInitializer = null;
                dictCommandInitializer = (command, builder, prefix, insertObj, bulkIndex) =>
                {
                    int index = 0;
                    var dict = insertObj as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                            || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                            || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                            continue;

                        if (index > 0) builder.Append(',');
                        var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}{prefix}{item.Key}{bulkIndex}";
                        builder.Append(parameterName);
                        command.Parameters.Add(sqlVisitor.OrmProvider.CreateParameter(propMapper, parameterName, item.Value));
                        index++;
                    }
                };
                commandInitializer = dictCommandInitializer;
            }
            else
            {
                Action<IDbCommand, StringBuilder, object, int> dictCommandInitializer = null;
                dictCommandInitializer = (command, builder, insertObj, bulkIndex) =>
                {
                    int index = 0;
                    var dict = insertObj as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                            || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                            || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                            continue;

                        if (index > 0) builder.Append(',');
                        var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}{item.Key}{bulkIndex}";
                        builder.Append(parameterName);
                        command.Parameters.Add(sqlVisitor.OrmProvider.CreateParameter(propMapper, parameterName, item.Value));
                        index++;
                    }
                };
                commandInitializer = dictCommandInitializer;
            }
        }
        else
        {
            var parameterType = parameter.GetType();
            if (!parameterType.IsEntityType(out _))
                throw new NotSupportedException("只支持类对象，不支持基础类型");
            headSql = BuildFieldsSqlPart(sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType, false);

            var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
            var commandInitializerCache = isMultiExecute ? createMultiBulkCommandInitializerCache : createBulkCommandInitializerCache;
            commandInitializer = commandInitializerCache.GetOrAdd(cacheKey, f =>
            {
                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
                var insertObjExpr = Expression.Parameter(typeof(object), "insertObj");
                var bulkIndexExpr = Expression.Parameter(typeof(int), "bulkIndex");

                ParameterExpression prefixExpr = null;
                ParameterExpression parameterNameExpr = null;
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                if (isMultiExecute)
                {
                    prefixExpr = Expression.Parameter(typeof(string), "prefix");
                    parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                    blockParameters.Add(parameterNameExpr);
                }

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var strIndexExpr = Expression.Variable(typeof(string), "strIndex");
                blockParameters.AddRange(new[] { typedParameterExpr, strIndexExpr });
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(insertObjExpr, parameterType)));
                var ormProviderExpr = Expression.Constant(sqlVisitor.OrmProvider);

                var appendMethodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
                var appendMethodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                var concatMethodInfo1 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) });
                var concatMethodInfo2 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });
                var toStringMethodInfo = typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes);
                blockBodies.Add(Expression.Assign(strIndexExpr, Expression.Call(bulkIndexExpr, toStringMethodInfo)));
                var parameterPrefixExpr = Expression.Constant(sqlVisitor.OrmProvider.ParameterPrefix);

                var index = 0;
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    if (index > 0)
                        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo1, Expression.Constant(',')));

                    Expression concatExpr = null;
                    if (isMultiExecute) concatExpr = Expression.Call(concatMethodInfo1,
                        parameterPrefixExpr, prefixExpr, Expression.Constant(propMapper.MemberName), strIndexExpr);
                    else concatExpr = Expression.Call(concatMethodInfo2,
                        parameterPrefixExpr, Expression.Constant(propMapper.MemberName), strIndexExpr);

                    blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo2, parameterNameExpr));
                    AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, propMapper, blockBodies);
                    index++;
                }
                object result = null;
                if (isMultiExecute) result = Expression.Lambda<Action<IDbCommand, StringBuilder, string, object, int>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, builderExpr, prefixExpr, insertObjExpr, bulkIndexExpr).Compile();
                else result = Expression.Lambda<Action<IDbCommand, StringBuilder, object, int>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, builderExpr, insertObjExpr, bulkIndexExpr).Compile();
                return result;
            });
        }
        return commandInitializer;
    }

    public static object BuildUpdateSqlParameters(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object updateObj, bool isMultiExecute)
    {
        object commandInitializer = null;
        if (updateObj is IDictionary<string, object> dict)
        {
            if (isMultiExecute)
            {
                Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string> dictCommandInitializer = null;
                dictCommandInitializer = (command, ormProvider, mapProvider, prefix, parameter) =>
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
                            parameterName = $"{ormProvider.ParameterPrefix}{prefix}k{item.Key}";
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
                        var dbParameter = ormProvider.CreateParameter(memberMapper, parameterName, item.Value);
                        command.Parameters.Add(dbParameter);

                    }
                    return fieldsBuilder.Append(whereBuilder).ToString();
                };
                commandInitializer = dictCommandInitializer;
            }
            else
            {
                Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string> dictCommandInitializer = null;
                dictCommandInitializer = (command, ormProvider, mapProvider, parameter) =>
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
                        var dbParameter = ormProvider.CreateParameter(memberMapper, parameterName, item.Value);
                        command.Parameters.Add(dbParameter);

                    }
                    return fieldsBuilder.Append(whereBuilder).ToString();
                };
                commandInitializer = dictCommandInitializer;
            }
        }
        else
        {
            var updateObjType = updateObj.GetType();
            var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, updateObjType);
            var commandInitializerCache = isMultiExecute ? updateMultiCommandInitializerCache : updateCommandInitializerCache;
            commandInitializer = commandInitializerCache.GetOrAdd(cacheKey, f =>
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var memberInfos = updateObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var mapProviderExpr = Expression.Parameter(typeof(IEntityMapProvider), "mapProvider");
                var updateObjExpr = Expression.Parameter(typeof(object), "updateObj");

                var typedUpdateObjExpr = Expression.Variable(updateObjType, "typedUpdateObj");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedUpdateObjExpr);
                blockBodies.Add(Expression.Assign(typedUpdateObjExpr, Expression.Convert(updateObjExpr, updateObjType)));

                StringBuilder builder = null;
                ParameterExpression prefixExpr = null;
                ParameterExpression parameterNameExpr = null;
                ParameterExpression builderExpr = null;
                if (isMultiExecute)
                {
                    prefixExpr = Expression.Parameter(typeof(string), "prefix");
                    builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
                    parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                    blockParameters.AddRange(new[] { builderExpr, parameterNameExpr });
                    var sqlExpr = Expression.Constant($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
                    var constructor = typeof(StringBuilder).GetConstructor(new Type[] { typeof(string) });
                    blockBodies.Add(Expression.Assign(builderExpr, Expression.New(constructor, sqlExpr)));
                }
                else builder = new StringBuilder($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");

                var dbParametersExpr = Expression.PropertyOrField(commandExpr, nameof(IDbCommand.Parameters));
                var methodInfo1 = typeof(Extensions).GetMethod(nameof(Extensions.CreateParameter));
                var methodInfo2 = typeof(IList).GetMethod(nameof(IList.Add));
                var appendMethodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
                var appendMethodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });
                int index = 0;
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;
                    if (memberMapper.IsKey) continue;

                    Expression myParameterNameExpr = null;
                    if (isMultiExecute)
                    {
                        if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo1, Expression.Constant(',')));
                        var parameterPrefixExpr = Expression.Constant(ormProvider.ParameterPrefix);
                        var concatExpr = Expression.Call(concatMethodInfo, parameterPrefixExpr, prefixExpr, Expression.Constant(memberMapper.MemberName));
                        blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));
                        myParameterNameExpr = parameterNameExpr;
                        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo2, Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}=")));
                        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo2, parameterNameExpr));
                    }
                    else
                    {
                        if (index > 0) builder.Append(',');
                        var parameterName = ormProvider.ParameterPrefix + memberInfo.Name;
                        myParameterNameExpr = Expression.Constant(parameterName);
                        builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                    }
                    AddMemberParameter(commandExpr, ormProviderExpr, myParameterNameExpr, typedUpdateObjExpr, memberMapper, blockBodies);
                    index++;
                }
                index = 0;
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    Expression myParameterNameExpr = null;
                    if (isMultiExecute)
                    {
                        if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo2, Expression.Constant(" WHERE ")));
                        var parameterPrefixExpr = Expression.Constant(ormProvider.ParameterPrefix);
                        var concatExpr = Expression.Call(concatMethodInfo, parameterPrefixExpr, prefixExpr, Expression.Constant($"k{keyMapper.MemberName}"));
                        blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));
                        myParameterNameExpr = parameterNameExpr;
                        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo2, Expression.Constant($"{ormProvider.GetFieldName(keyMapper.FieldName)}=")));
                        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo2, parameterNameExpr));
                    }
                    else
                    {
                        if (index > 0) builder.Append(" WHERE ");
                        var parameterName = $"{ormProvider.ParameterPrefix}k{keyMapper.MemberName}";
                        myParameterNameExpr = Expression.Constant(parameterName);
                        builder.Append($"{ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");
                    }
                    AddMemberParameter(commandExpr, ormProviderExpr, myParameterNameExpr, typedUpdateObjExpr, keyMapper, blockBodies);
                    index++;
                }
                Expression returnExpr = null;
                if (isMultiExecute)
                {
                    var methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
                    returnExpr = Expression.Call(builderExpr, methodInfo);
                }
                else returnExpr = Expression.Constant(builder.ToString());

                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));

                object result = null;
                if (isMultiExecute) result = Expression.Lambda<Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, mapProviderExpr, prefixExpr, updateObjExpr).Compile();
                else result = Expression.Lambda<Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, mapProviderExpr, updateObjExpr).Compile();
                return result;
            });
        }
        return commandInitializer;
    }
    public static Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, object, int> BuildUpdateBulkCommandInitializer(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object parameters)
    {
        Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, object, int> commandInitializer = null;
        var entities = parameters as IEnumerable;
        object parameter = null;
        foreach (var entity in entities)
        {
            parameter = entity;
            break;
        }
        if (parameter is IDictionary<string, object> dict)
        {
            commandInitializer = (command, ormProvider, mapProvider, builder, parameter, index) =>
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

                    if (memberMapper.IsKey)
                    {
                        var parameterName = $"{ormProvider.ParameterPrefix}k{item.Key}{index}";
                        whereBuilder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        keyDbParameters.Add(ormProvider.CreateParameter(memberMapper, parameterName, dict[item.Key]));
                    }
                    else
                    {
                        var parameterName = $"{ormProvider.ParameterPrefix}{item.Key}{index}";
                        builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        command.Parameters.Add(ormProvider.CreateParameter(memberMapper, parameterName, dict[item.Key]));
                    }
                }
                builder.Append(whereBuilder);
                keyDbParameters.ForEach(f => command.Parameters.Add(f));
            };
        }
        else
        {
            var updateObjType = parameter.GetType();
            var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, updateObjType);
            var objCommandInitializer = updateBulkCommandInitializerCache.GetOrAdd(cacheKey, f =>
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var memberInfos = updateObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "command");
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
                var appendMethodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                var appendMethodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
                var methodInfo = typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes);
                var updateHeadExpr = Expression.Constant($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
                blockBodies.Add(Expression.Assign(typedUpdateObjExpr, Expression.Convert(updateObjExpr, updateObjType)));
                blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo1, updateHeadExpr));
                blockBodies.Add(Expression.Assign(strIndexExpr, Expression.Call(bulkIndexExpr, methodInfo)));
                var dbParametersExpr = Expression.Property(commandExpr, nameof(IDbCommand.Parameters));

                int index = 0;
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                       || memberMapper.IsIgnore || memberMapper.IsNavigation
                       || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;
                    if (memberMapper.IsKey) continue;

                    if (index > 0) blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo2, Expression.Constant(',')));

                    var parameterName = $"{ormProvider.ParameterPrefix}{memberInfo.Name}";
                    var concatExpr = Expression.Call(concatMethodInfo, Expression.Constant(parameterName), strIndexExpr);
                    blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));
                    var setExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}=");
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo1, setExpr));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo1, parameterNameExpr));

                    AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedUpdateObjExpr, memberMapper, blockBodies);
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

                    if (index > 0) blockBodies.Add(Expression.Call(whereBuilderExpr, appendMethodInfo1, Expression.Constant(" AND ")));
                    var parameterName = $"{ormProvider.ParameterPrefix}k{keyMapper.MemberName}";
                    var callExpr = Expression.Call(concatMethodInfo, Expression.Constant(parameterName), strIndexExpr);
                    blockBodies.Add(Expression.Assign(parameterNameExpr, callExpr));
                    var setExpr = Expression.Constant($"{ormProvider.GetFieldName(keyMapper.FieldName)}=");
                    blockBodies.Add(Expression.Call(whereBuilderExpr, appendMethodInfo1, setExpr));
                    blockBodies.Add(Expression.Call(whereBuilderExpr, appendMethodInfo1, parameterNameExpr));

                    AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedUpdateObjExpr, keyMapper, blockBodies);
                    index++;
                }
                methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(StringBuilder) });
                blockBodies.Add(Expression.Call(builderExpr, methodInfo, whereBuilderExpr));
                return Expression.Lambda<Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, object, int>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, mapProviderExpr, builderExpr, updateObjExpr, bulkIndexExpr).Compile();
            });
        }
        return commandInitializer;
    }





    public static object BuildUpdateWithParameters(ISqlVisitor sqlVisitor, Type entityType, object updateObj, bool isWhere, bool isMultiExecute)
    {
        object commandInitializer = null;
        if (updateObj is IDictionary<string, object>)
        {
            if (isMultiExecute)
            {
                Action<IDbCommand, List<UpdateField>, object, int> dictSetFieldsInitializer = null;
                if (isWhere) dictSetFieldsInitializer = (command, setFields, updateObj, commandIndex) =>
                {
                    var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                    var dict = updateObj as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}k{item.Key}_m{commandIndex}";
                        setFields.Add(new UpdateField { MemberMapper = memberMapper, Value = parameterName });
                        command.Parameters.Add(sqlVisitor.OrmProvider.CreateParameter(memberMapper, parameterName, dict[item.Key]));
                    }
                };
                else dictSetFieldsInitializer = (command, setFields, updateObj, commandIndex) =>
                {
                    var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                    var dict = updateObj as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}{item.Key}_m{commandIndex}";
                        setFields.Add(new UpdateField { MemberMapper = memberMapper, Value = parameterName });
                        command.Parameters.Add(sqlVisitor.OrmProvider.CreateParameter(memberMapper, parameterName, dict[item.Key]));
                    }
                };
                commandInitializer = dictSetFieldsInitializer;
            }
            else
            {
                Action<IDbCommand, List<UpdateField>, object> dictSetFieldsInitializer = null;
                if (isWhere) dictSetFieldsInitializer = (command, setFields, updateObj) =>
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
                        setFields.Add(new UpdateField { MemberMapper = memberMapper, Value = parameterName });
                        command.Parameters.Add(sqlVisitor.OrmProvider.CreateParameter(memberMapper, parameterName, dict[item.Key]));
                    }
                };
                else dictSetFieldsInitializer = (command, setFields, updateObj) =>
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
                        setFields.Add(new UpdateField { MemberMapper = memberMapper, Value = parameterName });
                        command.Parameters.Add(sqlVisitor.OrmProvider.CreateParameter(memberMapper, parameterName, dict[item.Key]));
                    }
                };
                commandInitializer = dictSetFieldsInitializer;
            }
        }
        else
        {
            var parameterType = updateObj.GetType();
            var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
            var commandInitializerCache = isMultiExecute ? updateMultiSetFieldsCommandInitializerCache : updateSetFieldsCommandInitializerCache;
            if (!commandInitializerCache.TryGetValue(cacheKey, out commandInitializer))
            {
                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var updateFieldsExpr = Expression.Parameter(typeof(List<UpdateField>), "updateFields");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");
                ParameterExpression commandIndexExpr = null;
                if (isMultiExecute) commandIndexExpr = Expression.Parameter(typeof(int), "commandIndex");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();

                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
                var ormProviderExpr = Expression.Constant(sqlVisitor.OrmProvider);
                var dbParametersExpr = Expression.Property(commandExpr, nameof(IDbCommand.Parameters));

                var addMethodInfo1 = typeof(IList).GetMethod(nameof(IList.Add));
                var addMethodInfo2 = typeof(List<UpdateField>).GetMethod(nameof(List<UpdateField>.Add));
                var createParameterMethodInfo = typeof(Extensions).GetMethod(nameof(Extensions.CreateParameter));
                var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
                var toStringMethodInfo = typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes);
                var setTypeMethodInfo = typeof(UpdateField).GetProperty(nameof(UpdateField.Type)).GetSetMethod();
                var setMapperMethodInfo = typeof(UpdateField).GetProperty(nameof(UpdateField.MemberMapper)).GetSetMethod();
                var setValueMethodInfo = typeof(UpdateField).GetProperty(nameof(UpdateField.Value)).GetSetMethod();

                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;

                    var parameterName = sqlVisitor.OrmProvider.ParameterPrefix + memberInfo.Name;
                    Expression parameterNameExpr = null;
                    Expression updateFieldExpr = null;
                    var memberMapperExpr = Expression.Constant(memberMapper);

                    if (isMultiExecute)
                    {
                        var toStringExpr = Expression.Call(commandIndexExpr, toStringMethodInfo);
                        parameterNameExpr = Expression.Constant(sqlVisitor.OrmProvider.ParameterPrefix + memberMapper.MemberName + "_m");
                        parameterNameExpr = Expression.Call(concatMethodInfo, parameterNameExpr, toStringExpr);

                        updateFieldExpr = Expression.Variable(typeof(UpdateField), "updateField");
                        if (isWhere) blockBodies.Add(Expression.Call(updateFieldExpr, setTypeMethodInfo, Expression.Constant(UpdateFieldType.Where)));
                        else blockBodies.Add(Expression.Call(updateFieldExpr, setTypeMethodInfo, Expression.Constant(UpdateFieldType.SetField)));

                        blockBodies.Add(Expression.Call(updateFieldExpr, setMapperMethodInfo, memberMapperExpr));
                        blockBodies.Add(Expression.Call(updateFieldExpr, setValueMethodInfo, parameterNameExpr));
                    }
                    else
                    {
                        parameterNameExpr = Expression.Constant(sqlVisitor.OrmProvider.ParameterPrefix + memberMapper.MemberName);
                        UpdateField updateField = default;
                        if (isWhere) updateField = new UpdateField { Type = UpdateFieldType.Where, MemberMapper = memberMapper, Value = parameterName };
                        else updateField = new UpdateField { Type = UpdateFieldType.SetField, MemberMapper = memberMapper, Value = parameterName };
                        updateFieldExpr = Expression.Constant(updateField);
                    }
                    blockBodies.Add(Expression.Call(updateFieldsExpr, addMethodInfo2, updateFieldExpr));

                    Expression fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
                    if (fieldValueExpr.Type != typeof(object))
                        fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));

                    var dbParameterExpr = Expression.Call(createParameterMethodInfo, ormProviderExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr);
                    blockBodies.Add(Expression.Call(dbParametersExpr, addMethodInfo1, dbParameterExpr));
                    blockBodies.Add(Expression.Call(updateFieldsExpr, addMethodInfo2, updateFieldExpr));
                }

                if (isMultiExecute) commandInitializer = Expression.Lambda<Action<IDbCommand, List<UpdateField>, object, int>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, updateFieldsExpr, parameterExpr, commandIndexExpr).Compile();
                else commandInitializer = Expression.Lambda<Action<IDbCommand, List<UpdateField>, object>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, updateFieldsExpr, parameterExpr).Compile();
                commandInitializerCache.TryAdd(cacheKey, commandInitializer);
            }
        }
        return commandInitializer;
    }

    public static object BuildDeleteCommandInitializer(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj, bool isMultiExecute)
    {
        object commandInitializer = null;
        var whereObjType = whereObj.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, mapProvider, entityType, whereObjType);
        if (isMultiExecute)
        {
            commandInitializer = queryGetCommandInitializerCache.GetOrAdd(cacheKey, f =>
            {
                Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string> typedCommandInitializer = null;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                typedCommandInitializer = (command, ormProvider, mapProvider, prefix, parameter) =>
                {
                    var whereCommandInitializer = BuildWhereKeySqlParameters(ormProvider, mapProvider, entityType, whereObj, false);
                    var typedWhereCommandInitializer = whereCommandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, string, object, string>;
                    var builder = new StringBuilder($"DELETE FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                    return typedWhereCommandInitializer.Invoke(command, ormProvider, mapProvider, builder, prefix, parameter);
                };
                return typedCommandInitializer;
            });
        }
        else
        {
            commandInitializer = queryGetCommandInitializerCache.GetOrAdd(cacheKey, f =>
            {
                Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string> typedCommandInitializer = null;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                typedCommandInitializer = (command, ormProvider, mapProvider, parameter) =>
                {
                    var whereCommandInitializer = BuildWhereKeySqlParameters(ormProvider, mapProvider, entityType, whereObj, false);
                    var typedWhereCommandInitializer = whereCommandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, object, string>;
                    var builder = new StringBuilder($"DELETE FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                    return typedWhereCommandInitializer.Invoke(command, ormProvider, mapProvider, builder, parameter);
                };
                return typedCommandInitializer;
            });
        }
        return commandInitializer;
    }

    public static object BuildWhereWithKeysSqlParameters(ISqlVisitor sqlVisitor, Type entityType, object parameters, bool isMultiExecute)
    {
        object commandInitializer = null;
        if (parameters is IDictionary<string, object>)
        {
            if (isMultiExecute)
            {
                Func<IDbCommand, object, int, string> dictCommandInitializer = null;
                dictCommandInitializer = (command, whereObj, commandIndex) =>
                {
                    int index = 0;
                    var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                    var dict = whereObj as IDictionary<string, object>;
                    var builder = new StringBuilder();
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                            || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                            || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                            continue;

                        if (index > 0) builder.Append(" AND ");
                        var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}k{item.Key}_m{commandIndex}";
                        builder.Append(sqlVisitor.OrmProvider.GetFieldName(propMapper.FieldName));
                        command.Parameters.Add(sqlVisitor.OrmProvider.CreateParameter(propMapper, parameterName, item.Value));
                        index++;
                    }
                    return builder.ToString();
                };
                commandInitializer = dictCommandInitializer;
            }
            else
            {
                Func<IDbCommand, object, string> dictCommandInitializer = null;
                dictCommandInitializer = (command, whereObj) =>
                {
                    int index = 0;
                    var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                    var builder = new StringBuilder();
                    var dict = whereObj as IDictionary<string, object>;
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                            || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                            || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                            continue;

                        if (index > 0) builder.Append(" AND ");
                        var parameterName = sqlVisitor.OrmProvider.ParameterPrefix + "k" + item.Key;
                        builder.Append(sqlVisitor.OrmProvider.GetFieldName(propMapper.FieldName));
                        command.Parameters.Add(sqlVisitor.OrmProvider.CreateParameter(propMapper, parameterName, item.Value));
                        index++;
                    }
                    return builder.ToString();
                };
                commandInitializer = dictCommandInitializer;
            }
        }
        else
        {
            var parameterType = parameters.GetType();
            if (!parameterType.IsEntityType(out _))
                throw new NotSupportedException("只支持类对象，不支持基础类型");

            var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
            var whereWithCache = isMultiExecute ? mutilWhereWithKeysCommandInitializerCache : whereWithKeysCommandInitializerCache;
            if (!whereWithCache.TryGetValue(cacheKey, out commandInitializer))
            {
                int columnIndex = 0;
                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
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
                var dbParametersExpr = Expression.Property(commandExpr, nameof(IDbCommand.Parameters));
                var ormProviderExpr = Expression.Constant(sqlVisitor.OrmProvider);

                var addMethodInfo = typeof(IList).GetMethod(nameof(IList.Add));
                var createParameterMethodInfo = typeof(Extensions).GetMethod(nameof(Extensions.CreateParameter));
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
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    if (columnIndex > 0)
                    {
                        if (isMultiExecute)
                            blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                        else builder.Append(" AND ");
                    }
                    var fieldName = sqlVisitor.OrmProvider.GetFieldName(propMapper.FieldName);
                    var parameterName = sqlVisitor.OrmProvider.ParameterPrefix + "k" + propMapper.MemberName;
                    var memberMapperExpr = Expression.Constant(propMapper);
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

                    Expression fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, propMapper.MemberName);
                    if (fieldValueExpr.Type != typeof(object))
                        fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));

                    var dbParameterExpr = Expression.Call(createParameterMethodInfo, ormProviderExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr);
                    blockBodies.Add(Expression.Call(dbParametersExpr, addMethodInfo, dbParameterExpr));
                    columnIndex++;
                }
                var methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
                Expression returnExpr = isMultiExecute ? Expression.Call(builderExpr, methodInfo) : Expression.Constant(builder.ToString());
                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                if (isMultiExecute) commandInitializer = Expression.Lambda<Action<IDbCommand, object, int>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, parameterExpr, commandIndexExpr).Compile();
                else commandInitializer = Expression.Lambda<Func<IDbCommand, object, string>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, parameterExpr).Compile();
                whereWithCache.TryAdd(cacheKey, commandInitializer);
            }
        }
        return commandInitializer;
    }
}