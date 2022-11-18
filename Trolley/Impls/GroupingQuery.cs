using System;
using System.Linq.Expressions;

namespace Trolley;

class GroupingQuery<T, TGrouping> : IGroupingQuery<T, TGrouping>
{
    private readonly QueryVisitor visitor;

    public GroupingQuery(QueryVisitor visitor)
    {
        this.visitor = visitor;
    }
    public IGroupingQuery<T, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
    {
        return this;
    }
    public IGroupingQuery<T, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate) { throw new NotImplementedException(); }
    public IGroupingQuery<T, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr) { throw new NotImplementedException(); }
    public IGroupingQuery<T, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr) { throw new NotImplementedException(); }

    public IQuery<TGrouping> Select() { throw new NotImplementedException(); }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T, TTarget>> fieldsExpr) { throw new NotImplementedException(); }
    public string ToSql() { throw new NotImplementedException(); }
}
class GroupingQuery<T1, T2, TGrouping> : IGroupingQuery<T1, T2, TGrouping>
{
    private readonly QueryVisitor visitor;

    public GroupingQuery(QueryVisitor visitor)
    {
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
    {
        return this;
    }
    public IGroupingQuery<T1, T2, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr) { throw new NotImplementedException(); }

    public IQuery<TGrouping> Select() { throw new NotImplementedException(); }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TTarget>> fieldsExpr) { throw new NotImplementedException(); }
    public string ToSql() { throw new NotImplementedException(); }
}
class GroupingQuery<T1, T2, T3, TGrouping> : IGroupingQuery<T1, T2, T3, TGrouping>
{
    private readonly QueryVisitor visitor;

    public GroupingQuery(QueryVisitor visitor)
    {
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
    {
        return this;
    }
    public IGroupingQuery<T1, T2, T3, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr) { throw new NotImplementedException(); }

    public IQuery<TGrouping> Select() { throw new NotImplementedException(); }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TTarget>> fieldsExpr) { throw new NotImplementedException(); }
    public string ToSql() { throw new NotImplementedException(); }
}
class GroupingQuery<T1, T2, T3, T4, TGrouping> : IGroupingQuery<T1, T2, T3, T4, TGrouping>
{
    private readonly QueryVisitor visitor;

    public GroupingQuery(QueryVisitor visitor)
    {
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
    {
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr) { throw new NotImplementedException(); }

    public IQuery<TGrouping> Select() { throw new NotImplementedException(); }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TTarget>> fieldsExpr) { throw new NotImplementedException(); }
    public string ToSql() { throw new NotImplementedException(); }
}
class GroupingQuery<T1, T2, T3, T4, T5, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, TGrouping>
{
    private readonly QueryVisitor visitor;

    public GroupingQuery(QueryVisitor visitor)
    {
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
    {
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr) { throw new NotImplementedException(); }

    public IQuery<TGrouping> Select() { throw new NotImplementedException(); }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr) { throw new NotImplementedException(); }
    public string ToSql() { throw new NotImplementedException(); }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>
{
    private readonly QueryVisitor visitor;

    public GroupingQuery(QueryVisitor visitor)
    {
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr) { throw new NotImplementedException(); }

    public IQuery<TGrouping> Select() { throw new NotImplementedException(); }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr) { throw new NotImplementedException(); }
    public string ToSql() { throw new NotImplementedException(); }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>
{
    private readonly QueryVisitor visitor;

    public GroupingQuery(QueryVisitor visitor)
    {
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr) { throw new NotImplementedException(); }

    public IQuery<TGrouping> Select() { throw new NotImplementedException(); }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr) { throw new NotImplementedException(); }
    public string ToSql() { throw new NotImplementedException(); }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>
{
    private readonly QueryVisitor visitor;

    public GroupingQuery(QueryVisitor visitor)
    {
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr) { throw new NotImplementedException(); }

    public IQuery<TGrouping> Select() { throw new NotImplementedException(); }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr) { throw new NotImplementedException(); }
    public string ToSql() { throw new NotImplementedException(); }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>
{
    private readonly QueryVisitor visitor;

    public GroupingQuery(QueryVisitor visitor)
    {
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr) { throw new NotImplementedException(); }

    public IQuery<TGrouping> Select() { throw new NotImplementedException(); }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr) { throw new NotImplementedException(); }
    public string ToSql() { throw new NotImplementedException(); }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>
{
    private readonly QueryVisitor visitor;

    public GroupingQuery(QueryVisitor visitor)
    {
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr) { throw new NotImplementedException(); }

    public IQuery<TGrouping> Select() { throw new NotImplementedException(); }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr) { throw new NotImplementedException(); }
    public string ToSql() { throw new NotImplementedException(); }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>
{
    private readonly QueryVisitor visitor;

    public GroupingQuery(QueryVisitor visitor)
    {
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr) { throw new NotImplementedException(); }

    public IQuery<TGrouping> Select() { throw new NotImplementedException(); }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr) { throw new NotImplementedException(); }
    public string ToSql() { throw new NotImplementedException(); }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>
{
    private readonly QueryVisitor visitor;

    public GroupingQuery(QueryVisitor visitor)
    {
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr) { throw new NotImplementedException(); }

    public IQuery<TGrouping> Select() { throw new NotImplementedException(); }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr) { throw new NotImplementedException(); }
    public string ToSql() { throw new NotImplementedException(); }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>
{
    private readonly QueryVisitor visitor;

    public GroupingQuery(QueryVisitor visitor)
    {
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr) { throw new NotImplementedException(); }

    public IQuery<TGrouping> Select() { throw new NotImplementedException(); }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr) { throw new NotImplementedException(); }
    public string ToSql() { throw new NotImplementedException(); }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>
{
    private readonly QueryVisitor visitor;

    public GroupingQuery(QueryVisitor visitor)
    {
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr) { throw new NotImplementedException(); }

    public IQuery<TGrouping> Select() { throw new NotImplementedException(); }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr) { throw new NotImplementedException(); }
    public string ToSql() { throw new NotImplementedException(); }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>
{
    private readonly QueryVisitor visitor;

    public GroupingQuery(QueryVisitor visitor)
    {
        this.visitor = visitor;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        return this;
    }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr) { throw new NotImplementedException(); }
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr) { throw new NotImplementedException(); }

    public IQuery<TGrouping> Select() { throw new NotImplementedException(); }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr) { throw new NotImplementedException(); }
    public string ToSql() { throw new NotImplementedException(); }
}
class GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping> : IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>
{
    private readonly QueryVisitor visitor;

    public GroupingQuery(QueryVisitor visitor)
    {
        this.visitor = visitor;
    }
    public IQuery<TGrouping> Select() { throw new NotImplementedException(); }
    public string ToSql() { throw new NotImplementedException(); }
}