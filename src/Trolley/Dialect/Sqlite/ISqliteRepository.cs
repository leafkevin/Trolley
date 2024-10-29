namespace Trolley.Sqlite;

public interface ISqliteRepository : IRepository
{
    #region Create
    /// <summary>
    /// 创建TEntity类型插入对象
    /// </summary>
    /// <typeparam name="TEntity">插入实体类型</typeparam>
    /// <returns>返回插入对象</returns>
    new ISqliteCreate<TEntity> Create<TEntity>();
    #endregion

    #region Update
    /// <summary>
    /// 创建TEntity类型更新对象
    /// </summary>
    /// <typeparam name="TEntity">更新实体类型</typeparam>
    /// <returns>返回更新对象</returns>
    new ISqliteUpdate<TEntity> Update<TEntity>();
    #endregion
}