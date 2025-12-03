using System;
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
    public virtual int Count() => this.QueryScalar<int>("COUNT(*)", "COUNT_VALUE");
    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryScalarAsync<int>("COUNT(*)", "COUNT_VALUE", null, cancellationToken);
    public virtual long LongCount() => this.QueryScalar<long>("COUNT(*)", "COUNT_VALUE");
    public virtual async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryScalarAsync<long>("COUNT(*)", "COUNT_VALUE", null, cancellationToken);
    #endregion

    #region Count/Aggregate Internal
    protected int CountInternal(Expression fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryScalar<int>("COUNT({0})", "COUNT_VALUE", fieldExpr);
    }
    protected async Task<int> CountInternalAsync(Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryScalarAsync<int>("COUNT({0})", "COUNT_VALUE", fieldExpr, cancellationToken);
    }
    protected int CountDistinctInternal(Expression fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryScalar<int>("COUNT(DISTINCT {0})", "COUNT_VALUE", fieldExpr);
    }
    protected async Task<int> CountDistinctInternalAsync(Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryScalarAsync<int>("COUNT(DISTINCT {0})", "COUNT_VALUE", fieldExpr, cancellationToken);
    }
    protected long LongCountInternal(Expression fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryScalar<long>("COUNT({0})", "COUNT_VALUE", fieldExpr);
    }
    protected async Task<long> LongCountInternalAsync(Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryScalarAsync<long>("COUNT({0})", "COUNT_VALUE", fieldExpr, cancellationToken);
    }
    protected long LongCountDistinctInternal(Expression fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryScalar<long>("COUNT(DISTINCT {0})", "COUNT_VALUE", fieldExpr);
    }
    protected async Task<long> LongCountDistinctInternalAsync(Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryScalarAsync<long>("COUNT(DISTINCT {0})", "COUNT_VALUE", fieldExpr, cancellationToken);
    }
    protected decimal SumInternal<TField>(Expression fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryScalar<decimal>("SUM({0})", "SUM_VALUE", fieldExpr);
    }
    protected async Task<decimal> SumInternalAsync<TField>(Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryScalarAsync<decimal>("SUM({0})", "SUM_VALUE", fieldExpr, cancellationToken);
    }
    protected TField AvgInternal<TField>(Expression fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryScalar<TField>("AVG({0})", "AVG_VALUE", fieldExpr);
    }
    protected async Task<TField> AvgInternalAsync<TField>(Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryScalarAsync<TField>("AVG({0})", "AVG_VALUE", fieldExpr, cancellationToken);
    }
    protected TField MaxInternal<TField>(Expression fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryScalar<TField>("MAX({0})", "MAX_VALUE", fieldExpr);
    }
    protected async Task<TField> MaxInternalAsync<TField>(Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryScalarAsync<TField>("MAX({0})", "MAX_VALUE", fieldExpr, cancellationToken);
    }
    protected TField MinInternal<TField>(Expression fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryScalar<TField>("MIN({0})", "MIN_VALUE", fieldExpr);
    }
    protected async Task<TField> MinInternalAsync<TField>(Expression fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryScalarAsync<TField>("MIN({0})", "MIN_VALUE", fieldExpr, cancellationToken);
    }
    #endregion

    #region Exists
    public virtual bool Exists() => this.QueryScalar<int>("COUNT(*)", "COUNT_VALUE") > 0;
    public virtual async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        => await this.QueryScalarAsync<int>("COUNT(*)", "COUNT_VALUE", null, cancellationToken) > 0;
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
    protected TTarget QueryScalar<TTarget>(string sqlFormat, string shardingFieldAlias, Expression fieldExpr = null)
    {
        this.Visitor.AggFieldAlias = shardingFieldAlias;
        this.Visitor.Select(sqlFormat, fieldExpr);
        return this.DbContext.QueryScalar<TTarget>(this.Visitor);
    }
    protected async Task<TTarget> QueryScalarAsync<TTarget>(string sqlFormat, string shardingFieldAlias, Expression fieldExpr = null, CancellationToken cancellationToken = default)
    {
        this.Visitor.AggFieldAlias = shardingFieldAlias;
        this.Visitor.Select(sqlFormat, fieldExpr);
        return await this.DbContext.QueryScalarAsync<TTarget>(this.Visitor, cancellationToken);
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
    public virtual IQuery<T> UnionRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr)
    {
        base.UnionRecursiveInternal(subQueryExpr);
        return this;
    }
    public virtual IQuery<T> UnionAllRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr)
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
    public virtual IQuery<T> Where(Expression<Func<T, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IQuery<T> WherePredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
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
        var builder = new PredicateBuilder<T>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
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
        this.Visitor.Select(null, defaultExpr);
        return this;
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
        return this.DbContext.QueryFrom<T, T>(this.Visitor, true, (entityType, reader, readerFields) =>
        {
            T result = default;
            var deserializer = reader.GetReaderDeserializer(typeof(T), this.DbContext, readerFields);
            if (reader.Read())
                result = (T)deserializer.Invoke(reader);
            return result;
        });
    }
    public virtual async Task<T> FirstAsync(CancellationToken cancellationToken = default)
    {
        return await this.DbContext.QueryFromAsync<T, T>(this.Visitor, true, async (entityType, reader, readerFields, cancellationToken) =>
        {
            T result = default;
            var deserializer = reader.GetReaderDeserializer(typeof(T), this.DbContext, readerFields);
            if (await reader.ReadAsync(cancellationToken))
                result = (T)deserializer.Invoke(reader);
            return result;
        }, cancellationToken);
    }
    public virtual List<T> ToList()
    {
        return this.DbContext.QueryFrom<T, List<T>>(this.Visitor, false, (entityType, reader, readerFields) =>
        {
            var result = new List<T>();
            var deserializer = reader.GetReaderDeserializer(typeof(T), this.DbContext, readerFields);
            while (reader.Read())
                result.Add((T)deserializer.Invoke(reader));
            return result;
        });
    }
    public virtual async Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
    {
        return await this.DbContext.QueryFromAsync<T, List<T>>(this.Visitor, false, async (entityType, reader, readerFields, cancellationToken) =>
        {
            var result = new List<T>();
            var deserializer = reader.GetReaderDeserializer(typeof(T), this.DbContext, readerFields);
            while (await reader.ReadAsync(cancellationToken))
                result.Add((T)deserializer.Invoke(reader));
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

    #region AsCteTable
    public virtual ICteQuery<T> AsCteTable(string tableName)
    {
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
    public List<SqlFieldSegment> ReaderFields { get; set; }
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