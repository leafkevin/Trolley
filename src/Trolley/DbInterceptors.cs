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
    public Action<CommandCompletedEventArgs> OnCommandFailed { get; set; }
}
public enum CommandSqlType
{
    Select,
    Execute,
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
    public IOrmProvider OrmProvider { get; set; }
    public DateTime CreatedAt { get; set; }
}
public class CommandEventArgs : EventArgs
{
    public string CommandId { get; set; }
    public string DbKey { get; set; }
    public string ConnectionString { get; set; }
    public CommandSqlType SqlType { get; set; }
    public int BulkIndex { get; set; }
    public string Sql { get; set; }
    public IDataParameterCollection DbParameters { get; set; }
    public IOrmProvider OrmProvider { get; set; }
    public DateTime CreatedAt { get; set; }
}
public class CommandCompletedEventArgs : CommandEventArgs
{
    public bool IsSuccess { get; set; }
    public int Elapsed { get; set; }
    public Exception Exception { get; set; }
    public CommandCompletedEventArgs() { }
    public CommandCompletedEventArgs(CommandEventArgs eventArgs)
    {
        this.DbKey = eventArgs.DbKey;
        this.ConnectionString = eventArgs.ConnectionString;
        this.SqlType = eventArgs.SqlType;
        this.Sql = eventArgs.Sql;
        this.DbParameters = eventArgs.DbParameters;
        this.OrmProvider = eventArgs.OrmProvider;
        this.CreatedAt = eventArgs.CreatedAt;
        this.Elapsed = (int)DateTime.Now.Subtract(this.CreatedAt).TotalMilliseconds;
    }
}