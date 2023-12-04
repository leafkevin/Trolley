using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class QueryAnonymousObject : IQueryAnonymousObject
{
    #region Fields
    private readonly IQueryVisitor visitor;
    #endregion

    #region Constructor
    public QueryAnonymousObject(IQueryVisitor visitor) => this.visitor = visitor;
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = this.visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        return this.visitor.BuildSql(out _);
    }
    #endregion
}
class QueryBase : IQueryBase
{
    #region Properties
    public DbContext DbContext { get; private set; }
    public IQueryVisitor Visitor { get; private set; }
    #endregion

    #region Constructor
    public QueryBase(DbContext dbContext, IQueryVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Select	
    public IQueryAnonymousObject SelectAnonymous(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.Visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.Visitor);
    }
    public IQuery<TTarget> Select<TTarget>(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.Visitor.Select(fields, null);
        return new Query<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Count
    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        return this.Visitor.BuildSql(out _);
    }
    #endregion

    #region QueryFirstValue
    protected TTarget QueryFirstValue<TTarget>(string sqlFormat, Expression fieldExpr = null)
    {
        this.Visitor.Select(sqlFormat, fieldExpr);
        return this.DbContext.QueryFirst<TTarget>(f =>
        {
            f.CommandText = this.Visitor.BuildSql(out _);
            this.Visitor.DbParameters.CopyTo(f.Parameters);
        });
    }
    protected async Task<TTarget> QueryFirstValueAsync<TTarget>(string sqlFormat, Expression fieldExpr = null, CancellationToken cancellationToken = default)
    {
        this.Visitor.Select(sqlFormat, fieldExpr);
        return await this.DbContext.QueryFirstAsync<TTarget>(f =>
        {
            f.CommandText = this.Visitor.BuildSql(out _);
            this.Visitor.DbParameters.CopyTo(f.Parameters);
        }, cancellationToken);
    }
    #endregion
}
class Query<T> : QueryBase, IQuery<T>
{
    #region Fields
    private int? offset;
    private int pageIndex;
    private int pageSize;
    #endregion

    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Union/UnionAll
    public IQuery<T> Union(IQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.Visitor.BuildSql(out var readerFields, false, true);
        this.Visitor.Clear(true);
        var tableSegment = this.Visitor.WithTable(typeof(T), sql, readerFields, true, subQuery);
        sql += " UNION" + Environment.NewLine + subQuery.Visitor.BuildSql(out _, false, true);
        if (!this.Visitor.Equals(subQuery.Visitor))
            subQuery.Visitor.CopyTo(this.Visitor);

        this.Visitor.Union(tableSegment, sql);
        return this;
    }
    public IQuery<T> Union(Func<IFromQuery, IQuery<T>> subQuery)
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
    public IQuery<T> UnionAll(IQuery<T> subQuery)
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
    public IQuery<T> UnionAll(Func<IFromQuery, IQuery<T>> subQuery)
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
    public IQuery<T> UnionRecursive(Func<IFromQuery, IQuery<T>, IQuery<T>> subQuery, string cteTableName)
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
    public IQuery<T> UnionAllRecursive(Func<IFromQuery, IQuery<T>, IQuery<T>> subQuery, string cteTableName)
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
    public IQuery<T, TOther> NextWith<TOther>(Func<IFromQuery, IQuery<T>, IQuery<TOther>> cteSubQuery, string cteTableName = null, char tableAsStart = 'a')
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
        return new Query<T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithTable
    public IQuery<T, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var query = subQuery.Invoke(new FromQuery(this.DbContext, this.Visitor));
        var sql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.Visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.Visitor);

        this.Visitor.WithTable(typeof(TOther), sql, readerFields, false, query);
        return new Query<T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Join
    public IQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new Query<T, TOther>(this.DbContext, this.Visitor);
    }
    public IQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new Query<T, TOther>(this.DbContext, this.Visitor);
    }
    public IQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new Query<T, TOther>(this.DbContext, this.Visitor);
    }
    public IQuery<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
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
        return new Query<T, TOther>(this.DbContext, this.Visitor);
    }
    public IQuery<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
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
        return new Query<T, TOther>(this.DbContext, this.Visitor);
    }
    public IQuery<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
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
        return new Query<T, TOther>(this.DbContext, this.Visitor);
    }
    public IQuery<T, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
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
        return new Query<T, TOther>(this.DbContext, this.Visitor);
    }
    public IQuery<T, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
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
        return new Query<T, TOther>(this.DbContext, this.Visitor);
    }
    public IQuery<T, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
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
        return new Query<T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public IIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector);
        return new IncludableQuery<T, TMember>(this.DbContext, this.Visitor);
    }
    public IIncludableQuery<T, TElment> IncludeMany<TElment>(Expression<Func<T, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, true, filter);
        return new IncludableQuery<T, TElment>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where/And
    public IQuery<T> Where(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IQuery<T> And(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IQuery<T> And(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
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
    public IGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T, TGrouping>(this.DbContext, this.Visitor);
    }
    public IQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IQuery<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IQuery<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IQuery<T> Select()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.Visitor.Select(null, defaultExpr);
        return this;
    }
    //TODO:
    //public IQuery<TTarget> Select<TTarget>(TTarget parameters)
    //{
    //    if (parameters == null)
    //        throw new ArgumentNullException(nameof(parameters));
    //    //TODO:
    //    this.Visitor.Select(fields, null, true);
    //    var fromQuery = new FromQuery<TTarget>(this.connection, this.transaction,this.ormProvider, this.mapProvider, this.Visitor, this.insertType);
    //    //this.Visitor.Select()
    //    return fromQuery;
    //}
    public IQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.DbContext, this.Visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Distinct
    public IQuery<T> Distinct()
    {
        this.Visitor.Distinct();
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public IQuery<T> Skip(int offset)
    {
        this.offset = offset;
        if (this.pageSize > 0)
            this.pageIndex = (int)Math.Ceiling((double)offset / this.pageSize);
        this.Visitor.Skip(offset);
        return this;
    }
    public IQuery<T> Take(int limit)
    {
        this.pageSize = limit;
        if (this.offset.HasValue)
            this.pageIndex = (int)Math.Ceiling((double)this.offset.Value / limit);
        this.Visitor.Take(limit);
        return this;
    }
    public IQuery<T> Page(int pageIndex, int pageSize)
    {
        this.pageIndex = pageIndex;
        this.pageSize = pageSize;
        this.Visitor.Page(pageIndex, pageSize);
        return this;
    }
    #endregion

    #region Count
    public int Count<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public async Task<int> CountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    }
    public int CountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    }
    public long LongCount<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public async Task<long> LongCountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    }
    public long LongCountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    }
    #endregion

    #region Aggregate
    public TField Sum<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public async Task<TField> SumAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    }
    public TField Avg<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public async Task<TField> AvgAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public async Task<TField> MaxAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    public async Task<TField> MinAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    }
    #endregion

    #region First/ToList/ToPageList/ToDictionary
    public T First() => this.DbContext.QueryFirst<T>(this.Visitor);
    public async Task<T> FirstAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.QueryFirstAsync<T>(this.Visitor, cancellationToken);
    public List<T> ToList() => this.DbContext.Query<T>(this.Visitor);
    public async Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.QueryAsync<T>(this.Visitor, cancellationToken);
    public IPagedList<T> ToPageList() => this.DbContext.QueryPage<T>(this.Visitor, this.pageIndex, this.pageSize);
    public async Task<IPagedList<T>> ToPageListAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.QueryPageAsync<T>(this.Visitor, this.pageIndex, this.pageSize, cancellationToken);
    public Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector) where TKey : notnull
    {
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));

        return this.ToList().ToDictionary(keySelector, valueSelector);
    }
    public async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector, CancellationToken cancellationToken = default) where TKey : notnull
    {
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));

        var list = await this.ToListAsync(cancellationToken);
        return list.ToDictionary(keySelector, valueSelector);
    }
    #endregion

    #region ToSql
    public override string ToSql(out List<IDbDataParameter> dbParameters)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.Visitor.SelectDefault(defaultExpr);
        dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        return this.Visitor.BuildSql(out _);
    }
    #endregion
}