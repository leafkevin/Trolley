using System;
using System.Data;

namespace Trolley;

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
public enum CommandSqlType
{
    Select,
    RawExecute,
    Insert,
    BulkInsert,
    BulkCopyInsert,
    Update,
    BulkUpdate,
    BulkCopyUpdate,
    Delete,
    MultiQuery,
    MultiCommand
}
public class ConectionEventArgs : EventArgs
{
    public string ConnectionId { get; set; }
    public string DbKey { get; set; }
    public string ConnectionString { get; set; }
    public DateTime CreatedAt { get; set; }
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
    public DateTime CreatedAt { get; set; }
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
    public DateTime CreatedAt { get; set; }
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