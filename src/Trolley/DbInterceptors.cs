using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

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
public interface IDbInterceptor
{
}
public interface IDbConnectionInterceptor
{
    public Action<TransactionEventArgs> OnTransactionCreated { get; set; }
    public Action<TransactionCompletedEventArgs> OnTransactionCompleted { get; set; }

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

    bool TransactionCreating(ITheaConnection connection);
    ITheaTransaction TransactionCreated(ITheaTransaction transaction);

    bool TransactionCommitting(ITheaTransaction transaction);
    void TransactionCommitted(ITheaTransaction transaction);


    bool ReaderExecuting(ITheaDataReader reader);
    ValueTask<bool> ReaderExecutingAsync(ITheaDataReader reader, CancellationToken cancellationToken = default);
    void ReaderExecuted(ITheaDataReader reader);
    ValueTask ReaderExecutedAsync(ITheaDataReader reader, CancellationToken cancellationToken = default);

    bool DataReaderClosing(ITheaCommand command);
    ValueTask<bool> DataReaderClosingAsync(ITheaCommand command, CancellationToken cancellationToken = default);
    bool DataReaderDisposing(ITheaCommand command);

    bool NonQueryExecuting(ITheaCommand command);
    ValueTask<bool> NonQueryExecutingAsync(ITheaCommand command, CancellationToken cancellationToken = default);
    int NonQueryExecuted(ITheaCommand command, int affectedRows);
    ValueTask<int> NonQueryExecutedAsync(ITheaCommand command, int affectedRows, CancellationToken cancellationToken = default);

    bool ScalarExecuting(ITheaCommand command);
    ValueTask<bool> ExecuteScalarAsync(ITheaCommand command, CancellationToken cancellationToken = default);
    object ScalarExecuted(ITheaCommand command, object result);
    ValueTask<object> ScalarExecutedAsync(ITheaCommand command, object result, CancellationToken cancellationToken = default);

    void CommandFailed(ITheaCommand command);
    Task CommandFailedAsync(ITheaCommand command, CancellationToken cancellationToken = default);
}
public interface IDbCommandInterceptor
{
    bool CommandCreating(ITheaConnection connection);
    ITheaCommand CommandCreated(ITheaCommand command);
    ITheaCommand CommandInitialized(ITheaCommand command);
    void CommandCanceled(ITheaCommand command);
    Task CommandCanceledAsync(ITheaCommand command, CancellationToken cancellationToken = default);

    bool ReaderExecuting(ITheaDataReader reader);
    ValueTask<bool> ReaderExecutingAsync(ITheaDataReader reader, CancellationToken cancellationToken = default);
    void ReaderExecuted(ITheaDataReader reader);
    ValueTask ReaderExecutedAsync(ITheaDataReader reader, CancellationToken cancellationToken = default);

    bool DataReaderClosing(ITheaCommand command);
    ValueTask<bool> DataReaderClosingAsync(ITheaCommand command, CancellationToken cancellationToken = default);
    bool DataReaderDisposing(ITheaCommand command);

    bool NonQueryExecuting(ITheaCommand command);
    ValueTask<bool> NonQueryExecutingAsync(ITheaCommand command, CancellationToken cancellationToken = default);
    int NonQueryExecuted(ITheaCommand command, int affectedRows);
    ValueTask<int> NonQueryExecutedAsync(ITheaCommand command, int affectedRows, CancellationToken cancellationToken = default);

    bool ScalarExecuting(ITheaCommand command);
    ValueTask<bool> ExecuteScalarAsync(ITheaCommand command, CancellationToken cancellationToken = default);
    object ScalarExecuted(ITheaCommand command, object result);
    ValueTask<object> ScalarExecutedAsync(ITheaCommand command, object result, CancellationToken cancellationToken = default);

    void CommandFailed(ITheaCommand command);
    Task CommandFailedAsync(ITheaCommand command, CancellationToken cancellationToken = default);
}

public interface IDbTransactionInterceptor
{
    bool TransactionCommitting(ITheaTransaction transaction);
    ValueTask<bool> TransactionCommittingAsync(ITheaTransaction transaction, CancellationToken cancellationToken = default);
    void TransactionCommitted(ITheaTransaction transaction);
    Task TransactionCommittedAsync(ITheaTransaction transaction, CancellationToken cancellationToken = default);

    void TransactionFailed(
}


public class DbInterceptors
{
    public Action<ConectionEventArgs> OnConnectionCreated { get; set; }
    public Action<ConectionEventArgs> OnConnectionOpening { get; set; }
    public Action<ConectionEventArgs> OnConnectionOpened { get; set; }
    public Action<ConectionEventArgs> OnConnectionClosing { get; set; }
    public Action<ConectionEventArgs> OnConnectionClosed { get; set; }
    public Action<CommandEventArgs> OnCommandExecuting { get; set; }
    public Action<CommandCompletedEventArgs> OnCommandExecuted { get; set; }
    public Action<TransactionEventArgs> OnTransactionCreated { get; set; }
    public Action<TransactionCompletedEventArgs> OnTransactionCompleted { get; set; }
}
public class ConectionEventArgs : EventArgs
{
    public string ConnectionId { get; set; }
    public string DbKey { get; set; }
    public string ConnectionString { get; set; }
}
public class CommandEventArgs : EventArgs
{
    public string CommandId { get; set; }
    public string ConnectionId { get; set; }
    public string TransactionId { get; set; }
    public string DbKey { get; set; }
    public string ConnectionString { get; set; }
    public CommandSqlType SqlType { get; set; }
    public int Index { get; set; }
    public string Sql { get; set; }
    public IDataParameterCollection DbParameters { get; set; }
}
public class CommandCompletedEventArgs : CommandEventArgs
{
    public bool IsSuccess { get; set; }
    public int Elapsed { get; set; }
    public Exception Exception { get; set; }
}
public class TransactionEventArgs : EventArgs
{
    public string TransactionId { get; set; }
    public string ConnectionId { get; set; }
    public string DbKey { get; set; }
    public string ConnectionString { get; set; }
}
public enum TransactionAction
{
    Commit,
    Rollback,
    Save,
    Release,
}
public class TransactionCompletedEventArgs : TransactionEventArgs
{
    public bool IsSuccess { get; set; }
    public int Elapsed { get; set; }
    public TransactionAction Action { get; set; }
    public Exception Exception { get; set; }
}