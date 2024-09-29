using System;
using System.Data;
#if NET6_0_OR_GREATER
using System.Data.Common;
#endif
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.SqlConnector;

class SqlServerTheaConnection : ITheaConnection
{
    private readonly SqlConnection connection;

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

    public SqlServerTheaConnection(string dbKey, string connectionString)
        : this(dbKey, new SqlConnection(connectionString)) { }
    public SqlServerTheaConnection(string dbKey, SqlConnection connection)
    {
        this.DbKey = dbKey;
        this.ConnectionId = Guid.NewGuid().ToString("N");
        this.ConnectionString = connection.ConnectionString;
        this.connection = connection;
    }
    public void ChangeDatabase(string databaseName) => this.connection.ChangeDatabase(databaseName);
    public Task ChangeDatabaseAsync(string databaseName, CancellationToken cancellationToken = default)
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        => this.connection.ChangeDatabaseAsync(databaseName, cancellationToken);
#else
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
#endif

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
            ConnectionString = this.ConnectionString,
            CreatedAt = DateTime.Now
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
            ConnectionString = this.ConnectionString,
            CreatedAt = DateTime.Now
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
                ConnectionString = this.ConnectionString,
                CreatedAt = DateTime.Now
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
        if (command is not SqlCommand myCommand)
            return null;
        myCommand.Connection = this.connection;
        return new SqlServerTheaCommand(myCommand, this, null);
    }

#if NET6_0_OR_GREATER
    public DbBatch CreateBatch() => this.connection.CreateBatch();
#endif
    public ITheaTransaction BeginTransaction()
    {
        var transaction = this.connection.BeginTransaction();
        return new SqlServerTheaTransaction(this, transaction);
    }
    public ITheaTransaction BeginTransaction(IsolationLevel il)
    {
        var transaction = this.connection.BeginTransaction(il);
        return new SqlServerTheaTransaction(this, transaction);
    }
    public ValueTask<ITheaTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return new ValueTask<ITheaTransaction>(Task.FromCanceled<ITheaTransaction>(cancellationToken));
        try
        {
            var transaction = this.connection.BeginTransaction();
            return new ValueTask<ITheaTransaction>(new SqlServerTheaTransaction(this, transaction));
        }
        catch (Exception e)
        {
            return new ValueTask<ITheaTransaction>(Task.FromException<ITheaTransaction>(e));
        }
    }
    public ValueTask<ITheaTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return new ValueTask<ITheaTransaction>(Task.FromCanceled<ITheaTransaction>(cancellationToken));
        try
        {
            var transaction = this.connection.BeginTransaction(isolationLevel);
            return new ValueTask<ITheaTransaction>(new SqlServerTheaTransaction(this, transaction));
        }
        catch (Exception e)
        {
            return new ValueTask<ITheaTransaction>(Task.FromException<ITheaTransaction>(e));
        }
    }
    public void EnlistTransaction(System.Transactions.Transaction transaction)
        => this.connection.EnlistTransaction(transaction);
    public DataTable GetSchema() => this.connection.GetSchema();
    public DataTable GetSchema(string collectionName) => this.connection.GetSchema(collectionName);
    public DataTable GetSchema(string collectionName, string[] restrictionValues)
        => this.connection.GetSchema(collectionName, restrictionValues);
    public Task<DataTable> GetSchemaAsync(CancellationToken cancellationToken = default)
#if NETCOREAPP3_0_OR_GREATER
        => this.connection.GetSchemaAsync(cancellationToken);
#else
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<DataTable>(cancellationToken);
        try
        {
            return Task.FromResult(this.connection.GetSchema());
        }
        catch (Exception e)
        {
            return Task.FromException<DataTable>(e);
        }
    }
#endif
    public Task<DataTable> GetSchemaAsync(string collectionName, CancellationToken cancellationToken = default)
#if NETCOREAPP3_0_OR_GREATER
        => this.connection.GetSchemaAsync(collectionName, cancellationToken);
#else
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<DataTable>(cancellationToken);
        try
        {
            return Task.FromResult(this.connection.GetSchema(collectionName));
        }
        catch (Exception e)
        {
            return Task.FromException<DataTable>(e);
        }
    }
#endif
    public Task<DataTable> GetSchemaAsync(string collectionName, string[] restrictionValues, CancellationToken cancellationToken = default)
#if NETCOREAPP3_0_OR_GREATER
        => this.connection.GetSchemaAsync(collectionName, restrictionValues, cancellationToken);
#else
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<DataTable>(cancellationToken);
        try
        {
            return Task.FromResult(this.connection.GetSchema(collectionName, restrictionValues));
        }
        catch (Exception e)
        {
            return Task.FromException<DataTable>(e);
        }
    }
#endif
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