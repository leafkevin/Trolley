using System;

namespace Trolley.Sqlite;

public class SqliteRepository : Repository, ISqliteRepository
{
    #region Constructor
    public SqliteRepository(DbContext dbContext) : base(dbContext) { }
    #endregion

    #region From
    public new ISqliteQuery<T> From<T>(char tableAsStart = 'a')
        => base.From<T>(tableAsStart) as ISqliteQuery<T>;
    public new ISqliteQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
        => base.From<T1, T2>(tableAsStart) as ISqliteQuery<T1, T2>;
    public new ISqliteQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
        => base.From<T1, T2, T3>(tableAsStart) as ISqliteQuery<T1, T2, T3>;
    public new ISqliteQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4>(tableAsStart) as ISqliteQuery<T1, T2, T3, T4>;
    public new ISqliteQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5>(tableAsStart) as ISqliteQuery<T1, T2, T3, T4, T5>;
    public new ISqliteQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6>(tableAsStart) as ISqliteQuery<T1, T2, T3, T4, T5, T6>;
    public new ISqliteQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6, T7>(tableAsStart) as ISqliteQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new ISqliteQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6, T7, T8>(tableAsStart) as ISqliteQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new ISqliteQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(tableAsStart) as ISqliteQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new ISqliteQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(tableAsStart) as ISqliteQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    #endregion

    #region From SubQuery
    public new ISqliteQuery<T> From<T>(IQuery<T> subQuery)
        => base.From<T>(subQuery) as ISqliteQuery<T>;
    public new ISqliteQuery<T> From<T>(Func<IFromQuery, IQuery<T>> subQuery)
        => base.From<T>(subQuery) as ISqliteQuery<T>;
    #endregion

    #region Create
    public new ISqliteCreate<TEntity> Create<TEntity>() => this.OrmProvider.NewCreate<TEntity>(this.DbContext) as ISqliteCreate<TEntity>;
    #endregion

    #region Update
    public new ISqliteUpdate<TEntity> Update<TEntity>() => this.OrmProvider.NewUpdate<TEntity>(this.DbContext) as ISqliteUpdate<TEntity>;
    #endregion
}