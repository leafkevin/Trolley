using System;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.SqlConnector;

class SqlServerTheaTransaction : ITheaTransaction
{
    private readonly SqlTransaction transaction;
    public ITheaConnection Connection { get; private set; }
    public IDbTransaction BaseTransaction { get; private set; }
    public IsolationLevel IsolationLevel => this.transaction.IsolationLevel;
#if NET5_0_OR_GREATER
    public bool SupportsSavepoints => this.transaction.SupportsSavepoints;
#endif

    public SqlServerTheaTransaction(SqlTransaction transaction) : this(null, transaction) { }
    public SqlServerTheaTransaction(ITheaConnection connection, SqlTransaction transaction)
    {
        this.Connection = connection;
        this.transaction = transaction;
        this.BaseTransaction = transaction;
    }

    public void Commit() => this.transaction.Commit();
    public Task CommitAsync(CancellationToken cancellationToken = default)
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        => this.transaction.CommitAsync(cancellationToken);
#else
    {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);
            try
            {
                Commit();
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }
#endif
    public void Rollback(string savepointName = default) => this.transaction.Rollback(savepointName);
    public Task RollbackAsync(string savepointName = default, CancellationToken cancellationToken = default)
#if NETCOREAPP5_0_OR_GREATER
        => this.transaction.RollbackAsync(savepointName, cancellationToken);
#else
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);
        try
        {
            this.transaction.Rollback(savepointName);
            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            return Task.FromException(e);
        }
    }
#endif
    public void Release(string savepointName)
#if NETCOREAPP5_0_OR_GREATER
        => this.transaction.Release(savepointName);
#else
    { }
#endif
    public Task ReleaseAsync(string savepointName, CancellationToken cancellationToken = default)
#if NETCOREAPP5_0_OR_GREATER
        => this.transaction.ReleaseAsync(savepointName, cancellationToken);
#else
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);
        try
        {
            this.Release(savepointName);
            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            return Task.FromException(e);
        }
    }
#endif
    public void Save(string savepointName) => this.transaction.Save(savepointName);
    public Task SaveAsync(string savepointName, CancellationToken cancellationToken = default)
#if NETCOREAPP5_0_OR_GREATER
        => this.transaction.SaveAsync(savepointName, cancellationToken);
#else
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);
        try
        {
            this.Save(savepointName);
            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            return Task.FromException(e);
        }
    }
#endif
    public void Rollback() => this.transaction.Rollback();
    public Task RollbackAsync(CancellationToken cancellationToken = default)
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        => this.transaction.RollbackAsync(cancellationToken);
#else
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);
        try
        {
            this.Rollback();
            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            return Task.FromException(e);
        }
    }
#endif
    public void Dispose() => this.transaction.Dispose();
    public ValueTask DisposeAsync()
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        => this.transaction.DisposeAsync();
#else
    {
        this.transaction.Dispose();
        return default;
    }
#endif
}
