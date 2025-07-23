using System;
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

    #region Select
    public virtual IMultiQuery<TTarget> Select<TTarget>(string fields = "*")
    {
        base.SelectInternal(fields);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count() => this.QueryFirstValue<int>("COUNT(1)", "COUNT_VALUE");
    public virtual IMultipleQuery LongCount() => this.QueryFirstValue<long>("COUNT(1)", "COUNT_VALUE");
    protected IMultipleQuery CountInternal(Expression fieldExpr)
        => this.QueryFirstValue<int>("COUNT({0})", "COUNT_VALUE", fieldExpr);
    protected IMultipleQuery CountDistinctInternal(Expression fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", "COUNT_VALUE", fieldExpr);
    protected IMultipleQuery LongCountInternal(Expression fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", "COUNT_VALUE", fieldExpr);
    protected IMultipleQuery LongCountDistinctInternal(Expression fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", "COUNT_VALUE", fieldExpr);
    protected IMultipleQuery SumInternal<TField>(Expression fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", "SUM_VALUE", fieldExpr);
    protected IMultipleQuery AvgInternal<TField>(Expression fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", "AVG_VALUE", fieldExpr);
    protected IMultipleQuery MaxInternal<TField>(Expression fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", "MAX_VALUE", fieldExpr);
    protected IMultipleQuery MinInternal<TField>(Expression fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", "MIN_VALUE", fieldExpr);
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        return this.Visitor.BuildSql(true, out _);
    }
    #endregion

    #region QueryFirstValue
    protected IMultipleQuery QueryFirstValue<TTarget>(string sqlFormat, string shardingFieldAlias)
    {
        if (string.IsNullOrEmpty(sqlFormat))
            throw new ArgumentNullException(nameof(sqlFormat));

        this.Visitor.AggFieldAlias = shardingFieldAlias;
        this.Visitor.Select(sqlFormat, null);
        var sql = this.Visitor.BuildSql(true, out _);
        this.MultipleQuery.AddReader(typeof(TTarget), sql, true);
        return this.MultipleQuery;
    }
    protected IMultipleQuery QueryFirstValue<TTarget>(string sqlFormat, string shardingFieldAlias, Expression fieldExpr)
    {
        if (string.IsNullOrEmpty(sqlFormat))
            throw new ArgumentNullException(nameof(sqlFormat));
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        this.Visitor.AggFieldAlias = shardingFieldAlias;
        this.Visitor.Select(sqlFormat, fieldExpr);
        var sql = this.Visitor.BuildSql(true, out _);
        this.MultipleQuery.AddReader(typeof(TTarget), sql, true);
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
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IMultiQuery<T> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IMultiQuery<T> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public virtual IMultiQuery<T> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
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

    #region GetShardingTableNames
    public virtual IMultipleQuery GetShardingTableNames(Func<string, bool> tableNameSelector)
    {
        var tableSchema = this.Visitor.Tables[0].TableSchema;
        return this.MultipleQuery.GetShardingTableNames<T>(tableNameSelector, tableSchema);
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
    public virtual IMultiIncludableQuery<T, TElment> IncludeMany<TElment>(Expression<Func<T, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        base.IncludeManyInternal<TElment>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T, TElment>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T> Where(Expression<Func<T, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T> WherePredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
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
        var builder = new PredicateBuilder<T>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
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
    #endregion

    #region Select
    public virtual IMultiQuery<T> Select()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.Visitor.Select(null, defaultExpr);
        return this;
    }
    public virtual IMultiQuery<TTarget> SelectFlattenTo<TTarget>(Expression<Func<T, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
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
    public virtual IMultipleQuery First() => this.QueryResult(true);
    public virtual IMultipleQuery ToList() => this.QueryResult(false);
    public virtual IMultipleQuery ToPageList() => this.QueryResult(false);
    public virtual IMultipleQuery ToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector) where TKey : notnull
    {
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));

        return this.QueryResult(false);
    }
    #endregion

    #region QueryResult
    private IMultipleQuery QueryResult(bool isSingle)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.Visitor.SelectDefault(defaultExpr);
        var sql = this.Visitor.BuildSql(true, out var readerFields);
        this.MultipleQuery.AddReader(typeof(T), sql, isSingle, this.Visitor, this.Visitor.PageNumber, this.Visitor.PageSize);
        return this.MultipleQuery;
    }
    #endregion
}