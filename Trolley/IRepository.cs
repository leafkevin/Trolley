using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IRepository : IUnitOfWork, IDisposable
{
    #region 属性
    IOrmProvider OrmProvider { get; }
    IDbTransaction Transaction { get; }
    #endregion

    #region 同步方法
    TEntity QueryFirst<TEntity>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text);
    TTarget QueryFirst<TEntity, TTarget>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text);
    List<TEntity> Query<TEntity>(object whereObj);
    List<TTarget> Query<TEntity, TTarget>(object whereObj);
    List<TEntity> Query<TEntity>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text);
    List<TTarget> Query<TEntity, TTarget>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text);
    List<TEntity> QueryAll<TEntity>();
    List<TTarget> QueryAll<TEntity, TTarget>();
    IPagedList<TEntity> QueryPage<TEntity>(int pageIndex, int pageSize, string orderBy = null);
    IPagedList<TTarget> QueryPage<TEntity, TTarget>(int pageIndex, int pageSize, string orderBy = null);
    IPagedList<TEntity> QueryPage<TEntity>(object whereObj, int pageIndex, int pageSize, string orderBy = null);
    IPagedList<TTarget> QueryPage<TEntity, TTarget>(object whereObj, int pageIndex, int pageSize, string orderBy = null);
    IPagedList<TEntity> QueryPage<TEntity>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text);
    IPagedList<TEntity> QueryPage<TEntity>(string rawSql, int pageIndex, int pageSize, string orderBy = null, object whereObj = null, CommandType cmdType = CommandType.Text);
    IQueryReader QueryMultiple(string sql, object whereObj = null, CommandType cmdType = CommandType.Text);
    //IQueryReader QueryMultiple(Action<IQueryBuilder> queryInitializer);
    #endregion

    #region 异步方法
    Task<TEntity> QueryFirstAsync<TEntity>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default);
    Task<TTarget> QueryFirstAsync<TEntity, TTarget>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default);
    Task<List<TEntity>> QueryAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    Task<List<TTarget>> QueryAsync<TEntity, TTarget>(object whereObj, CancellationToken cancellationToken = default);
    Task<List<TEntity>> QueryAsync<TEntity>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default);
    Task<List<TTarget>> QueryAsync<TEntity, TTarget>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default);
    Task<List<TEntity>> QueryAllAsync<TEntity>(CancellationToken cancellationToken = default);
    Task<List<TTarget>> QueryAllAsync<TEntity, TTarget>(CancellationToken cancellationToken = default);
    Task<IPagedList<TEntity>> QueryPageAsync<TEntity>(int pageIndex, int pageSize, string orderBy = null, CancellationToken cancellationToken = default);
    Task<IPagedList<TTarget>> QueryPageAsync<TEntity, TTarget>(int pageIndex, int pageSize, string orderBy = null, CancellationToken cancellationToken = default);
    Task<IPagedList<TEntity>> QueryPageAsync<TEntity>(object whereObj, int pageIndex, int pageSize, string orderBy = null, CancellationToken cancellationToken = default);
    Task<IPagedList<TTarget>> QueryPageAsync<TEntity, TTarget>(object whereObj, int pageIndex, int pageSize, string orderBy = null, CancellationToken cancellationToken = default);
    Task<IPagedList<TEntity>> QueryPageAsync<TEntity>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default);
    Task<IPagedList<TEntity>> QueryPageAsync<TEntity>(string rawSql, int pageIndex, int pageSize, string orderBy = null, object whereObj = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default);
    Task<IQueryReader> QueryMultipleAsync(string sql, object whereObj = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default);
    //Task<IQueryReader> QueryMultipleAsync(Action<IQueryBuilder> queryInitializer, CancellationToken cancellationToken = default);
    #endregion

    #region 同步方法
    TEntity Get<TEntity>(object whereObj);
    TTarget Get<TEntity, TTarget>(object whereObj);
    int Create<TEntity>(object entity);
    int Update<TEntity>(object entity);
    int Update<TEntity>(object updateObj, object whereObj);
    int Delete<TEntity>(object whereObj);
    bool Exists<TEntity>(object whereObj);
    int Execute(string sql, object parameters = null, CommandType cmdType = CommandType.Text);
    #endregion

    #region 异步方法
    Task<TEntity> GetAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    Task<TTarget> GetAsync<TEntity, TTarget>(object whereObj, CancellationToken cancellationToken = default);
    Task<int> CreateAsync<TEntity>(object entity, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync<TEntity>(object entity, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync<TEntity>(object updateObj, object whereObj, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    Task<int> ExecuteAsync(string sql, object parameters = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default);
    #endregion

    ISqlExpression<TEntity> From<TEntity>();

    #region Others
    void Close();
    Task CloseAsync();
    #endregion
}
