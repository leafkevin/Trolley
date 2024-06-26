﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public sealed class DbContext : IDisposable, IAsyncDisposable
{
    #region Properties
    public string DbKey { get; set; }
    public IDbConnection Connection { get; set; }
    public string ConnectionString { get; set; }
    public IOrmProvider OrmProvider { get; set; }
    public IEntityMapProvider MapProvider { get; set; }
    public IShardingProvider ShardingProvider { get; set; }
    public IDbTransaction Transaction { get; set; }
    public bool IsParameterized { get; set; }
    public int CommandTimeout { get; set; }
    public bool IsNeedClose => this.Transaction == null;
    #endregion

    #region CreateCommand
    public IDbCommand CreateCommand()
    {
        var command = this.Connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandTimeout = this.CommandTimeout;
        command.Transaction = this.Transaction;
        return command;
    }
    public DbCommand CreateDbCommand()
    {
        var cmd = this.Connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.Transaction;
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");
        return command;
    }
    #endregion

    #region QueryFirst
    public TResult QueryFirst<TResult>(Action<IDbCommand> commandInitializer)
    {
        using var command = this.CreateCommand();
        TResult result = default;
        IDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            var entityType = typeof(TResult);
            commandInitializer.Invoke(command);
            this.Open();
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            reader = command.ExecuteReader(behavior);
            if (reader.Read())
            {
                if (entityType.IsEntityType(out _))
                    result = reader.To<TResult>(this.OrmProvider, this.MapProvider);
                else result = reader.To<TResult>(this.OrmProvider);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            reader?.Dispose();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<TResult> QueryFirstAsync<TResult>(Action<IDbCommand> commandInitializer, CancellationToken cancellationToken = default)
    {
        using var command = this.CreateDbCommand();
        TResult result = default;
        DbDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            var entityType = typeof(TResult);
            commandInitializer.Invoke(command);

            await this.OpenAsync(cancellationToken);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                if (entityType.IsEntityType(out _))
                    result = reader.To<TResult>(this.OrmProvider, this.MapProvider);
                else result = reader.To<TResult>(this.OrmProvider);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            if (reader != null)
                await reader.DisposeAsync();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    public TResult QueryFirst<TResult>(IQueryVisitor visitor)
    {
        using var command = this.CreateCommand();
        TResult result = default;
        IDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            if (visitor.IsNeedFetchShardingTables)
                this.FetchShardingTables(visitor as SqlVisitor);
            Expression<Func<TResult, TResult>> defaultExpr = f => f;
            visitor.SelectDefault(defaultExpr);
            var sql = visitor.BuildSql(out var readerFields);
            (var isOpened, sql) = this.BuildSql(visitor, sql, " UNOIN ALL ");
            command.CommandText = sql;
            visitor.DbParameters.CopyTo(command.Parameters);

            if (!isOpened) this.Open();
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            reader = command.ExecuteReader(behavior);
            var entityType = typeof(TResult);
            if (reader.Read())
            {
                if (entityType.IsEntityType(out _))
                    result = reader.To<TResult>(this.OrmProvider, readerFields);
                else result = reader.To<TResult>(this.OrmProvider);
            }
            if (visitor.BuildIncludeSql(entityType, result, out sql))
            {
                reader.Dispose();
                command.CommandText = sql;
                command.Parameters.Clear();
                visitor.NextDbParameters.CopyTo(command.Parameters);
                reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                visitor.SetIncludeValues(entityType, result, reader);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            reader?.Close();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close();
            visitor.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<TResult> QueryFirstAsync<TResult>(IQueryVisitor visitor, CancellationToken cancellationToken = default)
    {
        using var command = this.CreateDbCommand();
        TResult result = default;
        DbDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            if (visitor.IsNeedFetchShardingTables)
                await this.FetchShardingTablesAsync(visitor as SqlVisitor, cancellationToken);
            Expression<Func<TResult, TResult>> defaultExpr = f => f;
            visitor.SelectDefault(defaultExpr);
            var sql = visitor.BuildSql(out var readerFields);
            (var isOpened, sql) = this.BuildSql(visitor, sql, " UNION ALL ");
            command.CommandText = sql;
            visitor.DbParameters.CopyTo(command.Parameters);

            if (!isOpened) await this.OpenAsync(cancellationToken);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            var entityType = typeof(TResult);
            if (await reader.ReadAsync(cancellationToken))
            {
                if (entityType.IsEntityType(out _))
                    result = reader.To<TResult>(this.OrmProvider, readerFields);
                else result = reader.To<TResult>(this.OrmProvider);
            }
            if (visitor.BuildIncludeSql(entityType, result, out sql))
            {
                await reader.DisposeAsync();
                command.CommandText = sql;
                command.Parameters.Clear();
                visitor.NextDbParameters.CopyTo(command.Parameters);
                reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
                await visitor.SetIncludeValuesAsync(entityType, result, reader, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            if (reader != null)
                await reader.DisposeAsync();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
            visitor.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region Query
    public List<TResult> Query<TResult>(Action<IDbCommand> commandInitializer)
    {
        using var command = this.CreateCommand();
        var result = new List<TResult>();
        IDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            var entityType = typeof(TResult);
            commandInitializer.Invoke(command);

            this.Open();
            var behavior = CommandBehavior.SequentialAccess;
            reader = command.ExecuteReader(behavior);

            if (entityType.IsEntityType(out _))
            {
                while (reader.Read())
                {
                    result.Add(reader.To<TResult>(this.OrmProvider, this.MapProvider));
                }
            }
            else
            {
                while (reader.Read())
                {
                    result.Add(reader.To<TResult>(this.OrmProvider));
                }
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            reader?.Dispose();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<List<TResult>> QueryAsync<TResult>(Action<IDbCommand> commandInitializer, CancellationToken cancellationToken = default)
    {
        using var command = this.CreateDbCommand();
        var result = new List<TResult>();
        DbDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            var entityType = typeof(TResult);
            commandInitializer.Invoke(command);
            await this.OpenAsync(cancellationToken);
            var behavior = CommandBehavior.SequentialAccess;
            reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            if (entityType.IsEntityType(out _))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    result.Add(reader.To<TResult>(this.OrmProvider, this.MapProvider));
                }
            }
            else
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    result.Add(reader.To<TResult>(this.OrmProvider));
                }
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            if (reader != null)
                await reader.DisposeAsync();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    public List<TResult> Query<TResult>(IQueryVisitor visitor)
    {
        using var command = this.CreateCommand();
        var result = new List<TResult>();
        IDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            if (visitor.IsNeedFetchShardingTables)
                this.FetchShardingTables(visitor as SqlVisitor);
            Expression<Func<TResult, TResult>> defaultExpr = f => f;
            visitor.SelectDefault(defaultExpr);
            var sql = visitor.BuildSql(out var readerFields);
            (var isOpened, sql) = this.BuildSql(visitor, sql, " UNION ALL ");
            command.CommandText = sql;
            visitor.DbParameters.CopyTo(command.Parameters);

            if (!isOpened) this.Open();
            var behavior = CommandBehavior.SequentialAccess;
            reader = command.ExecuteReader(behavior);

            var entityType = typeof(TResult);
            if (entityType.IsEntityType(out _))
            {
                while (reader.Read())
                {
                    result.Add(reader.To<TResult>(this.OrmProvider, readerFields));
                }
            }
            else
            {
                while (reader.Read())
                {
                    result.Add(reader.To<TResult>(this.OrmProvider));
                }
            }
            if (visitor.BuildIncludeSql(entityType, result, out sql))
            {
                reader.Dispose();
                command.CommandText = sql;
                command.Parameters.Clear();
                visitor.NextDbParameters.CopyTo(command.Parameters);
                reader = command.ExecuteReader(behavior);
                visitor.SetIncludeValues(entityType, result, reader);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            reader?.Dispose();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close();
            visitor.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<List<TResult>> QueryAsync<TResult>(IQueryVisitor visitor, CancellationToken cancellationToken = default)
    {
        using var command = this.CreateDbCommand();
        var result = new List<TResult>();
        DbDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            if (visitor.IsNeedFetchShardingTables)
                await this.FetchShardingTablesAsync(visitor as SqlVisitor, cancellationToken);

            Expression<Func<TResult, TResult>> defaultExpr = f => f;
            visitor.SelectDefault(defaultExpr);
            var sql = visitor.BuildSql(out var readerFields);
            (var isOpened, sql) = this.BuildSql(visitor, sql, " UNION ALL ");
            command.CommandText = sql;
            visitor.DbParameters.CopyTo(command.Parameters);

            if (!isOpened) await this.OpenAsync(cancellationToken);
            var behavior = CommandBehavior.SequentialAccess;
            reader = await command.ExecuteReaderAsync(behavior, cancellationToken);

            var entityType = typeof(TResult);
            if (entityType.IsEntityType(out _))
            {
                while (reader.Read())
                {
                    result.Add(reader.To<TResult>(this.OrmProvider, readerFields));
                }
            }
            else
            {
                while (reader.Read())
                {
                    result.Add(reader.To<TResult>(this.OrmProvider));
                }
            }
            if (visitor.BuildIncludeSql(entityType, result, out sql))
            {
                await reader.DisposeAsync();
                command.CommandText = sql;
                command.Parameters.Clear();
                visitor.NextDbParameters.CopyTo(command.Parameters);
                reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
                await visitor.SetIncludeValuesAsync(entityType, result, reader, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            if (reader != null)
                await reader.DisposeAsync();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
            visitor.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region QueryPage
    public IPagedList<TResult> QueryPage<TResult>(IQueryVisitor visitor)
    {
        using var command = this.CreateCommand();
        var result = new PagedList<TResult> { Data = new List<TResult>() };
        IDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            Expression<Func<TResult, TResult>> defaultExpr = f => f;
            visitor.SelectDefault(defaultExpr);
            command.CommandText = visitor.BuildSql(out var readerFields);
            visitor.DbParameters.CopyTo(command.Parameters);

            this.Open();
            var behavior = CommandBehavior.SequentialAccess;
            reader = command.ExecuteReader(behavior);
            if (reader.Read()) result.TotalCount = reader.To<int>(this.OrmProvider);
            result.PageNumber = visitor.PageNumber;
            result.PageSize = visitor.PageSize;

            reader.NextResult();
            var entityType = typeof(TResult);
            if (entityType.IsEntityType(out _))
            {
                while (reader.Read())
                {
                    result.Data.Add(reader.To<TResult>(this.OrmProvider, readerFields));
                }
            }
            else
            {
                while (reader.Read())
                {
                    result.Data.Add(reader.To<TResult>(this.OrmProvider));
                }
            }
            result.Count = result.Data.Count;
            if (visitor.BuildIncludeSql(entityType, result.Data, out var sql))
            {
                reader.Dispose();
                command.CommandText = sql;
                command.Parameters.Clear();
                visitor.NextDbParameters.CopyTo(command.Parameters);
                reader = command.ExecuteReader(behavior);
                visitor.SetIncludeValues(entityType, result, reader);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            reader?.Dispose();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close();
            visitor.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<IPagedList<TResult>> QueryPageAsync<TResult>(IQueryVisitor visitor, CancellationToken cancellationToken = default)
    {
        using var command = this.CreateDbCommand();
        var result = new PagedList<TResult> { Data = new List<TResult>() };
        DbDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            Expression<Func<TResult, TResult>> defaultExpr = f => f;
            visitor.SelectDefault(defaultExpr);
            command.CommandText = visitor.BuildSql(out var readerFields);
            visitor.DbParameters.CopyTo(command.Parameters);

            await this.OpenAsync(cancellationToken);
            var behavior = CommandBehavior.SequentialAccess;
            reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            if (await reader.ReadAsync(cancellationToken)) result.TotalCount = reader.To<int>(this.OrmProvider);
            result.PageNumber = visitor.PageNumber;
            result.PageSize = visitor.PageSize;

            await reader.NextResultAsync(cancellationToken);
            var entityType = typeof(TResult);
            if (entityType.IsEntityType(out _))
            {
                while (reader.Read())
                {
                    result.Data.Add(reader.To<TResult>(this.OrmProvider, readerFields));
                }
            }
            else
            {
                while (reader.Read())
                {
                    result.Data.Add(reader.To<TResult>(this.OrmProvider));
                }
            }
            result.Count = result.Data.Count;
            if (visitor.BuildIncludeSql(entityType, result.Data, out var sql))
            {
                await reader.DisposeAsync();
                command.CommandText = sql;
                command.Parameters.Clear();
                visitor.NextDbParameters.CopyTo(command.Parameters);
                reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
                await visitor.SetIncludeValuesAsync(entityType, result.Data, reader, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            if (reader != null)
                await reader.DisposeAsync();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
            visitor.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region Get
    public TEntity Get<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        using var command = this.CreateCommand();
        TEntity result = default;
        IDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildGetSqlParameters(this.OrmProvider, this.MapProvider, entityType, whereObj, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this.OrmProvider, whereObj);

            this.Open();
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            reader = command.ExecuteReader(behavior);
            if (reader.Read())
                result = reader.To<TEntity>(this.OrmProvider, this.MapProvider);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            reader?.Dispose();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<TEntity> GetAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        using var command = this.CreateDbCommand();
        TEntity result = default;
        DbDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildGetSqlParameters(this.OrmProvider, this.MapProvider, entityType, whereObj, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this.OrmProvider, whereObj);

            await this.OpenAsync(cancellationToken);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            reader = await command.ExecuteReaderAsync(behavior, cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
                result = reader.To<TEntity>(this.OrmProvider, this.MapProvider);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            if (reader != null)
                await reader.DisposeAsync();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region Create
    public int Create(IDbCommand command, Type entityType, object insertObj)
    {
        this.BuildCreateCommand(command, entityType, insertObj, false);
        this.Open();
        return command.ExecuteNonQuery();
    }
    public async Task<int> CreateAsync(DbCommand command, Type entityType, object insertObj, CancellationToken cancellationToken = default)
    {
        this.BuildCreateCommand(command, entityType, insertObj, false);
        await this.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
    public TResult CreateResult<TResult>(Func<IDbCommand, DbContext, List<ReaderField>> commandInitializer)
    {
        TResult result = default;
        IDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        using var command = this.CreateCommand();
        try
        {
            var readerFields = commandInitializer.Invoke(command, this);
            this.Open();
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            reader = command.ExecuteReader(behavior);
            if (reader.Read())
            {
                if (readerFields != null && readerFields.Count > 0)
                    result = reader.To<TResult>(this.OrmProvider, readerFields);
                else result = reader.To<TResult>(this.OrmProvider);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            reader?.Dispose();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<TResult> CreateResultAsync<TResult>(Func<IDbCommand, DbContext, List<ReaderField>> commandInitializer, CancellationToken cancellationToken = default)
    {
        TResult result = default;
        DbDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        using var command = this.CreateDbCommand();
        try
        {
            var readerFields = commandInitializer.Invoke(command, this);
            await this.OpenAsync(cancellationToken);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                if (readerFields != null && readerFields.Count > 0)
                    result = reader.To<TResult>(this.OrmProvider, readerFields);
                else result = reader.To<TResult>(this.OrmProvider);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            if (reader != null)
                await reader.DisposeAsync();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    public List<TResult> CreateResult<TResult>(ICreateVisitor visitor)
    {
        var result = new List<TResult>();
        IDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        using var command = this.CreateCommand();
        try
        {
            command.CommandText = visitor.BuildCommand(command, false, out var readerFields);
            this.Open();
            reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
            while (reader.Read())
            {
                result.Add(reader.To<TResult>(this.OrmProvider, readerFields));
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            reader?.Dispose();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<List<TResult>> CreateResultAsync<TResult>(ICreateVisitor visitor, CancellationToken cancellationToken = default)
    {
        var result = new List<TResult>();
        DbDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        using var command = this.CreateDbCommand();
        try
        {
            command.CommandText = visitor.BuildCommand(command, false, out var readerFields);
            await this.OpenAsync(cancellationToken);
            reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<TResult>(this.OrmProvider, readerFields));
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            if (reader != null)
                await reader.DisposeAsync();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
        }
        if (exception != null) throw exception;
        return result;
    }

    public int CreateBulk(IDbCommand command, Type entityType, IEnumerable insertObjs, int bulkCount)
    {
        var builder = new StringBuilder();
        var entities = insertObjs as IEnumerable;
        object firstInsertObj = null;
        Type insertObjType = null;
        foreach (var entity in entities)
        {
            firstInsertObj = entity;
            break;
        }
        insertObjType = firstInsertObj.GetType();

        (var tableName, var headSqlSetter, var valuesSqlSetter) = RepositoryHelper.BuildCreateSqlParameters(
            this.OrmProvider, this.MapProvider, entityType, insertObjType, null, null, true, false);
        var typedValuesSqlSetter = valuesSqlSetter as Action<IDataParameterCollection, StringBuilder, object, string>;
        Action<string, object> firstExecutor = (tableName, insertObj) =>
        {
            headSqlSetter.Invoke(builder, tableName, insertObj);
            builder.Append(" VALUES");
        };
        Func<string, IEnumerable, int> executor = (tableName, insertObjs) =>
        {
            var isOpened = false;
            int count = 0, index = 0;
            foreach (var insertObj in insertObjs)
            {
                if (index > 0) builder.Append(',');
                typedValuesSqlSetter.Invoke(command.Parameters, builder, insertObj, index.ToString());
                if (index >= bulkCount)
                {
                    command.CommandText = builder.ToString();
                    if (!isOpened)
                    {
                        this.Open();
                        isOpened = true;
                    }
                    count += command.ExecuteNonQuery();
                    builder.Clear();
                    command.Parameters.Clear();
                    firstExecutor.Invoke(tableName, insertObj);
                    index = 0;
                    continue;
                }
                index++;
            }
            if (index > 0)
            {
                command.CommandText = builder.ToString();
                if (!isOpened) this.Open();
                count += command.ExecuteNonQuery();
            }
            return count;
        };

        int result = 0;
        if (this.ShardingProvider.TryGetShardingTable(entityType, out _))
        {
            var tabledInsertObjs = this.SplitShardingParameters(entityType, entities);
            foreach (var tabledInsertObj in tabledInsertObjs)
            {
                firstExecutor.Invoke(tabledInsertObj.Key, tabledInsertObj);
                result += executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
                builder.Clear();
                command.Parameters.Clear();
            }
        }
        else
        {
            firstExecutor.Invoke(tableName, firstInsertObj);
            result = executor.Invoke(tableName, entities);
        }
        builder.Clear();
        builder = null;
        return result;
    }
    public async Task<int> CreateBulkAsync(DbCommand command, Type entityType, IEnumerable insertObjs, int bulkCount, CancellationToken cancellationToken = default)
    {
        var builder = new StringBuilder();
        var entities = insertObjs as IEnumerable;
        object firstInsertObj = null;
        Type insertObjType = null;
        foreach (var entity in entities)
        {
            firstInsertObj = entity;
            break;
        }
        insertObjType = firstInsertObj.GetType();

        (var tableName, var headSqlSetter, var valuesSqlSetter) = RepositoryHelper.BuildCreateSqlParameters(
            this.OrmProvider, this.MapProvider, entityType, insertObjType, null, null, true, false);
        var typedValuesSqlSetter = valuesSqlSetter as Action<IDataParameterCollection, StringBuilder, object, string>;
        Action<string, object> firstExecutor = (tableName, insertObj) =>
        {
            headSqlSetter.Invoke(builder, tableName, insertObj);
            builder.Append(" VALUES");
        };
        Func<string, IEnumerable, Task<int>> executor = async (tableName, insertObjs) =>
        {
            var isOpened = false;
            int count = 0, index = 0;
            foreach (var insertObj in insertObjs)
            {
                if (index > 0) builder.Append(',');
                typedValuesSqlSetter.Invoke(command.Parameters, builder, insertObj, index.ToString());
                if (index >= bulkCount)
                {
                    command.CommandText = builder.ToString();
                    if (!isOpened)
                    {
                        await this.OpenAsync(cancellationToken);
                        isOpened = true;
                    }
                    count += await command.ExecuteNonQueryAsync(cancellationToken);
                    builder.Clear();
                    command.Parameters.Clear();
                    firstExecutor.Invoke(tableName, insertObj);
                    index = 0;
                    continue;
                }
                index++;
            }
            if (index > 0)
            {
                command.CommandText = builder.ToString();
                if (!isOpened) await this.OpenAsync(cancellationToken);
                count += await command.ExecuteNonQueryAsync(cancellationToken);
            }
            return count;
        };

        int result = 0;
        if (this.ShardingProvider.TryGetShardingTable(entityType, out _))
        {
            var tabledInsertObjs = this.SplitShardingParameters(entityType, entities);
            foreach (var tabledInsertObj in tabledInsertObjs)
            {
                firstExecutor.Invoke(tabledInsertObj.Key, tabledInsertObj);
                result += await executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
                builder.Clear();
                command.Parameters.Clear();
            }
        }
        else
        {
            firstExecutor.Invoke(tableName, firstInsertObj);
            result = await executor.Invoke(tableName, entities);
        }
        builder.Clear();
        builder = null;
        return result;
    }
    public int CreateBulk(ICreateVisitor visitor)
    {
        using var command = this.CreateCommand();
        var sqlBuilder = new StringBuilder();
        (var isNeedSplit, var tableName, var insertObjs, var bulkCount, var firstInsertObj,
            var headSqlSetter, var valuesSqlSetter, _) = visitor.BuildWithBulk(command);

        Action<string, object> clearCommand = (tableName, insertObj) =>
        {
            sqlBuilder.Clear();
            command.Parameters.Clear();
            headSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName, insertObj);
        };
        Func<string, IEnumerable, int> executor = (tableName, insertObjs) =>
        {
            var isOpened = false;
            int count = 0, index = 0;
            foreach (var insertObj in insertObjs)
            {
                if (index > 0) sqlBuilder.Append(',');
                valuesSqlSetter.Invoke(command.Parameters, sqlBuilder, insertObj, index.ToString());
                if (index >= bulkCount)
                {
                    command.CommandText = sqlBuilder.ToString();
                    if (!isOpened)
                    {
                        this.Open();
                        isOpened = true;
                    }
                    count += command.ExecuteNonQuery();
                    clearCommand.Invoke(tableName, insertObj);
                    index = 0;
                    continue;
                }
                index++;
            }
            if (index > 0)
            {
                command.CommandText = sqlBuilder.ToString();
                if (!isOpened) this.Open();
                count += command.ExecuteNonQuery();
            }
            return count;
        };
        int result = 0;
        if (isNeedSplit)
        {
            var entityType = visitor.Tables[0].EntityType;
            var tabledInsertObjs = this.SplitShardingParameters(entityType, insertObjs);
            foreach (var tabledInsertObj in tabledInsertObjs)
            {
                headSqlSetter.Invoke(command.Parameters, sqlBuilder, tabledInsertObj.Key, tabledInsertObj);
                result += executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
            }
        }
        else
        {
            headSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName, firstInsertObj);
            result = executor.Invoke(tableName, insertObjs);
        }
        sqlBuilder.Clear();
        sqlBuilder = null;
        return result;
    }
    public async Task<int> CreateBulkAsync(ICreateVisitor visitor, CancellationToken cancellationToken = default)
    {
        using var command = this.CreateDbCommand();
        var sqlBuilder = new StringBuilder();
        (var isNeedSplit, var tableName, var insertObjs, var bulkCount, var firstInsertObj,
            var headSqlSetter, var valuesSqlSetter, _) = visitor.BuildWithBulk(command);

        Action<string, object> clearCommand = (tableName, insertObj) =>
        {
            sqlBuilder.Clear();
            command.Parameters.Clear();
            headSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName, insertObj);
        };
        Func<string, IEnumerable, Task<int>> executor = async (tableName, insertObjs) =>
        {
            var isOpened = false;
            int count = 0, index = 0;
            foreach (var insertObj in insertObjs)
            {
                if (index > 0) sqlBuilder.Append(',');
                valuesSqlSetter.Invoke(command.Parameters, sqlBuilder, insertObj, index.ToString());
                if (index >= bulkCount)
                {
                    command.CommandText = sqlBuilder.ToString();
                    if (!isOpened)
                    {
                        await this.OpenAsync(cancellationToken);
                        isOpened = true;
                    }
                    count += await command.ExecuteNonQueryAsync(cancellationToken);
                    clearCommand.Invoke(tableName, insertObj);
                    index = 0;
                    continue;
                }
                index++;
            }
            if (index > 0)
            {
                command.CommandText = sqlBuilder.ToString();
                if (!isOpened) await this.OpenAsync(cancellationToken);
                count += await command.ExecuteNonQueryAsync(cancellationToken);
            }
            return count;
        };
        int result = 0;
        if (isNeedSplit)
        {
            var entityType = visitor.Tables[0].EntityType;
            var tabledInsertObjs = this.SplitShardingParameters(entityType, insertObjs);
            foreach (var tabledInsertObj in tabledInsertObjs)
            {
                headSqlSetter.Invoke(command.Parameters, sqlBuilder, tabledInsertObj.Key, tabledInsertObj);
                result += await executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
            }
        }
        else
        {
            headSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName, firstInsertObj);
            result = await executor.Invoke(tableName, insertObjs);
        }
        sqlBuilder.Clear();
        sqlBuilder = null;
        return result;
    }
    public void BuildCreateCommand(IDbCommand command, Type entityType, object insertObj, bool isReturnIdentity)
    {
        var insertObjType = insertObj.GetType();
        (var tableName, var headSqlSetter, var valuesSqlSetter) = RepositoryHelper.BuildCreateSqlParameters(
            this.OrmProvider, this.MapProvider, entityType, insertObjType, null, null, false, isReturnIdentity);
        var builder = new StringBuilder();
        if (this.ShardingProvider.TryGetShardingTable(entityType, out _))
            tableName = this.GetShardingTableName(entityType, insertObjType, insertObj);
        var typedValuesSqlSetter = valuesSqlSetter as Action<IDataParameterCollection, StringBuilder, object>;
        headSqlSetter.Invoke(builder, tableName, insertObj);
        builder.Append(" VALUES");
        typedValuesSqlSetter.Invoke(command.Parameters, builder, insertObj);
        command.CommandText = builder.ToString();
    }
    #endregion

    #region Execute
    public int Execute(Func<IDbCommand, bool> commandInitializer)
    {
        using var command = this.CreateCommand();
        int result = 0;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            if (!commandInitializer.Invoke(command))
                this.Open();
            result = command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<int> ExecuteAsync(Func<IDbCommand, bool> commandInitializer, CancellationToken cancellationToken = default)
    {
        using var command = this.CreateDbCommand();
        int result = 0;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            if (!commandInitializer.Invoke(command))
                await this.OpenAsync(cancellationToken);
            result = await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion   

    #region Others
    public void Open()
    {
        if (this.Connection.State == ConnectionState.Broken)
            this.Connection.Close();
        if (this.Connection.State == ConnectionState.Closed)
        {
            //关闭后，连接串被重置，需要重新设置
            this.Connection.ConnectionString = this.ConnectionString;
            this.Connection.Open();
        }
    }
    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        if (this.Connection is not DbConnection connection)
            throw new NotSupportedException("当前数据库驱动不支持异步操作");
        if (connection.State == ConnectionState.Broken)
            await connection.CloseAsync();
        if (connection.State == ConnectionState.Closed)
        {
            //关闭后，连接串被重置，需要重新设置
            connection.ConnectionString = this.ConnectionString;
            await connection.OpenAsync(cancellationToken);
        }
    }
    public IDbTransaction BeginTransaction()
    {
        this.Open();
        this.Transaction = this.Connection.BeginTransaction();
        return this.Transaction;
    }
    public async ValueTask<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await this.OpenAsync(cancellationToken);
        if (this.Connection is DbConnection connection)
            this.Transaction = await connection.BeginTransactionAsync(cancellationToken);
        else throw new NotSupportedException("当前数据库驱动不支持异步操作");
        return this.Transaction;
    }
    public void Commit()
    {
        this.Transaction?.Commit();
        this.Close();
        this.Transaction = null;
    }
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (this.Transaction != null)
        {
            if (this.Transaction is not DbTransaction dbTransaction)
                throw new NotSupportedException("当前数据库驱动不支持异步操作");
            await dbTransaction.CommitAsync(cancellationToken);
        }
        await this.CloseAsync();
        this.Transaction = null;
    }
    public void Rollback()
    {
        this.Transaction?.Rollback();
        this.Close();
        this.Transaction = null;
    }
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (this.Transaction != null)
        {
            if (this.Transaction is not DbTransaction dbTransaction)
                throw new NotSupportedException("当前数据库驱动不支持异步操作");
            await dbTransaction.RollbackAsync(cancellationToken);
        }
        await this.CloseAsync();
        this.Transaction = null;
    }
    public void Close() => this.Connection?.Close();
    public async Task CloseAsync()
    {
        if (this.Connection is not DbConnection connection)
            throw new NotSupportedException("当前数据库驱动不支持异步操作");
        await connection?.CloseAsync();
    }
    public void Dispose()
    {
        this.Close();
        this.Connection = null;
    }
    public async ValueTask DisposeAsync()
    {
        await this.CloseAsync();
        this.Connection = null;
    }
    #endregion

    public (bool, string) BuildSql(IQueryVisitor visitor, string formatSql, string jointMark)
    {
        var sql = formatSql;
        if (visitor.ShardingTables != null && visitor.ShardingTables.Count > 0)
            sql = this.BuildShardingTablesSqlByFormat(visitor as SqlVisitor, formatSql, jointMark);
        return (visitor.IsNeedFetchShardingTables, sql);
    }
    public void FetchShardingTables(SqlVisitor visitor)
    {
        var fetchSql = visitor.BuildShardingTablesSql(this.Connection.Database);
        using var command = this.CreateCommand();
        command.CommandText = fetchSql;
        this.Open();
        var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
        var shardingTables = new List<string>();
        while (reader.Read())
        {
            shardingTables.Add(reader.To<string>(this.OrmProvider));
        }
        reader.Dispose();
        command.Parameters.Clear();
        command.Dispose();
        visitor.SetShardingTables(shardingTables);
    }
    public async Task FetchShardingTablesAsync(SqlVisitor visitor, CancellationToken cancellationToken = default)
    {
        var fetchSql = visitor.BuildShardingTablesSql(this.Connection.Database);
        using var command = this.CreateDbCommand();
        command.CommandText = fetchSql;
        command.CommandText = fetchSql;
        command.Parameters.Clear();
        await this.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
        var shardingTables = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            shardingTables.Add(reader.To<string>(this.OrmProvider));
        }
        await reader.DisposeAsync();
        command.Parameters.Clear();
        await command.DisposeAsync();
        visitor.SetShardingTables(shardingTables);
    }
    public Dictionary<string, List<object>> SplitShardingParameters(Type entityType, IEnumerable parameters)
        => RepositoryHelper.SplitShardingParameters(this.DbKey, this.MapProvider, this.ShardingProvider, entityType, parameters);
    public string GetShardingTableName(Type entityType, Type parameterType, object parameter)
        => RepositoryHelper.GetShardingTableName(this.DbKey, this.MapProvider, this.ShardingProvider, entityType, parameterType, parameter);
    public string BuildShardingTablesSqlByFormat(SqlVisitor visitor, string formatSql, string jointMark)
    {
        //查询，分表多个表时，都使用表名替换生成分表sql
        var builder = new StringBuilder();
        if (visitor.ShardingTables.Count > 1)
        {
            var masterTableSegment = visitor.ShardingTables[0];
            var loopCount = masterTableSegment.TableNames.Count;
            var origMasterName = masterTableSegment.Mapper.TableName;
            for (int i = 0; i < loopCount; i++)
            {
                if (i > 0) builder.Append(jointMark);
                var masterTableName = masterTableSegment.TableNames[i];
                var sql = formatSql.Replace($"__SHARDING_{masterTableSegment.ShardingId}_{origMasterName}", masterTableName);

                for (int j = 1; j < visitor.ShardingTables.Count; j++)
                {
                    var tableSegment = visitor.ShardingTables[j];
                    var origName = tableSegment.Mapper.TableName;

                    //如果主表分表名不存在，直接忽略本次关联                       
                    var tableName = tableSegment.ShardingMapGetter.Invoke(this.DbKey, origMasterName, origName, masterTableName);
                    //主表存在分表，但从表不存在分表，直接忽略本次关联
                    //TOTO:此处需要记录日志
                    if (!tableSegment.TableNames.Exists(f => f == tableName))
                        continue;
                    sql = sql.Replace($"__SHARDING_{tableSegment.ShardingId}_{origName}", tableName);
                }
                builder.Append(sql);
            }
        }
        else
        {
            var tableNames = visitor.ShardingTables[0].TableNames;
            var tableSegment = visitor.ShardingTables[0];
            var origName = tableSegment.Mapper.TableName;
            for (int i = 0; i < tableNames.Count; i++)
            {
                if (i > 0) builder.Append(jointMark);
                var tableName = tableSegment.TableNames[i];
                var sql = formatSql.Replace($"__SHARDING_{tableSegment.ShardingId}_{origName}", tableName);
                builder.Append(sql);
            }
        }
        var result = builder.ToString();
        builder.Clear();
        builder = null;
        return result;
    }
}