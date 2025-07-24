using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public class MySqlGroupingCommand<T, TGrouping> : GroupingCommand<T, TGrouping>, IMySqlGroupingCommand<T, TGrouping>
{
    #region Constructor
    public MySqlGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IMySqlGroupingCommand<T, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
        => this.Having(true, predicate);
    public new IMySqlGroupingCommand<T, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public new IMySqlGroupingCommand<T, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IMySqlGroupingCommand<T, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        this.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new IMySqlGroupingCommand<T, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IMySqlGroupingCommand<T, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        this.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new IMySqlFromCommand<TGrouping> Select()
        => base.Select() as IMySqlFromCommand<TGrouping>;
    public new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T, TTarget>> fieldsExpr)
        => base.Select<TTarget>(fieldsExpr) as IMySqlFromCommand<TTarget>;
    #endregion
}
public class MySqlGroupingCommand<T1, T2, TGrouping> : GroupingCommand<T1, T2, TGrouping>, IMySqlGroupingCommand<T1, T2, TGrouping>
{
    #region Constructor
    public MySqlGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IMySqlGroupingCommand<T1, T2, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
        => this.Having(true, predicate);
    public new IMySqlGroupingCommand<T1, T2, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public new IMySqlGroupingCommand<T1, T2, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IMySqlGroupingCommand<T1, T2, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new IMySqlGroupingCommand<T1, T2, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IMySqlGroupingCommand<T1, T2, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new IMySqlFromCommand<TGrouping> Select()
        => base.Select() as IMySqlFromCommand<TGrouping>;
    public new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TTarget>> fieldsExpr)
        => base.Select<TTarget>(fieldsExpr) as IMySqlFromCommand<TTarget>;
    #endregion
}
public class MySqlGroupingCommand<T1, T2, T3, TGrouping> : GroupingCommand<T1, T2, T3, TGrouping>, IMySqlGroupingCommand<T1, T2, T3, TGrouping>
{
    #region Constructor
    public MySqlGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IMySqlGroupingCommand<T1, T2, T3, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
        => this.Having(true, predicate);
    public new IMySqlGroupingCommand<T1, T2, T3, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public new IMySqlGroupingCommand<T1, T2, T3, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IMySqlGroupingCommand<T1, T2, T3, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new IMySqlGroupingCommand<T1, T2, T3, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IMySqlGroupingCommand<T1, T2, T3, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new IMySqlFromCommand<TGrouping> Select()
        => base.Select() as IMySqlFromCommand<TGrouping>;
    public new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TTarget>> fieldsExpr)
        => base.Select<TTarget>(fieldsExpr) as IMySqlFromCommand<TTarget>;
    #endregion
}
public class MySqlGroupingCommand<T1, T2, T3, T4, TGrouping> : GroupingCommand<T1, T2, T3, T4, TGrouping>, IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping>
{
    #region Constructor
    public MySqlGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
        => this.Having(true, predicate);
    public new IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public new IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new IMySqlFromCommand<TGrouping> Select()
        => base.Select() as IMySqlFromCommand<TGrouping>;
    public new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TTarget>> fieldsExpr)
        => base.Select<TTarget>(fieldsExpr) as IMySqlFromCommand<TTarget>;
    #endregion
}
public class MySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> : GroupingCommand<T1, T2, T3, T4, T5, TGrouping>, IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping>
{
    #region Constructor
    public MySqlGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
        => this.Having(true, predicate);
    public new IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public new IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new IMySqlFromCommand<TGrouping> Select()
        => base.Select() as IMySqlFromCommand<TGrouping>;
    public new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
        => base.Select<TTarget>(fieldsExpr) as IMySqlFromCommand<TTarget>;
    #endregion
}
public class MySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> : GroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping>, IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping>
{
    #region Constructor
    public MySqlGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
        => this.Having(true, predicate);
    public new IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public new IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new IMySqlFromCommand<TGrouping> Select()
        => base.Select() as IMySqlFromCommand<TGrouping>;
    public new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
        => base.Select<TTarget>(fieldsExpr) as IMySqlFromCommand<TTarget>;
    #endregion
}