using System;

namespace Trolley;

public struct DbCommandExecutingEventArgs
{
    public bool IsCanExecuting { get; set; }
    public object EventData { get; set; }
    public ITheaCommand Command { get; set; }
}
public struct DbCommandCompletedEventArgs
{
    public ITheaCommand Command { get; set; }
    public bool IsSuccess { get; set; }
    public object EventData { get; set; }
    public Exception Exception { get; set; }
}
public struct DbTransactionExecutingEventArgs
{
    public ITheaTransaction Transaction { get; set; }
    public object EventData { get; set; }
}
public struct DbTransactionCompletedEventArgs
{
    public ITheaTransaction Transaction { get; set; }
    public bool IsSuccess { get; set; }
    public object EventData { get; set; }
    public Exception Exception { get; set; }
}
public interface IDbInterceptor
{
    void ConnectionCreating();
    ITheaConnection ConnectionCreated(ITheaConnection connection);
    void ConnectionOpening(ITheaConnection connection);
    void ConnectionOpened(ITheaConnection connection);
    void ConnectionClosing(ITheaConnection connection);
    void ConnectionClosed(ITheaConnection connection);
    void ConnectionDisposing(ITheaConnection connection);
    void ConnectionDisposed(ITheaConnection connection);

    void CommandCreating(ITheaConnection connection);
    ITheaCommand CommandCreated(ITheaCommand command);
    ITheaCommand CommandInitialized(ITheaCommand command);
    void CommandCanceled(ITheaCommand command);
    DbCommandExecutingEventArgs CommandExecuting(ITheaCommand command);
    void CommandExecuted(DbCommandCompletedEventArgs eventArgs);
    void CommandFailed(DbCommandCompletedEventArgs eventArgs);
    void CommandDisposing(ITheaCommand command);
    void CommandDisposed(ITheaCommand command);

    void DataReaderCreating(ITheaCommand command);
    ITheaDataReader DataReaderCreated(ITheaDataReader reader);
    void DataReaderClosing(ITheaDataReader reader);
    void DataReaderClosed(ITheaDataReader reader);
    void DataReaderDisposing(ITheaDataReader reader);
    void DataReaderDisposed(ITheaDataReader reader);

    void TransactionCreating(ITheaConnection connection);
    ITheaTransaction TransactionCreated(ITheaTransaction transaction);
    DbTransactionExecutingEventArgs TransactionCommitting(ITheaTransaction transaction);
    void TransactionCommitted(DbTransactionCompletedEventArgs eventArgs);
    DbTransactionExecutingEventArgs TransactionRollingBack(ITheaTransaction transaction);
    void TransactionRolledBack(DbTransactionCompletedEventArgs eventArgs);
    void TransactionFailed(DbTransactionCompletedEventArgs eventArgs);
}