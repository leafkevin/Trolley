using System;

namespace Trolley;

public class DefautDbInterceptor : IDbInterceptor
{
    public virtual bool ConnectionCreating() => true;
    public virtual ITheaConnection ConnectionCreated(ITheaConnection connection) => connection;
    public virtual bool ConnectionClosing(ITheaConnection connection) => true;
    public virtual void ConnectionClosed(ITheaConnection connection) { }
    public virtual void ConnectionDisposed(ITheaConnection connection) { }
    public virtual bool ConnectionDisposing(ITheaConnection connection) => true;

    public virtual bool CommandCreating(ITheaConnection connection) => true;
    public virtual ITheaCommand CommandCreated(ITheaCommand command) => command;
    public virtual ITheaCommand CommandInitialized(ITheaCommand command) => command;
    public virtual void CommandCanceled(ITheaCommand command) { }
    public virtual DbEventArgs CommandExecuting(ITheaCommand command)
        => new DbEventArgs { IsSuccess = true, EventData = DateTime.UtcNow };
    public virtual void CommandExecuted(ITheaCommand command, DbCommandCompletedEventArgs eventArgs) { }
    public virtual void CommandFailed(ITheaCommand command, DbCommandFailedEventArgs eventArgs) { }

    public virtual bool TransactionCreating(ITheaConnection connection) => true;
    public virtual ITheaTransaction TransactionCreated(ITheaTransaction transaction) => transaction;
    public virtual bool ConnectionOpening(ITheaConnection connection) => true;
    public virtual void ConnectionOpened(ITheaConnection connection) { }
    public virtual bool TransactionCommitting(ITheaTransaction transaction) => true;
    public virtual void TransactionCommitted(ITheaTransaction transaction) { }
    public virtual bool TransactionRollingBack(ITheaTransaction transaction) => true;
    public virtual void TransactionRolledBack(ITheaTransaction transaction) { }
    public virtual void TransactionFailed(ITheaTransaction transaction) { }
}