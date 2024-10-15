using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.SqlServer;

class SqlServerTheaConnection : ITheaConnection
{
    private readonly SqlConnection connection;

    public string DbKey { get; private set; }
    public string ConnectionId { get; private set; }

    public string ConnectionString { get; set; }
    public int ConnectionTimeout => this.connection.ConnectionTimeout;
    public string Database => this.connection.Database;
    public string ServerVersion => this.connection.ServerVersion;

    public ConnectionState State => this.connection.State;
    public IDbConnection BaseConnection => this.connection;

    public Action<ConectionEventArgs> OnOpening { get; set; }
    public Action<ConectionEventArgs> OnOpened { get; set; }
    public Action<ConectionEventArgs> OnClosing { get; set; }
    public Action<ConectionEventArgs> OnClosed { get; set; }
    public Action<TransactionEventArgs> OnTransactionCreated { get; set; }
    public Action<TransactionCompletedEventArgs> OnTransactionCompleted { get; set; }

    public SqlServerTheaConnection(string dbKey, string connectionString)
        : this(dbKey, new SqlConnection(connectionString)) { }
    public SqlServerTheaConnection(string dbKey, SqlConnection connection)
    {
        this.DbKey = dbKey;
        this.ConnectionId = Guid.NewGuid().ToString("N");
        this.ConnectionString = connection.ConnectionString;
        this.connection = connection;
    }

    public void Close()
    {
        if (this.connection == null || this.State == ConnectionState.Closed) return;
        this.OnClosing?.Invoke(new ConectionEventArgs
        {
            DbKey = this.DbKey,
            ConnectionId = this.ConnectionId,
            ConnectionString = this.ConnectionString
        });
        this.connection.Close();
        this.OnClosed?.Invoke(new ConectionEventArgs
        {
            DbKey = this.DbKey,
            ConnectionId = this.ConnectionId,
            ConnectionString = this.ConnectionString
        });
    }
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public async Task CloseAsync()
    {
        if (this.connection == null || this.State == ConnectionState.Closed)
            return;
#else
    public Task CloseAsync()
    {
        if (this.connection == null || this.State == ConnectionState.Closed)
            return Task.CompletedTask;
#endif
        this.OnClosing?.Invoke(new ConectionEventArgs
        {
            DbKey = this.DbKey,
            ConnectionId = this.ConnectionId,
            ConnectionString = this.ConnectionString
        });
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        await this.connection.CloseAsync();
#else
        this.connection.Close();
#endif
        this.OnClosed?.Invoke(new ConectionEventArgs
        {
            DbKey = this.DbKey,
            ConnectionId = this.ConnectionId,
            ConnectionString = this.ConnectionString
        });
#if !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
        return Task.CompletedTask;
#endif
    }
    public void Open()
    {
        if (this.connection == null || this.State == ConnectionState.Open) return;
        if (this.State == ConnectionState.Broken)
        {
            this.connection.Close();
            this.OnClosed?.Invoke(new ConectionEventArgs
            {
                DbKey = this.DbKey,
                ConnectionId = this.ConnectionId,
                ConnectionString = this.ConnectionString
            });
        }
        if (this.State == ConnectionState.Closed)
        {
            //关闭后，连接串被重置，需要重新设置
            this.connection.ConnectionString = this.ConnectionString;
            this.OnOpening?.Invoke(new ConectionEventArgs
            {
                DbKey = this.DbKey,
                ConnectionId = this.ConnectionId,
                ConnectionString = this.ConnectionString
            });
            this.connection.Open();
            this.OnOpened?.Invoke(new ConectionEventArgs
            {
                DbKey = this.DbKey,
                ConnectionId = this.ConnectionId,
                ConnectionString = this.ConnectionString
            });
        }
    }
    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        if (this.connection == null || this.State == ConnectionState.Open) return;
        if (this.State == ConnectionState.Broken)
            await this.CloseAsync();
        if (this.State == ConnectionState.Closed)
        {
            //关闭后，连接串被重置，需要重新设置
            this.connection.ConnectionString = this.ConnectionString;
            this.OnOpening?.Invoke(new ConectionEventArgs
            {
                DbKey = this.DbKey,
                ConnectionId = this.ConnectionId,
                ConnectionString = this.ConnectionString
            });
            await this.connection.OpenAsync(cancellationToken);
            this.OnOpened?.Invoke(new ConectionEventArgs
            {
                DbKey = this.DbKey,
                ConnectionId = this.ConnectionId,
                ConnectionString = this.ConnectionString
            });
        }
    }
    public ITheaCommand CreateCommand(IDbCommand command)
    {
        if (command is not SqlCommand myCommand)
            return null;
        myCommand.Connection = this.connection;
        return new SqlServerTheaCommand(myCommand, this, null);
    }
    public ITheaTransaction BeginTransaction()
    {
        var transaction = this.connection.BeginTransaction();
        var theaTransaction = new SqlServerTheaTransaction(this, transaction)
        {
            OnCreated = this.OnTransactionCreated,
            OnCompleted = this.OnTransactionCompleted
        };
        this.OnTransactionCreated?.Invoke(new TransactionEventArgs
        {
            DbKey = this.DbKey,
            TransactionId = theaTransaction.TransactionId,
            ConnectionId = this.ConnectionId,
            ConnectionString = this.ConnectionString
        });
        return theaTransaction;
    }
    public ValueTask<ITheaTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return new ValueTask<ITheaTransaction>(Task.FromCanceled<ITheaTransaction>(cancellationToken));
        try
        {
            var transaction = this.connection.BeginTransaction();
            var theaTransaction = new SqlServerTheaTransaction(this, transaction)
            {
                OnCreated = this.OnTransactionCreated,
                OnCompleted = this.OnTransactionCompleted
            };
            this.OnTransactionCreated?.Invoke(new TransactionEventArgs
            {
                DbKey = this.DbKey,
                TransactionId = theaTransaction.TransactionId,
                ConnectionId = this.ConnectionId,
                ConnectionString = this.ConnectionString
            });
            return new ValueTask<ITheaTransaction>(new SqlServerTheaTransaction(this, transaction));
        }
        catch (Exception e)
        {
            return new ValueTask<ITheaTransaction>(Task.FromException<ITheaTransaction>(e));
        }
    }

    public void Dispose() => this.connection.Dispose();
    public ValueTask DisposeAsync()
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        => this.connection.DisposeAsync();
#else
    {
        this.connection.Dispose();
        return default;
    }
#endif
}