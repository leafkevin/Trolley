using Npgsql;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.PostgreSql;

class PostgreSqlTheaConnection : ITheaConnection
{
    private readonly NpgsqlConnection connection;

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

    public PostgreSqlTheaConnection(string dbKey, string connectionString)
        : this(dbKey, new NpgsqlConnection(connectionString)) { }
    public PostgreSqlTheaConnection(string dbKey, NpgsqlConnection connection)
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
    public async Task CloseAsync()
    {
        if (this.connection == null || this.State == ConnectionState.Closed) return;
        this.OnClosing?.Invoke(new ConectionEventArgs
        {
            DbKey = this.DbKey,
            ConnectionId = this.ConnectionId,
            ConnectionString = this.ConnectionString
        });
        await this.connection.CloseAsync();
        this.OnClosed?.Invoke(new ConectionEventArgs
        {
            DbKey = this.DbKey,
            ConnectionId = this.ConnectionId,
            ConnectionString = this.ConnectionString
        });
    }
    public void Open()
    {
        if (this.connection == null || this.State == ConnectionState.Open) return;
        if (this.State == ConnectionState.Broken)
            this.Close();
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
        if (command is not NpgsqlCommand myCommand)
            return null;
        myCommand.Connection = this.connection;
        return new PostgreSqlTheaCommand(myCommand, this, null);
    }

    public ITheaTransaction BeginTransaction()
    {
        var transaction = this.connection.BeginTransaction();
        var theaTransaction = new PostgreSqlTheaTransaction(this, transaction)
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
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
    public async ValueTask<ITheaTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await this.connection.BeginTransactionAsync(cancellationToken);
        var theaTransaction = new PostgreSqlTheaTransaction(this, transaction)
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
#else
    public ValueTask<ITheaTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        ValueTask<ITheaTransaction> result = default;
        if (cancellationToken.IsCancellationRequested)
            result = new ValueTask<ITheaTransaction>(Task.FromCanceled<ITheaTransaction>(cancellationToken));
        else
        {
            PostgreSqlTheaTransaction theaTransaction = null;
            try
            {
                var transaction = this.connection.BeginTransaction();
                theaTransaction = new PostgreSqlTheaTransaction(this, transaction)
                {
                    OnCreated = this.OnTransactionCreated,
                    OnCompleted = this.OnTransactionCompleted
                };
                result = new ValueTask<ITheaTransaction>(theaTransaction);
            }
            catch (Exception e)
            {
                result = new ValueTask<ITheaTransaction>(Task.FromException<ITheaTransaction>(e));
            }
            this.OnTransactionCreated?.Invoke(new TransactionEventArgs
            {
                DbKey = this.DbKey,
                TransactionId = theaTransaction.TransactionId,
                ConnectionId = this.ConnectionId,
                ConnectionString = this.ConnectionString
            });
        }
        return result;
    }
#endif
    public void Dispose() => this.connection.Dispose();
    public ValueTask DisposeAsync() => this.connection.DisposeAsync();
}