using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface ITheaTransaction : IDbTransaction, IAsyncDisposable
{
    string DbKey { get; }
    string TransactionId { get; }
    new ITheaConnection Connection { get; }
    IDbTransaction DbTransaction { get; }
    IDbInterceptor Interceptor { get; set; }

    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}