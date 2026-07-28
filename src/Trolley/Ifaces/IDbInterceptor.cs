using System;

namespace Trolley;

public struct DbEventArgs
{
    public bool IsSuccess { get; set; }
    public object EventData { get; set; }
}
public struct DbCommandCompletedEventArgs
{
    public CommandSqlType SqlType { get; set; }
    public bool IsSuccess { get; internal set; }
    public object EventData { get; internal set; }
    public int Elapsed { get; internal set; }
    public Exception Exception { get; internal set; }
}
public struct DbCommandFailedEventArgs
{
    public CommandSqlType SqlType { get; set; }
    public object EventData { get; internal set; }
    public int Elapsed { get; internal set; }
    public Exception Exception { get; internal set; }
}
public interface IDbInterceptor
{
    bool ConnectionCreating();
    ITheaConnection ConnectionCreated(ITheaConnection connection);
    bool ConnectionOpening(ITheaConnection connection);
    void ConnectionOpened(ITheaConnection connection);
    bool ConnectionClosing(ITheaConnection connection);
    void ConnectionClosed(ITheaConnection connection);
    bool ConnectionDisposing(ITheaConnection connection);
    void ConnectionDisposed(ITheaConnection connection);

    bool CommandCreating(ITheaConnection connection);
    ITheaCommand CommandCreated(ITheaCommand command);
    ITheaCommand CommandInitialized(ITheaCommand command);
    void CommandCanceled(ITheaCommand command);
    DbEventArgs CommandExecuting(ITheaCommand command);
    void CommandExecuted(ITheaCommand command, DbCommandCompletedEventArgs eventArgs);
    void CommandFailed(ITheaCommand command, DbCommandFailedEventArgs eventArgs);

    bool TransactionCreating(ITheaConnection connection);
    ITheaTransaction TransactionCreated(ITheaTransaction transaction);
    bool TransactionCommitting(ITheaTransaction transaction);
    void TransactionCommitted(ITheaTransaction transaction);
    bool TransactionRollingBack(ITheaTransaction transaction);
    void TransactionRolledBack(ITheaTransaction transaction);
    void TransactionFailed(ITheaTransaction transaction);
}