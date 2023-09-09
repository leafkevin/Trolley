﻿using System;
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
    private static ConcurrentDictionary<int, string> querySqlPartCache = new();
    private static ConcurrentDictionary<int, Func<IDbCommand, IOrmProvider, string, object, string>> queryGetCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Func<IDbCommand, IOrmProvider, string, object, string>> queryWhereObjCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Action<IDbCommand, IOrmProvider, object>> queryRawSqlCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Func<IDbCommand, IOrmProvider, string, object, string>> queryExistsCommandInitializerCache = new();

    private static ConcurrentDictionary<int, Action<IDbCommand, ISqlVisitor, StringBuilder, int, object>> createBatchCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Action<ISqlVisitor, List<IDbDataParameter>, object, StringBuilder, StringBuilder>> createWithByCommandInitializerCache = new();

    private static ConcurrentDictionary<int, Func<IDbCommand, IOrmProvider, object, string>> updateEntityCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Action<IDbCommand, IOrmProvider, StringBuilder, int, object>> updateEntityBatchCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Action<ISqlVisitor, List<UpdateField>, List<IDbDataParameter>, object>> updateSetFieldsCache = new();
    private static ConcurrentDictionary<int, Action<ISqlVisitor, List<UpdateField>, List<IDbDataParameter>, object>> updateWhereWithCache = new();

    private static ConcurrentDictionary<int, Action<IDbCommand, IOrmProvider, StringBuilder, int, object>> deleteBatchCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Func<IDbCommand, IOrmProvider, object, string>> deleteCommandInitializerCache = new();

    private static ConcurrentDictionary<int, Func<ISqlVisitor, List<IDbDataParameter>, object, string>> whereWithKeysCommandInitializerCache = new();

    public static void AddValueParameter(ParameterExpression commandExpr, Expression ormProviderExpr, Expression parameterNameExpr,
        Expression parameterValueExpr, MemberMap memberMapper, List<Expression> blockBodies)
    {
        MethodInfo methodInfo = null;
        Expression dbParameterExpr = null;
        var fieldValueExpr = parameterValueExpr;
        if (parameterValueExpr.Type != typeof(object))
            fieldValueExpr = Expression.Convert(parameterValueExpr, typeof(object));
        if (memberMapper != null)
        {
            var memberMapperExpr = Expression.Constant(memberMapper);
            methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.CreateParameter));
            dbParameterExpr = Expression.Call(methodInfo, ormProviderExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr);
        }
        else
        {
            //var dbParameter = ormProvider.CreateParameter("@Name", objValue);
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
            dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, fieldValueExpr);
        }
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

    public static string BuildQuerySqlPart(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType)
    {
        var sqlCacheKey = HashCode.Combine(connection, ormProvider, mapProvider, entityType);
        if (!querySqlPartCache.TryGetValue(sqlCacheKey, out var sql))
        {
            var index = 0;
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var builder = new StringBuilder("SELECT ");
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || propMapper.IsNavigation
                    || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                if (propMapper.FieldName != propMapper.MemberName)
                    builder.Append(" AS " + ormProvider.GetFieldName(propMapper.MemberName));
                index++;
            }
            builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)}");
            sql = builder.ToString();
            querySqlPartCache.TryAdd(sqlCacheKey, sql);
        }
        return sql;
    }
    public static Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string> BuildGetSqlParameters(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj)
    {
        Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string> commandInitializer = null;
        if (whereObj is IDictionary<string, object> dict)
        {
            Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, IDictionary<string, object>, string> dictCommandInitializer = null;
            dictCommandInitializer = (command, ormProvider, mapProvider, prefix, dict) =>
            {
                var index = 0;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var sql = BuildQuerySqlPart(connection, ormProvider, mapProvider, entityType);
                var builder = new StringBuilder(sql + " WHERE ");
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    if (!dict.TryGetValue(keyMapper.MemberName, out var fieldValue))
                        throw new ArgumentNullException("whereObj", $"字典参数缺少主键字段{keyMapper.MemberName}");

                    if (index > 0) builder.Append(" AND ");
                    var parameterName = $"{ormProvider.ParameterPrefix}{prefix}{keyMapper.MemberName}";
                    builder.Append($"{ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");
                    command.Parameters.Add(ormProvider.CreateParameter(keyMapper, parameterName, fieldValue));
                    index++;
                }
                return builder.ToString();
            };
            commandInitializer = (command, ormProvider, mapProvider, prefix, parameters)
                => dictCommandInitializer.Invoke(command, ormProvider, mapProvider, prefix, dict);
        }
        else
        {
            var whereObjType = whereObj.GetType();
            var cacheKey = HashCode.Combine(connection, ormProvider, mapProvider, entityType, whereObjType);
            if (!queryGetCommandInitializerCache.TryGetValue(cacheKey, out var objCommandInitializer))
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var prefixExpr = Expression.Parameter(typeof(string), "prefix");
                var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

                ParameterExpression typedWhereObjExpr = null;
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                bool isEntityType = false;
                List<MemberInfo> memberInfos = null;

                var builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
                blockParameters.Add(builderExpr);
                var constructor = typeof(StringBuilder).GetConstructor(new Type[] { typeof(string) });
                var getSqlPart = BuildQuerySqlPart(connection, ormProvider, mapProvider, entityType);
                var wherePrefixExpr = Expression.Constant(getSqlPart + " WHERE ");
                blockBodies.Add(Expression.Assign(builderExpr, Expression.New(constructor, wherePrefixExpr)));

                if (whereObjType.IsEntityType(out _))
                {
                    isEntityType = true;
                    typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");
                    blockParameters.Add(typedWhereObjExpr);
                    blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));
                    memberInfos = whereObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                }
                else
                {
                    if (entityMapper.KeyMembers.Count > 1)
                        throw new NotSupportedException($"模型{entityType.FullName}有多个主键字段，不能使用单个值类型{whereObjType.FullName}作为参数");
                }

                var index = 0;
                var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });
                var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    if (isEntityType && !memberInfos.Exists(f => f.Name == keyMapper.MemberName))
                        throw new ArgumentNullException("whereObj", $"参数类型{whereObjType.FullName}缺少主键字段{keyMapper.MemberName}");

                    var parameterNameExpr = Expression.Call(concatMethodInfo,
                        Expression.Constant(ormProvider.ParameterPrefix), prefixExpr, Expression.Constant(keyMapper.MemberName));
                    if (index > 0)
                        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant($"{ormProvider.GetFieldName(keyMapper.FieldName)}=")));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));

                    if (isEntityType)
                        AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, keyMapper, blockBodies);
                    else AddValueParameter(commandExpr, ormProviderExpr, parameterNameExpr, whereObjExpr, keyMapper, blockBodies);
                    index++;
                }
                var methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
                var returnExpr = Expression.Call(builderExpr, methodInfo);
                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                objCommandInitializer = Expression.Lambda<Func<IDbCommand, IOrmProvider, string, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, prefixExpr, whereObjExpr).Compile();
                queryGetCommandInitializerCache.TryAdd(cacheKey, objCommandInitializer);
            }
            commandInitializer = (command, ormProvider, mapProvider, prefix, parameters)
                => objCommandInitializer.Invoke(command, ormProvider, prefix, parameters);
        }
        return commandInitializer;
    }
    public static Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string> BuildQueryWhereObjSqlParameters(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj)
    {
        Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string> commandInitializer = null;
        if (whereObj is IDictionary<string, object> dict)
        {
            Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, IDictionary<string, object>, string> dictCommandInitializer = null;
            dictCommandInitializer = (command, ormProvider, mapProvider, prefix, dict) =>
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var sql = BuildQuerySqlPart(connection, ormProvider, mapProvider, entityType);
                var builder = new StringBuilder(sql + " WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + prefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    command.Parameters.Add(ormProvider.CreateParameter(propMapper, parameterName, item.Value));
                    index++;
                }
                return builder.ToString();
            };
            commandInitializer = (command, ormProvider, mapProvider, prefix, parameters)
                => dictCommandInitializer.Invoke(command, ormProvider, mapProvider, prefix, dict);
        }
        else
        {
            var whereObjType = whereObj.GetType();
            var cacheKey = HashCode.Combine(connection, ormProvider, mapProvider, entityType, whereObjType);
            if (!queryWhereObjCommandInitializerCache.TryGetValue(cacheKey, out var objCommandInitializer))
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var memberInfos = whereObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var prefixExpr = Expression.Parameter(typeof(string), "prefix");
                var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

                var typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedWhereObjExpr);
                blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));

                var builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
                blockParameters.Add(builderExpr);
                var constructor = typeof(StringBuilder).GetConstructor(new Type[] { typeof(string) });
                var sql = BuildQuerySqlPart(connection, ormProvider, mapProvider, entityType);
                var wherePrefixExpr = Expression.Constant(sql + " WHERE ");
                blockBodies.Add(Expression.Assign(builderExpr, Expression.New(constructor, wherePrefixExpr)));

                var index = 0;
                var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });
                var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    var parameterNameExpr = Expression.Call(concatMethodInfo,
                       Expression.Constant(ormProvider.ParameterPrefix), prefixExpr, Expression.Constant(propMapper.MemberName));
                    if (index > 0)
                        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant($"{ormProvider.GetFieldName(propMapper.FieldName)}=")));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
                    AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, propMapper, blockBodies);
                    index++;
                }
                var methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.ToString), Type.EmptyTypes);
                var returnExpr = Expression.Call(builderExpr, methodInfo);
                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                objCommandInitializer = Expression.Lambda<Func<IDbCommand, IOrmProvider, string, object, string>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, prefixExpr, whereObjExpr).Compile();
                queryWhereObjCommandInitializerCache.TryAdd(cacheKey, objCommandInitializer);
            }
            commandInitializer = (command, ormProvider, mapProvider, prefix, parameters)
                => objCommandInitializer.Invoke(command, ormProvider, prefix, parameters);
        }
        return commandInitializer;
    }
    public static Action<IDbCommand, IOrmProvider, object> BuildQueryRawSqlParameters(IDbConnection connection, IOrmProvider ormProvider, string rawSql, object parameters)
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
            var cacheKey = HashCode.Combine(connection, rawSql, parameterType);
            if (!queryRawSqlCommandInitializerCache.TryGetValue(cacheKey, out commandInitializer))
            {
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
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
    public static Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string> BuildExistsSqlParameters(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj)
    {
        Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string> commandInitializer = null;
        if (whereObj is IDictionary<string, object> dict)
        {
            Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, IDictionary<string, object>, string> dictCommandInitializer = null;
            dictCommandInitializer = (command, ormProvider, mapProvider, prefix, dict) =>
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var builder = new StringBuilder($"SELECT COUNT(1) FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + prefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    command.Parameters.Add(ormProvider.CreateParameter(propMapper, parameterName, item.Value));
                    index++;
                }
                return builder.ToString();
            };
            commandInitializer = (command, ormProvider, mapProvider, prefix, parameters) =>
               dictCommandInitializer.Invoke(command, ormProvider, mapProvider, prefix, dict);
        }
        else
        {
            var whereObjType = whereObj.GetType();
            var cacheKey = HashCode.Combine(connection, ormProvider, mapProvider, entityType, whereObjType);
            if (!queryExistsCommandInitializerCache.TryGetValue(cacheKey, out var objCommandInitializer))
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var memberInfos = whereObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var prefixExpr = Expression.Parameter(typeof(object), "prefix");
                var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

                var typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedWhereObjExpr);
                blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));

                var builderExpr = Expression.Variable(typeof(StringBuilder), "builder");
                blockParameters.Add(builderExpr);
                var constructor = typeof(StringBuilder).GetConstructor(new Type[] { typeof(string) });
                var wherePrefixExpr = Expression.Constant("SELECT COUNT(1) WHERE ");
                blockBodies.Add(Expression.Assign(builderExpr, Expression.New(constructor, wherePrefixExpr)));

                var index = 0;
                var builder = new StringBuilder($"SELECT COUNT(1) FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string), typeof(string) });
                var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    var parameterNameExpr = Expression.Call(concatMethodInfo, Expression.Constant(ormProvider.ParameterPrefix),
                        prefixExpr, Expression.Constant(propMapper.MemberName));
                    if (index > 0)
                        blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(" AND ")));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant($"{ormProvider.GetFieldName(propMapper.FieldName)}=")));
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
                    index++;
                }
                if (index == 0)
                    throw new Exception($"{whereObjType.FullName}类中没有任何可以查询的字段");

                var returnExpr = Expression.Constant(builder.ToString());
                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                objCommandInitializer = Expression.Lambda<Func<IDbCommand, IOrmProvider, string, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, prefixExpr, whereObjExpr).Compile();
                queryExistsCommandInitializerCache.TryAdd(cacheKey, objCommandInitializer);
            }
            commandInitializer = (command, ormProvider, mapProvider, prefix, parameters) =>
                objCommandInitializer.Invoke(command, ormProvider, prefix, parameters);
        }
        return commandInitializer;
    }

    public static Action<ISqlVisitor, List<IDbDataParameter>, object, StringBuilder, StringBuilder> BuildCreateWithBiesCommandInitializer(ISqlVisitor sqlVisitor, Type entityType, object parameters)
    {
        Action<ISqlVisitor, List<IDbDataParameter>, object, StringBuilder, StringBuilder> commandInitializer = null;
        if (parameters is IDictionary<string, object> dict)
        {
            Action<ISqlVisitor, List<IDbDataParameter>, IDictionary<string, object>, StringBuilder, StringBuilder> dictCommandInitializer = null;
            dictCommandInitializer = (visitor, dbParameters, dict, insertBuilder, valuesBuilder) =>
            {
                int index = 0;
                var entityMapper = visitor.MapProvider.GetEntityMap(entityType);
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
                    var parameterName = visitor.OrmProvider.ParameterPrefix + propMapper.MemberName;
                    insertBuilder.Append(visitor.OrmProvider.GetFieldName(propMapper.FieldName));
                    valuesBuilder.Append(parameterName);
                    dbParameters.Add(visitor.OrmProvider.CreateParameter(propMapper, parameterName, item.Value));
                    index++;
                }
            };
            commandInitializer = (visitor, dbParameters, parameters, insertBuilder, valuesBuilder)
                => dictCommandInitializer.Invoke(visitor, dbParameters, dict, insertBuilder, valuesBuilder);
        }
        else
        {
            var parameterType = parameters.GetType();
            if (!parameterType.IsEntityType(out _))
                throw new NotSupportedException("只支持类对象，不支持基础类型");

            var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
            if (!createWithByCommandInitializerCache.TryGetValue(cacheKey, out commandInitializer))
            {
                int columnIndex = 0;
                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var visitorExpr = Expression.Parameter(typeof(ISqlVisitor), "visitor");
                var dbParametersExpr = Expression.Parameter(typeof(List<IDbDataParameter>), "dbParameters");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");
                var fieldsBuilderExpr = Expression.Parameter(typeof(StringBuilder), "fieldsBuilder");
                var valueBuilderExpr = Expression.Parameter(typeof(StringBuilder), "valueBuilder");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
                var ormProviderExpr = Expression.Property(visitorExpr, nameof(ISqlVisitor.OrmProvider));

                var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
                var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                var methodInfo3 = typeof(List<IDbDataParameter>).GetMethod(nameof(List<IDbDataParameter>.Add));
                var methodInfo4 = typeof(Extensions).GetMethod(nameof(Extensions.CreateParameter));
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    if (columnIndex > 0)
                    {
                        blockBodies.Add(Expression.Call(fieldsBuilderExpr, methodInfo1, Expression.Constant(',')));
                        blockBodies.Add(Expression.Call(valueBuilderExpr, methodInfo1, Expression.Constant(',')));
                    }

                    var fieldName = sqlVisitor.OrmProvider.GetFieldName(propMapper.FieldName);
                    blockBodies.Add(Expression.Call(fieldsBuilderExpr, methodInfo2, Expression.Constant(fieldName)));

                    var parameterNameExpr = Expression.Constant(sqlVisitor.OrmProvider.ParameterPrefix + propMapper.MemberName);
                    Expression fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, propMapper.MemberName);
                    if (fieldValueExpr.Type != typeof(object))
                        fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));

                    var memberMapperExpr = Expression.Constant(propMapper);
                    blockBodies.Add(Expression.Call(methodInfo4, memberMapperExpr, parameterNameExpr, fieldValueExpr));
                    blockBodies.Add(Expression.Call(valueBuilderExpr, methodInfo2, parameterNameExpr));
                    columnIndex++;
                }
                commandInitializer = Expression.Lambda<Action<ISqlVisitor, List<IDbDataParameter>, object, StringBuilder, StringBuilder>>(
                    Expression.Block(blockParameters, blockBodies), visitorExpr, dbParametersExpr, parameterExpr, fieldsBuilderExpr, valueBuilderExpr).Compile();
                createWithByCommandInitializerCache.TryAdd(cacheKey, commandInitializer);
            }
        }
        return commandInitializer;
    }
    public static Action<IDbCommand, ISqlVisitor, StringBuilder, int, object> BuildCreateWithBulkCommandInitializer(ISqlVisitor sqlVisitor, Type entityType, object parameters, out string headSql)
    {
        Action<IDbCommand, ISqlVisitor, StringBuilder, int, object> commandInitializer = null;
        var entities = parameters as IEnumerable;
        object parameter = null;
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

            Action<IDbCommand, ISqlVisitor, StringBuilder, int, IDictionary<string, object>> dictCommandInitializer = null;
            dictCommandInitializer = (command, visitor, sqlBuilder, index, dict) =>
            {
                int columnIndex = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    if (columnIndex > 0) sqlBuilder.Append(',');
                    var parameterName = visitor.OrmProvider.ParameterPrefix + item.Key + index.ToString();
                    sqlBuilder.Append(parameterName);
                    command.Parameters.Add(visitor.OrmProvider.CreateParameter(propMapper, parameterName, item.Value));
                    columnIndex++;
                }
            };
            commandInitializer = (command, visitor, builder, index, insertObj)
                => dictCommandInitializer.Invoke(command, visitor, builder, index, insertObj as IDictionary<string, object>);
        }
        else
        {
            var parameterType = parameter.GetType();
            if (!parameterType.IsEntityType(out _))
                throw new NotSupportedException("只支持类对象，不支持基础类型");
            var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
            var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                  .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

            int index = 0;
            var builder = new StringBuilder();
            foreach (var memberInfo in memberInfos)
            {
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var propMapper)
                    || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                    || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(sqlVisitor.OrmProvider.GetFieldName(propMapper.FieldName));
                index++;
            }
            headSql = builder.ToString();

            var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
            if (!createBatchCommandInitializerCache.TryGetValue(cacheKey, out commandInitializer))
            {
                int columnIndex = 0;
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var visitorExpr = Expression.Parameter(typeof(ISqlVisitor), "visitor");
                var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
                var indexExpr = Expression.Parameter(typeof(int), "index");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
                var ormProviderExpr = Expression.Property(visitorExpr, nameof(ISqlVisitor.OrmProvider));

                var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
                var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
                var toStringMethodInfo = typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes);

                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    if (columnIndex > 0) blockBodies.Add(Expression.Call(builderExpr, methodInfo1, Expression.Constant(',')));
                    builder.Append(sqlVisitor.OrmProvider.GetFieldName(propMapper.FieldName));
                    var parameterName = sqlVisitor.OrmProvider.ParameterPrefix + propMapper.MemberName;
                    var toStringExpr = Expression.Call(indexExpr, toStringMethodInfo);
                    var parameterNameExpr = Expression.Call(concatMethodInfo, Expression.Constant(parameterName), toStringExpr);

                    blockBodies.Add(Expression.Call(builderExpr, methodInfo2, parameterNameExpr));
                    AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, propMapper, blockBodies);
                    columnIndex++;
                }

                var returnExpr = Expression.Constant(builder.ToString());
                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                commandInitializer = Expression.Lambda<Action<IDbCommand, ISqlVisitor, StringBuilder, int, object>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, visitorExpr, builderExpr, indexExpr, parameterExpr).Compile();
                createBatchCommandInitializerCache.TryAdd(cacheKey, commandInitializer);
            }
        }
        return commandInitializer;
    }
    //public static Action<ISqlVisitor, List<InsertField>, List<IDbDataParameter>, object> BuildCreateWhereWithCommandInitializer(ISqlVisitor sqlVisitor, Type entityType, object whereObj, bool isOnlyKeys)
    //{
    //    Action<ISqlVisitor, List<InsertField>, List<IDbDataParameter>, object> whereInitializer = null;
    //    if (whereObj is IDictionary<string, object> dict)
    //    {
    //        Action<ISqlVisitor, List<InsertField>, List<IDbDataParameter>, IDictionary<string, object>> dictWhereInitializer = null;
    //        var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
    //        if (isOnlyKeys)
    //        {
    //            dictWhereInitializer = (visitor, whereFields, dbParameters, dict) =>
    //            {
    //                foreach (var keyMapper in entityMapper.KeyMembers)
    //                {
    //                    if (!dict.TryGetValue(keyMapper.MemberName, out var fieldValue))
    //                        throw new ArgumentNullException("whereObj", $"字典参数中缺少主键字段{keyMapper.MemberName}");

    //                    var parameterName = $"{visitor.OrmProvider.ParameterPrefix}k{keyMapper.MemberName}";
    //                    var dbParameter = visitor.OrmProvider.CreateParameter(keyMapper, parameterName, fieldValue);
    //                    whereFields.Add(new InsertField { Fields = visitor.OrmProvider.GetFieldName(keyMapper.FieldName), Values = parameterName });
    //                    dbParameters.Add(dbParameter);
    //                }
    //            };
    //        }
    //        else
    //        {
    //            dictWhereInitializer = (visitor, whereFields, dbParameters, dict) =>
    //            {
    //                foreach (var item in dict)
    //                {
    //                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
    //                        || memberMapper.IsIgnore || memberMapper.IsNavigation
    //                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
    //                        continue;

    //                    var parameterName = $"{visitor.OrmProvider.ParameterPrefix}k{memberMapper.MemberName}";
    //                    var dbParameter = visitor.OrmProvider.CreateParameter(memberMapper, parameterName, item.Value);
    //                    whereFields.Add(new InsertField { Fields = visitor.OrmProvider.GetFieldName(memberMapper.FieldName), Values = parameterName });
    //                    dbParameters.Add(dbParameter);
    //                }
    //            };
    //        }
    //        whereInitializer = (visitor, whereFields, dbParameters, parameters)
    //            => dictWhereInitializer.Invoke(visitor, whereFields, dbParameters, dict);
    //    }
    //    else
    //    {
    //        var parameterType = whereObj.GetType();
    //        var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
    //        if (!updateWhereWithCache.TryGetValue(cacheKey, out whereInitializer))
    //        {
    //            var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
    //            var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    //                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
    //            var visitorExpr = Expression.Parameter(typeof(ISqlVisitor), "visitor");
    //            var whereFieldsExpr = Expression.Parameter(typeof(List<InsertField>), "whereFields");
    //            var dbParametersExpr = Expression.Parameter(typeof(List<IDbDataParameter>), "dbParameters");
    //            var parameterExpr = Expression.Parameter(typeof(object), "parameter");

    //            var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
    //            var blockParameters = new List<ParameterExpression>();
    //            var blockBodies = new List<Expression>();

    //            blockParameters.Add(typedParameterExpr);
    //            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
    //            var ormProviderExpr = Expression.Property(visitorExpr, nameof(ISqlVisitor.OrmProvider));

    //            var whereMemberMappers = isOnlyKeys ? entityMapper.KeyMembers : entityMapper.MemberMaps;
    //            foreach (var memberMapper in whereMemberMappers)
    //            {
    //                if (!memberInfos.Exists(f => f.Name == memberMapper.MemberName))
    //                {
    //                    if (isOnlyKeys) throw new ArgumentNullException("whereObj", $"当前参数whereObj中缺少主键字段{memberMapper.MemberName}");
    //                    else continue;
    //                }
    //                if (memberMapper.IsIgnore || memberMapper.IsNavigation
    //                    || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
    //                    continue;

    //                var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}k{memberMapper.MemberName}";
    //                Expression fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
    //                if (fieldValueExpr.Type != typeof(object))
    //                    fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
    //                var memberMapperExpr = Expression.Constant(memberMapper);
    //                var parameterNameExpr = Expression.Constant(parameterName);
    //                var methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.CreateParameter));

    //                var dbParameterExpr = Expression.Call(methodInfo, ormProviderExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr);
    //                methodInfo = typeof(List<IDbDataParameter>).GetMethod(nameof(List<IDbDataParameter>.Add));
    //                blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr));

    //                methodInfo = typeof(List<UpdateField>).GetMethod(nameof(List<UpdateField>.Add));
    //                var whereField = new UpdateField { Type = UpdateFieldType.Where, MemberMapper = memberMapper, Value = parameterName };
    //                blockBodies.Add(Expression.Call(whereFieldsExpr, methodInfo, Expression.Constant(whereField)));
    //            }
    //            whereInitializer = Expression.Lambda<Action<ISqlVisitor, List<UpdateField>, List<IDbDataParameter>, object>>(Expression.Block(blockParameters, blockBodies), visitorExpr, whereFieldsExpr, dbParametersExpr, parameterExpr).Compile();
    //            updateWhereWithCache.TryAdd(cacheKey, whereInitializer);
    //        }
    //    }
    //    return whereInitializer;
    //}

    public static Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string> BuildUpdateEntitySqlParameters(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object updateObj)
    {
        Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string> commandInitializer = null;
        if (updateObj is IDictionary<string, object> dict)
        {
            Func<IDbCommand, IOrmProvider, IEntityMapProvider, IDictionary<string, object>, string> dictCommandInitializer = null;
            dictCommandInitializer = (command, ormProvider, mapProvider, dict) =>
            {
                int index = 0;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var builder = new StringBuilder($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;

                    if (memberMapper.IsKey) continue;
                    var parameterName = $"{ormProvider.ParameterPrefix}{item.Key}";
                    if (index > 0) builder.Append(',');
                    builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                    var dbParameter = ormProvider.CreateParameter(memberMapper, parameterName, dict[item.Key]);
                    command.Parameters.Add(dbParameter);
                    index++;
                }
                index = 0;
                builder.Append(" WHERE ");
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    if (!dict.ContainsKey(keyMapper.MemberName))
                        throw new Exception($"当前字典中不包含表{entityMapper.TableName}的主键字段{keyMapper.MemberName}无法完成更新操作");
                    var parameterName = $"{ormProvider.ParameterPrefix}k{keyMapper.MemberName}";
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");
                    var dbParameter = ormProvider.CreateParameter(keyMapper, parameterName, dict[keyMapper.MemberName]);
                    command.Parameters.Add(dbParameter);
                    index++;
                }
                return builder.ToString();
            };
            commandInitializer = (command, ormProvider, mapProvider, parameters)
                => dictCommandInitializer.Invoke(command, ormProvider, mapProvider, dict);
        }
        else
        {
            var parameterType = updateObj.GetType();
            var cacheKey = HashCode.Combine(connection, ormProvider, mapProvider, entityType, parameterType);
            if (!updateEntityCommandInitializerCache.TryGetValue(cacheKey, out var objCommandInitializer))
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "command");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                var builder = new StringBuilder($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");

                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                var dbParametersExpr = Expression.PropertyOrField(commandExpr, nameof(IDbCommand.Parameters));
                var methodInfo1 = typeof(Extensions).GetMethod(nameof(Extensions.CreateParameter));
                var methodInfo2 = typeof(IList).GetMethod(nameof(IList.Add));

                int index = 0;
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;
                    if (memberMapper.IsKey) continue;

                    if (index > 0) builder.Append(',');
                    var parameterName = ormProvider.ParameterPrefix + memberInfo.Name;
                    builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                    var memberMapperExpr = Expression.Constant(memberMapper);
                    var parameterNameExpr = Expression.Constant(parameterName);
                    Expression fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberInfo.Name);
                    if (fieldValueExpr.Type != typeof(object))
                        fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));

                    var createParameterExpr = Expression.Call(methodInfo1, ormProviderExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr);
                    blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo2, createParameterExpr));
                    index++;
                }
                index = 0;
                builder.Append(" WHERE ");
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    if (index > 0) builder.Append(" AND ");
                    var parameterName = $"{ormProvider.ParameterPrefix}k{keyMapper.MemberName}";
                    builder.Append($"{ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");
                    var memberMapperExpr = Expression.Constant(keyMapper);
                    var parameterNameExpr = Expression.Constant(parameterName);
                    Expression fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, keyMapper.MemberName);
                    if (fieldValueExpr.Type != typeof(object))
                        fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));

                    var createParameterExpr = Expression.Call(methodInfo1, ormProviderExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr);
                    blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo2, createParameterExpr));
                    index++;
                }

                var returnExpr = Expression.Constant(builder.ToString());
                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                objCommandInitializer = Expression.Lambda<Func<IDbCommand, IOrmProvider, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
                updateEntityCommandInitializerCache.TryAdd(cacheKey, objCommandInitializer);
            }
            commandInitializer = (command, ormProvider, mapProvider, parameters)
                => objCommandInitializer.Invoke(command, ormProvider, parameters);
        }
        return commandInitializer;
    }
    public static Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, int, object> BuildUpdateBatchCommandInitializer(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object parameters)
    {
        Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, int, object> commandInitializer = null;
        var entities = parameters as IEnumerable;
        object parameter = null;
        foreach (var entity in entities)
        {
            parameter = entity;
            break;
        }
        if (parameter is IDictionary<string, object> dict)
        {
            Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, int, IDictionary<string, object>> dictCommandInitializer = null;
            dictCommandInitializer = (command, ormProvider, mapProvider, builder, index, dict) =>
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
            commandInitializer = (command, ormProvider, mapProvider, builder, index, parameters)
                => dictCommandInitializer.Invoke(command, ormProvider, mapProvider, builder, index, parameters as IDictionary<string, object>);
        }
        else
        {
            var updateObjType = parameter.GetType();
            var cacheKey = HashCode.Combine(connection, ormProvider, mapProvider, entityType, updateObjType);
            if (!updateEntityBatchCommandInitializerCache.TryGetValue(cacheKey, out var objCommandInitializer))
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var memberInfos = updateObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "command");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
                var indexExpr = Expression.Parameter(typeof(int), "index");
                var updateObjExpr = Expression.Parameter(typeof(object), "updateObj");

                var typedUpdateObjExpr = Expression.Variable(updateObjType, "typedUpdateObj");
                var strIndexExpr = Expression.Variable(typeof(string), "strIndex");
                var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.AddRange(new[] { typedUpdateObjExpr, strIndexExpr, parameterNameExpr });

                var methodInfo1 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
                var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                var methodInfo3 = typeof(Extensions).GetMethod(nameof(Extensions.CreateParameter));
                var methodInfo4 = typeof(IList).GetMethod(nameof(IList.Add));

                var methodInfo = typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes);
                var updateHeadExpr = Expression.Constant($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
                blockBodies.Add(Expression.Assign(typedUpdateObjExpr, Expression.Convert(updateObjExpr, updateObjType)));
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, updateHeadExpr));
                blockBodies.Add(Expression.Assign(strIndexExpr, Expression.Call(indexExpr, methodInfo)));
                var dbParametersExpr = Expression.Property(commandExpr, nameof(IDbCommand.Parameters));

                int index = 0;
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                       || memberMapper.IsIgnore || memberMapper.IsNavigation
                       || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;
                    if (memberMapper.IsKey) continue;

                    methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
                    if (index > 0) blockBodies.Add(Expression.Call(builderExpr, methodInfo, Expression.Constant(',')));

                    var parameterName = $"{ormProvider.ParameterPrefix}{memberInfo.Name}";
                    var concatExpr = Expression.Call(methodInfo1, Expression.Constant(parameterName), strIndexExpr);
                    blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));
                    var setExpr = Expression.Constant($"{ormProvider.GetFieldName(memberMapper.FieldName)}=");
                    blockBodies.Add(Expression.Call(builderExpr, methodInfo2, setExpr));
                    blockBodies.Add(Expression.Call(builderExpr, methodInfo2, parameterNameExpr));

                    var memberMapperExpr = Expression.Constant(memberMapper);
                    Expression fieldValueExpr = Expression.PropertyOrField(typedUpdateObjExpr, memberInfo.Name);
                    if (fieldValueExpr.Type != typeof(object))
                        fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                    var createParameterExpr = Expression.Call(methodInfo3, ormProviderExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr);
                    var toObjectExpr = Expression.Convert(createParameterExpr, typeof(object));
                    blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo4, toObjectExpr));
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

                    if (index > 0) blockBodies.Add(Expression.Call(whereBuilderExpr, methodInfo2, Expression.Constant(" AND ")));
                    var parameterName = $"{ormProvider.ParameterPrefix}k{keyMapper.MemberName}";
                    var callExpr = Expression.Call(methodInfo1, Expression.Constant(parameterName), strIndexExpr);
                    blockBodies.Add(Expression.Assign(parameterNameExpr, callExpr));
                    var setExpr = Expression.Constant($"{ormProvider.GetFieldName(keyMapper.FieldName)}=");
                    blockBodies.Add(Expression.Call(whereBuilderExpr, methodInfo2, setExpr));
                    blockBodies.Add(Expression.Call(whereBuilderExpr, methodInfo2, parameterNameExpr));

                    var memberMapperExpr = Expression.Constant(keyMapper);
                    Expression fieldValueExpr = Expression.PropertyOrField(typedUpdateObjExpr, keyMapper.MemberName);
                    if (fieldValueExpr.Type != typeof(object))
                        fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                    var createParameterExpr = Expression.Call(methodInfo3, ormProviderExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr);
                    var toObjectExpr = Expression.Convert(createParameterExpr, typeof(object));
                    blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo4, toObjectExpr));
                    index++;
                }
                methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(StringBuilder) });
                blockBodies.Add(Expression.Call(builderExpr, methodInfo, whereBuilderExpr));

                objCommandInitializer = Expression.Lambda<Action<IDbCommand, IOrmProvider, StringBuilder, int, object>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, builderExpr, indexExpr, updateObjExpr).Compile();
                updateEntityBatchCommandInitializerCache.TryAdd(cacheKey, objCommandInitializer);
            }
            commandInitializer = (command, ormProvider, mapProvider, builder, index, parameters)
               => objCommandInitializer.Invoke(command, ormProvider, builder, index, parameters);
        }
        return commandInitializer;
    }

    public static Action<ISqlVisitor, List<UpdateField>, List<IDbDataParameter>, object> BuildUpdateSetWithParameters(ISqlVisitor sqlVisitor, Type entityType, object updateObj, bool isExceptKey)
    {
        Action<ISqlVisitor, List<UpdateField>, List<IDbDataParameter>, object> setFieldsInitializer = null;
        if (updateObj is IDictionary<string, object> dict)
        {
            Action<ISqlVisitor, List<UpdateField>, List<IDbDataParameter>, IDictionary<string, object>> dictSetFieldsInitializer = null;
            if (isExceptKey)
            {
                dictSetFieldsInitializer = (visitor, setFields, dbParameters, dict) =>
                {
                    var entityMapper = visitor.MapProvider.GetEntityMap(entityType);
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;
                        if (memberMapper.IsKey) continue;

                        var parameterName = $"{visitor.OrmProvider.ParameterPrefix}{item.Key}";
                        setFields.Add(new UpdateField { MemberMapper = memberMapper, Value = parameterName });
                        dbParameters.Add(visitor.OrmProvider.CreateParameter(memberMapper, parameterName, dict[item.Key]));
                    }
                };
            }
            else
            {
                dictSetFieldsInitializer = (visitor, setFields, dbParameters, dict) =>
                {
                    var entityMapper = visitor.MapProvider.GetEntityMap(entityType);
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        var parameterName = $"{visitor.OrmProvider.ParameterPrefix}{item.Key}";
                        setFields.Add(new UpdateField { MemberMapper = memberMapper, Value = parameterName });
                        dbParameters.Add(visitor.OrmProvider.CreateParameter(memberMapper, parameterName, dict[item.Key]));
                    }
                };
            }
            setFieldsInitializer = (visitor, memberMappers, dbParameters, parameters)
                => dictSetFieldsInitializer.Invoke(visitor, memberMappers, dbParameters, dict);
        }
        else
        {
            var parameterType = updateObj.GetType();
            var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType, isExceptKey);
            if (!updateSetFieldsCache.TryGetValue(cacheKey, out setFieldsInitializer))
            {
                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var visitorExpr = Expression.Parameter(typeof(ISqlVisitor), "visitor");
                var setFieldsExpr = Expression.Parameter(typeof(List<UpdateField>), "setFields");
                var dbParametersExpr = Expression.Parameter(typeof(List<IDbDataParameter>), "dbParameters");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();

                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
                var ormProviderExpr = Expression.Property(visitorExpr, nameof(ISqlVisitor.OrmProvider));

                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;
                    if (isExceptKey && memberMapper.IsKey) continue;

                    var parameterName = sqlVisitor.OrmProvider.ParameterPrefix + memberInfo.Name;
                    Expression fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
                    if (fieldValueExpr.Type != typeof(object))
                        fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                    var memberMapperExpr = Expression.Constant(memberMapper);
                    var parameterNameExpr = Expression.Constant(parameterName);
                    var methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.CreateParameter));

                    var dbParameterExpr = Expression.Call(methodInfo, ormProviderExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr);
                    methodInfo = typeof(List<IDbDataParameter>).GetMethod(nameof(List<IDbDataParameter>.Add));
                    blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr));

                    methodInfo = typeof(List<UpdateField>).GetMethod(nameof(List<UpdateField>.Add));
                    var setField = new UpdateField { MemberMapper = memberMapper, Value = parameterName };
                    blockBodies.Add(Expression.Call(setFieldsExpr, methodInfo, Expression.Constant(setField)));
                }
                setFieldsInitializer = Expression.Lambda<Action<ISqlVisitor, List<UpdateField>, List<IDbDataParameter>, object>>(Expression.Block(blockParameters, blockBodies), visitorExpr, setFieldsExpr, dbParametersExpr, parameterExpr).Compile();
                updateSetFieldsCache.TryAdd(cacheKey, setFieldsInitializer);
            }
        }
        return setFieldsInitializer;
    }
    public static Action<ISqlVisitor, List<UpdateField>, List<IDbDataParameter>, object> BuildUpdateWhereWithParameters(ISqlVisitor sqlVisitor, Type entityType, object whereObj, bool isOnlyKeys)
    {
        Action<ISqlVisitor, List<UpdateField>, List<IDbDataParameter>, object> whereInitializer = null;
        if (whereObj is IDictionary<string, object> dict)
        {
            Action<ISqlVisitor, List<UpdateField>, List<IDbDataParameter>, IDictionary<string, object>> dictWhereInitializer = null;
            var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
            if (isOnlyKeys)
            {
                dictWhereInitializer = (visitor, whereFields, dbParameters, dict) =>
                {
                    foreach (var keyMapper in entityMapper.KeyMembers)
                    {
                        if (!dict.TryGetValue(keyMapper.MemberName, out var fieldValue))
                            throw new ArgumentNullException("whereObj", $"字典参数中缺少主键字段{keyMapper.MemberName}");

                        var parameterName = $"{visitor.OrmProvider.ParameterPrefix}k{keyMapper.MemberName}";
                        whereFields.Add(new UpdateField { MemberMapper = keyMapper, Value = parameterName });
                        dbParameters.Add(visitor.OrmProvider.CreateParameter(keyMapper, parameterName, fieldValue));
                    }
                };
            }
            else
            {
                dictWhereInitializer = (visitor, whereFields, dbParameters, dict) =>
                {
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                            || memberMapper.IsIgnore || memberMapper.IsNavigation
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        var parameterName = $"{visitor.OrmProvider.ParameterPrefix}k{memberMapper.MemberName}";
                        whereFields.Add(new UpdateField { MemberMapper = memberMapper, Value = parameterName });
                        dbParameters.Add(visitor.OrmProvider.CreateParameter(memberMapper, parameterName, dict[item.Key]));
                    }
                };
            }
            whereInitializer = (visitor, whereFields, dbParameters, parameters)
                => dictWhereInitializer.Invoke(visitor, whereFields, dbParameters, dict);
        }
        else
        {
            var parameterType = whereObj.GetType();
            var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
            if (!updateWhereWithCache.TryGetValue(cacheKey, out whereInitializer))
            {
                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var visitorExpr = Expression.Parameter(typeof(ISqlVisitor), "visitor");
                var whereFieldsExpr = Expression.Parameter(typeof(List<UpdateField>), "whereFields");
                var dbParametersExpr = Expression.Parameter(typeof(List<IDbDataParameter>), "dbParameters");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();

                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
                var ormProviderExpr = Expression.Property(visitorExpr, nameof(ISqlVisitor.OrmProvider));

                var whereMemberMappers = isOnlyKeys ? entityMapper.KeyMembers : entityMapper.MemberMaps;
                foreach (var memberMapper in whereMemberMappers)
                {
                    if (!memberInfos.Exists(f => f.Name == memberMapper.MemberName))
                    {
                        if (isOnlyKeys) throw new ArgumentNullException("whereObj", $"当前参数whereObj中缺少主键字段{memberMapper.MemberName}");
                        else continue;
                    }
                    if (memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;

                    var parameterName = $"{sqlVisitor.OrmProvider.ParameterPrefix}k{memberMapper.MemberName}";
                    Expression fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
                    if (fieldValueExpr.Type != typeof(object))
                        fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                    var memberMapperExpr = Expression.Constant(memberMapper);
                    var parameterNameExpr = Expression.Constant(parameterName);
                    var methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.CreateParameter));

                    var dbParameterExpr = Expression.Call(methodInfo, ormProviderExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr);
                    methodInfo = typeof(List<IDbDataParameter>).GetMethod(nameof(List<IDbDataParameter>.Add));
                    blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr));

                    methodInfo = typeof(List<UpdateField>).GetMethod(nameof(List<UpdateField>.Add));
                    var whereField = new UpdateField { Type = UpdateFieldType.Where, MemberMapper = memberMapper, Value = parameterName };
                    blockBodies.Add(Expression.Call(whereFieldsExpr, methodInfo, Expression.Constant(whereField)));
                }
                whereInitializer = Expression.Lambda<Action<ISqlVisitor, List<UpdateField>, List<IDbDataParameter>, object>>(Expression.Block(blockParameters, blockBodies), visitorExpr, whereFieldsExpr, dbParametersExpr, parameterExpr).Compile();
                updateWhereWithCache.TryAdd(cacheKey, whereInitializer);
            }
        }
        return whereInitializer;
    }

    public static Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, int, object> BuildDeleteBatchCommandInitializer(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object deleteObjs, out bool isNeedEndParenthesis)
    {
        Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, int, object> commandInitializer = null;
        var entityMapper = mapProvider.GetEntityMap(entityType);
        if (entityMapper.KeyMembers.Count <= 0)
            throw new Exception($"表{entityMapper.TableName}在实体映射中没有配置任何主键，无法完成删除操作");
        var entities = deleteObjs as IEnumerable;
        object deleteObj = null;
        foreach (var entity in entities)
        {
            deleteObj = entity;
            break;
        }

        isNeedEndParenthesis = entityMapper.KeyMembers.Count == 1;
        if (deleteObj is IDictionary<string, object> dict)
        {
            Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, int, IDictionary<string, object>> dictCommandInitializer = null;
            if (entityMapper.KeyMembers.Count > 1)
            {
                dictCommandInitializer = (command, ormProvider, mapProvider, builder, index, dict) =>
                {
                    if (index > 0) builder.Append(';');
                    var entityMapper = mapProvider.GetEntityMap(entityType);
                    builder.Append($"DELETE FROM {ormProvider.GetFieldName(entityMapper.TableName)} WHERE ");
                    int keyIndex = 0;
                    foreach (var keyMapper in entityMapper.KeyMembers)
                    {
                        if (keyIndex > 0) builder.Append(" AND ");
                        var fieldName = ormProvider.GetFieldName(keyMapper.FieldName);
                        string parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName + index.ToString();
                        builder.Append($"{fieldName}={parameterName}");
                        command.Parameters.Add(ormProvider.CreateParameter(keyMapper, parameterName, dict[keyMapper.MemberName]));
                        keyIndex++;
                    }
                };
            }
            else
            {
                dictCommandInitializer = (command, ormProvider, mapProvider, builder, index, dict) =>
                {
                    if (index > 0) builder.Append(',');
                    else builder.Append($"DELETE FROM {ormProvider.GetFieldName(entityMapper.TableName)} WHERE {ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)} IN (");
                    var keyMapper = entityMapper.KeyMembers[0];
                    string parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName + index.ToString();
                    builder.Append(parameterName);
                    command.Parameters.Add(ormProvider.CreateParameter(keyMapper, parameterName, dict[keyMapper.MemberName]));
                };
            }
            commandInitializer = (command, ormProvider, mapProvider, builder, index, parameters) =>
               dictCommandInitializer.Invoke(command, ormProvider, mapProvider, builder, index, dict);
        }
        else
        {
            var parameterType = deleteObj.GetType();
            var cacheKey = HashCode.Combine(connection, ormProvider, entityType, parameterType);
            if (!deleteBatchCommandInitializerCache.TryGetValue(cacheKey, out var objCommandInitializer))
            {
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
                var indexExpr = Expression.Parameter(typeof(int), "index");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                ParameterExpression typedParameterExpr = null;
                var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                bool isEntityType = false;

                if (parameterType.IsEntityType(out _))
                {
                    isEntityType = true;
                    typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                    blockParameters.Add(typedParameterExpr);
                    blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
                }
                else
                {
                    if (entityMapper.KeyMembers.Count > 1)
                        throw new NotSupportedException($"模型{entityType.FullName}有多个主键字段，不能使用单个值类型{parameterType.FullName}作为参数");
                }
                blockParameters.Add(parameterNameExpr);

                var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
                var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                var methodInfo3 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });

                if (entityMapper.KeyMembers.Count > 1)
                {
                    var addCommaExpr = Expression.Call(builderExpr, methodInfo1, Expression.Constant(';'));
                    var greatThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0, typeof(int)));
                    blockBodies.Add(Expression.IfThen(greatThenExpr, addCommaExpr));

                    var sql = $"DELETE FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ";
                    blockBodies.Add(Expression.Call(builderExpr, methodInfo2, Expression.Constant(sql)));
                    int index = 0;
                    foreach (var keyMapper in entityMapper.KeyMembers)
                    {
                        if (index > 0)
                            blockBodies.Add(Expression.Call(builderExpr, methodInfo2, Expression.Constant(" AND ")));

                        var parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName;
                        var suffixExpr = Expression.Call(indexExpr, typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes));
                        var concatExpr = Expression.Call(methodInfo3, Expression.Constant(parameterName), suffixExpr);
                        blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));

                        var constantExpr = Expression.Constant(ormProvider.GetFieldName(keyMapper.FieldName) + "=");
                        blockBodies.Add(Expression.Call(builderExpr, methodInfo2, constantExpr));
                        blockBodies.Add(Expression.Call(builderExpr, methodInfo2, parameterNameExpr));

                        if (isEntityType)
                            AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, keyMapper, blockBodies);
                        else AddValueParameter(commandExpr, ormProviderExpr, parameterNameExpr, parameterExpr, keyMapper, blockBodies);
                        index++;
                    }
                }
                else
                {
                    var keyMapper = entityMapper.KeyMembers[0];
                    var addCommaExpr = Expression.Call(builderExpr, methodInfo1, Expression.Constant(','));
                    var deldeteHead = $"DELETE FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE {ormProvider.GetFieldName(keyMapper.FieldName)} IN (";
                    var AddHeadExpr = Expression.Call(builderExpr, methodInfo2, Expression.Constant(deldeteHead));
                    var greatThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0, typeof(int)));
                    blockBodies.Add(Expression.IfThenElse(greatThenExpr, addCommaExpr, AddHeadExpr));

                    var suffixExpr = Expression.Call(indexExpr, typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes));
                    var parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName;
                    var concatExpr = Expression.Call(methodInfo3, Expression.Constant(parameterName), suffixExpr);
                    blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));
                    blockBodies.Add(Expression.Call(builderExpr, methodInfo2, parameterNameExpr));
                    if (isEntityType)
                        AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, keyMapper, blockBodies);
                    else AddValueParameter(commandExpr, ormProviderExpr, parameterNameExpr, parameterExpr, keyMapper, blockBodies);
                }
                objCommandInitializer = Expression.Lambda<Action<IDbCommand, IOrmProvider, StringBuilder, int, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, builderExpr, indexExpr, parameterExpr).Compile();
                deleteBatchCommandInitializerCache.TryAdd(cacheKey, objCommandInitializer);
            }
            commandInitializer = (command, ormProvider, mapProvider, builder, index, parameters) =>
               objCommandInitializer.Invoke(command, ormProvider, builder, index, parameters);
        }
        return commandInitializer;
    }
    public static Func<IDbCommand, IOrmProvider, object, string> BuildDeleteCommandInitializer(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object parameters)
    {
        Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
        if (parameters is IDictionary<string, object>)
        {
            commandInitializer = (command, ormProvider, parameter) =>
            {
                int index = 0;
                var dict = parameter as IDictionary<string, object>;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var builder = new StringBuilder($"DELETE FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");

                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    if (!dict.TryGetValue(keyMapper.MemberName, out var fieldValue))
                        throw new ArgumentNullException($"字典参数中缺少主键字段{keyMapper.MemberName}", "keys");

                    if (index > 0)
                        builder.Append(',');
                    var parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName;
                    builder.Append($"{ormProvider.GetFieldName(keyMapper.MemberName)}={parameterName}");
                    command.Parameters.Add(ormProvider.CreateParameter(keyMapper, parameterName, fieldValue));
                    index++;
                }
                return builder.ToString();
            };
        }
        else
        {
            var parameterType = parameters.GetType();
            var cacheKey = HashCode.Combine(connection, ormProvider, entityType, parameterType);
            if (!deleteCommandInitializerCache.TryGetValue(cacheKey, out commandInitializer))
            {
                int index = 0;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();


                EntityMap parameterMapper = null;
                ParameterExpression typedParameterExpr = null;
                bool isEntityType = false;
                if (parameterType.IsEntityType(out _))
                {
                    isEntityType = true;
                    parameterMapper = mapProvider.GetEntityMap(parameterType);
                    typedParameterExpr = Expression.Parameter(parameterType, "typedParameter");
                    blockParameters.Add(typedParameterExpr);
                    blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
                }
                else
                {
                    if (entityMapper.KeyMembers.Count > 1)
                        throw new NotSupportedException($"模型{entityType.FullName}有多个主键字段，不能使用单个值类型{parameterType.FullName}作为参数");
                }

                var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
                var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                var methodInfo3 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });

                var builder = new StringBuilder($"DELETE FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    if (isEntityType && !parameterMapper.TryGetMemberMap(keyMapper.MemberName, out var propMapper))
                        throw new ArgumentNullException("keys", $"参数类型{parameterType.FullName}缺少主键字段{keyMapper.MemberName}");

                    var parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName;
                    if (index > 0)
                        builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");
                    var parameterNameExpr = Expression.Constant(parameterName);

                    if (isEntityType)
                        AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, keyMapper, blockBodies);
                    else AddValueParameter(commandExpr, ormProviderExpr, parameterNameExpr, parameterExpr, keyMapper, blockBodies);
                    index++;
                }
                var resultLabelExpr = Expression.Label(typeof(string));
                var returnExpr = Expression.Constant(builder.ToString());
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                commandInitializer = Expression.Lambda<Func<IDbCommand, IOrmProvider, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
                deleteCommandInitializerCache.TryAdd(cacheKey, commandInitializer);
            }
        }
        return commandInitializer;
    }

    public static Func<ISqlVisitor, List<IDbDataParameter>, object, string> BuildWhereWithKeysCommandInitializer(ISqlVisitor sqlVisitor, Type entityType, object parameters)
    {
        Func<ISqlVisitor, List<IDbDataParameter>, object, string> commandInitializer = null;
        if (parameters is IDictionary<string, object> dict)
        {
            Func<ISqlVisitor, List<IDbDataParameter>, IDictionary<string, object>, string> dictCommandInitializer = null;
            dictCommandInitializer = (visitor, dbParameters, dict) =>
            {
                int index = 0;
                var entityMapper = visitor.MapProvider.GetEntityMap(entityType);
                var builder = new StringBuilder();
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    if (index > 0) builder.Append(" AND ");
                    var parameterName = visitor.OrmProvider.ParameterPrefix + "k" + item.Key;
                    builder.Append(visitor.OrmProvider.GetFieldName(propMapper.FieldName));
                    dbParameters.Add(visitor.OrmProvider.CreateParameter(propMapper, parameterName, item.Value));
                    index++;
                }
                return builder.ToString();
            };
            commandInitializer = (visitor, dbParameters, parameters)
                => dictCommandInitializer.Invoke(visitor, dbParameters, dict);
        }
        else
        {
            var parameterType = parameters.GetType();
            if (!parameterType.IsEntityType(out _))
                throw new NotSupportedException("只支持类对象，不支持基础类型");

            var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
            if (!whereWithKeysCommandInitializerCache.TryGetValue(cacheKey, out commandInitializer))
            {
                int columnIndex = 0;
                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var visitorExpr = Expression.Parameter(typeof(ISqlVisitor), "visitor");
                var dbParametersExpr = Expression.Parameter(typeof(List<IDbDataParameter>), "dbParameters");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
                var ormProviderExpr = Expression.Property(visitorExpr, nameof(ISqlVisitor.OrmProvider));

                var methodInfo1 = typeof(List<IDbDataParameter>).GetMethod(nameof(List<IDbDataParameter>.Add));
                var methodInfo2 = typeof(Extensions).GetMethod(nameof(Extensions.CreateParameter));

                var builder = new StringBuilder();
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    if (columnIndex > 0) builder.Append(" AND ");
                    var fieldName = sqlVisitor.OrmProvider.GetFieldName(propMapper.FieldName);
                    var parameterName = sqlVisitor.OrmProvider.ParameterPrefix + "k" + propMapper.MemberName;
                    builder.Append($"{fieldName}={parameterName}");

                    Expression fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, propMapper.MemberName);
                    if (fieldValueExpr.Type != typeof(object))
                        fieldValueExpr = Expression.Convert(fieldValueExpr, typeof(object));
                    var memberMapperExpr = Expression.Constant(propMapper);
                    var parameterNameExpr = Expression.Constant(parameterName);
                    var dbParameterExpr = Expression.Call(methodInfo2, ormProviderExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr);
                    blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo1, dbParameterExpr));
                    columnIndex++;
                }
                var returnExpr = Expression.Constant(builder.ToString());
                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                commandInitializer = Expression.Lambda<Func<ISqlVisitor, List<IDbDataParameter>, object, string>>(
                    Expression.Block(blockParameters, blockBodies), visitorExpr, dbParametersExpr, parameterExpr).Compile();
                whereWithKeysCommandInitializerCache.TryAdd(cacheKey, commandInitializer);
            }
        }
        return commandInitializer;
    }
}