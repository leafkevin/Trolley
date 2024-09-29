using Npgsql;
using System;
using System.Data;
#if NET6_0_OR_GREATER
using System.Data.Common;
#endif
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
    public string DataSource => this.connection.DataSource;
    public string ServerVersion => this.connection.ServerVersion;
#if NET6_0_OR_GREATER
    public bool CanCreateBatch => this.connection.CanCreateBatch;
#else
    public bool CanCreateBatch => false;
#endif
    public ConnectionState State => this.connection.State;
    public IDbConnection BaseConnection => this.connection;

    public Action<ConectionEventArgs> OnCreated { get; set; }
    public Action<ConectionEventArgs> OnOpening { get; set; }
    public Action<ConectionEventArgs> OnOpened { get; set; }
    public Action<ConectionEventArgs> OnClosing { get; set; }
    public Action<ConectionEventArgs> OnClosed { get; set; }

    public PostgreSqlTheaConnection(string dbKey, string connectionString)
        : this(dbKey, new NpgsqlConnection(connectionString)) { }
    public PostgreSqlTheaConnection(string dbKey, NpgsqlConnection connection)
    {
        this.DbKey = dbKey;
        this.ConnectionId = Guid.NewGuid().ToString("N");
        this.ConnectionString = connection.ConnectionString;
        this.connection = connection;
    }
    public void ChangeDatabase(string databaseName) => this.connection.ChangeDatabase(databaseName);
    public Task ChangeDatabaseAsync(string databaseName, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);
        try
        {
            this.connection.ChangeDatabase(databaseName);
            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            return Task.FromException(e);
        }
    }
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
        if (command is not NpgsqlCommand myCommand)
            return null;
        myCommand.Connection = this.connection;
        return new PostgreSqlTheaCommand(myCommand, this, null);
    }

#if NET6_0_OR_GREATER
    public DbBatch CreateBatch() => this.connection.CreateBatch();
#endif
    public ITheaTransaction BeginTransaction()
    {
        var transaction = this.connection.BeginTransaction();
        return new PostgreSqlTheaTransaction(this, transaction);
    }
    public ITheaTransaction BeginTransaction(IsolationLevel il)
    {
        var transaction = this.connection.BeginTransaction(il);
        return new PostgreSqlTheaTransaction(this, transaction);
    }
#if NETSTANDARD2_1_OR_GREATER
    public async ValueTask<ITheaTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await this.connection.BeginTransactionAsync(cancellationToken);
        return new PostgreSqlTheaTransaction(this, transaction);
    }
    public async ValueTask<ITheaTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        var transaction = await this.connection.BeginTransactionAsync(isolationLevel, cancellationToken);
        return new PostgreSqlTheaTransaction(this, transaction);
    }
#else
    public ValueTask<ITheaTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = this.connection.BeginTransaction();
        return new ValueTask<ITheaTransaction>(new PostgreSqlTheaTransaction(this, transaction));
    }
    public ValueTask<ITheaTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken cancellationToken = default)
    {
        var transaction = this.connection.BeginTransaction(isolationLevel);
        return new ValueTask<ITheaTransaction>(new PostgreSqlTheaTransaction(this, transaction));
    }
#endif
    public void EnlistTransaction(System.Transactions.Transaction transaction)
        => this.connection.EnlistTransaction(transaction);
    public DataTable GetSchema() => this.connection.GetSchema();
    public DataTable GetSchema(string collectionName) => this.connection.GetSchema(collectionName);
    public DataTable GetSchema(string collectionName, string[] restrictionValues = default)
        => this.connection.GetSchema(collectionName, restrictionValues);

#if NETSTANDARD2_1_OR_GREATER
    public Task<DataTable> GetSchemaAsync(CancellationToken cancellationToken = default)
        => this.connection.GetSchemaAsync(cancellationToken);
    public Task<DataTable> GetSchemaAsync(string collectionName, CancellationToken cancellationToken = default)
        => this.connection.GetSchemaAsync(collectionName, cancellationToken);
    public Task<DataTable> GetSchemaAsync(string collectionName, string[] restrictionValues, CancellationToken cancellationToken = default)
        => this.connection.GetSchemaAsync(collectionName, restrictionValues, cancellationToken);
#else
    public Task<DataTable> GetSchemaAsync(CancellationToken cancellationToken = default)
    {
        var schema = this.connection.GetSchema("MetaDataCollections", null);
        return Task.FromResult(schema);
    }
    public Task<DataTable> GetSchemaAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        var schema = this.connection.GetSchema(collectionName, null);
        return Task.FromResult(schema);
    }
    public Task<DataTable> GetSchemaAsync(string collectionName, string[] restrictionValues, CancellationToken cancellationToken = default)
    {
        var schema = this.connection.GetSchema(collectionName, restrictionValues);
        return Task.FromResult(schema);
    }
#endif
    public void Dispose() => this.connection.Dispose();
    public ValueTask DisposeAsync() => this.connection.DisposeAsync();
}
