using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class Repository : IRepository
{
    private static ConcurrentDictionary<int, string> sqlCache = new();
    private static ConcurrentDictionary<int, object> commandInitializerCache = new();

    #region 字段
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
    public IQuery<T> From<T>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T));
        return new Query<T>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2> From<T1, T2>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2));
        return new Query<T1, T2>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3> From<T1, T2, T3>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3));
        return new Query<T1, T2, T3>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new Query<T1, T2, T3, T4>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new Query<T1, T2, T3, T4, T5>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return new Query<T1, T2, T3, T4, T5, T6>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return new Query<T1, T2, T3, T4, T5, T6, T7>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this.dbFactory, this.connection, this.transaction, visitor);
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
                if (propMapper.IsIgnore || propMapper.IsNavigation)
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
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || propMapper.IsIgnore)
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    command.Parameters.Add(dbParameter);
                    index++;
                }
                builder.Insert(0, sql);
                return builder.ToString();
            };
        }
        else
        {
            var cacheKey = HashCode.Combine("Query", connection.OrmProvider, entityType, whereObjType);
            if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
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
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore)
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedWhereObjExpr, parameterNameExpr, propMapper.MemberName, blockBodies);
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
                commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandSqlInitializer = (Func<IDbCommand, IOrmProvider, string, object, string>)commandInitializerDelegate;
        }

        var command = this.connection.CreateCommand();
        command.CommandText = commandSqlInitializer?.Invoke(command, this.connection.OrmProvider, sql, whereObj);
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        TEntity result = default;
        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read()) result = reader.To<TEntity>(this.dbFactory, this.connection);
        reader.Close();
        reader.Dispose();
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
                if (propMapper.IsIgnore || propMapper.IsNavigation)
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
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || propMapper.IsIgnore)
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    command.Parameters.Add(dbParameter);
                    index++;
                }
                builder.Insert(0, sql);
                return builder.ToString();
            };
        }
        else
        {
            var cacheKey = HashCode.Combine("Query", connection.OrmProvider, entityType, whereObjType);
            if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
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
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore)
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedWhereObjExpr, parameterNameExpr, propMapper.MemberName, blockBodies);
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
                commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandSqlInitializer = (Func<IDbCommand, IOrmProvider, string, object, string>)commandInitializerDelegate;
        }

        var cmd = this.connection.CreateCommand();
        cmd.CommandText = commandSqlInitializer?.Invoke(cmd, this.connection.OrmProvider, sql, whereObj);
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;

        if (cmd is not DbCommand command)
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        TEntity result = default;
        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
            result = reader.To<TEntity>(dbFactory, this.connection);
        await reader.CloseAsync();
        await reader.DisposeAsync();
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
                if (propMapper.IsIgnore || propMapper.IsNavigation)
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
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || propMapper.IsIgnore)
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    command.Parameters.Add(dbParameter);
                    index++;
                }
                builder.Insert(0, sql);
                return builder.ToString();
            };
        }
        else
        {
            var cacheKey = HashCode.Combine("Query", connection.OrmProvider, entityType, whereObjType);
            if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
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
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore)
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedWhereObjExpr, parameterNameExpr, propMapper.MemberName, blockBodies);
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
                commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandSqlInitializer = (Func<IDbCommand, IOrmProvider, string, object, string>)commandInitializerDelegate;
        }

        var command = this.connection.CreateCommand();
        command.CommandText = commandSqlInitializer?.Invoke(command, this.connection.OrmProvider, sql, whereObj);
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        var result = new List<TEntity>();
        connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(behavior);
        while (reader.Read())
        {
            result.Add(reader.To<TEntity>(dbFactory, this.connection));
        }
        reader.Close();
        reader.Dispose();
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
                if (propMapper.IsIgnore || propMapper.IsNavigation)
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
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || propMapper.IsIgnore)
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    command.Parameters.Add(dbParameter);
                    index++;
                }
                builder.Insert(0, sql);
                return builder.ToString();
            };
        }
        else
        {
            var cacheKey = HashCode.Combine("Query", connection.OrmProvider, entityType, whereObjType);
            if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
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
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper) || propMapper.IsIgnore)
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedWhereObjExpr, parameterNameExpr, propMapper.MemberName, blockBodies);
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
                commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandSqlInitializer = (Func<IDbCommand, IOrmProvider, string, object, string>)commandInitializerDelegate;
        }

        var cmd = this.connection.CreateCommand();
        cmd.CommandText = commandSqlInitializer?.Invoke(cmd, this.connection.OrmProvider, sql, whereObj);
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;

        if (cmd is not DbCommand command)
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        var result = new List<TEntity>();
        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(reader.To<TEntity>(dbFactory, this.connection));
        }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        return result;
    }

    //public IQueryReader QueryMultiple(Action<IMultiQuery> queries)
    //{
    //    if (queries == null) throw new ArgumentNullException(nameof(queries));
    //    var multiQuery = new MultiQuery();
    //    queries.Invoke(multiQuery);
    //    //TODO:
    //    multiQuery.ToSql();
    //    var visitor = new SqlExpressionVisitor(this.dbFactory, this.connection.OrmProvider);
    //    return new SqlExpression<TEntity>(this.dbFactory, this.connection, visitor.From(typeof(TEntity), sqlType));
    //}
    //public Task<IQueryReader> QueryMultipleAsync(Action<IMultiQuery> queries, CancellationToken cancellationToken = default)
    //{
    //    throw new NotImplementedException();
    //}
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
                if (propMapper.IsIgnore || propMapper.IsNavigation)
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                if (propMapper.FieldName != propMapper.MemberName)
                    builder.Append(" AS " + ormProvider.GetFieldName(propMapper.MemberName));
                index++;
            }
            builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
            index = 0;
            foreach (var propMapper in entityMapper.KeyMembers)
            {
                if (index > 0)
                    builder.Append(" AND ");
                var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                index++;
            }
            sql = builder.ToString();
            sqlCache.TryAdd(sqlCacheKey, sql);
        }
        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
        if (whereObj is Dictionary<string, object> dict)
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
                    var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                    command.Parameters.Add(dbParameter);
                }
            };
        }
        else
        {
            var whereObjType = whereObj.GetType();
            var cacheKey = HashCode.Combine("Get", connection.OrmProvider, entityType, whereObjType);
            if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
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
                foreach (var propMapper in entityMapper.KeyMembers)
                {
                    if (!whereObjMapper.TryGetMemberMap(propMapper.MemberName, out var whereObjPropMapper))
                        continue;

                    var parameterName = $"{ormProvider.ParameterPrefix}{propMapper.MemberName}";
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedWhereObjExpr, parameterNameExpr, propMapper.MemberName, blockBodies);
                    index++;
                }
                commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, whereObjExpr).Compile();
                commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
                commandInitializer = (Action<IDbCommand, IOrmProvider, object>)commandInitializerDelegate;
            }
        }

        var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;
        commandInitializer?.Invoke(command, this.connection.OrmProvider, whereObj);

        TEntity result = default;
        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read()) result = reader.To<TEntity>(dbFactory, this.connection);
        reader.Close();
        reader.Dispose();
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
                if (propMapper.IsIgnore || propMapper.IsNavigation)
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                if (propMapper.FieldName != propMapper.MemberName)
                    builder.Append(" AS " + ormProvider.GetFieldName(propMapper.MemberName));
                index++;
            }
            builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
            index = 0;
            foreach (var propMapper in entityMapper.KeyMembers)
            {
                if (index > 0)
                    builder.Append(" AND ");
                var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                index++;
            }
            sql = builder.ToString();
            sqlCache.TryAdd(sqlCacheKey, sql);
        }
        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
        if (whereObj is Dictionary<string, object> dict)
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
                    var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                    command.Parameters.Add(dbParameter);
                }
            };
        }
        else
        {
            var whereObjType = whereObj.GetType();
            var cacheKey = HashCode.Combine("Get", connection.OrmProvider, entityType, whereObjType);
            if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
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
                foreach (var propMapper in entityMapper.KeyMembers)
                {
                    if (!whereObjMapper.TryGetMemberMap(propMapper.MemberName, out var whereObjPropMapper))
                        continue;

                    var parameterName = $"{ormProvider.ParameterPrefix}{propMapper.MemberName}";
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedWhereObjExpr, parameterNameExpr, propMapper.MemberName, blockBodies);
                    index++;
                }
                commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, whereObjExpr).Compile();
                commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
                commandInitializer = (Action<IDbCommand, IOrmProvider, object>)commandInitializerDelegate;
            }
        }

        var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        commandInitializer?.Invoke(cmd, this.connection.OrmProvider, whereObj);

        if (cmd is not DbCommand command)
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        TEntity result = default;
        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
            result = reader.To<TEntity>(dbFactory, this.connection);
        await reader.CloseAsync();
        await reader.DisposeAsync();
        return result;
    }
    #endregion

    #region Create
    public ICreate<TEntity> Create<TEntity>() => new Create<TEntity>(this.dbFactory, this.connection, this.transaction);
    #endregion

    #region Update
    public int Update<TEntity>(object updateObj, object whereObj)
    {
        throw new NotImplementedException();
    }
    public Task<int> UpdateAsync<TEntity>(object updateObj, object whereObj, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public IUpdate<T> Update<T>() => new Update<T>(this.dbFactory, this.connection, this.transaction);
    #endregion

    #region Delete
    public int DeleteByKey<TEntity>(object keys)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteByKeyAsync<TEntity>(object keys, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public int Delete<TEntity>(object whereObj)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }


    public IDelete<T> Delete<T>() => new Delete<T>(this.dbFactory, this.connection, this.transaction);
    #endregion

    #region Exists
    public bool Exists<TEntity>(object anonymousObj)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public bool Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Execute
    public int Execute(string sql, object parameters = null)
    {
        throw new NotImplementedException();
    }

    public Task<int> ExecuteAsync(string sql, object parameters = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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
