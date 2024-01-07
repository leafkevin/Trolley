using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IUnitOfWork
{
    void BeginTransaction();
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    void Commit();
    Task CommitAsync(CancellationToken cancellationToken = default);
    void Rollback();
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
