using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface ITheaConnection : IDbConnection, IAsyncDisposable
{
    string DbKey { get; }
    string ConnectionId { get; }
    IDbConnection DbConnection { get; }
    IDbInterceptor Interceptor { get; set; }

    string ServerVersion { get; }

    new ITheaCommand CreateCommand();
    Task OpenAsync(CancellationToken cancellationToken = default);
    Task CloseAsync();
    new ITheaTransaction BeginTransaction();
    new ITheaTransaction BeginTransaction(IsolationLevel il);
    ValueTask<ITheaTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    ValueTask<ITheaTransaction> BeginTransactionAsync(IsolationLevel il, CancellationToken cancellationToken = default);
}