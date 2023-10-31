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
    protected bool isParameterized = false;
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
    public Repository(string dbKey, IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider)
    {
        this.DbKey = dbKey;
        this.connection = new TheaConnection { DbKey = dbKey, BaseConnection = connection };
        this.OrmProvider = ormProvider;
        this.MapProvider = mapProvider;
    }
    public Repository(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider)
    {
        this.DbKey = connection.DbKey;
        this.connection = connection;
        this.OrmProvider = ormProvider;
        this.MapProvider = mapProvider;
    }
    #endregion

    #region From
    public IQuery<T> From<T>(char tableAsStart = 'a', string suffixRawSql = null)
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T), suffixRawSql);
        return new Query<T>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor);
    }
    public IQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2));
        return new Query<T1, T2>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor);
    }
    public IQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3));
        return new Query<T1, T2, T3>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor);
    }
    public IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new Query<T1, T2, T3, T4>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new Query<T1, T2, T3, T4, T5>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return new Query<T1, T2, T3, T4, T5, T6>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return new Query<T1, T2, T3, T4, T5, T6, T7>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor);
    }
    #endregion

    #region From SubQuery
    public IQuery<T> From<T>(Func<IFromQuery, IQuery<T>> subQuery, char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        var fromQuery = new FromQuery(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor);
        var query = subQuery.Invoke(fromQuery);
        var sql = query.Visitor.BuildSql(out var readerFields);
        if (!visitor.Equals(query.Visitor))
            visitor = query.Visitor;
        visitor.WithTable(typeof(T), sql, readerFields);
        return query;
    }
    #endregion

    #region FromWith
    public IQuery<T> FromWith<T>(IQuery<T> cteSubQuery, string cteTableName = null, char tableAsStart = 'a')
    {
        var visitor = cteSubQuery.Visitor;
        var rawSql = cteSubQuery.Visitor.BuildSql(out var readerFields);
        visitor.BuildCteTable(cteTableName, rawSql, readerFields, cteSubQuery, true);
        return cteSubQuery;
    }
    public IQuery<T> FromWith<T>(Func<IFromQuery, IQuery<T>> cteSubQuery, string cteTableName = null, char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart, true);
        var fromQuery = new FromQuery(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor);
        var query = cteSubQuery.Invoke(fromQuery);
        if (!visitor.Equals(query.Visitor))
            visitor = query.Visitor;
        var rawSql = visitor.BuildSql(out var readerFields, false);
        visitor.BuildCteTable(cteTableName, rawSql, readerFields, query, true);
        return query;
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
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.DbKey, this.OrmProvider, rawSql, parameters);
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
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.DbKey, this.OrmProvider, rawSql, parameters);
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
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, false);
        var typedCommandInitializer = commandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string>;
        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;
        command.CommandText = typedCommandInitializer.Invoke(command, this.OrmProvider, this.MapProvider, whereObj);

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
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, false);
        var typedCommandInitializer = commandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string>;
        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;
        cmd.CommandText = typedCommandInitializer.Invoke(cmd, this.OrmProvider, this.MapProvider, whereObj);

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
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.DbKey, this.OrmProvider, rawSql, parameters);
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
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.DbKey, this.OrmProvider, rawSql, parameters);
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
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, false);
        var typedCommandInitializer = commandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string>;

        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;
        command.CommandText = typedCommandInitializer.Invoke(command, this.OrmProvider, this.MapProvider, whereObj);

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
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, false);
        var typedCommandInitializer = commandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string>;

        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;
        cmd.CommandText = typedCommandInitializer.Invoke(cmd, this.OrmProvider, this.MapProvider, whereObj);

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
        var commandInitializer = RepositoryHelper.BuildGetSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, false);
        var typedCommandInitializer = commandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string>;
        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;
        command.CommandText = typedCommandInitializer.Invoke(command, this.OrmProvider, this.MapProvider, whereObj);

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
        var commandInitializer = RepositoryHelper.BuildGetSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, false);
        var typedCommandInitializer = commandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string>;

        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;
        cmd.CommandText = typedCommandInitializer.Invoke(cmd, this.OrmProvider, this.MapProvider, whereObj);
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
    public ICreate<TEntity> Create<TEntity>()
        => new Create<TEntity>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, this.isParameterized);
    #endregion

    #region Update
    public IUpdate<TEntity> Update<TEntity>() => new Update<TEntity>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, this.isParameterized);
    public int Update<TEntity>(object updateObjs, int bulkCount = 500)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));

        int result = 0;
        var entityType = typeof(TEntity);
        bool isBulk = updateObjs is IEnumerable && updateObjs is not string && updateObjs is not IDictionary<string, object>;
        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;

        if (isBulk)
        {
            int index = 0;
            var sqlBuilder = new StringBuilder();
            var entities = updateObjs as IEnumerable;
            var commandInitializer = RepositoryHelper.BuildUpdateBulkCommandInitializer(this.DbKey, this.OrmProvider, this.MapProvider, entityType, updateObjs);
            foreach (var updateObj in entities)
            {
                if (index > 0) sqlBuilder.Append(';');
                commandInitializer.Invoke(command, this.OrmProvider, this.MapProvider, sqlBuilder, updateObj, index);
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
            var commandInitializer = RepositoryHelper.BuildUpdateSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, updateObjs, false);
            var typedCommandInitializer = commandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string>;
            command.CommandText = typedCommandInitializer.Invoke(command, this.OrmProvider, this.MapProvider, updateObjs);
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
        bool isBulk = updateObjs is IEnumerable && updateObjs is not string && updateObjs is not IDictionary<string, object>;
        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        if (isBulk)
        {
            int index = 0;
            var sqlBuilder = new StringBuilder();
            var entities = updateObjs as IEnumerable;
            var commandInitializer = RepositoryHelper.BuildUpdateBulkCommandInitializer(this.DbKey, this.OrmProvider, this.MapProvider, entityType, updateObjs);

            foreach (var updateObj in entities)
            {
                if (index > 0) sqlBuilder.Append(';');
                commandInitializer.Invoke(command, this.OrmProvider, this.MapProvider, sqlBuilder, updateObj, index);
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
            var commandInitializer = RepositoryHelper.BuildUpdateSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, updateObjs, false);
            var typedCommandInitializer = commandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string>;
            command.CommandText = typedCommandInitializer.Invoke(command, this.OrmProvider, this.MapProvider, updateObjs);
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
        bool isBulk = updateObjs is IEnumerable && updateObjs is not string && updateObjs is not IDictionary<string, object>;
        var visitor = this.OrmProvider.NewUpdateVisitor(this.DbKey, this.MapProvider, this.isParameterized);
        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;

        if (isBulk)
        {
            int index = 0;
            var sqlBuilder = new StringBuilder();
            var updateParameters = updateObjs as IEnumerable;
            visitor.SetBulkFirst(command, fieldsSelectorOrAssignment, updateObjs);
            visitor.SetBulkHead(sqlBuilder);

            foreach (var updateObj in updateParameters)
            {
                if (index > 0) sqlBuilder.Append(';');
                visitor.SetBulk(sqlBuilder, updateObj, index);

                if (index >= bulkCount)
                {
                    visitor.SetBulkTail(sqlBuilder);
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
            visitor.SetWith(fieldsSelectorOrAssignment, updateObjs)
               .WhereWith(updateObjs).BuildCommand(command);

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
        bool isMulti = updateObjs is IEnumerable && updateObjs is not string && updateObjs is not IDictionary<string, object>;
        var visitor = this.OrmProvider.NewUpdateVisitor(this.DbKey, this.MapProvider, this.isParameterized);
        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        if (isMulti)
        {
            int index = 0;
            var sqlBuilder = new StringBuilder();
            var updateParameters = updateObjs as IEnumerable;
            visitor.SetBulkFirst(command, fieldsSelectorOrAssignment, updateObjs);
            visitor.SetBulkHead(sqlBuilder);

            foreach (var updateObj in updateParameters)
            {
                if (index > 0) sqlBuilder.Append(';');
                visitor.SetBulk(sqlBuilder, updateObj, index);

                if (index >= bulkCount)
                {
                    visitor.SetBulkTail(sqlBuilder);
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
            visitor.WhereWith(updateObjs).BuildCommand(command);
            await this.connection.OpenAsync(cancellationToken);
            result = await command.ExecuteNonQueryAsync(cancellationToken);
        }
        await command.DisposeAsync();
        return result;
    }
    #endregion

    #region Delete
    public IDelete<TEntity> Delete<TEntity>()
        => new Delete<TEntity>(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, this.isParameterized);
    #endregion

    #region Exists
    public bool Exists<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildExistsSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, false);
        var typedCommandInitializer = commandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string>;
        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;
        command.CommandText = typedCommandInitializer.Invoke(command, this.OrmProvider, this.MapProvider, whereObj);

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
        var commandInitializer = RepositoryHelper.BuildExistsSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, false);
        var typedCommandInitializer = commandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, object, string>;
        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;
        cmd.CommandText = typedCommandInitializer.Invoke(cmd, this.OrmProvider, this.MapProvider, whereObj);
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
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.DbKey, this.OrmProvider, rawSql, parameters);
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
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.DbKey, this.OrmProvider, rawSql, parameters);
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
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;

        using var multiQuery = new MultipleQuery(this.connection, command, this.OrmProvider, this.MapProvider, this.isParameterized);
        subQueries.Invoke(multiQuery);
        command.CommandText = multiQuery.BuildSql(out var readerAfters);

        this.connection.Open();
        var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
        return new MultiQueryReader(command, reader, readerAfters);
    }
    public async Task<IMultiQueryReader> QueryMultipleAsync(Action<IMultipleQuery> subQueries, CancellationToken cancellationToken = default)
    {
        if (subQueries == null)
            throw new ArgumentNullException(nameof(subQueries));

        var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;

        using var multiQuery = new MultipleQuery(this.connection, cmd, this.OrmProvider, this.MapProvider, this.isParameterized);
        subQueries.Invoke(multiQuery);
        cmd.CommandText = multiQuery.BuildSql(out var readerAfters);

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");
        await this.connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
        return new MultiQueryReader(command, reader, readerAfters);
    }
    #endregion

    #region MultipleExecute
    public void MultipleExecute(List<MultipleCommand> commands)
    {
        if (commands == null || commands.Count == 0)
            throw new ArgumentNullException(nameof(commands));

        int commandIndex = 0;
        var sqlBuilder = new StringBuilder();
        var visitors = new Dictionary<MultipleCommandType, object>();
        using var command = this.connection.CreateCommand();

        foreach (var multiCcommand in commands)
        {
            bool isFirst = false;
            if (!visitors.TryGetValue(multiCcommand.CommandType, out var visitor))
            {
                visitor = multiCcommand.CommandType switch
                {
                    MultipleCommandType.Insert => this.OrmProvider.NewCreateVisitor(this.DbKey, this.MapProvider, this.isParameterized),
                    MultipleCommandType.Update => this.OrmProvider.NewUpdateVisitor(this.DbKey, this.MapProvider, this.isParameterized),
                    MultipleCommandType.Delete => this.OrmProvider.NewDeleteVisitor(this.DbKey, this.MapProvider, this.isParameterized),
                    _ => this.OrmProvider.NewUpdateVisitor(this.DbKey, this.MapProvider, this.isParameterized)
                };
                visitors.Add(multiCcommand.CommandType, visitor);
                isFirst = true;
            }
            switch (multiCcommand.CommandType)
            {
                case MultipleCommandType.Insert:
                    var insertVisitor = visitor as ICreateVisitor;
                    insertVisitor.Initialize(multiCcommand.EntityType, isFirst);
                    commandIndex += insertVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                    break;
                case MultipleCommandType.Update:
                    var updateVisitor = visitor as IUpdateVisitor;
                    updateVisitor.Initialize(multiCcommand.EntityType, isFirst);
                    commandIndex += updateVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                    break;
                case MultipleCommandType.Delete:
                    var deleteVisitor = visitor as IDeleteVisitor;
                    deleteVisitor.Initialize(multiCcommand.EntityType, isFirst);
                    commandIndex += deleteVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                    break;
            }
        }
        command.CommandText = sqlBuilder.ToString();
        command.CommandType = CommandType.Text;
        command.Transaction = this.Transaction;
        this.connection.Open();
        var result = command.ExecuteNonQuery();
        command.Dispose();
    }
    public async Task MultipleExecuteAsync(List<MultipleCommand> commands, CancellationToken cancellationToken = default)
    {
        if (commands == null || commands.Count == 0)
            throw new ArgumentNullException(nameof(commands));

        int commandIndex = 0;
        var sqlBuilder = new StringBuilder();
        var visitors = new Dictionary<MultipleCommandType, object>();
        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        foreach (var multiCcommand in commands)
        {
            bool isFirst = false;
            if (!visitors.TryGetValue(multiCcommand.CommandType, out var visitor))
            {
                visitor = multiCcommand.CommandType switch
                {
                    MultipleCommandType.Insert => this.OrmProvider.NewCreateVisitor(this.DbKey, this.MapProvider, this.isParameterized),
                    MultipleCommandType.Update => this.OrmProvider.NewUpdateVisitor(this.DbKey, this.MapProvider, this.isParameterized),
                    MultipleCommandType.Delete => this.OrmProvider.NewDeleteVisitor(this.DbKey, this.MapProvider, this.isParameterized),
                    _ => this.OrmProvider.NewUpdateVisitor(this.DbKey, this.MapProvider, this.isParameterized)
                };
                visitors.Add(multiCcommand.CommandType, visitor);
                isFirst = true;
            }
            switch (multiCcommand.CommandType)
            {
                case MultipleCommandType.Insert:
                    var insertVisitor = visitor as ICreateVisitor;
                    insertVisitor.Initialize(multiCcommand.EntityType, isFirst);
                    commandIndex += insertVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                    break;
                case MultipleCommandType.Update:
                    var updateVisitor = visitor as IUpdateVisitor;
                    updateVisitor.Initialize(multiCcommand.EntityType, isFirst);
                    commandIndex += updateVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                    break;
                case MultipleCommandType.Delete:
                    var deleteVisitor = visitor as IDeleteVisitor;
                    deleteVisitor.Initialize(multiCcommand.EntityType, isFirst);
                    commandIndex += deleteVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                    break;
            }
        }
        cmd.CommandText = sqlBuilder.ToString();
        await this.connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        await command.DisposeAsync();
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

    private IQueryVisitor CreateQueryVisitor(char tableAsStart, bool isCteQuery = false)
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        if (isCteQuery)
        {
            visitor.CteTables = new();
            visitor.CteQueries = new();
            visitor.CteTableSegments = new();
        }
        return visitor;
    }
    #endregion
}