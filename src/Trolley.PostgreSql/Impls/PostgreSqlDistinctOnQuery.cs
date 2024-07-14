using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public class PostgreSqlDistinctOnQueryBase<TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    #region Properties
    public DbContext DbContext { get; protected set; }
    public IQueryVisitor Visitor { get; protected set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public PostgreSqlDistinctOnQueryBase(DbContext dbContext, IQueryVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Select
    public virtual IPostgreSqlQuery<TDistinctOn> Select()
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.SelectDistinctOn();
        return this.OrmProvider.NewQuery<TDistinctOn>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TDistinctOn>;
    }
    public virtual IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.Visitor.Select(fields, null);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    #endregion
}
public class PostgreSqlDistinctOnQuery<T, TDistinctOn> : PostgreSqlDistinctOnQueryBase<TDistinctOn>, IPostgreSqlDistinctOnQuery<T, TDistinctOn>
{
    #region Constructor
    public PostgreSqlDistinctOnQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IPostgreSqlDistinctOnQuery<T, TDistinctOn> OrderBy<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public virtual IPostgreSqlDistinctOnQuery<T, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select
    public virtual IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    public virtual IPostgreSqlQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    #endregion
}
public class PostgreSqlDistinctOnQuery<T1, T2, TDistinctOn> : PostgreSqlDistinctOnQueryBase<TDistinctOn>, IPostgreSqlDistinctOnQuery<T1, T2, TDistinctOn>
{
    #region Constructor
    public PostgreSqlDistinctOnQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, TDistinctOn> OrderBy<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select
    public virtual IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    public virtual IPostgreSqlQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    #endregion
}
public class PostgreSqlDistinctOnQuery<T1, T2, T3, TDistinctOn> : PostgreSqlDistinctOnQueryBase<TDistinctOn>, IPostgreSqlDistinctOnQuery<T1, T2, T3, TDistinctOn>
{
    #region Constructor
    public PostgreSqlDistinctOnQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, TDistinctOn> OrderBy<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select
    public virtual IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    public virtual IPostgreSqlQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    #endregion
}
public class PostgreSqlDistinctOnQuery<T1, T2, T3, T4, TDistinctOn> : PostgreSqlDistinctOnQueryBase<TDistinctOn>, IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, TDistinctOn>
{
    #region Constructor
    public PostgreSqlDistinctOnQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, TDistinctOn> OrderBy<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select
    public virtual IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    public virtual IPostgreSqlQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    #endregion
}
public class PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, TDistinctOn> : PostgreSqlDistinctOnQueryBase<TDistinctOn>, IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, TDistinctOn>
{
    #region Constructor
    public PostgreSqlDistinctOnQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, TDistinctOn> OrderBy<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select
    public virtual IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    public virtual IPostgreSqlQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    #endregion
}
public class PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, TDistinctOn> : PostgreSqlDistinctOnQueryBase<TDistinctOn>, IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, TDistinctOn>
{
    #region Constructor
    public PostgreSqlDistinctOnQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, TDistinctOn> OrderBy<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select
    public virtual IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    public virtual IPostgreSqlQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    #endregion
}
public class PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, TDistinctOn> : PostgreSqlDistinctOnQueryBase<TDistinctOn>, IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, TDistinctOn>
{
    #region Constructor
    public PostgreSqlDistinctOnQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, TDistinctOn> OrderBy<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select
    public virtual IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    public virtual IPostgreSqlQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    #endregion
}
public class PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, TDistinctOn> : PostgreSqlDistinctOnQueryBase<TDistinctOn>, IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, TDistinctOn>
{
    #region Constructor
    public PostgreSqlDistinctOnQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, TDistinctOn> OrderBy<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select
    public virtual IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    public virtual IPostgreSqlQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    #endregion
}
public class PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDistinctOn> : PostgreSqlDistinctOnQueryBase<TDistinctOn>, IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDistinctOn>
{
    #region Constructor
    public PostgreSqlDistinctOnQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDistinctOn> OrderBy<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select
    public virtual IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    public virtual IPostgreSqlQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    #endregion
}
public class PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDistinctOn> : PostgreSqlDistinctOnQueryBase<TDistinctOn>, IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDistinctOn>
{
    #region Constructor
    public PostgreSqlDistinctOnQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDistinctOn> OrderBy<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select
    public virtual IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    public virtual IPostgreSqlQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    #endregion
}
public class PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDistinctOn> : PostgreSqlDistinctOnQueryBase<TDistinctOn>, IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDistinctOn>
{
    #region Constructor
    public PostgreSqlDistinctOnQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDistinctOn> OrderBy<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select
    public virtual IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    public virtual IPostgreSqlQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    #endregion
}
public class PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDistinctOn> : PostgreSqlDistinctOnQueryBase<TDistinctOn>, IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDistinctOn>
{
    #region Constructor
    public PostgreSqlDistinctOnQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDistinctOn> OrderBy<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select
    public virtual IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    public virtual IPostgreSqlQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    #endregion
}
public class PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDistinctOn> : PostgreSqlDistinctOnQueryBase<TDistinctOn>, IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDistinctOn>
{
    #region Constructor
    public PostgreSqlDistinctOnQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDistinctOn> OrderBy<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select
    public virtual IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    public virtual IPostgreSqlQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    #endregion
}
public class PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDistinctOn> : PostgreSqlDistinctOnQueryBase<TDistinctOn>, IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDistinctOn>
{
    #region Constructor
    public PostgreSqlDistinctOnQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDistinctOn> OrderBy<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select
    public virtual IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    public virtual IPostgreSqlQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    #endregion
}
public class PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDistinctOn> : PostgreSqlDistinctOnQueryBase<TDistinctOn>, IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDistinctOn>
{
    #region Constructor
    public PostgreSqlDistinctOnQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDistinctOn> OrderBy<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select
    public virtual IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    public virtual IPostgreSqlQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<IGroupingObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor) as IPostgreSqlQuery<TTarget>;
    }
    #endregion
}
public class PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TDistinctOn> : PostgreSqlDistinctOnQueryBase<TDistinctOn>, IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TDistinctOn>
{
    #region Constructor
    public PostgreSqlDistinctOnQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion
}