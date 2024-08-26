using System;

namespace Trolley.PostgreSql;

public class PostgreSqlRepository : Repository, IPostgreSqlRepository
{
    #region Constructor
    public PostgreSqlRepository(DbContext dbContext) : base(dbContext) { }
    #endregion

    #region From
    public override IPostgreSqlQuery<T> From<T>(char tableAsStart = 'a')
        => base.From<T>(tableAsStart) as IPostgreSqlQuery<T>;
    public override IPostgreSqlQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
        => base.From<T1, T2>(tableAsStart) as IPostgreSqlQuery<T1, T2>;
    public override IPostgreSqlQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
        => base.From<T1, T2, T3>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3>;
    public override IPostgreSqlQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public override IPostgreSqlQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public override IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public override IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6, T7>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public override IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6, T7, T8>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public override IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public override IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    #endregion

    #region From SubQuery
    public override IPostgreSqlQuery<T> From<T>(IQuery<T> subQuery)
        => base.From<T>(subQuery) as IPostgreSqlQuery<T>;
    public override IPostgreSqlQuery<T> From<T>(Func<IFromQuery, IQuery<T>> subQuery)
        => base.From<T>(subQuery) as IPostgreSqlQuery<T>;
    #endregion

    #region Create
    public override IPostgreSqlCreate<TEntity> Create<TEntity>()
        => this.OrmProvider.NewCreate<TEntity>(this.DbContext) as IPostgreSqlCreate<TEntity>;
    #endregion

    #region Update
    public override IPostgreSqlUpdate<TEntity> Update<TEntity>()
        => this.OrmProvider.NewUpdate<TEntity>(this.DbContext) as IPostgreSqlUpdate<TEntity>;
    #endregion
}