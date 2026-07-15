using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;



public interface IDbInterceptor
{
}
public interface IDbCommandInterceptor
{
    ITheaCommand CommandCreating(ITheaCommand command);
    void CommandCreated(ITheaCommand command);
    ITheaCommand CommandInitialized(ITheaCommand command);
    void CommandCanceled(ITheaCommand command);
    Task CommandCanceledAsync(ITheaCommand command, CancellationToken cancellationToken = default);
    void CommandFailed(ITheaCommand command);
    ValueTask CommandFailedAsync(ITheaCommand command, CancellationToken cancellationToken = default);
    void DataReaderClosing(ITheaCommand command);
    ValueTask DataReaderClosingAsync(ITheaCommand command, CancellationToken cancellationToken = default);
    void NonQueryExecuting(ITheaCommand command);
    ValueTask NonQueryExecutingAsync(ITheaCommand command, CancellationToken cancellationToken = default);
    int NonQueryExecuted(ITheaCommand command, int result);
    ValueTask<int> NonQueryExecutedAsync(ITheaCommand command, int result, CancellationToken cancellationToken = default);
    //int NonQueryExecuted(ITheaCommand command, int result);
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