using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public class PostgreSqlGroupingCommand<T, TGrouping> : GroupingCommand<T, TGrouping>, IPostgreSqlGroupingCommand<T, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingCommand<T, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingCommand<T, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingCommand<T, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingCommand<T, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingCommand<T, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingCommand<T, TGrouping>;
    public new IPostgreSqlGroupingCommand<T, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingCommand<T, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingCommand<T, TGrouping>;
    public new IPostgreSqlGroupingCommand<T, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlGroupingCommand<T, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TGrouping> Select()
        => base.Select() as IPostgreSqlFromCommand<TGrouping>;
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T, TTarget>> fieldsExpr)
        => base.Select<TTarget>(fieldsExpr) as IPostgreSqlFromCommand<TTarget>;
    #endregion
}
public class PostgreSqlGroupingCommand<T1, T2, TGrouping> : GroupingCommand<T1, T2, TGrouping>, IPostgreSqlGroupingCommand<T1, T2, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingCommand<T1, T2, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingCommand<T1, T2, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingCommand<T1, T2, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingCommand<T1, T2, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingCommand<T1, T2, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingCommand<T1, T2, TGrouping>;
    public new IPostgreSqlGroupingCommand<T1, T2, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingCommand<T1, T2, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingCommand<T1, T2, TGrouping>;
    public new IPostgreSqlGroupingCommand<T1, T2, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlGroupingCommand<T1, T2, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TGrouping> Select()
        => base.Select() as IPostgreSqlFromCommand<TGrouping>;
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TTarget>> fieldsExpr)
        => base.Select<TTarget>(fieldsExpr) as IPostgreSqlFromCommand<TTarget>;
    #endregion
}
public class PostgreSqlGroupingCommand<T1, T2, T3, TGrouping> : GroupingCommand<T1, T2, T3, TGrouping>, IPostgreSqlGroupingCommand<T1, T2, T3, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingCommand<T1, T2, T3, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingCommand<T1, T2, T3, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingCommand<T1, T2, T3, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingCommand<T1, T2, T3, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingCommand<T1, T2, T3, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingCommand<T1, T2, T3, TGrouping>;
    public new IPostgreSqlGroupingCommand<T1, T2, T3, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingCommand<T1, T2, T3, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingCommand<T1, T2, T3, TGrouping>;
    public new IPostgreSqlGroupingCommand<T1, T2, T3, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2, T3>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlGroupingCommand<T1, T2, T3, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TGrouping> Select()
        => base.Select() as IPostgreSqlFromCommand<TGrouping>;
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TTarget>> fieldsExpr)
        => base.Select<TTarget>(fieldsExpr) as IPostgreSqlFromCommand<TTarget>;
    #endregion
}
public class PostgreSqlGroupingCommand<T1, T2, T3, T4, TGrouping> : GroupingCommand<T1, T2, T3, T4, TGrouping>, IPostgreSqlGroupingCommand<T1, T2, T3, T4, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingCommand<T1, T2, T3, T4, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingCommand<T1, T2, T3, T4, TGrouping>;
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingCommand<T1, T2, T3, T4, TGrouping>;
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2, T3, T4>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlGroupingCommand<T1, T2, T3, T4, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TGrouping> Select()
        => base.Select() as IPostgreSqlFromCommand<TGrouping>;
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TTarget>> fieldsExpr)
        => base.Select<TTarget>(fieldsExpr) as IPostgreSqlFromCommand<TTarget>;
    #endregion
}
public class PostgreSqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> : GroupingCommand<T1, T2, T3, T4, T5, TGrouping>, IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping>;
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping>;
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TGrouping> Select()
        => base.Select() as IPostgreSqlFromCommand<TGrouping>;
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
        => base.Select<TTarget>(fieldsExpr) as IPostgreSqlFromCommand<TTarget>;
    #endregion
}
public class PostgreSqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> : GroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping>, IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping>;
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping>;
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TGrouping> Select()
        => base.Select() as IPostgreSqlFromCommand<TGrouping>;
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
        => base.Select<TTarget>(fieldsExpr) as IPostgreSqlFromCommand<TTarget>;
    #endregion
}