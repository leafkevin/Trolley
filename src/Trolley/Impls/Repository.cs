using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
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
    private bool isParameterized = false;
    private TheaConnection connection;
    #endregion

    #region 属性
    public string DbKey { get; private set; }
    public IDbConnection Connection => this.connection;
    public IOrmProvider OrmProvider { get; private set; }
    public IEntityMapProvider MapProvider { get; private set; }
    public IDbTransaction Transaction { get; private set; }
    #endregion

    #region 构造方法
    public Repository(string dbKey, IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider entityMapProvider)
    {
        this.DbKey = dbKey;
        this.connection = new TheaConnection { DbKey = dbKey, BaseConnection = connection };
        this.OrmProvider = ormProvider;
        this.MapProvider = entityMapProvider;
    }
    public Repository(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider entityMapProvider)
    {
        this.DbKey = connection.DbKey;
        this.connection = connection;
        this.OrmProvider = ormProvider;
        this.MapProvider = entityMapProvider;
    }
    #endregion

    #region Query
    public IQuery<T> From<T>(char tableAsStart = 'a', string suffixRawSql = null)
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T), suffixRawSql);
        return new Query<T>(this.connection, this.Transaction, visitor);
    }
    public IQuery<T> From<T>(Func<IFromQuery, IFromQuery<T>> subQuery, char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart, "p1w");
        subQuery.Invoke(new FromQuery(visitor));
        var sql = visitor.BuildSql(out var dbDataParameters, out var readerFields);
        var newVisitor = visitor.Clone(tableAsStart);
        newVisitor.WithTable(typeof(T), sql, dbDataParameters, readerFields);
        return new Query<T>(this.connection, this.Transaction, newVisitor);
    }
    public IQuery<T> FromWith<T>(Func<IFromQuery, IFromQuery<T>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart, "p1w");
        cteSubQuery.Invoke(new FromQuery(visitor));
        var rawSql = visitor.BuildSql(out var dbDataParameters, out var readerFields);
        var newVisitor = visitor.Clone(tableAsStart);
        newVisitor.WithCteTable(typeof(T), cteTableName, false, rawSql, dbDataParameters, readerFields);
        return new Query<T>(this.connection, this.Transaction, newVisitor);
    }
    public IQuery<T> FromWithRecursive<T>(Func<IFromQuery, string, IFromQuery<T>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart, "p1w");
        cteSubQuery.Invoke(new FromQuery(visitor), cteTableName);
        var rawSql = visitor.BuildSql(out var dbDataParameters, out var readerFields);
        var newVisitor = visitor.Clone(tableAsStart);
        newVisitor.WithCteTable(typeof(T), cteTableName, true, rawSql, dbDataParameters, readerFields);
        return new Query<T>(this.connection, this.Transaction, newVisitor);
    }
    public IQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2));
        return new Query<T1, T2>(this.connection, this.Transaction, visitor);
    }
    public IQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3));
        return new Query<T1, T2, T3>(this.connection, this.Transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new Query<T1, T2, T3, T4>(this.connection, this.Transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new Query<T1, T2, T3, T4, T5>(this.connection, this.Transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return new Query<T1, T2, T3, T4, T5, T6>(this.connection, this.Transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return new Query<T1, T2, T3, T4, T5, T6, T7>(this.connection, this.Transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8>(this.connection, this.Transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this.connection, this.Transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this.connection, this.Transaction, visitor);
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
                        var parameterName = this.OrmProvider.ParameterPrefix + item.Key;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var dbParameter = this.OrmProvider.CreateParameter(parameterName, dict[item.Key]);
                        command.Parameters.Add(dbParameter);
                    }
                };
            }
            else
            {
                var parameterType = parameters.GetType();
                var cacheKey = HashCode.Combine("Execute", this.connection, rawSql, parameterType);
                if (!sqlCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
                {
                    var parameterMapper = this.MapProvider.GetEntityMap(parameterType);
                    var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                    var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                    var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                    var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
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
                        RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, memberMapper, this.OrmProvider, blockBodies);
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
        command.Transaction = this.Transaction;
        if (parameters != null)
            commandInitializer.Invoke(command, this.OrmProvider, parameters);

        TEntity result = default;
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read())
        {
            var entityType = typeof(TEntity);
            if (entityType.IsEntityType())
                result = reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider);
            else result = reader.To<TEntity>();
        }
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
                        var parameterName = this.OrmProvider.ParameterPrefix + item.Key;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var dbParameter = this.OrmProvider.CreateParameter(parameterName, dict[item.Key]);
                        command.Parameters.Add(dbParameter);
                    }
                };
            }
            else
            {
                var parameterType = parameters.GetType();
                var cacheKey = HashCode.Combine("QueryRaw", this.connection, rawSql, parameterType);
                if (!sqlCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
                {
                    var parameterMapper = this.MapProvider.GetEntityMap(parameterType);
                    var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                    var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                    var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                    var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
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
                        RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, memberMapper, this.OrmProvider, blockBodies);
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
        cmd.Transaction = this.Transaction;
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
        {
            var entityType = typeof(TEntity);
            if (entityType.IsEntityType())
                result = reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider);
            else result = reader.To<TEntity>();
        }
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
        var sqlCacheKey = HashCode.Combine("Query", this.connection, entityType, typeof(object));
        if (!sqlCache.TryGetValue(sqlCacheKey, out var sql))
        {
            var index = 0;
            var entityMapper = this.MapProvider.GetEntityMap(entityType);
            var builder = new StringBuilder("SELECT ");
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || propMapper.IsNavigation
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(this.OrmProvider.GetFieldName(propMapper.FieldName));
                if (propMapper.FieldName != propMapper.MemberName)
                    builder.Append(" AS " + this.OrmProvider.GetFieldName(propMapper.MemberName));
                index++;
            }
            builder.Append($" FROM {this.OrmProvider.GetTableName(entityMapper.TableName)}");
            sql = builder.ToString();
            sqlCache.TryAdd(sqlCacheKey, sql);
        }

        Func<IDbCommand, IOrmProvider, string, object, string> commandSqlInitializer = null;
        if (whereObj is Dictionary<string, object> dict)
        {
            commandSqlInitializer = (command, ormProvider, sql, whereObj) =>
            {
                var dict = whereObj as Dictionary<string, object>;
                var entityMapper = this.MapProvider.GetEntityMap(entityType);
                var builder = new StringBuilder(" WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType() && memberMapper.TypeHandler == null))
                        continue;

                    var parameterName = this.OrmProvider.ParameterPrefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");

                    if (memberMapper.NativeDbType != null)
                        command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, item.Value));
                    else command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, item.Value));
                    index++;
                }
                builder.Insert(0, sql);
                return builder.ToString();
            };
        }
        else
        {
            var cacheKey = HashCode.Combine("Query", this.connection, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = this.MapProvider.GetEntityMap(entityType);
                var whereObjMapper = this.MapProvider.GetEntityMap(whereObjType);
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
                foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = this.OrmProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, propMapper, this.OrmProvider, blockBodies);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{this.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
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
        command.CommandText = commandSqlInitializer?.Invoke(command, this.OrmProvider, sql, whereObj);
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;

        TEntity result = default;
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read())
        {
            if (entityType.IsEntityType())
                result = reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider);
            else result = reader.To<TEntity>();
        }
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
        var sqlCacheKey = HashCode.Combine("Query", this.connection, entityType, typeof(object));
        if (!sqlCache.TryGetValue(sqlCacheKey, out var sql))
        {
            var index = 0;
            var entityMapper = this.MapProvider.GetEntityMap(entityType);
            var builder = new StringBuilder("SELECT ");
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || propMapper.IsNavigation
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(this.OrmProvider.GetFieldName(propMapper.FieldName));
                if (propMapper.FieldName != propMapper.MemberName)
                    builder.Append(" AS " + this.OrmProvider.GetFieldName(propMapper.MemberName));
                index++;
            }
            builder.Append($" FROM {this.OrmProvider.GetTableName(entityMapper.TableName)}");
            sql = builder.ToString();
            sqlCache.TryAdd(sqlCacheKey, sql);
        }

        Func<IDbCommand, IOrmProvider, string, object, string> commandSqlInitializer = null;
        if (whereObj is Dictionary<string, object> dict)
        {
            commandSqlInitializer = (command, ormProvider, sql, whereObj) =>
            {
                var dict = whereObj as Dictionary<string, object>;
                var entityMapper = this.MapProvider.GetEntityMap(entityType);
                var builder = new StringBuilder(" WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = this.OrmProvider.ParameterPrefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{this.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                    if (propMapper.NativeDbType != null)
                        command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                    else command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, item.Value));
                    index++;
                }
                builder.Insert(0, sql);
                return builder.ToString();
            };
        }
        else
        {
            var cacheKey = HashCode.Combine("Query", this.connection, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = this.MapProvider.GetEntityMap(entityType);
                var whereObjMapper = this.MapProvider.GetEntityMap(whereObjType);
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
                foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = this.OrmProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, propMapper, this.OrmProvider, blockBodies);

                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{this.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
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
        cmd.CommandText = commandSqlInitializer?.Invoke(cmd, this.OrmProvider, sql, whereObj);
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        TEntity result = default;
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            if (entityType.IsEntityType())
                result = reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider);
            else result = reader.To<TEntity>();
        }
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
                        var parameterName = this.OrmProvider.ParameterPrefix + item.Key;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var dbParameter = this.OrmProvider.CreateParameter(parameterName, dict[item.Key]);
                        command.Parameters.Add(dbParameter);
                    }
                };
            }
            else
            {
                var parameterType = parameters.GetType();
                var cacheKey = HashCode.Combine("Execute", this.connection, rawSql, parameterType);
                if (!sqlCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
                {
                    var parameterMapper = this.MapProvider.GetEntityMap(parameterType);
                    var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                    var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                    var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                    var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
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
                        RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, memberMapper, this.OrmProvider, blockBodies);
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
        command.Transaction = this.Transaction;
        if (parameters != null)
            commandInitializer.Invoke(command, this.OrmProvider, parameters);

        var result = new List<TEntity>();
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(behavior);
        var entityType = typeof(TEntity);
        if (entityType.IsEntityType())
        {
            while (reader.Read())
            {
                result.Add(reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider));
            }
        }
        else
        {
            while (reader.Read())
            {
                result.Add(reader.To<TEntity>());
            }
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
                        var parameterName = this.OrmProvider.ParameterPrefix + item.Key;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var dbParameter = this.OrmProvider.CreateParameter(parameterName, dict[item.Key]);
                        command.Parameters.Add(dbParameter);
                    }
                };
            }
            else
            {
                var parameterType = parameters.GetType();
                var cacheKey = HashCode.Combine("QueryRaw", this.connection, rawSql, parameterType);
                if (!sqlCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
                {
                    var parameterMapper = this.MapProvider.GetEntityMap(parameterType);
                    var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                    var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                    var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                    var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
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
                        RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, memberMapper, this.OrmProvider, blockBodies);
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
        cmd.Transaction = this.Transaction;
        if (parameters != null)
            commandInitializer.Invoke(cmd, this.OrmProvider, parameters);

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        var result = new List<TEntity>();
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        var entityType = typeof(TEntity);
        if (entityType.IsEntityType())
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider));
            }
        }
        else
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<TEntity>());
            }
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
        var sqlCacheKey = HashCode.Combine("Query", this.connection, entityType, typeof(object));
        if (!sqlCache.TryGetValue(sqlCacheKey, out var sql))
        {
            var index = 0;
            var entityMapper = this.MapProvider.GetEntityMap(entityType);
            var builder = new StringBuilder("SELECT ");
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || propMapper.IsNavigation
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(this.OrmProvider.GetFieldName(propMapper.FieldName));
                if (propMapper.FieldName != propMapper.MemberName)
                    builder.Append(" AS " + this.OrmProvider.GetFieldName(propMapper.MemberName));
                index++;
            }
            builder.Append($" FROM {this.OrmProvider.GetTableName(entityMapper.TableName)}");
            sql = builder.ToString();
            sqlCache.TryAdd(sqlCacheKey, sql);
        }

        Func<IDbCommand, IOrmProvider, string, object, string> commandSqlInitializer = null;
        if (whereObj is Dictionary<string, object> dict)
        {
            commandSqlInitializer = (command, ormProvider, sql, whereObj) =>
            {
                var dict = whereObj as Dictionary<string, object>;
                var entityMapper = this.MapProvider.GetEntityMap(entityType);
                var builder = new StringBuilder(" WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = this.OrmProvider.ParameterPrefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{this.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                    if (propMapper.NativeDbType != null)
                        command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                    else command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, item.Value));
                    index++;
                }
                builder.Insert(0, sql);
                return builder.ToString();
            };
        }
        else
        {
            var cacheKey = HashCode.Combine("Query", this.connection, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = this.MapProvider.GetEntityMap(entityType);
                var whereObjMapper = this.MapProvider.GetEntityMap(whereObjType);
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
                foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = this.OrmProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));

                    RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, propMapper, this.OrmProvider, blockBodies);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{this.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
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
        command.CommandText = commandSqlInitializer?.Invoke(command, this.OrmProvider, sql, whereObj);
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;

        var result = new List<TEntity>();
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(behavior);
        if (entityType.IsEntityType())
        {
            while (reader.Read())
            {
                result.Add(reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider));
            }
        }
        else
        {
            while (reader.Read())
            {
                result.Add(reader.To<TEntity>());
            }
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
        var sqlCacheKey = HashCode.Combine("Query", this.connection, entityType, typeof(object));
        if (!sqlCache.TryGetValue(sqlCacheKey, out var sql))
        {
            var index = 0;
            var entityMapper = this.MapProvider.GetEntityMap(entityType);
            var builder = new StringBuilder("SELECT ");
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || propMapper.IsNavigation
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(this.OrmProvider.GetFieldName(propMapper.FieldName));
                if (propMapper.FieldName != propMapper.MemberName)
                    builder.Append(" AS " + this.OrmProvider.GetFieldName(propMapper.MemberName));
                index++;
            }
            builder.Append($" FROM {this.OrmProvider.GetTableName(entityMapper.TableName)}");
            sql = builder.ToString();
            sqlCache.TryAdd(sqlCacheKey, sql);
        }

        Func<IDbCommand, IOrmProvider, string, object, string> commandSqlInitializer = null;
        if (whereObj is Dictionary<string, object> dict)
        {
            commandSqlInitializer = (command, ormProvider, sql, whereObj) =>
            {
                var dict = whereObj as Dictionary<string, object>;
                var entityMapper = this.MapProvider.GetEntityMap(entityType);
                var builder = new StringBuilder(" WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = this.OrmProvider.ParameterPrefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{this.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                    if (propMapper.NativeDbType != null)
                        command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                    else command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, item.Value));
                    index++;
                }
                builder.Insert(0, sql);
                return builder.ToString();
            };
        }
        else
        {
            var cacheKey = HashCode.Combine("Query", this.connection, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = this.MapProvider.GetEntityMap(entityType);
                var whereObjMapper = this.MapProvider.GetEntityMap(whereObjType);
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
                foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = this.OrmProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{this.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, propMapper, this.OrmProvider, blockBodies);
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
        cmd.CommandText = commandSqlInitializer?.Invoke(cmd, this.OrmProvider, sql, whereObj);
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        var result = new List<TEntity>();
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (entityType.IsEntityType())
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider));
            }
        }
        else
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<TEntity>());
            }
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
        var sqlCacheKey = HashCode.Combine("Get", this.connection, entityType, entityType);
        if (!sqlCache.TryGetValue(sqlCacheKey, out var sql))
        {
            var index = 0;
            var entityMapper = this.MapProvider.GetEntityMap(entityType);

            var builder = new StringBuilder("SELECT ");
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || propMapper.IsNavigation
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(this.OrmProvider.GetFieldName(propMapper.FieldName));
                if (propMapper.FieldName != propMapper.MemberName)
                    builder.Append(" AS " + this.OrmProvider.GetFieldName(propMapper.MemberName));
                index++;
            }
            builder.Append($" FROM {this.OrmProvider.GetTableName(entityMapper.TableName)} WHERE ");
            index = 0;
            foreach (var keyMapper in entityMapper.KeyMembers)
            {
                if (index > 0)
                    builder.Append(" AND ");
                var parameterName = this.OrmProvider.ParameterPrefix + keyMapper.MemberName;
                builder.Append($"{this.OrmProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");
                index++;
            }
            sql = builder.ToString();
            sqlCache.TryAdd(sqlCacheKey, sql);
        }
        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;

        if (whereObj is Dictionary<string, object>)
        {
            commandInitializer = (command, ormProvider, whereObj) =>
            {
                var dict = whereObj as Dictionary<string, object>;
                var entityMapper = this.MapProvider.GetEntityMap(entityType);
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || !propMapper.IsKey)
                        continue;

                    var parameterName = this.OrmProvider.ParameterPrefix + item.Key;
                    if (propMapper.NativeDbType != null)
                        command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                    else command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, item.Value));
                }
            };
        }
        else
        {
            var whereObjType = whereObj.GetType();
            var cacheKey = HashCode.Combine("Get", this.connection, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = this.MapProvider.GetEntityMap(entityType);
                var whereObjMapper = this.MapProvider.GetEntityMap(whereObjType);
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");
                ParameterExpression typedWhereObjExpr = null;

                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                bool isEntityType = false;

                if (whereObjType.IsEntityType())
                {
                    isEntityType = true;
                    typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");
                    blockParameters.Add(typedWhereObjExpr);
                    blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));
                }
                else
                {
                    if (entityMapper.KeyMembers.Count > 1)
                        throw new NotSupportedException($"模型{entityType.FullName}有多个主键字段，不能使用单个值类型{whereObjType.FullName}作为参数");
                }

                var index = 0;
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    if (isEntityType && !whereObjMapper.TryGetMemberMap(keyMapper.MemberName, out var whereObjPropMapper))
                        throw new ArgumentNullException($"参数类型{whereObjType.FullName}缺少主键字段{keyMapper.MemberName}", "whereObj");

                    var parameterName = $"{this.OrmProvider.ParameterPrefix}{keyMapper.MemberName}";
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));

                    if (isEntityType)
                        RepositoryHelper.AddKeyMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, keyMapper, this.OrmProvider, blockBodies);
                    else RepositoryHelper.AddKeyValueParameter(commandExpr, ormProviderExpr, parameterNameExpr, whereObjExpr, keyMapper, this.OrmProvider, blockBodies);
                    index++;
                }
                commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, whereObjExpr).Compile();
                queryCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandInitializer = (Action<IDbCommand, IOrmProvider, object>)commandInitializerDelegate;
        }

        using var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;
        commandInitializer?.Invoke(command, this.OrmProvider, whereObj);

        TEntity result = default;
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read()) result = reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider);
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
        var sqlCacheKey = HashCode.Combine("Get", this.connection, entityType, entityType);
        if (!sqlCache.TryGetValue(sqlCacheKey, out var sql))
        {
            var index = 0;
            var entityMapper = this.MapProvider.GetEntityMap(entityType);

            var builder = new StringBuilder("SELECT ");
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || propMapper.IsNavigation
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(this.OrmProvider.GetFieldName(propMapper.FieldName));
                if (propMapper.FieldName != propMapper.MemberName)
                    builder.Append(" AS " + this.OrmProvider.GetFieldName(propMapper.MemberName));
                index++;
            }
            builder.Append($" FROM {this.OrmProvider.GetTableName(entityMapper.TableName)} WHERE ");
            index = 0;
            foreach (var keyMapper in entityMapper.KeyMembers)
            {
                if (index > 0)
                    builder.Append(" AND ");
                var parameterName = this.OrmProvider.ParameterPrefix + keyMapper.MemberName;
                builder.Append($"{this.OrmProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");
                index++;
            }
            sql = builder.ToString();
            sqlCache.TryAdd(sqlCacheKey, sql);
        }
        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;

        if (whereObj is Dictionary<string, object>)
        {
            commandInitializer = (command, ormProvider, whereObj) =>
            {
                var dict = whereObj as Dictionary<string, object>;
                var entityMapper = this.MapProvider.GetEntityMap(entityType);
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || !propMapper.IsKey)
                        continue;

                    var parameterName = this.OrmProvider.ParameterPrefix + item.Key;
                    if (propMapper.NativeDbType != null)
                        command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                    else command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, item.Value));
                }
            };
        }
        else
        {
            var whereObjType = whereObj.GetType();
            var cacheKey = HashCode.Combine("Get", this.connection, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = this.MapProvider.GetEntityMap(entityType);
                var whereObjMapper = this.MapProvider.GetEntityMap(whereObjType);
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");
                ParameterExpression typedWhereObjExpr = null;

                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                bool isEntityType = false;

                if (whereObjType.IsEntityType())
                {
                    isEntityType = true;
                    typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");
                    blockParameters.Add(typedWhereObjExpr);
                    blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));
                }
                else
                {
                    if (entityMapper.KeyMembers.Count > 1)
                        throw new NotSupportedException($"模型{entityType.FullName}有多个主键字段，不能使用单个值类型{whereObjType.FullName}作为参数");
                }

                var index = 0;
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    if (isEntityType && !whereObjMapper.TryGetMemberMap(keyMapper.MemberName, out var whereObjPropMapper))
                        throw new ArgumentNullException($"参数类型{whereObjType.FullName}缺少主键字段{keyMapper.MemberName}", "whereObj");

                    var parameterName = $"{this.OrmProvider.ParameterPrefix}{keyMapper.MemberName}";
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    if (isEntityType)
                        RepositoryHelper.AddKeyMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, keyMapper, this.OrmProvider, blockBodies);
                    else RepositoryHelper.AddKeyValueParameter(commandExpr, ormProviderExpr, parameterNameExpr, whereObjExpr, keyMapper, this.OrmProvider, blockBodies);
                    index++;
                }
                commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, whereObjExpr).Compile();
                queryCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandInitializer = (Action<IDbCommand, IOrmProvider, object>)commandInitializerDelegate;
        }

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;
        commandInitializer?.Invoke(cmd, this.OrmProvider, whereObj);

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        TEntity result = default;
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
            result = reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider);
        await reader.CloseAsync();
        await reader.DisposeAsync();
        await command.DisposeAsync();
        return result;
    }
    #endregion

    #region Create
    public ICreate<TEntity> Create<TEntity>() => new Create<TEntity>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, this.isParameterized);
    #endregion

    #region Update
    public IUpdate<TEntity> Update<TEntity>() => new Update<TEntity>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, this.isParameterized);
    #endregion

    #region Delete
    public IDelete<TEntity> Delete<TEntity>() => new Delete<TEntity>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, this.isParameterized);
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
                var entityMapper = this.MapProvider.GetEntityMap(whereObjType);
                var builder = new StringBuilder($"SELECT COUNT(1) FROM {this.OrmProvider.GetTableName(entityMapper.TableName)} WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = this.OrmProvider.ParameterPrefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{this.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                    if (propMapper.NativeDbType != null)
                        command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                    else command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, item.Value));
                    index++;
                }
                return builder.ToString();
            };
        }
        else
        {
            var entityType = typeof(TEntity);
            var cacheKey = HashCode.Combine("Exists", this.connection, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = this.MapProvider.GetEntityMap(entityType);
                var whereObjMapper = this.MapProvider.GetEntityMap(whereObjType);
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

                var typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedWhereObjExpr);
                blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));

                var index = 0;
                var builder = new StringBuilder(" WHERE ");
                foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = this.OrmProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, propMapper, this.OrmProvider, blockBodies);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{this.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
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
        command.Transaction = this.Transaction;

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
                var entityMapper = this.MapProvider.GetEntityMap(whereObjType);
                var builder = new StringBuilder($"SELECT COUNT(1) FROM {this.OrmProvider.GetTableName(entityMapper.TableName)} WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = this.OrmProvider.ParameterPrefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{this.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                    if (propMapper.NativeDbType != null)
                        command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                    else command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, item.Value));
                    index++;
                }
                return builder.ToString();
            };
        }
        else
        {
            var entityType = typeof(TEntity);
            var cacheKey = HashCode.Combine("Exists", this.connection, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = this.MapProvider.GetEntityMap(entityType);
                var whereObjMapper = this.MapProvider.GetEntityMap(whereObjType);
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

                var typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedWhereObjExpr);
                blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));

                var index = 0;
                var builder = new StringBuilder(" WHERE ");
                foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = this.OrmProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, propMapper, this.OrmProvider, blockBodies);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{this.OrmProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
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
        cmd.Transaction = this.Transaction;

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
                        var parameterName = this.OrmProvider.ParameterPrefix + item.Key;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var dbParameter = this.OrmProvider.CreateParameter(parameterName, dict[item.Key]);
                        command.Parameters.Add(dbParameter);
                    }
                };
            }
            else
            {
                var parameterType = parameters.GetType();
                var cacheKey = HashCode.Combine("Execute", this.connection, rawSql, parameterType);
                if (!sqlCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
                {
                    var parameterMapper = this.MapProvider.GetEntityMap(parameterType);
                    var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                    var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                    var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                    var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
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
                        RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, memberMapper, this.OrmProvider, blockBodies);
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
        command.Transaction = this.Transaction;
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
                        var parameterName = this.OrmProvider.ParameterPrefix + item.Key;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var dbParameter = this.OrmProvider.CreateParameter(parameterName, dict[item.Key]);
                        command.Parameters.Add(dbParameter);
                    }
                };
            }
            else
            {
                var parameterType = parameters.GetType();
                var cacheKey = HashCode.Combine("Execute", this.connection, rawSql, parameterType);
                if (!sqlCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
                {
                    var parameterMapper = this.MapProvider.GetEntityMap(parameterType);
                    var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                    var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                    var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                    var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
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
                        RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, memberMapper, this.OrmProvider, blockBodies);
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
        cmd.Transaction = this.Transaction;
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
    public void Close() => this.Dispose();
    public async Task CloseAsync() => await this.DisposeAsync();
    public IRepository Timeout(int timeout)
    {
        this.connection.CommandTimeout = timeout;
        return this;
    }
    public IRepository WithParameterized(bool isParameterized = true)
    {
        this.isParameterized = isParameterized;
        return this;
    }
    public IRepository With(OrmDbFactoryOptions options)
    {
        if (options == null) return this;
        this.isParameterized = options.IsParameterized;
        this.connection.CommandTimeout = options.Timeout;
        return this;
    }
    public void BeginTransaction()
    {
        this.connection.Open();
        this.Transaction = this.connection.BeginTransaction();
    }
    public void Commit()
    {
        this.Transaction?.Commit();
        this.Transaction?.Dispose();
        this.Transaction = null;
    }
    public void Rollback()
    {
        this.Transaction?.Rollback();
        this.Transaction?.Dispose();
        this.Transaction = null;
    }
    public void Dispose()
    {
        this.Transaction?.Dispose();
        this.connection?.Dispose();
        this.Transaction = null;
    }
    public async ValueTask DisposeAsync()
    {
        if (this.Transaction is DbTransaction dbTransaction)
            await dbTransaction.DisposeAsync();
        await this.connection?.DisposeAsync();
        this.Transaction = null;
    }
    #endregion
}
