﻿using Npgsql;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.PostgreSql;

class PostgreSqlTheaTransaction : ITheaTransaction
{
    private readonly NpgsqlTransaction transaction;
    private readonly DateTime createdAt;

    public string TransactionId { get; private set; }
    public ITheaConnection Connection { get; private set; }
    public IDbTransaction BaseTransaction { get; private set; }
    public Action<TransactionEventArgs> OnCreated { get; set; }
    public Action<TransactionCompletedEventArgs> OnCompleted { get; set; }

    public PostgreSqlTheaTransaction(NpgsqlTransaction transaction) : this(null, transaction) { }
    public PostgreSqlTheaTransaction(ITheaConnection connection, NpgsqlTransaction transaction)
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
            CreatedAt = this.createdAt,
            IsSuccess = isSuccess,
            Action = TransactionAction.Commit,
            Elapsed = (int)elapsed,
            Exception = exception
        });
    }
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
            CreatedAt = this.createdAt,
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
        try { this.transaction.Rollback(); }
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
            CreatedAt = this.createdAt,
            IsSuccess = isSuccess,
            Action = TransactionAction.Rollback,
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
            CreatedAt = this.createdAt,
            IsSuccess = isSuccess,
            Action = TransactionAction.Rollback,
            Elapsed = (int)elapsed,
            Exception = exception
        });
    }
    public void Dispose() => this.transaction.Dispose();
    public ValueTask DisposeAsync() => this.transaction.DisposeAsync();
}