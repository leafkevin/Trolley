using MySqlConnector;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

class MySqlTheaConnection : ITheaConnection
{
    private MySqlConnection connection;

    public string DbKey { get; private set; }
    public string ConnectionId { get; private set; }
    public string ConnectionString
    {
        get => this.connection.ConnectionString;
        set => this.connection.ConnectionString = value;
    }
    public int ConnectionTimeout => this.connection.ConnectionTimeout;
    public string Database => this.connection.Database;
    public string ServerVersion => this.connection.ServerVersion;
    public ConnectionState State => this.connection.State;
    public IDbConnection DbConnection
    {
        get => this.connection;
        internal set
        {
            if (value is MySqlConnection dbConnection)
                this.connection = dbConnection;
            if (value is MySqlTheaConnection theaConnection)
                this.connection = theaConnection.connection;
        }
    }
    public IDbInterceptor Interceptor { get; set; }

    public MySqlTheaConnection(string dbKey, string connectionString)
        : this(dbKey, new MySqlConnection(connectionString)) { }
    public MySqlTheaConnection(string dbKey, MySqlConnection connection)
    {
        this.DbKey = dbKey;
        this.ConnectionId = Guid.NewGuid().ToString("N");
        this.ConnectionString = connection.ConnectionString;
        this.connection = connection;
    }

    public ITheaCommand CreateCommand()
    {
        var dbCommand = this.connection.CreateCommand();
        return new MySqlTheaCommand(this.DbKey, dbCommand, this);
    }   
    public void ChangeDatabase(string databaseName)
        => this.connection.ChangeDatabaseAsync(databaseName);
    public void Close()
    {
        if (this.connection == null || this.State == ConnectionState.Closed)
            return;

        bool isSuccess = true;
        Exception exception = null;
        this.Interceptor?.ConnectionClosing(this);
        try
        {
            this.connection.Close();
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            this.Interceptor?.ConnectionClosed(this);
        }
        if (!isSuccess) throw exception;
    }
    public async Task CloseAsync()
    {
        if (this.connection == null || this.State == ConnectionState.Closed) return;

        bool isSuccess = true;
        Exception exception = null;
        this.Interceptor?.ConnectionClosing(this);
        try
        {
            await this.connection.CloseAsync();
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            this.Interceptor?.ConnectionClosed(this);
        }
        if (!isSuccess) throw exception;
    }
    public void Open()
    {
        if (this.connection == null || this.State == ConnectionState.Open)
            return;

        bool isSuccess = true;
        Exception exception = null;
        this.Interceptor?.ConnectionOpening(this);
        try
        {
            this.connection.Open();
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            this.Interceptor?.ConnectionOpened(this);
        }
        if (!isSuccess) throw exception;
    }
    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        if (this.connection == null || this.State == ConnectionState.Open)
            return;
        if (this.State == ConnectionState.Broken)
            await this.CloseAsync();

        bool isSuccess = true;
        Exception exception = null;
        this.Interceptor?.ConnectionOpening(this);
        try
        {
            await this.connection.OpenAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            this.Interceptor?.ConnectionOpened(this);
        }
        if (!isSuccess) throw exception;
    }
    public ITheaTransaction BeginTransaction() => this.BeginTransaction(IsolationLevel.Unspecified);
    public ITheaTransaction BeginTransaction(IsolationLevel il)
    {
        bool isSuccess = true;
        Exception exception = null;
        ITheaTransaction transaction = null;
        this.Interceptor?.TransactionCreating(this);
        try
        {
            var dbTransaction = this.connection.BeginTransaction(il);
            transaction = new MySqlTheaTransaction(this.DbKey, this, dbTransaction);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            this.Interceptor?.ConnectionOpened(this);
        }
        if (!isSuccess)
        {
            if (!isSuccess) this.Close();
            throw exception;
        }
        return transaction;
    }
    public async ValueTask<ITheaTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => await this.BeginTransactionAsync(IsolationLevel.Unspecified, cancellationToken);
    public async ValueTask<ITheaTransaction> BeginTransactionAsync(IsolationLevel il, CancellationToken cancellationToken = default)
    {
        bool isSuccess = true;
        Exception exception = null;
        ITheaTransaction transaction = null;
        this.Interceptor?.TransactionCreating(this);
        try
        {
            var dbTransaction = await this.connection.BeginTransactionAsync(il);
            transaction = new MySqlTheaTransaction(this.DbKey, this, dbTransaction);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            this.Interceptor?.ConnectionOpened(this);
        }
        if (!isSuccess)
        {
            if (!isSuccess) this.Close();
            throw exception;
        }
        return transaction;
    }
    public void Dispose()
    {
        this.Interceptor?.ConnectionDisposing(this);
        this.connection.Dispose();
        this.Interceptor?.ConnectionDisposed(this);
    }
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public ValueTask DisposeAsync()
    {
        this.connection.DisposeAsync();
        return default;
    }
#else
    public ValueTask DisposeAsync()
    {
        this.Interceptor?.ConnectionDisposing(this);
        this.connection.Dispose();
        this.Interceptor?.ConnectionDisposed(this);
        return default;
    }
#endif
    IDbCommand IDbConnection.CreateCommand() => this.CreateCommand().DbCommand;
    IDbTransaction IDbConnection.BeginTransaction()
    {
        var transaction = this.BeginTransaction();
        return transaction.DbTransaction;
    }
    IDbTransaction IDbConnection.BeginTransaction(IsolationLevel il)
    {
        var transaction = this.BeginTransaction(il);
        return transaction.DbTransaction;
    }
}