﻿using System;
using System.Linq.Expressions;

namespace Trolley;

public class MultiGroupingQueryBase<TGrouping> : QueryInternal, IMultiGroupingQueryBase<TGrouping>
{
    #region Properties
    public MultipleQuery MultipleQuery { get; set; }
    #endregion

    #region Constructor
    public MultiGroupingQueryBase(MultipleQuery multiQuery, IQueryVisitor visitor)
    {
        this.MultipleQuery = multiQuery;
        this.Visitor = visitor;
    }
    #endregion

    #region Select
    public IMultiQuery<TGrouping> Select()
    {
        this.Visitor.SelectGrouping();
        return this.OrmProvider.NewMultiQuery<TGrouping>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> Select<TTarget>(string fields = "*")
    {
        base.SelectInternal(fields);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion
}
class MultiGroupingQuery<T, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
        => this.Having(true, predicate);
    public IMultiGroupingQuery<T, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiGroupingQuery<T, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiGroupingQuery<T, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion
}
public class MultiGroupingQuery<T1, T2, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
        => this.Having(true, predicate);
    public IMultiGroupingQuery<T1, T2, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion
}
public class MultiGroupingQuery<T1, T2, T3, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
        => this.Having(true, predicate);
    public IMultiGroupingQuery<T1, T2, T3, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion
}
public class MultiGroupingQuery<T1, T2, T3, T4, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
        => this.Having(true, predicate);
    public IMultiGroupingQuery<T1, T2, T3, T4, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion
}
public class MultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
        => this.Having(true, predicate);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion
}
public class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
        => this.Having(true, predicate);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion
}
public class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
        => this.Having(true, predicate);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion
}
public class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
        => this.Having(true, predicate);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion
}
public class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
        => this.Having(true, predicate);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion
}
public class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
        => this.Having(true, predicate);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion
}
public class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
        => this.Having(true, predicate);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion
}
public class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
        => this.Having(true, predicate);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion
}
public class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
        => this.Having(true, predicate);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion
}
public class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
        => this.Having(true, predicate);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion
}
public class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
        => this.Having(true, predicate);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion
}
public class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion
}