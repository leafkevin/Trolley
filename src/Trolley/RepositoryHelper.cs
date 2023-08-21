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

class RepositoryHelper
{
    private static ConcurrentDictionary<int, string> queryGetSqlCache = new();
    private static ConcurrentDictionary<int, string> querySqlPartCache = new();
    private static ConcurrentDictionary<int, Action<IDbCommand, IOrmProvider, object>> queryGetCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Func<IDbCommand, IOrmProvider, string, object, string>> queryWhereCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Action<IDbCommand, IOrmProvider, object>> queryRawSqlCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Func<IDbCommand, IOrmProvider, object, string>> queryExistsCommandInitializerCache = new();

    private static ConcurrentDictionary<int, Action<IDbCommand, IOrmProvider, object>> createRawSqlCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Action<IDbCommand, IOrmProvider, StringBuilder, int, object>> createBatchCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Action<IDbCommand, IOrmProvider, object, StringBuilder, StringBuilder>> createWithBiesCommandInitializerCache = new();

    private static ConcurrentDictionary<int, Func<IDbCommand, IOrmProvider, object, string>> updateEntityCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Action<List<SetField>, List<IDbDataParameter>, object>> updateSetFieldsCache = new();
    private static ConcurrentDictionary<int, Action<IDbCommand, ISqlVisitor, StringBuilder, object, int>> updateBulkSetFieldsCache = new();

    private static ConcurrentDictionary<int, Action<IDbCommand, IOrmProvider, StringBuilder, int, object>> deleteBatchCommandInitializerCache = new();
    private static ConcurrentDictionary<int, Func<IDbCommand, IOrmProvider, object, string>> deleteCommandInitializerCache = new();

    private static ConcurrentDictionary<int, Action<List<IDbDataParameter>, IOrmProvider, object>> rawSqlParametersCache = new();

    public static void AddValueParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr, Expression parameterNameExpr,
        Expression parameterValueExpr, MemberMap memberMapper, List<Expression> blockBodies)
    {
        var methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.CreateParameter));
        Expression dbParameterExpr = null;
        var fieldValueExpr = parameterValueExpr;
        if (parameterValueExpr.Type != typeof(object))
            fieldValueExpr = Expression.Convert(parameterValueExpr, typeof(object));
        if (memberMapper != null)
        {
            var memberMapperExpr = Expression.Constant(memberMapper);
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
        var methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.CreateParameter));
        Expression dbParameterExpr = null;
        var fieldValueExpr = parameterValueExpr;
        if (parameterValueExpr.Type != typeof(object))
            fieldValueExpr = Expression.Convert(parameterValueExpr, typeof(object));

        //var dbParameter = ormProvider.CreateParameter("@Name", objValue);
        methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
        dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, fieldValueExpr);

        var parametersExpr = Expression.Property(commandExpr, nameof(IDbCommand.Parameters));
        methodInfo = typeof(IList).GetMethod(nameof(IDbCommand.Parameters.Add));
        var addParameterExpr = Expression.Call(parametersExpr, methodInfo, dbParameterExpr);
        blockBodies.Add(addParameterExpr);
    }
    public static void AddMemberParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr, Expression parameterNameExpr,
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

    public static string BuildGetSql(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType)
    {
        var sqlCacheKey = HashCode.Combine(connection, ormProvider, mapProvider, entityType);
        if (!queryGetSqlCache.TryGetValue(sqlCacheKey, out var sql))
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
            builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
            index = 0;
            foreach (var keyMapper in entityMapper.KeyMembers)
            {
                if (index > 0)
                    builder.Append(" AND ");
                var parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName;
                builder.Append($"{ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");
                index++;
            }
            sql = builder.ToString();
            queryGetSqlCache.TryAdd(sqlCacheKey, sql);
        }
        return sql;
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

    public static Action<IDbCommand, IOrmProvider, IEntityMapProvider, object> BuildGetParameters(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj)
    {
        Action<IDbCommand, IOrmProvider, IEntityMapProvider, object> commandInitializer = null;
        if (whereObj is IDictionary<string, object> dict)
        {
            Action<IDbCommand, IOrmProvider, IEntityMapProvider, IDictionary<string, object>> dictCommandInitializer = null;
            dictCommandInitializer = (command, ormProvider, mapProvider, dict) =>
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || !propMapper.IsKey)
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    command.Parameters.Add(ormProvider.CreateParameter(propMapper, parameterName, item.Value));
                }
            };
            commandInitializer = (command, ormProvider, mapProvider, parameters)
                => dictCommandInitializer.Invoke(command, ormProvider, mapProvider, dict);
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
                var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

                ParameterExpression typedWhereObjExpr = null;
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                bool isEntityType = false;
                List<MemberInfo> memberInfos = null;

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
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    if (isEntityType && !memberInfos.Exists(f => f.Name == keyMapper.MemberName))
                        throw new ArgumentNullException($"参数类型{whereObjType.FullName}缺少主键字段{keyMapper.MemberName}", "whereObj");

                    var parameterName = $"{ormProvider.ParameterPrefix}{keyMapper.MemberName}";
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));

                    if (isEntityType)
                        AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, keyMapper, blockBodies);
                    else AddValueParameter(commandExpr, ormProviderExpr, parameterNameExpr, whereObjExpr, keyMapper, blockBodies);
                    index++;
                }
                objCommandInitializer = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, whereObjExpr).Compile();
                queryGetCommandInitializerCache.TryAdd(cacheKey, objCommandInitializer);
            }
            commandInitializer = (command, ormProvider, mapProvider, parameters)
                => objCommandInitializer.Invoke(command, ormProvider, parameters);
        }
        return commandInitializer;
    }
    public static Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string> BuildQueryWhereSqlParameters(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj)
    {
        Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string> commandInitializer = null;
        if (whereObj is IDictionary<string, object> dict)
        {
            Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, IDictionary<string, object>, string> dictCommandInitializer = null;
            dictCommandInitializer = (command, ormProvider, mapProvider, sql, dict) =>
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var builder = new StringBuilder(" WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    command.Parameters.Add(ormProvider.CreateParameter(propMapper, parameterName, item.Value));
                    index++;
                }
                builder.Insert(0, sql);
                return builder.ToString();
            };
            commandInitializer = (command, ormProvider, mapProvider, sql, parameters)
                => dictCommandInitializer.Invoke(command, ormProvider, mapProvider, sql, dict);
        }
        else
        {
            var whereObjType = whereObj.GetType();
            var cacheKey = HashCode.Combine(connection, ormProvider, mapProvider, entityType, whereObjType);
            if (!queryWhereCommandInitializerCache.TryGetValue(cacheKey, out var objCommandInitializer))
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var memberInfos = whereObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var sqlExpr = Expression.Parameter(typeof(string), "sql");
                var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

                var typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedWhereObjExpr);
                blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));

                var index = 0;
                var builder = new StringBuilder(" WHERE ");
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, propMapper, blockBodies);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    index++;
                }
                var methodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
                var returnExpr = Expression.Call(methodInfo, sqlExpr, Expression.Constant(builder.ToString(), typeof(string)));

                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                objCommandInitializer = Expression.Lambda<Func<IDbCommand, IOrmProvider, string, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, sqlExpr, whereObjExpr).Compile();
                queryWhereCommandInitializerCache.TryAdd(cacheKey, objCommandInitializer);
            }
            commandInitializer = (command, ormProvider, mapProvider, sql, parameters)
                => objCommandInitializer.Invoke(command, ormProvider, sql, parameters);
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
    public static Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string> BuildExistsSqlParameters(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj)
    {
        Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string> commandInitializer = null;
        if (whereObj is IDictionary<string, object> dict)
        {
            Func<IDbCommand, IOrmProvider, IEntityMapProvider, IDictionary<string, object>, string> dictCommandInitializer = null;
            dictCommandInitializer = (command, ormProvider, mapProvider, dict) =>
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

                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    command.Parameters.Add(ormProvider.CreateParameter(propMapper, parameterName, item.Value));
                    index++;
                }
                return builder.ToString();
            };
            commandInitializer = (command, ormProvider, mapProvider, parameters) =>
               dictCommandInitializer.Invoke(command, ormProvider, mapProvider, dict);
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
                var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

                var typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedWhereObjExpr);
                blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));

                var index = 0;
                var builder = new StringBuilder($"SELECT COUNT(1) FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, propMapper, blockBodies);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    index++;
                }
                if (index == 0)
                    throw new Exception($"{whereObjType.FullName}类中没有任何可以查询的字段");

                var returnExpr = Expression.Constant(builder.ToString());
                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                objCommandInitializer = Expression.Lambda<Func<IDbCommand, IOrmProvider, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, whereObjExpr).Compile();
                queryExistsCommandInitializerCache.TryAdd(cacheKey, objCommandInitializer);
            }
            commandInitializer = (command, ormProvider, mapProvider, parameters) =>
                objCommandInitializer.Invoke(command, ormProvider, parameters);
        }
        return commandInitializer;
    }

    public static Action<IDbCommand, IOrmProvider, object> BuildCreateRawSqlParameters(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, string rawSql, object parameters)
    {
        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
        var parameterType = parameters.GetType();
        var cacheKey = HashCode.Combine(connection, ormProvider, mapProvider, rawSql, entityType, parameterType);
        if (!createRawSqlCommandInitializerCache.TryGetValue(cacheKey, out commandInitializer))
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
            createRawSqlCommandInitializerCache.TryAdd(cacheKey, commandInitializer);
        }
        return commandInitializer;
    }
    public static Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, int, object> BuildCreateBatchCommandInitializer(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object parameters)
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
                int columnIndex = 0;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                if (index == 0)
                {
                    builder.Append($"INSERT INTO {ormProvider.GetTableName(entityMapper.TableName)} (");
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                            || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                            || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                            continue;
                        if (columnIndex > 0)
                            builder.Append(',');
                        builder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                        columnIndex++;
                    }
                    builder.Append(") VALUES ");
                }
                else builder.Append(',');
                columnIndex = 0;
                builder.Append('(');
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    if (columnIndex > 0)
                        builder.Append(',');
                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName + index.ToString();
                    builder.Append(parameterName);
                    command.Parameters.Add(ormProvider.CreateParameter(propMapper, parameterName, item.Value));
                    columnIndex++;
                }
                builder.Append(')');
            };
            commandInitializer = (command, ormProvider, mapProvider, builder, index, parameters)
                => dictCommandInitializer.Invoke(command, ormProvider, mapProvider, builder, index, parameters as IDictionary<string, object>);
        }
        else
        {
            var parameterType = parameter.GetType();
            var cacheKey = HashCode.Combine(connection, ormProvider, mapProvider, entityType, parameterType);
            if (!createBatchCommandInitializerCache.TryGetValue(cacheKey, out var objCommandInitializer))
            {
                int columnIndex = 0;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
                var indexExpr = Expression.Parameter(typeof(int), "index");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                var insertBuilder = new StringBuilder($"INSERT INTO {ormProvider.GetTableName(entityMapper.TableName)} (");
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    if (columnIndex > 0)
                        insertBuilder.Append(',');
                    insertBuilder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                    columnIndex++;
                }
                insertBuilder.Append(") VALUES ");

                var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
                var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                var methodInfo3 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });

                var addInsertExpr = Expression.Call(builderExpr, methodInfo2, Expression.Constant(insertBuilder.ToString()));
                var addCommaExpr = Expression.Call(builderExpr, methodInfo1, Expression.Constant(','));
                var greatThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0, typeof(int)));
                blockBodies.Add(Expression.IfThenElse(greatThenExpr, addCommaExpr, addInsertExpr));
                blockBodies.Add(Expression.Call(builderExpr, methodInfo1, Expression.Constant('(')));
                columnIndex = 0;
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    if (columnIndex > 0)
                        blockBodies.Add(Expression.Call(builderExpr, methodInfo1, Expression.Constant(',')));

                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var suffixExpr = Expression.Call(indexExpr, typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes));
                    var parameterNameExpr = Expression.Call(methodInfo3, Expression.Constant(parameterName), suffixExpr);
                    blockBodies.Add(Expression.Call(builderExpr, methodInfo2, parameterNameExpr));

                    AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, propMapper, blockBodies);
                    columnIndex++;
                }
                blockBodies.Add(Expression.Call(builderExpr, methodInfo1, Expression.Constant(')')));

                objCommandInitializer = Expression.Lambda<Action<IDbCommand, IOrmProvider, StringBuilder, int, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, builderExpr, indexExpr, parameterExpr).Compile();
                createBatchCommandInitializerCache.TryAdd(cacheKey, objCommandInitializer);
            }
            commandInitializer = (command, ormProvider, mapProvider, builder, index, parameters)
               => objCommandInitializer.Invoke(command, ormProvider, builder, index, parameters);
        }
        return commandInitializer;
    }
    public static Action<IDbCommand, IOrmProvider, IEntityMapProvider, object, StringBuilder, StringBuilder> BuildCreateWithBiesCommandInitializer(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object parameters)
    {
        Action<IDbCommand, IOrmProvider, IEntityMapProvider, object, StringBuilder, StringBuilder> commandInitializer = null;
        if (parameters is IDictionary<string, object> dict)
        {
            Action<IDbCommand, IOrmProvider, IEntityMapProvider, IDictionary<string, object>, StringBuilder, StringBuilder> dictCommandInitializer = null;
            dictCommandInitializer = (command, ormProvider, mapProvider, dict, insertBuilder, valuesBuilder) =>
            {
                int index = 0;
                var entityMapper = mapProvider.GetEntityMap(entityType);
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
                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    insertBuilder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                    valuesBuilder.Append(parameterName);
                    command.Parameters.Add(ormProvider.CreateParameter(propMapper, parameterName, item.Value));
                    index++;
                }
            };
            commandInitializer = (command, ormProvider, mapProvider, parameters, insertBuilder, valuesBuilder)
                => dictCommandInitializer.Invoke(command, ormProvider, mapProvider, dict, insertBuilder, valuesBuilder);
        }
        else
        {
            Action<IDbCommand, IOrmProvider, object, StringBuilder, StringBuilder> objCommandInitializer = null;
            var parameterType = parameters.GetType();
            if (!parameterType.IsEntityType(out _))
                throw new NotSupportedException("只支持类对象，不支持基础类型");

            var cacheKey = HashCode.Combine(connection, ormProvider, mapProvider, entityType, parameterType);
            if (!createWithBiesCommandInitializerCache.TryGetValue(cacheKey, out objCommandInitializer))
            {
                int columnIndex = 0;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");
                var insertBuilderExpr = Expression.Parameter(typeof(StringBuilder), "insertBuilder");
                var valueBuilderExpr = Expression.Parameter(typeof(StringBuilder), "valueBuilder");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                var insertBuilder = new StringBuilder();
                var valuesBuilder = new StringBuilder();
                var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
                var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                        continue;

                    if (columnIndex > 0)
                    {
                        blockBodies.Add(Expression.Call(insertBuilderExpr, methodInfo1, Expression.Constant(',')));
                        blockBodies.Add(Expression.Call(valueBuilderExpr, methodInfo1, Expression.Constant(',')));
                    }
                    var fieldName = ormProvider.GetFieldName(propMapper.FieldName);
                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName);
                    blockBodies.Add(Expression.Call(insertBuilderExpr, methodInfo2, Expression.Constant(fieldName)));
                    blockBodies.Add(Expression.Call(valueBuilderExpr, methodInfo2, parameterNameExpr));
                    AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, propMapper, blockBodies);
                    columnIndex++;
                }
                objCommandInitializer = Expression.Lambda<Action<IDbCommand, IOrmProvider, object, StringBuilder, StringBuilder>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr, insertBuilderExpr, valueBuilderExpr).Compile();
                createWithBiesCommandInitializerCache.TryAdd(cacheKey, objCommandInitializer);
            }
            commandInitializer = (command, ormProvider, mapProvider, parameters, insertBuilder, valuesBuilder)
                => objCommandInitializer.Invoke(command, ormProvider, parameters, insertBuilder, valuesBuilder);
        }
        return commandInitializer;
    }

    public static Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string> BuildUpdateEntitySqlParameters(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object updateObj)
    {
        Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string> commandInitializer = null;
        if (updateObj is IDictionary<string, object> dict)
        {
            Func<IDbCommand, IOrmProvider, IEntityMapProvider, IDictionary<string, object>, string> dictCommandInitializer = null;
            dictCommandInitializer = (command, ormProvider, mapProvider, dict) =>
            {
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
                    builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                    var dbParameter = ormProvider.CreateParameter(memberMapper, parameterName, dict[item.Key]);
                    command.Parameters.Add(dbParameter);
                }
                builder.Append(" WHERE ");
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    if (!dict.ContainsKey(keyMapper.MemberName))
                        throw new Exception($"当前字典中不包含表{entityMapper.TableName}的主键字段{keyMapper.MemberName}无法完成更新操作");
                    var parameterName = $"{ormProvider.ParameterPrefix}k{keyMapper.MemberName}";
                    builder.Append($"{ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");
                    var dbParameter = ormProvider.CreateParameter(keyMapper, parameterName, dict[keyMapper.MemberName]);
                    command.Parameters.Add(dbParameter);
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
                var members = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
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

                foreach (var memberInfo in members)
                {
                    var parameterName = ormProvider.ParameterPrefix + memberInfo.Name;
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;

                    builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                    var fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, memberInfo.Name);
                    var createParameterExpr = Expression.Call(methodInfo1, Expression.Constant(memberMapper), Expression.Constant(parameterName), fieldValueExpr);
                    blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo2, createParameterExpr));
                }
                builder.Append(" WHERE ");
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    var parameterName = $"{ormProvider.ParameterPrefix}k{keyMapper.MemberName}";
                    builder.Append($"{ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");
                    var fieldValueExpr = Expression.PropertyOrField(typedParameterExpr, keyMapper.MemberName);
                    var createParameterExpr = Expression.Call(methodInfo1, Expression.Constant(keyMapper), Expression.Constant(parameterName), fieldValueExpr);
                    blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo2, createParameterExpr));
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
    public static Action<ISqlVisitor, List<SetField>, List<IDbDataParameter>, object> BuildUpdateSetWithParameters(ISqlVisitor sqlVisitor, Type entityType, object updateObj)
    {
        Action<ISqlVisitor, List<SetField>, List<IDbDataParameter>, object> setFieldsInitializer = null;
        if (updateObj is IDictionary<string, object> dict)
        {
            Action<ISqlVisitor, List<SetField>, List<IDbDataParameter>, IDictionary<string, object>> dictSetFieldsInitializer = null;
            dictSetFieldsInitializer = (visitor, memberMappers, dbParameters, dict) =>
            {
                var entityMapper = visitor.MapProvider.GetEntityMap(entityType);
                var keyMappers = new List<SetField>();
                var keyDbParameters = new List<IDbDataParameter>();
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;

                    if (memberMapper.IsKey)
                    {
                        var parameterName = $"{visitor.OrmProvider.ParameterPrefix}k{item.Key}";
                        keyMappers.Add(new SetField { MemberMapper = memberMapper, Value = parameterName });
                        var dbParameter = visitor.OrmProvider.CreateParameter(memberMapper, parameterName, dict[item.Key]);
                        keyDbParameters.Add(dbParameter);
                    }
                    else
                    {
                        var parameterName = $"{visitor.OrmProvider.ParameterPrefix}{item.Key}";
                        memberMappers.Add(new SetField { MemberMapper = memberMapper, Value = parameterName });
                        var dbParameter = visitor.OrmProvider.CreateParameter(memberMapper, parameterName, dict[item.Key]);
                        keyDbParameters.Add(dbParameter);
                    }
                }
                memberMappers.AddRange(keyMappers);
                dbParameters.AddRange(keyDbParameters);
            };
            setFieldsInitializer = (visitor, memberMappers, dbParameters, parameters)
                => dictSetFieldsInitializer.Invoke(visitor, memberMappers, dbParameters, dict);
        }
        else
        {
            var parameterType = updateObj.GetType();
            var cacheKey = HashCode.Combine(sqlVisitor.DbKey, sqlVisitor.OrmProvider, sqlVisitor.MapProvider, entityType, parameterType);
            if (!updateSetFieldsCache.TryGetValue(cacheKey, out var objSetFieldsInitializer))
            {
                var entityMapper = sqlVisitor.MapProvider.GetEntityMap(entityType);
                var members = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var setFieldsExpr = Expression.Parameter(typeof(List<SetField>), "setFields");
                var dbParametersExpr = Expression.Parameter(typeof(List<IDbDataParameter>), "dbParameters");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();

                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                foreach (var memberInfo in members)
                {
                    var parameterName = sqlVisitor.OrmProvider.ParameterPrefix + memberInfo.Name;
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;

                    var fieldValue = sqlVisitor.EvaluateAndCache(updateObj, memberInfo.Name);
                    var methodInfo = typeof(List<IDbDataParameter>).GetMethod(nameof(List<IDbDataParameter>.Add));
                    var dbParameter = sqlVisitor.OrmProvider.CreateParameter(memberMapper, parameterName, fieldValue);
                    blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo, Expression.Constant(dbParameter)));

                    methodInfo = typeof(List<SetField>).GetMethod(nameof(List<SetField>.Add));
                    var setField = new SetField { MemberMapper = memberMapper, Value = parameterName };
                    blockBodies.Add(Expression.Call(setFieldsExpr, methodInfo, Expression.Constant(setField)));
                }
                objSetFieldsInitializer = Expression.Lambda<Action<List<SetField>, List<IDbDataParameter>, object>>(Expression.Block(blockParameters, blockBodies), setFieldsExpr, dbParametersExpr, parameterExpr).Compile();
                updateSetFieldsCache.TryAdd(cacheKey, objSetFieldsInitializer);
            }
            setFieldsInitializer = (visitor, memberMappers, dbParameters, parameters)
                => objSetFieldsInitializer.Invoke(memberMappers, dbParameters, parameters);
        }
        return setFieldsInitializer;
    }
    public static Action<IDbCommand, ISqlVisitor, StringBuilder, object, int> BuildUpdateBulkSetFieldsParameters(ISqlVisitor visitor, Type entityType, object updateObjs)
    {
        Action<IDbCommand, ISqlVisitor, StringBuilder, object, int> bulkSetFieldsInitializer = null;
        var entities = updateObjs as IEnumerable;
        object updateObj = null;
        foreach (var entity in entities)
        {
            updateObj = entity;
            break;
        }
        var updateObjType = updateObj.GetType();
        if (typeof(IDictionary<string, object>).IsAssignableFrom(updateObjType))
        {
            Action<IDbCommand, ISqlVisitor, StringBuilder, IDictionary<string, object>, int> dictBulkSetFieldsInitializer = null;
            dictBulkSetFieldsInitializer = (command, visitor, builder, dict, index) =>
            {
                var entityMapper = visitor.MapProvider.GetEntityMap(entityType);
                var whereBuilder = new StringBuilder();
                var keyDbParameters = new List<IDbDataParameter>();
                builder.Append($"UPDATE {visitor.OrmProvider.GetTableName(entityMapper.TableName)} SET ");
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;

                    if (memberMapper.IsKey)
                    {
                        var parameterName = $"{visitor.OrmProvider.ParameterPrefix}k{item.Key}{index}";
                        whereBuilder.Append($"{visitor.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        var dbParameter = visitor.OrmProvider.CreateParameter(memberMapper, parameterName, dict[item.Key]);
                        keyDbParameters.Add(dbParameter);
                    }
                    else
                    {
                        var parameterName = $"{visitor.OrmProvider.ParameterPrefix}{item.Key}{index}";
                        builder.Append($"{visitor.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        var dbParameter = visitor.OrmProvider.CreateParameter(memberMapper, parameterName, dict[item.Key]);
                        command.Parameters.Add(dbParameter);
                    }
                }
                builder.Append(whereBuilder);
                keyDbParameters.ForEach(f => command.Parameters.Add(f));
            };
            bulkSetFieldsInitializer = (command, visitor, builder, parameters, index)
                => dictBulkSetFieldsInitializer.Invoke(command, visitor, builder, parameters as IDictionary<string, object>, index);
        }
        else
        {
            var cacheKey = HashCode.Combine(visitor.DbKey, visitor.OrmProvider, visitor.MapProvider, entityType, updateObjType);
            if (!updateBulkSetFieldsCache.TryGetValue(cacheKey, out bulkSetFieldsInitializer))
            {
                var entityMapper = visitor.MapProvider.GetEntityMap(entityType);
                var memberInfos = updateObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "command");
                var visitorExpr = Expression.Parameter(typeof(ISqlVisitor), "visitor");
                var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
                var indexExpr = Expression.Parameter(typeof(int), "index");
                var updateObjExpr = Expression.Parameter(updateObjType, "updateObj");

                var strIndexExpr = Expression.Variable(typeof(string), "strIndex");
                var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.AddRange(new[] { strIndexExpr, parameterNameExpr });

                var methodInfo1 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
                var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                var methodInfo3 = typeof(Extensions).GetMethod(nameof(Extensions.CreateParameter));
                var methodInfo4 = typeof(IList).GetMethod(nameof(IList.Add));

                var methodInfo = typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes);
                var updateHeadExpr = Expression.Constant($"UPDATE {visitor.OrmProvider.GetTableName(entityMapper.TableName)} SET ");
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, updateHeadExpr));
                blockBodies.Add(Expression.Assign(strIndexExpr, Expression.Call(indexExpr, methodInfo)));
                var dbParametersExpr = Expression.Property(commandExpr, nameof(IDbCommand.Parameters));
                var ormProviderExpr = Expression.Property(visitorExpr, nameof(ISqlVisitor.OrmProvider));

                foreach (var memberInfo in memberInfos)
                {
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                       || memberMapper.IsIgnore || memberMapper.IsNavigation
                       || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;
                    if (memberMapper.IsKey) continue;

                    var parameterName = $"{visitor.OrmProvider.ParameterPrefix}{memberInfo.Name}";
                    var concatExpr = Expression.Call(Expression.Constant(parameterName), methodInfo1, strIndexExpr);
                    blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));
                    var setExpr = Expression.Constant($"{visitor.OrmProvider.GetFieldName(memberMapper.FieldName)}=");
                    blockBodies.Add(Expression.Call(builderExpr, methodInfo2, setExpr));
                    blockBodies.Add(Expression.Call(builderExpr, methodInfo2, parameterNameExpr));

                    var memberMapperExpr = Expression.Constant(memberMapper);
                    var fieldValueExpr = Expression.PropertyOrField(updateObjExpr, memberInfo.Name);
                    var createParameterExpr = Expression.Call(methodInfo3, ormProviderExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr);
                    var toObjectExpr = Expression.Convert(createParameterExpr, typeof(object));
                    blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo4, toObjectExpr));
                }
                var whereBuilderExpr = Expression.Variable(typeof(StringBuilder), "whereBuilder");
                blockParameters.Add(whereBuilderExpr);
                var constructor = typeof(StringBuilder).GetConstructor(new Type[] { typeof(string) });
                blockBodies.Add(Expression.Assign(whereBuilderExpr, Expression.New(constructor, Expression.Constant(" WHERE "))));

                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    if (!memberInfos.Exists(f => f.Name == keyMapper.MemberName))
                        throw new ArgumentNullException($"参数类型{updateObjType.FullName}缺少主键字段{keyMapper.MemberName}");

                    var parameterName = $"{visitor.OrmProvider.ParameterPrefix}k{keyMapper.MemberName}";
                    Expression.Assign(parameterNameExpr, Expression.Call(Expression.Constant(parameterName), methodInfo1, strIndexExpr));
                    var setExpr = Expression.Constant($"{visitor.OrmProvider.GetFieldName(keyMapper.FieldName)}=");
                    blockBodies.Add(Expression.Call(whereBuilderExpr, methodInfo2, setExpr));
                    blockBodies.Add(Expression.Call(whereBuilderExpr, methodInfo2, parameterNameExpr));

                    var memberMapperExpr = Expression.Constant(keyMapper);
                    var fieldValueExpr = Expression.PropertyOrField(updateObjExpr, keyMapper.MemberName);
                    var createParameterExpr = Expression.Call(methodInfo3, ormProviderExpr, memberMapperExpr, parameterNameExpr, fieldValueExpr);
                    var toObjectExpr = Expression.Convert(createParameterExpr, typeof(object));
                    blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo4, toObjectExpr));
                }
                methodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(StringBuilder) });
                blockBodies.Add(Expression.Call(builderExpr, methodInfo, whereBuilderExpr));

                bulkSetFieldsInitializer = Expression.Lambda<Action<IDbCommand, ISqlVisitor, StringBuilder, object, int>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, visitorExpr, builderExpr, updateObjExpr, indexExpr).Compile();
                updateBulkSetFieldsCache.TryAdd(cacheKey, bulkSetFieldsInitializer);
            }
        }
        return bulkSetFieldsInitializer;
    }

    public static Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, int, object> BuildDeleteBatchCommandInitializer(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object deleteObj, out bool isNeedEndParenthesis)
    {
        Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, int, object> commandInitializer = null;
        var entityMapper = mapProvider.GetEntityMap(entityType);
        if (entityMapper.KeyMembers.Count <= 0)
            throw new Exception($"表{entityMapper.TableName}在实体映射中没有配置任何主键，无法完成删除操作");
        isNeedEndParenthesis = entityMapper.KeyMembers.Count == 1;
        if (deleteObj is Dictionary<string, object> dict)
        {
            Action<IDbCommand, IOrmProvider, IEntityMapProvider, StringBuilder, int, Dictionary<string, object>> dictCommandInitializer = null;
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
        if (parameters is Dictionary<string, object>)
        {
            commandInitializer = (command, ormProvider, parameter) =>
            {
                int index = 0;
                var dict = parameter as Dictionary<string, object>;
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

                    if (keyMapper.NativeDbType != null)
                        command.Parameters.Add(ormProvider.CreateParameter(parameterName, keyMapper.NativeDbType, fieldValue));
                    else command.Parameters.Add(ormProvider.CreateParameter(parameterName, fieldValue));
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
                        throw new ArgumentNullException($"参数类型{parameterType.FullName}缺少主键字段{keyMapper.MemberName}", "keys");

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

    public static Action<List<IDbDataParameter>, IOrmProvider, object> BuildRawSqlParameters(string dbKey, IOrmProvider ormProvider, string opKey, string rawSql, Type entityType, object parameters)
    {
        Action<List<IDbDataParameter>, IOrmProvider, object> dbParametersInitializer = null;
        if (parameters is Dictionary<string, object>)
        {
            dbParametersInitializer = (dbParameters, ormProvider, parameter) =>
            {
                var dict = parameter as Dictionary<string, object>;
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
            var cacheKey = HashCode.Combine(dbKey, opKey, ormProvider, rawSql, entityType, parameterType);
            if (!rawSqlParametersCache.TryGetValue(cacheKey, out dbParametersInitializer))
            {
                var members = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
                var dbParametersExpr = Expression.Parameter(typeof(List<IDbDataParameter>), "dbParameters");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();

                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                foreach (var memberInfo in members)
                {
                    var parameterName = ormProvider.ParameterPrefix + memberInfo.Name;
                    if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                        continue;
                    var parameterNameExpr = Expression.Constant(parameterName);
                    var valueExpr = Expression.Convert(Expression.PropertyOrField(typedParameterExpr, memberInfo.Name), typeof(object));
                    var methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
                    var dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, valueExpr);
                    methodInfo = typeof(List<IDbDataParameter>).GetMethod(nameof(List<IDbDataParameter>.Add));
                    blockBodies.Add(Expression.Call(dbParametersExpr, methodInfo, dbParameterExpr));
                }
                dbParametersInitializer = Expression.Lambda<Action<List<IDbDataParameter>, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, parameterExpr).Compile();
                rawSqlParametersCache.TryAdd(cacheKey, dbParametersInitializer);
            }
        }
        return dbParametersInitializer;
    }
}