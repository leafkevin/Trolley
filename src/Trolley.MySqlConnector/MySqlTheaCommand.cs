using MySqlConnector;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

class MySqlTheaCommand : ITheaCommand
{
    private readonly MySqlCommand command;
    private MySqlTheaConnection connection;
    private MySqlTheaTransaction transaction;

    public string DbKey { get; private set; }
    public string CommandId { get; private set; }
    public IDbCommand DbCommand => this.command;
    public IDbConnection DbConnection => this.connection;
    public IDbTransaction DbTransaction => this.transaction;
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
    public IDbConnection Connection
    {
        get => this.connection;
        set
        {
            if (value is MySqlTheaConnection theaConnection)
            {
                this.connection = theaConnection;
                if (string.IsNullOrEmpty(this.DbKey))
                    this.DbKey = theaConnection.DbKey;
            }
            else if (value is MySqlConnection dbConnection)
                this.connection = new MySqlTheaConnection(this.DbKey, dbConnection);
            else throw new NotSupportedException("不支持的连接类型，只支持MySqlTheaConnection类型或是MySqlConnection类型");
        }
    }
    public IDbTransaction Transaction
    {
        get => this.transaction;
        set
        {
            if (value is MySqlTheaTransaction theaTransaction)
            {
                this.transaction = theaTransaction;
                if (string.IsNullOrEmpty(this.DbKey))
                    this.DbKey = theaTransaction.DbKey;
            }
            else if (value is MySqlTransaction dbTransaction)
                this.transaction = new MySqlTheaTransaction(this.DbKey, this.connection, dbTransaction);
            else throw new NotSupportedException("不支持的事务类型，只支持MySqlTheaTransaction类型或是MySqlTransaction类型");
            if (!ReferenceEquals(this.connection, this.transaction.Connection))
                this.connection = this.transaction.Connection as MySqlTheaConnection;
        }
    }
    public UpdateRowSource UpdatedRowSource
    {
        get => this.command.UpdatedRowSource;
        set => this.command.UpdatedRowSource = value;
    }

    public MySqlTheaCommand(MySqlCommand command, MySqlTheaConnection connection = null, MySqlTheaTransaction transaction = null)
    {
        this.CommandId = Guid.NewGuid().ToString("N");
        this.command = command;
        this.Connection = connection;
        this.transaction = transaction;
    }
    public MySqlTheaCommand(string dbKey, MySqlCommand command, MySqlTheaConnection connection = null, MySqlTheaTransaction transaction = null)
    {
        this.DbKey = dbKey;
        this.CommandId = Guid.NewGuid().ToString("N");
        this.command = command;
        this.Connection = connection;
        this.transaction = transaction;
    }

    public void Prepare() => this.command.Prepare();
    public void Cancel() => this.command.Cancel();
    public IDbDataParameter CreateParameter() => this.command.CreateParameter();
    public int ExecuteNonQuery()
    {
        int recordsAffected = 0;
        bool isSuccess = true;
        Exception exception = null;
        if (!this.OnExecuting(out var evtData)) return 0;
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
            this.OnExecuted(isSuccess, evtData, exception);
        }
        if (!isSuccess)
        {
            this.Dispose();
            if (this.IsNeedClose) this.connection.Close();
            throw exception;
        }
        return recordsAffected;
    }
    public async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default)
    {
        int recordsAffected = 0;
        bool isSuccess = true;
        Exception exception = null;
        if (!this.OnExecuting(out var evtData)) return 0;
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
            this.OnExecuted(isSuccess, evtData, exception);
        }
        if (!isSuccess)
        {
            this.Dispose();
            if (this.IsNeedClose) this.connection.Close();
            throw exception;
        }
        return recordsAffected;
    }
    public ITheaDataReader ExecuteReader() => this.ExecuteReader(CommandBehavior.Default);
    public ITheaDataReader ExecuteReader(CommandBehavior behavior)
    {
        MySqlTheaDataReader reader = null;
        bool isSuccess = true;
        Exception exception = null;
        if (!this.OnExecuting(out var evtData)) return reader;
        try
        {
            var dbReader = this.command.ExecuteReader(behavior);
            reader = new MySqlTheaDataReader(dbReader);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            this.OnExecuted(isSuccess, evtData, exception);
        }
        if (!isSuccess)
        {
            this.Dispose();
            if (this.IsNeedClose) this.connection.Close();
            throw exception;
        }
        return reader;
    }
    public async Task<ITheaDataReader> ExecuteReaderAsync(CancellationToken cancellationToken = default)
        => await this.ExecuteReaderAsync(CommandBehavior.Default, cancellationToken);
    public async Task<ITheaDataReader> ExecuteReaderAsync(CommandBehavior behavior = default, CancellationToken cancellationToken = default)
    {
        MySqlTheaDataReader reader = null;
        bool isSuccess = true;
        Exception exception = null;
        if (!this.OnExecuting(out var evtData)) return reader;
        try
        {
            var dbReader = await this.command.ExecuteReaderAsync(behavior, cancellationToken);
            reader = new MySqlTheaDataReader(dbReader);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            this.OnExecuted(isSuccess, evtData, exception);
        }
        if (!isSuccess)
        {
            await this.DisposeAsync();
            if (this.IsNeedClose) this.connection.Close();
            throw exception;
        }
        return reader;
    }
    public object ExecuteScalar()
    {
        object result = null;
        bool isSuccess = true;
        Exception exception = null;
        if (!this.OnExecuting(out var evtData)) return 0;
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
            this.OnExecuted(isSuccess, evtData, exception);
        }
        if (!isSuccess)
        {
            this.Dispose();
            if (this.IsNeedClose) this.connection.Close();
            throw exception;
        }
        return result;
    }
    public async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken = default)
    {
        object result = null;
        bool isSuccess = true;
        Exception exception = null;
        if (!this.OnExecuting(out var evtData)) return 0;
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
            this.OnExecuted(isSuccess, evtData, exception);
        }
        if (!isSuccess)
        {
            await this.DisposeAsync();
            if (this.IsNeedClose) this.connection.Close();
            throw exception;
        }
        return result;
    }
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public virtual async Task PrepareAsync(CancellationToken cancellationToken = default)
        => await this.command.PrepareAsync(cancellationToken);
#else
    public virtual Task PrepareAsync(CancellationToken cancellationToken = default)
    {
        this.command.Prepare();
        return Task.CompletedTask;
    }
#endif
    public void Dispose()
    {
        this.command.Dispose();
        this.Parameters.Clear();
    }
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public ValueTask DisposeAsync()
    {
        this.command.DisposeAsync();
        this.Parameters.Clear();
        return default;
    }
#else
    public ValueTask DisposeAsync()
    {
        this.command.Dispose();
        this.Parameters.Clear();
        return default;
    }
#endif
    private bool OnExecuting(out object evtData)
    {
        if (this.Interceptor != null)
        {
            var evtArgs = this.Interceptor.CommandExecuting(this);
            evtData = evtArgs.EventData;
            return evtArgs.IsCanExecuting;
        }
        evtData = null;
        return true;
    }
    private void OnExecuted(bool isSuccess, object evtData, Exception exception)
    {
        if (this.Interceptor != null)
        {
            var evtAgs = new DbCommandCompletedEventArgs
            {
                Command = this,
                IsSuccess = isSuccess,
                EventData = evtData,
                Exception = exception
            };
            this.Interceptor.CommandExecuted(evtAgs);
            if (!isSuccess) this.Interceptor.CommandFailed(evtAgs);
        }
    }
    public object Clone()
    {
        var dbCommand = this.connection.DbConnection.CreateCommand() as MySqlCommand;
        var command = new MySqlTheaCommand(this.DbKey, dbCommand, this.connection, this.transaction)
        {
            CommandText = this.CommandText,
            CommandType = this.CommandType
        };
        GC.SuppressFinalize(this);
        return command;
    }

    IDataReader IDbCommand.ExecuteReader()
    {
        var reader = this.ExecuteReader();
        return reader.DbDataReader;
    }
    IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
    {
        var reader = this.ExecuteReader(behavior);
        return reader.DbDataReader;
    }
}