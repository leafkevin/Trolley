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

public sealed class DbContext
{
    #region Properties
    public string DbKey { get; set; }
    public TheaConnection Connection { get; set; }
    public string ConnectionString { get; set; }
    public TheaDatabase Database { get; set; }
    public string DefaultTableSchema { get; set; }
    public IOrmProvider OrmProvider { get; set; }
    public IEntityMapProvider MapProvider { get; set; }
    public ITableShardingProvider ShardingProvider { get; set; }
    public IDbTransaction Transaction { get; set; }
    public bool IsParameterized { get; set; }
    public int CommandTimeout { get; set; }
    public string ParameterPrefix { get; set; } = "p";
    public DbInterceptors DbInterceptors { get; set; }
    #endregion   

    #region UseMasterCommand
    public (bool, TheaConnection, IDbCommand) UseMasterCommand()
    {
        bool isNeedClose = false;
        TheaConnection connection;
        if (this.Transaction != null)
            connection = this.Connection;
        else
        {
            isNeedClose = true;
            var connString = this.Database.ConnectionString;
            var conn = this.OrmProvider.CreateConnection(connString);
            connection = new TheaConnection(connString, conn);
            this.DbInterceptors.OnConnectionCreated?.Invoke(new ConectionEventArgs
            {
                ConnectionId = connection.ConnectionId,
                DbKey = this.DbKey,
                ConnectionString = connString,
                OrmProvider = this.OrmProvider,
                CreatedAt = DateTime.UtcNow
            });
        }
        var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandTimeout = this.CommandTimeout;
        command.Transaction = this.Transaction;
        return (isNeedClose, connection, command);
    }
    public (bool, TheaConnection, DbCommand) UseMasterDbCommand()
    {
        bool isNeedClose = false;
        TheaConnection connection;
        if (this.Transaction != null)
            connection = this.Connection;
        else
        {
            isNeedClose = true;
            var connString = this.Database.ConnectionString;
            var conn = this.OrmProvider.CreateConnection(connString);
            connection = new TheaConnection(connString, conn);
            this.DbInterceptors.OnConnectionCreated?.Invoke(new ConectionEventArgs
            {
                ConnectionId = connection.ConnectionId,
                DbKey = this.DbKey,
                ConnectionString = connString,
                OrmProvider = this.OrmProvider,
                CreatedAt = DateTime.UtcNow
            });
        }
        var cmd = connection.CreateCommand();
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");
        cmd.CommandType = CommandType.Text;
        cmd.CommandTimeout = this.CommandTimeout;
        cmd.Transaction = this.Transaction;
        return (isNeedClose, connection, command);
    }
    #endregion

    #region UseSlaveCommand
    public (bool, TheaConnection, IDbCommand) UseSlaveCommand(bool isUseMaster, IDbCommand command = null)
    {
        bool isNeedClose = false;
        TheaConnection connection;
        if (this.Transaction != null)
            connection = this.Connection;
        else
        {
            isNeedClose = true;
            var connString = isUseMaster ? this.Database.ConnectionString : this.Database.UseSlave();
            var conn = this.OrmProvider.CreateConnection(connString);
            connection = new TheaConnection(connString, conn);
            this.DbInterceptors.OnConnectionCreated?.Invoke(new ConectionEventArgs
            {
                ConnectionId = connection.ConnectionId,
                DbKey = this.DbKey,
                ConnectionString = connString,
                OrmProvider = this.OrmProvider,
                CreatedAt = DateTime.UtcNow
            });
        }
        if (command == null)
        {
            command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandTimeout = this.CommandTimeout;
            command.Transaction = this.Transaction;
        }
        return (isNeedClose, connection, command);
    }
    public (bool, TheaConnection, DbCommand) UseSlaveDbCommand(bool isUseMaster, IDbCommand command = null)
    {
        bool isNeedClose = false;
        TheaConnection connection;
        if (this.Transaction != null)
            connection = this.Connection;
        else
        {
            isNeedClose = true;
            var connString = isUseMaster ? this.Database.ConnectionString : this.Database.UseSlave();
            var conn = this.OrmProvider.CreateConnection(connString);
            connection = new TheaConnection(connString, conn);
            this.DbInterceptors.OnConnectionCreated?.Invoke(new ConectionEventArgs
            {
                ConnectionId = connection.ConnectionId,
                DbKey = this.DbKey,
                ConnectionString = connString,
                OrmProvider = this.OrmProvider,
                CreatedAt = DateTime.UtcNow
            });
        }
        if (command == null)
        {
            command = connection.CreateCommand();
            if (command is not DbCommand)
                throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");
            command.CommandType = CommandType.Text;
            command.CommandTimeout = this.CommandTimeout;
            command.Transaction = this.Transaction;
        }
        return (isNeedClose, connection, command as DbCommand);
    }
    #endregion

    #region QueryFirst
    public TResult QueryFirst<TResult>(Action<IDbCommand> commandInitializer)
    {
        TResult result = default;
        IDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(false);
        try
        {
            var entityType = typeof(TResult);
            commandInitializer.Invoke(command);
            this.Open(connection);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Select);
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
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Select, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Select, eventArgs, exception == null, exception);
            reader?.Dispose();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close(connection);
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<TResult> QueryFirstAsync<TResult>(Action<IDbCommand> commandInitializer, CancellationToken cancellationToken = default)
    {
        TResult result = default;
        DbDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseSlaveDbCommand(false);
        try
        {
            var entityType = typeof(TResult);
            commandInitializer.Invoke(command);

            await this.OpenAsync(connection, cancellationToken);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Select);
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
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Select, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Select, eventArgs, exception == null, exception);
            if (reader != null)
                await reader.DisposeAsync();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync(connection);
        }
        if (exception != null) throw exception;
        return result;
    }
    public TResult QueryFirst<TResult>(IQueryVisitor visitor)
    {
        TResult result = default;
        IDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor.IsUseMaster);
        try
        {
            if (visitor.IsNeedFetchShardingTables)
                this.FetchShardingTables(visitor as SqlVisitor);
            Expression<Func<TResult, TResult>> defaultExpr = f => f;
            visitor.SelectDefault(defaultExpr);
            var sql = visitor.BuildSql(out var readerFields);
            sql = this.BuildSql(visitor, sql, " UNOIN ALL ");
            command.CommandText = sql;
            visitor.DbParameters.CopyTo(command.Parameters);

            this.Open(connection);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Select);
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
                eventArgs = this.AddCommandNextBeforeFilter(command, eventArgs);
                reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                visitor.SetIncludeValues(entityType, result, reader);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Select, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Select, eventArgs, exception == null, exception);
            reader?.Close();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close(connection);
            visitor.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<TResult> QueryFirstAsync<TResult>(IQueryVisitor visitor, CancellationToken cancellationToken = default)
    {
        TResult result = default;
        DbDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseSlaveDbCommand(visitor.IsUseMaster);
        try
        {
            if (visitor.IsNeedFetchShardingTables)
                await this.FetchShardingTablesAsync(visitor as SqlVisitor, cancellationToken);
            Expression<Func<TResult, TResult>> defaultExpr = f => f;
            visitor.SelectDefault(defaultExpr);
            var sql = visitor.BuildSql(out var readerFields);
            sql = this.BuildSql(visitor, sql, " UNION ALL ");
            command.CommandText = sql;
            visitor.DbParameters.CopyTo(command.Parameters);

            await this.OpenAsync(connection, cancellationToken);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Select);
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
                eventArgs = this.AddCommandNextBeforeFilter(command, eventArgs);
                reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
                await visitor.SetIncludeValuesAsync(entityType, result, reader, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Select, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Select, eventArgs, exception == null, exception);
            reader?.Close();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync(connection);
            visitor.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region Query
    public List<TResult> Query<TResult>(Action<IDbCommand> commandInitializer)
    {
        var result = new List<TResult>();
        IDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(false);
        try
        {
            var entityType = typeof(TResult);
            commandInitializer.Invoke(command);

            this.Open(connection);
            var behavior = CommandBehavior.SequentialAccess;
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Select);
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
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Select, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Select, eventArgs, exception == null, exception);
            reader?.Dispose();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close(connection);
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<List<TResult>> QueryAsync<TResult>(Action<IDbCommand> commandInitializer, CancellationToken cancellationToken = default)
    {
        var result = new List<TResult>();
        DbDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseSlaveDbCommand(false);
        try
        {
            var entityType = typeof(TResult);
            commandInitializer.Invoke(command);
            await this.OpenAsync(connection, cancellationToken);
            var behavior = CommandBehavior.SequentialAccess;
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Select);
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
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Select, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Select, eventArgs, exception == null, exception);
            if (reader != null)
                await reader.DisposeAsync();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync(connection);
        }
        if (exception != null) throw exception;
        return result;
    }
    public List<TResult> Query<TResult>(IQueryVisitor visitor)
    {
        var result = new List<TResult>();
        IDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor.IsUseMaster);
        try
        {
            if (visitor.IsNeedFetchShardingTables)
                this.FetchShardingTables(visitor as SqlVisitor);
            Expression<Func<TResult, TResult>> defaultExpr = f => f;
            visitor.SelectDefault(defaultExpr);
            var sql = visitor.BuildSql(out var readerFields);
            sql = this.BuildSql(visitor, sql, " UNION ALL ");
            command.CommandText = sql;
            visitor.DbParameters.CopyTo(command.Parameters);

            this.Open(connection);
            var behavior = CommandBehavior.SequentialAccess;
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Select);
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
                eventArgs = this.AddCommandNextBeforeFilter(command, eventArgs);
                reader = command.ExecuteReader(behavior);
                visitor.SetIncludeValues(entityType, result, reader);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Select, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Select, eventArgs, exception == null, exception);
            reader?.Dispose();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close(connection);
            visitor.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<List<TResult>> QueryAsync<TResult>(IQueryVisitor visitor, CancellationToken cancellationToken = default)
    {
        var result = new List<TResult>();
        DbDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseSlaveDbCommand(visitor.IsUseMaster);
        try
        {
            if (visitor.IsNeedFetchShardingTables)
                await this.FetchShardingTablesAsync(visitor as SqlVisitor, cancellationToken);

            Expression<Func<TResult, TResult>> defaultExpr = f => f;
            visitor.SelectDefault(defaultExpr);
            var sql = visitor.BuildSql(out var readerFields);
            sql = this.BuildSql(visitor, sql, " UNION ALL ");
            command.CommandText = sql;
            visitor.DbParameters.CopyTo(command.Parameters);

            await this.OpenAsync(connection, cancellationToken);
            var behavior = CommandBehavior.SequentialAccess;
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Select);
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
                eventArgs = this.AddCommandNextBeforeFilter(command, eventArgs);
                reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
                await visitor.SetIncludeValuesAsync(entityType, result, reader, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Select, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Select, eventArgs, exception == null, exception);
            if (reader != null)
                await reader.DisposeAsync();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync(connection);
            visitor.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region QueryPage
    public IPagedList<TResult> QueryPage<TResult>(IQueryVisitor visitor)
    {
        IDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        var result = new PagedList<TResult> { Data = new List<TResult>() };
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor.IsUseMaster);
        try
        {
            Expression<Func<TResult, TResult>> defaultExpr = f => f;
            visitor.SelectDefault(defaultExpr);
            command.CommandText = visitor.BuildSql(out var readerFields);
            visitor.DbParameters.CopyTo(command.Parameters);

            this.Open(connection);
            var behavior = CommandBehavior.SequentialAccess;
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Select);
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
                eventArgs = this.AddCommandNextBeforeFilter(command, eventArgs);
                reader = command.ExecuteReader(behavior);
                visitor.SetIncludeValues(entityType, result, reader);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Select, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Select, eventArgs, exception == null, exception);
            reader?.Dispose();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close(connection);
            visitor.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<IPagedList<TResult>> QueryPageAsync<TResult>(IQueryVisitor visitor, CancellationToken cancellationToken = default)
    {
        DbDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        var result = new PagedList<TResult> { Data = new List<TResult>() };
        (var isNeedClose, var connection, var command) = this.UseSlaveDbCommand(visitor.IsUseMaster);
        try
        {
            Expression<Func<TResult, TResult>> defaultExpr = f => f;
            visitor.SelectDefault(defaultExpr);
            command.CommandText = visitor.BuildSql(out var readerFields);
            visitor.DbParameters.CopyTo(command.Parameters);

            await this.OpenAsync(connection, cancellationToken);
            var behavior = CommandBehavior.SequentialAccess;
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Select);
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
                eventArgs = this.AddCommandNextBeforeFilter(command, eventArgs);
                reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
                await visitor.SetIncludeValuesAsync(entityType, result.Data, reader, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Select, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Select, eventArgs, exception == null, exception);
            if (reader != null)
                await reader.DisposeAsync();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync(connection);
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

        TEntity result = default;
        IDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(false);
        try
        {
            var entityType = typeof(TEntity);
            var whereObjType = whereObj.GetType();
            var commandInitializer = RepositoryHelper.BuildGetSqlParameters(this.OrmProvider, this.MapProvider, entityType, whereObjType, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this.OrmProvider, whereObj);

            this.Open(connection);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Select);
            reader = command.ExecuteReader(behavior);
            if (reader.Read())
                result = reader.To<TEntity>(this.OrmProvider, this.MapProvider);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Select, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Select, eventArgs, exception == null, exception);
            reader?.Dispose();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close(connection);
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<TEntity> GetAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        TEntity result = default;
        DbDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseSlaveDbCommand(false);
        try
        {
            var entityType = typeof(TEntity);
            var whereObjType = whereObj.GetType();
            var commandInitializer = RepositoryHelper.BuildGetSqlParameters(this.OrmProvider, this.MapProvider, entityType, whereObjType, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this.OrmProvider, whereObj);

            await this.OpenAsync(connection, cancellationToken);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Select);
            reader = await command.ExecuteReaderAsync(behavior, cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
                result = reader.To<TEntity>(this.OrmProvider, this.MapProvider);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Select, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Select, eventArgs, exception == null, exception);
            if (reader != null)
                await reader.DisposeAsync();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync(connection);
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region Create
    public TResult CreateResult<TResult>(Func<IDbCommand, DbContext, List<SqlFieldSegment>> commandInitializer)
    {
        TResult result = default;
        IDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        try
        {
            var readerFields = commandInitializer.Invoke(command, this);
            this.Open(connection);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Insert);
            reader = command.ExecuteReader(behavior);
            if (reader.Read())
            {
                if (readerFields != null && readerFields.Count > 0)
                    result = reader.To<TResult>(this.OrmProvider, readerFields, true);
                else result = reader.To<TResult>(this.OrmProvider);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Insert, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Insert, eventArgs, exception == null, exception);
            reader?.Dispose();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close(connection);
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<TResult> CreateResultAsync<TResult>(Func<IDbCommand, DbContext, List<SqlFieldSegment>> commandInitializer, CancellationToken cancellationToken = default)
    {
        TResult result = default;
        DbDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseMasterDbCommand();
        try
        {
            var readerFields = commandInitializer.Invoke(command, this);
            await this.OpenAsync(connection, cancellationToken);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Insert);
            reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                if (readerFields != null && readerFields.Count > 0)
                    result = reader.To<TResult>(this.OrmProvider, readerFields, true);
                else result = reader.To<TResult>(this.OrmProvider);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Insert, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Insert, eventArgs, exception == null, exception);
            if (reader != null)
                await reader.DisposeAsync();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync(connection);
        }
        if (exception != null) throw exception;
        return result;
    }
    public List<TResult> CreateResult<TResult>(ICreateVisitor visitor)
    {
        var result = new List<TResult>();
        IDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        try
        {
            command.CommandText = visitor.BuildCommand(command, false, out var readerFields);
            this.Open(connection);
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Insert);
            reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
            while (reader.Read())
            {
                result.Add(reader.To<TResult>(this.OrmProvider, readerFields, true));
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Insert, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Insert, eventArgs, exception == null, exception);
            reader?.Dispose();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close(connection);
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<List<TResult>> CreateResultAsync<TResult>(ICreateVisitor visitor, CancellationToken cancellationToken = default)
    {
        var result = new List<TResult>();
        DbDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseMasterDbCommand();
        try
        {
            command.CommandText = visitor.BuildCommand(command, false, out var readerFields);
            await this.OpenAsync(connection, cancellationToken);
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Insert);
            reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<TResult>(this.OrmProvider, readerFields, true));
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Insert, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Insert, eventArgs, exception == null, exception);
            if (reader != null)
                await reader.DisposeAsync();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync(connection);
            visitor.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }

    public void BuildCreateCommand(IDbCommand command, Type entityType, object insertObj, bool isReturnIdentity)
    {
        var insertObjType = insertObj.GetType();
        var fieldsSqlPartSetter = RepositoryHelper.BuildCreateFieldsSqlPart(this.OrmProvider, this.MapProvider, entityType, insertObjType, null, null);
        var valuesSqlPartSetter = RepositoryHelper.BuildCreateValuesSqlParametes(this.OrmProvider, this.MapProvider, entityType, insertObjType, null, null, false);
        bool isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(insertObjType);

        Action<IDataParameterCollection, StringBuilder, string> firstSqlSetter = null;
        Action<IDataParameterCollection, StringBuilder, object> loopSqlSetter = null;

        var entityMapper = this.MapProvider.GetEntityMap(entityType);
        var tableName = entityMapper.TableName;
        if (isDictionary)
        {
            var typedFieldsSqlPartSetter = fieldsSqlPartSetter as Func<StringBuilder, object, List<MemberMap>>;
            var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, List<MemberMap>, object>;

            var builder = new StringBuilder();
            var memberMappers = typedFieldsSqlPartSetter.Invoke(builder, insertObj);
            builder.Append(") VALUES ");
            var firstHeadSql = builder.ToString();
            builder.Clear();

            firstSqlSetter = (dbParameters, builder, tableName) =>
            {
                builder.Append($"INSERT INTO {this.OrmProvider.GetTableName(tableName)} (");
                builder.Append(firstHeadSql);
            };
            loopSqlSetter = (dbParameters, builder, insertObj) =>
            {
                builder.Append('(');
                typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.OrmProvider, memberMappers, insertObj);
                builder.Append(')');
            };
        }
        else
        {
            var typedFieldsSqlPartSetter = fieldsSqlPartSetter as Action<StringBuilder>;
            var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object>;

            firstSqlSetter = (dbParameters, builder, tableName) =>
            {
                builder.Append($"INSERT INTO {this.OrmProvider.GetTableName(tableName)} (");
                typedFieldsSqlPartSetter.Invoke(builder);
                builder.Append(") VALUES ");
            };
            loopSqlSetter = (dbParameters, builder, insertObj) =>
            {
                builder.Append('(');
                typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.OrmProvider, insertObj);
                builder.Append(')');
            };
        }
        var sqlBuilder = new StringBuilder();
        if (this.ShardingProvider != null && this.ShardingProvider.TryGetTableSharding(entityType, out _))
            tableName = this.GetShardingTableName(entityType, insertObjType, insertObj);
        firstSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName);
        loopSqlSetter.Invoke(command.Parameters, sqlBuilder, insertObj);
        if (isReturnIdentity)
        {
            var keyField = entityMapper.KeyMembers[0].FieldName;
            keyField = this.OrmProvider.GetFieldName(keyField);
            sqlBuilder.Append(this.OrmProvider.GetIdentitySql(keyField));
        }
        command.CommandText = sqlBuilder.ToString();
    }
    #endregion

    #region Execute
    public int Execute(Func<IDbCommand, bool> commandInitializer)
    {
        int result = 0;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        try
        {
            if (!commandInitializer.Invoke(command))
                this.Open(connection);
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Execute);
            result = command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Execute, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Execute, eventArgs, exception == null, exception);
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close(connection);
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<int> ExecuteAsync(Func<IDbCommand, bool> commandInitializer, CancellationToken cancellationToken = default)
    {
        int result = 0;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseMasterDbCommand();
        try
        {
            if (!commandInitializer.Invoke(command))
                await this.OpenAsync(connection, cancellationToken);
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Execute);
            result = await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Execute, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Execute, eventArgs, exception == null, exception);
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync(connection);
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion   

    #region Others
    public void Open(TheaConnection connection)
    {
        if (connection.State == ConnectionState.Broken)
        {
            connection.Close();
            this.DbInterceptors.OnConnectionClosed?.Invoke(new ConectionEventArgs
            {
                ConnectionId = connection.ConnectionId,
                ConnectionString = connection.ConnectionString,
                DbKey = this.DbKey,
                OrmProvider = this.OrmProvider,
                CreatedAt = DateTime.Now
            });
        }
        if (connection.State == ConnectionState.Closed)
        {
            //关闭后，连接串被重置，需要重新设置
            connection.BaseConnection.ConnectionString = connection.ConnectionString;
            this.DbInterceptors.OnConnectionOpening?.Invoke(new ConectionEventArgs
            {
                ConnectionId = connection.ConnectionId,
                ConnectionString = connection.ConnectionString,
                DbKey = this.DbKey,
                OrmProvider = this.OrmProvider,
                CreatedAt = DateTime.Now
            });
            connection.Open();
            this.DbInterceptors.OnConnectionOpened?.Invoke(new ConectionEventArgs
            {
                ConnectionId = connection.ConnectionId,
                ConnectionString = connection.ConnectionString,
                DbKey = this.DbKey,
                OrmProvider = this.OrmProvider,
                CreatedAt = DateTime.Now
            });
        }
    }
    public async Task OpenAsync(TheaConnection connection, CancellationToken cancellationToken = default)
    {
        if (connection.BaseConnection is not DbConnection)
            throw new NotSupportedException("当前数据库驱动不支持异步操作");
        if (connection.State == ConnectionState.Broken)
        {
            this.DbInterceptors.OnConnectionClosing?.Invoke(new ConectionEventArgs
            {
                ConnectionId = connection.ConnectionId,
                ConnectionString = connection.ConnectionString,
                DbKey = this.DbKey,
                OrmProvider = this.OrmProvider,
                CreatedAt = DateTime.Now
            });
            await connection.CloseAsync();
            this.DbInterceptors.OnConnectionClosed?.Invoke(new ConectionEventArgs
            {
                ConnectionId = connection.ConnectionId,
                ConnectionString = connection.ConnectionString,
                DbKey = this.DbKey,
                OrmProvider = this.OrmProvider,
                CreatedAt = DateTime.Now
            });
        }
        if (connection.State == ConnectionState.Closed)
        {
            //关闭后，连接串被重置，需要重新设置
            connection.BaseConnection.ConnectionString = connection.ConnectionString;
            this.DbInterceptors.OnConnectionOpening?.Invoke(new ConectionEventArgs
            {
                ConnectionId = connection.ConnectionId,
                ConnectionString = connection.ConnectionString,
                DbKey = this.DbKey,
                OrmProvider = this.OrmProvider,
                CreatedAt = DateTime.Now
            });
            await connection.OpenAsync(cancellationToken);
            this.DbInterceptors.OnConnectionOpened?.Invoke(new ConectionEventArgs
            {
                ConnectionId = connection.ConnectionId,
                ConnectionString = connection.ConnectionString,
                DbKey = this.DbKey,
                OrmProvider = this.OrmProvider,
                CreatedAt = DateTime.Now
            });
        }
    }
    public IDbTransaction BeginTransaction()
    {
        if (this.Transaction != null)
            throw new Exception("上一个事务还没有完成，无法开启新事务");
        this.UseMaster();
        this.Open(this.Connection);
        this.Transaction = this.Connection.BeginTransaction();
        return this.Transaction;
    }
    public async ValueTask<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (this.Transaction != null)
            throw new Exception("上一个事务还没有完成，无法开启新事务");
        this.UseMaster();
        await this.OpenAsync(this.Connection, cancellationToken);
        if (this.Connection.BaseConnection is DbConnection connection)
            this.Transaction = await connection.BeginTransactionAsync(cancellationToken);
        else throw new NotSupportedException("当前数据库驱动不支持异步操作");
        return this.Transaction;
    }
    public void Commit()
    {
        this.Transaction?.Commit();
        this.Close(this.Connection);
        this.Transaction = null;
        this.Connection = null;
    }
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (this.Transaction != null)
        {
            if (this.Transaction is not DbTransaction dbTransaction)
                throw new NotSupportedException("当前数据库驱动不支持异步操作");
            await dbTransaction.CommitAsync(cancellationToken);
        }
        await this.CloseAsync(this.Connection);
        this.Transaction = null;
        this.Connection = null;
    }
    public void Rollback()
    {
        this.Transaction?.Rollback();
        this.Close(this.Connection);
        this.Transaction = null;
        this.Connection = null;
    }
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (this.Transaction != null)
        {
            if (this.Transaction is not DbTransaction dbTransaction)
                throw new NotSupportedException("当前数据库驱动不支持异步操作");
            await dbTransaction.RollbackAsync(cancellationToken);
        }
        await this.CloseAsync(this.Connection);
        this.Transaction = null;
        this.Connection = null;
    }
    public void Close(TheaConnection connection)
    {
        if (connection == null) return;
        if (connection.State == ConnectionState.Closed)
            return;
        this.DbInterceptors.OnConnectionClosing?.Invoke(new ConectionEventArgs
        {
            ConnectionId = connection.ConnectionId,
            ConnectionString = connection.ConnectionString,
            DbKey = this.DbKey,
            OrmProvider = this.OrmProvider,
            CreatedAt = DateTime.Now
        });
        connection.Close();
        this.DbInterceptors.OnConnectionClosed?.Invoke(new ConectionEventArgs
        {
            ConnectionId = connection.ConnectionId,
            ConnectionString = connection.ConnectionString,
            DbKey = this.DbKey,
            OrmProvider = this.OrmProvider,
            CreatedAt = DateTime.Now
        });
    }
    public async Task CloseAsync(TheaConnection connection)
    {
        if (connection == null) return;
        if (connection.State == ConnectionState.Closed)
            return;
        if (connection.BaseConnection is not DbConnection)
            throw new NotSupportedException("当前数据库驱动不支持异步操作");

        this.DbInterceptors.OnConnectionClosing?.Invoke(new ConectionEventArgs
        {
            ConnectionId = connection.ConnectionId,
            ConnectionString = connection.ConnectionString,
            DbKey = this.DbKey,
            OrmProvider = this.OrmProvider,
            CreatedAt = DateTime.Now
        });
        await connection.CloseAsync();
        this.DbInterceptors.OnConnectionClosed?.Invoke(new ConectionEventArgs
        {
            ConnectionId = connection.ConnectionId,
            ConnectionString = connection.ConnectionString,
            DbKey = this.DbKey,
            OrmProvider = this.OrmProvider,
            CreatedAt = DateTime.Now
        });
    }
    private void UseMaster()
    {
        this.ConnectionString = this.Database.ConnectionString;
        var connection = this.OrmProvider.CreateConnection(this.ConnectionString);
        this.Connection = new TheaConnection(this.ConnectionString, connection);
        this.DbInterceptors.OnConnectionCreated?.Invoke(new ConectionEventArgs
        {
            ConnectionId = this.Connection.ConnectionId,
            DbKey = this.DbKey,
            ConnectionString = this.ConnectionString,
            OrmProvider = this.OrmProvider,
            CreatedAt = DateTime.UtcNow
        });
    }
    #endregion

    public string BuildSql(IQueryVisitor visitor, string formatSql, string jointMark)
    {
        var sql = formatSql;
        if (visitor.ShardingTables != null && visitor.ShardingTables.Count > 0)
            sql = this.BuildShardingTablesSqlByFormat(visitor as SqlVisitor, formatSql, jointMark);
        return sql;
    }
    public void FetchShardingTables(SqlVisitor visitor)
    {
        var fetchSql = visitor.BuildTableShardingsSql();
        IDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor.IsUseMaster);
        try
        {
            command.CommandText = fetchSql;
            this.Open(connection);
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Select);
            reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
            var shardingTables = new List<string>();
            while (reader.Read())
            {
                shardingTables.Add(reader.To<string>(this.OrmProvider));
            }
            visitor.SetShardingTables(shardingTables);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Select, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Select, eventArgs, exception == null, exception);
            reader.Dispose();
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close(connection);
        }
        if (exception != null) throw exception;
    }
    public async Task FetchShardingTablesAsync(SqlVisitor visitor, CancellationToken cancellationToken = default)
    {
        var fetchSql = visitor.BuildTableShardingsSql();
        DbDataReader reader = null;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.UseSlaveDbCommand(visitor.IsUseMaster);
        try
        {
            command.CommandText = fetchSql;
            command.Parameters.Clear();
            await this.OpenAsync(connection, cancellationToken);
            eventArgs = this.AddCommandBeforeFilter(connection, command, CommandSqlType.Select);
            reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
            var shardingTables = new List<string>();
            while (await reader.ReadAsync(cancellationToken))
            {
                shardingTables.Add(reader.To<string>(this.OrmProvider));
            }
            visitor.SetShardingTables(shardingTables);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.AddCommandFailedFilter(connection, command, CommandSqlType.Select, eventArgs, exception);
        }
        finally
        {
            this.AddCommandAfterFilter(connection, command, CommandSqlType.Select, eventArgs, exception == null, exception);
            await reader.DisposeAsync();
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) this.Close(connection);
        }
        if (exception != null) throw exception;
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
            if (loopCount > 1) masterTableSegment.TableNames.Sort((x, y) => x.CompareTo(y));
            var origMasterName = masterTableSegment.Mapper.TableName;
            Dictionary<TableSegment, List<string>> tableShardings = new();
            for (int i = 0; i < loopCount; i++)
            {
                if (builder.Length > 0) builder.Append(jointMark);
                var masterTableName = masterTableSegment.TableNames[i];
                var sql = formatSql.Replace($"__SHARDING_{masterTableSegment.ShardingId}_{origMasterName}", masterTableName);

                if (this.GetdShardingMapTableName(visitor, origMasterName, masterTableName, sql, tableShardings, out sql))
                    builder.Append(sql);
            }
            if (tableShardings.Count > 0)
            {
                foreach (var tableSharding in tableShardings)
                {
                    tableSharding.Key.TableNames = tableSharding.Value;
                }
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
    private bool GetdShardingMapTableName(SqlVisitor visitor, string origMasterName, string masterTableName, string formatSql, Dictionary<TableSegment, List<string>> tableShardingNames, out string sql)
    {
        sql = formatSql;
        for (int j = 1; j < visitor.ShardingTables.Count; j++)
        {
            var tableSegment = visitor.ShardingTables[j];
            var origName = tableSegment.Mapper.TableName;

            //如果主表分表名不存在，直接忽略本次关联
            var tableName = tableSegment.ShardingMapGetter.Invoke(origMasterName, origName, masterTableName);
            //主表存在分表，但从表不存在分表，直接忽略本次关联
            //TOTO:此处需要记录日志
            if (!tableSegment.TableNames.Exists(f => f == tableName))
                return false;
            sql = sql.Replace($"__SHARDING_{tableSegment.ShardingId}_{origName}", tableName);
            //1:N include表，需要统计一下表名，后续会用到
            if (visitor.IncludeTables != null && visitor.IncludeTables.Contains(tableSegment))
            {
                if (!tableShardingNames.TryGetValue(tableSegment, out var tableNames))
                    tableShardingNames.TryAdd(tableSegment, tableNames = new List<string>());
                tableNames.Add(tableName);
            }
        }
        return true;
    }
    public CommandEventArgs AddCommandBeforeFilter(TheaConnection connection, IDbCommand command, CommandSqlType sqlType)
    {
        var eventArgs = new CommandEventArgs
        {
            DbKey = this.DbKey,
            ConnectionString = connection.ConnectionString,
            SqlType = sqlType,
            Sql = command.CommandText,
            DbParameters = command.Parameters,
            OrmProvider = this.OrmProvider,
            CreatedAt = DateTime.Now
        };
        this.DbInterceptors.OnCommandExecuting?.Invoke(eventArgs);
        return eventArgs;
    }
    public CommandEventArgs AddCommandNextBeforeFilter(IDbCommand command, CommandEventArgs eventArgs)
    {
        eventArgs.Sql = command.CommandText;
        eventArgs.DbParameters = command.Parameters;
        this.DbInterceptors.OnCommandExecuting?.Invoke(eventArgs);
        return eventArgs;
    }
    public CommandEventArgs AddCommandBeforeFilter(TheaConnection connection, CommandSqlType sqlType, CommandEventArgs eventArgs)
    {
        if (eventArgs == null)
        {
            eventArgs = new CommandEventArgs
            {
                DbKey = this.DbKey,
                ConnectionString = connection.ConnectionString,
                SqlType = sqlType,
                OrmProvider = this.OrmProvider,
                CreatedAt = DateTime.Now
            };
        }
        else eventArgs.BulkIndex++;
        this.DbInterceptors.OnCommandExecuting?.Invoke(eventArgs);
        return eventArgs;
    }
    public CommandEventArgs AddCommandBeforeFilter(TheaConnection connection, IDbCommand command, CommandSqlType sqlType, CommandEventArgs eventArgs)
    {
        if (eventArgs == null)
        {
            eventArgs = new CommandEventArgs
            {
                DbKey = this.DbKey,
                ConnectionString = connection.ConnectionString,
                SqlType = sqlType,
                Sql = command.CommandText,
                DbParameters = command.Parameters,
                OrmProvider = this.OrmProvider,
                CreatedAt = DateTime.Now
            };
        }
        else
        {
            eventArgs.Sql = command.CommandText;
            eventArgs.DbParameters = command.Parameters;
            eventArgs.BulkIndex++;
        }
        this.DbInterceptors.OnCommandExecuting?.Invoke(eventArgs);
        return eventArgs;
    }
    public void AddCommandAfterFilter(TheaConnection connection, IDbCommand command, CommandSqlType sqlType, CommandEventArgs eventArgs, bool isSuccess = true, Exception exception = null)
    {
        if (eventArgs == null)
        {
            this.DbInterceptors.OnCommandExecuted?.Invoke(new CommandCompletedEventArgs
            {
                IsSuccess = isSuccess,
                CommandId = Guid.NewGuid().ToString("N"),
                DbKey = this.DbKey,
                ConnectionString = connection.ConnectionString,
                SqlType = sqlType,
                Sql = command.CommandText,
                DbParameters = command.Parameters,
                OrmProvider = this.OrmProvider,
                CreatedAt = DateTime.Now,
                Exception = exception
            });
        }
        else
        {
            this.DbInterceptors.OnCommandExecuted?.Invoke(new CommandCompletedEventArgs(eventArgs)
            {
                IsSuccess = isSuccess,
                Exception = exception
            });
        }
    }
    public void AddCommandFailedFilter(TheaConnection connection, IDbCommand command, CommandSqlType sqlType, CommandEventArgs eventArgs, Exception exception)
    {
        if (eventArgs == null)
        {
            this.DbInterceptors.OnCommandExecuted?.Invoke(new CommandCompletedEventArgs
            {
                IsSuccess = false,
                CommandId = Guid.NewGuid().ToString("N"),
                DbKey = this.DbKey,
                ConnectionString = connection.ConnectionString,
                SqlType = sqlType,
                Sql = command.CommandText,
                DbParameters = command.Parameters,
                OrmProvider = this.OrmProvider,
                CreatedAt = DateTime.Now,
                Exception = exception
            });
        }
        else
        {
            this.DbInterceptors.OnCommandExecuted?.Invoke(new CommandCompletedEventArgs(eventArgs)
            {
                IsSuccess = false,
                Exception = exception
            });
        }
    }
}