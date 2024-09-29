﻿using MySqlConnector;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

class MySqlTheaTransaction : ITheaTransaction
{
    private readonly MySqlTransaction transaction;
    public ITheaConnection Connection { get; private set; }
    public IDbTransaction BaseTransaction { get; private set; }
    public IsolationLevel IsolationLevel => this.transaction.IsolationLevel;
#if NET5_0_OR_GREATER
    public bool SupportsSavepoints => this.transaction.SupportsSavepoints;
#endif

    public MySqlTheaTransaction(MySqlTransaction transaction) : this(null, transaction) { }
    public MySqlTheaTransaction(ITheaConnection connection, MySqlTransaction transaction)
    {
        this.Connection = connection;
        this.transaction = transaction;
        this.BaseTransaction = transaction;
    }

    public void Commit() => this.transaction.Commit();
    public Task CommitAsync(CancellationToken cancellationToken = default)
        => this.transaction.CommitAsync(cancellationToken);
    public void Rollback(string savepointName = default) => this.transaction.Rollback(savepointName);
    public Task RollbackAsync(string savepointName = default, CancellationToken cancellationToken = default)
        => this.transaction.RollbackAsync(savepointName, cancellationToken);
    public void Release(string savepointName) => this.transaction.Release(savepointName);
    public Task ReleaseAsync(string savepointName, CancellationToken cancellationToken = default)
        => this.transaction.RollbackAsync(savepointName, cancellationToken);
    public void Save(string savepointName) => this.transaction.Rollback(savepointName);
    public Task SaveAsync(string savepointName, CancellationToken cancellationToken = default)
        => this.transaction.RollbackAsync(savepointName, cancellationToken);
    public void Rollback() => this.transaction.Rollback();
    public Task RollbackAsync(CancellationToken cancellationToken = default)
        => this.transaction.RollbackAsync(cancellationToken);
    public void Dispose() => this.transaction.Dispose();
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public ValueTask DisposeAsync() => this.transaction.DisposeAsync();
#else
    public ValueTask DisposeAsync() => new ValueTask(this.transaction.DisposeAsync());
#endif
}
