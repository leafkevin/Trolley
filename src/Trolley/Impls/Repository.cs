using System;
using System.Collections;
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
    #region Fields
    private bool isParameterized = false;
    protected TheaConnection connection;
    #endregion

    #region Properties
    public string DbKey { get; private set; }
    public IDbConnection Connection => this.connection;
    public IOrmProvider OrmProvider { get; private set; }
    public IEntityMapProvider MapProvider { get; private set; }
    public IDbTransaction Transaction { get; private set; }
    #endregion

    #region Constructor
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

    #region From
    public IQuery<T> From<T>(char tableAsStart = 'a', string suffixRawSql = null)
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T), suffixRawSql);
        return new Query<T>(this.connection, this.Transaction, this.OrmProvider, visitor);
    }
    public IQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2));
        return new Query<T1, T2>(this.connection, this.Transaction, this.OrmProvider, visitor);
    }
    public IQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3));
        return new Query<T1, T2, T3>(this.connection, this.Transaction, this.OrmProvider, visitor);
    }
    public IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new Query<T1, T2, T3, T4>(this.connection, this.Transaction, this.OrmProvider, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new Query<T1, T2, T3, T4, T5>(this.connection, this.Transaction, this.OrmProvider, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return new Query<T1, T2, T3, T4, T5, T6>(this.connection, this.Transaction, this.OrmProvider, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return new Query<T1, T2, T3, T4, T5, T6, T7>(this.connection, this.Transaction, this.OrmProvider, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8>(this.connection, this.Transaction, this.OrmProvider, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this.connection, this.Transaction, this.OrmProvider, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this.connection, this.Transaction, this.OrmProvider, visitor);
    }
    #endregion

    #region From SubQuery
    public IQuery<T> From<T>(Func<IFromQuery, IFromQuery<T>> subQuery, char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        subQuery.Invoke(new FromQuery(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor));
        var sql = visitor.BuildSql(out _, out var readerFields);
        var newVisitor = visitor.Clone(tableAsStart);
        newVisitor.WithTable(typeof(T), sql, readerFields);
        return new Query<T>(this.connection, this.Transaction, this.OrmProvider, newVisitor);
    }
    #endregion

    #region FromWith/FromWithRecursive
    public IQuery<T> FromWith<T>(Func<IFromQuery, IFromQuery<T>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        cteSubQuery.Invoke(new FromQuery(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor));
        var rawSql = visitor.BuildSql(out _, out var readerFields);
        var newVisitor = visitor.Clone(tableAsStart);
        newVisitor.WithCteTable(typeof(T), cteTableName, false, rawSql, readerFields);
        return new Query<T>(this.connection, this.Transaction, this.OrmProvider, newVisitor);
    }
    public IQuery<T> FromWithRecursive<T>(Func<IFromQuery, string, IFromQuery<T>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        cteSubQuery.Invoke(new FromQuery(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor), cteTableName);
        var rawSql = visitor.BuildSql(out _, out var readerFields);
        var newVisitor = visitor.Clone(tableAsStart);
        newVisitor.WithCteTable(typeof(T), cteTableName, true, rawSql, readerFields);
        return new Query<T>(this.connection, this.Transaction, this.OrmProvider, newVisitor);
    }
    #endregion

    #region QueryFirst/Query
    public TEntity QueryFirst<TEntity>(string rawSql, object parameters = null)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        var entityType = typeof(TEntity);
        using var command = this.connection.CreateCommand();
        command.CommandText = rawSql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;
        if (parameters != null)
        {
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.Connection, this.OrmProvider, rawSql, parameters);
            commandInitializer.Invoke(command, this.OrmProvider, parameters);
        }

        TEntity result = default;
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read())
        {
            if (entityType.IsEntityType(out _))
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
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        var entityType = typeof(TEntity);
        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = rawSql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;
        if (parameters != null)
        {
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.Connection, this.OrmProvider, rawSql, parameters);
            commandInitializer.Invoke(cmd, this.OrmProvider, parameters);
        }

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        TEntity result = default;
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            if (entityType.IsEntityType(out _))
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
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.connection, this.OrmProvider, this.MapProvider, entityType, whereObj);

        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;
        command.CommandText = commandInitializer.Invoke(command, this.OrmProvider, this.MapProvider, "", whereObj);

        TEntity result = default;
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read())
        {
            if (entityType.IsEntityType(out _))
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
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.connection, this.OrmProvider, this.MapProvider, entityType, whereObj);

        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;
        cmd.CommandText = commandInitializer.Invoke(cmd, this.OrmProvider, this.MapProvider, "", whereObj);

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        TEntity result = default;
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            if (entityType.IsEntityType(out _))
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

        var entityType = typeof(TEntity);
        using var command = this.connection.CreateCommand();
        command.CommandText = rawSql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;
        if (parameters != null)
        {
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.Connection, this.OrmProvider, rawSql, parameters);
            commandInitializer.Invoke(command, this.OrmProvider, parameters);
        }

        var result = new List<TEntity>();
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(behavior);

        if (entityType.IsEntityType(out _))
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

        var entityType = typeof(TEntity);
        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = rawSql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;
        if (parameters != null)
        {
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.Connection, this.OrmProvider, rawSql, parameters);
            commandInitializer.Invoke(cmd, this.OrmProvider, parameters);
        }

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        var result = new List<TEntity>();
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (entityType.IsEntityType(out _))
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
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.connection, this.OrmProvider, this.MapProvider, entityType, whereObj);

        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;
        command.CommandText = commandInitializer.Invoke(command, this.OrmProvider, this.MapProvider, "", whereObj);

        var result = new List<TEntity>();
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(behavior);
        if (entityType.IsEntityType(out _))
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
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.connection, this.OrmProvider, this.MapProvider, entityType, whereObj);

        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;
        cmd.CommandText = commandInitializer.Invoke(cmd, this.OrmProvider, this.MapProvider, "", whereObj);

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        var result = new List<TEntity>();
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (entityType.IsEntityType(out _))
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
        var commandInitializer = RepositoryHelper.BuildGetSqlParameters(this.connection, this.OrmProvider, this.MapProvider, entityType, whereObj);

        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;
        command.CommandText = commandInitializer.Invoke(command, this.OrmProvider, this.MapProvider, "", whereObj);

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
        var commandInitializer = RepositoryHelper.BuildGetSqlParameters(this.connection, this.OrmProvider, this.MapProvider, entityType, whereObj);

        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;
        cmd.CommandText = commandInitializer.Invoke(cmd, this.OrmProvider, this.MapProvider, "", whereObj);
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
    public int Create<TEntity>(object insertObjs, int bulkCount = 500)
    {
        bool isMulti = insertObjs is IEnumerable && insertObjs is not string && insertObjs is not IDictionary<string, object>;
        if (isMulti) return this.Create<TEntity>().WithBulk(insertObjs as IEnumerable, bulkCount).Execute();
        else return this.Create<TEntity>().WithBy(insertObjs).Execute();
    }
    public async Task<int> CreateAsync<TEntity>(object insertObjs, int bulkCount = 500, CancellationToken cancellationToken = default)
    {
        bool isMulti = insertObjs is IEnumerable && insertObjs is not string && insertObjs is not IDictionary<string, object>;
        if (isMulti) return await this.Create<TEntity>().WithBulk(insertObjs as IEnumerable, bulkCount).ExecuteAsync(cancellationToken);
        else return await this.Create<TEntity>().WithBy(insertObjs).ExecuteAsync(cancellationToken);
    }
    public long CreateLong<TEntity>(object insertObjs, int bulkCount = 500)
    {
        bool isMulti = insertObjs is IEnumerable && insertObjs is not string && insertObjs is not IDictionary<string, object>;
        if (isMulti) return this.Create<TEntity>().WithBulk(insertObjs as IEnumerable, bulkCount).ExecuteLong();
        else return this.Create<TEntity>().WithBy(insertObjs).ExecuteLong();
    }
    public async Task<long> CreateLongAsync<TEntity>(object insertObjs, int bulkCount = 500, CancellationToken cancellationToken = default)
    {
        bool isMulti = insertObjs is IEnumerable && insertObjs is not string && insertObjs is not IDictionary<string, object>;
        if (isMulti) return await this.Create<TEntity>().WithBulk(insertObjs as IEnumerable, bulkCount).ExecuteLongAsync(cancellationToken);
        else return await this.Create<TEntity>().WithBy(insertObjs).ExecuteLongAsync(cancellationToken);
    }
    #endregion

    #region Update
    public IUpdate<TEntity> Update<TEntity>() => new Update<TEntity>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, this.isParameterized);
    public int Update<TEntity>(object updateObjs, int bulkCount = 500)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));

        int result = 0;
        var entityType = typeof(TEntity);
        bool isMulti = updateObjs is IEnumerable && updateObjs is not string && updateObjs is not IDictionary<string, object>;
        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;

        if (isMulti)
        {
            int index = 0;
            var sqlBuilder = new StringBuilder();
            var entities = updateObjs as IEnumerable;
            var commandInitializer = RepositoryHelper.BuildUpdateBatchCommandInitializer(this.connection, this.OrmProvider, this.MapProvider, entityType, updateObjs);

            foreach (var updateObj in entities)
            {
                if (index > 0) sqlBuilder.Append(';');
                commandInitializer.Invoke(command, this.OrmProvider, this.MapProvider, sqlBuilder, index, updateObj);
                if (index >= bulkCount)
                {
                    command.CommandText = sqlBuilder.ToString();
                    this.connection.Open();
                    result += command.ExecuteNonQuery();
                    command.Parameters.Clear();
                    sqlBuilder.Clear();
                    index = 0;
                    continue;
                }
                index++;
            }
            if (index > 0)
            {
                command.CommandText = sqlBuilder.ToString();
                this.connection.Open();
                result += command.ExecuteNonQuery();
            }
            sqlBuilder.Clear();
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildUpdateEntitySqlParameters(this.connection, this.OrmProvider, this.MapProvider, entityType, updateObjs);
            command.CommandText = commandInitializer.Invoke(command, this.OrmProvider, this.MapProvider, updateObjs);
            this.connection.Open();
            result = command.ExecuteNonQuery();
        }
        command.Dispose();
        return result;
    }
    public async Task<int> UpdateAsync<TEntity>(object updateObjs, int bulkCount = 500, CancellationToken cancellationToken = default)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));

        int result = 0;
        var entityType = typeof(TEntity);
        bool isMulti = updateObjs is IEnumerable && updateObjs is not string && updateObjs is not IDictionary<string, object>;
        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        if (isMulti)
        {
            int index = 0;
            var sqlBuilder = new StringBuilder();
            var entities = updateObjs as IEnumerable;
            var commandInitializer = RepositoryHelper.BuildUpdateBatchCommandInitializer(this.connection, this.OrmProvider, this.MapProvider, entityType, updateObjs);

            foreach (var updateObj in entities)
            {
                if (index > 0) sqlBuilder.Append(';');
                commandInitializer.Invoke(command, this.OrmProvider, this.MapProvider, sqlBuilder, index, updateObj);
                if (index >= bulkCount)
                {
                    command.CommandText = sqlBuilder.ToString();
                    await this.connection.OpenAsync(cancellationToken);
                    result += await command.ExecuteNonQueryAsync(cancellationToken);
                    command.Parameters.Clear();
                    sqlBuilder.Clear();
                    index = 0;
                    continue;
                }
                index++;
            }
            if (index > 0)
            {
                command.CommandText = sqlBuilder.ToString();
                await this.connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(cancellationToken);
            }
            sqlBuilder.Clear();
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildUpdateEntitySqlParameters(this.connection, this.OrmProvider, this.MapProvider, entityType, updateObjs);
            command.CommandText = commandInitializer.Invoke(command, this.OrmProvider, this.MapProvider, updateObjs);
            await this.connection.OpenAsync(cancellationToken);
            result = await command.ExecuteNonQueryAsync(cancellationToken);
        }
        await command.DisposeAsync();
        return result;
    }
    public int Update<TEntity>(Expression<Func<TEntity, object>> fieldsSelectorOrAssignment, object updateObjs, int bulkCount = 500)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));

        int result = 0;
        var entityType = typeof(TEntity);
        bool isMulti = updateObjs is IEnumerable && updateObjs is not string && updateObjs is not IDictionary<string, object>;
        var visitor = this.OrmProvider.NewUpdateVisitor(this.DbKey, this.MapProvider, entityType, this.isParameterized);
        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;

        if (isMulti)
        {
            visitor.SetBulkFirst(fieldsSelectorOrAssignment, updateObjs);

            int index = 0;
            var sqlBuilder = new StringBuilder();
            var updateParameters = updateObjs as IEnumerable;
            foreach (var updateObj in updateParameters)
            {
                if (index > 0) sqlBuilder.Append(';');
                visitor.SetBulk(sqlBuilder, command, updateObj, index);

                if (index >= bulkCount)
                {
                    command.CommandText = sqlBuilder.ToString();
                    this.connection.Open();
                    result += command.ExecuteNonQuery();
                    command.Parameters.Clear();
                    sqlBuilder.Clear();
                    index = 0;
                    continue;
                }
                index++;
            }
            if (index > 0)
            {
                command.CommandText = sqlBuilder.ToString();
                this.connection.Open();
                result += command.ExecuteNonQuery();
            }
        }
        else
        {
            var sql = visitor.SetWith(fieldsSelectorOrAssignment, updateObjs)
                .WhereWith(updateObjs, true)
                .BuildSql(out var dbParameters);
            command.CommandText = sql;
            if (dbParameters != null && dbParameters.Count > 0)
                dbParameters.ForEach(f => command.Parameters.Add(f));

            this.connection.Open();
            result = command.ExecuteNonQuery();
        }
        command.Dispose();
        return result;
    }
    public async Task<int> UpdateAsync<TEntity>(Expression<Func<TEntity, object>> fieldsSelectorOrAssignment, object updateObjs, int bulkCount = 500, CancellationToken cancellationToken = default)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));

        int result = 0;
        var entityType = typeof(TEntity);
        bool isMulti = updateObjs is IEnumerable && updateObjs is not string && updateObjs is not IDictionary<string, object>;
        var visitor = this.OrmProvider.NewUpdateVisitor(this.DbKey, this.MapProvider, entityType, this.isParameterized);
        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        if (isMulti)
        {
            visitor.SetBulkFirst(fieldsSelectorOrAssignment, updateObjs);

            int index = 0;
            var sqlBuilder = new StringBuilder();
            var updateParameters = updateObjs as IEnumerable;
            foreach (var updateObj in updateParameters)
            {
                if (index > 0) sqlBuilder.Append(';');
                visitor.SetBulk(sqlBuilder, command, updateObj, index);

                if (index >= bulkCount)
                {
                    command.CommandText = sqlBuilder.ToString();
                    await this.connection.OpenAsync(cancellationToken);
                    result += await command.ExecuteNonQueryAsync(cancellationToken);
                    command.Parameters.Clear();
                    sqlBuilder.Clear();
                    index = 0;
                    continue;
                }
                index++;
            }
            if (index > 0)
            {
                command.CommandText = sqlBuilder.ToString();
                await this.connection.OpenAsync(cancellationToken);
                result += await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        else
        {
            var sql = visitor.WhereWith(updateObjs, true).BuildSql(out var dbParameters);
            command.CommandText = sql;
            if (dbParameters != null && dbParameters.Count > 0)
                dbParameters.ForEach(f => command.Parameters.Add(f));

            await this.connection.OpenAsync(cancellationToken);
            result = await command.ExecuteNonQueryAsync(cancellationToken);
        }
        await command.DisposeAsync();
        return result;
    }
    #endregion

    #region Delete
    public IDelete<TEntity> Delete<TEntity>() => new Delete<TEntity>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, this.isParameterized);
    public int Delete<TEntity>(object keys)
        => this.Delete<TEntity>().Where(keys).Execute();
    public async Task<int> DeleteAsync<TEntity>(object keys, CancellationToken cancellationToken = default)
        => await this.Delete<TEntity>().Where(keys).ExecuteAsync(cancellationToken);
    #endregion

    #region Exists
    public bool Exists<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildExistsSqlParameters(this.Connection, this.OrmProvider, this.MapProvider, entityType, whereObj);
        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;
        command.CommandText = commandInitializer.Invoke(command, this.OrmProvider, this.MapProvider, "", whereObj);

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

        var entityType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildExistsSqlParameters(this.Connection, this.OrmProvider, this.MapProvider, entityType, whereObj);
        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;
        cmd.CommandText = commandInitializer.Invoke(cmd, this.OrmProvider, this.MapProvider, "", whereObj);
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

        using var command = this.connection.CreateCommand();
        command.CommandText = rawSql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;
        if (parameters != null)
        {
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.Connection, this.OrmProvider, rawSql, parameters);
            commandInitializer.Invoke(command, this.OrmProvider, parameters);
        }

        this.connection.Open();
        var result = command.ExecuteNonQuery();
        command.Dispose();
        return result;
    }
    public async Task<int> ExecuteAsync(string rawSql, object parameters = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = rawSql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;
        if (parameters != null)
        {
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.Connection, this.OrmProvider, rawSql, parameters);
            commandInitializer.Invoke(cmd, this.OrmProvider, parameters);
        }

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        await command.DisposeAsync();
        return result;
    }
    #endregion

    #region QueryMultiple
    public IMultiQueryReader QueryMultiple(Action<IMultipleQuery> subQueries)
    {
        if (subQueries == null)
            throw new ArgumentNullException(nameof(subQueries));

        using var command = this.connection.CreateCommand();
        var multiQuery = new MultipleQuery(this.connection, this.OrmProvider, this.MapProvider, command, this.isParameterized);
        subQueries.Invoke(multiQuery);

        command.CommandText = multiQuery.BuildSql();
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;

        this.connection.Open();
        var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
        return new MultiQueryReader(command, reader, multiQuery.ReaderAfters);
    }
    public async Task<IMultiQueryReader> QueryMultipleAsync(Action<IMultipleQuery> subQueries, CancellationToken cancellationToken = default)
    {
        if (subQueries == null)
            throw new ArgumentNullException(nameof(subQueries));

        var cmd = this.connection.CreateCommand();
        var multiQuery = new MultipleQuery(this.connection, this.OrmProvider, this.MapProvider, cmd, this.isParameterized);
        subQueries.Invoke(multiQuery);

        cmd.CommandText = multiQuery.BuildSql();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");
        await this.connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
        return new MultiQueryReader(command, reader, multiQuery.ReaderAfters);
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
        GC.SuppressFinalize(this);
    }
    public async ValueTask DisposeAsync()
    {
        if (this.Transaction is DbTransaction dbTransaction)
            await dbTransaction.DisposeAsync();
        await this.connection?.DisposeAsync();
        this.Transaction = null;
        GC.SuppressFinalize(this);
    }
    ~Repository() => this.Dispose();
    #endregion
}
