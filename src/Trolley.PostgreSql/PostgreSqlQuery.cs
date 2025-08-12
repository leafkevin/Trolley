using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Threading;

namespace Trolley.PostgreSql;

public class PostgreSqlQuery<T> : Query<T>, IPostgreSqlQuery<T>
{
    #region Fields
    private PostgreSqlProvider dialectProvider => this.DbContext.OrmProvider as PostgreSqlProvider;
    #endregion

    #region Constructor
    public PostgreSqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlQuery<T> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlQuery<T>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlQuery<T> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlQuery<T>;
    #endregion

    #region GetShardingTableNames
    public override List<string> GetShardingTableNames(Func<string, bool> tableNameSelector)
    {
        var tableSchema = this.Visitor.Tables[0].TableSchema;
        return this.dialectProvider.GetShardingTableNames<T>(this.DbContext, tableNameSelector, tableSchema);
    }
    public override async Task<List<string>> GetShardingTableNamesAsync(Func<string, bool> tableNameSelector, CancellationToken cancellationToken = default)
    {
        var tableSchema = this.Visitor.Tables[0].TableSchema;
        return await this.dialectProvider.GetShardingTableNamesAsync<T>(this.DbContext, tableNameSelector, tableSchema);
    }
    #endregion

    #region Union/UnionAll
    public new IPostgreSqlQuery<T> Union(IQuery<T> subQuery)
        => base.Union(subQuery) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> Union(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
        => base.Union(subQueryExpr) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> UnionAll(IQuery<T> subQuery)
        => base.UnionAll(subQuery) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> UnionAll(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
        => base.UnionAll(subQueryExpr) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> UnionRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr)
        => base.UnionRecursive(subQueryExpr) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> UnionAllRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr)
        => base.UnionAllRecursive(subQueryExpr) as IPostgreSqlQuery<T>;
    #endregion

    #region WithTable
    public new IPostgreSqlQuery<T, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlQuery<T, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlQuery<T, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlQuery<T, TOther>;
    public new IPostgreSqlQuery<T, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlQuery<T, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T, TOther>;
    public new IPostgreSqlQuery<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T, TOther>;
    public new IPostgreSqlQuery<T, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T, TOther>;
    public new IPostgreSqlQuery<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T, TOther>;
    public new IPostgreSqlQuery<T, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T, TOther>;
    public new IPostgreSqlQuery<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T, TOther>;
    public new IPostgreSqlQuery<T, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T, TOther>;
    #endregion

    #region Include
    public new IPostgreSqlIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector)
        => base.Include(memberSelector) as IPostgreSqlIncludableQuery<T, TMember>;
    public new IPostgreSqlIncludableQuery<T, TElement> IncludeMany<TElement>(Expression<Func<T, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => base.IncludeMany(memberSelector, filter) as IPostgreSqlIncludableQuery<T, TElement>;
    #endregion

    #region Where
    public new IPostgreSqlQuery<T> Where(Expression<Func<T, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> WherePredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlQuery<T>;
    #endregion

    #region And
    public new IPostgreSqlQuery<T> And(Expression<Func<T, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlQuery<T> And(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> AndPredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlQuery<T>;
    #endregion

    #region Or
    public new IPostgreSqlQuery<T> Or(Expression<Func<T, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlQuery<T> Or(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> OrPredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlQuery<T>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingQuery<T, TGrouping>;
    #endregion

    #region DistinctOn
    public IPostgreSqlDistinctOnQuery<T, TDistinctOn> DistinctOn<TDistinctOn>(Expression<Func<T, TDistinctOn>> fieldsSelector)
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return new PostgreSqlDistinctOnQuery<T, TDistinctOn>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public new IPostgreSqlQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlQuery<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlQuery<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> OrderByDynamic(Func<OrderByBuilder<T>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlQuery<T>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlQuery<T> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> Take(int limit)
        => base.Take(limit) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlQuery<T>;
    #endregion

    #region Select
    public new IPostgreSqlQuery<T> Select()
        => base.Select() as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectTo<TTarget>(Expression<Func<T, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
        => base.SelectAggregate(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    #endregion

    #region Distinct
    public new IPostgreSqlQuery<T> Distinct()
        => base.Distinct() as IPostgreSqlQuery<T>;
    #endregion
}
public class PostgreSqlQuery<T1, T2> : Query<T1, T2>, IPostgreSqlQuery<T1, T2>
{
    #region Constructor
    public PostgreSqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlQuery<T1, T2> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlQuery<T1, T2>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlQuery<T1, T2> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlQuery<T1, T2>;
    #endregion

    #region WithTable
    public new IPostgreSqlQuery<T1, T2, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlQuery<T1, T2, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlQuery<T1, T2, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlQuery<T1, T2, TOther>;
    public new IPostgreSqlQuery<T1, T2, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlQuery<T1, T2, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlQuery<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, TOther>;
    public new IPostgreSqlQuery<T1, T2, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, TOther>;
    public new IPostgreSqlQuery<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, TOther>;
    public new IPostgreSqlQuery<T1, T2, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, TOther>;
    public new IPostgreSqlQuery<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlQuery<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, TOther>;
    public new IPostgreSqlQuery<T1, T2, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, TOther>;
    public new IPostgreSqlQuery<T1, T2, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, TOther>;
    #endregion

    #region Include
    public new IPostgreSqlIncludableQuery<T1, T2, TMember> Include<TMember>(Expression<Func<T1, T2, TMember>> memberSelector)
        => base.Include(memberSelector) as IPostgreSqlIncludableQuery<T1, T2, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => base.IncludeMany(memberSelector, filter) as IPostgreSqlIncludableQuery<T1, T2, TElement>;
    #endregion

    #region Where
    public new IPostgreSqlQuery<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlQuery<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2> WherePredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2>;
    #endregion

    #region And
    public new IPostgreSqlQuery<T1, T2> And(Expression<Func<T1, T2, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlQuery<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2> AndPredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2>;
    #endregion

    #region Or
    public new IPostgreSqlQuery<T1, T2> Or(Expression<Func<T1, T2, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlQuery<T1, T2> Or(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2> OrPredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingQuery<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingQuery<T1, T2, TGrouping>;
    #endregion

    #region DistinctOn
    public IPostgreSqlDistinctOnQuery<T1, T2, TDistinctOn> DistinctOn<TDistinctOn>(Expression<Func<T1, T2, TDistinctOn>> fieldsSelector)
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return new PostgreSqlDistinctOnQuery<T1, T2, TDistinctOn>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public new IPostgreSqlQuery<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2> OrderByDynamic(Func<OrderByBuilder<T1, T2>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlQuery<T1, T2>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlQuery<T1, T2> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2> Take(int limit)
        => base.Take(limit) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlQuery<T1, T2>;
    #endregion

    #region Select 
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlQuery<T1, T2, T3> : Query<T1, T2, T3>, IPostgreSqlQuery<T1, T2, T3>
{
    #region Constructor
    public PostgreSqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlQuery<T1, T2, T3> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlQuery<T1, T2, T3>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlQuery<T1, T2, T3> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlQuery<T1, T2, T3>;
    #endregion

    #region WithTable
    public new IPostgreSqlQuery<T1, T2, T3, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlQuery<T1, T2, T3, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlQuery<T1, T2, T3, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlQuery<T1, T2, T3, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlQuery<T1, T2, T3, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlQuery<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlQuery<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, TOther>;
    #endregion

    #region Include
    public new IPostgreSqlIncludableQuery<T1, T2, T3, TMember> Include<TMember>(Expression<Func<T1, T2, T3, TMember>> memberSelector)
        => base.Include(memberSelector) as IPostgreSqlIncludableQuery<T1, T2, T3, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => base.IncludeMany(memberSelector, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, TElement>;
    #endregion

    #region Where
    public new IPostgreSqlQuery<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3> WherePredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3>;
    #endregion

    #region And
    public new IPostgreSqlQuery<T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3> AndPredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3>;
    #endregion

    #region Or
    public new IPostgreSqlQuery<T1, T2, T3> Or(Expression<Func<T1, T2, T3, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3> Or(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3> OrPredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingQuery<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, TGrouping>;
    #endregion

    #region DistinctOn
    public IPostgreSqlDistinctOnQuery<T1, T2, T3, TDistinctOn> DistinctOn<TDistinctOn>(Expression<Func<T1, T2, T3, TDistinctOn>> fieldsSelector)
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return new PostgreSqlDistinctOnQuery<T1, T2, T3, TDistinctOn>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public new IPostgreSqlQuery<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlQuery<T1, T2, T3>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlQuery<T1, T2, T3> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3> Take(int limit)
        => base.Take(limit) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlQuery<T1, T2, T3>;
    #endregion

    #region Select 
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlQuery<T1, T2, T3, T4> : Query<T1, T2, T3, T4>, IPostgreSqlQuery<T1, T2, T3, T4>
{
    #region Constructor
    public PostgreSqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlQuery<T1, T2, T3, T4> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlQuery<T1, T2, T3, T4>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlQuery<T1, T2, T3, T4> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlQuery<T1, T2, T3, T4>;
    #endregion

    #region WithTable
    public new IPostgreSqlQuery<T1, T2, T3, T4, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlQuery<T1, T2, T3, T4, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlQuery<T1, T2, T3, T4, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlQuery<T1, T2, T3, T4, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlQuery<T1, T2, T3, T4, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, TOther>;
    #endregion

    #region Include
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> memberSelector)
        => base.Include(memberSelector) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => base.IncludeMany(memberSelector, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TElement>;
    #endregion

    #region Where
    public new IPostgreSqlQuery<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4>;
    #endregion

    #region And
    public new IPostgreSqlQuery<T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4>;
    #endregion

    #region Or
    public new IPostgreSqlQuery<T1, T2, T3, T4> Or(Expression<Func<T1, T2, T3, T4, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4> Or(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, TGrouping>;
    #endregion

    #region DistinctOn
    public IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, TDistinctOn> DistinctOn<TDistinctOn>(Expression<Func<T1, T2, T3, T4, TDistinctOn>> fieldsSelector)
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return new PostgreSqlDistinctOnQuery<T1, T2, T3, T4, TDistinctOn>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public new IPostgreSqlQuery<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlQuery<T1, T2, T3, T4>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlQuery<T1, T2, T3, T4> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4> Take(int limit)
        => base.Take(limit) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlQuery<T1, T2, T3, T4>;
    #endregion

    #region Select 
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlQuery<T1, T2, T3, T4, T5> : Query<T1, T2, T3, T4, T5>, IPostgreSqlQuery<T1, T2, T3, T4, T5>
{
    #region Constructor
    public PostgreSqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    #endregion

    #region WithTable
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region Include
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> memberSelector)
        => base.Include(memberSelector) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => base.IncludeMany(memberSelector, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TElement>;
    #endregion

    #region Where
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    #endregion

    #region And
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    #endregion

    #region Or
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> Or(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, TGrouping>;
    #endregion

    #region DistinctOn
    public IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, TDistinctOn> DistinctOn<TDistinctOn>(Expression<Func<T1, T2, T3, T4, T5, TDistinctOn>> fieldsSelector)
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return new PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, TDistinctOn>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> Take(int limit)
        => base.Take(limit) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    #endregion

    #region Select 
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlQuery<T1, T2, T3, T4, T5, T6> : Query<T1, T2, T3, T4, T5, T6>, IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>
{
    #region Constructor
    public PostgreSqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region WithTable
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, TOther>;
    #endregion

    #region Include
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> memberSelector)
        => base.Include(memberSelector) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => base.IncludeMany(memberSelector, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TElement>;
    #endregion

    #region Where
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region And
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region Or
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> Or(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>;
    #endregion

    #region DistinctOn
    public IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, TDistinctOn> DistinctOn<TDistinctOn>(Expression<Func<T1, T2, T3, T4, T5, T6, TDistinctOn>> fieldsSelector)
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return new PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, TDistinctOn>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> Take(int limit)
        => base.Take(limit) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region Select 
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> : Query<T1, T2, T3, T4, T5, T6, T7>, IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>
{
    #region Constructor
    public PostgreSqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    #endregion

    #region WithTable
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, TOther>;
    #endregion

    #region Include
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> memberSelector)
        => base.Include(memberSelector) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => base.IncludeMany(memberSelector, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElement>;
    #endregion

    #region Where
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    #endregion

    #region And
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    #endregion

    #region Or
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>;
    #endregion

    #region DistinctOn
    public IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, TDistinctOn> DistinctOn<TDistinctOn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TDistinctOn>> fieldsSelector)
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return new PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, TDistinctOn>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> Take(int limit)
        => base.Take(limit) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    #endregion

    #region Select 
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> : Query<T1, T2, T3, T4, T5, T6, T7, T8>, IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>
{
    #region Constructor
    public PostgreSqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    #endregion

    #region WithTable
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>;
    #endregion

    #region Include
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> memberSelector)
        => base.Include(memberSelector) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => base.IncludeMany(memberSelector, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElement>;
    #endregion

    #region Where
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    #endregion

    #region And
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    #endregion

    #region Or
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>;
    #endregion

    #region DistinctOn
    public IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, TDistinctOn> DistinctOn<TDistinctOn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TDistinctOn>> fieldsSelector)
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return new PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, TDistinctOn>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> Take(int limit)
        => base.Take(limit) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    #endregion

    #region Select 
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>, IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    #region Constructor
    public PostgreSqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    #endregion

    #region WithTable
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>;
    #endregion

    #region Include
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> memberSelector)
        => base.Include(memberSelector) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => base.IncludeMany(memberSelector, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElement>;
    #endregion

    #region Where
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    #endregion

    #region And
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    #endregion

    #region Or
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>;
    #endregion

    #region DistinctOn
    public IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDistinctOn> DistinctOn<TDistinctOn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDistinctOn>> fieldsSelector)
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return new PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDistinctOn>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Take(int limit)
        => base.Take(limit) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    #endregion

    #region Select 
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    #region Constructor
    public PostgreSqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    #endregion

    #region WithTable
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>;
    #endregion

    #region Include
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> memberSelector)
        => base.Include(memberSelector) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => base.IncludeMany(memberSelector, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElement>;
    #endregion

    #region Where
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    #endregion

    #region And
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    #endregion

    #region Or
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>;
    #endregion

    #region DistinctOn
    public IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDistinctOn> DistinctOn<TDistinctOn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDistinctOn>> fieldsSelector)
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return new PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDistinctOn>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Take(int limit)
        => base.Take(limit) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    #endregion

    #region Select 
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    #region Constructor
    public PostgreSqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    #endregion

    #region WithTable
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>;
    #endregion

    #region Include
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> memberSelector)
        => base.Include(memberSelector) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => base.IncludeMany(memberSelector, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElement>;
    #endregion

    #region Where
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    #endregion

    #region And
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    #endregion

    #region Or
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>;
    #endregion

    #region DistinctOn
    public IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDistinctOn> DistinctOn<TDistinctOn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDistinctOn>> fieldsSelector)
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return new PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDistinctOn>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Take(int limit)
        => base.Take(limit) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;
    #endregion

    #region Select 
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    #region Constructor
    public PostgreSqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    #endregion

    #region WithTable
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>;
    #endregion

    #region Include
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> memberSelector)
        => base.Include(memberSelector) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => base.IncludeMany(memberSelector, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElement>;
    #endregion

    #region Where
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    #endregion

    #region And
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    #endregion

    #region Or
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>;
    #endregion

    #region DistinctOn
    public IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDistinctOn> DistinctOn<TDistinctOn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDistinctOn>> fieldsSelector)
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return new PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDistinctOn>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Take(int limit)
        => base.Take(limit) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;
    #endregion

    #region Select 
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    #region Constructor
    public PostgreSqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    #endregion

    #region WithTable
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>;
    #endregion

    #region Include
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> memberSelector)
        => base.Include(memberSelector) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => base.IncludeMany(memberSelector, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElement>;
    #endregion

    #region Where
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    #endregion

    #region And
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    #endregion

    #region Or
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>;
    #endregion

    #region DistinctOn
    public IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDistinctOn> DistinctOn<TDistinctOn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDistinctOn>> fieldsSelector)
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return new PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDistinctOn>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Take(int limit)
        => base.Take(limit) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;
    #endregion

    #region Select 
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    #region Constructor
    public PostgreSqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    #endregion

    #region WithTable
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>;
    #endregion

    #region Include
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> memberSelector)
        => base.Include(memberSelector) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => base.IncludeMany(memberSelector, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElement>;
    #endregion

    #region Where
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    #endregion

    #region And
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    #endregion

    #region Or
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>;
    #endregion

    #region DistinctOn
    public IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDistinctOn> DistinctOn<TDistinctOn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDistinctOn>> fieldsSelector)
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return new PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDistinctOn>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Take(int limit)
        => base.Take(limit) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;
    #endregion

    #region Select 
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    #region Constructor
    public PostgreSqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    #endregion

    #region WithTable
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>;
    #endregion

    #region Include
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> memberSelector)
        => base.Include(memberSelector) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => base.IncludeMany(memberSelector, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElement>;
    #endregion

    #region Where
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    #endregion

    #region And
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    #endregion

    #region Or
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>;
    #endregion

    #region DistinctOn
    public IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDistinctOn> DistinctOn<TDistinctOn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDistinctOn>> fieldsSelector)
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return new PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDistinctOn>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Take(int limit)
        => base.Take(limit) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;
    #endregion

    #region Select 
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlQuery<TTarget>;
    #endregion
}
public class PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
{
    #region Constructor
    public PostgreSqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    #endregion

    #region RightJoin
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    #endregion

    #region Include
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> memberSelector)
        => base.Include(memberSelector) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
        => base.IncludeMany(memberSelector, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TElement>;
    #endregion

    #region Where
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    #endregion

    #region And
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    #endregion

    #region Or
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>;
    #endregion

    #region DistinctOn
    public IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TDistinctOn> DistinctOn<TDistinctOn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TDistinctOn>> fieldsSelector)
    {
        var dialectVisitor = this.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return new PostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TDistinctOn>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Take(int limit)
        => base.Take(limit) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>;
    #endregion

    #region Select 
    public new IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlQuery<TTarget>;
    public new IPostgreSqlQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlQuery<TTarget>;
    #endregion
}