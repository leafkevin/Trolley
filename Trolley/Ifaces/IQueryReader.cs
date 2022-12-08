using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IQueryReader : IDisposable , IAsyncDisposable
{
    TEntity Read<TEntity>();
    List<TEntity> ReadList<TEntity>();
    IPagedList<TEntity> ReadPageList<TEntity>();
    Dictionary<TKey, TElement> ToDictionary<TEntity, TKey, TElement>(Func<TEntity, TKey> keySelector, Func<TEntity, TElement> valueSelector) where TKey : notnull;

    Task<TEntity> ReadAsync<TEntity>(CancellationToken cancellationToken = default);
    Task<List<TEntity>> ReadListAsync<TEntity>(CancellationToken cancellationToken = default);
    Task<IPagedList<TEntity>> ReadPageListAsync<TEntity>(CancellationToken cancellationToken = default);
    Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TEntity, TKey, TElement>(Func<TEntity, TKey> keySelector, Func<TEntity, TElement> valueSelector, CancellationToken cancellationToken = default) where TKey : notnull;
}
