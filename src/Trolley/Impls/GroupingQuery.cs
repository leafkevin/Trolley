﻿using System;
using System.Linq.Expressions;

namespace Trolley;

public class GroupingQueryBase<TGrouping> : IGroupingQueryBase<TGrouping>
{
    #region Properties   
    public DbContext DbContext { get; protected set; }
    public IQueryVisitor Visitor { get; protected set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public GroupingQueryBase(DbContext dbContext, IQueryVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Select/SelectAnonymous
    public IQueryAnonymousObject SelectAnonymous()
    {
        this.Visitor.Select("*");
        return new QueryAnonymousObject(this.Visitor);
    }
    public IQuery<TGrouping> Select()
    {
        this.Visitor.SelectGrouping();
        return this.OrmProvider.NewQuery<TGrouping>(this.DbContext, this.Visitor);
    }
    public IQuery<TTarget> Select<TTarget>(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.Visitor.Select(fields, null);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingQuery<T, TGrouping> : GroupingQueryBase<TGrouping>, IGroupingQuery<T, TGrouping>
{
    #region Constructor
    public GroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public IGroupingQuery<T, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
        => this.Having(true, predicate);
    public IGroupingQuery<T, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
    {
        if (condition)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            this.Visitor.Having(predicate);
        }
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IGroupingQuery<T, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IGroupingQuery<T, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IGroupingQuery<T, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IGroupingQuery<T, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
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
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingQuery<T1, T2, TGrouping> : GroupingQueryBase<TGrouping>, IGroupingQuery<T1, T2, TGrouping>
{
    #region Constructor
    public GroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public IGroupingQuery<T1, T2, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
    {
        if (condition)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            this.Visitor.Having(predicate);
        }
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IGroupingQuery<T1, T2, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IGroupingQuery<T1, T2, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IGroupingQuery<T1, T2, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IGroupingQuery<T1, T2, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
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
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingQuery<T1, T2, T3, TGrouping> : GroupingQueryBase<TGrouping>, IGroupingQuery<T1, T2, T3, TGrouping>
{
    #region Constructor
    public GroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public IGroupingQuery<T1, T2, T3, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
    {
        if (condition)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            this.Visitor.Having(predicate);
        }
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IGroupingQuery<T1, T2, T3, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IGroupingQuery<T1, T2, T3, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
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
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingQuery<T1, T2, T3, T4, TGrouping> : GroupingQueryBase<TGrouping>, IGroupingQuery<T1, T2, T3, T4, TGrouping>
{
    #region Constructor
    public GroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public IGroupingQuery<T1, T2, T3, T4, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
    {
        if (condition)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            this.Visitor.Having(predicate);
        }
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IGroupingQuery<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
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
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingQuery<T1, T2, T3, T4, T5, TGrouping> : GroupingQueryBase<TGrouping>, IGroupingQuery<T1, T2, T3, T4, T5, TGrouping>
{
    #region Constructor
    public GroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (condition)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            this.Visitor.Having(predicate);
        }
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
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
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> : GroupingQueryBase<TGrouping>, IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>
{
    #region Constructor
    public GroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (condition)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            this.Visitor.Having(predicate);
        }
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
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
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> : GroupingQueryBase<TGrouping>, IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>
{
    #region Constructor
    public GroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (condition)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            this.Visitor.Having(predicate);
        }
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
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
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> : GroupingQueryBase<TGrouping>, IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>
{
    #region Constructor
    public GroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (condition)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            this.Visitor.Having(predicate);
        }
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
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
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> : GroupingQueryBase<TGrouping>, IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>
{
    #region Constructor
    public GroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (condition)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            this.Visitor.Having(predicate);
        }
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
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
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> : GroupingQueryBase<TGrouping>, IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>
{
    #region Constructor
    public GroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        if (condition)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            this.Visitor.Having(predicate);
        }
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
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
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> : GroupingQueryBase<TGrouping>, IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>
{
    #region Constructor
    public GroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        if (condition)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            this.Visitor.Having(predicate);
        }
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
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
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> : GroupingQueryBase<TGrouping>, IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>
{
    #region Constructor
    public GroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        if (condition)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            this.Visitor.Having(predicate);
        }
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
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
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> : GroupingQueryBase<TGrouping>, IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>
{
    #region Constructor
    public GroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        if (condition)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            this.Visitor.Having(predicate);
        }
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
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
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> : GroupingQueryBase<TGrouping>, IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>
{
    #region Constructor
    public GroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        if (condition)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            this.Visitor.Having(predicate);
        }
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
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
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> : GroupingQueryBase<TGrouping>, IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>
{
    #region Constructor
    public GroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        if (condition)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            this.Visitor.Having(predicate);
        }
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
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
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping> : GroupingQueryBase<TGrouping>, IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>
{
    #region Constructor
    public GroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion
}