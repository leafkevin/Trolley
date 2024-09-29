using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface ITheaTransaction : IDisposable
{
    ITheaConnection Connection { get; }
    IDbTransaction BaseTransaction { get; }
    IsolationLevel IsolationLevel { get; }
#if NET5_0_OR_GREATER
    bool SupportsSavepoints { get; }
#endif
    void Commit();
    Task CommitAsync(CancellationToken cancellationToken = default);
    void Rollback(string savepointName = default);
    Task RollbackAsync(string savepointName = default, CancellationToken cancellationToken = default);
    void Release(string savepointName);
    Task ReleaseAsync(string savepointName, CancellationToken cancellationToken = default);
    void Save(string savepointName);
    Task SaveAsync(string savepointName, CancellationToken cancellationToken = default);
    ValueTask DisposeAsync();
}
