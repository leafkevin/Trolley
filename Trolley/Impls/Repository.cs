using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class Repository : IRepository
{
    #region 字段
    private static ConcurrentDictionary<int, string> sqlCache = new();
    private static ConcurrentDictionary<int, object> queryCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> sqlCommandInitializerCache = new();
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private IDbTransaction transaction;
    #endregion

    #region 属性
    public IOrmProvider OrmProvider => this.connection.OrmProvider;
    public IDbConnection Connection => this.connection;
    public IDbTransaction Transaction => this.transaction;
    #endregion

    #region 构造方法
    internal Repository(IOrmDbFactory dbFactory, TheaConnection connection)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
    }
    #endregion

    #region Query
    public IQuery<T> From<T>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs);
        visitor.From(typeof(T));
        return new Query<T>(visitor);
    }
    public IQuery<T1, T2> From<T1, T2>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs);
        visitor.From(typeof(T1), typeof(T2));
        return new Query<T1, T2>(visitor);
    }
    public IQuery<T1, T2, T3> From<T1, T2, T3>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs);
        visitor.From(typeof(T1), typeof(T2), typeof(T3));
        return new Query<T1, T2, T3>(visitor);
    }
    public IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new Query<T1, T2, T3, T4>(visitor);
    }
    public IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new Query<T1, T2, T3, T4, T5>(visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return new Query<T1, T2, T3, T4, T5, T6>(visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return new Query<T1, T2, T3, T4, T5, T6, T7>(visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8>(visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>(visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(visitor);
    }

    public TEntity QueryFirst<TEntity>(string rawSql, object parameters = null)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
        if (parameters != null)
        {
            if (parameters is Dictionary<string, object>)
            {
                commandInitializer = (command, ormProvider, parameter) =>
                {
                    var dict = parameter as Dictionary<string, object>;
                    foreach (var item in dict)
                    {
                        var parameterName = ormProvider.ParameterPrefix + item.Key;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                        command.Parameters.Add(dbParameter);
                    }
                };
            }
            else
            {
                var parameterType = parameters.GetType();
                var cacheKey = HashCode.Combine("Execute", this.OrmProvider, rawSql, parameterType);
                if (!sqlCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
                {
                    var parameterMapper = dbFactory.GetEntityMap(parameterType);
                    var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                    var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                    var parameterExpr = Expression.Parameter(typeof(object), "parameter");
                    var typedParameterExpr = Expression.Parameter(parameterType, "typedParameter");

                    var blockParameters = new List<ParameterExpression>();
                    var blockBodies = new List<Expression>();
                    blockParameters.Add(typedParameterExpr);
                    blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                    foreach (var memberMapper in parameterMapper.MemberMaps)
                    {
                        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var parameterNameExpr = Expression.Constant(parameterName);
                        RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, memberMapper.MemberName, blockBodies);
                    }
                    commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
                    sqlCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
                }
                commandInitializer = commandInitializerDelegate as Action<IDbCommand, IOrmProvider, object>;
            }
        }
        using var command = this.connection.CreateCommand();
        command.CommandText = rawSql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;
        if (parameters != null)
            commandInitializer.Invoke(command, this.OrmProvider, parameters);

        TEntity result = default;
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read()) result = reader.To<TEntity>(this.dbFactory, this.connection);
        reader.Close();
        reader.Dispose();
        command.Dispose();
        return result;
    }
    public async Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, object parameters = null, CancellationToken cancellationToken = default)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
        if (parameters != null)
        {
            if (parameters is Dictionary<string, object>)
            {
                commandInitializer = (command, ormProvider, parameter) =>
                {
                    var dict = parameter as Dictionary<string, object>;
                    foreach (var item in dict)
                    {
                        var parameterName = ormProvider.ParameterPrefix + item.Key;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                        command.Parameters.Add(dbParameter);
                    }
                };
            }
            else
            {
                var parameterType = parameters.GetType();
                var cacheKey = HashCode.Combine("QueryRaw", this.OrmProvider, rawSql, parameterType);
                if (!sqlCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
                {
                    var parameterMapper = dbFactory.GetEntityMap(parameterType);
                    var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                    var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                    var parameterExpr = Expression.Parameter(typeof(object), "parameter");
                    var typedParameterExpr = Expression.Parameter(parameterType, "typedParameter");

                    var blockParameters = new List<ParameterExpression>();
                    var blockBodies = new List<Expression>();
                    blockParameters.Add(typedParameterExpr);
                    blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                    foreach (var memberMapper in parameterMapper.MemberMaps)
                    {
                        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var parameterNameExpr = Expression.Constant(parameterName);
                        RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, memberMapper.MemberName, blockBodies);
                    }
                    commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
                    sqlCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
                }
                commandInitializer = commandInitializerDelegate as Action<IDbCommand, IOrmProvider, object>;
            }
        }

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = rawSql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        if (parameters != null)
            commandInitializer.Invoke(cmd, this.OrmProvider, parameters);

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        TEntity result = default;
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
            result = reader.To<TEntity>(dbFactory, this.connection);
        await reader.CloseAsync();
        await reader.DisposeAsync();
        await command.DisposeAsync();
        return result;
    }
    public TEntity QueryFirst<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        var whereObjType = whereObj.GetType();
        var sqlCacheKey = HashCode.Combine("Query", connection.OrmProvider, entityType, typeof(object));
        if (!sqlCache.TryGetValue(sqlCacheKey, out var sql))
        {
            var index = 0;
            var ormProvider = connection.OrmProvider;
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var builder = new StringBuilder("SELECT ");
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                if (propMapper.FieldName != propMapper.MemberName)
                    builder.Append(" AS " + ormProvider.GetFieldName(propMapper.MemberName));
                index++;
            }
            builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)}");
            sql = builder.ToString();
            sqlCache.TryAdd(sqlCacheKey, sql);
        }

        Func<IDbCommand, IOrmProvider, string, object, string> commandSqlInitializer = null;
        if (whereObj is Dictionary<string, object> dict)
        {
            commandSqlInitializer = (command, ormProvider, sql, whereObj) =>
            {
                var dict = whereObj as Dictionary<string, object>;
                var entityMapper = dbFactory.GetEntityMap(entityType);
                var builder = new StringBuilder(" WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                    if (propMapper.NativeDbType.HasValue)
                        command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType.Value, item.Value));
                    else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                    index++;
                }
                builder.Insert(0, sql);
                return builder.ToString();
            };
        }
        else
        {
            var cacheKey = HashCode.Combine("Query", connection.OrmProvider, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = dbFactory.GetEntityMap(entityType);
                var whereObjMapper = dbFactory.GetEntityMap(whereObjType);
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
                var ormProvider = connection.OrmProvider;
                var builder = new StringBuilder(" WHERE ");
                foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedWhereObjExpr, parameterNameExpr, propMapper.NativeDbType, propMapper.MemberName, blockBodies);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    index++;
                }
                var methodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
                var returnExpr = Expression.Call(methodInfo, sqlExpr, Expression.Constant(builder.ToString(), typeof(string)));

                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                commandInitializerDelegate = Expression.Lambda<Func<IDbCommand, IOrmProvider, string, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, sqlExpr, whereObjExpr).Compile();
                queryCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandSqlInitializer = (Func<IDbCommand, IOrmProvider, string, object, string>)commandInitializerDelegate;
        }

        using var command = this.connection.CreateCommand();
        command.CommandText = commandSqlInitializer?.Invoke(command, this.connection.OrmProvider, sql, whereObj);
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        TEntity result = default;
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read()) result = reader.To<TEntity>(this.dbFactory, this.connection);
        reader.Close();
        reader.Dispose();
        command.Dispose();
        return result;
    }
    public async Task<TEntity> QueryFirstAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        var whereObjType = whereObj.GetType();
        var sqlCacheKey = HashCode.Combine("Query", connection.OrmProvider, entityType, typeof(object));
        if (!sqlCache.TryGetValue(sqlCacheKey, out var sql))
        {
            var index = 0;
            var ormProvider = connection.OrmProvider;
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var builder = new StringBuilder("SELECT ");
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                if (propMapper.FieldName != propMapper.MemberName)
                    builder.Append(" AS " + ormProvider.GetFieldName(propMapper.MemberName));
                index++;
            }
            builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)}");
            sql = builder.ToString();
            sqlCache.TryAdd(sqlCacheKey, sql);
        }

        Func<IDbCommand, IOrmProvider, string, object, string> commandSqlInitializer = null;
        if (whereObj is Dictionary<string, object> dict)
        {
            commandSqlInitializer = (command, ormProvider, sql, whereObj) =>
            {
                var dict = whereObj as Dictionary<string, object>;
                var entityMapper = dbFactory.GetEntityMap(entityType);
                var builder = new StringBuilder(" WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                    if (propMapper.NativeDbType.HasValue)
                        command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType.Value, item.Value));
                    else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                    index++;
                }
                builder.Insert(0, sql);
                return builder.ToString();
            };
        }
        else
        {
            var cacheKey = HashCode.Combine("Query", connection.OrmProvider, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = dbFactory.GetEntityMap(entityType);
                var whereObjMapper = dbFactory.GetEntityMap(whereObjType);
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
                var ormProvider = connection.OrmProvider;
                var builder = new StringBuilder(" WHERE ");
                foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedWhereObjExpr, parameterNameExpr, propMapper.NativeDbType, propMapper.MemberName, blockBodies);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    index++;
                }
                var methodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
                var returnExpr = Expression.Call(methodInfo, sqlExpr, Expression.Constant(builder.ToString(), typeof(string)));

                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                commandInitializerDelegate = Expression.Lambda<Func<IDbCommand, IOrmProvider, string, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, sqlExpr, whereObjExpr).Compile();
                queryCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandSqlInitializer = (Func<IDbCommand, IOrmProvider, string, object, string>)commandInitializerDelegate;
        }

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = commandSqlInitializer?.Invoke(cmd, this.connection.OrmProvider, sql, whereObj);
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        TEntity result = default;
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
            result = reader.To<TEntity>(dbFactory, this.connection);
        await reader.CloseAsync();
        await reader.DisposeAsync();
        await command.DisposeAsync();
        return result;
    }
    public List<TEntity> Query<TEntity>(string rawSql, object parameters = null)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
        if (parameters != null)
        {
            if (parameters is Dictionary<string, object>)
            {
                commandInitializer = (command, ormProvider, parameter) =>
                {
                    var dict = parameter as Dictionary<string, object>;
                    foreach (var item in dict)
                    {
                        var parameterName = ormProvider.ParameterPrefix + item.Key;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                        command.Parameters.Add(dbParameter);
                    }
                };
            }
            else
            {
                var parameterType = parameters.GetType();
                var cacheKey = HashCode.Combine("Execute", this.OrmProvider, rawSql, parameterType);
                if (!sqlCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
                {
                    var parameterMapper = dbFactory.GetEntityMap(parameterType);
                    var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                    var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                    var parameterExpr = Expression.Parameter(typeof(object), "parameter");
                    var typedParameterExpr = Expression.Parameter(parameterType, "typedParameter");

                    var blockParameters = new List<ParameterExpression>();
                    var blockBodies = new List<Expression>();
                    blockParameters.Add(typedParameterExpr);
                    blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                    foreach (var memberMapper in parameterMapper.MemberMaps)
                    {
                        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var parameterNameExpr = Expression.Constant(parameterName);
                        RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, memberMapper.MemberName, blockBodies);
                    }
                    commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
                    sqlCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
                }
                commandInitializer = commandInitializerDelegate as Action<IDbCommand, IOrmProvider, object>;
            }
        }
        using var command = this.connection.CreateCommand();
        command.CommandText = rawSql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;
        if (parameters != null)
            commandInitializer.Invoke(command, this.OrmProvider, parameters);

        var result = new List<TEntity>();
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(behavior);
        while (reader.Read())
        {
            result.Add(reader.To<TEntity>(dbFactory, this.connection));
        }
        reader.Close();
        reader.Dispose();
        command.Dispose();
        return result;
    }
    public async Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, object parameters = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
        if (parameters != null)
        {
            if (parameters is Dictionary<string, object>)
            {
                commandInitializer = (command, ormProvider, parameter) =>
                {
                    var dict = parameter as Dictionary<string, object>;
                    foreach (var item in dict)
                    {
                        var parameterName = ormProvider.ParameterPrefix + item.Key;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                        command.Parameters.Add(dbParameter);
                    }
                };
            }
            else
            {
                var parameterType = parameters.GetType();
                var cacheKey = HashCode.Combine("QueryRaw", this.OrmProvider, rawSql, parameterType);
                if (!sqlCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
                {
                    var parameterMapper = dbFactory.GetEntityMap(parameterType);
                    var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                    var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                    var parameterExpr = Expression.Parameter(typeof(object), "parameter");
                    var typedParameterExpr = Expression.Parameter(parameterType, "typedParameter");

                    var blockParameters = new List<ParameterExpression>();
                    var blockBodies = new List<Expression>();
                    blockParameters.Add(typedParameterExpr);
                    blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                    foreach (var memberMapper in parameterMapper.MemberMaps)
                    {
                        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var parameterNameExpr = Expression.Constant(parameterName);
                        RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, memberMapper.MemberName, blockBodies);
                    }
                    commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
                    sqlCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
                }
                commandInitializer = commandInitializerDelegate as Action<IDbCommand, IOrmProvider, object>;
            }
        }

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = rawSql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        if (parameters != null)
            commandInitializer.Invoke(cmd, this.OrmProvider, parameters);

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        var result = new List<TEntity>();
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(reader.To<TEntity>(dbFactory, this.connection));
        }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        await command.DisposeAsync();
        return result;
    }
    public List<TEntity> Query<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        var whereObjType = whereObj.GetType();
        var sqlCacheKey = HashCode.Combine("Query", connection.OrmProvider, entityType, typeof(object));
        if (!sqlCache.TryGetValue(sqlCacheKey, out var sql))
        {
            var index = 0;
            var ormProvider = connection.OrmProvider;
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var builder = new StringBuilder("SELECT ");
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                if (propMapper.FieldName != propMapper.MemberName)
                    builder.Append(" AS " + ormProvider.GetFieldName(propMapper.MemberName));
                index++;
            }
            builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)}");
            sql = builder.ToString();
            sqlCache.TryAdd(sqlCacheKey, sql);
        }

        Func<IDbCommand, IOrmProvider, string, object, string> commandSqlInitializer = null;
        if (whereObj is Dictionary<string, object> dict)
        {
            commandSqlInitializer = (command, ormProvider, sql, whereObj) =>
            {
                var dict = whereObj as Dictionary<string, object>;
                var entityMapper = dbFactory.GetEntityMap(entityType);
                var builder = new StringBuilder(" WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                    if (propMapper.NativeDbType.HasValue)
                        command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType.Value, item.Value));
                    else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                    index++;
                }
                builder.Insert(0, sql);
                return builder.ToString();
            };
        }
        else
        {
            var cacheKey = HashCode.Combine("Query", connection.OrmProvider, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = dbFactory.GetEntityMap(entityType);
                var whereObjMapper = dbFactory.GetEntityMap(whereObjType);
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
                var ormProvider = connection.OrmProvider;
                var builder = new StringBuilder(" WHERE ");
                foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));

                    RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedWhereObjExpr, parameterNameExpr, propMapper.NativeDbType, propMapper.MemberName, blockBodies);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    index++;
                }
                var methodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
                var returnExpr = Expression.Call(methodInfo, sqlExpr, Expression.Constant(builder.ToString(), typeof(string)));

                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                commandInitializerDelegate = Expression.Lambda<Func<IDbCommand, IOrmProvider, string, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, sqlExpr, whereObjExpr).Compile();
                queryCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandSqlInitializer = (Func<IDbCommand, IOrmProvider, string, object, string>)commandInitializerDelegate;
        }

        using var command = this.connection.CreateCommand();
        command.CommandText = commandSqlInitializer?.Invoke(command, this.connection.OrmProvider, sql, whereObj);
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        var result = new List<TEntity>();
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(behavior);
        while (reader.Read())
        {
            result.Add(reader.To<TEntity>(dbFactory, this.connection));
        }
        reader.Close();
        reader.Dispose();
        command.Dispose();
        return result;
    }
    public async Task<List<TEntity>> QueryAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        var whereObjType = whereObj.GetType();
        var sqlCacheKey = HashCode.Combine("Query", connection.OrmProvider, entityType, typeof(object));
        if (!sqlCache.TryGetValue(sqlCacheKey, out var sql))
        {
            var index = 0;
            var ormProvider = connection.OrmProvider;
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var builder = new StringBuilder("SELECT ");
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                if (propMapper.FieldName != propMapper.MemberName)
                    builder.Append(" AS " + ormProvider.GetFieldName(propMapper.MemberName));
                index++;
            }
            builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)}");
            sql = builder.ToString();
            sqlCache.TryAdd(sqlCacheKey, sql);
        }

        Func<IDbCommand, IOrmProvider, string, object, string> commandSqlInitializer = null;
        if (whereObj is Dictionary<string, object> dict)
        {
            commandSqlInitializer = (command, ormProvider, sql, whereObj) =>
            {
                var dict = whereObj as Dictionary<string, object>;
                var entityMapper = dbFactory.GetEntityMap(entityType);
                var builder = new StringBuilder(" WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                    if (propMapper.NativeDbType.HasValue)
                        command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType.Value, item.Value));
                    else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                    index++;
                }
                builder.Insert(0, sql);
                return builder.ToString();
            };
        }
        else
        {
            var cacheKey = HashCode.Combine("Query", connection.OrmProvider, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = dbFactory.GetEntityMap(entityType);
                var whereObjMapper = dbFactory.GetEntityMap(whereObjType);
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
                var ormProvider = connection.OrmProvider;
                var builder = new StringBuilder(" WHERE ");
                foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedWhereObjExpr, parameterNameExpr, propMapper.NativeDbType, propMapper.MemberName, blockBodies);
                    index++;
                }
                var methodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
                var returnExpr = Expression.Call(methodInfo, sqlExpr, Expression.Constant(builder.ToString(), typeof(string)));

                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                commandInitializerDelegate = Expression.Lambda<Func<IDbCommand, IOrmProvider, string, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, sqlExpr, whereObjExpr).Compile();
                queryCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandSqlInitializer = (Func<IDbCommand, IOrmProvider, string, object, string>)commandInitializerDelegate;
        }

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = commandSqlInitializer?.Invoke(cmd, this.connection.OrmProvider, sql, whereObj);
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        var result = new List<TEntity>();
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(reader.To<TEntity>(dbFactory, this.connection));
        }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        await command.DisposeAsync();
        return result;
    }
    #endregion

    #region Get
    public TEntity Get<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        var sqlCacheKey = HashCode.Combine("Get", connection.OrmProvider, entityType, entityType);
        if (!sqlCache.TryGetValue(sqlCacheKey, out var sql))
        {
            var index = 0;
            var ormProvider = connection.OrmProvider;
            var entityMapper = dbFactory.GetEntityMap(entityType);

            var builder = new StringBuilder("SELECT ");
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
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
            sqlCache.TryAdd(sqlCacheKey, sql);
        }
        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
        if (entityType.IsEntityType())
        {
            if (whereObj is Dictionary<string, object>)
            {
                commandInitializer = (command, ormProvider, whereObj) =>
                {
                    var dict = whereObj as Dictionary<string, object>;
                    var entityMapper = dbFactory.GetEntityMap(entityType);
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || !propMapper.IsKey)
                            continue;

                        var parameterName = ormProvider.ParameterPrefix + item.Key;
                        if (propMapper.NativeDbType.HasValue)
                            command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType.Value, item.Value));
                        else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                    }
                };
            }
            else
            {
                var whereObjType = whereObj.GetType();
                var cacheKey = HashCode.Combine("Get", connection.OrmProvider, entityType, whereObjType);
                if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
                {
                    var entityMapper = dbFactory.GetEntityMap(entityType);
                    var whereObjMapper = dbFactory.GetEntityMap(whereObjType);
                    var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                    var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                    var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");
                    var typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");

                    var blockParameters = new List<ParameterExpression>();
                    var blockBodies = new List<Expression>();
                    blockParameters.Add(typedWhereObjExpr);
                    blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));

                    var index = 0;
                    var ormProvider = connection.OrmProvider;
                    foreach (var keyMapper in entityMapper.KeyMembers)
                    {
                        if (!whereObjMapper.TryGetMemberMap(keyMapper.MemberName, out var whereObjPropMapper))
                            throw new ArgumentNullException($"参数类型{whereObjType.FullName}缺少主键字段{keyMapper.MemberName}", "whereObj");

                        var parameterName = $"{ormProvider.ParameterPrefix}{keyMapper.MemberName}";
                        var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                        RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedWhereObjExpr, parameterNameExpr, keyMapper.NativeDbType, keyMapper.MemberName, blockBodies);
                        index++;
                    }
                    commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, whereObjExpr).Compile();
                    queryCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
                    commandInitializer = (Action<IDbCommand, IOrmProvider, object>)commandInitializerDelegate;
                }
            }
        }
        else
        {
            var entityMapper = dbFactory.GetEntityMap(entityType);
            if (entityMapper.KeyMembers.Count > 1)
                throw new ArgumentException($"模型{entityType.FullName}有多个主键栏位，whereObj参数值与主键栏位不匹配");

            commandInitializer = (command, ormProvider, whereObj) =>
            {
                var keyMapper = entityMapper.KeyMembers[0];
                var parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName;
                if (keyMapper.NativeDbType.HasValue)
                    command.Parameters.Add(ormProvider.CreateParameter(parameterName, keyMapper.NativeDbType.Value, whereObj));
                else command.Parameters.Add(ormProvider.CreateParameter(parameterName, whereObj));
            };
        }
        using var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;
        commandInitializer?.Invoke(command, this.connection.OrmProvider, whereObj);

        TEntity result = default;
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read()) result = reader.To<TEntity>(dbFactory, this.connection);
        reader.Close();
        reader.Dispose();
        command.Dispose();
        return result;
    }
    public async Task<TEntity> GetAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        var sqlCacheKey = HashCode.Combine("Get", connection.OrmProvider, entityType, entityType);
        if (!sqlCache.TryGetValue(sqlCacheKey, out var sql))
        {
            var index = 0;
            var ormProvider = connection.OrmProvider;
            var entityMapper = dbFactory.GetEntityMap(entityType);

            var builder = new StringBuilder("SELECT ");
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
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
            sqlCache.TryAdd(sqlCacheKey, sql);
        }
        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
        if (entityType.IsEntityType())
        {
            if (whereObj is Dictionary<string, object>)
            {
                commandInitializer = (command, ormProvider, whereObj) =>
                {
                    var dict = whereObj as Dictionary<string, object>;
                    var entityMapper = dbFactory.GetEntityMap(entityType);
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || !propMapper.IsKey)
                            continue;

                        var parameterName = ormProvider.ParameterPrefix + item.Key;
                        if (propMapper.NativeDbType.HasValue)
                            command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType.Value, item.Value));
                        else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                    }
                };
            }
            else
            {
                var whereObjType = whereObj.GetType();
                var cacheKey = HashCode.Combine("Get", connection.OrmProvider, entityType, whereObjType);
                if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
                {
                    var entityMapper = dbFactory.GetEntityMap(entityType);
                    var whereObjMapper = dbFactory.GetEntityMap(whereObjType);
                    var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                    var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                    var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");
                    var typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");

                    var blockParameters = new List<ParameterExpression>();
                    var blockBodies = new List<Expression>();
                    blockParameters.Add(typedWhereObjExpr);
                    blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));

                    var index = 0;
                    var ormProvider = connection.OrmProvider;
                    foreach (var keyMapper in entityMapper.KeyMembers)
                    {
                        if (!whereObjMapper.TryGetMemberMap(keyMapper.MemberName, out var whereObjPropMapper))
                            throw new ArgumentNullException($"参数类型{whereObjType.FullName}缺少主键字段{keyMapper.MemberName}", "whereObj");

                        var parameterName = $"{ormProvider.ParameterPrefix}{keyMapper.MemberName}";
                        var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                        RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedWhereObjExpr, parameterNameExpr, keyMapper.NativeDbType, keyMapper.MemberName, blockBodies);
                        index++;
                    }
                    commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, whereObjExpr).Compile();
                    queryCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
                    commandInitializer = (Action<IDbCommand, IOrmProvider, object>)commandInitializerDelegate;
                }
            }
        }
        else
        {
            var entityMapper = dbFactory.GetEntityMap(entityType);
            if (entityMapper.KeyMembers.Count > 1)
                throw new ArgumentException($"模型{entityType.FullName}有多个主键栏位，whereObj参数值与主键栏位不匹配");

            commandInitializer = (command, ormProvider, whereObj) =>
            {
                var keyMapper = entityMapper.KeyMembers[0];
                var parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName;
                if (keyMapper.NativeDbType.HasValue)
                    command.Parameters.Add(ormProvider.CreateParameter(parameterName, keyMapper.NativeDbType.Value, whereObj));
                else command.Parameters.Add(ormProvider.CreateParameter(parameterName, whereObj));
            };
        }

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        commandInitializer?.Invoke(cmd, this.connection.OrmProvider, whereObj);

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        TEntity result = default;
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
            result = reader.To<TEntity>(dbFactory, this.connection);
        await reader.CloseAsync();
        await reader.DisposeAsync();
        await command.DisposeAsync();
        return result;
    }
    #endregion

    #region Create
    public ICreate<TEntity> Create<TEntity>() => new Create<TEntity>(this.dbFactory, this.connection, this.transaction);
    #endregion

    #region Update
    public IUpdate<T> Update<T>() => new Update<T>(this.dbFactory, this.connection, this.transaction);
    #endregion

    #region Delete
    public IDelete<T> Delete<T>() => new Delete<T>(this.dbFactory, this.connection, this.transaction);
    #endregion

    #region Exists
    public bool Exists<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var whereObjType = whereObj.GetType();
        Func<IDbCommand, IOrmProvider, object, string> commandSqlInitializer = null;
        if (whereObj is Dictionary<string, object> dict)
        {
            commandSqlInitializer = (command, ormProvider, whereObj) =>
            {
                var dict = whereObj as Dictionary<string, object>;
                var entityMapper = this.dbFactory.GetEntityMap(whereObjType);
                var builder = new StringBuilder($"SELECT COUNT(1) FROM {this.OrmProvider.GetTableName(entityMapper.TableName)} WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                    if (propMapper.NativeDbType.HasValue)
                        command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType.Value, item.Value));
                    else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                    index++;
                }
                return builder.ToString();
            };
        }
        else
        {
            var entityType = typeof(TEntity);
            var cacheKey = HashCode.Combine("Exists", connection.OrmProvider, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = dbFactory.GetEntityMap(entityType);
                var whereObjMapper = dbFactory.GetEntityMap(whereObjType);
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");
                var typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");

                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedWhereObjExpr);
                blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));

                var index = 0;
                var ormProvider = connection.OrmProvider;
                var builder = new StringBuilder(" WHERE ");
                foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedWhereObjExpr, parameterNameExpr, propMapper.NativeDbType, propMapper.MemberName, blockBodies);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    index++;
                }
                var returnExpr = Expression.Constant(builder.ToString());
                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                commandInitializerDelegate = Expression.Lambda<Func<IDbCommand, IOrmProvider, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, whereObjExpr).Compile();
                queryCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandSqlInitializer = (Func<IDbCommand, IOrmProvider, object, string>)commandInitializerDelegate;
        }

        using var command = this.connection.CreateCommand();
        command.CommandText = commandSqlInitializer.Invoke(command, this.OrmProvider, whereObj);
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        int result = 0;
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read()) result = reader.To<int>();
        reader.Close();
        reader.Dispose();
        command.Dispose();
        return result > 0;
    }
    public async Task<bool> ExistsAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var whereObjType = whereObj.GetType();
        Func<IDbCommand, IOrmProvider, object, string> commandSqlInitializer = null;
        if (whereObj is Dictionary<string, object> dict)
        {
            commandSqlInitializer = (command, ormProvider, whereObj) =>
            {
                var dict = whereObj as Dictionary<string, object>;
                var entityMapper = this.dbFactory.GetEntityMap(whereObjType);
                var builder = new StringBuilder($"SELECT COUNT(1) FROM {this.OrmProvider.GetTableName(entityMapper.TableName)} WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                    if (propMapper.NativeDbType.HasValue)
                        command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType.Value, item.Value));
                    else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                    index++;
                }
                return builder.ToString();
            };
        }
        else
        {
            var entityType = typeof(TEntity);
            var cacheKey = HashCode.Combine("Exists", connection.OrmProvider, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = dbFactory.GetEntityMap(entityType);
                var whereObjMapper = dbFactory.GetEntityMap(whereObjType);
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");
                var typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");

                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedWhereObjExpr);
                blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));

                var index = 0;
                var ormProvider = connection.OrmProvider;
                var builder = new StringBuilder(" WHERE ");
                foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.MemberType.IsEntityType())
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedWhereObjExpr, parameterNameExpr, propMapper.NativeDbType, propMapper.MemberName, blockBodies);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    index++;
                }
                var returnExpr = Expression.Constant(builder.ToString());
                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                commandInitializerDelegate = Expression.Lambda<Func<IDbCommand, IOrmProvider, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, whereObjExpr).Compile();
                queryCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandSqlInitializer = (Func<IDbCommand, IOrmProvider, object, string>)commandInitializerDelegate;
        }

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = commandSqlInitializer.Invoke(cmd, this.OrmProvider, whereObj);
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        int result = 0;
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (reader.Read()) result = reader.To<int>();
        await command.DisposeAsync();
        return result > 0;
    }
    public bool Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate)
        => this.From<TEntity>().Where(wherePredicate).Count() > 0;
    public async Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
        => await this.From<TEntity>().Where(wherePredicate).CountAsync() > 0;
    #endregion

    #region Execute
    public int Execute(string rawSql, object parameters = null)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
        if (parameters != null)
        {
            if (parameters is Dictionary<string, object>)
            {
                commandInitializer = (command, ormProvider, parameter) =>
                {
                    var dict = parameter as Dictionary<string, object>;
                    foreach (var item in dict)
                    {
                        var parameterName = ormProvider.ParameterPrefix + item.Key;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                        command.Parameters.Add(dbParameter);
                    }
                };
            }
            else
            {
                var parameterType = parameters.GetType();
                var cacheKey = HashCode.Combine("Execute", this.OrmProvider, rawSql, parameterType);
                if (!sqlCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
                {
                    var parameterMapper = dbFactory.GetEntityMap(parameterType);
                    var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                    var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                    var parameterExpr = Expression.Parameter(typeof(object), "parameter");
                    var typedParameterExpr = Expression.Parameter(parameterType, "typedParameter");

                    var blockParameters = new List<ParameterExpression>();
                    var blockBodies = new List<Expression>();
                    blockParameters.Add(typedParameterExpr);
                    blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                    foreach (var memberMapper in parameterMapper.MemberMaps)
                    {
                        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var parameterNameExpr = Expression.Constant(parameterName);
                        RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, memberMapper.MemberName, blockBodies);
                    }
                    commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
                    sqlCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
                }
                commandInitializer = commandInitializerDelegate as Action<IDbCommand, IOrmProvider, object>;
            }
        }
        using var command = this.connection.CreateCommand();
        command.CommandText = rawSql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;
        if (parameters != null)
            commandInitializer.Invoke(command, this.OrmProvider, parameters);

        this.connection.Open();
        var result = command.ExecuteNonQuery();
        command.Dispose();
        return result;
    }
    public async Task<int> ExecuteAsync(string rawSql, object parameters = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
        if (parameters != null)
        {
            if (parameters is Dictionary<string, object>)
            {
                commandInitializer = (command, ormProvider, parameter) =>
                {
                    var dict = parameter as Dictionary<string, object>;
                    foreach (var item in dict)
                    {
                        var parameterName = ormProvider.ParameterPrefix + item.Key;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                        command.Parameters.Add(dbParameter);
                    }
                };
            }
            else
            {
                var parameterType = parameters.GetType();
                var cacheKey = HashCode.Combine("Execute", this.OrmProvider, rawSql, parameterType);
                if (!sqlCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
                {
                    var parameterMapper = dbFactory.GetEntityMap(parameterType);
                    var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                    var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                    var parameterExpr = Expression.Parameter(typeof(object), "parameter");
                    var typedParameterExpr = Expression.Parameter(parameterType, "typedParameter");

                    var blockParameters = new List<ParameterExpression>();
                    var blockBodies = new List<Expression>();
                    blockParameters.Add(typedParameterExpr);
                    blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                    foreach (var memberMapper in parameterMapper.MemberMaps)
                    {
                        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var parameterNameExpr = Expression.Constant(parameterName);
                        RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, memberMapper.MemberName, blockBodies);
                    }
                    commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
                    sqlCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
                }
                commandInitializer = commandInitializerDelegate as Action<IDbCommand, IOrmProvider, object>;
            }
        }

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = rawSql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        if (parameters != null)
            commandInitializer.Invoke(cmd, this.OrmProvider, parameters);

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        await command.DisposeAsync();
        return result;
    }
    #endregion

    #region Others
    public void Close() => this.connection?.Close();
    public async Task CloseAsync() => await this.connection?.CloseAsync();
    public void Timeout(int timeout) => this.connection.CommandTimeout = timeout;
    public void BeginTransaction() => this.transaction = this.Connection.BeginTransaction();
    public void Commit() => this.transaction.Commit();
    public void Rollback() => this.transaction.Rollback();
    public void Dispose() => this.connection?.Dispose();
    public async ValueTask DisposeAsync() => await this.connection?.DisposeAsync();
    #endregion
}
