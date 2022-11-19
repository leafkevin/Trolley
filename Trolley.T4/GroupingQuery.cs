using System;
using System.Linq.Expressions;

namespace Trolley;

class GroupingQuery<T, TGrouping> : IGroupingQuery<T, TGrouping>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public GroupingQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IGroupingQuery<T, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IGroupingQuery<T, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new Query<TGrouping>(dbFactory, connection, transaction, visitor);
    }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T, TTarget>> fieldsExpr)
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(dbFactory, connection, transaction, visitor);
    }
    public string ToSql()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out _, out _);
    }
}
class GroupingQuery<T1, T2, TGrouping> : IGroupingQuery<T1, T2, TGrouping>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public GroupingQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IGroupingQuery<T1, T2, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new Query<TGrouping>(dbFactory, connection, transaction, visitor);
    }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(dbFactory, connection, transaction, visitor);
    }
    public string ToSql()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out _, out _);
    }
}
class GroupingQuery<T1, T2, T3, TGrouping> : IGroupingQuery<T1, T2, T3, TGrouping>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public GroupingQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new Query<TGrouping>(dbFactory, connection, transaction, visitor);
    }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(dbFactory, connection, transaction, visitor);
    }
    public string ToSql()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out _, out _);
    }
}
class GroupingQuery<T1, T2, T3, T4, TGrouping> : IGroupingQuery<T1, T2, T3, T4, TGrouping>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public GroupingQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new Query<TGrouping>(dbFactory, connection, transaction, visitor);
    }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(dbFactory, connection, transaction, visitor);
    }
    public string ToSql()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out _, out _);
    }
}
class GroupingQuery<T1, T2, T3, T4, T5, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, TGrouping>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public GroupingQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new Query<TGrouping>(dbFactory, connection, transaction, visitor);
    }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(dbFactory, connection, transaction, visitor);
    }
    public string ToSql()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out _, out _);
    }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public GroupingQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new Query<TGrouping>(dbFactory, connection, transaction, visitor);
    }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(dbFactory, connection, transaction, visitor);
    }
    public string ToSql()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out _, out _);
    }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public GroupingQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new Query<TGrouping>(dbFactory, connection, transaction, visitor);
    }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(dbFactory, connection, transaction, visitor);
    }
    public string ToSql()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out _, out _);
    }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public GroupingQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new Query<TGrouping>(dbFactory, connection, transaction, visitor);
    }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(dbFactory, connection, transaction, visitor);
    }
    public string ToSql()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out _, out _);
    }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public GroupingQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new Query<TGrouping>(dbFactory, connection, transaction, visitor);
    }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(dbFactory, connection, transaction, visitor);
    }
    public string ToSql()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out _, out _);
    }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public GroupingQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new Query<TGrouping>(dbFactory, connection, transaction, visitor);
    }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(dbFactory, connection, transaction, visitor);
    }
    public string ToSql()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out _, out _);
    }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public GroupingQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new Query<TGrouping>(dbFactory, connection, transaction, visitor);
    }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(dbFactory, connection, transaction, visitor);
    }
    public string ToSql()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out _, out _);
    }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public GroupingQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new Query<TGrouping>(dbFactory, connection, transaction, visitor);
    }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(dbFactory, connection, transaction, visitor);
    }
    public string ToSql()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out _, out _);
    }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public GroupingQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new Query<TGrouping>(dbFactory, connection, transaction, visitor);
    }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(dbFactory, connection, transaction, visitor);
    }
    public string ToSql()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out _, out _);
    }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public GroupingQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new Query<TGrouping>(dbFactory, connection, transaction, visitor);
    }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(dbFactory, connection, transaction, visitor);
    }
    public string ToSql()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out _, out _);
    }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public GroupingQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new Query<TGrouping>(dbFactory, connection, transaction, visitor);
    }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(dbFactory, connection, transaction, visitor);
    }
    public string ToSql()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out _, out _);
    }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public GroupingQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new Query<TGrouping>(dbFactory, connection, transaction, visitor);
    }
    public string ToSql()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out _, out _);
    }
}