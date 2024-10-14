using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface ITheaConnection : IDisposable, IAsyncDisposable
{
    string DbKey { get; }
    string ConnectionId { get; }
    IDbConnection BaseConnection { get; }

    string ConnectionString { get; set; }
    int ConnectionTimeout { get; }
    string Database { get; }
    ConnectionState State { get; }
    string ServerVersion { get; }

    Action<ConectionEventArgs> OnOpening { get; set; }
    Action<ConectionEventArgs> OnOpened { get; set; }
    Action<ConectionEventArgs> OnClosing { get; set; }
    Action<ConectionEventArgs> OnClosed { get; set; }
    Action<TransactionEventArgs> OnTransactionCreated { get; set; }
    Action<TransactionCompletedEventArgs> OnTransactionCompleted { get; set; }

    ITheaCommand CreateCommand(IDbCommand command);
    void Open();
    Task OpenAsync(CancellationToken cancellationToken = default);
    void Close();
    Task CloseAsync();
    ITheaTransaction BeginTransaction();
    ValueTask<ITheaTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}