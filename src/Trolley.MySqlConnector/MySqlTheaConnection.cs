using MySqlConnector;
using System;
using System.Data;
#if NET6_0_OR_GREATER
using System.Data.Common;
#endif
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

class MySqlTheaConnection : ITheaConnection
{
    private readonly MySqlConnection connection;

    public string DbKey { get; private set; }
    public string ConnectionId { get; private set; }

    public string ConnectionString { get; set; }
    public int ConnectionTimeout => this.connection.ConnectionTimeout;
    public string Database => this.connection.Database;
    public string DataSource => this.connection.DataSource;
    public string ServerVersion => this.connection.ServerVersion;
    public bool CanCreateBatch => this.connection.CanCreateBatch;
    public ConnectionState State => this.connection.State;
    public IDbConnection BaseConnection => this.connection;

    public Action<ConectionEventArgs> OnCreated { get; set; }
    public Action<ConectionEventArgs> OnOpening { get; set; }
    public Action<ConectionEventArgs> OnOpened { get; set; }
    public Action<ConectionEventArgs> OnClosing { get; set; }
    public Action<ConectionEventArgs> OnClosed { get; set; }

    public MySqlTheaConnection(string dbKey, string connectionString)
        : this(dbKey, new MySqlConnection(connectionString)) { }
    public MySqlTheaConnection(string dbKey, MySqlConnection connection)
    {
        this.DbKey = dbKey;
        this.ConnectionId = Guid.NewGuid().ToString("N");
        this.ConnectionString = connection.ConnectionString;
        this.connection = connection;
    }
    public void ChangeDatabase(string databaseName) => this.connection.ChangeDatabase(databaseName);
    public async Task ChangeDatabaseAsync(string databaseName, CancellationToken cancellationToken = default)
        => await this.connection.ChangeDatabaseAsync(databaseName, cancellationToken);
    public void Close()
    {
        if (this.connection == null || this.State == ConnectionState.Closed) return;
        this.OnClosing?.Invoke(new ConectionEventArgs
        {
            DbKey = this.DbKey,
            ConnectionId = this.ConnectionId,
            ConnectionString = this.ConnectionString,
            CreatedAt = DateTime.Now
        });
        this.connection.Close();
        this.OnClosed?.Invoke(new ConectionEventArgs
        {
            DbKey = this.DbKey,
            ConnectionId = this.ConnectionId,
            ConnectionString = this.ConnectionString,
            CreatedAt = DateTime.Now
        });
    }
    public async Task CloseAsync()
    {
        if (this.connection == null || this.State == ConnectionState.Closed) return;
        this.OnClosing?.Invoke(new ConectionEventArgs
        {
            DbKey = this.DbKey,
            ConnectionId = this.ConnectionId,
            ConnectionString = this.ConnectionString,
            CreatedAt = DateTime.Now
        });
        await this.connection.CloseAsync();
        this.OnClosed?.Invoke(new ConectionEventArgs
        {
            DbKey = this.DbKey,
            ConnectionId = this.ConnectionId,
            ConnectionString = this.ConnectionString,
            CreatedAt = DateTime.Now
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
                ConnectionString = this.ConnectionString,
                CreatedAt = DateTime.Now
            });
            this.connection.Open();
            this.OnOpened?.Invoke(new ConectionEventArgs
            {
                DbKey = this.DbKey,
                ConnectionId = this.ConnectionId,
                ConnectionString = this.ConnectionString,
                CreatedAt = DateTime.Now
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
                ConnectionString = this.ConnectionString,
                CreatedAt = DateTime.Now
            });
            await this.connection.OpenAsync(cancellationToken);
            this.OnOpened?.Invoke(new ConectionEventArgs
            {
                DbKey = this.DbKey,
                ConnectionId = this.ConnectionId,
                ConnectionString = this.ConnectionString,
                CreatedAt = DateTime.Now
            });
        }
    }
    public ITheaCommand CreateCommand(IDbCommand command)
    {
        if (command is not MySqlCommand myCommand)
            return null;
        myCommand.Connection = this.connection;
        return new MySqlTheaCommand(myCommand, this, null);
    }

#if NET6_0_OR_GREATER
    public DbBatch CreateBatch() => this.connection.CreateBatch();
#endif
    public ITheaTransaction BeginTransaction()
    {
        var transaction = this.connection.BeginTransaction();
        return new MySqlTheaTransaction(this, transaction);
    }
    public ITheaTransaction BeginTransaction(IsolationLevel il)
    {
        var transaction = this.connection.BeginTransaction(il);
        return new MySqlTheaTransaction(this, transaction);
    }
    public async ValueTask<ITheaTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await this.connection.BeginTransactionAsync();
        return new MySqlTheaTransaction(this, transaction);
    }
    public async ValueTask<ITheaTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        var transaction = await this.connection.BeginTransactionAsync(isolationLevel, cancellationToken);
        return new MySqlTheaTransaction(this, transaction);
    }
    public void EnlistTransaction(System.Transactions.Transaction transaction)
        => this.connection.EnlistTransaction(transaction);
    public DataTable GetSchema() => this.connection.GetSchema();
    public DataTable GetSchema(string collectionName) => this.connection.GetSchema(collectionName);
    public DataTable GetSchema(string collectionName, string[] restrictionValues)
        => this.connection.GetSchema(collectionName, restrictionValues);
    public async Task<DataTable> GetSchemaAsync(CancellationToken cancellationToken = default)
        => await this.connection.GetSchemaAsync(cancellationToken);
    public async Task<DataTable> GetSchemaAsync(string collectionName, CancellationToken cancellationToken = default)
        => await this.connection.GetSchemaAsync(collectionName, cancellationToken);
    public async Task<DataTable> GetSchemaAsync(string collectionName, string[] restrictionValues, CancellationToken cancellationToken = default)
        => await this.connection.GetSchemaAsync(collectionName, restrictionValues, cancellationToken);
    public void Dispose() => this.connection.Dispose();
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public ValueTask DisposeAsync() => this.connection.DisposeAsync();
#else
    public ValueTask DisposeAsync() => new ValueTask(this.connection.DisposeAsync());
#endif
}