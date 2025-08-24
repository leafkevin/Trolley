using System;
using System.Linq.Expressions;

namespace Trolley;

public class GroupingCommandBase<TEntity, TGrouping> : QueryInternal, IGroupingCommandBase<TEntity, TGrouping>
{
    #region Constructor
    public GroupingCommandBase(DbContext dbContext, IQueryVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Select    
    public virtual IFromCommand<TEntity, TGrouping> Select()
    {
        this.Visitor.SelectGrouping();
        return this.OrmProvider.NewFromCommand<TEntity, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingCommand<TEntity, T, TGrouping> : GroupingCommandBase<TEntity, TGrouping>, IGroupingCommand<TEntity, T, TGrouping>
{
    #region Constructor
    public GroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public virtual IGroupingCommand<TEntity, T, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
        => this.Having(true, predicate);
    public virtual IGroupingCommand<TEntity, T, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IGroupingCommand<TEntity, T, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IGroupingCommand<TEntity, T, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        this.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IGroupingCommand<TEntity, T, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IGroupingCommand<TEntity, T, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        this.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IGroupingCommand<TEntity, T, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<IGroupingAggregate<TGrouping>, T>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingCommand<TEntity, T1, T2, TGrouping> : GroupingCommandBase<TEntity, TGrouping>, IGroupingCommand<TEntity, T1, T2, TGrouping>
{
    #region Constructor
    public GroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public virtual IGroupingCommand<TEntity, T1, T2, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
        => this.Having(true, predicate);
    public virtual IGroupingCommand<TEntity, T1, T2, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IGroupingCommand<TEntity, T1, T2, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IGroupingCommand<TEntity, T1, T2, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IGroupingCommand<TEntity, T1, T2, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IGroupingCommand<TEntity, T1, T2, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IGroupingCommand<TEntity, T1, T2, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingCommand<TEntity, T1, T2, T3, TGrouping> : GroupingCommandBase<TEntity, TGrouping>, IGroupingCommand<TEntity, T1, T2, T3, TGrouping>
{
    #region Constructor
    public GroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public virtual IGroupingCommand<TEntity, T1, T2, T3, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
        => this.Having(true, predicate);
    public virtual IGroupingCommand<TEntity, T1, T2, T3, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IGroupingCommand<TEntity, T1, T2, T3, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IGroupingCommand<TEntity, T1, T2, T3, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IGroupingCommand<TEntity, T1, T2, T3, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IGroupingCommand<TEntity, T1, T2, T3, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IGroupingCommand<TEntity, T1, T2, T3, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2, T3>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2, T3>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingCommand<TEntity, T1, T2, T3, T4, TGrouping> : GroupingCommandBase<TEntity, TGrouping>, IGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping>
{
    #region Constructor
    public GroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
        => this.Having(true, predicate);
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2, T3, T4>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2, T3, T4>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping> : GroupingCommandBase<TEntity, TGrouping>, IGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping>
{
    #region Constructor
    public GroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
        => this.Having(true, predicate);
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class GroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping> : GroupingCommandBase<TEntity, TGrouping>, IGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping>
{
    #region Constructor
    public GroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
        => this.Having(true, predicate);
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.HavingInternal(condition, predicate);
        return this;
    }
    #endregion

    #region OrderBy/OrderByDescending
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}