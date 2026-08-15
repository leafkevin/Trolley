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
            if (value is not PostgreSqlTheaTransaction theaTransaction)
                throw new NotSupportedException("不支持的事务类型，只支持PostgreSqlTheaTransaction类型");
            this.transaction = theaTransaction;
            this.DbKey = theaTransaction.DbKey;
            this.connection = this.transaction.Connection as PostgreSqlTheaConnection;
        }
    }
    public UpdateRowSource UpdatedRowSource
    {
        get => this.command.UpdatedRowSource;
        set => this.command.UpdatedRowSource = value;
    }

    public PostgreSqlTheaCommand(NpgsqlCommand command, PostgreSqlTheaConnection connection = null, PostgreSqlTheaTransaction transaction = null)
        : this(null, command, connection, transaction) { }
    public PostgreSqlTheaCommand(string dbKey, NpgsqlCommand command, PostgreSqlTheaConnection connection = null, PostgreSqlTheaTransaction transaction = null)
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
        this.OnExecuted(isSuccess, evtData, exception);
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
        this.OnExecuted(isSuccess, evtData, exception);
        if (!isSuccess)
        {
            await this.DisposeAsync();
            if (this.IsNeedClose) await this.connection.CloseAsync();
            throw exception;
        }
        return recordsAffected;
    }
    public ITheaDataReader ExecuteReader() => this.ExecuteReader(CommandBehavior.Default);
    public ITheaDataReader ExecuteReader(CommandBehavior behavior)
    {
        ITheaDataReader reader = null;
        bool isSuccess = true;
        Exception exception = null;
        if (!this.OnExecuting(out var evtData)) return reader;
        try
        {
            this.Interceptor?.DataReaderCreating(this);
            var dbReader = this.command.ExecuteReader(behavior);
            reader = new PostgreSqlTheaDataReader(dbReader) { Interceptor = this.Interceptor };
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        if (this.Interceptor != null)
            reader = this.Interceptor.DataReaderCreated(reader);
        this.OnExecuted(isSuccess, evtData, exception);
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
        ITheaDataReader reader = null;
        bool isSuccess = true;
        Exception exception = null;
        if (!this.OnExecuting(out var evtData)) return reader;
        try
        {
            this.Interceptor?.DataReaderCreating(this);
            var dbReader = await this.command.ExecuteReaderAsync(behavior, cancellationToken);
            reader = new PostgreSqlTheaDataReader(dbReader) { Interceptor = this.Interceptor };
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        if (this.Interceptor != null)
            reader = this.Interceptor.DataReaderCreated(reader);
        this.OnExecuted(isSuccess, evtData, exception);
        if (!isSuccess)
        {
            await this.DisposeAsync();
            if (this.IsNeedClose) await this.connection.CloseAsync();
            throw exception;
        }
        return reader;
    }
    public object ExecuteScalar()
    {
        object result = null;
        bool isSuccess = true;
        Exception exception = null;
        if (!this.OnExecuting(out var evtData)) return result;
        try
        {
            result = this.command.ExecuteScalar();
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        this.OnExecuted(isSuccess, evtData, exception);
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
        if (!this.OnExecuting(out var evtData)) return result;
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
            if (this.IsNeedClose) await this.connection.CloseAsync();
            throw exception;
        }
        return result;
    }
    public virtual async Task PrepareAsync(CancellationToken cancellationToken = default)
        => await this.command.PrepareAsync(cancellationToken);
    public void Dispose()
    {
        this.Interceptor?.CommandDisposing(this);
        this.command.Dispose();
        this.Interceptor?.CommandDisposed(this);
        this.Parameters.Clear();
    }
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public async ValueTask DisposeAsync()
    {
        this.Interceptor?.CommandDisposing(this);
        await this.command.DisposeAsync();
        this.Interceptor?.CommandDisposed(this);
        this.Parameters.Clear();
    }
#else
    public ValueTask DisposeAsync()
    {
        this.Interceptor?.CommandDisposing(this);
        this.command.Dispose();
        this.Interceptor?.CommandDisposed(this);
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
        var dbCommand = this.command.Clone();
        return new PostgreSqlTheaCommand(this.DbKey, dbCommand, this.connection, this.transaction);
    }

    IDbConnection IDbCommand.Connection
    {
        get => this.connection.DbConnection;
        set
        {
            if (value is PostgreSqlTheaConnection theaConnection)
            {
                this.connection = theaConnection;
                this.DbKey = theaConnection.DbKey;
            }
            else if (value is NpgsqlConnection dbConnection)
                this.connection.DbConnection = value;
            else throw new NotSupportedException("不支持的连接类型，只支持PostgreSqlTheaConnection类型");
        }
    }
    IDbTransaction IDbCommand.Transaction
    {
        get => this.transaction.DbTransaction;
        set
        {
            if (value is PostgreSqlTheaTransaction theaTransaction)
            {
                this.transaction = theaTransaction;
                if (!ReferenceEquals(this.connection, theaTransaction.Connection)
                    && theaTransaction.Connection is PostgreSqlTheaConnection theaConnection)
                {
                    this.connection = theaConnection;
                    this.DbKey = theaConnection.DbKey;
                }
            }
            else if (value is NpgsqlTransaction dbTransaction)
                this.transaction.DbTransaction = dbTransaction;
            else throw new NotSupportedException("不支持的连接类型，只支持PostgreSqlTheaConnection类型");
        }
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