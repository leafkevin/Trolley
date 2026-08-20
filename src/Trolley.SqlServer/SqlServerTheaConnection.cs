using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.SqlServer;

class SqlServerTheaConnection : ITheaConnection
{
    private SqlConnection connection;

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
            if (value is SqlServerTheaConnection theaConnection)
            {
                this.connection = theaConnection.connection;
                this.DbKey = theaConnection.DbKey;
            }
            else if (value is SqlConnection dbConnection)
                this.connection = dbConnection;
            else throw new NotSupportedException("不支持的连接类型，只支持SqlServerTheaConnection或是SqlConnection类型");
        }
    }
    public IDbInterceptor Interceptor { get; set; }

    public SqlServerTheaConnection(string dbKey, string connectionString)
        : this(dbKey, new SqlConnection(connectionString)) { }
    public SqlServerTheaConnection(string dbKey, SqlConnection connection)
    {
        this.DbKey = dbKey;
        this.ConnectionId = Guid.NewGuid().ToString("N");
        this.connection = connection;
    }

    public ITheaCommand CreateCommand()
    {
        var dbCommand = this.connection.CreateCommand();
        return new SqlServerTheaCommand(this.DbKey, dbCommand, this);
    }
    public void ChangeDatabase(string databaseName)
        => this.connection.ChangeDatabase(databaseName);
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
        this.Interceptor?.ConnectionClosed(this);
        if (!isSuccess) throw exception;
    }
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public async Task CloseAsync()
    {
        if (this.connection == null || this.State == ConnectionState.Closed)
            return;

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
        this.Interceptor?.ConnectionClosed(this);
        if (!isSuccess) throw exception;
    }
#else
    public Task CloseAsync()
    {
        if (this.connection == null || this.State == ConnectionState.Closed)
            return Task.CompletedTask;
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
        this.Interceptor?.ConnectionClosed(this);
        if (!isSuccess) throw exception;
        return Task.CompletedTask;
    }
#endif
    public void Open()
    {
        if (this.connection == null || this.State == ConnectionState.Open)
            return;
        if (this.State == ConnectionState.Broken)
            this.Close();

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
        this.Interceptor?.ConnectionOpened(this);
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
        this.Interceptor?.ConnectionOpened(this);
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
            transaction = new SqlServerTheaTransaction(this, dbTransaction);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        if (this.Interceptor != null)
            transaction = this.Interceptor.TransactionCreated(transaction);
        if (!isSuccess)
        {
            if (!isSuccess) this.Close();
            throw exception;
        }
        return transaction;
    }
    public async ValueTask<ITheaTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => await this.BeginTransactionAsync(IsolationLevel.Unspecified, cancellationToken);
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public async ValueTask<ITheaTransaction> BeginTransactionAsync(IsolationLevel il, CancellationToken cancellationToken = default)
    {
        bool isSuccess = true;
        Exception exception = null;
        ITheaTransaction transaction = null;
        this.Interceptor?.TransactionCreating(this);
        try
        {
            var dbTransaction = await this.connection.BeginTransactionAsync(il);
            transaction = new SqlServerTheaTransaction(this, dbTransaction as SqlTransaction);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        if (this.Interceptor != null)
            transaction = this.Interceptor.TransactionCreated(transaction);
        if (!isSuccess)
        {
            if (!isSuccess) await this.CloseAsync();
            throw exception;
        }
        return transaction;
    }
#else
    public ValueTask<ITheaTransaction> BeginTransactionAsync(IsolationLevel il, CancellationToken cancellationToken = default)
    {
        bool isSuccess = true;
        Exception exception = null;
        ITheaTransaction transaction = null;
        this.Interceptor?.TransactionCreating(this);
        try
        {
            var dbTransaction = this.connection.BeginTransaction(il);
            transaction = new SqlServerTheaTransaction(this, dbTransaction);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        if (this.Interceptor != null)
            transaction = this.Interceptor.TransactionCreated(transaction);
        if (!isSuccess)
        {
            if (!isSuccess) this.Close();
            throw exception;
        }
        return new ValueTask<ITheaTransaction>(transaction);
    }
#endif
    public void Dispose()
    {
        this.Interceptor?.ConnectionDisposing(this);
        this.connection.Dispose();
        this.Interceptor?.ConnectionDisposed(this);
    }
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public async ValueTask DisposeAsync()
    {
        this.Interceptor?.ConnectionDisposing(this);
        await this.connection.DisposeAsync();
        this.Interceptor?.ConnectionDisposed(this);
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
        => this.BeginTransaction().DbTransaction;
    IDbTransaction IDbConnection.BeginTransaction(IsolationLevel il)
        => this.BeginTransaction(il).DbTransaction;
}