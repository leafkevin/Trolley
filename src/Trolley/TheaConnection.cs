using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public sealed class TheaConnection : IDbConnection
{
    private int isDisposed = 0;

    public string DbKey { get; set; }
    public string ConnectionString { get; set; }
    public IDbConnection BaseConnection { get; set; }
    public int ConnectionTimeout => this.BaseConnection.ConnectionTimeout;
    public int CommandTimeout { get; set; } = 30;
    public string Database => this.BaseConnection.Database;
    public ConnectionState State => this.BaseConnection.State;
    internal IOrmProvider OrmProvider { get; set; }

    public IDbTransaction BeginTransaction()
    {
        if (Interlocked.CompareExchange(ref this.isDisposed, 0, 1) == 1)
            this.BaseConnection = this.OrmProvider.CreateConnection(this.ConnectionString);
        return this.BaseConnection.BeginTransaction();
    }
    public IDbTransaction BeginTransaction(IsolationLevel il)
    {
        if (Interlocked.CompareExchange(ref this.isDisposed, 0, 1) == 1)
            this.BaseConnection = this.OrmProvider.CreateConnection(this.ConnectionString);
        return this.BaseConnection.BeginTransaction(il);
    }
    public async ValueTask<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref this.isDisposed, 0, 1) == 1)
            this.BaseConnection = this.OrmProvider.CreateConnection(this.ConnectionString);
        if (this.BaseConnection is DbConnection connection)
            return await connection.BeginTransactionAsync(cancellationToken);
        else throw new Exception("当前数据库驱动不支持异步操作");
    }
    public async ValueTask<IDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref this.isDisposed, 0, 1) == 1)
            this.BaseConnection = this.OrmProvider.CreateConnection(this.ConnectionString);
        if (this.BaseConnection is DbConnection connection)
            return await connection.BeginTransactionAsync(isolationLevel, cancellationToken);
        else throw new Exception("当前数据库驱动不支持异步操作");
    }
    public void ChangeDatabase(string databaseName) => this.BaseConnection.ChangeDatabase(databaseName);

    public IDbCommand CreateCommand()
    {
        if (Interlocked.CompareExchange(ref this.isDisposed, 0, 1) == 1)
            this.BaseConnection = this.OrmProvider.CreateConnection(this.ConnectionString);

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
        if (this.BaseConnection is not DbConnection connection)
            throw new Exception("当前数据库驱动不支持异步操作");
        if (connection.State == ConnectionState.Broken)
            await connection.CloseAsync();
        if (connection.State == ConnectionState.Closed)
        {
            //关闭后，连接串被重置，需要重新设置
            connection.ConnectionString = this.ConnectionString;
            await connection.OpenAsync(cancellationToken);
        }
    }
    public void Close()
    {
        if (Interlocked.CompareExchange(ref this.isDisposed, 1, 0) != 0)
            return;
        this.BaseConnection.Close();
    }
    public async Task CloseAsync()
    {
        if (Interlocked.CompareExchange(ref this.isDisposed, 1, 0) != 0)
            return;
        if (this.BaseConnection is not DbConnection connection)
            throw new Exception("当前数据库驱动不支持异步操作");
        await connection.CloseAsync();
    }
    public void Dispose()
    {
        this.Close();
        GC.SuppressFinalize(this);
    }
    public async Task DisposeAsync()
    {
        await this.CloseAsync();
        GC.SuppressFinalize(this);
    }
    public override int GetHashCode() => HashCode.Combine(this.DbKey, this.ConnectionString);
}
