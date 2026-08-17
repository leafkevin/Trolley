using System;

namespace Trolley;

public class DefautDbInterceptor : IDbInterceptor
{
    public virtual void ConnectionCreating() { }
    public virtual ITheaConnection ConnectionCreated(ITheaConnection connection) => connection;
    public virtual void ConnectionOpening(ITheaConnection connection) { }
    public virtual void ConnectionOpened(ITheaConnection connection) { }
    public virtual void ConnectionClosing(ITheaConnection connection) { }
    public virtual void ConnectionClosed(ITheaConnection connection) { }
    public virtual void ConnectionDisposing(ITheaConnection connection) { }
    public virtual void ConnectionDisposed(ITheaConnection connection) { }

    public virtual void CommandCreating(ITheaConnection connection) { }
    public virtual ITheaCommand CommandCreated(ITheaCommand command) => command;
    public virtual ITheaCommand CommandInitialized(ITheaCommand command) => command;
    public virtual void CommandCanceled(ITheaCommand command) { }
    public virtual DbCommandExecutingEventArgs CommandExecuting(ITheaCommand command)
        => new DbCommandExecutingEventArgs { IsCanExecuting = true, Command = command, EventData = DateTime.UtcNow };
    public virtual void CommandExecuted(DbCommandCompletedEventArgs eventArgs) { }
    public virtual void CommandFailed(DbCommandCompletedEventArgs eventArgs) { }
    public virtual void CommandDisposing(ITheaCommand command) { }
    public virtual void CommandDisposed(ITheaCommand command) { }

    public void DataReaderCreating(ITheaCommand command) { }
    public ITheaDataReader DataReaderCreated(ITheaDataReader reader) => reader;
    public void DataReaderClosing(ITheaDataReader reader) { }
    public void DataReaderClosed(ITheaDataReader reader) { }
    public void DataReaderDisposing(ITheaDataReader reader) { }
    public void DataReaderDisposed(ITheaDataReader reader) { }

    public virtual void TransactionCreating(ITheaConnection connection) { }
    public virtual ITheaTransaction TransactionCreated(ITheaTransaction transaction) => transaction;
    public virtual DbTransactionExecutingEventArgs TransactionCommitting(ITheaTransaction transaction)
        => new DbTransactionExecutingEventArgs { EventData = DateTime.UtcNow, Transaction = transaction };
    public virtual void TransactionCommitted(DbTransactionCompletedEventArgs eventArgs) { }
    public virtual DbTransactionExecutingEventArgs TransactionRollingBack(ITheaTransaction transaction)
        => new DbTransactionExecutingEventArgs { EventData = DateTime.UtcNow, Transaction = transaction };
    public virtual void TransactionRolledBack(DbTransactionCompletedEventArgs eventArgs) { }
    public virtual void TransactionFailed(DbTransactionCompletedEventArgs eventArgs) { }
    public virtual void TransactionDisposing(ITheaTransaction transaction) { }
    public virtual void TransactionDisposed(ITheaTransaction transaction) { }
}