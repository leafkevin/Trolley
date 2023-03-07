using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IRepository : IUnitOfWork, IDisposable, IAsyncDisposable
{
    #region 属性
    IOrmProvider OrmProvider { get; }
    IDbConnection Connection { get; }
    IDbTransaction Transaction { get; }
    #endregion

    #region Query
    IQuery<T> From<T>(char tableAsStart = 'a', string suffixRawSql = null);
    IQuery<T> From<T>(Func<IFromQuery, IFromQuery<T>> subQuery, char tableAsStart = 'a');
    IQuery<T> From<T>(string rawSql, object parameters = null, char tableAsStart = 'a');
    IQuery<T> FromWith<T>(Func<IFromQuery, IFromQuery<T>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');

    IQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a');
    IQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a');
    TEntity QueryFirst<TEntity>(string rawSql, object parameters = null);
    Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, object parameters = null, CancellationToken cancellationToken = default);
    TEntity QueryFirst<TEntity>(object whereObj);
    Task<TEntity> QueryFirstAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    List<TEntity> Query<TEntity>(string rawSql, object parameters = null);
    Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, object parameters = null, CancellationToken cancellationToken = default);
    List<TEntity> Query<TEntity>(object whereObj);
    Task<List<TEntity>> QueryAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    #endregion

    #region Get
    TEntity Get<TEntity>(object whereObj);
    Task<TEntity> GetAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    #endregion

    #region Create
    ICreate<T> Create<T>();
    #endregion

    #region Update  
    IUpdate<T> Update<T>();
    #endregion

    #region Delete
    IDelete<T> Delete<T>();
    #endregion

    #region Exists
    bool Exists<TEntity>(object whereObj);
    Task<bool> ExistsAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    bool Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate);
    Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default);
    #endregion

    #region Execute
    int Execute(string sql, object parameters = null);
    Task<int> ExecuteAsync(string sql, object parameters = null, CancellationToken cancellationToken = default);
    #endregion

    #region Others
    void Close();
    Task CloseAsync();
    void Timeout(int timeout);
    #endregion
}
