using Npgsql;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.PostgreSql;

class PostgreSqlTheaCommand : ITheaCommand
{
    private readonly NpgsqlCommand command;
    private ITheaConnection connection;
    private ITheaTransaction transaction;
    private int index = -1;

    public string DbKey => this.connection?.DbKey;
    public string CommandId { get; private set; }
    public IDbCommand BaseCommand => this.command;
    public bool IsNeedClose => this.transaction == null;

    public string CommandText { get => this.command.CommandText; set => this.command.CommandText = value; }
    public int CommandTimeout { get => this.command.CommandTimeout; set => this.command.CommandTimeout = value; }
    public CommandType CommandType { get => this.command.CommandType; set => this.command.CommandType = value; }
    public IDataParameterCollection Parameters => this.command.Parameters;
    public ITheaConnection Connection
    {
        get => this.connection;
        set
        {
            this.connection = value;
            this.BaseCommand.Connection = value.BaseConnection;
        }
    }
    public ITheaTransaction Transaction
    {
        get => this.transaction;
        set
        {
            this.transaction = value;
            this.BaseCommand.Transaction = value?.BaseTransaction ?? null;
        }
    }
    public Action<CommandEventArgs> OnExecuting { get; set; }
    public Action<CommandCompletedEventArgs> OnExecuted { get; set; }

    public PostgreSqlTheaCommand(NpgsqlCommand command, ITheaConnection connection, ITheaTransaction transaction)
    {
        this.CommandId = Guid.NewGuid().ToString("N");
        this.command = command;
        this.Connection = connection;
        this.transaction = transaction;
    }

    public int ExecuteNonQuery(CommandSqlType sqlType)
    {
        this.index++;
        var createdAt = DateTime.Now;
        this.OnExecuting?.Invoke(new CommandEventArgs
        {
            DbKey = this.DbKey,
            CommandId = this.CommandId,
            ConnectionId = this.Connection.ConnectionString,
            TransactionId = this.Transaction?.TransactionId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType,
            CreatedAt = createdAt
        });
        int recordsAffected = 0;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            recordsAffected = this.command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
            this.OnExecuted?.Invoke(new CommandCompletedEventArgs
            {
                DbKey = this.DbKey,
                CommandId = this.CommandId,
                ConnectionId = this.Connection.ConnectionString,
                TransactionId = this.Transaction?.TransactionId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed,
                CreatedAt = createdAt
            });
        }
        if (!isSuccess)
        {
            this.Dispose();
            if (this.IsNeedClose) this.connection.Close();
            throw exception;
        }
        return recordsAffected;
    }
    public async Task<int> ExecuteNonQueryAsync(CommandSqlType sqlType, CancellationToken cancellationToken = default)
    {
        this.index++;
        var createdAt = DateTime.Now;
        this.OnExecuting?.Invoke(new CommandEventArgs
        {
            DbKey = this.DbKey,
            CommandId = this.CommandId,
            ConnectionId = this.Connection.ConnectionString,
            TransactionId = this.Transaction?.TransactionId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType,
            CreatedAt = createdAt
        });
        int recordsAffected = 0;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            recordsAffected = await this.command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
            this.OnExecuted?.Invoke(new CommandCompletedEventArgs
            {
                DbKey = this.DbKey,
                CommandId = this.CommandId,
                ConnectionId = this.Connection.ConnectionString,
                TransactionId = this.Transaction?.TransactionId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed,
                CreatedAt = createdAt
            });
        }
        if (!isSuccess)
        {
            await this.DisposeAsync();
            if (this.IsNeedClose) await this.connection.CloseAsync();
            throw exception;
        }
        return recordsAffected;
    }
    public ITheaDataReader ExecuteReader(CommandSqlType sqlType, CommandBehavior behavior = default)
    {
        this.index++;
        bool isNeedClose = this.IsNeedClose;
        var createdAt = DateTime.Now;
        this.OnExecuting?.Invoke(new CommandEventArgs
        {
            DbKey = this.DbKey,
            CommandId = this.CommandId,
            ConnectionId = this.Connection.ConnectionString,
            TransactionId = this.Transaction?.TransactionId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType,
            CreatedAt = createdAt
        });
        NpgsqlDataReader reader = null;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            reader = this.command.ExecuteReader(behavior);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
            this.OnExecuted?.Invoke(new CommandCompletedEventArgs
            {
                DbKey = this.DbKey,
                CommandId = this.CommandId,
                ConnectionId = this.Connection.ConnectionString,
                TransactionId = this.Transaction?.TransactionId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed,
                CreatedAt = createdAt
            });
        }
        if (!isSuccess)
        {
            this.Dispose();
            if (this.IsNeedClose) this.connection.Close();
            throw exception;
        }
        return new PostgreSqlTheaDataReader(reader);
    }
    public async Task<ITheaDataReader> ExecuteReaderAsync(CommandSqlType sqlType, CommandBehavior behavior = default, CancellationToken cancellationToken = default)
    {
        this.index++;
        var createdAt = DateTime.Now;
        this.OnExecuting?.Invoke(new CommandEventArgs
        {
            DbKey = this.DbKey,
            CommandId = this.CommandId,
            ConnectionId = this.Connection.ConnectionString,
            TransactionId = this.Transaction?.TransactionId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType,
            CreatedAt = createdAt
        });
        NpgsqlDataReader reader = null;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            reader = await this.command.ExecuteReaderAsync(behavior, cancellationToken);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
            this.OnExecuted?.Invoke(new CommandCompletedEventArgs
            {
                DbKey = this.DbKey,
                CommandId = this.CommandId,
                ConnectionId = this.Connection.ConnectionString,
                TransactionId = this.Transaction?.TransactionId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed,
                CreatedAt = createdAt
            });
        }
        if (!isSuccess)
        {
            await this.DisposeAsync();
            if (this.IsNeedClose) await this.connection.CloseAsync();
            throw exception;
        }
        return new PostgreSqlTheaDataReader(reader);
    }
    public object ExecuteScalar(CommandSqlType sqlType)
    {
        this.index++;
        var createdAt = DateTime.Now;
        this.OnExecuting?.Invoke(new CommandEventArgs
        {
            DbKey = this.DbKey,
            CommandId = this.CommandId,
            ConnectionId = this.Connection.ConnectionString,
            TransactionId = this.Transaction?.TransactionId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType,
            CreatedAt = createdAt
        });
        object result = null;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            result = this.command.ExecuteScalar();
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
            this.OnExecuted?.Invoke(new CommandCompletedEventArgs
            {
                DbKey = this.DbKey,
                CommandId = this.CommandId,
                ConnectionId = this.Connection.ConnectionString,
                TransactionId = this.Transaction?.TransactionId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed,
                CreatedAt = createdAt
            });
        }
        if (!isSuccess)
        {
            this.Dispose();
            if (this.IsNeedClose) this.connection.Close();
            throw exception;
        }
        return result;
    }
    public async Task<object> ExecuteScalarAsync(CommandSqlType sqlType, CancellationToken cancellationToken = default)
    {
        this.index++;
        var createdAt = DateTime.Now;
        this.OnExecuting?.Invoke(new CommandEventArgs
        {
            DbKey = this.DbKey,
            CommandId = this.CommandId,
            ConnectionId = this.Connection.ConnectionString,
            TransactionId = this.Transaction?.TransactionId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType,
            CreatedAt = createdAt
        });
        object result = null;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            result = await this.command.ExecuteScalarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
            this.OnExecuted?.Invoke(new CommandCompletedEventArgs
            {
                DbKey = this.DbKey,
                CommandId = this.CommandId,
                ConnectionId = this.Connection.ConnectionString,
                TransactionId = this.Transaction?.TransactionId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed,
                CreatedAt = createdAt
            });
        }
        if (!isSuccess)
        {
            await this.DisposeAsync();
            if (this.IsNeedClose) await this.connection.CloseAsync();
            throw exception;
        }
        return result;
    }
    public void Dispose()
    {
        this.command.CommandText = null;
        this.command.Parameters.Clear();
        this.command.Dispose();
    }
    public ValueTask DisposeAsync()
    {
        this.command.CommandText = null;
        this.command.Parameters.Clear();
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return this.command.DisposeAsync();    
#else       
        this.Dispose();
        return default;
#endif
    }
}
