using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface ITheaTransaction
{
    string TransactionId { get; }
    ITheaConnection Connection { get; }
    IDbTransaction BaseTransaction { get; }
    IDbInterceptor DbInterceptor { get; set; }

    void Commit();
    Task CommitAsync(CancellationToken cancellationToken = default);
    void Rollback();
    Task RollbackAsync(CancellationToken cancellationToken = default);
    void Dispose();
    ValueTask DisposeAsync();
}