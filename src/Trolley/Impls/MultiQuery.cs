using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Trolley;

class MultiQueryBase : IMultiQueryBase
{
    #region Fields
    protected readonly string dbKey;
    protected readonly MultipleQuery multiQuery;
    protected readonly TheaConnection connection;
    protected readonly IDbTransaction transaction;
    protected readonly IOrmProvider ormProvider;
    protected readonly IEntityMapProvider mapProvider;
    protected readonly IQueryVisitor visitor;
    #endregion

    #region QueryVisitor
    /// <summary>
    /// Visitor对象
    /// </summary>
    public IQueryVisitor Visitor => visitor;
    #endregion

    #region Constructor
    public MultiQueryBase(MultipleQuery multiQuery, IQueryVisitor visitor)
    {
        this.multiQuery = multiQuery;
        this.dbKey = multiQuery.DbKey;
        this.connection = multiQuery.Connection as TheaConnection;
        this.transaction = multiQuery.Transaction;
        this.ormProvider = multiQuery.OrmProvider;
        this.mapProvider = multiQuery.MapProvider;
        this.visitor = visitor;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null);
        return new MultiQuery<TTarget>(this.multiQuery, this.visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count() => this.QueryFirstValue<int>("COUNT(1)");
    public IMultipleQuery LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    #endregion    

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = this.visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        return this.visitor.BuildSql(out _);
    }
    #endregion

    #region QueryFirstValue
    protected IMultipleQuery QueryFirstValue<TTarget>(string sqlFormat, Expression fieldExpr = null)
    {
        this.visitor.Select(sqlFormat, fieldExpr);
        var sql = this.visitor.BuildSql(out _);
        Func<IDataReader, object> readerGetter = reader => reader.To<TTarget>();
        this.multiQuery.AddReader(sql, readerGetter);
        return this.multiQuery;
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

        var sql = this.visitor.BuildSql(out var readerFields, false, true);
        this.visitor.Clear(true);
        var tableSegment = this.visitor.WithTable(typeof(T), sql, readerFields, true, subQuery);
        sql += " UNION" + Environment.NewLine + subQuery.Visitor.BuildSql(out _, false, true);
        if (!this.visitor.Equals(subQuery.Visitor))
            subQuery.Visitor.CopyTo(this.visitor);

        this.visitor.Union(tableSegment, sql);
        return this;
    }
    public IMultiQuery<T> Union(Func<IFromQuery, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.visitor.BuildSql(out var readerFields, false, true);
        this.visitor.Clear(true);
        var tableSegment = this.visitor.WithTable(typeof(T), sql, readerFields, true);
        var fromQuery = new FromQuery(this.multiQuery, this.visitor);
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION" + Environment.NewLine + query.Visitor.BuildSql(out _, false, true);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        this.visitor.Union(tableSegment, sql);
        return this;
    }
    public IMultiQuery<T> UnionAll(IMultiQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.visitor.BuildSql(out var readerFields, false, true);
        this.visitor.Clear(true);
        var tableSegment = this.visitor.WithTable(typeof(T), sql, readerFields, true, subQuery);
        sql += " UNION ALL" + Environment.NewLine + subQuery.Visitor.BuildSql(out _, false, true);
        if (!this.visitor.Equals(subQuery.Visitor))
            subQuery.Visitor.CopyTo(this.visitor);

        this.visitor.Union(tableSegment, sql);
        return this;
    }
    public IMultiQuery<T> UnionAll(Func<IFromQuery, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.visitor.BuildSql(out var readerFields, false, true);
        this.visitor.Clear(true);
        var tableSegment = this.visitor.WithTable(typeof(T), sql, readerFields, true);
        var fromQuery = new FromQuery(this.multiQuery, this.visitor);
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL" + Environment.NewLine + query.Visitor.BuildSql(out _, false, true);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        this.visitor.Union(tableSegment, sql);
        return this;
    }
    public IMultiQuery<T> UnionRecursive(Func<IFromQuery, IMultiQuery<T>, IQuery<T>> subQuery, string cteTableName)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.visitor.BuildSql(out var readerFields, false, true);
        this.visitor.Clear(true);
        var tableSegment = this.visitor.WithTable(typeof(T), sql, readerFields, cteTableName, this);
        var fromQuery = new FromQuery(this.multiQuery, this.visitor);
        var query = subQuery.Invoke(fromQuery, this);
        sql += " UNION" + Environment.NewLine + query.Visitor.BuildSql(out _, false, true);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        this.visitor.Union(tableSegment, sql);
        return this;
    }
    public IMultiQuery<T> UnionAllRecursive(Func<IFromQuery, IMultiQuery<T>, IQuery<T>> subQuery, string cteTableName)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.visitor.BuildSql(out var readerFields, false, true);
        this.visitor.Clear(true);
        var tableSegment = this.visitor.WithTable(typeof(T), sql, readerFields, cteTableName, this);
        this.visitor.Clear(true);
        var fromQuery = new FromQuery(this.multiQuery, this.visitor);
        var query = subQuery.Invoke(fromQuery, this);
        sql += " UNION ALL" + Environment.NewLine + query.Visitor.BuildSql(out _, false, true);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        this.visitor.Union(tableSegment, sql);
        return this;
    }
    #endregion

    #region CTE NextWith
    public IMultiQuery<T, TOther> NextWith<TOther>(Func<IFromQuery, IMultiQuery<T>, IQuery<TOther>> cteSubQuery, string cteTableName = null, char tableAsStart = 'a')
    {
        if (cteSubQuery == null)
            throw new ArgumentNullException(nameof(cteSubQuery));

        this.visitor.Clear(true);
        var fromQuery = new FromQuery(this.multiQuery, this.visitor);
        var query = cteSubQuery.Invoke(fromQuery, this);
        var rawSql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        this.visitor.BuildCteTable(cteTableName, rawSql, readerFields, query, true);
        return new MultiQuery<T, TOther>(this.multiQuery, this.visitor);
    }
    #endregion

    #region WithTable
    public IMultiQuery<T, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var query = subQuery.Invoke(new FromQuery(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor));
        var sql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        this.visitor.WithTable(typeof(TOther), sql, readerFields, false, query);
        return new MultiQuery<T, TOther>(this.multiQuery, this.visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new MultiQuery<T, TOther>(this.multiQuery, this.visitor);
    }
    public IMultiQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new MultiQuery<T, TOther>(this.multiQuery, this.visitor);
    }
    public IMultiQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new MultiQuery<T, TOther>(this.multiQuery, this.visitor);
    }
    public IMultiQuery<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
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
        return new MultiQuery<T, TOther>(this.multiQuery, this.visitor);
    }
    public IMultiQuery<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
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
        return new MultiQuery<T, TOther>(this.multiQuery, this.visitor);
    }
    public IMultiQuery<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
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
        return new MultiQuery<T, TOther>(this.multiQuery, this.visitor);
    }
    public IMultiQuery<T, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var fromQuery = new FromQuery(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
        var query = subQuery.Invoke(fromQuery);
        var sql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        var tableSegment = this.visitor.WithTable(typeof(TOther), sql, readerFields, false, query);
        this.visitor.Join("INNER JOIN", tableSegment, joinOn);
        return new MultiQuery<T, TOther>(this.multiQuery, this.visitor);
    }
    public IMultiQuery<T, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var fromQuery = new FromQuery(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
        var query = subQuery.Invoke(fromQuery);
        var sql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        var tableSegment = this.visitor.WithTable(typeof(TOther), sql, readerFields, false, query);
        this.visitor.Join("LEFT JOIN", tableSegment, joinOn);
        return new MultiQuery<T, TOther>(this.multiQuery, this.visitor);
    }
    public IMultiQuery<T, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var fromQuery = new FromQuery(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
        var query = subQuery.Invoke(fromQuery);
        var sql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.visitor.Equals(query.Visitor))
            query.Visitor.CopyTo(this.visitor);

        var tableSegment = this.visitor.WithTable(typeof(TOther), sql, readerFields, false, query);
        this.visitor.Join("RIGHT JOIN", tableSegment, joinOn);
        return new MultiQuery<T, TOther>(this.multiQuery, this.visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.visitor.Include(memberSelector);
        return new MultiIncludableQuery<T, TMember>(this.multiQuery, this.visitor);
    }
    public IMultiIncludableQuery<T, TElment> IncludeMany<TElment>(Expression<Func<T, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.visitor.Include(memberSelector, true, filter);
        return new MultiIncludableQuery<T, TElment>(this.multiQuery, this.visitor);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T> Where(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T> And(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T> And(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
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
    public IMultiGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new MultiGroupingQuery<T, TGrouping>(this.multiQuery, this.visitor);
    }
    public IMultiQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiQuery<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiQuery<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<T> Select()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
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

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.multiQuery, this.visitor);
    }
    #endregion

    #region Distinct
    public IMultiQuery<T> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public IMultiQuery<T> Skip(int offset)
    {
        this.offset = offset;
        if (this.pageSize > 0)
            this.pageIndex = (int)Math.Ceiling((double)offset / this.pageSize);
        this.visitor.Skip(offset);
        return this;
    }
    public IMultiQuery<T> Take(int limit)
    {
        this.pageSize = limit;
        if (this.offset.HasValue)
            this.pageIndex = (int)Math.Ceiling((double)this.offset.Value / limit);
        this.visitor.Take(limit);
        return this;
    }
    public IMultiQuery<T> Page(int pageIndex, int pageSize)
    {
        this.pageIndex = pageIndex;
        this.pageSize = pageSize;
        this.visitor.Page(pageIndex, pageSize);
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
        this.visitor.SelectDefault(defaultExpr);
        var sql = this.visitor.BuildSql(out var readerFields);
        var targetType = typeof(T);
        Func<IDataReader, object> readerGetter = null;
        if (targetType.IsEntityType(out _))
            readerGetter = reader => reader.To<T>(this.dbKey, this.ormProvider, readerFields);
        else readerGetter = reader => reader.To<T>();
        IQueryVisitor queryVisitor = null;
        if (this.visitor.HasIncludeTables())
            queryVisitor = this.visitor;
        this.multiQuery.AddReader(sql, readerGetter, queryVisitor, this.pageIndex, this.pageSize);
        return this.multiQuery;
    }
    #endregion
}