using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

class FromQuery : IFromQuery
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly string parameterPrefix = "p";
    private QueryVisitor visitor;

    public FromQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, string parameterPrefix = "p")
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.parameterPrefix = parameterPrefix;
    }
    public FromQuery(QueryVisitor visitor, string parameterPrefix = "p")
    {
        this.visitor = visitor;
        this.dbFactory = visitor.dbFactory;
        this.connection = visitor.connection;
        this.transaction = visitor.transaction;
        this.parameterPrefix = parameterPrefix;
    }
    public IFromQuery<T> From<T>(char tableStartAs = 'a')
    {
        this.visitor ??= new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        this.visitor.From(typeof(T));
        return new FromQuery<T>(this.visitor);
    }
    public IFromQuery<T1, T2> From<T1, T2>(char tableStartAs = 'a')
    {
        this.visitor ??= new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        this.visitor.From(typeof(T1), typeof(T2));
        return new FromQuery<T1, T2>(this.visitor);
    }
    public IFromQuery<T1, T2, T3> From<T1, T2, T3>(char tableStartAs = 'a')
    {
        this.visitor ??= new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        this.visitor.From(typeof(T1), typeof(T2), typeof(T3));
        return new FromQuery<T1, T2, T3>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableStartAs = 'a')
    {
        this.visitor ??= new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        this.visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new FromQuery<T1, T2, T3, T4>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableStartAs = 'a')
    {
        this.visitor ??= new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        this.visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new FromQuery<T1, T2, T3, T4, T5>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableStartAs = 'a')
    {
        this.visitor ??= new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        this.visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return new FromQuery<T1, T2, T3, T4, T5, T6>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableStartAs = 'a')
    {
        this.visitor ??= new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        this.visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableStartAs = 'a')
    {
        this.visitor ??= new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        this.visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableStartAs = 'a')
    {
        this.visitor ??= new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        this.visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this.visitor);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T> : IFromQuery<T>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly QueryVisitor visitor;

    public FromQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromQuery<T, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var fromQuery = new FromQuery(this.visitor, $"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new FromQuery<T, TOther>(this.visitor);
    }
    public IFromQuery<T> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T> Where(Expression<Func<T, bool>> predicate = null)
    {
        if (predicate != null)
            this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T> And(bool condition, Expression<Func<T, bool>> predicate = null)
    {
        if (condition && predicate != null)
            this.visitor.And(predicate);
        return this;
    }
    public IFromGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T, TGrouping>(this.visitor);
    }
    public IFromQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<T> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2> : IFromQuery<T1, T2>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly QueryVisitor visitor;

    public FromQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromQuery<T1, T2, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var fromQuery = new FromQuery(this.visitor, $"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new FromQuery<T1, T2, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T1, T2> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate = null)
    {
        if (predicate != null)
            this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> predicate = null)
    {
        if (condition && predicate != null)
            this.visitor.And(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, TGrouping>(this.visitor);
    }
    public IFromQuery<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3> : IFromQuery<T1, T2, T3>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly QueryVisitor visitor;

    public FromQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromQuery<T1, T2, T3, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var fromQuery = new FromQuery(this.visitor, $"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new FromQuery<T1, T2, T3, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T1, T2, T3> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate = null)
    {
        if (predicate != null)
            this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> predicate = null)
    {
        if (condition && predicate != null)
            this.visitor.And(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, TGrouping>(this.visitor);
    }
    public IFromQuery<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4> : IFromQuery<T1, T2, T3, T4>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly QueryVisitor visitor;

    public FromQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromQuery<T1, T2, T3, T4, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var fromQuery = new FromQuery(this.visitor, $"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new FromQuery<T1, T2, T3, T4, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate = null)
    {
        if (predicate != null)
            this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> predicate = null)
    {
        if (condition && predicate != null)
            this.visitor.And(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, TGrouping>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4, T5> : IFromQuery<T1, T2, T3, T4, T5>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly QueryVisitor visitor;

    public FromQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromQuery<T1, T2, T3, T4, T5, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var fromQuery = new FromQuery(this.visitor, $"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new FromQuery<T1, T2, T3, T4, T5, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate = null)
    {
        if (predicate != null)
            this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate = null)
    {
        if (condition && predicate != null)
            this.visitor.And(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, TGrouping>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4, T5, T6> : IFromQuery<T1, T2, T3, T4, T5, T6>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly QueryVisitor visitor;

    public FromQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromQuery<T1, T2, T3, T4, T5, T6, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var fromQuery = new FromQuery(this.visitor, $"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new FromQuery<T1, T2, T3, T4, T5, T6, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate = null)
    {
        if (predicate != null)
            this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate = null)
    {
        if (condition && predicate != null)
            this.visitor.And(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7> : IFromQuery<T1, T2, T3, T4, T5, T6, T7>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly QueryVisitor visitor;

    public FromQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var fromQuery = new FromQuery(this.visitor, $"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate = null)
    {
        if (predicate != null)
            this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate = null)
    {
        if (condition && predicate != null)
            this.visitor.And(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8> : IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly QueryVisitor visitor;

    public FromQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var fromQuery = new FromQuery(this.visitor, $"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate = null)
    {
        if (predicate != null)
            this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate = null)
    {
        if (condition && predicate != null)
            this.visitor.And(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}

class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    private int unionIndex = 0;
    private readonly QueryVisitor visitor;

    public FromQuery(QueryVisitor visitor) => this.visitor = visitor;
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.visitor, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(TOther), sql, dbParameters);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", null, joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate = null)
    {
        if (predicate != null)
            this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate = null)
    {
        if (condition && predicate != null)
            this.visitor.And(predicate);
        return this;
    }
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new FromQuery<TTarget>(this.visitor);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}