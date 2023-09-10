using System;
using System.Linq.Expressions;

namespace Trolley;

class MultiGroupingQueryBase<TGrouping>
{
    #region Fields
    protected MultipleQuery multiQuery;
    protected IOrmProvider ormProvider;
    protected IQueryVisitor visitor;
    #endregion

    #region Constructor
    public MultiGroupingQueryBase(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
    {
        this.multiQuery = multiQuery;
        this.ormProvider = ormProvider;
        this.visitor = visitor;
    }
    #endregion

    #region Select
    public IMultiQuery<TGrouping> Select()
    {
        this.visitor.SelectGrouping(true);
        return new MultiQuery<TGrouping>(this.multiQuery, this.ormProvider, this.visitor);
    }
    public IMultiQuery<TTarget> Select<TTarget>(string fields)
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new MultiQuery<TTarget>(this.multiQuery, this.ormProvider, this.visitor);
    }
    #endregion
}
class MultiGroupingQuery<T, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IMultiGroupingQuery<T, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.ormProvider, this.visitor);
    }
    #endregion
}
class MultiGroupingQuery<T1, T2, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.ormProvider, this.visitor);
    }
    #endregion
}
class MultiGroupingQuery<T1, T2, T3, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.ormProvider, this.visitor);
    }
    #endregion
}
class MultiGroupingQuery<T1, T2, T3, T4, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.ormProvider, this.visitor);
    }
    #endregion
}
class MultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.ormProvider, this.visitor);
    }
    #endregion
}
class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.ormProvider, this.visitor);
    }
    #endregion
}
class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.ormProvider, this.visitor);
    }
    #endregion
}
class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.ormProvider, this.visitor);
    }
    #endregion
}
class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.ormProvider, this.visitor);
    }
    #endregion
}
class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.ormProvider, this.visitor);
    }
    #endregion
}
class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.ormProvider, this.visitor);
    }
    #endregion
}
class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.ormProvider, this.visitor);
    }
    #endregion
}
class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.ormProvider, this.visitor);
    }
    #endregion
}
class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }
    #endregion

    #region Having
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.ormProvider, this.visitor);
    }
    #endregion
}
class MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> : MultiGroupingQueryBase<TGrouping>, IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>
{
    #region Constructor
    public MultiGroupingQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.ormProvider, this.visitor);
    }
    #endregion
}