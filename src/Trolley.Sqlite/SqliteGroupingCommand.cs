using System;
using System.Linq.Expressions;

namespace Trolley.Sqlite;

public class SqliteGroupingCommand<T, TGrouping> : GroupingCommand<T, TGrouping>, ISqliteGroupingCommand<T, TGrouping>
{
    #region Constructor
    public SqliteGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new ISqliteGroupingCommand<T, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
        => this.Having(true, predicate);
    public new ISqliteGroupingCommand<T, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public new ISqliteGroupingCommand<T, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new ISqliteGroupingCommand<T, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        this.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new ISqliteGroupingCommand<T, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new ISqliteGroupingCommand<T, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        this.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new ISqliteFromCommand<TGrouping> Select()
        => base.Select() as ISqliteFromCommand<TGrouping>;
    public new ISqliteFromCommand<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as ISqliteFromCommand<TTarget>;
    public new ISqliteFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as ISqliteFromCommand<TTarget>;
    #endregion
}
public class SqliteGroupingCommand<T1, T2, TGrouping> : GroupingCommand<T1, T2, TGrouping>, ISqliteGroupingCommand<T1, T2, TGrouping>
{
    #region Constructor
    public SqliteGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new ISqliteGroupingCommand<T1, T2, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
        => this.Having(true, predicate);
    public new ISqliteGroupingCommand<T1, T2, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public new ISqliteGroupingCommand<T1, T2, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new ISqliteGroupingCommand<T1, T2, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new ISqliteGroupingCommand<T1, T2, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new ISqliteGroupingCommand<T1, T2, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new ISqliteFromCommand<TGrouping> Select()
        => base.Select() as ISqliteFromCommand<TGrouping>;
    public new ISqliteFromCommand<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as ISqliteFromCommand<TTarget>;
    public new ISqliteFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as ISqliteFromCommand<TTarget>;
    #endregion
}
public class SqliteGroupingCommand<T1, T2, T3, TGrouping> : GroupingCommand<T1, T2, T3, TGrouping>, ISqliteGroupingCommand<T1, T2, T3, TGrouping>
{
    #region Constructor
    public SqliteGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new ISqliteGroupingCommand<T1, T2, T3, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
        => this.Having(true, predicate);
    public new ISqliteGroupingCommand<T1, T2, T3, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public new ISqliteGroupingCommand<T1, T2, T3, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new ISqliteGroupingCommand<T1, T2, T3, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new ISqliteGroupingCommand<T1, T2, T3, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new ISqliteGroupingCommand<T1, T2, T3, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new ISqliteFromCommand<TGrouping> Select()
        => base.Select() as ISqliteFromCommand<TGrouping>;
    public new ISqliteFromCommand<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as ISqliteFromCommand<TTarget>;
    public new ISqliteFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as ISqliteFromCommand<TTarget>;
    #endregion
}
public class SqliteGroupingCommand<T1, T2, T3, T4, TGrouping> : GroupingCommand<T1, T2, T3, T4, TGrouping>, ISqliteGroupingCommand<T1, T2, T3, T4, TGrouping>
{
    #region Constructor
    public SqliteGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new ISqliteGroupingCommand<T1, T2, T3, T4, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
        => this.Having(true, predicate);
    public new ISqliteGroupingCommand<T1, T2, T3, T4, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public new ISqliteGroupingCommand<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new ISqliteGroupingCommand<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new ISqliteGroupingCommand<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new ISqliteGroupingCommand<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new ISqliteFromCommand<TGrouping> Select()
        => base.Select() as ISqliteFromCommand<TGrouping>;
    public new ISqliteFromCommand<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as ISqliteFromCommand<TTarget>;
    public new ISqliteFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as ISqliteFromCommand<TTarget>;
    #endregion
}
public class SqliteGroupingCommand<T1, T2, T3, T4, T5, TGrouping> : GroupingCommand<T1, T2, T3, T4, T5, TGrouping>, ISqliteGroupingCommand<T1, T2, T3, T4, T5, TGrouping>
{
    #region Constructor
    public SqliteGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new ISqliteGroupingCommand<T1, T2, T3, T4, T5, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
        => this.Having(true, predicate);
    public new ISqliteGroupingCommand<T1, T2, T3, T4, T5, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public new ISqliteGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new ISqliteGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new ISqliteGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new ISqliteGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new ISqliteFromCommand<TGrouping> Select()
        => base.Select() as ISqliteFromCommand<TGrouping>;
    public new ISqliteFromCommand<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as ISqliteFromCommand<TTarget>;
    public new ISqliteFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as ISqliteFromCommand<TTarget>;
    #endregion
}
public class SqliteGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> : GroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping>, ISqliteGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping>
{
    #region Constructor
    public SqliteGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new ISqliteGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
        => this.Having(true, predicate);
    public new ISqliteGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public new ISqliteGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new ISqliteGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new ISqliteGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new ISqliteGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new ISqliteFromCommand<TGrouping> Select()
        => base.Select() as ISqliteFromCommand<TGrouping>;
    public new ISqliteFromCommand<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as ISqliteFromCommand<TTarget>;
    public new ISqliteFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as ISqliteFromCommand<TTarget>;
    #endregion
}