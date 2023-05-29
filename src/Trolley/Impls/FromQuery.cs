using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

class FromQuery : IFromQuery
{
    private readonly IQueryVisitor visitor;

    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;

    public IFromQuery<T> From<T>(char tableAsStart = 'a')
    {
        this.visitor.TableAsStart = tableAsStart;
        this.visitor.From(tableAsStart, typeof(T));
        return new FromQuery<T>(this.visitor);
    }
    public IFromQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
    {
        this.visitor.TableAsStart = tableAsStart;
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2));
        return new FromQuery<T1, T2>(this.visitor);
    }
    public IFromQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
    {
        this.visitor.TableAsStart = tableAsStart;
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3));
        return new FromQuery<T1, T2, T3>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
    {
        this.visitor.TableAsStart = tableAsStart;
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new FromQuery<T1, T2, T3, T4>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
    {
        this.visitor.TableAsStart = tableAsStart;
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new FromQuery<T1, T2, T3, T4, T5>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
    {
        this.visitor.TableAsStart = tableAsStart;
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return new FromQuery<T1, T2, T3, T4, T5, T6>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
    {
        this.visitor.TableAsStart = tableAsStart;
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
    {
        this.visitor.TableAsStart = tableAsStart;
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
    {
        this.visitor.TableAsStart = tableAsStart;
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
    {
        this.visitor.TableAsStart = tableAsStart;
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(char tableAsStart = 'a')
    {
        this.visitor.TableAsStart = tableAsStart;
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(char tableAsStart = 'a')
    {
        this.visitor.TableAsStart = tableAsStart;
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(char tableAsStart = 'a')
    {
        this.visitor.TableAsStart = tableAsStart;
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(char tableAsStart = 'a')
    {
        this.visitor.TableAsStart = tableAsStart;
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(char tableAsStart = 'a')
    {
        this.visitor.TableAsStart = tableAsStart;
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this.visitor);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T> : IFromQuery<T>
{
    private int unionIndex = 0;
    private readonly IQueryVisitor visitor;

    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;

    #region Union/UnionAll
    public IFromQuery<T> Union(Func<IFromQuery, IFromQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone('a', $"p{this.unionIndex++}u");
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = " UNION" + Environment.NewLine + newVisitor.BuildSql(out var dbParameters, out var readerFields, true);
        this.visitor.Union(sql, readerFields, dbParameters);
        return this;
    }
    public IFromQuery<T> UnionAll(Func<IFromQuery, IFromQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone('a', $"p{this.unionIndex++}u");
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = " UNION ALL" + Environment.NewLine + newVisitor.BuildSql(out var dbParameters, out var readerFields, true);
        this.visitor.Union(sql, readerFields, dbParameters);
        return this;
    }
    public IFromQuery<T> UnionRecursive(Func<IFromQuery, IFromQuery<T>, IFromQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone('a', $"p{this.unionIndex++}u");
        subQuery.Invoke(new FromQuery(newVisitor), null);
        var sql = " UNION" + Environment.NewLine + newVisitor.BuildSql(out var dbParameters, out var readerFields, true);
        this.visitor.Union(sql, readerFields, dbParameters);
        return this;
    }
    public IFromQuery<T> UnionAllRecursive(Func<IFromQuery, IFromQuery<T>, IFromQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone('a', $"p{this.unionIndex++}u");
        subQuery.Invoke(new FromQuery(newVisitor), null);
        var sql = " UNION ALL" + Environment.NewLine + newVisitor.BuildSql(out var dbParameters, out var readerFields, true);
        this.visitor.Union(sql, readerFields, dbParameters);
        return this;
    }
    #endregion

    #region Join
    public IFromQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T, TOther>(this.visitor);
    }
    public IFromQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T, TOther>(this.visitor);
    }
    public IFromQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T, TOther>(this.visitor);
    }
    public IFromQuery<T, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T, TTarget>(this.visitor);
    }
    public IFromQuery<T, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T, TTarget>(this.visitor);
    }
    public IFromQuery<T, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T, TTarget>(this.visitor);
    }
    #endregion

    #region Where/And
    public IFromQuery<T> Where(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    public IFromQuery<T> And(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T> And(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
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
    public IFromQuery<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (condition)
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
    public IFromQuery<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IFromQuery<T> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IFromQuery<T> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }

    #region Select
    public IFromQuery<T> Select()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr, true);
        return this;
    }
    public IQueryAnonymousObject Select(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    #endregion

    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2> : IFromQuery<T1, T2>
{
    private readonly IQueryVisitor visitor;

    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;

    #region Join
    public IFromQuery<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, TTarget>(this.visitor);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2> And(Expression<Func<T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
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
    public IFromQuery<T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
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
    public IFromQuery<T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IFromQuery<T1, T2> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }

    #region Select
    public IQueryAnonymousObject Select(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    #endregion

    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3> : IFromQuery<T1, T2, T3>
{
    private readonly IQueryVisitor visitor;

    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;

    #region Join
    public IFromQuery<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, TTarget>(this.visitor);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
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
    public IFromQuery<T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
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
    public IFromQuery<T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IFromQuery<T1, T2, T3> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }

    #region Select
    public IQueryAnonymousObject Select(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    #endregion

    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4> : IFromQuery<T1, T2, T3, T4>
{
    private readonly IQueryVisitor visitor;

    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;

    #region Join
    public IFromQuery<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, TTarget>(this.visitor);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
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
    public IFromQuery<T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
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
    public IFromQuery<T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IFromQuery<T1, T2, T3, T4> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }

    #region Select
    public IQueryAnonymousObject Select(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    #endregion

    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4, T5> : IFromQuery<T1, T2, T3, T4, T5>
{
    private readonly IQueryVisitor visitor;

    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, TTarget>(this.visitor);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
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
    public IFromQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
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
    public IFromQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IFromQuery<T1, T2, T3, T4, T5> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }

    #region Select
    public IQueryAnonymousObject Select(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    #endregion

    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4, T5, T6> : IFromQuery<T1, T2, T3, T4, T5, T6>
{
    private readonly IQueryVisitor visitor;

    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, TTarget>(this.visitor);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
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
    public IFromQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
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
    public IFromQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IFromQuery<T1, T2, T3, T4, T5, T6> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }

    #region Select
    public IQueryAnonymousObject Select(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    #endregion

    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7> : IFromQuery<T1, T2, T3, T4, T5, T6, T7>
{
    private readonly IQueryVisitor visitor;

    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget>(this.visitor);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
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
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
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
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }

    #region Select
    public IQueryAnonymousObject Select(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    #endregion

    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8> : IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8>
{
    private readonly IQueryVisitor visitor;

    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>(this.visitor);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
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
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
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
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }

    #region Select
    public IQueryAnonymousObject Select(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    #endregion

    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    private readonly IQueryVisitor visitor;

    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>(this.visitor);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
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
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
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
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }

    #region Select
    public IQueryAnonymousObject Select(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    #endregion

    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    private readonly IQueryVisitor visitor;

    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>(this.visitor);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }

    #region Select
    public IQueryAnonymousObject Select(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    #endregion

    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    private readonly IQueryVisitor visitor;

    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>(this.visitor);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }

    #region Select
    public IQueryAnonymousObject Select(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    #endregion

    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    private readonly IQueryVisitor visitor;

    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>(this.visitor);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }

    #region Select
    public IQueryAnonymousObject Select(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    #endregion

    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    private readonly IQueryVisitor visitor;

    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>(this.visitor);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }

    #region Select
    public IQueryAnonymousObject Select(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    #endregion

    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    private readonly IQueryVisitor visitor;

    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>(this.visitor);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>(this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }

    #region Select
    public IQueryAnonymousObject Select(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    #endregion

    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    private readonly IQueryVisitor visitor;

    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region OrderBy
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IQueryAnonymousObject Select(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.visitor);
    }
    #endregion

    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}