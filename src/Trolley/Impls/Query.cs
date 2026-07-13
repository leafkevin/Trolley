using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class QueryBase : QueryInternal, IQueryBase
{
    #region Constructor
    public QueryBase(DbContext dbContext, IQueryVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Count
    public virtual int Count() => this.QueryScalar<int>("COUNT(*)");
    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryScalarAsync<int>("COUNT(*)", cancellationToken);
    public virtual long LongCount() => this.QueryScalar<long>("COUNT(*)");
    public virtual async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryScalarAsync<long>("COUNT(*)", cancellationToken);
    #endregion

    #region Count/Aggregate Internal
    protected int CountInternal(Expression fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryScalar<int>("COUNT({0})", fieldExpr);
    }
    protected async Task<int> CountInternalAsync(Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryScalarAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    }
    protected int CountDistinctInternal(Expression fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryScalar<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    protected async Task<int> CountDistinctInternalAsync(Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryScalarAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    }
    protected long LongCountInternal(Expression fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryScalar<long>("COUNT({0})", fieldExpr);
    }
    protected async Task<long> LongCountInternalAsync(Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryScalarAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    }
    protected long LongCountDistinctInternal(Expression fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryScalar<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    protected async Task<long> LongCountDistinctInternalAsync(Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryScalarAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    }
    protected decimal SumInternal<TField>(Expression fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryScalar<decimal>("SUM({0})", fieldExpr);
    }
    protected async Task<decimal> SumInternalAsync<TField>(Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryScalarAsync<decimal>("SUM({0})", fieldExpr, cancellationToken);
    }
    protected TField AvgInternal<TField>(Expression fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryScalar<TField>("AVG({0})", fieldExpr);
    }
    protected async Task<TField> AvgInternalAsync<TField>(Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryScalarAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    }
    protected TField MaxInternal<TField>(Expression fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryScalar<TField>("MAX({0})", fieldExpr);
    }
    protected async Task<TField> MaxInternalAsync<TField>(Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryScalarAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    }
    protected TField MinInternal<TField>(Expression fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryScalar<TField>("MIN({0})", fieldExpr);
    }
    protected async Task<TField> MinInternalAsync<TField>(Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryScalarAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    }
    #endregion   

    #region Exists
    public virtual bool Exists()
    {
        this.Visitor.SelectRaw(typeof(int), "1");
        this.Visitor.Take(1);
        return this.DbContext.QueryExists(this.Visitor);
    }
    public virtual async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        this.Visitor.SelectRaw(typeof(int), "1");
        this.Visitor.Take(1);
        return await this.DbContext.QueryExistsAsync(this.Visitor, cancellationToken);
    }
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        var sql = this.Visitor.BuildSql(true, out var dbDataParameters);
        dbParameters = dbDataParameters.Cast<IDbDataParameter>().ToList();
        this.Visitor.Dispose();
        return sql;
    }
    #endregion

    #region QueryScalar
    protected TTarget QueryScalar<TTarget>(string aggSql)
    {
        this.Visitor.SelectRaw(typeof(TTarget), aggSql);
        return this.DbContext.QueryScalar<TTarget>(this.Visitor);
    }
    protected TTarget QueryScalar<TTarget>(string aggSqlFormat, Expression fieldExpr)
    {
        this.Visitor.Select(aggSqlFormat, fieldExpr);
        return this.DbContext.QueryScalar<TTarget>(this.Visitor);
    }
    protected async Task<TTarget> QueryScalarAsync<TTarget>(string aggSql, CancellationToken cancellationToken = default)
    {
        this.Visitor.SelectRaw(typeof(TTarget), aggSql);
        return await this.DbContext.QueryScalarAsync<TTarget>(this.Visitor, cancellationToken);
    }
    protected async Task<TTarget> QueryScalarAsync<TTarget>(string aggSqlFormat, Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        this.Visitor.Select(aggSqlFormat, fieldExpr);
        return await this.DbContext.QueryScalarAsync<TTarget>(this.Visitor, cancellationToken);
    }
    #endregion

    #region NewCreateVisitor
    public virtual ICreateVisitor NewCreateVisitor(Type entityType)
    {
        var createVisiter = this.OrmProvider.NewCreateVisitor(entityType,
            this.DbContext, this.Visitor.TableAliasStart, this.Visitor.Command);
        createVisiter.Tables = this.Visitor.Tables;
        createVisiter.RefQueries = this.Visitor.RefQueries;
        createVisiter.ShardingTables = this.Visitor.ShardingTables;
        //createVisiter.RefTableAliases = this.Visitor.RefTableAliases;
        createVisiter.IsRecursive = this.Visitor.IsRecursive;
        //createVisiter.CteQueryObj = this.Visitor.CteQueryObj;
        createVisiter.FromSql = this.Visitor.BuildCommandSql(entityType, out _);
        return createVisiter;
    }
    #endregion
}
public class Query<T> : QueryBase, IQuery<T>
{
    #region Properties
    /// <summary>
    /// 表名或是子查询表SQL，CTE表场景时，在AsCteTable方法调用前，一个临时表名
    /// </summary>
    public string Body { get; set; }
    #endregion

    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IQuery<T> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(TableShardingUsageMode.ReadOnly, false, tableNames);
        return this;
    }
    public virtual IQuery<T> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.ReadOnly, false, fieldValues);
        return this;
    }
    public virtual IQuery<T> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableShardingUsageMode.ReadOnly, false, fieldValues);
        return this;
    }
    public virtual IQuery<T> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(false, rawSql);
        return this;
    }
    #endregion

    #region GetShardingTableNames
    public virtual List<string> GetShardingTableNames(Func<string, bool> tableNameSelector) => null;
    public virtual Task<List<string>> GetShardingTableNamesAsync(Func<string, bool> tableNameSelector, CancellationToken cancellationToken = default)
        => Task.FromResult(this.GetShardingTableNames(tableNameSelector));
    #endregion

    #region Union/UnionAll
    public virtual IQuery<T> Union(IQuery<T> subQuery)
    {
        base.UnionInternal(subQuery);
        return this;
    }
    public virtual IQuery<T> Union(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        base.UnionInternal(subQueryExpr);
        return this;
    }
    public virtual IQuery<T> UnionAll(IQuery<T> subQuery)
    {
        base.UnionAllInternal(subQuery);
        return this;
    }
    public virtual IQuery<T> UnionAll(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        base.UnionAllInternal(subQueryExpr);
        return this;
    }
    public virtual IQuery<T> UnionRecursive(Expression<Func<IFromQuery, ICteQuery<T>, IQuery<T>>> subQueryExpr)
    {
        base.UnionRecursiveInternal(subQueryExpr);
        return this;
    }
    public virtual IQuery<T> UnionAllRecursive(Expression<Func<IFromQuery, ICteQuery<T>, IQuery<T>>> subQueryExpr)
    {
        base.UnionAllRecursiveInternal(subQueryExpr);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IQuery<T, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IQuery<T, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T, TMember>(this.DbContext, this.Visitor, isIncludeMany);
    }
    public virtual IIncludableQuery<T, TElement> IncludeMany<TElement>(Expression<Func<T, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T, TElement>(this.DbContext, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IQuery<T> WhereBy(object whereObj)
        => this.AndBy(whereObj);
    public virtual IQuery<T> WhereBy(bool condition, object whereObj)
        => this.AndBy(condition, whereObj);
    public virtual IQuery<T> WhereById(object whereKey)
        => this.AndById(whereKey);
    public virtual IQuery<T> WhereById(bool condition, object whereKey)
        => this.AndById(condition, whereKey);
    public virtual IQuery<T> WhereByIds(IEnumerable whereKeys)
        => this.AndByIds(whereKeys);
    public virtual IQuery<T> WhereByIds(bool condition, IEnumerable whereKeys)
        => this.AndByIds(condition, whereKeys);
    public virtual IQuery<T> Where(Expression<Func<T, bool>> predicate)
        => this.And(true, predicate);
    public virtual IQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IQuery<T> WherePredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IQuery<T> AndBy(object whereObj)
    {
        base.AndByInternal(whereObj);
        return this;
    }
    public virtual IQuery<T> AndBy(bool condition, object whereObj)
    {
        if (!condition) return this;
        base.AndByInternal(whereObj);
        return this;
    }
    public virtual IQuery<T> AndById(object whereKey)
    {
        base.AndByIdInternal(whereKey);
        return this;
    }
    public virtual IQuery<T> AndById(bool condition, object whereKey)
    {
        if (!condition) return this;
        base.AndByIdInternal(whereKey);
        return this;
    }
    public virtual IQuery<T> AndByIds(IEnumerable whereKeys)
    {
        base.AndByIdsInternal(whereKeys);
        return this;
    }
    public virtual IQuery<T> AndByIds(bool condition, IEnumerable whereKeys)
    {
        if (!condition) return this;
        base.AndByIdsInternal(whereKeys);
        return this;
    }
    public virtual IQuery<T> And(Expression<Func<T, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IQuery<T> And(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T> AndPredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IQuery<T> OrBy(object whereObj)
    {
        base.OrByInternal(whereObj);
        return this;
    }
    public virtual IQuery<T> OrBy(bool condition, object whereObj)
    {
        if (!condition) return this;
        base.OrByInternal(whereObj);
        return this;
    }
    public virtual IQuery<T> OrById(object whereKey)
    {
        base.OrByIdInternal(whereKey);
        return this;
    }
    public virtual IQuery<T> OrById(bool condition, object whereKey)
    {
        if (!condition) return this;
        base.OrByIdInternal(whereKey);
        return this;
    }
    public virtual IQuery<T> OrByIds(IEnumerable whereKeys)
    {
        base.OrByIdsInternal(whereKeys);
        return this;
    }
    public virtual IQuery<T> OrByIds(bool condition, IEnumerable whereKeys)
    {
        if (!condition) return this;
        base.OrByIdsInternal(whereKeys);
        return this;
    }
    public virtual IQuery<T> Or(Expression<Func<T, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IQuery<T> Or(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T> OrPredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T> OrderByDynamic(Func<OrderByBuilder<T>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IQuery<T> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IQuery<T> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IQuery<T> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IQuery<T> Select()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.Visitor.SelectDefault(defaultExpr);
        return this;
    }
    public virtual IQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Distinct
    public virtual IQuery<T> Distinct()
    {
        this.Visitor.Distinct();
        return this;
    }
    #endregion

    #region WithRawSql
    public virtual IQuery<T> WithLeadingSql(string rawSql)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        this.Visitor.WithLeadingSql(rawSql);
        return this;
    }
    public virtual IQuery<T> WithTrailingSql(string rawSql)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        this.Visitor.WithTrailingSql(rawSql);
        return this;
    }
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountInternalAsync(fieldExpr, cancellationToken);
    public virtual int CountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountDistinctInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCount<TField>(Expression<Func<T, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountDistinctInternalAsync(fieldExpr);
    #endregion

    #region Aggregate
    public virtual decimal Sum<TField>(Expression<Func<T, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual async Task<decimal> SumAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.SumInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Avg<TField>(Expression<Func<T, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.AvgInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Max<TField>(Expression<Func<T, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MaxInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Min<TField>(Expression<Func<T, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MinInternalAsync<TField>(fieldExpr, cancellationToken);
    #endregion

    #region First/ToList/ToPageList/ToDictionary
    public virtual T First()
    {
        return this.DbContext.QueryFrom<T, T>(this.Visitor, false, (entityType, reader, readerFields) =>
        {
            T result = default;
            var deserializer = reader.GetReaderDeserializer(typeof(T), this.DbContext, readerFields);
            if (reader.Read())
                result = (T)deserializer.Invoke(reader, readerFields);
            return result;
        });
    }
    public virtual async Task<T> FirstAsync(CancellationToken cancellationToken = default)
    {
        return await this.DbContext.QueryFromAsync<T, T>(this.Visitor, false, async (entityType, reader, readerFields, cancellationToken) =>
        {
            T result = default;
            var deserializer = reader.GetReaderDeserializer(typeof(T), this.DbContext, readerFields);
            if (await reader.ReadAsync(cancellationToken))
                result = (T)deserializer.Invoke(reader, readerFields);
            return result;
        }, cancellationToken);
    }
    public virtual List<T> ToList()
    {
        return this.DbContext.QueryFrom<T, List<T>>(this.Visitor, true, (entityType, reader, readerFields) =>
        {
            var result = new List<T>();
            var deserializer = reader.GetReaderDeserializer(typeof(T), this.DbContext, readerFields);
            while (reader.Read())
                result.Add((T)deserializer.Invoke(reader, readerFields));
            return result;
        });
    }
    public virtual async Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
    {
        return await this.DbContext.QueryFromAsync<T, List<T>>(this.Visitor, true, async (entityType, reader, readerFields, cancellationToken) =>
        {
            var result = new List<T>();
            var deserializer = reader.GetReaderDeserializer(typeof(T), this.DbContext, readerFields);
            while (await reader.ReadAsync(cancellationToken))
                result.Add((T)deserializer.Invoke(reader, readerFields));
            return result;
        }, cancellationToken);
    }
    public virtual IPagedList<T> ToPageList() => this.DbContext.QueryPage<T>(this.Visitor);
    public virtual async Task<IPagedList<T>> ToPageListAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.QueryPageAsync<T>(this.Visitor, cancellationToken);
    public virtual Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector) where TKey : notnull
    {
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));

        return this.ToList().ToDictionary(keySelector, valueSelector);
    }
    public virtual async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector, CancellationToken cancellationToken = default) where TKey : notnull
    {
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));

        var list = await this.ToListAsync(cancellationToken);
        return list.ToDictionary(keySelector, valueSelector);
    }
    #endregion

    #region ToCreate
    public virtual IFromCreate<TEntity> ToCreate<TEntity>()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.Visitor.SelectDefault(defaultExpr);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    /// <summary>
    /// 生成插入数据的查询构造器，fieldsSelector参数指定要插入的字段，未指定的字段将使用默认值，fieldsSelector表达式支持匿名对象和实体对象两种形式，例如：
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="fieldsSelector"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public virtual IFromCreate<TEntity> ToCreate<TEntity>(Expression<Func<T, object>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));

        this.SelectInternal(fieldsSelector);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    #endregion

    #region AsCteTable
    public virtual ICteQuery<T> AsCteTable(string tableName)
    {
        //TODO: 清除Command对象，参数列表单独整理出来，方便后面引用
        if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
            throw new NotSupportedException("CTE暂时不支持多分表，只支持单个分表");

        this.Visitor.IsCteTable = true;
        if (this.Visitor.CteQueryObj != null && this.Visitor.IsRecursive && !string.IsNullOrEmpty(this.Visitor.UnionSql))
        {
            var tempTableName = this.Visitor.CteQueryObj.TableName;
            this.Visitor.UnionSql = this.Visitor.UnionSql.Replace(tempTableName, tableName);
        }
        this.Visitor.CteQueryObj ??= new CteQuery<T>(this.DbContext, this.Visitor);
        this.Visitor.CteQueryObj.Body = this.Visitor.BuildCteTableSql(tableName, out var readerFields);
        this.Visitor.CteQueryObj.ReaderFields = readerFields;
        this.Visitor.CteQueryObj.TableName = tableName;
        return this.Visitor.CteQueryObj as ICteQuery<T>;
    }
    #endregion    

    #region ToSql
    public override string ToSql(out List<IDbDataParameter> dbParameters)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.Visitor.SelectDefault(defaultExpr);
        (var sql, var readerFields) = this.DbContext.BuildSql(this.Visitor);
        dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        this.Visitor.Dispose();
        return sql;
    }
    #endregion
}
public class CteQuery<T> : Query<T>, ICteQuery<T>
{
    #region Properties
    public string TableName { get; set; }
    public List<ReaderField> ReaderFields { get; set; }
    public override bool IsCteTable => true;
    public bool IsRecursive { get; set; }
    #endregion

    #region Constructor
    public CteQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region 不支持的方法
    public override IIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持IncludeMany操作");
    public override IIncludableQuery<T, TElement> IncludeMany<TElement>(Expression<Func<T, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持IncludeMany操作");
    public override int Count() => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override Task<int> CountAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override int Count<TField>(Expression<Func<T, TField>> fieldExpr)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override Task<int> CountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override int CountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override Task<int> CountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override long LongCount() => throw new NotSupportedException("不支持的方法调用");
    public override Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用");
    public override long LongCount<TField>(Expression<Func<T, TField>> fieldExpr)
        => throw new NotSupportedException("不支持的方法调用");
    public override Task<long> LongCountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用");
    public override long LongCountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
        => throw new NotSupportedException("不支持的方法调用");
    public override Task<long> LongCountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default) => throw new NotSupportedException("不支持的方法调用");
    public override decimal Sum<TField>(Expression<Func<T, TField>> fieldExpr)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override Task<decimal> SumAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override TField Avg<TField>(Expression<Func<T, TField>> fieldExpr)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override Task<TField> AvgAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override TField Max<TField>(Expression<Func<T, TField>> fieldExpr)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override Task<TField> MaxAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override TField Min<TField>(Expression<Func<T, TField>> fieldExpr)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override Task<TField> MinAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override T First() => throw new NotSupportedException("不支持的方法调用");
    public override Task<T> FirstAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用");
    public override List<T> ToList() => throw new NotSupportedException("不支持的方法调用");
    public override Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override IPagedList<T> ToPageList() => throw new NotSupportedException("不支持的方法调用");
    public override Task<IPagedList<T>> ToPageListAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    public override Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    #endregion
}