using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public sealed class TheaConnection : IDbConnection
{
    public string ConnectionId { get; set; }
    public IDbConnection BaseConnection { get; set; }
    public string ConnectionString
    {
        get => this.BaseConnection.ConnectionString;
        set => this.BaseConnection.ConnectionString = value;
    }
    public int ConnectionTimeout => this.BaseConnection.ConnectionTimeout;
    public string Database => this.BaseConnection.Database;
    public string DataSource
    {
        get
        {
            if (this.BaseConnection is DbConnection dbConnection)
                return dbConnection.DataSource;
            return null;
        }
    }
    public string ServerVersion
    {
        get
        {
            if (this.BaseConnection is DbConnection dbConnection)
                return dbConnection.DataSource;
            return null;
        }
    }
    public bool CanCreateBatch
    {
        get
        {
            if (this.BaseConnection is DbConnection dbConnection)
                return dbConnection.CanCreateBatch;
            return false;
        }
    }
    public ConnectionState State => this.BaseConnection.State;

    public event StateChangeEventHandler StateChange;

    public TheaConnection(IDbConnection connection)
    {
        this.ConnectionId = Guid.NewGuid().ToString("N");
        this.BaseConnection = connection;
    }

    public void ChangeDatabase(string databaseName) => this.BaseConnection.ChangeDatabase(databaseName);
    public async Task ChangeDatabaseAsync(string databaseName, CancellationToken cancellationToken = default)
    {
        if (this.BaseConnection is DbConnection dbConnection)
            await dbConnection.ChangeDatabaseAsync(databaseName, cancellationToken);
    }
    public void Close() => this.BaseConnection.Close();
    public async Task CloseAsync()
    {
        if (this.BaseConnection is DbConnection dbConnection)
            await dbConnection.CloseAsync();
    }
    public void Open() => this.BaseConnection.Open();
    public async Task OpenAsync()
    {
        if (this.BaseConnection is DbConnection dbConnection)
            await dbConnection.OpenAsync();
    }
    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        if (this.BaseConnection is DbConnection dbConnection)
            await dbConnection.OpenAsync(cancellationToken);
    }

    public IDbCommand CreateCommand() => this.BaseConnection.CreateCommand();
    public DbBatch CreateBatch()
    {
        if (this.BaseConnection is DbConnection dbConnection)
            return dbConnection.CreateBatch();
        return null;
    }
    public IDbTransaction BeginTransaction() => this.BaseConnection.BeginTransaction();
    public async ValueTask<DbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (this.BaseConnection is DbConnection dbConnection)
            return await dbConnection.BeginTransactionAsync(isolationLevel, cancellationToken);
        return null;
    }
    public IDbTransaction BeginTransaction(IsolationLevel il) => this.BaseConnection.BeginTransaction(il);
    public async ValueTask<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (this.BaseConnection is DbConnection dbConnection)
            return await dbConnection.BeginTransactionAsync(cancellationToken);
        return null;
    }
    public void EnlistTransaction(System.Transactions.Transaction transaction)
    {
        if (this.BaseConnection is DbConnection dbConnection)
            dbConnection.EnlistTransaction(transaction);
    }
    public DataTable GetSchema()
    {
        if (this.BaseConnection is DbConnection dbConnection)
            return dbConnection.GetSchema();
        return null;
    }
    public async Task<DataTable> GetSchemaAsync(CancellationToken cancellationToken = default)
    {
        if (this.BaseConnection is DbConnection dbConnection)
            return await dbConnection.GetSchemaAsync(cancellationToken);
        return null;
    }
    public DataTable GetSchema(string collectionName)
    {
        if (this.BaseConnection is DbConnection dbConnection)
            return dbConnection.GetSchema(collectionName);
        return null;
    }
    public async Task<DataTable> GetSchemaAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        if (this.BaseConnection is DbConnection dbConnection)
            return await dbConnection.GetSchemaAsync(collectionName, cancellationToken);
        return null;
    }
    public DataTable GetSchema(string collectionName, string[] restrictionValues)
    {
        if (this.BaseConnection is DbConnection dbConnection)
            return dbConnection.GetSchema(collectionName, restrictionValues);
        return null;
    }
    public async Task<DataTable> GetSchemaAsync(string collectionName, string[] restrictionValues, CancellationToken cancellationToken = default)
    {
        if (this.BaseConnection is DbConnection dbConnection)
            return await dbConnection.GetSchemaAsync(collectionName, restrictionValues, cancellationToken);
        return null;
    }
    public void Dispose() => this.BaseConnection.Dispose();
    public async ValueTask DisposeAsync()
    {
        if (this.BaseConnection is DbConnection dbConnection)
            await dbConnection.DisposeAsync();
    }
}
