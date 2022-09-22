using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public sealed class TheaConnection : IDbConnection
{
    private IDbConnection baseConnection;
    public string DbKey { get; set; }
    public string ConnectionString { get; set; }
    public IOrmProvider OrmProvider { get; private set; }
    public int ConnectionTimeout => this.baseConnection.ConnectionTimeout;
    public string Database => this.baseConnection.Database;
    public ConnectionState State => this.baseConnection.State;
    public TheaConnection() { }
    public TheaConnection(TheaConnectionInfo connectionInfo)
    {
        this.DbKey = connectionInfo.DbKey;
        this.ConnectionString = connectionInfo.ConnectionString;
        this.OrmProvider = connectionInfo.OrmProvider;
        this.baseConnection = this.OrmProvider.CreateConnection(connectionInfo.ConnectionString);
    }
    public TheaConnection(string dbKey, string connectionString, IOrmProvider ormProvider)
    {
        this.DbKey = dbKey;
        this.ConnectionString = connectionString;
        this.OrmProvider = ormProvider;
        this.baseConnection = this.OrmProvider.CreateConnection(connectionString);
    }
    public IDbTransaction BeginTransaction() => this.baseConnection.BeginTransaction();
    public IDbTransaction BeginTransaction(IsolationLevel il) => this.baseConnection.BeginTransaction(il);
    public async ValueTask<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (this.baseConnection is DbConnection connection)
            return await connection.BeginTransactionAsync(cancellationToken);
        else throw new Exception("当前数据库驱动不支持异步操作");
    }
    public async ValueTask<IDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (this.baseConnection is DbConnection connection)
            return await connection.BeginTransactionAsync(isolationLevel, cancellationToken);
        else throw new Exception("当前数据库驱动不支持异步操作");
    }
    public void ChangeDatabase(string databaseName) => this.baseConnection.ChangeDatabase(databaseName);
    public void Close() => this.baseConnection.Close();
    public IDbCommand CreateCommand() => this.baseConnection.CreateCommand();
    public void Open()
    {
        if (this.baseConnection.State == ConnectionState.Broken)
            this.baseConnection.Close();
        if (this.baseConnection.State == ConnectionState.Closed)
            this.baseConnection.Open();
    }
    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        if (this.baseConnection is DbConnection connection)
        {
            if (this.baseConnection.State == ConnectionState.Broken)
                await connection.CloseAsync();
            if (this.baseConnection.State == ConnectionState.Closed)
                await connection.OpenAsync(cancellationToken);
        }
        else throw new Exception("当前数据库驱动不支持异步操作");
    }
    public async Task CloseAsync()
    {
        if (this.baseConnection is DbConnection connection)
            await connection.CloseAsync();
        else throw new Exception("当前数据库驱动不支持异步操作");
    }
    public void Dispose() => this.baseConnection.Dispose();
    public async Task DisposeAsync()
    {
        if (this.baseConnection is DbConnection connection)
            await connection.DisposeAsync();
        else throw new Exception("当前数据库驱动不支持异步操作");
    }
    public override int GetHashCode() => HashCode.Combine(this.ConnectionString);
}
