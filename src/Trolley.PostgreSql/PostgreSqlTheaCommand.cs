using Npgsql;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.PostgreSql;

class PostgreSqlTheaCommand : ITheaCommand
{
    private readonly NpgsqlCommand command;
    private PostgreSqlTheaConnection connection;
    private PostgreSqlTheaTransaction transaction;

    public string DbKey { get; private set; }
    public string CommandId { get; private set; }
    public IDbCommand DbCommand => this.command;
    public IDbConnection DbConnection => this.connection.DbConnection;
    public IDbTransaction DbTransaction => this.transaction.DbTransaction;
    public bool IsNeedClose => this.transaction == null;
    public IDbInterceptor Interceptor { get; set; }

    public string CommandText
    {
        get => this.command.CommandText;
        set => this.command.CommandText = value;
    }
    public int CommandTimeout
    {
        get => this.command.CommandTimeout;
        set => this.command.CommandTimeout = value;
    }
    public CommandType CommandType
    {
        get => this.command.CommandType;
        set => this.command.CommandType = value;
    }
    public IDataParameterCollection Parameters => this.command.Parameters;
    public ITheaConnection Connection
    {
        get => this.connection;
        set
        {
            if (value is not PostgreSqlTheaConnection theaConnection)
                throw new NotSupportedException("不支持的连接类型，只支持PostgreSqlTheaConnection类型");
            this.connection = theaConnection;
            this.DbKey = theaConnection.DbKey;
        }
    }
    public ITheaTransaction Transaction
    {
        get => this.transaction;
        set
        {
            if (value is not MySqlTheaTransaction theaTransaction)
                throw new NotSupportedException("不支持的事务类型，只支持MySqlTheaTransaction类型");
            this.transaction = theaTransaction;
            if (string.IsNullOrEmpty(this.DbKey))
                this.DbKey = theaTransaction.DbKey;
            if (!ReferenceEquals(this.connection, this.transaction.Connection))
                this.connection = this.transaction.Connection as MySqlTheaConnection;
        }
    }
    public UpdateRowSource UpdatedRowSource
    {
        get => this.command.UpdatedRowSource;
        set => this.command.UpdatedRowSource = value;
    }
    IDbConnection IDbCommand.Connection
    {
        get => this.connection.DbConnection;
        set
        {
            if(value is MySqlConnection dbConnection)
            this.connection.DbConnection = value;
        }
    }
    public Action<CommandEventArgs> OnExecuting { get; set; }
    public Action<CommandCompletedEventArgs> OnExecuted { get; set; }

    public PostgreSqlTheaCommand(string dbKey, NpgsqlCommand command, ITheaConnection connection, ITheaTransaction transaction)
    {
        this.DbKey = dbKey;
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
            ConnectionId = this.connection.ConnectionId,
            TransactionId = this.transaction?.TransactionId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType
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
                ConnectionId = this.connection.ConnectionId,
                TransactionId = this.transaction?.TransactionId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed
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
            ConnectionId = this.connection.ConnectionId,
            TransactionId = this.transaction?.TransactionId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType
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
                ConnectionId = this.connection.ConnectionId,
                TransactionId = this.transaction?.TransactionId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed
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
            ConnectionId = this.connection.ConnectionId,
            TransactionId = this.transaction?.TransactionId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType
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
                ConnectionId = this.connection.ConnectionId,
                TransactionId = this.transaction?.TransactionId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed
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
            ConnectionId = this.connection.ConnectionId,
            TransactionId = this.transaction?.TransactionId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType
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
                ConnectionId = this.connection.ConnectionId,
                TransactionId = this.transaction?.TransactionId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed
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
            ConnectionId = this.connection.ConnectionId,
            TransactionId = this.transaction?.TransactionId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType
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
                ConnectionId = this.connection.ConnectionId,
                TransactionId = this.transaction?.TransactionId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed
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
            ConnectionId = this.connection.ConnectionId,
            TransactionId = this.transaction?.TransactionId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType
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
                ConnectionId = this.connection.ConnectionId,
                TransactionId = this.transaction?.TransactionId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed
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
        this.command.Dispose();
        this.Parameters.Clear();
    }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public async ValueTask DisposeAsync()
    {
        await this.command.DisposeAsync();
        this.Parameters.Clear();
#else
    public ValueTask DisposeAsync()
    {
        this.Dispose();
        return default;
#endif
    }
}
