using System;
using System.Threading;
using Xunit.Abstractions;

namespace Trolley.Test;

public class MyDbInterceptor : DefautDbInterceptor
{
    private readonly ITestOutputHelper output;
    private static int connTotal = 0;
    private static int connOpenTotal = 0;
    private static int tranTotal = 0;

    public MyDbInterceptor(ITestOutputHelper output)
    {
        this.output = output;
    }

    public override ITheaConnection ConnectionCreated(ITheaConnection connection)
    {
        Interlocked.Increment(ref connTotal);
        this.output.WriteLine($"Connection {connection.ConnectionId} Created, Total:{Volatile.Read(ref connTotal)}");
        return connection;
    }
    public override void ConnectionOpened(ITheaConnection connection)
    {
        Interlocked.Increment(ref connOpenTotal);
        this.output.WriteLine($"Connection {connection.ConnectionId} Opened, Total:{Volatile.Read(ref connOpenTotal)}");
    }
    public override void ConnectionClosed(ITheaConnection connection)
    {
        Interlocked.Decrement(ref connOpenTotal);
        Interlocked.Decrement(ref connTotal);
        this.output.WriteLine($"Connection {connection.ConnectionId} Closed, Total:{Volatile.Read(ref connOpenTotal)}");
    }
    public override DbCommandExecutingEventArgs CommandExecuting(ITheaCommand command)
    {
        this.output.WriteLine($"Begin, CommandId:{command.CommandId} Sql: {command.CommandText}, Parameters: {command.Parameters.ToMySqlParametersString()}");
        return new DbCommandExecutingEventArgs { IsCanExecuting = true, Command = command, EventData = DateTime.UtcNow };
    }
    public override void CommandExecuted(DbCommandCompletedEventArgs eventArgs)
    {
        var command = eventArgs.Command;
        var startTime = (DateTime)eventArgs.EventData;
        var elapsed = DateTime.UtcNow.Subtract(startTime).TotalMilliseconds;
        this.output.WriteLine($"End, CommandId:{command.CommandId} Elapsed: {elapsed} ms");
    }
    public override ITheaTransaction TransactionCreated(ITheaTransaction transaction)
    {
        Interlocked.Increment(ref tranTotal);
        this.output.WriteLine($"Transaction {transaction.TransactionId} Created, Total:{Volatile.Read(ref tranTotal)}");
        return transaction;
    }
    public override void TransactionCommitted(DbTransactionCompletedEventArgs eventArgs)
    {
        var transaction = eventArgs.Transaction;
        var startTime = (DateTime)eventArgs.EventData;
        var elapsed = DateTime.UtcNow.Subtract(startTime).TotalMilliseconds;
        this.output.WriteLine($"Commit completed, Total:{Volatile.Read(ref tranTotal)}, TransactionId:{transaction.TransactionId}, Elapsed: {elapsed} ms");
    }
    public override void TransactionFailed(DbTransactionCompletedEventArgs eventArgs)
    {
        var transaction = eventArgs.Transaction;
        var startTime = (DateTime)eventArgs.EventData;
        var elapsed = DateTime.UtcNow.Subtract(startTime).TotalMilliseconds;
        this.output.WriteLine($"Rollback completed, Total:{Volatile.Read(ref tranTotal)}, TransactionId:{transaction.TransactionId}, Elapsed: {elapsed} ms");
    }
}