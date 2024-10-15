using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.SqlServer;

class SqlServerTheaTransaction : ITheaTransaction
{
    private readonly SqlTransaction transaction;
    private readonly DateTime createdAt;

    public string TransactionId { get; private set; }
    public ITheaConnection Connection { get; private set; }
    public IDbTransaction BaseTransaction { get; private set; }
    public IsolationLevel IsolationLevel => this.transaction.IsolationLevel;
    public Action<TransactionEventArgs> OnCreated { get; set; }
    public Action<TransactionCompletedEventArgs> OnCompleted { get; set; }

    public SqlServerTheaTransaction(SqlTransaction transaction) : this(null, transaction) { }
    public SqlServerTheaTransaction(ITheaConnection connection, SqlTransaction transaction)
    {
        this.TransactionId = Guid.NewGuid().ToString("N");
        this.Connection = connection;
        this.transaction = transaction;
        this.BaseTransaction = transaction;
        this.createdAt = DateTime.Now;
    }

    public void Commit()
    {
        bool isSuccess = true;
        Exception exception = null;
        try { transaction.Commit(); }
        catch (Exception ex)
        {
            isSuccess = false;
            exception = ex;
        }
        var elapsed = DateTime.Now.Subtract(this.createdAt).TotalMilliseconds;
        this.OnCompleted?.Invoke(new TransactionCompletedEventArgs
        {
            DbKey = this.Connection.DbKey,
            TransactionId = this.TransactionId,
            ConnectionId = this.Connection.ConnectionId,
            ConnectionString = this.Connection.ConnectionString,
            IsSuccess = isSuccess,
            Action = TransactionAction.Commit,
            Elapsed = (int)elapsed,
            Exception = exception
        });
    }
    public void Rollback()
    {
        bool isSuccess = true;
        Exception exception = null;
        try { transaction.Rollback(); }
        catch (Exception ex)
        {
            isSuccess = false;
            exception = ex;
        }
        var elapsed = DateTime.Now.Subtract(this.createdAt).TotalMilliseconds;
        this.OnCompleted?.Invoke(new TransactionCompletedEventArgs
        {
            DbKey = this.Connection.DbKey,
            TransactionId = this.TransactionId,
            ConnectionId = this.Connection.ConnectionId,
            ConnectionString = this.Connection.ConnectionString,
            IsSuccess = isSuccess,
            Action = TransactionAction.Rollback,
            Elapsed = (int)elapsed,
            Exception = exception
        });
    }
    public void Dispose() => this.transaction.Dispose();
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public async Task CommitAsync(CancellationToken cancellationToken = default)
	{
        bool isSuccess = true;
        Exception exception = null;
        try { await this.transaction.CommitAsync(cancellationToken); }
        catch (Exception ex)
        {
            isSuccess = false;
            exception = ex;
        }
        var elapsed = DateTime.Now.Subtract(this.createdAt).TotalMilliseconds;
        this.OnCompleted?.Invoke(new TransactionCompletedEventArgs
        {
            DbKey = this.Connection.DbKey,
            TransactionId = this.TransactionId,
            ConnectionId = this.Connection.ConnectionId,
            ConnectionString = this.Connection.ConnectionString,
            IsSuccess = isSuccess,
            Action = TransactionAction.Commit,
            Elapsed = (int)elapsed,
            Exception = exception
        });
    }
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        bool isSuccess = true;
        Exception exception = null;
        try { await this.transaction.RollbackAsync(cancellationToken); }
        catch (Exception ex)
        {
            isSuccess = false;
            exception = ex;
        }
        var elapsed = DateTime.Now.Subtract(this.createdAt).TotalMilliseconds;
        this.OnCompleted?.Invoke(new TransactionCompletedEventArgs
        {
            DbKey = this.Connection.DbKey,
            TransactionId = this.TransactionId,
            ConnectionId = this.Connection.ConnectionId,
            ConnectionString = this.Connection.ConnectionString,
            IsSuccess = isSuccess,
            Action = TransactionAction.Rollback,
            Elapsed = (int)elapsed,
            Exception = exception
        });
    }
    public ValueTask DisposeAsync() => this.transaction.DisposeAsync();
#else
    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        Task result;
        bool isSuccess = true;
        Exception exception = null;
        if (cancellationToken.IsCancellationRequested)
            result = Task.FromCanceled(cancellationToken);
        else
        {
            try
            {
                this.transaction.Commit();
                result = Task.CompletedTask;
            }
            catch (Exception ex)
            {
                isSuccess = false;
                exception = ex;
                result = Task.FromException(ex);
            }
        }
        var elapsed = DateTime.Now.Subtract(this.createdAt).TotalMilliseconds;
        this.OnCompleted?.Invoke(new TransactionCompletedEventArgs
        {
            DbKey = this.Connection.DbKey,
            TransactionId = this.TransactionId,
            ConnectionId = this.Connection.ConnectionId,
            ConnectionString = this.Connection.ConnectionString,
            IsSuccess = isSuccess,
            Action = TransactionAction.Commit,
            Elapsed = (int)elapsed,
            Exception = exception
        });
        return result;
    }
    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        Task result;
        bool isSuccess = true;
        Exception exception = null;
        if (cancellationToken.IsCancellationRequested)
            result = Task.FromCanceled(cancellationToken);
        else
        {
            try
            {
                this.transaction.Rollback();
                result = Task.CompletedTask;
            }
            catch (Exception ex)
            {
                isSuccess = false;
                exception = ex;
                result = Task.FromException(ex);
            }
        }
        var elapsed = DateTime.Now.Subtract(this.createdAt).TotalMilliseconds;
        this.OnCompleted?.Invoke(new TransactionCompletedEventArgs
        {
            DbKey = this.Connection.DbKey,
            TransactionId = this.TransactionId,
            ConnectionId = this.Connection.ConnectionId,
            ConnectionString = this.Connection.ConnectionString,
            IsSuccess = isSuccess,
            Action = TransactionAction.Rollback,
            Elapsed = (int)elapsed,
            Exception = exception
        });
        return result;
    }
    public ValueTask DisposeAsync()
    {
        this.transaction.Dispose();
        return default;
    }
#endif
}
