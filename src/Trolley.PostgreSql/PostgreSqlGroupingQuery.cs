using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public class PostgreSqlGroupingQuery<T, TGrouping> : GroupingQuery<T, TGrouping>, IPostgreSqlGroupingQuery<T, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingQuery<T, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingQuery<T, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingQuery<T, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingQuery<T, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T, TGrouping>;
    public new IPostgreSqlGroupingQuery<T, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlQuery<TGrouping> Select()
        => base.Select() as IPostgreSqlQuery<TGrouping>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>("*") as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlGroupingQuery<T1, T2, TGrouping> : GroupingQuery<T1, T2, TGrouping>, IPostgreSqlGroupingQuery<T1, T2, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingQuery<T1, T2, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingQuery<T1, T2, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingQuery<T1, T2, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingQuery<T1, T2, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, TGrouping>;
    public new IPostgreSqlGroupingQuery<T1, T2, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlQuery<TGrouping> Select()
        => base.Select() as IPostgreSqlQuery<TGrouping>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlGroupingQuery<T1, T2, T3, TGrouping> : GroupingQuery<T1, T2, T3, TGrouping>, IPostgreSqlGroupingQuery<T1, T2, T3, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingQuery<T1, T2, T3, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingQuery<T1, T2, T3, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingQuery<T1, T2, T3, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, TGrouping>;
    public new IPostgreSqlGroupingQuery<T1, T2, T3, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlQuery<TGrouping> Select()
        => base.Select() as IPostgreSqlQuery<TGrouping>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>("*") as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlGroupingQuery<T1, T2, T3, T4, TGrouping> : GroupingQuery<T1, T2, T3, T4, TGrouping>, IPostgreSqlGroupingQuery<T1, T2, T3, T4, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, TGrouping>;
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlQuery<TGrouping> Select()
        => base.Select() as IPostgreSqlQuery<TGrouping>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>("*") as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, TGrouping> : GroupingQuery<T1, T2, T3, T4, T5, TGrouping>, IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, TGrouping>;
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlQuery<TGrouping> Select()
        => base.Select() as IPostgreSqlQuery<TGrouping>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>("*") as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> : GroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>, IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>;
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlQuery<TGrouping> Select()
        => base.Select() as IPostgreSqlQuery<TGrouping>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>("*") as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> : GroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>, IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>;
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlQuery<TGrouping> Select()
        => base.Select() as IPostgreSqlQuery<TGrouping>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>("*") as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> : GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>, IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>;
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlQuery<TGrouping> Select()
        => base.Select() as IPostgreSqlQuery<TGrouping>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>("*") as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> : GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>, IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>;
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlQuery<TGrouping> Select()
        => base.Select() as IPostgreSqlQuery<TGrouping>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>("*") as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> : GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>, IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>;
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlQuery<TGrouping> Select()
        => base.Select() as IPostgreSqlQuery<TGrouping>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>("*") as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> : GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>, IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>;
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlQuery<TGrouping> Select()
        => base.Select() as IPostgreSqlQuery<TGrouping>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>("*") as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> : GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>, IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>;
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlQuery<TGrouping> Select()
        => base.Select() as IPostgreSqlQuery<TGrouping>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>("*") as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> : GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>, IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>;
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlQuery<TGrouping> Select()
        => base.Select() as IPostgreSqlQuery<TGrouping>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>("*") as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> : GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>, IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>;
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlQuery<TGrouping> Select()
        => base.Select() as IPostgreSqlQuery<TGrouping>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>("*") as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> : GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>, IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Having
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
        => this.Having(true, predicate);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
        => base.Having(condition, predicate) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>;
    #endregion

    #region OrderBy/OrderByDescending
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>;
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>;
    #endregion

    #region Select
    public new IPostgreSqlQuery<TGrouping> Select()
        => base.Select() as IPostgreSqlQuery<TGrouping>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>("*") as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping> : GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>, IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>
{
    #region Constructor
    public PostgreSqlGroupingQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion
}