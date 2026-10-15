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
    public virtual int Count() => this.QueryScalar<int>("COUNT(*)", "SUM");
    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryScalarAsync<int>("COUNT(*)", "SUM", cancellationToken);
    public virtual long LongCount() => this.QueryScalar<long>("COUNT(*)", "SUM");
    public virtual async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryScalarAsync<long>("COUNT(*)", "SUM", cancellationToken);
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
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        this.Visitor.IsScalar = true;
        command.CommandText = this.Visitor.BuildSql(out _);
        return this.Exists(isNeedClose, connection, command);
    }
    public virtual async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        this.Visitor.SelectRaw(typeof(int), "1");
        this.Visitor.Take(1);
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        this.Visitor.IsScalar = true;
        command.CommandText = this.Visitor.BuildSql(out _);
        return await this.ExistsAsync(isNeedClose, connection, command, cancellationToken);
    }
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        (_, var connection, var command) = this.Visitor.UseCommand();
        var sql = this.Visitor.BuildSql(out _);
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        connection.Dispose();
        this.Visitor.Dispose();
        this.Visitor = null;
        return sql;
    }
    #endregion

    #region QueryScalar
    protected TTarget QueryScalar<TTarget>(string aggSql, string aggFunc)
    {
        this.Visitor.SelectRaw(typeof(TTarget), aggSql, aggFunc);
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        this.Visitor.IsScalar = true;
        command.CommandText = this.Visitor.BuildSql(out _);
        return this.QueryScalar<TTarget>(isNeedClose, connection, command);
    }
    protected async Task<TTarget> QueryScalarAsync<TTarget>(string aggSql, string aggFunc, CancellationToken cancellationToken = default)
    {
        this.Visitor.SelectRaw(typeof(TTarget), aggSql, aggFunc);
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        this.Visitor.IsScalar = true;
        command.CommandText = this.Visitor.BuildSql(out _);
        return await this.QueryScalarAsync<TTarget>(isNeedClose, connection, command, cancellationToken);
    }
    protected TTarget QueryScalar<TTarget>(string aggSqlFormat, Expression fieldExpr)
    {
        this.Visitor.Select(aggSqlFormat, fieldExpr);
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        this.Visitor.IsScalar = true;
        command.CommandText = this.Visitor.BuildSql(out _);
        return this.QueryScalar<TTarget>(isNeedClose, connection, command);
    }
    protected async Task<TTarget> QueryScalarAsync<TTarget>(string aggSqlFormat, Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        this.Visitor.Select(aggSqlFormat, fieldExpr);
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        this.Visitor.IsScalar = true;
        command.CommandText = this.Visitor.BuildSql(out _);
        return await this.QueryScalarAsync<TTarget>(isNeedClose, connection, command, cancellationToken);
    }
    #endregion

    #region NewCreateVisitor
    public virtual ICreateVisitor NewCreateVisitor(Type entityType)
    {
        var createVisiter = this.OrmProvider.NewCreateVisitor(entityType, this.DbContext, 'a', this.Visitor.Command);
        createVisiter.Tables = this.Visitor.Tables;
        createVisiter.SharedQueryObjs = this.Visitor.SharedQueryObjs;
        createVisiter.ShardingTables = this.Visitor.ShardingTables;
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
        this.Visitor.UseTable(this.Visitor.Tables.Last(), tableNames);
        return this;
    }
    public virtual IQuery<T> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    public virtual IQuery<T> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.Visitor.Tables.Last(), tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.Visitor.Tables.Last(), rawSql);
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
        base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T, TElement> IncludeMany<TElement>(Expression<Func<T, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeInternal<TElement>(memberSelector, filter);
        return this.OrmProvider.NewIncludableQuery<T, TElement>(this.DbContext, this.Visitor);
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
    public virtual T First() => this.QuerySingle<T>(this.Visitor);
    public virtual async Task<T> FirstAsync(CancellationToken cancellationToken = default)
        => await this.QuerySingleAsync<T>(this.Visitor, cancellationToken);
    public virtual List<T> ToList() => this.Query<T>(this.Visitor);
    public virtual async Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
         => await this.QueryAsync<T>(this.Visitor, cancellationToken);
    public virtual IPagedList<T> ToPageList() => this.QueryPage<T>(this.Visitor);
    public virtual async Task<IPagedList<T>> ToPageListAsync(CancellationToken cancellationToken = default)
        => await this.QueryPageAsync<T>(this.Visitor, cancellationToken);
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
        if (string.IsNullOrEmpty(tableName))
            throw new ArgumentNullException(nameof(tableName));
        var cteQuery = this.Visitor.AsCteTable(typeof(T), tableName);
        return cteQuery as ICteQuery<T>;
    }
    #endregion

    #region AsSharedQuery
    public virtual IQuery<T> AsSharedQuery()
    {
        this.Visitor.AsSharedQuery();
        return this;
    }
    #endregion

    #region ToSql
    public override string ToSql(out List<IDbDataParameter> dbParameters)
    {
        this.Select();
        (_, var connection, var command) = this.Visitor.UseCommand();
        var sql = this.Visitor.BuildSql(out _);
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        connection.Dispose();
        this.Visitor.Dispose();
        this.Visitor = null;
        return sql;
    }
    #endregion
}
public class CteQuery<T> : Query<T>, ICteQuery<T>
{
    #region Properties
    public string TableName { get; set; }
    public override bool IsCteQuery => true;
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

public class Query<T1, T2> : QueryBase, IQuery<T1, T2>
{
    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IQuery<T1, T2> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.Visitor.Tables.Last(), tableNames);
        return this;
    }
    public virtual IQuery<T1, T2> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.Visitor.Tables.Last(), tableNameGetter);
        return this;
    }
    public virtual IQuery<T1, T2> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    public virtual IQuery<T1, T2> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T1, T2> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.Visitor.Tables.Last(), tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T1, T2> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.Visitor.Tables.Last(), rawSql);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IQuery<T1, T2, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewQuery<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IQuery<T1, T2, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewQuery<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewQuery<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IQuery<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IQuery<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T1, T2, TMember> Include<TMember>(Expression<Func<T1, T2, TMember>> memberSelector)
    {
        base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T1, T2, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeInternal<TElement>(memberSelector, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, TElement>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IQuery<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
        => this.And(predicate);
    public virtual IQuery<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IQuery<T1, T2> WherePredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IQuery<T1, T2> And(Expression<Func<T1, T2, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2> AndPredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IQuery<T1, T2> Or(Expression<Func<T1, T2, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2> Or(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2> OrPredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T1, T2, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2> OrderByDynamic(Func<OrderByBuilder<T1, T2>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IQuery<T1, T2> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IQuery<T1, T2> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IQuery<T1, T2> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountInternalAsync(fieldExpr, cancellationToken);
    public virtual int CountDistinct<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountDistinctInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCount<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCountDistinct<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountDistinctInternalAsync(fieldExpr, cancellationToken);
    #endregion

    #region Aggregate
    public virtual decimal Sum<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual async Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.SumInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Avg<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.AvgInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Max<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MaxInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Min<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MinInternalAsync<TField>(fieldExpr, cancellationToken);
    #endregion

    #region ToCreate
    public virtual IFromCreate<TEntity> ToCreate<TEntity>(Expression<Func<T1, T2, object>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));

        this.SelectInternal(fieldsSelector);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    #endregion
}
public class Query<T1, T2, T3> : QueryBase, IQuery<T1, T2, T3>
{
    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IQuery<T1, T2, T3> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.Visitor.Tables.Last(), tableNames);
        return this;
    }
    public virtual IQuery<T1, T2, T3> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.Visitor.Tables.Last(), tableNameGetter);
        return this;
    }
    public virtual IQuery<T1, T2, T3> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    public virtual IQuery<T1, T2, T3> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T1, T2, T3> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.Visitor.Tables.Last(), tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T1, T2, T3> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.Visitor.Tables.Last(), rawSql);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IQuery<T1, T2, T3, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewQuery<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IQuery<T1, T2, T3, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewQuery<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewQuery<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IQuery<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IQuery<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T1, T2, T3, TMember> Include<TMember>(Expression<Func<T1, T2, T3, TMember>> memberSelector)
    {
        base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeInternal<TElement>(memberSelector, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, TElement>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IQuery<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
        => this.And(predicate);
    public virtual IQuery<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IQuery<T1, T2, T3> WherePredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IQuery<T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3> AndPredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IQuery<T1, T2, T3> Or(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3> Or(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3> OrPredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T1, T2, T3, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IQuery<T1, T2, T3> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IQuery<T1, T2, T3> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IQuery<T1, T2, T3> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountInternalAsync(fieldExpr, cancellationToken);
    public virtual int CountDistinct<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountDistinctInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCount<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountDistinctInternalAsync(fieldExpr, cancellationToken);
    #endregion

    #region Aggregate
    public virtual decimal Sum<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual async Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.SumInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Avg<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.AvgInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Max<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MaxInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Min<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MinInternalAsync<TField>(fieldExpr, cancellationToken);
    #endregion

    #region ToCreate
    public virtual IFromCreate<TEntity> ToCreate<TEntity>(Expression<Func<T1, T2, T3, object>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));

        this.SelectInternal(fieldsSelector);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    #endregion
}
public class Query<T1, T2, T3, T4> : QueryBase, IQuery<T1, T2, T3, T4>
{
    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IQuery<T1, T2, T3, T4> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.Visitor.Tables.Last(), tableNames);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.Visitor.Tables.Last(), tableNameGetter);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T1, T2, T3, T4> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.Visitor.Tables.Last(), tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T1, T2, T3, T4> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.Visitor.Tables.Last(), rawSql);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IQuery<T1, T2, T3, T4, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IQuery<T1, T2, T3, T4, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IQuery<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IQuery<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T1, T2, T3, T4, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> memberSelector)
    {
        base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeInternal<TElement>(memberSelector, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, TElement>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IQuery<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
        => this.And(predicate);
    public virtual IQuery<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IQuery<T1, T2, T3, T4> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IQuery<T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IQuery<T1, T2, T3, T4> Or(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4> Or(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T1, T2, T3, T4, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IQuery<T1, T2, T3, T4> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountInternalAsync(fieldExpr, cancellationToken);
    public virtual int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountDistinctInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCount<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountDistinctInternalAsync(fieldExpr, cancellationToken);
    #endregion

    #region Aggregate
    public virtual decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual async Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.SumInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Avg<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.AvgInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Max<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MaxInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Min<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MinInternalAsync<TField>(fieldExpr, cancellationToken);
    #endregion

    #region ToCreate
    public virtual IFromCreate<TEntity> ToCreate<TEntity>(Expression<Func<T1, T2, T3, T4, object>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));

        this.SelectInternal(fieldsSelector);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    #endregion
}
public class Query<T1, T2, T3, T4, T5> : QueryBase, IQuery<T1, T2, T3, T4, T5>
{
    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IQuery<T1, T2, T3, T4, T5> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.Visitor.Tables.Last(), tableNames);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.Visitor.Tables.Last(), tableNameGetter);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T1, T2, T3, T4, T5> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.Visitor.Tables.Last(), tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T1, T2, T3, T4, T5> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.Visitor.Tables.Last(), rawSql);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IQuery<T1, T2, T3, T4, T5, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IQuery<T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IQuery<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IQuery<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> memberSelector)
    {
        base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeInternal<TElement>(memberSelector, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, TElement>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IQuery<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        => this.And(predicate);
    public virtual IQuery<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IQuery<T1, T2, T3, T4, T5> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IQuery<T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IQuery<T1, T2, T3, T4, T5> Or(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T1, T2, T3, T4, T5, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IQuery<T1, T2, T3, T4, T5> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountInternalAsync(fieldExpr, cancellationToken);
    public virtual int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountDistinctInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountDistinctInternalAsync(fieldExpr, cancellationToken);
    #endregion

    #region Aggregate
    public virtual decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual async Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.SumInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.AvgInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MaxInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MinInternalAsync<TField>(fieldExpr, cancellationToken);
    #endregion

    #region ToCreate
    public virtual IFromCreate<TEntity> ToCreate<TEntity>(Expression<Func<T1, T2, T3, T4, T5, object>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));

        this.SelectInternal(fieldsSelector);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    #endregion
}
public class Query<T1, T2, T3, T4, T5, T6> : QueryBase, IQuery<T1, T2, T3, T4, T5, T6>
{
    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IQuery<T1, T2, T3, T4, T5, T6> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.Visitor.Tables.Last(), tableNames);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.Visitor.Tables.Last(), tableNameGetter);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T1, T2, T3, T4, T5, T6> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.Visitor.Tables.Last(), tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T1, T2, T3, T4, T5, T6> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.Visitor.Tables.Last(), rawSql);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IQuery<T1, T2, T3, T4, T5, T6, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IQuery<T1, T2, T3, T4, T5, T6, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> memberSelector)
    {
        base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeInternal<TElement>(memberSelector, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, TElement>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IQuery<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
        => this.And(predicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IQuery<T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IQuery<T1, T2, T3, T4, T5, T6> Or(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T1, T2, T3, T4, T5, T6, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IQuery<T1, T2, T3, T4, T5, T6> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountInternalAsync(fieldExpr, cancellationToken);
    public virtual int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountDistinctInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountDistinctInternalAsync(fieldExpr, cancellationToken);
    #endregion

    #region Aggregate
    public virtual decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual async Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.SumInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.AvgInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MaxInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MinInternalAsync<TField>(fieldExpr, cancellationToken);
    #endregion

    #region ToCreate
    public virtual IFromCreate<TEntity> ToCreate<TEntity>(Expression<Func<T1, T2, T3, T4, T5, T6, object>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));

        this.SelectInternal(fieldsSelector);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    #endregion
}
public class Query<T1, T2, T3, T4, T5, T6, T7> : QueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7>
{
    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.Visitor.Tables.Last(), tableNames);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.Visitor.Tables.Last(), tableNameGetter);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.Visitor.Tables.Last(), tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.Visitor.Tables.Last(), rawSql);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> memberSelector)
    {
        base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeInternal<TElement>(memberSelector, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElement>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
        => this.And(predicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountInternalAsync(fieldExpr, cancellationToken);
    public virtual int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountDistinctInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountDistinctInternalAsync(fieldExpr, cancellationToken);
    #endregion

    #region Aggregate
    public virtual decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual async Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.SumInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.AvgInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MaxInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MinInternalAsync<TField>(fieldExpr, cancellationToken);
    #endregion

    #region ToCreate
    public virtual IFromCreate<TEntity> ToCreate<TEntity>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, object>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));

        this.SelectInternal(fieldsSelector);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    #endregion
}
public class Query<T1, T2, T3, T4, T5, T6, T7, T8> : QueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8>
{
    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.Visitor.Tables.Last(), tableNames);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.Visitor.Tables.Last(), tableNameGetter);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.Visitor.Tables.Last(), tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.Visitor.Tables.Last(), rawSql);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> memberSelector)
    {
        base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeInternal<TElement>(memberSelector, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElement>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
        => this.And(predicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountInternalAsync(fieldExpr, cancellationToken);
    public virtual int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountDistinctInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountDistinctInternalAsync(fieldExpr, cancellationToken);
    #endregion

    #region Aggregate
    public virtual decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual async Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.SumInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.AvgInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MaxInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MinInternalAsync<TField>(fieldExpr, cancellationToken);
    #endregion

    #region ToCreate
    public virtual IFromCreate<TEntity> ToCreate<TEntity>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, object>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));

        this.SelectInternal(fieldsSelector);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    #endregion
}
public class Query<T1, T2, T3, T4, T5, T6, T7, T8, T9> : QueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.Visitor.Tables.Last(), tableNames);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.Visitor.Tables.Last(), tableNameGetter);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.Visitor.Tables.Last(), tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.Visitor.Tables.Last(), rawSql);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> memberSelector)
    {
        base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeInternal<TElement>(memberSelector, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElement>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
        => this.And(predicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountInternalAsync(fieldExpr, cancellationToken);
    public virtual int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountDistinctInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountDistinctInternalAsync(fieldExpr, cancellationToken);
    #endregion

    #region Aggregate
    public virtual decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual async Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.SumInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.AvgInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MaxInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MinInternalAsync<TField>(fieldExpr, cancellationToken);
    #endregion

    #region ToCreate
    public virtual IFromCreate<TEntity> ToCreate<TEntity>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, object>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));

        this.SelectInternal(fieldsSelector);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    #endregion
}
public class Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : QueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.Visitor.Tables.Last(), tableNames);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.Visitor.Tables.Last(), tableNameGetter);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.Visitor.Tables.Last(), tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.Visitor.Tables.Last(), rawSql);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> memberSelector)
    {
        base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeInternal<TElement>(memberSelector, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElement>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
        => this.And(predicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountInternalAsync(fieldExpr, cancellationToken);
    public virtual int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountDistinctInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountDistinctInternalAsync(fieldExpr, cancellationToken);
    #endregion

    #region Aggregate
    public virtual decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual async Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.SumInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.AvgInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MaxInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MinInternalAsync<TField>(fieldExpr, cancellationToken);
    #endregion

    #region ToCreate
    public virtual IFromCreate<TEntity> ToCreate<TEntity>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));

        this.SelectInternal(fieldsSelector);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    #endregion
}
public class Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : QueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.Visitor.Tables.Last(), tableNames);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.Visitor.Tables.Last(), tableNameGetter);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.Visitor.Tables.Last(), tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.Visitor.Tables.Last(), rawSql);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> memberSelector)
    {
        base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeInternal<TElement>(memberSelector, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElement>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
        => this.And(predicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountInternalAsync(fieldExpr, cancellationToken);
    public virtual int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountDistinctInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountDistinctInternalAsync(fieldExpr, cancellationToken);
    #endregion

    #region Aggregate
    public virtual decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual async Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.SumInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.AvgInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MaxInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MinInternalAsync<TField>(fieldExpr, cancellationToken);
    #endregion

    #region ToCreate
    public virtual IFromCreate<TEntity> ToCreate<TEntity>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));

        this.SelectInternal(fieldsSelector);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    #endregion
}
public class Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : QueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.Visitor.Tables.Last(), tableNames);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.Visitor.Tables.Last(), tableNameGetter);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.Visitor.Tables.Last(), tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.Visitor.Tables.Last(), rawSql);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> memberSelector)
    {
        base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeInternal<TElement>(memberSelector, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElement>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
        => this.And(predicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountInternalAsync(fieldExpr, cancellationToken);
    public virtual int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountDistinctInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountDistinctInternalAsync(fieldExpr, cancellationToken);
    #endregion

    #region Aggregate
    public virtual decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual async Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.SumInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.AvgInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MaxInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MinInternalAsync<TField>(fieldExpr, cancellationToken);
    #endregion

    #region ToCreate
    public virtual IFromCreate<TEntity> ToCreate<TEntity>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, object>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));

        this.SelectInternal(fieldsSelector);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    #endregion
}
public class Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : QueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.Visitor.Tables.Last(), tableNames);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.Visitor.Tables.Last(), tableNameGetter);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.Visitor.Tables.Last(), tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.Visitor.Tables.Last(), rawSql);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> memberSelector)
    {
        base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeInternal<TElement>(memberSelector, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElement>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
        => this.And(predicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountInternalAsync(fieldExpr, cancellationToken);
    public virtual int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountDistinctInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountDistinctInternalAsync(fieldExpr, cancellationToken);
    #endregion

    #region Aggregate
    public virtual decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual async Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.SumInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.AvgInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MaxInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MinInternalAsync<TField>(fieldExpr, cancellationToken);
    #endregion

    #region ToCreate
    public virtual IFromCreate<TEntity> ToCreate<TEntity>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, object>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));

        this.SelectInternal(fieldsSelector);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    #endregion
}
public class Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : QueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.Visitor.Tables.Last(), tableNames);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.Visitor.Tables.Last(), tableNameGetter);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.Visitor.Tables.Last(), tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.Visitor.Tables.Last(), rawSql);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> memberSelector)
    {
        base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeInternal<TElement>(memberSelector, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElement>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
        => this.And(predicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountInternalAsync(fieldExpr, cancellationToken);
    public virtual int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountDistinctInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountDistinctInternalAsync(fieldExpr, cancellationToken);
    #endregion

    #region Aggregate
    public virtual decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual async Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.SumInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.AvgInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MaxInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MinInternalAsync<TField>(fieldExpr, cancellationToken);
    #endregion

    #region ToCreate
    public virtual IFromCreate<TEntity> ToCreate<TEntity>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, object>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));

        this.SelectInternal(fieldsSelector);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    #endregion
}
public class Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : QueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.Visitor.Tables.Last(), tableNames);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.Visitor.Tables.Last(), tableNameGetter);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.Visitor.Tables.Last(), tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.Visitor.Tables.Last(), rawSql);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> memberSelector)
    {
        base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeInternal<TElement>(memberSelector, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElement>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
        => this.And(predicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountInternalAsync(fieldExpr, cancellationToken);
    public virtual int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountDistinctInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountDistinctInternalAsync(fieldExpr, cancellationToken);
    #endregion

    #region Aggregate
    public virtual decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual async Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.SumInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.AvgInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MaxInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MinInternalAsync<TField>(fieldExpr, cancellationToken);
    #endregion

    #region ToCreate
    public virtual IFromCreate<TEntity> ToCreate<TEntity>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, object>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));

        this.SelectInternal(fieldsSelector);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    #endregion
}
public class Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : QueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
{
    #region Constructor
    public Query(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.Visitor.Tables.Last(), tableNames);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.Visitor.Tables.Last(), tableNameGetter);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.Visitor.Tables.Last(), fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.Visitor.Tables.Last(), tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.Visitor.Tables.Last(), rawSql);
        return this;
    }
    #endregion

    #region InnerJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    #endregion

    #region LeftJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    #endregion

    #region RightJoin
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> memberSelector)
    {
        base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeInternal<TElement>(memberSelector, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TElement>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate)
        => this.And(predicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountInternalAsync(fieldExpr, cancellationToken);
    public virtual int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.CountDistinctInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountInternalAsync(fieldExpr, cancellationToken);
    public virtual long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.LongCountDistinctInternalAsync(fieldExpr, cancellationToken);
    #endregion

    #region Aggregate
    public virtual decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual async Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.SumInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.AvgInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MaxInternalAsync<TField>(fieldExpr, cancellationToken);
    public virtual TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await base.MinInternalAsync<TField>(fieldExpr, cancellationToken);
    #endregion

    #region ToCreate
    public virtual IFromCreate<TEntity> ToCreate<TEntity>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, object>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));

        this.SelectInternal(fieldsSelector);
        var createVisitor = this.NewCreateVisitor(typeof(TEntity));
        return this.OrmProvider.NewFromCreate<TEntity>(this.DbContext, createVisitor);
    }
    #endregion
}