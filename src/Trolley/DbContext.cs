using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public sealed class DbContext
{
    #region Properties
    public string DbKey { get; set; }
    public TheaConnection Connection { get; set; }
    public IOrmProvider OrmProvider { get; set; }
    public IEntityMapProvider MapProvider { get; set; }
    public IDbTransaction Transaction { get; set; }
    public bool IsParameterized { get; set; }
    public int Timeout { get; set; }
    public bool IsNeedClose => this.Transaction == null;
    #endregion

    #region CreateCommand
    public IDbCommand CreateCommand()
    {
        var command = this.Connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandTimeout = this.Timeout;
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

            this.Connection.Open();
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            reader = command.ExecuteReader(behavior);
            if (reader.Read())
            {
                if (entityType.IsEntityType(out _))
                    result = reader.To<TResult>(this.DbKey, this.OrmProvider, this.MapProvider);
                else result = reader.To<TResult>();
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
            command.Dispose();
            if (isNeedClose) this.Close();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<TResult> QueryFirstAsync<TResult>(Action<IDbCommand> commandInitializer, CancellationToken cancellationToken)
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

            await this.Connection.OpenAsync(cancellationToken);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                if (entityType.IsEntityType(out _))
                    result = reader.To<TResult>(this.DbKey, this.OrmProvider, this.MapProvider);
                else result = reader.To<TResult>();
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
            Expression<Func<TResult, TResult>> defaultExpr = f => f;
            visitor.SelectDefault(defaultExpr);
            command.CommandText = visitor.BuildSql(out var readerFields);
            visitor.DbParameters.CopyTo(command.Parameters);

            this.Connection.Open();
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            reader = command.ExecuteReader(behavior);
            var entityType = typeof(TResult);
            if (reader.Read())
            {
                if (entityType.IsEntityType(out _))
                    result = reader.To<TResult>(this.DbKey, this.OrmProvider, readerFields);
                else result = reader.To<TResult>();
            }
            reader.Dispose();

            if (visitor.BuildIncludeSql(entityType, result, out var sql))
            {
                command.CommandText = sql;
                command.Parameters.Clear();
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
            command.Dispose();
            if (isNeedClose) this.Close();
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
            Expression<Func<TResult, TResult>> defaultExpr = f => f;
            visitor.SelectDefault(defaultExpr);
            command.CommandText = visitor.BuildSql(out var readerFields);
            visitor.DbParameters.CopyTo(command.Parameters);

            await this.Connection.OpenAsync(cancellationToken);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            var entityType = typeof(TResult);
            if (await reader.ReadAsync(cancellationToken))
            {
                if (entityType.IsEntityType(out _))
                    result = reader.To<TResult>(this.DbKey, this.OrmProvider, readerFields);
                else result = reader.To<TResult>();
            }
            await reader.DisposeAsync();

            if (visitor.BuildIncludeSql(entityType, result, out var sql))
            {
                command.CommandText = sql;
                command.Parameters.Clear();
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
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
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

            this.Connection.Open();
            var behavior = CommandBehavior.SequentialAccess;
            reader = command.ExecuteReader(behavior);

            if (entityType.IsEntityType(out _))
            {
                while (reader.Read())
                {
                    result.Add(reader.To<TResult>(this.DbKey, this.OrmProvider, this.MapProvider));
                }
            }
            else
            {
                while (reader.Read())
                {
                    result.Add(reader.To<TResult>());
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
            command.Dispose();
            if (isNeedClose) this.Close();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<List<TResult>> QueryAsync<TResult>(Action<IDbCommand> commandInitializer, CancellationToken cancellationToken)
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
            await this.Connection.OpenAsync(cancellationToken);
            var behavior = CommandBehavior.SequentialAccess;
            reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            if (entityType.IsEntityType(out _))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    result.Add(reader.To<TResult>(this.DbKey, this.OrmProvider, this.MapProvider));
                }
            }
            else
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    result.Add(reader.To<TResult>());
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
            Expression<Func<TResult, TResult>> defaultExpr = f => f;
            visitor.SelectDefault(defaultExpr);
            command.CommandText = visitor.BuildSql(out var readerFields);
            visitor.DbParameters.CopyTo(command.Parameters);

            this.Connection.Open();
            var behavior = CommandBehavior.SequentialAccess;
            reader = command.ExecuteReader(behavior);
            var entityType = typeof(TResult);
            if (entityType.IsEntityType(out _))
            {
                while (reader.Read())
                {
                    result.Add(reader.To<TResult>(this.DbKey, this.OrmProvider, readerFields));
                }
            }
            else
            {
                while (reader.Read())
                {
                    result.Add(reader.To<TResult>());
                }
            }
            reader.Dispose();

            if (visitor.BuildIncludeSql(entityType, result, out var sql))
            {
                command.CommandText = sql;
                command.Parameters.Clear();
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
            command.Dispose();
            if (isNeedClose) this.Close();
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
            Expression<Func<TResult, TResult>> defaultExpr = f => f;
            visitor.SelectDefault(defaultExpr);
            command.CommandText = visitor.BuildSql(out var readerFields);
            visitor.DbParameters.CopyTo(command.Parameters);

            await this.Connection.OpenAsync(cancellationToken);
            var behavior = CommandBehavior.SequentialAccess;
            reader = await command.ExecuteReaderAsync(behavior, cancellationToken);

            var entityType = typeof(TResult);
            if (entityType.IsEntityType(out _))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    result.Add(reader.To<TResult>(this.DbKey, this.OrmProvider, readerFields));
                }
            }
            else
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    result.Add(reader.To<TResult>());
                }
            }
            await reader.DisposeAsync();

            if (visitor.BuildIncludeSql(entityType, result, out var sql))
            {
                command.CommandText = sql;
                command.Parameters.Clear();
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
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region QueryPage
    public IPagedList<TResult> QueryPage<TResult>(IQueryVisitor visitor, int pageIndex, int pageSize)
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

            this.Connection.Open();
            var behavior = CommandBehavior.SequentialAccess;
            reader = command.ExecuteReader(behavior);
            if (reader.Read()) result.TotalCount = reader.To<int>();
            result.PageIndex = pageIndex;
            result.PageSize = pageSize;

            reader.NextResult();
            var entityType = typeof(TResult);
            if (entityType.IsEntityType(out _))
            {
                while (reader.Read())
                {
                    result.Data.Add(reader.To<TResult>(this.DbKey, this.OrmProvider, readerFields));
                }
            }
            else
            {
                while (reader.Read())
                {
                    result.Data.Add(reader.To<TResult>());
                }
            }
            reader.Dispose();

            if (visitor.BuildIncludeSql(entityType, result.Data, out var sql))
            {
                command.CommandText = sql;
                command.Parameters.Clear();
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
            command.Dispose();
            if (isNeedClose) this.Close();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<IPagedList<TResult>> QueryPageAsync<TResult>(IQueryVisitor visitor, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
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

            await this.Connection.OpenAsync(cancellationToken);
            var behavior = CommandBehavior.SequentialAccess;
            reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            if (await reader.ReadAsync()) result.TotalCount = reader.To<int>();
            result.PageIndex = pageIndex;
            result.PageSize = pageSize;

            await reader.NextResultAsync(cancellationToken);
            var entityType = typeof(TResult);
            if (entityType.IsEntityType(out _))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    result.Data.Add(reader.To<TResult>(this.DbKey, this.OrmProvider, readerFields));
                }
            }
            else
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    result.Data.Add(reader.To<TResult>());
                }
            }
            await reader.DisposeAsync();

            if (visitor.BuildIncludeSql(entityType, result.Data, out var sql))
            {
                command.CommandText = sql;
                command.Parameters.Clear();
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
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
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
            var commandInitializer = RepositoryHelper.BuildGetSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this.OrmProvider, whereObj);

            this.Connection.Open();
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            reader = command.ExecuteReader(behavior);
            if (reader.Read())
                result = reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            reader?.Dispose();
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
            var commandInitializer = RepositoryHelper.BuildGetSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this.OrmProvider, whereObj);

            await this.Connection.OpenAsync(cancellationToken);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            reader = await command.ExecuteReaderAsync(behavior, cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
                result = reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider);
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
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region CreateIdentity
    public TResult CreateIdentity<TResult>(Action<IDbCommand> commandInitializer)
    {
        using var command = this.CreateCommand();
        TResult result = default;
        IDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            commandInitializer.Invoke(command);
            this.Connection.Open();
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            reader = command.ExecuteReader(behavior);
            if (reader.Read()) result = reader.To<TResult>();
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            reader?.Dispose();
            command.Dispose();
            if (isNeedClose) this.Close();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<TResult> CreateIdentityAsync<TResult>(Action<IDbCommand> commandInitializer, CancellationToken cancellationToken)
    {
        using var command = this.CreateDbCommand();
        TResult result = default;
        DbDataReader reader = null;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            commandInitializer.Invoke(command);
            await this.Connection.OpenAsync(cancellationToken);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
                result = reader.To<TResult>();
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
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region Execute
    public int Execute(Action<IDbCommand> commandInitializer)
    {
        using var command = this.CreateCommand();
        int result = 0;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            commandInitializer.Invoke(command);
            this.Connection.Open();
            result = command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            command.Dispose();
            if (isNeedClose) this.Close();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<int> ExecuteAsync(Action<IDbCommand> commandInitializer, CancellationToken cancellationToken)
    {
        using var command = this.CreateDbCommand();
        int result = 0;
        bool isNeedClose = this.IsNeedClose;
        Exception exception = null;
        try
        {
            commandInitializer.Invoke(command);
            await this.Connection.OpenAsync(cancellationToken);
            result = await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region Dispose
    public void Close()
    {
        this.Connection?.Close();
        this.Transaction = null;
    }
    public async ValueTask CloseAsync()
    {
        await this.Connection.CloseAsync();
        this.Transaction = null;
    }
    #endregion
}
