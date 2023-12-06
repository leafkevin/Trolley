namespace Trolley.SqlServer;

public class SqlServerUpdate<TEntity> : Update<TEntity>, ISqlServerUpdate<TEntity>
{
    #region Constructor
    public SqlServerUpdate(DbContext dbContext)
        : base(dbContext) { }
    #endregion

    #region From
    public IUpdateFrom<TEntity, T> From<T>()
    {
        this.Visitor.From(typeof(T));
        return new UpdateFrom<TEntity, T>(this.DbContext, this.Visitor);
    }
    public IUpdateFrom<TEntity, T1, T2> From<T1, T2>()
    {
        this.Visitor.From(typeof(T1), typeof(T2));
        return new UpdateFrom<TEntity, T1, T2>(this.DbContext, this.Visitor);
    }
    public IUpdateFrom<TEntity, T1, T2, T3> From<T1, T2, T3>()
    {
        this.Visitor.From(typeof(T1), typeof(T2), typeof(T3));
        return new UpdateFrom<TEntity, T1, T2, T3>(this.DbContext, this.Visitor);
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>()
    {
        this.Visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new UpdateFrom<TEntity, T1, T2, T3, T4>(this.DbContext, this.Visitor);
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
    {
        this.Visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new UpdateFrom<TEntity, T1, T2, T3, T4, T5>(this.DbContext, this.Visitor);
    }
    #endregion
}
