using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Trolley;

public class MultiQueryBase : QueryInternal, IMultiQueryBase
{
    #region Properties
    public IMultipleQuery MultipleQuery { get; set; }
    #endregion

    #region Constructor
    public MultiQueryBase(IMultipleQuery multipleQuery, IQueryVisitor visitor)
    {
        this.MultipleQuery = multipleQuery;
        this.Visitor = visitor;
        this.DbContext = multipleQuery.DbContext;
    }
    #endregion

    #region Exists
    public virtual IMultipleQuery Exists()
    {
        this.Visitor.Select("1", null);
        this.Visitor.Take(1);
        return this.QueryScalar<bool>();
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count() => this.QueryScalar<int>("COUNT(1)", "COUNT_VALUE");
    public virtual IMultipleQuery LongCount() => this.QueryScalar<long>("COUNT(1)", "COUNT_VALUE");
    protected IMultipleQuery CountInternal(Expression fieldExpr)
        => this.QueryScalar<int>("COUNT({0})", "COUNT_VALUE", fieldExpr);
    protected IMultipleQuery CountDistinctInternal(Expression fieldExpr)
        => this.QueryScalar<int>("COUNT(DISTINCT {0})", "COUNT_VALUE", fieldExpr);
    protected IMultipleQuery LongCountInternal(Expression fieldExpr)
        => this.QueryScalar<long>("COUNT({0})", "COUNT_VALUE", fieldExpr);
    protected IMultipleQuery LongCountDistinctInternal(Expression fieldExpr)
        => this.QueryScalar<long>("COUNT(DISTINCT {0})", "COUNT_VALUE", fieldExpr);
    protected IMultipleQuery SumInternal<TField>(Expression fieldExpr)
        => this.QueryScalar<TField>("SUM({0})", "SUM_VALUE", fieldExpr);
    protected IMultipleQuery AvgInternal<TField>(Expression fieldExpr)
        => this.QueryScalar<TField>("AVG({0})", "AVG_VALUE", fieldExpr);
    protected IMultipleQuery MaxInternal<TField>(Expression fieldExpr)
        => this.QueryScalar<TField>("MAX({0})", "MAX_VALUE", fieldExpr);
    protected IMultipleQuery MinInternal<TField>(Expression fieldExpr)
        => this.QueryScalar<TField>("MIN({0})", "MIN_VALUE", fieldExpr);
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        return this.Visitor.BuildSql(true, out _);
    }
    #endregion

    #region QueryScalar
    protected IMultipleQuery QueryScalar<TTarget>()
    {
        var sql = this.Visitor.BuildSql(true, out _);
        this.MultipleQuery.AddReader(typeof(TTarget), sql, ReaderResultType.Value);
        return this.MultipleQuery;
    }
    protected IMultipleQuery QueryScalar<TTarget>(string sqlFormat, string shardingFieldAlias)
    {
        if (string.IsNullOrEmpty(sqlFormat))
            throw new ArgumentNullException(nameof(sqlFormat));

        this.Visitor.AggFieldAlias = shardingFieldAlias;
        this.Visitor.Select(sqlFormat, null);
        var sql = this.Visitor.BuildSql(true, out _);
        this.MultipleQuery.AddReader(typeof(TTarget), sql, ReaderResultType.Value);
        return this.MultipleQuery;
    }
    protected IMultipleQuery QueryScalar<TTarget>(string sqlFormat, string shardingFieldAlias, Expression fieldExpr)
    {
        if (string.IsNullOrEmpty(sqlFormat))
            throw new ArgumentNullException(nameof(sqlFormat));
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        this.Visitor.AggFieldAlias = shardingFieldAlias;
        this.Visitor.Select(sqlFormat, fieldExpr);
        var sql = this.Visitor.BuildSql(true, out _);
        this.MultipleQuery.AddReader(typeof(TTarget), sql, ReaderResultType.Value);
        return this.MultipleQuery;
    }
    #endregion
}
public class MultiQuery<T> : MultiQueryBase, IMultiQuery<T>
{
    #region Constructor
    public MultiQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(TableShardingUsageMode.ReadOnly, false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(TableShardingUsageMode.ReadOnly, false, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.ReadOnly, false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableShardingUsageMode.ReadOnly, false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IMultiQuery<T> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region GetShardingTableName
    public virtual IMultipleQuery GetShardingTableName(params object[] fieldValues)
    {
        var tableSchema = this.Visitor.Tables[0].TableSchema;
        return this.MultipleQuery.GetShardingTableName<T>(fieldValues);
    }
    #endregion

    #region Union/UnionAll
    public virtual IMultiQuery<T> Union(IQuery<T> subQuery)
    {
        base.UnionInternal(subQuery);
        return this;
    }
    public virtual IMultiQuery<T> Union(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        base.UnionInternal(subQueryExpr);
        return this;
    }
    public virtual IMultiQuery<T> UnionAll(IQuery<T> subQuery)
    {
        base.UnionAllInternal(subQuery);
        return this;
    }
    public virtual IMultiQuery<T> UnionAll(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        base.UnionAllInternal(subQueryExpr);
        return this;
    }
    public virtual IMultiQuery<T> UnionRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr)
    {
        base.UnionRecursiveInternal(subQueryExpr);
        return this;
    }
    public virtual IMultiQuery<T> UnionAllRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr)
    {
        base.UnionAllRecursiveInternal(subQueryExpr);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IMultiQuery<T, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewMultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IMultiQuery<T, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewMultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewMultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IMultiQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IMultiQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IMultiQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IMultiIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T, TElement> IncludeMany<TElement>(Expression<Func<T, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T> WhereBy(object whereObj)
      => this.AndBy(whereObj);
    public virtual IMultiQuery<T> WhereBy(bool condition, object whereObj)
        => this.AndBy(condition, whereObj);
    public virtual IMultiQuery<T> WhereById(object whereKey)
        => this.AndById(whereKey);
    public virtual IMultiQuery<T> WhereById(bool condition, object whereKey)
        => this.AndById(condition, whereKey);
    public virtual IMultiQuery<T> WhereByIds(IEnumerable whereKeys)
        => this.AndByIds(whereKeys);
    public virtual IMultiQuery<T> WhereByIds(bool condition, IEnumerable whereKeys)
        => this.AndByIds(condition, whereKeys);
    public virtual IMultiQuery<T> Where(Expression<Func<T, bool>> predicate)
        => this.And(true, predicate);
    public virtual IMultiQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IMultiQuery<T> WherePredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IMultiQuery<T> AndBy(object whereObj)
    {
        base.AndByInternal(whereObj);
        return this;
    }
    public virtual IMultiQuery<T> AndBy(bool condition, object whereObj)
    {
        if (!condition) return this;
        base.AndByInternal(whereObj);
        return this;
    }
    public virtual IMultiQuery<T> AndById(object whereKey)
    {
        base.AndByIdInternal(whereKey);
        return this;
    }
    public virtual IMultiQuery<T> AndById(bool condition, object whereKey)
    {
        if (!condition) return this;
        base.AndByIdInternal(whereKey);
        return this;
    }
    public virtual IMultiQuery<T> AndByIds(IEnumerable whereKeys)
    {
        base.AndByIdsInternal(whereKeys);
        return this;
    }
    public virtual IMultiQuery<T> AndByIds(bool condition, IEnumerable whereKeys)
    {
        if (!condition) return this;
        base.AndByIdsInternal(whereKeys);
        return this;
    }
    public virtual IMultiQuery<T> And(Expression<Func<T, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T> And(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T> AndPredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IMultiQuery<T> OrBy(object whereObj)
    {
        base.OrByInternal(whereObj);
        return this;
    }
    public virtual IMultiQuery<T> OrBy(bool condition, object whereObj)
    {
        if (!condition) return this;
        base.OrByInternal(whereObj);
        return this;
    }
    public virtual IMultiQuery<T> OrById(object whereKey)
    {
        base.OrByIdInternal(whereKey);
        return this;
    }
    public virtual IMultiQuery<T> OrById(bool condition, object whereKey)
    {
        if (!condition) return this;
        base.OrByIdInternal(whereKey);
        return this;
    }
    public virtual IMultiQuery<T> OrByIds(IEnumerable whereKeys)
    {
        base.OrByIdsInternal(whereKeys);
        return this;
    }
    public virtual IMultiQuery<T> OrByIds(bool condition, IEnumerable whereKeys)
    {
        if (!condition) return this;
        base.OrByIdsInternal(whereKeys);
        return this;
    }
    public virtual IMultiQuery<T> Or(Expression<Func<T, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T> Or(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T> OrPredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IMultiGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IMultiQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IMultiQuery<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IMultiQuery<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T> OrderByDynamic(Func<OrderByBuilder<T>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public virtual IMultiQuery<T> Select()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.Visitor.Select(null, defaultExpr);
        return this;
    }
    public virtual IMultiQuery<TTarget> Select<TTarget>(string rawFields)
    {
        base.SelectRawInternal(typeof(TTarget), rawFields);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectTo<TTarget>(Expression<Func<T, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Distinct
    public virtual IMultiQuery<T> Distinct()
    {
        this.Visitor.Distinct();
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IMultiQuery<T> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IMultiQuery<T> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IMultiQuery<T> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count<TField>(Expression<Func<T, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual IMultipleQuery CountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual IMultipleQuery LongCount<TField>(Expression<Func<T, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual IMultipleQuery LongCountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    #endregion

    #region Aggregate
    public virtual IMultipleQuery Sum<TField>(Expression<Func<T, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Avg<TField>(Expression<Func<T, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Max<TField>(Expression<Func<T, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Min<TField>(Expression<Func<T, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    #endregion

    #region First/ToList/ToPageList/ToDictionary
    public virtual IMultipleQuery First() => this.QueryResult(ReaderResultType.Entity, false);
    public virtual IMultipleQuery ToList() => this.QueryResult(ReaderResultType.List, false);
    public virtual IMultipleQuery ToPageList() => this.QueryResult(ReaderResultType.List, false);
    #endregion

    #region QueryResult
    private IMultipleQuery QueryResult(ReaderResultType resultType, bool isExists)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.Visitor.SelectDefault(defaultExpr);
        var sql = this.Visitor.BuildSql(true, out _);
        this.MultipleQuery.AddReader(typeof(T), sql, resultType, isExists, this.Visitor);
        return this.MultipleQuery;
    }
    #endregion
}