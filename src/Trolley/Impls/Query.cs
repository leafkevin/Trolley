﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class QueryAnonymousObject : IQueryAnonymousObject
{
    #region Properties
    public virtual DbContext DbContext { get; private set; }
    public virtual IQueryVisitor Visitor { get; protected set; }
    #endregion

    #region Constructor
    public QueryAnonymousObject(IQueryVisitor visitor) => this.Visitor = visitor;
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        var sql = this.Visitor.BuildSql(out _);
        this.Visitor.Dispose();
        return sql;
    }
    #endregion
}
public class QueryBase : IQueryBase
{
    #region Properties
    public virtual DbContext DbContext { get; private set; }
    public virtual IQueryVisitor Visitor { get; protected set; }
    public virtual IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public QueryBase(DbContext dbContext, IQueryVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Select	
    public virtual IQueryAnonymousObject SelectAnonymous()
    {
        this.Visitor.Select("*");
        return new QueryAnonymousObject(this.Visitor);
    }
    #endregion

    #region Count
    public virtual int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public virtual long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public virtual async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        var sql = this.Visitor.BuildSql(out _);
        this.Visitor.Dispose();
        return sql;
    }
    #endregion

    #region QueryFirstValue
    protected TTarget QueryFirstValue<TTarget>(string sqlFormat, Expression fieldExpr = null)
    {
        this.Visitor.Select(sqlFormat, fieldExpr);
        return this.DbContext.QueryFirst<TTarget>(this.Visitor);
    }
    protected async Task<TTarget> QueryFirstValueAsync<TTarget>(string sqlFormat, Expression fieldExpr = null, CancellationToken cancellationToken = default)
    {
        this.Visitor.Select(sqlFormat, fieldExpr);
        return await this.DbContext.QueryFirstAsync<TTarget>(this.Visitor, cancellationToken);
    }
    #endregion
}
public class Query<T> : QueryBase, IQuery<T>
{
    #region Fields
    private int? offset;
    private int pageSize;
    #endregion

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
        var entityType = typeof(T);
        this.Visitor.UseTable(entityType, tableNames);
        return this;
    }
    public virtual IQuery<T> UseTable(Func<string, bool> tableNamePredicate)
    {
        var entityType = typeof(T);
        this.Visitor.UseTable(entityType, tableNamePredicate);
        return this;
    }
    public virtual IQuery<T> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter)
    {
        var entityType = typeof(T);
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(entityType, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IQuery<T> UseTableBy(object field1Value, object field2Value = null)
    {
        var entityType = typeof(T);
        this.Visitor.UseTableBy(entityType, field1Value, field2Value);
        return this;
    }
    public virtual IQuery<T> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        var entityType = typeof(T);
        this.Visitor.UseTableByRange(entityType, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IQuery<T> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        var entityType = typeof(T);
        this.Visitor.UseTableByRange(entityType, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region Union/UnionAll
    public virtual IQuery<T> Union(IQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION", typeof(T), subQuery);
        return this;
    }
    public virtual IQuery<T> Union(Func<IFromQuery, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION", typeof(T), this.DbContext, subQuery);
        return this;
    }
    public virtual IQuery<T> UnionAll(IQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION ALL", typeof(T), subQuery);
        return this;
    }
    public virtual IQuery<T> UnionAll(Func<IFromQuery, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION ALL", typeof(T), this.DbContext, subQuery);
        return this;
    }
    public virtual IQuery<T> UnionRecursive(Func<IFromQuery, IQuery<T>, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var cteQuery = new CteQuery<T>(this);
        this.Visitor.UnionRecursive(" UNION", this.DbContext, cteQuery, subQuery);
        return this;
    }
    public virtual IQuery<T> UnionAllRecursive(Func<IFromQuery, IQuery<T>, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var cteQuery = new CteQuery<T>(this);
        this.Visitor.UnionRecursive(" UNION ALL", this.DbContext, cteQuery, subQuery);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IQuery<T, TOther> WithTable<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), subQuery);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Join
    public virtual IQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<T, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewQuery<T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector);
        return this.OrmProvider.NewIncludableQuery<T, TMember>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T, TElment> IncludeMany<TElment>(Expression<Func<T, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, true, filter);
        return this.OrmProvider.NewIncludableQuery<T, TElment>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where/And
    public virtual IQuery<T> Where(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public virtual IQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public virtual IQuery<T> And(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public virtual IQuery<T> And(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public virtual IGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewGroupQuery<T, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IQuery<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public virtual IQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IQuery<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("DESC", fieldsExpr);
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
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<T, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewQuery<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
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

    #region Skip/Take/Page
    public virtual IQuery<T> Skip(int offset)
    {
        this.offset = offset;
        if (this.pageSize > 0)
        {
            var pageIndex = (int)Math.Ceiling((double)this.offset.Value / this.pageSize);
            this.Visitor.Page(pageIndex + 1, this.pageSize);
        }
        else this.Visitor.Skip(offset);
        return this;
    }
    public virtual IQuery<T> Take(int limit)
    {
        this.pageSize = limit;
        if (this.offset.HasValue)
        {
            var pageIndex = (int)Math.Ceiling((double)this.offset.Value / limit);
            this.Visitor.Page(pageIndex + 1, limit);
        }
        else this.Visitor.Take(limit);
        return this;
    }
    public virtual IQuery<T> Page(int pageNumber, int pageSize)
    {
        this.pageSize = pageSize;
        this.Visitor.Page(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Exists
    public virtual bool Exists(Expression<Func<T, bool>> predicate) => this.QueryFirstValue<int>("COUNT(1)") > 0;
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken) > 0;
    #endregion

    #region Count
    public virtual int Count<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public virtual async Task<int> CountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    }
    public virtual int CountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public virtual async Task<int> CountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    }
    public virtual long LongCount<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public virtual async Task<long> LongCountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    }
    public virtual long LongCountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public virtual async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    }
    #endregion

    #region Aggregate
    public virtual TField Sum<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public virtual async Task<TField> SumAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    }
    public virtual TField Avg<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public virtual async Task<TField> AvgAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    }
    public virtual TField Max<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public virtual async Task<TField> MaxAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    }
    public virtual TField Min<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    public virtual async Task<TField> MinAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    }
    #endregion

    #region First/ToList/ToPageList/ToDictionary
    public virtual T First() => this.DbContext.QueryFirst<T>(this.Visitor);
    public virtual async Task<T> FirstAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.QueryFirstAsync<T>(this.Visitor, cancellationToken);
    public virtual List<T> ToList() => this.DbContext.Query<T>(this.Visitor);
    public virtual async Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.QueryAsync<T>(this.Visitor, cancellationToken);
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
        //防止重复创建对象
        if (this is ICteQuery<T> cteQueryObj)
            return cteQueryObj;
        ICteQuery<T> queryObj = null;
        if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
            throw new NotSupportedException("CTE暂时不支持多分表，只支持单个分表");

        if (this.Visitor.SelfRefQueryObj != null)
        {
            queryObj = this.Visitor.SelfRefQueryObj as CteQuery<T>;
            queryObj.Body = this.Visitor.BuildCteTableSql(tableName, out _, out _);
        }
        else
        {
            queryObj = new CteQuery<T>(this.DbContext, this.Visitor);
            queryObj.Body = this.Visitor.BuildCteTableSql(tableName, out var readerFields, out _);
            queryObj.TableName = tableName;
            queryObj.ReaderFields = readerFields;
        }
        return queryObj;
    }
    #endregion    

    #region ToSql
    public override string ToSql(out List<IDbDataParameter> dbParameters)
    {
        if (this.Visitor.IsNeedFetchShardingTables)
            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
        Expression<Func<T, T>> defaultExpr = f => f;
        this.Visitor.SelectDefault(defaultExpr);
        var sql = this.Visitor.BuildSql(out _);
        (_, sql) = this.DbContext.BuildSql(this.Visitor, sql, " UNION ALL ");
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
    public bool IsRecursive { get; set; }
    #endregion

    #region Constructor
    public CteQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    public CteQuery(IQuery queryObj)
        : base(queryObj.DbContext, queryObj.Visitor) { }
    #endregion

    #region 不支持的方法
    [Obsolete("不支持的方法调用，CTE查询中不支持Include操作")]
    public override IIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持IncludeMany操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持IncludeMany操作")]
    public override IIncludableQuery<T, TElment> IncludeMany<TElment>(Expression<Func<T, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持IncludeMany操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override int Count() => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override Task<int> CountAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override int Count<TField>(Expression<Func<T, TField>> fieldExpr)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override Task<int> CountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override int CountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override Task<int> CountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override long LongCount() => throw new NotSupportedException("不支持的方法调用");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override long LongCount<TField>(Expression<Func<T, TField>> fieldExpr)
        => throw new NotSupportedException("不支持的方法调用");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override Task<long> LongCountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override long LongCountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
        => throw new NotSupportedException("不支持的方法调用");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override Task<long> LongCountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default) => throw new NotSupportedException("不支持的方法调用");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override TField Sum<TField>(Expression<Func<T, TField>> fieldExpr)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override Task<TField> SumAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override TField Avg<TField>(Expression<Func<T, TField>> fieldExpr)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override Task<TField> AvgAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override TField Max<TField>(Expression<Func<T, TField>> fieldExpr)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override Task<TField> MaxAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override TField Min<TField>(Expression<Func<T, TField>> fieldExpr)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override Task<TField> MinAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");

    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override T First() => throw new NotSupportedException("不支持的方法调用");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override Task<T> FirstAsync(CancellationToken cancellationToken = default) 
        => throw new NotSupportedException("不支持的方法调用");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override List<T> ToList() => throw new NotSupportedException("不支持的方法调用");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override Task<List<T>> ToListAsync(CancellationToken cancellationToken = default) 
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override IPagedList<T> ToPageList() => throw new NotSupportedException("不支持的方法调用");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override Task<IPagedList<T>> ToPageListAsync(CancellationToken cancellationToken = default) 
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector) 
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    [Obsolete("不支持的方法调用，CTE查询中不支持返回结果操作")]
    public override Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector, CancellationToken cancellationToken = default) 
        => throw new NotSupportedException("不支持的方法调用，CTE查询中不支持返回结果操作");
    #endregion
}