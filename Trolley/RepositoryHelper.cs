using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Trolley;

class RepositoryHelper
{
    private static MethodInfo readerItemByIndex = typeof(IDataRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)
             .Where(p => p.GetIndexParameters().Length > 0 && p.GetIndexParameters()[0].ParameterType == typeof(int)).Select(p => p.GetGetMethod()).First();
    private static ConcurrentDictionary<RuntimeTypeHandle, bool> isAutoIncrementCache = new ConcurrentDictionary<RuntimeTypeHandle, bool>();
    private static ConcurrentDictionary<int, string> typedSqlCache = new ConcurrentDictionary<int, string>();
    private static ConcurrentDictionary<int, CommandInitializer> typedInitializerCache = new ConcurrentDictionary<int, CommandInitializer>();
    private static ConcurrentDictionary<int, CommandInitializer> sqlInitializerCache = new ConcurrentDictionary<int, CommandInitializer>();
    private static ConcurrentDictionary<int, PagedCommandInitializer> pagedInitializerCache = new ConcurrentDictionary<int, PagedCommandInitializer>();
    private static ConcurrentDictionary<int, Func<IDataReader, object>> typedReaderCache = new ConcurrentDictionary<int, Func<IDataReader, object>>();
    private static ConcurrentDictionary<int, Func<IDataReader, object>> sqlReaderCache = new ConcurrentDictionary<int, Func<IDataReader, object>>();
    private static ConcurrentDictionary<int, Func<IDataReader, object>> valueTupleReaderCache = new ConcurrentDictionary<int, Func<IDataReader, object>>();

    public static (CommandInitializer Initializer, bool IsAutoIncrement) BuildCreateCache(IOrmDbFactory dbFactory, TheaConnection connection, Type entityType, ParameterInfo insertObj)
    {
        CommandInitializer commandInitializer = null;
        if (insertObj.IsDictionary)
        {
            var entityMapper = dbFactory.GetEntityMap(entityType);
            commandInitializer = (command, parameters) =>
            {
                var dict = parameters[0] as Dictionary<string, object>;

                var index = 0;
                var insertBuilder = new StringBuilder();
                if (!string.IsNullOrEmpty(command.CommandText))
                    insertBuilder.Append(command.CommandText + ";");
                insertBuilder.Append("INSERT INTO " + connection.OrmProvider.GetTableName(entityMapper.TableName) + " (");
                var valueBuilder = new StringBuilder(") VALUES(");
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || propMapper.IsIgnore)
                        continue;

                    if (index > 0)
                    {
                        insertBuilder.Append(",");
                        valueBuilder.Append(",");
                    }
                    var parameterName = $"{connection.OrmProvider.ParameterPrefix}{item.Key}{insertObj.MulitIndex}";
                    insertBuilder.Append(connection.OrmProvider.GetFieldName(propMapper.FieldName));
                    valueBuilder.Append(parameterName);

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = parameterName;
                    parameter.Direction = ParameterDirection.Input;
                    var itemValue = dict[item.Key];
                    if (itemValue != null && itemValue is string strValue)
                        parameter.Size = strValue.Length;
                    if (itemValue == null) parameter.Value = DBNull.Value;
                    else parameter.Value = itemValue;
                    command.Parameters.Add(parameter);
                    index++;
                }
                valueBuilder.Append(") ");
                if (entityMapper.IsAutoIncrement)
                    valueBuilder.AppendFormat(connection.OrmProvider.SelectIdentitySql, entityMapper.AutoIncrementField);

                command.CommandText = $"{insertBuilder}{valueBuilder}";
                command.CommandType = CommandType.Text;
            };
            return (commandInitializer, entityMapper.IsAutoIncrement);
        }
        var cacheKey = GetTypedSqlParameterKey(connection, "BuildCreateCache", insertObj.IsMulti, entityType, entityType, insertObj.ParameterType);
        if (!typedInitializerCache.TryGetValue(cacheKey, out commandInitializer))
        {
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var insertObjMapper = dbFactory.GetEntityMap(insertObj.ParameterType);
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var objExpr = Expression.Parameter(typeof(object), "obj");
            var typedObjExpr = Expression.Variable(insertObj.ParameterType, "entity");

            ParameterExpression builderExpr = null;
            ParameterExpression suffixExpr = null;
            if (insertObj.IsMulti)
            {
                builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
                suffixExpr = Expression.Parameter(typeof(string), "suffix");
            }

            blockParameters.Add(typedObjExpr);
            blockBodies.Add(Expression.Assign(typedObjExpr, Expression.Convert(objExpr, insertObj.ParameterType)));
            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var concatMethodInfo = typeof(string).GetMethod(nameof(String.Concat), new Type[] { typeof(string), typeof(string) });

            int index = 0;
            StringBuilder sqlBuilder = null;
            if (!insertObj.IsMulti) sqlBuilder = new StringBuilder("(");
            else blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant("(", typeof(string))));

            foreach (var insertObjPropMapper in insertObjMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(insertObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore || propMapper.IsAutoIncrement)
                    continue;

                if (index > 0)
                {
                    if (!insertObj.IsMulti) sqlBuilder.Append(",");
                    else blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(",", typeof(string))));
                }
                var parameterName = $"{connection.OrmProvider.ParameterPrefix}{insertObjPropMapper.MemberName}{insertObj.MulitIndex}";

                Expression parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                if (insertObj.IsMulti)
                {
                    parameterNameExpr = Expression.Call(concatMethodInfo, parameterNameExpr, suffixExpr);
                    blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, parameterNameExpr));
                }
                else sqlBuilder.Append(parameterName);

                AddParameter(connection, commandExpr, parameterNameExpr, typedObjExpr, insertObjPropMapper, propMapper, insertObj, blockParameters, blockBodies);
                index++;
            }
            if (!insertObj.IsMulti) sqlBuilder.Append(")");
            else blockBodies.Add(Expression.Call(builderExpr, appendMethodInfo, Expression.Constant(")", typeof(string))));

            //isAutoIncrement与实体类型相关，只要实体类型存在自增列，就为true             
            isAutoIncrementCache.AddOrUpdate(entityType.TypeHandle, entityMapper.IsAutoIncrement, (k, o) => entityMapper.IsAutoIncrement);

            var sql = BuildInsertSql(connection, entityMapper, insertObjMapper);
            var insertSql = sql.InsertSql;
            var valuesSql = sql.ValuesSql;
            if (insertObj.IsMulti)
            {
                var initializer = Expression.Lambda<Action<IDbCommand, StringBuilder, string, object>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, builderExpr, suffixExpr, objExpr).Compile();

                commandInitializer = (command, parameters) =>
                {
                    var resultBuilder = new StringBuilder();
                    if (!string.IsNullOrEmpty(command.CommandText))
                        resultBuilder.Append(command.CommandText + ";");

                    resultBuilder.Append(insertSql);
                    resultBuilder.Append(" VALUES ");
                    int index = 1;
                    var enumerable = parameters[0] as IEnumerable;
                    foreach (var item in enumerable)
                    {
                        if (index > 1) resultBuilder.Append(",");
                        initializer(command, resultBuilder, index.ToString(), item);
                        index++;
                    }

                    command.CommandText = resultBuilder.ToString();
                    command.CommandType = CommandType.Text;
                };
            }
            else
            {
                var initializer = Expression.Lambda<Action<IDbCommand, object>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, objExpr).Compile();
                var usedSql = $"{insertSql} {valuesSql}";
                if (entityMapper.IsAutoIncrement)
                    usedSql += string.Format(connection.OrmProvider.SelectIdentitySql, connection.OrmProvider.GetFieldName(entityMapper.AutoIncrementField));

                commandInitializer = (command, parameters) =>
                {
                    initializer(command, parameters[0]);
                    var sql = usedSql;
                    if (!string.IsNullOrEmpty(command.CommandText))
                        sql = command.CommandText + ";" + usedSql;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                };
            }

            typedInitializerCache.TryAdd(cacheKey, commandInitializer);
        }
        isAutoIncrementCache.TryGetValue(entityType.TypeHandle, out var isAutoIncrement);
        return (commandInitializer, isAutoIncrement);
    }
    /// <summary>
    /// 查询返回指定实体类型的数据，生成SELECT * FROM TABLE WHERE @whereObj SQL语句，并初始化参数
    /// </summary>
    /// <param name="dbFactory"></param>
    /// <param name="connection"></param>
    /// <param name="entityType"></param>
    /// <param name="targetObjType"></param>
    /// <param name="whereObj"></param>
    /// <returns></returns>
    public static CommandInitializer BuildQueryCache(IOrmDbFactory dbFactory, TheaConnection connection, Type entityType, Type targetObjType, ParameterInfo whereObj)
    {
        CommandInitializer commandInitializer = null;
        if (whereObj.IsDictionary)
        {
            commandInitializer = (command, parameters) =>
            {
                var dict = parameters[0] as Dictionary<string, object>;
                var entityMapper = dbFactory.GetEntityMap(entityType);
                var sql = BuildQuerySelectCache(dbFactory, connection, entityType, targetObjType);

                var index = 0;
                var sqlBuilder = new StringBuilder();
                if (!string.IsNullOrEmpty(command.CommandText))
                    sqlBuilder.Append(command.CommandText + ";");
                sqlBuilder.Append(sql + " WHERE ");
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || propMapper.IsIgnore)
                        continue;

                    if (index > 0) sqlBuilder.Append(" AND ");
                    var parameterName = $"{connection.OrmProvider.ParameterPrefix}{item.Key}{whereObj.MulitIndex}";
                    sqlBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} = {parameterName}");

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = parameterName;
                    parameter.Direction = ParameterDirection.Input;
                    var itemValue = dict[item.Key];
                    if (itemValue != null && itemValue is string strValue)
                        parameter.Size = strValue.Length;
                    if (itemValue == null) parameter.Value = DBNull.Value;
                    else parameter.Value = itemValue;
                    command.Parameters.Add(parameter);
                    index++;
                }
                command.CommandText = sqlBuilder.ToString();
                command.CommandType = CommandType.Text;
            };
            return commandInitializer;
        }

        var cacheKey = GetTypedSqlParameterKey(connection, "BuildQueryCache", whereObj.IsMulti, entityType, targetObjType, whereObj.ParameterType);
        if (!typedInitializerCache.TryGetValue(cacheKey, out commandInitializer))
        {
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var sqlBuilder = new StringBuilder("SELECT ");

            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var objExpr = Expression.Parameter(typeof(object), "obj");

            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            var typedWhereObjExpr = Expression.Variable(whereObj.ParameterType, "whereObj");

            EntityMap selectObjMapper = null;
            if (entityType != targetObjType)
                selectObjMapper = dbFactory.GetEntityMap(targetObjType);
            else selectObjMapper = entityMapper;
            var whereObjMapper = dbFactory.GetEntityMap(whereObj.ParameterType);

            blockParameters.Add(typedWhereObjExpr);
            blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(objExpr, whereObj.ParameterType)));

            int index = 0;
            string parameterName = null;
            ParameterExpression deferredExpr = null;
            bool hasDeferred = false;
            foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore)
                    continue;

                if (propMapper.MemberType != whereObjPropMapper.MemberType
                    && typeof(IEnumerable).IsAssignableFrom(whereObjPropMapper.MemberType)
                    && whereObjPropMapper.MemberType != typeof(string))
                {
                    //有数组参数，将不再设置sqlBuilder变量
                    if (!connection.OrmProvider.IsSupportArrayParameter)
                    {
                        hasDeferred = true;
                        break;
                    }
                }
            }
            if (hasDeferred)
            {
                deferredExpr = Expression.Variable(typeof(Stack<Action<StringBuilder>>), "deferredStack");
                blockParameters.Add(deferredExpr);
                var newExp = Expression.New(typeof(Stack<Action<StringBuilder>>).GetConstructor(Type.EmptyTypes));
                blockBodies.Add(Expression.Assign(deferredExpr, newExp));
            }
            index = 0;
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore) continue;
                if (entityType == targetObjType || selectObjMapper.TryGetMemberMap(propMapper.MemberName, out var memberMap))
                {
                    if (index > 0) sqlBuilder.Append(",");
                    sqlBuilder.Append(GetAliasParameterSql(connection, propMapper.FieldName, propMapper.MemberName));
                    index++;
                }
            }
            sqlBuilder.Append($" FROM {connection.OrmProvider.GetTableName(entityMapper.TableName)} WHERE ");
            index = 0;
            foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore)
                    continue;
                if (index > 0) sqlBuilder.Append(" AND ");
                parameterName = $"{connection.OrmProvider.ParameterPrefix}{propMapper.MemberName}{whereObj.MulitIndex}";

                //支持数组参数
                if (propMapper.MemberType != whereObjPropMapper.MemberType
                    && typeof(IEnumerable).IsAssignableFrom(whereObjPropMapper.MemberType)
                    && whereObjPropMapper.MemberType != typeof(string))
                {
                    if (connection.OrmProvider.IsSupportArrayParameter)
                    {
                        sqlBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} = ANY({parameterName})");
                        AddParameter(connection, commandExpr, parameterName, typedWhereObjExpr, whereObjPropMapper, propMapper, whereObj, blockParameters, blockBodies);
                    }
                    else
                    {
                        sqlBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} IN ");
                        BuildWhereInSqlParameters(connection, commandExpr, typedWhereObjExpr, deferredExpr, parameterName, whereObjPropMapper, sqlBuilder.Length, blockBodies);
                    }
                }
                else
                {
                    sqlBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} = {parameterName}");
                    AddParameter(connection, commandExpr, parameterName, typedWhereObjExpr, whereObjPropMapper, propMapper, whereObj, blockParameters, blockBodies);
                }
                index++;
            }
            if (hasDeferred)
            {
                var returnLabel = Expression.Label(typeof(Stack<Action<StringBuilder>>));
                blockBodies.Add(Expression.Return(returnLabel, deferredExpr));

                var converExpr = Expression.Convert(Expression.Constant(null), typeof(Stack<Action<StringBuilder>>));
                blockBodies.Add(Expression.Label(returnLabel, converExpr));
            }

            string sql = sqlBuilder.ToString();
            if (hasDeferred)
            {
                var initializer = Expression.Lambda<Func<IDbCommand, object, Stack<Action<StringBuilder>>>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, objExpr).Compile();

                commandInitializer = (command, parameters) =>
                {
                    var resultBuilder = new StringBuilder();
                    if (!string.IsNullOrEmpty(command.CommandText))
                        resultBuilder.Append(command.CommandText + ";");
                    resultBuilder.Append(sql);
                    var stack = initializer(command, parameters[0]);
                    while (stack.TryPop(out var deferred))
                    {
                        deferred(resultBuilder);
                    }
                    command.CommandText = resultBuilder.ToString();
                    command.CommandType = CommandType.Text;
                };
            }
            else
            {
                var initializer = Expression.Lambda<Action<IDbCommand, object>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, objExpr).Compile();

                commandInitializer = (command, parameters) =>
                {
                    initializer(command, parameters[0]);
                    var resultSql = sql;
                    if (!string.IsNullOrEmpty(command.CommandText))
                        resultSql = command.CommandText + ";" + sql;
                    command.CommandText = resultSql;
                    command.CommandType = CommandType.Text;
                };
            }
            typedInitializerCache.TryAdd(cacheKey, commandInitializer);
        }
        return commandInitializer;
    }
    public static CommandInitializer BuildQuerySelectCache(IOrmDbFactory dbFactory, TheaConnection connection, Type entityType, Type targetObjType)
    {
        var sql = BuildQuerySelectSql(dbFactory, connection, entityType, targetObjType);
        return (command, parameters) =>
        {
            if (!string.IsNullOrEmpty(command.CommandText))
                sql = command.CommandText + ";" + sql;
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
        };
    }
    /// <summary>
    /// 生成SELECT SQL语句
    /// </summary>
    /// <param name="dbFactory"></param>
    /// <param name="connection"></param>
    /// <param name="entityType"></param>
    /// <param name="targetObjType"></param>
    /// <returns></returns>
    private static string BuildQuerySelectSql(IOrmDbFactory dbFactory, TheaConnection connection, Type entityType, Type targetObjType)
    {
        var cacheKey = GetTypedSqlKey(connection, entityType, targetObjType);
        if (!typedSqlCache.TryGetValue(cacheKey, out var sql))
        {
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var sqlBuilder = new StringBuilder("SELECT ");
            var selectObjMapper = dbFactory.GetEntityMap(targetObjType);
            int index = 0;
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore) continue;
                if (entityType == targetObjType || selectObjMapper.TryGetMemberMap(propMapper.MemberName, out var memberMapper))
                {
                    if (index > 0) sqlBuilder.Append(",");
                    sqlBuilder.Append(GetAliasParameterSql(connection, propMapper.FieldName, propMapper.MemberName));
                    index++;
                }
            }
            sqlBuilder.Append($" FROM {connection.OrmProvider.GetTableName(entityMapper.TableName)}");
            typedSqlCache.TryAdd(cacheKey, sql = sqlBuilder.ToString());
        }
        return sql;
    }
    /// <summary>
    /// 根据SQL初始化参数,已有sql
    /// </summary>
    /// <param name="dbFactory"></param>
    /// <param name="connection"></param>
    /// <param name="cmdType"></param>
    /// <param name="whereObj"></param>
    /// <returns></returns>
    public static CommandInitializer BuildQueryWhereSqlCache(IOrmDbFactory dbFactory, TheaConnection connection, CommandType cmdType, ParameterInfo whereObj)
    {
        CommandInitializer commandInitializer = null;
        if (whereObj.IsDictionary)
        {
            commandInitializer = (command, parameters) =>
            {
                var sql = parameters[0] as string;
                var dict = parameters[1] as Dictionary<string, object>;

                foreach (var item in dict)
                {
                    var parameterName = $"{connection.OrmProvider.ParameterPrefix}{item.Key}{whereObj.MulitIndex}";
                    if (!Regex.IsMatch(sql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                        continue;

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = parameterName;
                    parameter.Direction = ParameterDirection.Input;
                    var itemValue = dict[item.Key];
                    if (itemValue != null && itemValue is string strValue)
                        parameter.Size = strValue.Length;
                    if (itemValue == null) parameter.Value = DBNull.Value;
                    else parameter.Value = itemValue;
                    command.Parameters.Add(parameter);
                }
            };
            return commandInitializer;
        }

        var cacheKey = GetSqlParameterKey(connection, cmdType, whereObj.IsMulti, whereObj.ParameterType);
        if (!sqlInitializerCache.TryGetValue(cacheKey, out commandInitializer))
        {
            var whereObjMapper = dbFactory.GetEntityMap(whereObj.ParameterType);
            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var objExpr = Expression.Parameter(typeof(object), "obj");
            var sqlExpr = Expression.Variable(typeof(string), "sql");
            var typedWhereObjExpr = Expression.Variable(whereObj.ParameterType, "whereObj");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            blockParameters.AddRange(new[] { typedWhereObjExpr, sqlExpr });
            blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(objExpr, whereObj.ParameterType)));
            var methodInfo = typeof(IDbCommand).GetProperty(nameof(IDbCommand.CommandText)).GetGetMethod();
            blockBodies.Add(Expression.Assign(sqlExpr, Expression.Call(commandExpr, methodInfo)));

            methodInfo = typeof(Regex).GetMethod(nameof(Regex.IsMatch), new Type[] { typeof(string), typeof(string), typeof(RegexOptions) });
            foreach (var propMapper in whereObjMapper.MemberMaps)
            {
                var parameterName = $"{connection.OrmProvider.ParameterPrefix}{propMapper.MemberName}";
                var isMatchExpr = Expression.Call(methodInfo, sqlExpr, Expression.Constant(parameterName + @"([^\p{L}\p{N}_]+|$)", typeof(string)),
                    Expression.Constant(RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant, typeof(RegexOptions)));

                var parameterBlockBodies = new List<Expression>();
                //支持数组参数
                if (typeof(IEnumerable).IsAssignableFrom(propMapper.MemberType) && propMapper.MemberType != typeof(string))
                {
                    if (connection.OrmProvider.IsSupportArrayParameter)
                    {
                        var parameterBlockParameters = new List<ParameterExpression>();
                        AddParameter(connection, commandExpr, parameterName, typedWhereObjExpr, propMapper, propMapper, whereObj, parameterBlockParameters, parameterBlockBodies);
                        blockBodies.Add(Expression.IfThen(isMatchExpr, Expression.Block(parameterBlockParameters, parameterBlockBodies)));
                    }
                    else
                    {
                        BuildWhereInSqlParameters(connection, commandExpr, typedWhereObjExpr, propMapper, parameterBlockBodies);
                        blockBodies.Add(Expression.IfThen(isMatchExpr, Expression.Block(parameterBlockBodies)));
                    }
                }
                else
                {
                    var parameterBlockParameters = new List<ParameterExpression>();
                    AddParameter(connection, commandExpr, parameterName, typedWhereObjExpr, propMapper, propMapper, whereObj, parameterBlockParameters, parameterBlockBodies);
                    blockBodies.Add(Expression.IfThen(isMatchExpr, Expression.Block(parameterBlockParameters, parameterBlockBodies)));
                }
            }

            var initializer = Expression.Lambda<Action<IDbCommand, object>>(
                Expression.Block(blockParameters, blockBodies), commandExpr, objExpr).Compile();
            commandInitializer = (command, parameters) => initializer(command, parameters[0]);

            sqlInitializerCache.TryAdd(cacheKey, commandInitializer);
        }
        return commandInitializer;
    }
    /// <summary>
    /// 根据实体生成SELECT分页 SQL语句, 没有whereObj参数
    /// </summary>
    /// <param name="dbFactory"></param>
    /// <param name="connection"></param>
    /// <param name="entityType"></param>
    /// <param name="targetObjType"></param>
    /// <returns></returns>
    public static PagedCommandInitializer BuildQueryPageCache(IOrmDbFactory dbFactory, TheaConnection connection, Type entityType, Type targetObjType)
    {
        var cacheKey = GetTypedSqlParameterKey(connection, "BuildQueryPageCache1", false, entityType, targetObjType, null);
        if (!pagedInitializerCache.TryGetValue(cacheKey, out var commandInitializer))
        {
            EntityMap entityMapper = dbFactory.GetEntityMap(entityType);
            EntityMap selectObjMapper = null;
            if (entityType != targetObjType)
                selectObjMapper = dbFactory.GetEntityMap(targetObjType);
            else selectObjMapper = entityMapper;

            int index = 0;
            var sqlBuilder = new StringBuilder();
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore) continue;
                if (entityType == targetObjType || selectObjMapper.TryGetMemberMap(propMapper.MemberName, out var memberMap))
                {
                    if (index > 0) sqlBuilder.Append(",");
                    sqlBuilder.Append(GetAliasParameterSql(connection, propMapper.FieldName, propMapper.MemberName));
                    index++;
                }
            }
            var selectSql = sqlBuilder.ToString();
            var tableSql = connection.OrmProvider.GetTableName(entityMapper.TableName);
            commandInitializer = (command, pageIndex, pageSize, orderBy, parameters) =>
            {
                if (pageIndex >= 1) pageIndex = pageIndex - 1;
                var skip = pageIndex * pageSize;
                //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ WHERE /**conditions**/");                   
                var pageSql = connection.OrmProvider.GetPagingTemplate(skip, pageSize, orderBy);
                pageSql = pageSql.Replace("/**fields**/", selectSql);
                pageSql = pageSql.Replace("/**tables**/", tableSql);
                pageSql = pageSql.Replace("WHERE /**conditions**/", string.Empty);

                var sql = $"SELECT COUNT(*) FROM {tableSql};{pageSql}";
                if (!string.IsNullOrEmpty(command.CommandText))
                    sql = command.CommandText + ";" + sql;
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
            };
            pagedInitializerCache.TryAdd(cacheKey, commandInitializer);
        }
        return commandInitializer;
    }
    /// <summary>
    /// 根据类型生成SELECT+WHERE分页 SQL语句，并初始化参数
    /// </summary>
    /// <param name="dbFactory"></param>
    /// <param name="connection"></param>
    /// <param name="entityType"></param>
    /// <param name="targetObjType"></param>
    /// <param name="whereObj"></param>
    /// <returns></returns>
    public static PagedCommandInitializer BuildQueryPageCache(IOrmDbFactory dbFactory, TheaConnection connection, Type entityType, Type targetObjType, ParameterInfo whereObj)
    {
        PagedCommandInitializer commandInitializer = null;
        if (whereObj.IsDictionary)
        {
            commandInitializer = (command, pageIndex, pageSize, orderBy, parameters) =>
            {
                var dict = parameters[0] as Dictionary<string, object>;
                var entityMapper = dbFactory.GetEntityMap(entityType);
                var selectSql = BuildQuerySelectSql(dbFactory, connection, entityType, targetObjType);

                var index = 0;
                var whereBuilder = new StringBuilder();
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || propMapper.IsIgnore)
                        continue;

                    if (index > 0) whereBuilder.Append(" AND ");
                    var parameterName = $"{connection.OrmProvider.ParameterPrefix}{item.Key}{whereObj.MulitIndex}";
                    whereBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = parameterName;
                    parameter.Direction = ParameterDirection.Input;
                    var itemValue = dict[item.Key];
                    if (itemValue != null && itemValue is string strValue)
                        parameter.Size = strValue.Length;
                    if (itemValue == null) parameter.Value = DBNull.Value;
                    else parameter.Value = itemValue;
                    command.Parameters.Add(parameter);
                    index++;
                }

                if (pageIndex >= 1) pageIndex = pageIndex - 1;
                var skip = pageIndex * pageSize;
                //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ WHERE /**conditions**/");
                var tableSql = connection.OrmProvider.GetTableName(entityMapper.TableName);
                var pageSql = connection.OrmProvider.GetPagingTemplate(skip, pageSize, orderBy);
                pageSql = pageSql.Replace("/**fields**/", selectSql.Substring(7));//去掉SELECT
                pageSql = pageSql.Replace("/**tables**/", tableSql);
                pageSql = pageSql.Replace("/**conditions**/", whereBuilder.ToString());

                var sql = $"SELECT COUNT(*) FROM {tableSql};{pageSql}";
                if (!string.IsNullOrEmpty(command.CommandText))
                    sql = command.CommandText + ";" + sql;
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
            };
            return commandInitializer;
        }

        var cacheKey = GetTypedSqlParameterKey(connection, "BuildQueryPageCache2", whereObj.IsMulti, entityType, targetObjType, whereObj.ParameterType);
        if (!pagedInitializerCache.TryGetValue(cacheKey, out commandInitializer))
        {
            var entityMapper = dbFactory.GetEntityMap(entityType);

            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var objExpr = Expression.Parameter(typeof(object), "obj");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            var typedWhereObjExpr = Expression.Variable(whereObj.ParameterType, "whereObj");

            var selectObjMapper = dbFactory.GetEntityMap(targetObjType);
            var whereObjMapper = dbFactory.GetEntityMap(whereObj.ParameterType);

            ParameterExpression deferredExpr = null;
            bool hasDeferred = false;
            foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore)
                    continue;

                if (propMapper.MemberType != whereObjPropMapper.MemberType
                    && typeof(IEnumerable).IsAssignableFrom(whereObjPropMapper.MemberType)
                    && whereObjPropMapper.MemberType != typeof(string))
                {
                    //有数组参数，将不再设置sqlBuilder变量
                    if (!connection.OrmProvider.IsSupportArrayParameter)
                    {
                        hasDeferred = true;
                        break;
                    }
                }
            }
            if (hasDeferred)
            {
                deferredExpr = Expression.Variable(typeof(Stack<Action<StringBuilder>>), "deferredStack");
                blockParameters.Add(deferredExpr);
                var newExp = Expression.New(typeof(Stack<Action<StringBuilder>>).GetConstructor(Type.EmptyTypes));
                blockBodies.Add(Expression.Assign(deferredExpr, newExp));
            }

            blockParameters.Add(typedWhereObjExpr);
            blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(objExpr, whereObj.ParameterType)));

            int index = 0;
            string parameterName = null;
            var selectBuilder = new StringBuilder();
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore) continue;
                if (entityType == targetObjType || selectObjMapper.TryGetMemberMap(propMapper.MemberName, out var memberMap))
                {
                    if (index > 0) selectBuilder.Append(",");
                    selectBuilder.Append(GetAliasParameterSql(connection, propMapper.FieldName, propMapper.MemberName));
                    index++;
                }
            }

            index = 0;
            var whereBuilder = new StringBuilder();
            foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore)
                    continue;

                if (index > 0) whereBuilder.Append(" AND ");
                parameterName = $"{connection.OrmProvider.ParameterPrefix}{propMapper.MemberName}{whereObj.MulitIndex}";

                //支持数组参数
                if (propMapper.MemberType != whereObjPropMapper.MemberType
                    && typeof(IEnumerable).IsAssignableFrom(whereObjPropMapper.MemberType)
                    && whereObjPropMapper.MemberType != typeof(string))
                {
                    if (connection.OrmProvider.IsSupportArrayParameter)
                    {
                        whereBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} = ANY({parameterName})");
                        AddParameter(connection, commandExpr, parameterName, typedWhereObjExpr, whereObjPropMapper, propMapper, whereObj, blockParameters, blockBodies);
                    }
                    else
                    {
                        whereBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} IN ");
                        BuildWhereInSqlParameters(connection, commandExpr, typedWhereObjExpr, deferredExpr, parameterName, whereObjPropMapper, whereBuilder.Length, blockBodies);
                    }
                }
                else
                {
                    whereBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    AddParameter(connection, commandExpr, parameterName, typedWhereObjExpr, whereObjPropMapper, propMapper, whereObj, blockParameters, blockBodies);
                }
                index++;
            }

            var selectSql = selectBuilder.ToString();
            var whereSql = whereBuilder.ToString();
            var tableSql = connection.OrmProvider.GetTableName(entityMapper.TableName);

            if (hasDeferred)
            {
                var returnLabel = Expression.Label(typeof(Stack<Action<StringBuilder>>));
                blockBodies.Add(Expression.Return(returnLabel, deferredExpr));

                var converExpr = Expression.Convert(Expression.Constant(null), typeof(Stack<Action<StringBuilder>>));
                blockBodies.Add(Expression.Label(returnLabel, converExpr));

                var initializer = Expression.Lambda<Func<IDbCommand, object, Stack<Action<StringBuilder>>>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, objExpr).Compile();

                commandInitializer = (command, pageIndex, pageSize, orderBy, parameters) =>
                {
                    var resultBuidler = new StringBuilder(whereSql);
                    var stack = initializer(command, parameters[0]);
                    while (stack.TryPop(out var deferred))
                    {
                        deferred(resultBuidler);
                    }
                    var result = resultBuidler.ToString();

                    if (pageIndex >= 1) pageIndex = pageIndex - 1;
                    var skip = pageIndex * pageSize;
                    var pageSql = connection.OrmProvider.GetPagingTemplate(skip, pageSize, orderBy);
                    //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ WHERE /**conditions**/");
                    pageSql = pageSql.Replace("/**fields**/", selectSql);
                    pageSql = pageSql.Replace("/**tables**/", tableSql);
                    pageSql = pageSql.Replace("/**conditions**/", result);

                    var sql = $"SELECT COUNT(*) FROM {tableSql} WHERE {result};{pageSql}";
                    if (!string.IsNullOrEmpty(command.CommandText))
                        sql = command.CommandText + ";" + sql;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                };
            }
            else
            {
                var initializer = Expression.Lambda<Action<IDbCommand, object>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, objExpr).Compile();

                commandInitializer = (command, pageIndex, pageSize, orderBy, parameters) =>
                {
                    initializer(command, parameters[0]);
                    if (pageIndex >= 1) pageIndex = pageIndex - 1;
                    var skip = pageIndex * pageSize;
                    var pageSql = connection.OrmProvider.GetPagingTemplate(skip, pageSize, orderBy);
                    //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ WHERE /**conditions**/");
                    pageSql = pageSql.Replace("/**fields**/", selectSql);
                    pageSql = pageSql.Replace("/**tables**/", tableSql);
                    pageSql = pageSql.Replace("/**conditions**/", whereSql);

                    var sql = $"SELECT COUNT(*) FROM {tableSql} WHERE {whereSql};{pageSql}";
                    if (!string.IsNullOrEmpty(command.CommandText))
                        sql = command.CommandText + ";" + sql;

                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                };
            };
            pagedInitializerCache.TryAdd(cacheKey, commandInitializer);
        }
        return commandInitializer;
    }
    public static CommandInitializer BuildUpdateKeyCache(IOrmDbFactory dbFactory, TheaConnection connection, Type entityType, ParameterInfo updateObj)
    {
        CommandInitializer commandInitializer = null;
        if (updateObj.IsDictionary)
        {
            commandInitializer = (command, parameters) =>
            {
                var dict = parameters[0] as Dictionary<string, object>;
                var entityMapper = dbFactory.GetEntityMap(entityType);

                int updateIndex = 0, whereIndex = 0;
                var updateBuilder = new StringBuilder($"UPDATE {connection.OrmProvider.GetTableName(entityMapper.TableName)} SET ");
                var whereBuilder = new StringBuilder(" WHERE ");
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || propMapper.IsIgnore)
                        continue;

                    var parameterName = $"{connection.OrmProvider.ParameterPrefix}{item.Key}{updateObj.MulitIndex}";
                    if (propMapper.IsKey)
                    {
                        if (whereIndex > 0) whereBuilder.Append(" AND ");
                        whereBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                        whereIndex++;
                    }
                    else
                    {
                        if (updateIndex > 0) updateBuilder.Append(",");
                        updateBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                        updateIndex++;
                    }

                    if (command.Parameters.Contains(parameterName))
                        continue;

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = parameterName;
                    parameter.Direction = ParameterDirection.Input;
                    var itemValue = dict[item.Key];
                    if (itemValue != null && itemValue is string strValue)
                        parameter.Size = strValue.Length;
                    if (itemValue == null) parameter.Value = DBNull.Value;
                    else parameter.Value = itemValue;
                    command.Parameters.Add(parameter);
                }
                if (whereIndex == 0) throw new Exception("当前字典参数中不包含主键信息");
                if (updateIndex == 0) throw new Exception("当前字典参数没有可更新的内容");

                var sql = $"{updateBuilder}{whereBuilder}";
                if (!string.IsNullOrEmpty(command.CommandText))
                    sql = command.CommandText + ";" + sql;

                command.CommandText = sql;
                command.CommandType = CommandType.Text;
            };
            return commandInitializer;
        }

        var cacheKey = GetTypedSqlParameterKey(connection, "BuildUpdateKeyCache", updateObj.IsMulti, entityType, updateObj.ParameterType, updateObj.ParameterType);
        if (!typedInitializerCache.TryGetValue(cacheKey, out commandInitializer))
        {
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var updateObjMapper = dbFactory.GetEntityMap(updateObj.ParameterType);

            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var objExpr = Expression.Parameter(typeof(object), "obj");

            //传入参数的实际元素类型，如果传入参数是数组，typedUpdateObjExpr就是其元素的类型
            var typedUpdateObjExpr = Expression.Variable(updateObj.ParameterType, "entity");
            blockParameters.Add(typedUpdateObjExpr);

            var appendMethodInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });

            bool hasDeferred = false;
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || !propMapper.IsKey) continue;
                if (!updateObjMapper.TryGetMemberMap(propMapper.MemberName, out var updateObjPropMapper))
                    throw new Exception($"更新参数中不包含主键栏位的实体成员{updateObjPropMapper.MemberName}");

                if (propMapper.MemberType != updateObjPropMapper.MemberType
                    && typeof(IEnumerable).IsAssignableFrom(updateObjPropMapper.MemberType)
                    && updateObjPropMapper.MemberType != typeof(string))
                {
                    //有数组参数，将不再设置sqlBuilder变量
                    if (!connection.OrmProvider.IsSupportArrayParameter)
                    {
                        hasDeferred = true;
                        break;
                    }
                }
            }

            ParameterExpression sqlBuilderExpr = null;
            ParameterExpression deferredExpr = null;
            ParameterExpression suffixExpr = null;
            StringBuilder sqlBuidler = null;

            blockBodies.Add(Expression.Assign(typedUpdateObjExpr, Expression.Convert(objExpr, updateObj.ParameterType)));
            if (updateObj.IsMulti)
            {
                sqlBuilderExpr = Expression.Parameter(typeof(StringBuilder), "sqlBuidler");
                suffixExpr = Expression.Parameter(typeof(string), "suffix");
            }
            if (hasDeferred)
            {
                deferredExpr = Expression.Variable(typeof(Stack<Action<StringBuilder>>), "deferredStack");
                blockParameters.Add(deferredExpr);
                var newExp = Expression.New(typeof(Stack<Action<StringBuilder>>).GetConstructor(Type.EmptyTypes));
                blockBodies.Add(Expression.Assign(deferredExpr, newExp));
            }

            var index = 0;
            var sql = $"UPDATE {connection.OrmProvider.GetTableName(entityMapper.TableName)} SET ";
            if (!updateObj.IsMulti) sqlBuidler = new StringBuilder(sql);
            else blockBodies.Add(Expression.Call(sqlBuilderExpr, appendMethodInfo, Expression.Constant(sql, typeof(string))));

            foreach (var updateObjPropMapper in updateObjMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(updateObjPropMapper.MemberName, out var propMapper)
                    || propMapper.IsIgnore || propMapper.IsKey)
                    continue;

                if (index > 0)
                {
                    if (!updateObj.IsMulti) sqlBuidler.Append(",");
                    else blockBodies.Add(Expression.Call(sqlBuilderExpr, appendMethodInfo, Expression.Constant(",", typeof(string))));
                }

                var parameterName = $"{connection.OrmProvider.ParameterPrefix}{propMapper.MemberName}{updateObj.MulitIndex}";
                Expression parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                if (updateObj.IsMulti)
                    parameterNameExpr = Expression.Call(concatMethodInfo, parameterNameExpr, suffixExpr);

                //field=@propName
                if (updateObj.IsMulti)
                {
                    blockBodies.Add(Expression.Call(sqlBuilderExpr, appendMethodInfo,
                        Expression.Constant(connection.OrmProvider.GetFieldName(propMapper.FieldName) + "=", typeof(string))));
                    blockBodies.Add(Expression.Call(sqlBuilderExpr, appendMethodInfo, parameterNameExpr));
                }
                else sqlBuidler.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                AddParameter(connection, commandExpr, parameterNameExpr, typedUpdateObjExpr, updateObjPropMapper, propMapper, updateObj, blockParameters, blockBodies);
                index++;
            }

            index = 0;
            if (!updateObj.IsMulti) sqlBuidler.Append(" WHERE ");
            else blockBodies.Add(Expression.Call(sqlBuilderExpr, appendMethodInfo, Expression.Constant(" WHERE ", typeof(string))));

            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || !propMapper.IsKey) continue;

                if (!updateObjMapper.TryGetMemberMap(propMapper.MemberName, out var updateObjPropMapper))
                    throw new Exception($"更新参数中不包含主键栏位的实体成员{updateObjPropMapper.MemberName}");

                if (index > 0)
                {
                    if (!updateObj.IsMulti) sqlBuidler.Append(" AND ");
                    else blockBodies.Add(Expression.Call(sqlBuilderExpr, appendMethodInfo, Expression.Constant(" AND ", typeof(string))));
                }

                //@propName
                var parameterName = $"{connection.OrmProvider.ParameterPrefix}{propMapper.MemberName}{updateObj.MulitIndex}";
                Expression parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                //@propName1,2,3
                if (updateObj.IsMulti)
                    parameterNameExpr = Expression.Call(concatMethodInfo, parameterNameExpr, suffixExpr);

                //属性是数组 hasDeferred=true
                if (propMapper.MemberType != updateObjPropMapper.MemberType
                    && typeof(IEnumerable).IsAssignableFrom(updateObjPropMapper.MemberType)
                    && updateObjPropMapper.MemberType != typeof(string))
                {
                    //有数组参数，将不再设置sqlBuilder变量
                    if (connection.OrmProvider.IsSupportArrayParameter)
                    {
                        //field=ANY(@propName)
                        if (updateObj.IsMulti)
                        {
                            blockBodies.Add(Expression.Call(sqlBuilderExpr, appendMethodInfo,
                                Expression.Constant(connection.OrmProvider.GetFieldName(propMapper.FieldName) + " = ANY(", typeof(string))));
                            blockBodies.Add(Expression.Call(sqlBuilderExpr, appendMethodInfo, parameterNameExpr));
                            blockBodies.Add(Expression.Call(sqlBuilderExpr, appendMethodInfo, Expression.Constant(")", typeof(string))));
                        }
                        else
                        {
                            sqlBuidler.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} = ANY(");
                            sqlBuidler.Append(parameterName + ")");
                        }
                        AddParameter(connection, commandExpr, parameterNameExpr, typedUpdateObjExpr, updateObjPropMapper, propMapper, updateObj, blockParameters, blockBodies);
                    }
                    else
                    {
                        //field IN (@prop1,@prop2,@prop3,...)
                        if (updateObj.IsMulti)
                        {
                            blockBodies.Add(Expression.Call(sqlBuilderExpr, appendMethodInfo,
                              Expression.Constant(connection.OrmProvider.GetFieldName(propMapper.FieldName) + " IN ", typeof(string))));

                            BuildWhereInSqlParameters(connection, commandExpr, typedUpdateObjExpr, sqlBuilderExpr, parameterName, updateObjPropMapper, blockBodies);
                        }
                        else
                        {
                            sqlBuidler.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} IN ");
                            BuildWhereInSqlParameters(connection, commandExpr, typedUpdateObjExpr, deferredExpr, parameterName, updateObjPropMapper, sqlBuidler.Length, blockBodies);
                        }
                    }
                }
                else
                {
                    //属性不是数组，单体 hasDeferred=false
                    //field = @propName
                    if (updateObj.IsMulti)
                    {
                        blockBodies.Add(Expression.Call(sqlBuilderExpr, appendMethodInfo,
                            Expression.Constant(connection.OrmProvider.GetFieldName(propMapper.FieldName) + "=", typeof(string))));
                        blockBodies.Add(Expression.Call(sqlBuilderExpr, appendMethodInfo, parameterNameExpr));
                    }
                    else sqlBuidler.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                    AddParameter(connection, commandExpr, parameterNameExpr, typedUpdateObjExpr, updateObjPropMapper, propMapper, updateObj, blockParameters, blockBodies);
                }
                index++;
            }
            if (hasDeferred)
            {
                var returnLabel = Expression.Label(typeof(Stack<Action<StringBuilder>>));
                blockBodies.Add(Expression.Return(returnLabel, deferredExpr));

                var converExpr = Expression.Convert(Expression.Constant(null), typeof(Stack<Action<StringBuilder>>));
                blockBodies.Add(Expression.Label(returnLabel, converExpr));
            }

            if (updateObj.IsMulti)
            {
                if (hasDeferred)
                {
                    var initializer = Expression.Lambda<Func<IDbCommand, StringBuilder, string, object, Stack<Action<StringBuilder>>>>(
                        Expression.Block(blockParameters, blockBodies), commandExpr, sqlBuilderExpr, suffixExpr, objExpr).Compile();

                    commandInitializer = (command, parameters) =>
                    {
                        var enumerable = parameters[0] as IEnumerable;
                        var resultBuilder = new StringBuilder();
                        var sqlItemBuidler = new StringBuilder();
                        if (!string.IsNullOrEmpty(command.CommandText))
                            resultBuilder.Append(command.CommandText + ";");

                        var index = 1;
                        foreach (var item in enumerable)
                        {
                            var stack = initializer(command, sqlItemBuidler, index.ToString(), item);
                            while (stack.TryPop(out var deferred))
                            {
                                deferred(sqlItemBuidler);
                            }
                            resultBuilder.Append(sqlItemBuidler);
                            resultBuilder.Append(";");
                            sqlItemBuidler.Clear();
                            index++;
                        }
                        command.CommandText = resultBuilder.ToString();
                        command.CommandType = CommandType.Text;
                    };
                }
                else
                {
                    var initializer = Expression.Lambda<Action<IDbCommand, StringBuilder, string, object>>(
                        Expression.Block(blockParameters, blockBodies), commandExpr, sqlBuilderExpr, suffixExpr, objExpr).Compile();

                    commandInitializer = (command, parameters) =>
                    {
                        var enumerable = parameters[0] as IEnumerable;
                        var resultBuilder = new StringBuilder();
                        var sqlItemBuidler = new StringBuilder();
                        if (!string.IsNullOrEmpty(command.CommandText))
                            resultBuilder.Append(command.CommandText + ";");

                        var index = 1;
                        foreach (var item in enumerable)
                        {
                            initializer(command, sqlItemBuidler, index.ToString(), item);
                            resultBuilder.Append(sqlItemBuidler);
                            resultBuilder.Append(";");
                            sqlItemBuidler.Clear();
                            index++;
                        }
                        command.CommandText = resultBuilder.ToString();
                        command.CommandType = CommandType.Text;
                    };
                }
            }
            else
            {
                var updateSql = sqlBuidler.ToString();
                if (hasDeferred)
                {
                    var initializer = Expression.Lambda<Func<IDbCommand, object, Stack<Action<StringBuilder>>>>(
                        Expression.Block(blockParameters, blockBodies), commandExpr, objExpr).Compile();

                    commandInitializer = (command, parameters) =>
                    {
                        var resultBuilder = new StringBuilder();
                        if (!string.IsNullOrEmpty(command.CommandText))
                            resultBuilder.Append(command.CommandText + ";");
                        resultBuilder.Append(updateSql);
                        var stack = initializer(command, parameters[0]);
                        while (stack.TryPop(out var deferred))
                        {
                            deferred(resultBuilder);
                        }
                        command.CommandText = resultBuilder.ToString();
                        command.CommandType = CommandType.Text;
                    };
                }
                else
                {

                    var initializer = Expression.Lambda<Action<IDbCommand, object>>(
                        Expression.Block(blockParameters, blockBodies), commandExpr, objExpr).Compile();

                    commandInitializer = (command, parameters) =>
                    {
                        initializer(command, parameters[0]);
                        var sql = updateSql;
                        if (!string.IsNullOrEmpty(command.CommandText))
                            sql = command.CommandText + ";" + updateSql;
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;
                    };
                }
            }
            typedInitializerCache.TryAdd(cacheKey, commandInitializer);
        }
        return commandInitializer;
    }
    public static CommandInitializer BuildUpdateCache(IOrmDbFactory dbFactory, TheaConnection connection, Type entityType, ParameterInfo updateObj, ParameterInfo whereObj)
    {
        CommandInitializer commandInitializer = null;
        if (updateObj.IsDictionary || whereObj.IsDictionary) throw new Exception("暂时参数不支持字典类型");

        var cacheKey = GetTypedSqlParameterKey(connection, "BuildUpdateCache", whereObj.IsMulti, entityType, updateObj.ParameterType, whereObj.ParameterType);
        if (!typedInitializerCache.TryGetValue(cacheKey, out commandInitializer))
        {
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var updateObjMapper = dbFactory.GetEntityMap(updateObj.ParameterType);
            var whereObjMapper = dbFactory.GetEntityMap(whereObj.ParameterType);

            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var updateObjExpr = Expression.Parameter(typeof(object), "updateObj");
            var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

            var typedUpdateObjExpr = Expression.Variable(updateObj.ParameterType, "typedUpdateObj");
            var typedWhereObjExpr = Expression.Variable(whereObj.ParameterType, "typedWhereObj");
            blockParameters.AddRange(new[] { typedUpdateObjExpr, typedWhereObjExpr });
            blockBodies.Add(Expression.Assign(typedUpdateObjExpr, Expression.Convert(updateObjExpr, updateObj.ParameterType)));
            blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObj.ParameterType)));

            ParameterExpression deferredExpr = null;
            bool hasDeferred = false;
            foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore)
                    continue;

                if (propMapper.MemberType != whereObjPropMapper.MemberType
                    && typeof(IEnumerable).IsAssignableFrom(whereObjPropMapper.MemberType)
                    && whereObjPropMapper.MemberType != typeof(string))
                {
                    //有数组参数，将不再设置sqlBuilder变量
                    if (!connection.OrmProvider.IsSupportArrayParameter)
                    {
                        hasDeferred = true;
                        break;
                    }
                }
            }
            if (hasDeferred)
            {
                deferredExpr = Expression.Variable(typeof(Stack<Action<StringBuilder>>), "deferredStack");
                blockParameters.Add(deferredExpr);
                var newExp = Expression.New(typeof(Stack<Action<StringBuilder>>).GetConstructor(Type.EmptyTypes));
                blockBodies.Add(Expression.Assign(deferredExpr, newExp));
            }

            int index = 0;
            string parameterName = null;
            var sqlBuilder = new StringBuilder($"UPDATE {connection.OrmProvider.GetTableName(entityMapper.TableName)} SET ");

            foreach (var updateObjPropMapper in updateObjMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(updateObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore)
                    continue;

                if (index > 0) sqlBuilder.Append(",");
                parameterName = $"{connection.OrmProvider.ParameterPrefix}{propMapper.MemberName}{updateObj.MulitIndex}";
                sqlBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} = {parameterName}");

                AddParameter(connection, commandExpr, parameterName, typedUpdateObjExpr, updateObjPropMapper, propMapper, updateObj, blockParameters, blockBodies);
                index++;
            }

            index = 0;
            sqlBuilder.Append(" WHERE ");
            foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore)
                    continue;

                if (index > 0) sqlBuilder.Append(" AND ");
                parameterName = $"{connection.OrmProvider.ParameterPrefix}p{propMapper.MemberName}{whereObj.MulitIndex}";

                if (propMapper.MemberType != whereObjPropMapper.MemberType
                    && typeof(IEnumerable).IsAssignableFrom(whereObjPropMapper.MemberType)
                    && whereObjPropMapper.MemberType != typeof(string))
                {
                    if (connection.OrmProvider.IsSupportArrayParameter)
                    {
                        //field=ANY(@propName)
                        sqlBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} = ANY(");
                        sqlBuilder.Append($"{parameterName})");
                        AddParameter(connection, commandExpr, parameterName, typedWhereObjExpr, whereObjPropMapper, propMapper, whereObj, blockParameters, blockBodies);
                    }
                    else
                    {
                        //field IN (@prop1,@prop2,@prop3,...)
                        sqlBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} IN ");
                        BuildWhereInSqlParameters(connection, commandExpr, typedWhereObjExpr, deferredExpr, parameterName, whereObjPropMapper, sqlBuilder.Length, blockBodies);
                    }
                }
                else
                {
                    //field = @propName
                    sqlBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} = {parameterName}");
                    AddParameter(connection, commandExpr, parameterName, typedWhereObjExpr, whereObjPropMapper, propMapper, whereObj, blockParameters, blockBodies);
                }
                index++;
            }
            if (index == 0) throw new Exception($"当前更新语句缺少where条件,SQL:{sqlBuilder}");

            var updateSql = sqlBuilder.ToString();
            if (hasDeferred)
            {
                var returnLabel = Expression.Label(typeof(Stack<Action<StringBuilder>>));
                if (hasDeferred) blockBodies.Add(Expression.Return(returnLabel, deferredExpr));

                var converExpr = Expression.Convert(Expression.Constant(null), typeof(Stack<Action<StringBuilder>>));
                blockBodies.Add(Expression.Label(returnLabel, converExpr));

                var initializer = Expression.Lambda<Func<IDbCommand, object, object, Stack<Action<StringBuilder>>>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, updateObjExpr, whereObjExpr).Compile();

                commandInitializer = (command, parameters) =>
                {
                    var resultBuilder = new StringBuilder();
                    if (!string.IsNullOrEmpty(command.CommandText))
                        resultBuilder.Append(command.CommandText + ";");
                    resultBuilder.Append(updateSql);

                    var stack = initializer(command, parameters[0], parameters[1]);
                    while (stack.TryPop(out var deferred))
                    {
                        deferred(resultBuilder);
                    }
                    command.CommandText = resultBuilder.ToString();
                    command.CommandType = CommandType.Text;
                };
            }
            else
            {
                var initializer = Expression.Lambda<Action<IDbCommand, object, object>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, updateObjExpr, whereObjExpr).Compile();

                commandInitializer = (command, parameters) =>
                {
                    initializer(command, parameters[0], parameters[1]);
                    var sql = updateSql;
                    if (!string.IsNullOrEmpty(command.CommandText))
                        sql = command.CommandText + ";" + updateSql;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                };
            }
            typedInitializerCache.TryAdd(cacheKey, commandInitializer);
        }
        return commandInitializer;
    }
    public static CommandInitializer BuildDeleteCache(IOrmDbFactory dbFactory, TheaConnection connection, Type entityType, ParameterInfo whereObj)
    {
        CommandInitializer commandInitializer = null;
        if (whereObj.IsDictionary)
        {
            commandInitializer = (command, parameters) =>
            {
                var dict = parameters[0] as Dictionary<string, object>;
                var entityMapper = dbFactory.GetEntityMap(entityType);

                int index = 0;
                var sqlBuilder = new StringBuilder();
                if (!string.IsNullOrEmpty(command.CommandText))
                    sqlBuilder.Append(command.CommandText + ";");
                sqlBuilder.Append($"DELETE FROM {connection.OrmProvider.GetTableName(entityMapper.TableName)} WHERE ");
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || propMapper.IsIgnore)
                        continue;

                    var parameterName = $"{connection.OrmProvider.ParameterPrefix}{item.Key}{whereObj.MulitIndex}";
                    if (index > 0) sqlBuilder.Append(" AND ");
                    sqlBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} = {parameterName}");

                    if (command.Parameters.Contains(parameterName))
                        continue;

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = parameterName;
                    parameter.Direction = ParameterDirection.Input;
                    var itemValue = dict[item.Key];
                    if (itemValue != null && itemValue is string strValue)
                        parameter.Size = strValue.Length;
                    if (itemValue == null) parameter.Value = DBNull.Value;
                    else parameter.Value = itemValue;
                    command.Parameters.Add(parameter);
                    index++;
                }
                if (index == 0) throw new Exception($"当前字典参数中不包含删除语句，SQL:{sqlBuilder}");

                command.CommandText = sqlBuilder.ToString();
                command.CommandType = CommandType.Text;
            };
            return commandInitializer;
        }

        var cacheKey = GetTypedSqlParameterKey(connection, "BuildDeleteCache", whereObj.IsMulti, entityType, null, whereObj.ParameterType);
        if (!typedInitializerCache.TryGetValue(cacheKey, out commandInitializer))
        {
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var whereObjMapper = dbFactory.GetEntityMap(whereObj.ParameterType);

            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var objExpr = Expression.Parameter(typeof(object), "obj");
            var typedWhereObjExpr = Expression.Variable(whereObj.ParameterType, "whereObj");
            blockParameters.Add(typedWhereObjExpr);
            blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(objExpr, whereObj.ParameterType)));

            ParameterExpression deferredExpr = null;
            bool hasDeferred = false;
            foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore)
                    continue;

                if (propMapper.MemberType != whereObjPropMapper.MemberType
                    && typeof(IEnumerable).IsAssignableFrom(whereObjPropMapper.MemberType)
                    && whereObjPropMapper.MemberType != typeof(string))
                {
                    //有数组参数，将不再设置sqlBuilder变量
                    if (!connection.OrmProvider.IsSupportArrayParameter)
                    {
                        hasDeferred = true;
                        break;
                    }
                }
            }
            if (hasDeferred)
            {
                deferredExpr = Expression.Variable(typeof(Stack<Action<StringBuilder>>), "deferredStack");
                blockParameters.Add(deferredExpr);
                var newExp = Expression.New(typeof(Stack<Action<StringBuilder>>).GetConstructor(Type.EmptyTypes));
                blockBodies.Add(Expression.Assign(deferredExpr, newExp));
            }

            int index = 0;
            var sqlBuilder = new StringBuilder($"DELETE FROM {connection.OrmProvider.GetTableName(entityMapper.TableName)} WHERE ");
            foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore)
                    continue;

                if (index > 0) sqlBuilder.Append(" AND ");
                var parameterName = $"{connection.OrmProvider.ParameterPrefix}{whereObjPropMapper.MemberName}{whereObj.MulitIndex}";

                if (propMapper.MemberType != whereObjPropMapper.MemberType
                    && typeof(IEnumerable).IsAssignableFrom(whereObjPropMapper.MemberType)
                    && whereObjPropMapper.MemberType != typeof(string))
                {
                    if (connection.OrmProvider.IsSupportArrayParameter)
                    {
                        //field=ANY(@propName)
                        sqlBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} = ANY(");
                        sqlBuilder.Append($"{parameterName})");
                        AddParameter(connection, commandExpr, parameterName, typedWhereObjExpr, whereObjPropMapper, propMapper, whereObj, blockParameters, blockBodies);
                    }
                    else
                    {
                        if (!hasDeferred)
                        {
                            blockParameters.Add(deferredExpr);
                            var newExp = Expression.New(typeof(Stack<Action<StringBuilder>>).GetConstructor(Type.EmptyTypes));
                            blockBodies.Add(Expression.Assign(deferredExpr, newExp));
                            hasDeferred = true;
                        }
                        //field IN (@prop1,@prop2,@prop3,...)
                        sqlBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} IN ");
                        BuildWhereInSqlParameters(connection, commandExpr, typedWhereObjExpr, deferredExpr, parameterName, whereObjPropMapper, sqlBuilder.Length, blockBodies);
                    }
                }
                else
                {
                    //field = @propName
                    sqlBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} = {parameterName}");
                    AddParameter(connection, commandExpr, parameterName, typedWhereObjExpr, whereObjPropMapper, propMapper, whereObj, blockParameters, blockBodies);
                }
                index++;
            }
            if (index == 0) throw new Exception($"当前删除语句缺少where条件,SQL:{sqlBuilder}");

            var deleteSql = sqlBuilder.ToString();
            if (hasDeferred)
            {
                var returnLabel = Expression.Label(typeof(Stack<Action<StringBuilder>>));
                if (hasDeferred) blockBodies.Add(Expression.Return(returnLabel, deferredExpr));

                var converExpr = Expression.Convert(Expression.Constant(null), typeof(Stack<Action<StringBuilder>>));
                blockBodies.Add(Expression.Label(returnLabel, converExpr));

                var initializer = Expression.Lambda<Func<IDbCommand, object, Stack<Action<StringBuilder>>>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, objExpr).Compile();

                commandInitializer = (command, parameters) =>
                {
                    var resultBuilder = new StringBuilder();
                    if (!string.IsNullOrEmpty(command.CommandText))
                        resultBuilder.Append(command.CommandText + ";");
                    resultBuilder.Append(deleteSql);
                    var stack = initializer(command, parameters[0]);
                    while (stack.TryPop(out var deferred))
                    {
                        deferred(resultBuilder);
                    }

                    command.CommandText = resultBuilder.ToString();
                    command.CommandType = CommandType.Text;
                };
            }
            else
            {
                var initializer = Expression.Lambda<Action<IDbCommand, object>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, objExpr).Compile();

                commandInitializer = (command, parameters) =>
                {
                    initializer(command, parameters[0]);
                    var sql = deleteSql;
                    if (!string.IsNullOrEmpty(command.CommandText))
                        sql = command.CommandText + ";" + deleteSql;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                };
            }
            typedInitializerCache.TryAdd(cacheKey, commandInitializer);
        }
        return commandInitializer;
    }
    public static CommandInitializer BuildExistsCache(IOrmDbFactory dbFactory, TheaConnection connection, Type entityType, ParameterInfo whereObj)
    {
        CommandInitializer commandInitializer = null;
        if (whereObj.IsDictionary)
        {
            commandInitializer = (command, parameters) =>
            {
                var dict = parameters[0] as Dictionary<string, object>;
                var entityMapper = dbFactory.GetEntityMap(entityType);

                int index = 0;
                var sqlBuilder = new StringBuilder();
                if (!string.IsNullOrEmpty(command.CommandText))
                    sqlBuilder.Append(command.CommandText + ";");
                sqlBuilder.Append($"SELECT COUNT(*) FROM {connection.OrmProvider.GetTableName(entityMapper.TableName)} WHERE ");
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || propMapper.IsIgnore)
                        continue;

                    var parameterName = $"{connection.OrmProvider.ParameterPrefix}{item.Key}{whereObj.MulitIndex}";
                    if (index > 0) sqlBuilder.Append(" AND ");
                    sqlBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} = {parameterName}");

                    if (command.Parameters.Contains(parameterName))
                        continue;

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = parameterName;
                    parameter.Direction = ParameterDirection.Input;
                    var itemValue = dict[item.Key];
                    if (itemValue != null && itemValue is string strValue)
                        parameter.Size = strValue.Length;
                    if (itemValue == null) parameter.Value = DBNull.Value;
                    else parameter.Value = itemValue;
                    command.Parameters.Add(parameter);
                    index++;
                }
                //if (index == 0) throw new Exception($"当前查询语句缺少where条件，SQL:{sqlBuilder}");

                command.CommandText = sqlBuilder.ToString();
                command.CommandType = CommandType.Text;
            };
            return commandInitializer;
        }

        var cacheKey = GetTypedSqlParameterKey(connection, "BuildExistsCache", whereObj.IsMulti, entityType, null, whereObj.ParameterType);
        if (!typedInitializerCache.TryGetValue(cacheKey, out commandInitializer))
        {
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var whereObjMapper = dbFactory.GetEntityMap(whereObj.ParameterType);

            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var objExpr = Expression.Parameter(typeof(object), "obj");
            var typedWhereObjExpr = Expression.Variable(whereObj.ParameterType, "whereObj");
            blockParameters.Add(typedWhereObjExpr);
            blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(objExpr, whereObj.ParameterType)));

            ParameterExpression deferredExpr = null;
            bool hasDeferred = false;
            foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore)
                    continue;

                if (propMapper.MemberType != whereObjPropMapper.MemberType
                    && typeof(IEnumerable).IsAssignableFrom(whereObjPropMapper.MemberType)
                    && whereObjPropMapper.MemberType != typeof(string))
                {
                    //有数组参数，将不再设置sqlBuilder变量
                    if (!connection.OrmProvider.IsSupportArrayParameter)
                    {
                        hasDeferred = true;
                        break;
                    }
                }
            }
            if (hasDeferred)
            {
                deferredExpr = Expression.Variable(typeof(Stack<Action<StringBuilder>>), "deferredStack");
                blockParameters.Add(deferredExpr);
                var newExp = Expression.New(typeof(Stack<Action<StringBuilder>>).GetConstructor(Type.EmptyTypes));
                blockBodies.Add(Expression.Assign(deferredExpr, newExp));
            }

            int index = 0;
            var sqlBuilder = new StringBuilder($"SELECT COUNT(*) FROM {connection.OrmProvider.GetTableName(entityMapper.TableName)} WHERE ");
            foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore)
                    continue;

                if (index > 0) sqlBuilder.Append(" AND ");
                var parameterName = $"{connection.OrmProvider.ParameterPrefix}{whereObjPropMapper.MemberName}{whereObj.MulitIndex}";

                if (propMapper.MemberType != whereObjPropMapper.MemberType
                    && typeof(IEnumerable).IsAssignableFrom(whereObjPropMapper.MemberType)
                    && whereObjPropMapper.MemberType != typeof(string))
                {
                    if (connection.OrmProvider.IsSupportArrayParameter)
                    {
                        //field=ANY(@propName)
                        sqlBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} = ANY(");
                        sqlBuilder.Append($"{parameterName})");
                        AddParameter(connection, commandExpr, parameterName, typedWhereObjExpr, whereObjPropMapper, propMapper, whereObj, blockParameters, blockBodies);
                    }
                    else
                    {
                        if (!hasDeferred)
                        {
                            blockParameters.Add(deferredExpr);
                            var newExp = Expression.New(typeof(Stack<Action<StringBuilder>>).GetConstructor(Type.EmptyTypes));
                            blockBodies.Add(Expression.Assign(deferredExpr, newExp));
                            hasDeferred = true;
                        }
                        //field IN (@prop1,@prop2,@prop3,...)
                        sqlBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} IN ");
                        BuildWhereInSqlParameters(connection, commandExpr, typedWhereObjExpr, deferredExpr, parameterName, whereObjPropMapper, sqlBuilder.Length, blockBodies);
                    }
                }
                else
                {
                    //field = @propName
                    sqlBuilder.Append($"{connection.OrmProvider.GetFieldName(propMapper.FieldName)} = {parameterName}");
                    AddParameter(connection, commandExpr, parameterName, typedWhereObjExpr, whereObjPropMapper, propMapper, whereObj, blockParameters, blockBodies);
                }
                index++;
            }
            //if (index == 0) throw new Exception($"当前查询语句缺少where条件,SQL:{sqlBuilder}");

            var existsSql = sqlBuilder.ToString();
            if (hasDeferred)
            {
                var returnLabel = Expression.Label(typeof(Stack<Action<StringBuilder>>));
                if (hasDeferred) blockBodies.Add(Expression.Return(returnLabel, deferredExpr));

                var converExpr = Expression.Convert(Expression.Constant(null), typeof(Stack<Action<StringBuilder>>));
                blockBodies.Add(Expression.Label(returnLabel, converExpr));

                var initializer = Expression.Lambda<Func<IDbCommand, object, Stack<Action<StringBuilder>>>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, objExpr).Compile();

                commandInitializer = (command, parameters) =>
                {
                    var resultBuilder = new StringBuilder();
                    if (!string.IsNullOrEmpty(command.CommandText))
                        resultBuilder.Append(command.CommandText + ";");
                    sqlBuilder.Append(existsSql);

                    var stack = initializer(command, parameters[0]);
                    while (stack.TryPop(out var deferred))
                    {
                        deferred(resultBuilder);
                    }

                    command.CommandText = resultBuilder.ToString();
                    command.CommandType = CommandType.Text;
                };
            }
            else
            {
                var initializer = Expression.Lambda<Action<IDbCommand, object>>(
                    Expression.Block(blockParameters, blockBodies), commandExpr, objExpr).Compile();

                commandInitializer = (command, parameters) =>
                {
                    initializer(command, parameters[0]);
                    var sql = existsSql;
                    if (!string.IsNullOrEmpty(command.CommandText))
                        sql = command.CommandText + ";" + existsSql;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                };
            }
            typedInitializerCache.TryAdd(cacheKey, commandInitializer);
        }
        return commandInitializer;
    }
    public static Func<IDataReader, object> GetReader(bool isTyped, IOrmDbFactory dbFactory, TheaConnection connection, IDataReader reader, Type entityType, Type targetType)
    {
        Type type = targetType;
        Type underlyingType = null;
        if (targetType == typeof(object) || targetType == typeof(TheaRow))
            return GetTheaRowReader(reader);
        else if (targetType.IsValueType && (targetType.FullName.StartsWith("System.ValueTuple`")
            || ((underlyingType = Nullable.GetUnderlyingType(type)) != null && underlyingType.FullName.StartsWith("System.ValueTuple`"))))
            return GetValueTupleReader(connection, reader, targetType);
        else if (!(DbTypeMap.ContainsKey(type) || type.IsEnum || type.IsArray || type.FullName == DbTypeMap.LinqBinary
            || (type.IsValueType && (underlyingType ?? (underlyingType = Nullable.GetUnderlyingType(type))) != null && underlyingType.IsEnum)))
            return GetEntityReader(isTyped, dbFactory, connection, reader, entityType, targetType);
        else return GetStructDeserializer(type, underlyingType ?? Nullable.GetUnderlyingType(type) ?? type);
    }

    public static int GetTypedSqlKey(IDbConnection connection, Type entityType, Type targetType)
    {
        unchecked
        {
            var hashCode = 17;
            hashCode = hashCode * 23 + connection.GetHashCode();
            hashCode = hashCode * 23 + entityType.GetHashCode();
            hashCode = hashCode * 23 + targetType.GetHashCode();
            return hashCode;
        }
    }
    public static int GetTypedSqlParameterKey(IDbConnection connection, string sqlType, bool isMulti, Type entityType, Type targetType, Type whereType)
    {
        unchecked
        {
            var hashCode = 17;
            hashCode = hashCode * 23 + connection.GetHashCode();
            hashCode = hashCode * 23 + sqlType.GetHashCode();
            hashCode = hashCode * 23 + isMulti.GetHashCode();
            hashCode = hashCode * 23 + entityType.GetHashCode();
            hashCode = hashCode * 23;
            if (targetType != null) hashCode += targetType.GetHashCode();
            hashCode = hashCode * 23;
            if (whereType != null) hashCode += whereType.GetHashCode();
            return hashCode;
        }
    }
    public static int GetSqlParameterKey(IDbConnection connection, CommandType cmdType, bool isMulti, Type whereType)
    {
        unchecked
        {
            var hashCode = 17;
            hashCode = hashCode * 23 + connection.GetHashCode();
            hashCode = hashCode * 23 + cmdType.GetHashCode();
            hashCode = hashCode * 23 + isMulti.GetHashCode();
            hashCode = hashCode * 23 + whereType.GetHashCode();
            return hashCode;
        }
    }
    public static int GetTypedReaderKey(IDbConnection connection, Type entityType, Type targetType)
    {
        unchecked
        {
            var hashCode = 17;
            hashCode = hashCode * 23 + connection.GetHashCode();
            hashCode = hashCode * 23 + entityType.GetHashCode();
            hashCode = hashCode * 23 + targetType.GetHashCode();
            return hashCode;
        }
    }
    public static int GetSqlReaderKey(IDbConnection connection, IDataReader reader, Type entityType, Type targetType)
    {
        unchecked
        {
            int hashCode = reader.FieldCount;
            hashCode = hashCode * -37 + connection.GetHashCode();
            hashCode = hashCode * -37 + entityType.GetHashCode();
            hashCode = hashCode * -37 + targetType.GetHashCode();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                object fieldName = reader.GetName(i);
                hashCode = (-79 * ((hashCode * 31) + (fieldName?.GetHashCode() ?? 0))) + (reader.GetFieldType(i)?.GetHashCode() ?? 0);
            }
            return hashCode;
        }
    }
    public static int GetReaderKey(IDbConnection connection, string sqlType, Type entityType, Type targetType)
    {
        unchecked
        {
            var hashCode = 17;
            hashCode = hashCode * 23 + connection.GetHashCode();
            hashCode = hashCode * 23 + entityType.GetHashCode();
            hashCode = hashCode * 23 + sqlType.GetHashCode();
            hashCode = hashCode * 23 + targetType.GetHashCode();
            return hashCode;
        }
    }
    public static int GetReaderKey(IDbConnection connection, string sqlType, IDataReader reader, Type targetType)
    {
        unchecked
        {
            int hashCode = reader.FieldCount;
            hashCode = hashCode * -37 + connection.GetHashCode();
            hashCode = hashCode * -37 + sqlType.GetHashCode();
            hashCode = hashCode * -37 + targetType.GetHashCode();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                object fieldName = reader.GetName(i);
                hashCode = (-79 * ((hashCode * 31) + (fieldName?.GetHashCode() ?? 0))) + (reader.GetFieldType(i)?.GetHashCode() ?? 0);
            }
            return hashCode;
        }
    }
    public static ParameterInfo CreateParameterInfo(object parameters)
    {
        if (parameters is Dictionary<string, object>)
        {
            return new ParameterInfo
            {
                IsDictionary = true,
                IsMulti = false,
                ParameterType = parameters.GetType(),
                Parameters = parameters
            };
        }
        if (parameters is IEnumerable entities)
        {
            foreach (var entity in entities)
            {
                if (entity is Dictionary<string, object>)
                {
                    return new ParameterInfo
                    {
                        IsDictionary = true,
                        IsMulti = true,
                        ParameterType = entity.GetType(),
                        Parameters = parameters
                    };
                }
                return new ParameterInfo
                {
                    IsDictionary = false,
                    IsMulti = true,
                    ParameterType = entity.GetType(),
                    Parameters = parameters
                };
            }
        }
        return new ParameterInfo
        {
            IsDictionary = false,
            IsMulti = false,
            ParameterType = parameters.GetType(),
            Parameters = parameters
        };
    }
    private static (string InsertSql, string ValuesSql) BuildInsertSql(TheaConnection connection, EntityMap entityMapper, EntityMap insertObjMapper)
    {
        var insertBuilder = new StringBuilder("INSERT INTO " + connection.OrmProvider.GetTableName(entityMapper.TableName) + " (");
        var valuesBuilder = new StringBuilder("VALUES(");
        int index = 0;
        foreach (var insertObjPropMapper in insertObjMapper.MemberMaps)
        {
            if (!entityMapper.TryGetMemberMap(insertObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore || propMapper.IsAutoIncrement)
                continue;

            if (index > 0)
            {
                insertBuilder.Append(",");
                valuesBuilder.Append(",");
            }
            insertBuilder.Append(connection.OrmProvider.GetFieldName(propMapper.FieldName));
            valuesBuilder.Append($"{connection.OrmProvider.ParameterPrefix}{propMapper.MemberName}");
            index++;
        }
        insertBuilder.Append(")");
        valuesBuilder.Append(")");
        return (insertBuilder.ToString(), valuesBuilder.ToString());
    }
    private static Func<IDataReader, object> GetTheaRowReader(IDataReader reader)
    {
        var fieldCount = reader.FieldCount;
        TheaTable table = null;
        return reader =>
        {
            if (table == null)
            {
                string[] names = new string[fieldCount];
                for (int i = 0; i < fieldCount; i++)
                {
                    names[i] = reader.GetName(i);
                }
                table = new TheaTable(names);
            }
            var values = new object[fieldCount];
            for (int i = 0; i < values.Length; i++)
            {
                object readerValue = reader.GetValue(i);
                values[i] = readerValue is DBNull ? null : readerValue;
            }
            return new TheaRow(table, values);
        };
    }
    private static Func<IDataReader, object> GetStructDeserializer(Type type, Type effectiveType)
    {
        if (type == typeof(char))
        {
            return reader =>
            {
                var readerValue = reader.GetValue(0);
                if (readerValue is string s && s.Length == 1) return s[0];
                if (readerValue is char c) return c;
                throw new ArgumentNullException("readerValue", "数据为null,不能转换为char类型");
            };
        }
        if (type == typeof(char?))
        {
            return reader =>
            {
                var readerValue = reader.GetValue(0);
                if (readerValue is string s && s.Length == 1) return s[0];
                if (readerValue is char c) return c;
                return null;
            };
        }
        if (type.FullName == DbTypeMap.LinqBinary)
            return reader => Activator.CreateInstance(type, reader.GetValue(0));
        if (type == typeof(Guid))
        {
            return reader =>
            {
                var readerValue = reader.GetValue(0);
                if (readerValue is string strValue) return new Guid(strValue);
                if (readerValue is byte[] bValue) return new Guid(bValue);
                return Guid.Empty;
            };
        }
        if (type == typeof(Guid?))
        {
            return reader =>
            {
                var readerValue = reader.GetValue(0);
                if (readerValue is string strValue) return new Guid(strValue);
                if (readerValue is byte[] bValue) return new Guid(bValue);
                return null;
            };
        }
        if (effectiveType.IsEnum)
        {
            return reader =>
            {
                var readerValue = reader.GetValue(0);
                if (readerValue is float || readerValue is double || readerValue is decimal)
                {
                    readerValue = Convert.ChangeType(readerValue, Enum.GetUnderlyingType(effectiveType), CultureInfo.InvariantCulture);
                }
                return readerValue is DBNull ? null : Enum.ToObject(effectiveType, readerValue);
            };
        }
        return reader =>
        {
            var readerValue = reader.GetValue(0);
            return readerValue is DBNull ? null : readerValue;
        };
    }
    private static Func<IDataReader, object> GetValueTupleReader(TheaConnection connection, IDataReader reader, Type targetType)
    {
        int cacheKey = RepositoryHelper.GetReaderKey(connection, "GetValueTupleReader", reader, targetType);
        if (!valueTupleReaderCache.TryGetValue(cacheKey, out var readerFunc))
        {
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            var valuesExprs = new List<Expression>();

            var readerExpr = Expression.Parameter(typeof(IDataReader), "reader");
            var resultLabelExpr = Expression.Label(typeof(object), "result");
            var isNullable = true;
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType == null)
            {
                underlyingType = targetType;
                isNullable = false;
            }
            var fields = underlyingType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            for (int index = 0; index < reader.FieldCount; index++)
            {
                //达到元组个数后，停止遍历
                if (index >= fields.Length) break;
                var fieldName = reader.GetName(index);
                AddReaderParameter(reader, readerExpr, index, fieldName, fields[index].FieldType, blockParameters, blockBodies, valuesExprs);
            }
            //var returnValue=new (reader[0],reader[1],...);               
            var returnValueExpr = NewValueTupleExpression(targetType, valuesExprs);
            if (isNullable) returnValueExpr = Expression.Convert(returnValueExpr, targetType);
            blockBodies.Add(Expression.Return(resultLabelExpr, returnValueExpr));

            Expression defaultValueExpr = null;
            if (isNullable) defaultValueExpr = Expression.Convert(Expression.Constant(null), targetType);
            else defaultValueExpr = Expression.Default(targetType);
            blockBodies.Add(Expression.Label(resultLabelExpr, defaultValueExpr));

            readerFunc = Expression.Lambda<Func<IDataReader, object>>(Expression.Block(blockBodies), readerExpr).Compile();
            valueTupleReaderCache.TryAdd(cacheKey, readerFunc);
        }
        return readerFunc;
    }
    private static Func<IDataReader, object> GetEntityReader(bool isTyped, IOrmDbFactory dbFactory, TheaConnection connection, IDataReader reader, Type entityType, Type targetType)
    {
        int cacheKey;
        ConcurrentDictionary<int, Func<IDataReader, object>> readerCache = null;
        if (isTyped)
        {
            cacheKey = RepositoryHelper.GetTypedReaderKey(connection, entityType, targetType);
            readerCache = typedReaderCache;
        }
        else
        {
            cacheKey = RepositoryHelper.GetSqlReaderKey(connection, reader, entityType, targetType);
            readerCache = sqlReaderCache;
        }
        if (!readerCache.TryGetValue(cacheKey, out var readerFunc))
        {
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var targetMapper = dbFactory.GetEntityMap(targetType);

            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            var readerExpr = Expression.Parameter(typeof(IDataReader), "reader");
            var targetExpr = Expression.Variable(targetType, "target");
            var resultLabelExpr = Expression.Label(typeof(object));
            blockParameters.Add(targetExpr);
            var localValueExpr = Expression.Variable(typeof(object), "localValue");
            blockBodies.Add(Expression.Assign(targetExpr, NewEntityExpression(targetType)));
            bool isDefinedLocal = false;

            for (int index = 0; index < reader.FieldCount; index++)
            {
                var fieldName = reader.GetName(index);
                MemberMap targetPropMapper = null;
                if (entityMapper.TryGetMemberMap(fieldName, out var propMapper))
                {
                    if (targetMapper.TryGetMemberMap(fieldName, out targetPropMapper))
                    {
                        AddReaderParameter(reader, readerExpr, targetExpr, index, fieldName, targetPropMapper, localValueExpr, ref isDefinedLocal, blockParameters, blockBodies);
                        continue;
                    }
                }
                if (targetMapper.TryGetMemberMap(fieldName, out targetPropMapper))
                    AddReaderParameter(reader, readerExpr, targetExpr, index, fieldName, targetPropMapper, localValueExpr, ref isDefinedLocal, blockParameters, blockBodies);
            }

            var returnExpr = Expression.Return(resultLabelExpr, targetExpr);

            Expression defaultValueExpr = null;
            if (entityMapper.EntityType.IsValueType && !entityMapper.IsNullable)
                defaultValueExpr = Expression.Convert(Expression.Default(targetType), typeof(object));
            else defaultValueExpr = Expression.Constant(null);

            var returnLabelExpr = Expression.Label(resultLabelExpr, defaultValueExpr);
            blockBodies.Add(returnExpr);
            blockBodies.Add(returnLabelExpr);

            readerFunc = Expression.Lambda<Func<IDataReader, object>>(Expression.Block(blockParameters, blockBodies), readerExpr).Compile();
            readerCache.TryAdd(cacheKey, readerFunc);
        }
        return readerFunc;
    }
    private static void AddParameter(TheaConnection connection, ParameterExpression commandExpr, string parameterName,
       Expression typedObjExpr, MemberMap paramMapper, MemberMap propMapper, ParameterInfo parameterInfo, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
        AddParameter(connection, commandExpr, parameterNameExpr, typedObjExpr, paramMapper, propMapper, parameterInfo, blockParameters, blockBodies);
    }
    private static void AddParameter(TheaConnection connection, ParameterExpression commandExpr, Expression parameterNameExpr,
        Expression typedObjExpr, MemberMap paramMapper, MemberMap propMapper, ParameterInfo parameterInfo, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        //TODO:
        Type dbParameterType = null;// connection.OrmProvider.NativeDbParameterType;
        var parameterExpr = Expression.Variable(dbParameterType, "parameter");
        blockParameters.Add(parameterExpr);

        //var parameter=command.CreateParameter();
        var createParameterExpr = Expression.Call(commandExpr, typeof(IDbCommand).GetMethod(nameof(IDbCommand.CreateParameter)));
        var parameterAssignExpr = Expression.Assign(parameterExpr, Expression.Convert(createParameterExpr, dbParameterType));
        blockBodies.Add(parameterAssignExpr);

        //parameter.ParameterName="@1"或"@Name";
        var methodInfo = typeof(IDataParameter).GetProperty(nameof(IDbDataParameter.ParameterName)).GetSetMethod();
        Expression setPropExpr = Expression.Call(parameterExpr, methodInfo, parameterNameExpr);
        blockBodies.Add(setPropExpr);

        //parameter.Direction = ParameterDirection.Input;
        methodInfo = typeof(IDataParameter).GetProperty(nameof(IDbDataParameter.Direction)).GetSetMethod();
        setPropExpr = Expression.Call(parameterExpr, methodInfo, Expression.Constant(ParameterDirection.Input, typeof(ParameterDirection)));
        blockBodies.Add(setPropExpr);

        var valueExpr = Expression.PropertyOrField(typedObjExpr, paramMapper.MemberName);

        if (paramMapper.MemberType == propMapper.MemberType)
        {
            //支持数据参数，获取元素类型              
            if (connection.OrmProvider.IsSupportArrayParameter
                && typeof(IEnumerable).IsAssignableFrom(paramMapper.MemberType)
                && paramMapper.MemberType != typeof(string))
            {
                Type itemType = null;
                if (paramMapper.MemberType.IsArray) itemType = paramMapper.MemberType.GetElementType();
                else
                {
                    var objValue = GetArrayItemValue(parameterInfo, paramMapper);
                    itemType = objValue.GetType();
                }
                //TODO:待测试
                var nativeDbType = connection.OrmProvider.GetNativeDbType(paramMapper.MemberType);
                var nativeDbTypeValueExpr = Expression.Constant(nativeDbType, typeof(int));
                //TODO:
                //var nativeDbTypeValue = Expression.Convert(nativeDbTypeValueExpr, connection.OrmProvider.NativeDbTypeType);
                //methodInfo = connection.OrmProvider.NativeDbTypePropertyOfDbParameter.GetSetMethod();
                //setPropExpr = Expression.Call(parameterExpr, methodInfo, nativeDbTypeValue);
            }
            else
            {
                if (propMapper.NativeDbType.HasValue)
                {
                    //parameter.NpgsqlDbType=NpgsqlDbType.Jsonb;
                    //TODO:
                    //methodInfo = connection.OrmProvider.NativeDbTypePropertyOfDbParameter.GetSetMethod();
                    //var nativeDbTypeValueExpr = Expression.Constant(propMapper.NativeDbType.Value, typeof(int));
                    //var nativeDbTypeValue = Expression.Convert(nativeDbTypeValueExpr, connection.OrmProvider.NativeDbTypeType);
                    //setPropExpr = Expression.Call(parameterExpr, methodInfo, nativeDbTypeValue);
                }
                else
                {
                    //parameter.DbType=DbType.String;
                    methodInfo = typeof(IDataParameter).GetProperty(nameof(IDbDataParameter.DbType)).GetSetMethod();
                    setPropExpr = Expression.Call(parameterExpr, methodInfo, Expression.Constant(propMapper.DbType, typeof(DbType)));
                }
            }
        }
        else
        {
            //支持数据参数，获取元素类型
            if (connection.OrmProvider.IsSupportArrayParameter
                && typeof(IEnumerable).IsAssignableFrom(paramMapper.MemberType)
                && paramMapper.MemberType != typeof(string))
            {
                Type itemType = null;
                if (paramMapper.MemberType.IsArray) itemType = paramMapper.MemberType.GetElementType();
                else
                {
                    var objValue = GetArrayItemValue(parameterInfo, paramMapper);
                    itemType = objValue.GetType();
                }
                var nativeDbType = connection.OrmProvider.GetNativeDbType(paramMapper.MemberType);
                var nativeDbTypeValueExpr = Expression.Constant(nativeDbType, typeof(int));
                //TODO:
                //var nativeDbTypeValue = Expression.Convert(nativeDbTypeValueExpr, connection.OrmProvider.NativeDbTypeType);
                //methodInfo = connection.OrmProvider.NativeDbTypePropertyOfDbParameter.GetSetMethod();
                //setPropExpr = Expression.Call(parameterExpr, methodInfo, nativeDbTypeValue);
            }
            else
            {
                //parameter.DbType=DbType.String;
                methodInfo = typeof(IDataParameter).GetProperty(nameof(IDbDataParameter.DbType)).GetSetMethod();
                var dbType = DbTypeMap.FindDbType(paramMapper.MemberType);
                setPropExpr = Expression.Call(parameterExpr, methodInfo, Expression.Constant(dbType, typeof(DbType)));
            }
        }
        blockBodies.Add(setPropExpr);

        //parameter.Size=4000;
        if (paramMapper.MemberType == typeof(string))
        {
            methodInfo = typeof(string).GetProperty(nameof(string.Length)).GetGetMethod();
            var lengthExpr = Expression.Call(valueExpr, methodInfo);
            var lessThenExpr = Expression.LessThan(lengthExpr, Expression.Constant(4000, typeof(int)));

            methodInfo = typeof(IDbDataParameter).GetProperty(nameof(IDbDataParameter.Size)).GetSetMethod();
            setPropExpr = Expression.IfThenElse(lessThenExpr,
                Expression.Call(parameterExpr, methodInfo, Expression.Constant(4000, typeof(int))),
                Expression.Call(parameterExpr, methodInfo, Expression.Constant(-1, typeof(int))));

            setPropExpr = Expression.IfThen(Expression.NotEqual(valueExpr, Expression.Constant(null)), setPropExpr);
            blockBodies.Add(setPropExpr);
        }

        //if(propValue==null)paramValue=DbNull.Value;
        //else paramValue=(int)propValue;//(int)Gender.Female
        //paramValue=propValue; 不为null

        Expression underlyingValueExpr = null;
        Expression setValueExpr = null;
        Expression setParameterValueExpr = null;
        var parameterValueExpr = Expression.Variable(typeof(object), "objValue");
        blockParameters.Add(parameterValueExpr);

        if (paramMapper.MemberType.IsValueType)
        {
            //paramValue=(int)Gender.Female/propValue
            if (paramMapper.IsEnum) underlyingValueExpr = Expression.Convert(valueExpr, paramMapper.UnderlyingType);
            else underlyingValueExpr = valueExpr;
            underlyingValueExpr = Expression.Convert(underlyingValueExpr, typeof(object));
            setValueExpr = Expression.Assign(parameterValueExpr, underlyingValueExpr);
            if (paramMapper.IsNullable)
            {
                var isNullExpr = Expression.Equal(valueExpr, Expression.Constant(null));
                var setNullExpr = Expression.Assign(parameterValueExpr, Expression.Constant(DBNull.Value, typeof(DBNull)));

                //if(propValue==null)paramValue=DbNull.Value;
                //else paramValue=(int)propValue;或(int)Gender.Female
                setParameterValueExpr = Expression.IfThenElse(isNullExpr, setNullExpr, setValueExpr);
            }
            else setParameterValueExpr = setValueExpr;
        }
        //paramValue=propValue;
        else
        {
            var isNullExpr = Expression.Equal(valueExpr, Expression.Constant(null));
            var setNullExpr = Expression.Assign(parameterValueExpr, Expression.Constant(DBNull.Value, typeof(DBNull)));
            //LinqBinary               
            if (paramMapper.MemberType.FullName == DbTypeMap.LinqBinary)
            {
                var toArrayMethodInfo = paramMapper.MemberType.GetMethod("ToArray", BindingFlags.Public | BindingFlags.Instance);
                underlyingValueExpr = Expression.Call(valueExpr, toArrayMethodInfo);
                underlyingValueExpr = Expression.Convert(underlyingValueExpr, typeof(object));
            }
            //其他类型，如:String
            else underlyingValueExpr = Expression.Convert(valueExpr, typeof(object));
            setValueExpr = Expression.Assign(parameterValueExpr, underlyingValueExpr);

            //if(propValue==null)paramValue=DbNull.Value;
            //else paramValue=byte[] propValue;
            setParameterValueExpr = Expression.IfThenElse(isNullExpr, setNullExpr, setValueExpr);
        }

        blockBodies.Add(setParameterValueExpr);
        methodInfo = typeof(IDataParameter).GetProperty(nameof(IDataParameter.Value)).GetSetMethod();
        var setParamValueExpr = Expression.Call(parameterExpr, methodInfo, parameterValueExpr);
        blockBodies.Add(setParamValueExpr);

        //command.Parameters.Add(parameter);
        var propertyInfo = typeof(IDbCommand).GetProperty(nameof(IDbCommand.Parameters));
        var parametersExpr = Expression.MakeMemberAccess(commandExpr, propertyInfo);
        //methodInfo = typeof(IDataParameterCollection).GetMethod(nameof(IDbCommand.Parameters.Contains));
        //var containsExpr = Expression.Call(parametersExpr, methodInfo, parameterNameExpr);
        methodInfo = typeof(IList).GetMethod(nameof(IDbCommand.Parameters.Add));
        var addParameterExpr = Expression.Call(parametersExpr, methodInfo, parameterExpr);
        //blockBodies.Add(Expression.IfThen(Expression.IsFalse(containsExpr), addParameterExpr));
        blockBodies.Add(addParameterExpr);
    }
    private static void AddReaderParameter(IDataReader reader, ParameterExpression readerExpr, int index, string fieldName, Type memberType,
       List<ParameterExpression> blockParameters, List<Expression> blockBodies, List<Expression> valuesBodies)
    {
        MethodInfo methodInfo = null;
        Expression defaultValueExpr = null;
        Expression typedValueExpr = null;

        var readerType = reader.GetFieldType(index);
        //reader[index];
        Expression readerValueExpr = Expression.Call(readerExpr, readerItemByIndex, Expression.Constant(index, typeof(int)));

        //null或default(int)
        var isNullable = false;
        Type underlyingType = null;
        if (memberType.IsValueType)
        {
            isNullable = Nullable.GetUnderlyingType(memberType) != null;
            if (isNullable)
            {
                defaultValueExpr = Expression.Convert(Expression.Constant(null), memberType);
                underlyingType = Nullable.GetUnderlyingType(memberType);
            }
            else defaultValueExpr = Expression.Default(memberType);
        }
        else defaultValueExpr = Expression.Default(memberType);

        if (memberType.IsAssignableFrom(readerType))
            typedValueExpr = Expression.Convert(readerValueExpr, memberType);
        else if (memberType == typeof(Guid) || memberType == typeof(Guid?))
        {
            if (readerType == typeof(string))
                typedValueExpr = Expression.New(typeof(Guid).GetConstructor(new Type[] { typeof(string) }), Expression.Convert(readerValueExpr, typeof(string)));
            else if (readerType == typeof(byte[]))
                typedValueExpr = Expression.New(typeof(Guid).GetConstructor(new Type[] { typeof(byte[]) }), Expression.Convert(readerValueExpr, typeof(byte[])));
            else throw new Exception($"数据库字段{fieldName}类型{readerType.FullName}与当前ValueTuple类型{memberType.FullName}不匹配，无法赋值");

            if (!isNullable) defaultValueExpr = Expression.Constant(Guid.Empty, typeof(Guid));
        }
        else
        {
            //else propValue=Convert.ToInt32(reader[index]);
            if (!isNullable) underlyingType = memberType;
            var typeCode = Type.GetTypeCode(underlyingType);
            string toTypeMethod = null;
            switch (typeCode)
            {
                case TypeCode.Boolean: toTypeMethod = nameof(Convert.ToBoolean); break;
                case TypeCode.Byte: toTypeMethod = nameof(Convert.ToByte); break;
                case TypeCode.SByte: toTypeMethod = nameof(Convert.ToSByte); break;
                case TypeCode.Int16: toTypeMethod = nameof(Convert.ToInt16); break;
                case TypeCode.UInt16: toTypeMethod = nameof(Convert.ToUInt16); break;
                case TypeCode.Int32: toTypeMethod = nameof(Convert.ToInt32); break;
                case TypeCode.UInt32: toTypeMethod = nameof(Convert.ToUInt32); break;
                case TypeCode.Int64: toTypeMethod = nameof(Convert.ToInt64); break;
                case TypeCode.UInt64: toTypeMethod = nameof(Convert.ToUInt64); break;
                case TypeCode.Single: toTypeMethod = nameof(Convert.ToSingle); break;
                case TypeCode.Double: toTypeMethod = nameof(Convert.ToDouble); break;
                case TypeCode.Decimal: toTypeMethod = nameof(Convert.ToDecimal); break;
                case TypeCode.DateTime: toTypeMethod = nameof(Convert.ToDateTime); break;
                case TypeCode.String: toTypeMethod = nameof(Convert.ToString); break;
            }

            methodInfo = typeof(Convert).GetMethod(toTypeMethod, new Type[] { typeof(object), typeof(IFormatProvider) });
            typedValueExpr = Expression.Call(methodInfo, readerValueExpr, Expression.Constant(CultureInfo.CurrentCulture));
            if (underlyingType.IsEnum)
            {
                //targetMapper.EnumType
                var enumType = underlyingType;
                switch (Type.GetTypeCode(underlyingType))
                {
                    case TypeCode.Byte: underlyingType = typeof(byte); break;
                    case TypeCode.SByte: underlyingType = typeof(sbyte); break;
                    case TypeCode.Int16: underlyingType = typeof(short); break;
                    case TypeCode.Int32: underlyingType = typeof(int); break;
                    case TypeCode.Int64: underlyingType = typeof(long); break;
                    case TypeCode.UInt16: underlyingType = typeof(ushort); break;
                    case TypeCode.UInt32: underlyingType = typeof(uint); break;
                    case TypeCode.UInt64: underlyingType = typeof(ulong); break;
                }
                methodInfo = typeof(Enum).GetMethod(nameof(Enum.ToObject), new Type[] { typeof(Type), underlyingType });
                var toEnumExpr = Expression.Call(methodInfo, Expression.Constant(enumType), typedValueExpr);
                typedValueExpr = Expression.Convert(toEnumExpr, enumType);
            }
            if (isNullable) typedValueExpr = Expression.Convert(typedValueExpr, memberType);
        }

        var localValueExpr = Expression.Variable(memberType, $"local{fieldName}");
        blockParameters.Add(localValueExpr);

        //if(localValue is DBNull)
        var isNullExpr = Expression.TypeIs(readerValueExpr, typeof(DBNull));
        var setDefaultValueExpr = Expression.Assign(localValueExpr, defaultValueExpr);
        var setTypedValueExpr = Expression.Assign(localValueExpr, typedValueExpr);
        Expression setParameterValueExpr = Expression.IfThenElse(isNullExpr, setDefaultValueExpr, setTypedValueExpr);
        blockBodies.Add(setParameterValueExpr);

        valuesBodies.Add(localValueExpr);
    }
    private static void AddReaderParameter(IDataReader reader, ParameterExpression readerExpr, ParameterExpression targetExpr, int index, string fieldName,
        MemberMap targetMapper, ParameterExpression localValueExpr, ref bool isDefinedLocal, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        MethodInfo methodInfo = null;
        Expression defaultValueExpr = null;
        Expression typedValueExpr = null;

        var readerType = reader.GetFieldType(index);
        //reader[index];
        Expression readerValueExpr = Expression.Call(readerExpr, readerItemByIndex, Expression.Constant(index, typeof(int)));

        //null或default(int)
        if (targetMapper.MemberType.IsValueType)
        {
            if (targetMapper.IsNullable) defaultValueExpr = Expression.Convert(Expression.Constant(null), targetMapper.MemberType);
            else defaultValueExpr = Expression.Default(targetMapper.MemberType);
        }
        else defaultValueExpr = Expression.Default(targetMapper.MemberType);

        if (!isDefinedLocal)
        {
            blockParameters.Add(localValueExpr);
            isDefinedLocal = true;
        }
        blockBodies.Add(Expression.Assign(localValueExpr, readerValueExpr));
        readerValueExpr = localValueExpr;

        if (targetMapper.MemberType.IsAssignableFrom(readerType))
            typedValueExpr = Expression.Convert(readerValueExpr, targetMapper.MemberType);
        else if (targetMapper.MemberType == typeof(Guid) || targetMapper.MemberType == typeof(Guid?))
        {
            if (readerType == typeof(string))
                typedValueExpr = Expression.New(typeof(Guid).GetConstructor(new Type[] { typeof(string) }), Expression.Convert(readerValueExpr, typeof(string)));
            else if (readerType == typeof(byte[]))
                typedValueExpr = Expression.New(typeof(Guid).GetConstructor(new Type[] { typeof(byte[]) }), Expression.Convert(readerValueExpr, typeof(byte[])));
            else throw new Exception($"数据库字段{fieldName}类型{readerType.FullName}与实体属性{targetMapper.MemberName}类型{targetMapper.MemberType}不匹配，无法赋值");

            if (!targetMapper.IsNullable) defaultValueExpr = Expression.Constant(Guid.Empty, typeof(Guid));
        }
        else
        {
            //else propValue=Convert.ToInt32(reader[index]);
            var typeCode = Type.GetTypeCode(targetMapper.UnderlyingType);
            string toTypeMethod = null;
            switch (typeCode)
            {
                case TypeCode.Boolean: toTypeMethod = nameof(Convert.ToBoolean); break;
                case TypeCode.Byte: toTypeMethod = nameof(Convert.ToByte); break;
                case TypeCode.SByte: toTypeMethod = nameof(Convert.ToSByte); break;
                case TypeCode.Int16: toTypeMethod = nameof(Convert.ToInt16); break;
                case TypeCode.UInt16: toTypeMethod = nameof(Convert.ToUInt16); break;
                case TypeCode.Int32: toTypeMethod = nameof(Convert.ToInt32); break;
                case TypeCode.UInt32: toTypeMethod = nameof(Convert.ToUInt32); break;
                case TypeCode.Int64: toTypeMethod = nameof(Convert.ToInt64); break;
                case TypeCode.UInt64: toTypeMethod = nameof(Convert.ToUInt64); break;
                case TypeCode.Single: toTypeMethod = nameof(Convert.ToSingle); break;
                case TypeCode.Double: toTypeMethod = nameof(Convert.ToDouble); break;
                case TypeCode.Decimal: toTypeMethod = nameof(Convert.ToDecimal); break;
                case TypeCode.DateTime: toTypeMethod = nameof(Convert.ToDateTime); break;
                case TypeCode.String: toTypeMethod = nameof(Convert.ToString); break;
            }

            methodInfo = typeof(Convert).GetMethod(toTypeMethod, new Type[] { typeof(object), typeof(IFormatProvider) });
            typedValueExpr = Expression.Call(methodInfo, readerValueExpr, Expression.Constant(CultureInfo.CurrentCulture));
            if (targetMapper.IsEnum)
            {
                //targetMapper.EnumType
                methodInfo = typeof(Enum).GetMethod(nameof(Enum.ToObject), new Type[] { typeof(Type), targetMapper.UnderlyingType });
                var toEnumExpr = Expression.Call(methodInfo, Expression.Constant(targetMapper.EnumUnderlyingType), typedValueExpr);
                typedValueExpr = Expression.Convert(toEnumExpr, targetMapper.EnumUnderlyingType);
            }
            if (targetMapper.IsNullable) typedValueExpr = Expression.Convert(typedValueExpr, targetMapper.MemberType);
        }

        //if(localValue is DBNull)
        var isNullExpr = Expression.TypeIs(readerValueExpr, typeof(DBNull));
        var setDefaultValueExpr = Expression.Assign(Expression.PropertyOrField(targetExpr, targetMapper.MemberName), defaultValueExpr);
        var setTypedValueExpr = Expression.Assign(Expression.PropertyOrField(targetExpr, targetMapper.MemberName), typedValueExpr);
        Expression setParameterValueExpr = Expression.IfThenElse(isNullExpr, setDefaultValueExpr, setTypedValueExpr);
        blockBodies.Add(setParameterValueExpr);
    }
    private static Expression NewEntityExpression(Type type)
    {
        var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        if (ctor != null) return Expression.New(ctor);
        ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(f => f.IsPublic ? 0 : (f.IsPrivate ? 2 : 1)).First();
        var parameters = new List<Expression>();
        Array.ForEach(ctor.GetParameters(), f => parameters.Add(Expression.Default(f.ParameterType)));
        return Expression.New(ctor, parameters);
    }
    private static Expression NewValueTupleExpression(Type type, List<Expression> parameters)
    {
        var fieldTypes = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(f => f.FieldType).ToArray();
        var ctor = type.GetConstructor(fieldTypes);
        if (ctor != null) return Expression.New(ctor, parameters);
        return Expression.Empty();
    }
    private static string GetAliasParameterSql(TheaConnection connection, string fieldName, string propName)
    {
        //return connection.OrmProvider.GetFieldName(fieldName);
        if (fieldName == propName) return connection.OrmProvider.GetFieldName(fieldName);
        else return connection.OrmProvider.GetFieldName(fieldName) + " AS " + connection.OrmProvider.GetFieldName(propName);
    }
    private static void BuildWhereInSqlParameters(TheaConnection connection, ParameterExpression commandExpr, Expression typedObjExpr, Expression deferredExpr, string parameterName, MemberMap paramMapper, int position, List<Expression> blockBodies)
    {
        var methodInfo = typeof(RepositoryHelper).GetMethod(nameof(RepositoryHelper.BuildWhereInParameters),
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null,
            new Type[] { typeof(IDbCommand), typeof(IOrmProvider), typeof(int), typeof(string), typeof(object), typeof(Stack<Action<StringBuilder>>) }, null);
        var callExpr = Expression.Call(methodInfo, commandExpr,
            Expression.Constant(connection.OrmProvider, typeof(IOrmProvider)),
            Expression.Constant(position, typeof(int)),
            Expression.Constant(parameterName, typeof(string)),
            Expression.Convert(Expression.PropertyOrField(typedObjExpr, paramMapper.MemberName), typeof(object)),
            deferredExpr);
        blockBodies.Add(callExpr);
    }
    private static void BuildWhereInSqlParameters(TheaConnection connection, ParameterExpression commandExpr, Expression typedObjExpr, MemberMap paramMapper, List<Expression> blockBodies)
    {
        var methodInfo = typeof(RepositoryHelper).GetMethod(nameof(RepositoryHelper.BuildWhereInParameters),
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null,
            new Type[] { typeof(IDbCommand), typeof(IOrmProvider), typeof(string), typeof(object) }, null);
        var callExpr = Expression.Call(methodInfo, commandExpr,
            Expression.Constant(connection.OrmProvider, typeof(IOrmProvider)),
            Expression.Constant(paramMapper.MemberName, typeof(string)),
            Expression.Convert(Expression.PropertyOrField(typedObjExpr, paramMapper.MemberName), typeof(object)));
        blockBodies.Add(callExpr);
    }
    private static void BuildWhereInSqlParameters(TheaConnection connection, ParameterExpression commandExpr, Expression typedObjExpr, Expression sqlBuidlerExpr, string parameterName, MemberMap paramMapper, List<Expression> blockBodies)
    {
        var methodInfo = typeof(RepositoryHelper).GetMethod(nameof(RepositoryHelper.BuildWhereInParameters),
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null,
            new Type[] { typeof(IDbCommand), typeof(IOrmProvider), typeof(string), typeof(object), typeof(StringBuilder) }, null);
        var callExpr = Expression.Call(methodInfo, commandExpr,
            Expression.Constant(connection.OrmProvider, typeof(IOrmProvider)),
            Expression.Constant(parameterName, typeof(string)),
            Expression.Convert(Expression.PropertyOrField(typedObjExpr, paramMapper.MemberName), typeof(object)),
            sqlBuidlerExpr);
        blockBodies.Add(callExpr);
    }
    private static void BuildWhereInParameters(IDbCommand command, IOrmProvider ormProvider, int position, string paramName, object value, Stack<Action<StringBuilder>> deferredStack)
    {
        if (value == null) return;
        var enumerable = value as IEnumerable;
        bool isString = value is IEnumerable<string>;

        DbType dbType = 0;
        var index = 1;
        var sqlBuilder = new StringBuilder($"(");
        foreach (var item in enumerable)
        {
            if (index == 1)
            {
                //TODO:为null的数据跳过  
                if (item == null) continue;
                dbType = DbTypeMap.FindDbType(item.GetType());
            }

            var parameterName = paramName + index.ToString();
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            if (isString)
            {
                parameter.Size = 4000;
                if (item != null && ((string)item).Length > 4000)
                    parameter.Size = -1;
            }
            parameter.Value = ToUnboxValue(item);
            parameter.DbType = dbType;
            command.Parameters.Add(parameter);
            if (index > 1) sqlBuilder.Append(",");
            sqlBuilder.Append(parameterName);
            index++;
        }
        if (index > 1)
        {
            sqlBuilder.Append(")");
            Action<StringBuilder> initializer = builder => builder.Insert(position, sqlBuilder.ToString());
            deferredStack.Push(initializer);
        }
    }
    private static void BuildWhereInParameters(IDbCommand command, IOrmProvider ormProvider, string propName, object value)
    {
        if (value == null) return;
        var paramName = ormProvider.ParameterPrefix + propName;
        var enumerable = value as IEnumerable;
        bool isString = value is IEnumerable<string>;

        DbType dbType = 0;
        var index = 1;
        var sqlBuilder = new StringBuilder("(");
        var regexIncludingUnknown = "([?@:]" + Regex.Escape(propName) + @")(?!\w)(\s+(?i)unknown(?-i))?";
        foreach (var item in enumerable)
        {
            if (index == 1)
            {
                //TODO:为null的数据跳过  
                if (item == null) continue;
                dbType = DbTypeMap.FindDbType(item.GetType());
            }

            var parameter = command.CreateParameter();
            var parameterName = paramName + index.ToString();
            parameter.ParameterName = parameterName;
            if (isString)
            {
                parameter.Size = 4000;
                if (item != null && ((string)item).Length > 4000)
                    parameter.Size = -1;
            }
            parameter.Value = ToUnboxValue(item);
            parameter.DbType = dbType;

            command.Parameters.Add(parameter);
            if (index > 1) sqlBuilder.Append(",");
            sqlBuilder.Append(parameterName);
            index++;
        }
        if (index > 1)
        {
            sqlBuilder.Append(")");
            command.CommandText = Regex.Replace(command.CommandText, regexIncludingUnknown, match =>
            {
                var variableName = match.Groups[1].Value;
                if (match.Groups[2].Success)
                {
                    // looks like an optimize hint; expand it
                    var suffix = match.Groups[2].Value;
                    var builder = new StringBuilder();
                    builder.Append(variableName).Append(1).Append(suffix);
                    for (int i = 2; i <= index - 1; i++)
                    {
                        builder.Append(',').Append(variableName).Append(i).Append(suffix);
                    }
                    return builder.ToString();
                }
                else
                {
                    var builder = new StringBuilder();
                    builder.Append('(').Append(variableName);
                    builder.Append(1);
                    for (int i = 2; i <= index - 1; i++)
                    {
                        builder.Append(',').Append(variableName);
                        builder.Append(i);
                    }
                    return builder.Append(')').ToString();
                }
            }, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);
        }
        else
        {
            command.CommandText = Regex.Replace(command.CommandText, regexIncludingUnknown, match =>
            {
                var variableName = match.Groups[1].Value;
                if (match.Groups[2].Success) return match.Value;
                else return "(SELECT " + variableName + " WHERE 1 = 0)";
            }, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);
            var dummyParam = command.CreateParameter();
            dummyParam.ParameterName = paramName;
            dummyParam.Value = DBNull.Value;
            command.Parameters.Add(dummyParam);
        }
    }
    private static void BuildWhereInParameters(IDbCommand command, IOrmProvider ormProvider, string paramName, object value, StringBuilder sqlBuilder)
    {
        if (value == null) return;
        var enumerable = value as IEnumerable;
        bool isString = value is IEnumerable<string>;

        DbType dbType = 0;
        var index = 1;
        sqlBuilder.Append("(");
        foreach (var item in enumerable)
        {
            if (index == 1)
            {
                //TODO:为null的数据跳过  
                if (item == null) continue;
                dbType = DbTypeMap.FindDbType(item.GetType());
            }

            var parameterName = paramName + index.ToString();
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            if (isString)
            {
                parameter.Size = 4000;
                if (item != null && ((string)item).Length > 4000)
                    parameter.Size = -1;
            }
            parameter.Value = ToUnboxValue(item);
            parameter.DbType = dbType;
            command.Parameters.Add(parameter);
            if (index > 1) sqlBuilder.Append(",");
            sqlBuilder.Append(parameterName);
            index++;
        }
        if (index > 1) sqlBuilder.Append(")");
    }
    private static object GetArrayItemValue(ParameterInfo parameterInfo, MemberMap paramMapper)
    {
        var breakLabel = Expression.Label(typeof(object));
        var continueLabel = Expression.Label(typeof(object));
        var typedObjExpr = Expression.Parameter(typeof(object), "obj");
        var enumeratorExpr = Expression.Variable(typeof(IEnumerator), "enumerator");
        var objExpr = Expression.Variable(typeof(object), "item");

        var valueExpr = Expression.PropertyOrField(typedObjExpr, paramMapper.MemberName);
        var enumerableExpr = Expression.Convert(valueExpr, typeof(IEnumerable));
        var enumeratorCallExpr = Expression.Call(enumerableExpr, typeof(IEnumerable).GetMethod(nameof(IEnumerable.GetEnumerator)));

        var currentExpr = Expression.Call(enumeratorExpr, typeof(IEnumerator).GetProperty(nameof(IEnumerator.Current)).GetGetMethod());

        var func = Expression.Lambda<Func<object, object>>(
            Expression.Loop(Expression.Block(new ParameterExpression[] { enumeratorExpr, objExpr },
                Expression.Assign(enumeratorExpr, enumeratorCallExpr),
                Expression.Call(enumeratorExpr, typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext))),
                Expression.Assign(objExpr, Expression.Convert(currentExpr, typeof(object))),
                Expression.IfThenElse(Expression.Equal(objExpr, Expression.Constant(null)),
                    Expression.Continue(continueLabel),
                    Expression.Break(breakLabel, objExpr))
                ), breakLabel, continueLabel), typedObjExpr).Compile();

        return func(parameterInfo.Parameters);
    }
    private static object ToUnboxValue(object value)
    {
        if (value == null) return DBNull.Value;
        if (value is Enum)
        {
            TypeCode typeCode = value is IConvertible convertible
                ? convertible.GetTypeCode()
                : Type.GetTypeCode(Enum.GetUnderlyingType(value.GetType()));

            switch (typeCode)
            {
                case TypeCode.Byte: return (byte)value;
                case TypeCode.SByte: return (sbyte)value;
                case TypeCode.Int16: return (short)value;
                case TypeCode.Int32: return (int)value;
                case TypeCode.Int64: return (long)value;
                case TypeCode.UInt16: return (ushort)value;
                case TypeCode.UInt32: return (uint)value;
                case TypeCode.UInt64: return (ulong)value;
            }
        }
        return value;
    }
}
