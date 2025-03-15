using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public interface IMySqlCreated<TEntity, TResult> : ICreated<TEntity>
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
public interface IMySqlBulkCreated<TEntity, TResult> : ICreated<TEntity>
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
