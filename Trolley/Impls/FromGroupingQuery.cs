using System;
using System.Linq.Expressions;

namespace Trolley;

class FromGroupingQuery<T, TGrouping> : IFromGroupingQuery<T, TGrouping>
{
    private readonly QueryVisitor visitor;

    public FromGroupingQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromGroupingQuery<T, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new FromQuery<TGrouping>(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T, TTarget>> fieldsExpr)
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
}
class FromGroupingQuery<T1, T2, TGrouping> : IFromGroupingQuery<T1, T2, TGrouping>
{
    private readonly QueryVisitor visitor;

    public FromGroupingQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromGroupingQuery<T1, T2, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new FromQuery<TGrouping>(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
}
class FromGroupingQuery<T1, T2, T3, TGrouping> : IFromGroupingQuery<T1, T2, T3, TGrouping>
{
    private readonly QueryVisitor visitor;

    public FromGroupingQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromGroupingQuery<T1, T2, T3, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new FromQuery<TGrouping>(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
}
class FromGroupingQuery<T1, T2, T3, T4, TGrouping> : IFromGroupingQuery<T1, T2, T3, T4, TGrouping>
{
    private readonly QueryVisitor visitor;

    public FromGroupingQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromGroupingQuery<T1, T2, T3, T4, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new FromQuery<TGrouping>(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
}
class FromGroupingQuery<T1, T2, T3, T4, T5, TGrouping> : IFromGroupingQuery<T1, T2, T3, T4, T5, TGrouping>
{
    private readonly QueryVisitor visitor;

    public FromGroupingQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new FromQuery<TGrouping>(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
}
class FromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> : IFromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>
{
    private readonly QueryVisitor visitor;

    public FromGroupingQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new FromQuery<TGrouping>(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
}
class FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> : IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>
{
    private readonly QueryVisitor visitor;

    public FromGroupingQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new FromQuery<TGrouping>(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
}
class FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> : IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>
{
    private readonly QueryVisitor visitor;

    public FromGroupingQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new FromQuery<TGrouping>(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
}
class FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> : IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>
{
    private readonly QueryVisitor visitor;

    public FromGroupingQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (condition) this.visitor.Having(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TGrouping> Select()
    {
        Expression<Func<TGrouping, TGrouping>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return new FromQuery<TGrouping>(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
}
