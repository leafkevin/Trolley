using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public interface IMySqlDeleted<TEntity, TResult> : IDeleted<TEntity>
{
    #region Execute
    /// <summary>
    /// 执行删除操作，并返回已删除数据
    /// </summary>
    /// <returns>返回已删除的选择字段值列表</returns>
    new List<TResult> Execute();
    /// <summary>
    /// 执行删除操作，并返回已删除数据
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回已删除的选择字段值列表</returns>
    new Task<List<TResult>> ExecuteAsync(CancellationToken cancellationToken = default);
    #endregion
}