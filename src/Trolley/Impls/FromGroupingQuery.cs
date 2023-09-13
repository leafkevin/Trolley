using System;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

class FromGroupingQueryBase<TGrouping>
{
    #region Fields
    protected TheaConnection connection;
    protected IDbTransaction transaction;
    protected IOrmProvider ormProvider;
    protected IEntityMapProvider mapProvider;
    protected IQueryVisitor visitor;
    protected Type insertType;
    #endregion

    #region Constructor
    public FromGroupingQueryBase(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.visitor = visitor;
        this.insertType = insertType;
    }
    #endregion

    #region Select
    public IFromQuery<TGrouping> Select()
    {
        this.visitor.SelectGrouping(true);
        return new FromQuery<TGrouping>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IQueryAnonymousObject Select(string fields)
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(string fields)
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new FromQuery<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromGroupingQuery<T, TGrouping> : FromGroupingQueryBase<TGrouping>, IFromGroupingQuery<T, TGrouping>
{
    #region Constructor
    public FromGroupingQuery(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, transaction, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Having
    public IFromGroupingQuery<T, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition) this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region Having
    public IFromGroupingQuery<T, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromGroupingQuery<T1, T2, TGrouping> : FromGroupingQueryBase<TGrouping>, IFromGroupingQuery<T1, T2, TGrouping>
{
    #region Constructor
    public FromGroupingQuery(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, transaction, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Having
    public IFromGroupingQuery<T1, T2, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IFromGroupingQuery<T1, T2, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromGroupingQuery<T1, T2, T3, TGrouping> : FromGroupingQueryBase<TGrouping>, IFromGroupingQuery<T1, T2, T3, TGrouping>
{
    #region Constructor
    public FromGroupingQuery(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, transaction, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Having
    public IFromGroupingQuery<T1, T2, T3, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IFromGroupingQuery<T1, T2, T3, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromGroupingQuery<T1, T2, T3, T4, TGrouping> : FromGroupingQueryBase<TGrouping>, IFromGroupingQuery<T1, T2, T3, T4, TGrouping>
{
    #region Constructor
    public FromGroupingQuery(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, transaction, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Having
    public IFromGroupingQuery<T1, T2, T3, T4, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IFromGroupingQuery<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromGroupingQuery<T1, T2, T3, T4, T5, TGrouping> : FromGroupingQueryBase<TGrouping>, IFromGroupingQuery<T1, T2, T3, T4, T5, TGrouping>
{
    #region Constructor
    public FromGroupingQuery(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, transaction, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Having
    public IFromGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IFromGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> : FromGroupingQueryBase<TGrouping>, IFromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>
{
    #region Constructor
    public FromGroupingQuery(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, transaction, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Having
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> : FromGroupingQueryBase<TGrouping>, IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>
{
    #region Constructor
    public FromGroupingQuery(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, transaction, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Having
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> : FromGroupingQueryBase<TGrouping>, IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>
{
    #region Constructor
    public FromGroupingQuery(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, transaction, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Having
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> : FromGroupingQueryBase<TGrouping>, IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>
{
    #region Constructor
    public FromGroupingQuery(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, transaction, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Having
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> : FromGroupingQueryBase<TGrouping>, IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>
{
    #region Constructor
    public FromGroupingQuery(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, transaction, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Having
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> : FromGroupingQueryBase<TGrouping>, IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>
{
    #region Constructor
    public FromGroupingQuery(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, transaction, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Having
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> : FromGroupingQueryBase<TGrouping>, IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>
{
    #region Constructor
    public FromGroupingQuery(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, transaction, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Having
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> : FromGroupingQueryBase<TGrouping>, IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>
{
    #region Constructor
    public FromGroupingQuery(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, transaction, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Having
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> : FromGroupingQueryBase<TGrouping>, IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>
{
    #region Constructor
    public FromGroupingQuery(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, transaction, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Having
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.Having(predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> : FromGroupingQueryBase<TGrouping>, IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>
{
    #region Constructor
    public FromGroupingQuery(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, transaction, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}