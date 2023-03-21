using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public sealed class TheaConnection : IDbConnection
{
    public string DbKey { get; set; }
    public string ConnectionString { get; set; }
    public IDbConnection BaseConnection { get; set; }
    public int ConnectionTimeout => this.BaseConnection.ConnectionTimeout;
    public int CommandTimeout { get; set; } = 30;
    public string Database => this.BaseConnection.Database;
    public ConnectionState State => this.BaseConnection.State;

    public TheaConnection(string dbKey, IDbConnection baseConnection, int commandTimeout = 30)
    {
        this.DbKey = dbKey;
        this.ConnectionString = baseConnection.ConnectionString;
        this.BaseConnection = baseConnection;
        this.CommandTimeout = commandTimeout;
    }
    public TheaConnection(string dbKey, string connectionString, IDbConnection baseConnection, int commandTimeout = 30)
    {
        this.DbKey = dbKey;
        this.ConnectionString = connectionString;
        this.BaseConnection = baseConnection;
        this.CommandTimeout = commandTimeout;
    }

    public IDbTransaction BeginTransaction() => this.BaseConnection.BeginTransaction();
    public IDbTransaction BeginTransaction(IsolationLevel il) => this.BaseConnection.BeginTransaction(il);
    public async ValueTask<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (this.BaseConnection is DbConnection connection)
            return await connection.BeginTransactionAsync(cancellationToken);
        else throw new Exception("当前数据库驱动不支持异步操作");
    }
    public async ValueTask<IDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (this.BaseConnection is DbConnection connection)
            return await connection.BeginTransactionAsync(isolationLevel, cancellationToken);
        else throw new Exception("当前数据库驱动不支持异步操作");
    }
    public void ChangeDatabase(string databaseName) => this.BaseConnection.ChangeDatabase(databaseName);
    public void Close()
    {
        if (this.BaseConnection.State != ConnectionState.Closed)
            this.BaseConnection.Close();
    }
    public IDbCommand CreateCommand()
    {
        var command = this.BaseConnection.CreateCommand();
        command.CommandTimeout = this.CommandTimeout;
        return command;
    }
    public void Open()
    {
        if (this.BaseConnection.State == ConnectionState.Broken)
            this.BaseConnection.Close();
        if (this.BaseConnection.State == ConnectionState.Closed)
        {
            //关闭后，连接串被重置，需要重新设置
            this.BaseConnection.ConnectionString = this.ConnectionString;
            this.BaseConnection.Open();
        }
    }
    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        if (this.BaseConnection is DbConnection connection)
        {
            if (this.BaseConnection.State == ConnectionState.Broken)
                await connection.CloseAsync();
            if (this.BaseConnection.State == ConnectionState.Closed)
            {
                //关闭后，连接串被重置，需要重新设置
                connection.ConnectionString = this.ConnectionString;
                await connection.OpenAsync(cancellationToken);
            }
        }
        else throw new Exception("当前数据库驱动不支持异步操作");
    }
    public async Task CloseAsync()
    {
        if (this.BaseConnection is DbConnection connection && connection.State != ConnectionState.Closed)
            await connection.CloseAsync();
        else throw new Exception("当前数据库驱动不支持异步操作");
    }
    public void Dispose() => this.BaseConnection.Dispose();
    public async Task DisposeAsync()
    {
        if (this.BaseConnection is DbConnection connection)
            await connection.DisposeAsync();
        else throw new Exception("当前数据库驱动不支持异步操作");
    }
    public override int GetHashCode() => HashCode.Combine(this.DbKey, this.ConnectionString);
}
