using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public class MySqlUpdate<TEntity> : Update<TEntity>, IMySqlUpdate<TEntity>
{
    #region Constructor
    public MySqlUpdate(DbContext dbContext)
        : base(dbContext) { }
    #endregion

    #region Join
    public IUpdateJoin<TEntity, T> InnerJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        this.Visitor.Join("INNER JOIN", typeof(T), joinOn);
        return new UpdateJoin<TEntity, T>(this.DbContext, this.Visitor);
    }
    public IUpdateJoin<TEntity, T> LeftJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        this.Visitor.Join("LEFT JOIN", typeof(T), joinOn);
        return new UpdateJoin<TEntity, T>(this.DbContext, this.Visitor);
    }
    #endregion

    #region FromWith CTE
    public IUpdateFrom<TEntity, TSource> FromWith<TSource>(IQuery<TSource> subQuery)
    {
        this.Visitor.From(typeof(TSource));
        return new UpdateFrom<TEntity, TSource>(this.DbContext, this.Visitor);
    }
    public IUpdateFrom<TEntity, TSource> FromWith<TSource>(Func<IFromQuery, IQuery<TSource>> subQuery)
    {
        this.Visitor.From(typeof(TSource));
        return new UpdateFrom<TEntity, TSource>(this.DbContext, this.Visitor);
    }
    #endregion
}
