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

    public virtual void TransactionCreating(ITheaConnection connection) { }
    public virtual ITheaTransaction TransactionCreated(ITheaTransaction transaction) => transaction;
    public virtual DbTransactionExecutingEventArgs TransactionCommitting(ITheaTransaction transaction)
    => new DbTransactionExecutingEventArgs { EventData = DateTime.UtcNow, Transaction = transaction };
    public virtual void TransactionCommitted(DbTransactionCompletedEventArgs eventArgs) { }
    public virtual DbTransactionExecutingEventArgs TransactionRollingBack(ITheaTransaction transaction)
    => new DbTransactionExecutingEventArgs { EventData = DateTime.UtcNow, Transaction = transaction };
    public virtual void TransactionRolledBack(DbTransactionCompletedEventArgs eventArgs) { }
    public virtual void TransactionFailed(DbTransactionCompletedEventArgs eventArgs) { }
}