using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
        dbParameters = this.visitor.DbParameters;
        return this.visitor.BuildSql(out _);
    }
    #endregion
}
class QueryBase : IQueryBase
{
    #region Fields
    protected string dbKey;
    protected TheaConnection connection;
    protected IDbTransaction transaction;
    protected IOrmProvider ormProvider;
    protected IEntityMapProvider mapProvider;
    protected IQueryVisitor visitor;
    #endregion

    #region Properties
    public IQueryVisitor Visitor => this.visitor;
    public bool IsCteQuery { get; set; }
    #endregion

    #region Constructor
    public QueryBase(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.visitor = visitor;
        this.dbKey = connection.DbKey;
        this.mapProvider = mapProvider;
    }
    #endregion

    #region Select	
    public IQueryAnonymousObject SelectAnonymous(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IQuery<TTarget> Select<TTarget>(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null);
        return new Query<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
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
        dbParameters = this.visitor.DbParameters;
        return this.visitor.BuildSql(out _);
    }
    #endregion

    #region QueryFirstValue
    protected TTarget QueryFirstValue<TTarget>(string sqlFormat, Expression fieldExpr = null)
    {
        this.visitor.Select(sqlFormat, fieldExpr);
        using var command = this.connection.CreateCommand();
        command.CommandText = this.visitor.BuildSql(out _);
        command.CommandType = CommandType.Text;
        if (this.visitor.DbParameters != null && this.visitor.DbParameters.Count > 0)
            this.visitor.DbParameters.ForEach(f => command.Parameters.Add(f));

        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        TTarget result = default;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read()) result = reader.To<TTarget>();
        reader.Close();
        reader.Dispose();
        command.Dispose();
        return result;
    }
    protected async Task<TTarget> QueryFirstValueAsync<TTarget>(string sqlFormat, Expression fieldExpr = null, CancellationToken cancellationToken = default)
    {
        this.visitor.Select(sqlFormat, fieldExpr);
        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = this.visitor.BuildSql(out _);
        cmd.CommandType = CommandType.Text;
        if (this.visitor.DbParameters != null && this.visitor.DbParameters.Count > 0)
            this.visitor.DbParameters.ForEach(f => cmd.Parameters.Add(f));

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        TTarget result = default;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await this.connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
            result = reader.To<TTarget>();
        await reader.CloseAsync();
        await reader.DisposeAsync();
        await command.DisposeAsync();
        return result;
    }
    #endregion
}
class Query<T> : QueryBase, IQuery<T>
{
    #region Fields
    private int? offset;
    private int pageIndex;
    private int pageSize;
    protected Type insertType;
    #endregion

    #region Constructor
    public Query(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor)
        : base(connection, transaction, ormProvider, mapProvider, visitor) { }
    #endregion

    #region Union/UnionAll
    public IQuery<T> Union(IQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.visitor.BuildSql(out var readerFields, false, true);
        this.visitor.Clear(true);
        var tableSegment = this.visitor.WithTable(typeof(T), sql, readerFields, true, subQuery);
        sql += " UNION" + Environment.NewLine + subQuery.Visitor.BuildSql(out _, false, true);
        if (!this.visitor.Equals(subQuery.Visitor))
            subQuery.Visitor.CopyTo(this.visitor);

        this.visitor.Union(tableSegment, sql);
        return this;
    }
    public IQuery<T> Union(Func<IFromQuery, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.visitor.BuildSql(out var readerFields, false, true);
        this.visitor.Clear(true);
        var tableSegment = this.visitor.WithTable(typeof(T), sql, readerFields, true);
        var query = subQuery.Invoke(new FromQuery(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor));
        sql += " UNION" + Environment.NewLine + query.Visitor.BuildSql(out _, false, true);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        this.visitor.Union(tableSegment, sql);
        return this;
    }
    public IQuery<T> UnionAll(IQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.visitor.BuildSql(out var readerFields, false, true);
        this.visitor.Clear(true);
        var tableSegment = this.visitor.WithTable(typeof(T), sql, readerFields, true);
        sql += " UNION ALL" + Environment.NewLine + subQuery.Visitor.BuildSql(out _, false, true);
        if (!this.visitor.Equals(subQuery.Visitor))
            subQuery.Visitor.CopyTo(this.visitor);

        this.visitor.Union(tableSegment, sql);
        return this;
    }
    public IQuery<T> UnionAll(Func<IFromQuery, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.visitor.BuildSql(out var readerFields, false, true);
        this.visitor.Clear(true);
        var tableSegment = this.visitor.WithTable(typeof(T), sql, readerFields, true);
        var query = subQuery.Invoke(new FromQuery(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor));
        sql += " UNION ALL" + Environment.NewLine + query.Visitor.BuildSql(out _, false, true);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        this.visitor.Union(tableSegment, sql);
        return this;
    }
    public IQuery<T> UnionRecursive(Func<IFromQuery, IQuery<T>, IQuery<T>> subQuery, string cteTableName)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.visitor.BuildSql(out var readerFields, false, true);
        this.visitor.Clear(true);
        var tableSegment = this.visitor.WithTable(typeof(T), sql, readerFields, cteTableName, this);
        var query = subQuery.Invoke(new FromQuery(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor), this);
        sql += " UNION" + Environment.NewLine + query.Visitor.BuildSql(out _, false, true);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        this.visitor.Union(tableSegment, sql);
        return this;
    }
    public IQuery<T> UnionAllRecursive(Func<IFromQuery, IQuery<T>, IQuery<T>> subQuery, string cteTableName)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.visitor.BuildSql(out var readerFields, false, true);
        this.visitor.Clear(true);
        var tableSegment = this.visitor.WithTable(typeof(T), sql, readerFields, cteTableName, this);
        this.visitor.Clear(true);
        var query = subQuery.Invoke(new FromQuery(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor), this);
        sql += " UNION ALL" + Environment.NewLine + query.Visitor.BuildSql(out _, false, true);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        this.visitor.Union(tableSegment, sql);
        return this;
    }
    #endregion

    #region CTE NextWith
    public IQuery<T, TOther> NextWith<TOther>(Func<IFromQuery, IQuery<T>, IQuery<TOther>> cteSubQuery, string cteTableName = null, char tableAsStart = 'a')
    {
        if (cteSubQuery == null)
            throw new ArgumentNullException(nameof(cteSubQuery));

        this.visitor.Clear(true);
        var query = cteSubQuery.Invoke(new FromQuery(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor), this);
        var rawSql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        this.visitor.BuildCteTable(cteTableName, rawSql, readerFields, query, true);
        return new Query<T, TOther>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    #endregion

    #region WithTable
    public IQuery<T, TOther> WithTable<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = subQuery.Visitor.BuildSql(out var readerFields, false);
        if (!this.visitor.Equals(subQuery.Visitor))
            subQuery.Visitor.CopyTo(this.visitor);

        this.visitor.WithTable(typeof(TOther), sql, readerFields, true, subQuery);
        return new Query<T, TOther>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var query = subQuery.Invoke(new FromQuery(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor));
        var sql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        this.visitor.WithTable(typeof(TOther), sql, readerFields, false, query);
        return new Query<T, TOther>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new Query<T, TOther>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new Query<T, TOther>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new Query<T, TOther>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var sql = subQuery.Visitor.BuildSql(out var readerFields, false);
        if (!this.visitor.Equals(subQuery.Visitor))
            subQuery.Visitor.CopyTo(this.visitor);

        var tableSegment = this.visitor.WithTable(typeof(TOther), sql, readerFields, false, subQuery);
        this.visitor.Join("INNER JOIN", tableSegment, joinOn);
        return new Query<T, TOther>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var sql = subQuery.Visitor.BuildSql(out var readerFields, false);
        if (!this.visitor.Equals(subQuery.Visitor))
            subQuery.Visitor.CopyTo(this.visitor);

        var tableSegment = this.visitor.WithTable(typeof(TOther), sql, readerFields, false, subQuery);
        this.visitor.Join("LEFT JOIN", tableSegment, joinOn);
        return new Query<T, TOther>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var sql = subQuery.Visitor.BuildSql(out var readerFields, false);
        if (!this.visitor.Equals(subQuery.Visitor))
            subQuery.Visitor.CopyTo(this.visitor);

        var tableSegment = this.visitor.WithTable(typeof(TOther), sql, readerFields, false, subQuery);
        this.visitor.Join("RIGHT JOIN", tableSegment, joinOn);
        return new Query<T, TOther>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var query = subQuery.Invoke(new FromQuery(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor));
        var sql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        var tableSegment = this.visitor.WithTable(typeof(TOther), sql, readerFields, false, query);
        this.visitor.Join("INNER JOIN", tableSegment, joinOn);
        return new Query<T, TOther>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var query = subQuery.Invoke(new FromQuery(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor));
        var sql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        var tableSegment = this.visitor.WithTable(typeof(TOther), sql, readerFields, false, query);
        this.visitor.Join("LEFT JOIN", tableSegment, joinOn);
        return new Query<T, TOther>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var query = subQuery.Invoke(new FromQuery(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor));
        var sql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        var tableSegment = this.visitor.WithTable(typeof(TOther), sql, readerFields, false, query);
        this.visitor.Join("RIGHT JOIN", tableSegment, joinOn);
        return new Query<T, TOther>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    #endregion

    #region Include
    public IIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.visitor.Include(memberSelector);
        return new IncludableQuery<T, TMember>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IIncludableQuery<T, TElment> IncludeMany<TElment>(Expression<Func<T, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.visitor.Include(memberSelector, true, filter);
        return new IncludableQuery<T, TElment>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    #endregion

    #region Where/And
    public IQuery<T> Where(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IQuery<T> And(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IQuery<T> And(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
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
    public IGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T, TGrouping>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IQuery<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IQuery<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IQuery<T> Select()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return this;
    }
    //TODO:
    //public IQuery<TTarget> Select<TTarget>(TTarget parameters)
    //{
    //    if (parameters == null)
    //        throw new ArgumentNullException(nameof(parameters));
    //    //TODO:
    //    this.visitor.Select(fields, null, true);
    //    var fromQuery = new FromQuery<TTarget>(this.connection, this.transaction,this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    //    //this.visitor.Select()
    //    return fromQuery;
    //}
    public IQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    #endregion

    #region Distinct
    public IQuery<T> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public IQuery<T> Skip(int offset)
    {
        this.offset = offset;
        if (this.pageSize > 0)
            this.pageIndex = (int)Math.Ceiling((double)offset / this.pageSize);
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T> Take(int limit)
    {
        this.pageSize = limit;
        if (this.offset.HasValue)
            this.pageIndex = (int)Math.Ceiling((double)this.offset.Value / limit);
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T> Page(int pageIndex, int pageSize)
    {
        this.pageIndex = pageIndex;
        this.pageSize = pageSize;
        this.visitor.Page(pageIndex, pageSize);
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
    public T First()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.SelectDefault(defaultExpr);
        using var command = this.connection.CreateCommand();
        command.CommandText = this.visitor.BuildSql(out var readerFields);
        command.CommandType = CommandType.Text;
        if (this.visitor.DbParameters != null && this.visitor.DbParameters.Count > 0)
            this.visitor.DbParameters.ForEach(f => command.Parameters.Add(f));

        T result = default;
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read())
        {
            var entityType = typeof(T);
            if (entityType.IsEntityType(out _))
                result = reader.To<T>(this.connection.DbKey, this.ormProvider, readerFields);
            else result = reader.To<T>();
        }
        reader.Close();
        reader.Dispose();

        if (this.visitor.BuildIncludeSql(result, out var sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = command.ExecuteReader(CommandBehavior.SequentialAccess);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        command.Dispose();
        return result;
    }
    public async Task<T> FirstAsync(CancellationToken cancellationToken = default)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.SelectDefault(defaultExpr);

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = this.visitor.BuildSql(out var readerFields);
        cmd.CommandType = CommandType.Text;
        if (this.visitor.DbParameters != null && this.visitor.DbParameters.Count > 0)
            this.visitor.DbParameters.ForEach(f => cmd.Parameters.Add(f));

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        T result = default;
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            var entityType = typeof(T);
            if (entityType.IsEntityType(out _))
                result = reader.To<T>(this.dbKey, this.ormProvider, readerFields);
            else result = reader.To<T>();
        }
        await reader.CloseAsync();
        await reader.DisposeAsync();

        if (this.visitor.BuildIncludeSql(result, out var sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        await command.DisposeAsync();
        return result;
    }
    public List<T> ToList()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.SelectDefault(defaultExpr);

        using var command = this.connection.CreateCommand();
        command.CommandText = this.visitor.BuildSql(out var readerFields);
        command.CommandType = CommandType.Text;
        if (this.visitor.DbParameters != null && this.visitor.DbParameters.Count > 0)
            this.visitor.DbParameters.ForEach(f => command.Parameters.Add(f));

        var result = new List<T>();
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(behavior);
        var entityType = typeof(T);
        if (entityType.IsEntityType(out _))
        {
            while (reader.Read())
            {
                result.Add(reader.To<T>(this.dbKey, this.ormProvider, readerFields));
            }
        }
        else
        {
            while (reader.Read())
            {
                result.Add(reader.To<T>());
            }
        }
        reader.Close();
        reader.Dispose();

        if (this.visitor.BuildIncludeSql(result, out var sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = command.ExecuteReader(behavior);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        command.Dispose();
        return result;
    }
    public async Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.SelectDefault(defaultExpr);

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = this.visitor.BuildSql(out var readerFields);
        cmd.CommandType = CommandType.Text;
        if (this.visitor.DbParameters != null && this.visitor.DbParameters.Count > 0)
            this.visitor.DbParameters.ForEach(f => cmd.Parameters.Add(f));

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        var result = new List<T>();
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);

        var entityType = typeof(T);
        if (entityType.IsEntityType(out _))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<T>(this.connection.DbKey, this.ormProvider, readerFields));
            }
        }
        else
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<T>());
            }
        }
        await reader.CloseAsync();
        await reader.DisposeAsync();

        if (this.visitor.BuildIncludeSql(result, out var sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        await command.DisposeAsync();
        return result;
    }
    public IPagedList<T> ToPageList()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.SelectDefault(defaultExpr);

        using var command = this.connection.CreateCommand();
        command.CommandText = this.visitor.BuildSql(out var readerFields);
        command.CommandType = CommandType.Text;
        if (this.visitor.DbParameters != null && this.visitor.DbParameters.Count > 0)
            this.visitor.DbParameters.ForEach(f => command.Parameters.Add(f));

        var result = new PagedList<T> { Data = new List<T>() };
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read())
            result.TotalCount = reader.To<int>();
        result.PageIndex = this.pageIndex;
        result.PageSize = this.pageSize;

        reader.NextResult();
        var entityType = typeof(T);
        if (entityType.IsEntityType(out _))
        {
            while (reader.Read())
            {
                result.Data.Add(reader.To<T>(this.dbKey, this.ormProvider, readerFields));
            }
        }
        else
        {
            while (reader.Read())
            {
                result.Data.Add(reader.To<T>());
            }
        }
        reader.Close();
        reader.Dispose();

        if (this.visitor.BuildIncludeSql(result, out var sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = command.ExecuteReader(behavior);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        command.Dispose();
        return result;
    }
    public async Task<IPagedList<T>> ToPageListAsync(CancellationToken cancellationToken = default)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.SelectDefault(defaultExpr);

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = this.visitor.BuildSql(out var readerFields);
        cmd.CommandType = CommandType.Text;
        if (this.visitor.DbParameters != null && this.visitor.DbParameters.Count > 0)
            this.visitor.DbParameters.ForEach(f => cmd.Parameters.Add(f));

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        var result = new PagedList<T> { Data = new List<T>() };
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (await reader.ReadAsync())
            result.TotalCount = reader.To<int>();
        result.PageIndex = this.pageIndex;
        result.PageSize = this.pageSize;

        await reader.NextResultAsync(cancellationToken);
        var entityType = typeof(T);
        if (entityType.IsEntityType(out _))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Data.Add(reader.To<T>(this.dbKey, this.ormProvider, readerFields));
            }
        }
        else
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Data.Add(reader.To<T>());
            }
        }
        await reader.CloseAsync();
        await reader.DisposeAsync();

        if (this.visitor.BuildIncludeSql(result.Data, out var sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            this.visitor.SetIncludeValues(result.Data, includeReader);
        }
        await command.DisposeAsync();
        return result;
    }
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
        this.visitor.SelectDefault(defaultExpr);
        dbParameters = this.visitor.DbParameters;
        return this.visitor.BuildSql(out _);
    }
    #endregion
}