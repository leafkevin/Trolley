using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Trolley;

class MultiQueryBase : IMultiQueryBase
{
    #region Properties
    public IQueryVisitor Visitor { get; set; }
    public MultipleQuery MultipleQuery { get; set; }
    public DbContext DbContext => this.MultipleQuery.DbContext;
    #endregion

    #region Constructor
    public MultiQueryBase(MultipleQuery multipleQuery, IQueryVisitor visitor)
    {
        this.MultipleQuery = multipleQuery;
        this.Visitor = visitor;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.Visitor.Select(fields, null);
        return new MultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count() => this.QueryFirstValue<int>("COUNT(1)");
    public IMultipleQuery LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    #endregion    

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        return this.Visitor.BuildSql(out _);
    }
    #endregion

    #region QueryFirstValue
    protected IMultipleQuery QueryFirstValue<TTarget>(string sqlFormat, Expression fieldExpr = null)
    {
        this.Visitor.Select(sqlFormat, fieldExpr);
        var sql = this.Visitor.BuildSql(out _);
        Func<IDataReader, object> readerGetter = reader => reader.To<TTarget>();
        this.MultipleQuery.AddReader(sql, readerGetter);
        return this.MultipleQuery;
    }
    #endregion
}
class MultiQuery<T> : MultiQueryBase, IMultiQuery<T>
{
    #region Fields
    private int? offset;
    private int pageIndex;
    private int pageSize;
    #endregion

    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Union/UnionAll
    public IMultiQuery<T> Union(IMultiQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.Visitor.BuildSql(out var readerFields, false, true);
        this.Visitor.Clear(true);
        var tableSegment = this.Visitor.WithTable(typeof(T), sql, readerFields, true, subQuery);
        sql += " UNION" + Environment.NewLine + subQuery.Visitor.BuildSql(out _, false, true);
        if (!this.Visitor.Equals(subQuery.Visitor))
        {
            this.Visitor.Dispose();
            subQuery.Visitor.CopyTo(this.Visitor);
        }

        this.Visitor.Union(tableSegment, sql);
        return this;
    }
    public IMultiQuery<T> Union(Func<IFromQuery, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.Visitor.BuildSql(out var readerFields, false, true);
        this.Visitor.Clear(true);
        var tableSegment = this.Visitor.WithTable(typeof(T), sql, readerFields, true);
        var fromQuery = new FromQuery(this.DbContext, this.Visitor);
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION" + Environment.NewLine + query.Visitor.BuildSql(out _, false, true);
        if (!this.Visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.Visitor);

        this.Visitor.Union(tableSegment, sql);
        return this;
    }
    public IMultiQuery<T> UnionAll(IMultiQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.Visitor.BuildSql(out var readerFields, false, true);
        this.Visitor.Clear(true);
        var tableSegment = this.Visitor.WithTable(typeof(T), sql, readerFields, true, subQuery);
        sql += " UNION ALL" + Environment.NewLine + subQuery.Visitor.BuildSql(out _, false, true);
        if (!this.Visitor.Equals(subQuery.Visitor))
            subQuery.Visitor.CopyTo(this.Visitor);

        this.Visitor.Union(tableSegment, sql);
        return this;
    }
    public IMultiQuery<T> UnionAll(Func<IFromQuery, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.Visitor.BuildSql(out var readerFields, false, true);
        this.Visitor.Clear(true);
        var tableSegment = this.Visitor.WithTable(typeof(T), sql, readerFields, true);
        var fromQuery = new FromQuery(this.DbContext, this.Visitor);
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL" + Environment.NewLine + query.Visitor.BuildSql(out _, false, true);
        if (!this.Visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.Visitor);

        this.Visitor.Union(tableSegment, sql);
        return this;
    }
    public IMultiQuery<T> UnionRecursive(Func<IFromQuery, IMultiQuery<T>, IQuery<T>> subQuery, string cteTableName)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.Visitor.BuildSql(out var readerFields, false, true);
        this.Visitor.Clear(true);
        var tableSegment = this.Visitor.WithTable(typeof(T), sql, readerFields, cteTableName, this);
        var fromQuery = new FromQuery(this.DbContext, this.Visitor);
        var query = subQuery.Invoke(fromQuery, this);
        sql += " UNION" + Environment.NewLine + query.Visitor.BuildSql(out _, false, true);
        if (!this.Visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.Visitor);

        this.Visitor.Union(tableSegment, sql);
        return this;
    }
    public IMultiQuery<T> UnionAllRecursive(Func<IFromQuery, IMultiQuery<T>, IQuery<T>> subQuery, string cteTableName)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.Visitor.BuildSql(out var readerFields, false, true);
        this.Visitor.Clear(true);
        var tableSegment = this.Visitor.WithTable(typeof(T), sql, readerFields, cteTableName, this);
        this.Visitor.Clear(true);
        var fromQuery = new FromQuery(this.DbContext, this.Visitor);
        var query = subQuery.Invoke(fromQuery, this);
        sql += " UNION ALL" + Environment.NewLine + query.Visitor.BuildSql(out _, false, true);
        if (!this.Visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.Visitor);

        this.Visitor.Union(tableSegment, sql);
        return this;
    }
    #endregion

    #region CTE NextWith
    public IMultiQuery<T, TOther> NextWith<TOther>(Func<IFromQuery, IMultiQuery<T>, IQuery<TOther>> cteSubQuery, string cteTableName = null, char tableAsStart = 'a')
    {
        if (cteSubQuery == null)
            throw new ArgumentNullException(nameof(cteSubQuery));

        this.Visitor.Clear(true);
        var fromQuery = new FromQuery(this.DbContext, this.Visitor);
        var query = cteSubQuery.Invoke(fromQuery, this);
        var rawSql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.Visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.Visitor);

        this.Visitor.BuildCteTable(cteTableName, rawSql, readerFields, query, true);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region WithTable
    public IMultiQuery<T, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var query = subQuery.Invoke(new FromQuery(this.DbContext, this.Visitor));
        var sql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.Visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.Visitor);

        this.Visitor.WithTable(typeof(TOther), sql, readerFields, false, query);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var sql = subQuery.Visitor.BuildSql(out var readerFields, false);
        if (!this.Visitor.Equals(subQuery.Visitor))
            subQuery.Visitor.CopyTo(this.Visitor);

        var tableSegment = this.Visitor.WithTable(typeof(TOther), sql, readerFields, false, subQuery);
        this.Visitor.Join("INNER JOIN", tableSegment, joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var sql = subQuery.Visitor.BuildSql(out var readerFields, false);
        if (!this.Visitor.Equals(subQuery.Visitor))
            subQuery.Visitor.CopyTo(this.Visitor);

        var tableSegment = this.Visitor.WithTable(typeof(TOther), sql, readerFields, false, subQuery);
        this.Visitor.Join("LEFT JOIN", tableSegment, joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var sql = subQuery.Visitor.BuildSql(out var readerFields, false);
        if (!this.Visitor.Equals(subQuery.Visitor))
            subQuery.Visitor.CopyTo(this.Visitor);

        var tableSegment = this.Visitor.WithTable(typeof(TOther), sql, readerFields, false, subQuery);
        this.Visitor.Join("RIGHT JOIN", tableSegment, joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var fromQuery = new FromQuery(this.DbContext, this.Visitor);
        var query = subQuery.Invoke(fromQuery);
        var sql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.Visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.Visitor);

        var tableSegment = this.Visitor.WithTable(typeof(TOther), sql, readerFields, false, query);
        this.Visitor.Join("INNER JOIN", tableSegment, joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var fromQuery = new FromQuery(this.DbContext, this.Visitor);
        var query = subQuery.Invoke(fromQuery);
        var sql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.Visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.Visitor);

        var tableSegment = this.Visitor.WithTable(typeof(TOther), sql, readerFields, false, query);
        this.Visitor.Join("LEFT JOIN", tableSegment, joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var fromQuery = new FromQuery(this.DbContext, this.Visitor);
        var query = subQuery.Invoke(fromQuery);
        var sql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.Visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.Visitor);

        var tableSegment = this.Visitor.WithTable(typeof(TOther), sql, readerFields, false, query);
        this.Visitor.Join("RIGHT JOIN", tableSegment, joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector);
        return new MultiIncludableQuery<T, TMember>(this.MultipleQuery, this.Visitor);
    }
    public IMultiIncludableQuery<T, TElment> IncludeMany<TElment>(Expression<Func<T, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, true, filter);
        return new MultiIncludableQuery<T, TElment>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T> Where(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T> And(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T> And(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IMultiGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return new MultiGroupingQuery<T, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiQuery<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiQuery<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<T> Select()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.Visitor.Select(null, defaultExpr);
        return this;
    }
    //TODO:
    //public IMultiQuery<TTarget> Select<TTarget>(TTarget parameters)
    //{
    //    if (parameters == null)
    //        throw new ArgumentNullException(nameof(parameters));
    //    //TODO:
    //    this.visitor.Select(fields, null, true);
    //    var fromQuery = new FromQuery<TTarget>(this.connection, this.transaction,this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    //    //this.visitor.Select()
    //    return fromQuery;
    //}
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Distinct
    public IMultiQuery<T> Distinct()
    {
        this.Visitor.Distinct();
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public IMultiQuery<T> Skip(int offset)
    {
        this.offset = offset;
        if (this.pageSize > 0)
            this.pageIndex = (int)Math.Ceiling((double)offset / this.pageSize);
        this.Visitor.Skip(offset);
        return this;
    }
    public IMultiQuery<T> Take(int limit)
    {
        this.pageSize = limit;
        if (this.offset.HasValue)
            this.pageIndex = (int)Math.Ceiling((double)this.offset.Value / limit);
        this.Visitor.Take(limit);
        return this;
    }
    public IMultiQuery<T> Page(int pageIndex, int pageSize)
    {
        this.pageIndex = pageIndex;
        this.pageSize = pageSize;
        this.Visitor.Page(pageIndex, pageSize);
        return this;
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion

    #region First/ToList/ToPageList/ToDictionary
    public IMultipleQuery First() => this.QueryResult();
    public IMultipleQuery ToList() => this.QueryResult();
    public IMultipleQuery ToPageList() => this.QueryResult();
    public IMultipleQuery ToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector) where TKey : notnull
    {
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));

        return this.QueryResult();
    }
    #endregion

    #region QueryResult
    private IMultipleQuery QueryResult()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.Visitor.SelectDefault(defaultExpr);
        var sql = this.Visitor.BuildSql(out var readerFields);
        var targetType = typeof(T);
        Func<IDataReader, object> readerGetter = null;
        if (targetType.IsEntityType(out _))
            readerGetter = reader => reader.To<T>(this.DbContext, readerFields);
        else readerGetter = reader => reader.To<T>();
        IQueryVisitor queryVisitor = null;
        if (this.Visitor.HasIncludeTables())
            queryVisitor = this.Visitor;
        this.MultipleQuery.AddReader(sql, readerGetter, queryVisitor, this.pageIndex, this.pageSize);
        return this.MultipleQuery;
    }
    #endregion
}