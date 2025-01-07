using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Trolley.PostgreSql;

public interface IPostgreSqlCreated<TEntity> : ICreated<TEntity> { }
public interface IPostgreSqlCreated<TEntity, TResult> : IPostgreSqlCreated<TEntity>
{
    #region Execute
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <returns>返回插入行数</returns>
    new TResult Execute();
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回插入行数</returns>
    new Task<TResult> ExecuteAsync(CancellationToken cancellationToken = default);
    #endregion
}
public interface IPostgreSqlBulkCreated<TEntity, TResult> : IPostgreSqlCreated<TEntity>
{
    #region Execute
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <returns>返回插入行数</returns>
    new List<TResult> Execute();
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回插入行数</returns>
    new Task<List<TResult>> ExecuteAsync(CancellationToken cancellationToken = default);
    #endregion   
}