using MySqlConnector;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

class MySqlTheaTransaction : ITheaTransaction
{
    private readonly MySqlTheaConnection connection;
    private readonly MySqlTransaction transaction;

    public string DbKey { get; private set; }
    public string TransactionId { get; private set; }
    public ITheaConnection Connection => this.connection;
    public IDbTransaction DbTransaction => this.transaction;
    public IsolationLevel IsolationLevel => this.transaction.IsolationLevel;
    public IDbInterceptor Interceptor { get; set; }
    IDbConnection IDbTransaction.Connection => this.connection.DbConnection;

    public MySqlTheaTransaction(string dbKey, MySqlTheaConnection connection, MySqlTransaction transaction)
    {
        this.DbKey = dbKey;
        this.TransactionId = Guid.NewGuid().ToString("N");
        this.connection = connection;
        this.transaction = transaction;
    }

    public void Commit()
    {
        bool isSuccess = true;
        Exception exception = null;
        var eventArgs = this.Interceptor.TransactionCommitting(this);
        try { this.transaction.Commit(); }
        catch (Exception ex)
        {
            isSuccess = false;
            exception = ex;
        }
        finally
        {
            var completedEventArgs = new DbTransactionCompletedEventArgs
            {
                IsSuccess = isSuccess,
                EventData = eventArgs.EventData,
                Exception = exception,
                Transaction = this
            };
            this.Interceptor.TransactionCommitted(completedEventArgs);
            if (!isSuccess) this.Interceptor.TransactionFailed(completedEventArgs);
        }
        if (!isSuccess)
        {
            if (!isSuccess) this.connection.Close();
            throw exception;
        }
    }
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        bool isSuccess = true;
        Exception exception = null;
        var eventArgs = this.Interceptor.TransactionCommitting(this);
        try { await this.transaction.CommitAsync(cancellationToken); }
        catch (Exception ex)
        {
            isSuccess = false;
            exception = ex;
        }
        finally
        {
            var completedEventArgs = new DbTransactionCompletedEventArgs
            {
                IsSuccess = isSuccess,
                EventData = eventArgs.EventData,
                Exception = exception,
                Transaction = this
            };
            this.Interceptor.TransactionCommitted(completedEventArgs);
            if (!isSuccess) this.Interceptor.TransactionFailed(completedEventArgs);
        }
        if (!isSuccess)
        {
            if (!isSuccess) this.connection.Close();
            throw exception;
        }
    }
    public void Rollback()
    {
        bool isSuccess = true;
        Exception exception = null;
        var eventArgs = this.Interceptor.TransactionRollingBack(this);
        try { this.transaction.Rollback(); }
        catch (Exception ex)
        {
            isSuccess = false;
            exception = ex;
        }
        finally
        {
            var completedEventArgs = new DbTransactionCompletedEventArgs
            {
                IsSuccess = isSuccess,
                EventData = eventArgs.EventData,
                Exception = exception,
                Transaction = this
            };
            this.Interceptor.TransactionRolledBack(completedEventArgs);
            if (!isSuccess) this.Interceptor.TransactionFailed(completedEventArgs);
        }
        if (!isSuccess)
        {
            if (!isSuccess) this.connection.Close();
            throw exception;
        }
    }
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        bool isSuccess = true;
        Exception exception = null;
        var eventArgs = this.Interceptor.TransactionRollingBack(this);
        try { await this.transaction.RollbackAsync(cancellationToken); }
        catch (Exception ex)
        {
            isSuccess = false;
            exception = ex;
        }
        finally
        {
            var completedEventArgs = new DbTransactionCompletedEventArgs
            {
                IsSuccess = isSuccess,
                EventData = eventArgs.EventData,
                Exception = exception,
                Transaction = this
            };
            this.Interceptor.TransactionRolledBack(completedEventArgs);
            if (!isSuccess) this.Interceptor.TransactionFailed(completedEventArgs);
        }
        if (!isSuccess)
        {
            if (!isSuccess) this.connection.Close();
            throw exception;
        }
    }
    public void Dispose() => this.transaction.Dispose();
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public ValueTask DisposeAsync()
    {
        this.transaction.DisposeAsync();
        return default;
    }
#else
    public ValueTask DisposeAsync()
    {
        this.transaction.Dispose();
        return default;
    }
#endif
}