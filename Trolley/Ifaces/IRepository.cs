using System;
using System.Collections;
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
    IQuery<T> From<T>();
    IQuery<T1, T2> From<T1, T2>();
    IQuery<T1, T2, T3> From<T1, T2, T3>();
    IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>();
    IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>();
    IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>();
    IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>();
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>();
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>();
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
    //IQueryReader QueryMultiple(Action<IMultiQuery> queries);
    //Task<IQueryReader> QueryMultipleAsync(Action<IMultiQuery> queries, CancellationToken cancellationToken = default);
    #endregion

    #region Get
    TEntity Get<TEntity>(object whereObj);
    Task<TEntity> GetAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    #endregion

    #region Create
    ICreate<T> Create<T>();
    #endregion

    #region Update  
    int Update<TEntity>(object updateObj, object whereObj);
    Task<int> UpdateAsync<TEntity>(object updateObj, object whereObj, CancellationToken cancellationToken = default);
    IUpdate<T> Update<T>();
    #endregion

    #region Delete
    int DeleteByKey<TEntity>(object keys);
    Task<int> DeleteByKeyAsync<TEntity>(object keys, CancellationToken cancellationToken = default);
    int Delete<TEntity>(object whereObj);
    Task<int> DeleteAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    IDelete<T> Delete<T>();
    #endregion

    #region Exists
    bool Exists<TEntity>(object anonymousObj);
    Task<bool> ExistsAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    bool Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate);
    Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default);
    #endregion

    #region Execute
    int Execute(string sql, object parameters = null);
    Task<int> ExecuteAsync(string sql, object parameters = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default);
    #endregion

    #region Others
    void Close();
    Task CloseAsync();
    void Timeout(int timeout);
    #endregion
}
