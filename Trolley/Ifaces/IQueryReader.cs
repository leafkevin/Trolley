using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IQueryReader : IDisposable//, IAsyncDisposable
{
    TEntity Read<TEntity>();
    List<TEntity> ReadList<TEntity>();
    IPagedList<TEntity> ReadPageList<TEntity>();

    Task<TEntity> ReadAsync<TEntity>(CancellationToken cancellationToken = default);
    Task<List<TEntity>> ReadListAsync<TEntity>(CancellationToken cancellationToken = default);
    Task<IPagedList<TEntity>> ReadPageListAsync<TEntity>(CancellationToken cancellationToken = default);
}
