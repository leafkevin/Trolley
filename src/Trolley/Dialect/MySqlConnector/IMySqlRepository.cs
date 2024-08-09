namespace Trolley.MySqlConnector;

public interface IMySqlRepository : IRepository
{
    #region Create
    /// <summary>
    /// 创建TEntity类型插入对象
    /// </summary>
    /// <typeparam name="TEntity">插入实体类型</typeparam>
    /// <returns>返回插入对象</returns>
    new IMySqlCreate<TEntity> Create<TEntity>();
    #endregion

    #region Update
    /// <summary>
    /// 创建TEntity类型更新对象
    /// </summary>
    /// <typeparam name="TEntity">更新实体类型</typeparam>
    /// <returns>返回更新对象</returns>
    new IMySqlUpdate<TEntity> Update<TEntity>();
    #endregion
}
