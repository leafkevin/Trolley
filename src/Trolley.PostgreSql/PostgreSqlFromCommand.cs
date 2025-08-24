using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public class PostgreSqlFromCommand<TEntity, T> : FromCommand<TEntity, T>, IPostgreSqlFromCommand<TEntity, T>
{
    #region Constructor
    public PostgreSqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlFromCommand<TEntity, T> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlFromCommand<TEntity, T>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlFromCommand<TEntity, T> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlFromCommand<TEntity, T>;
    #endregion

    #region Union/UnionAll
    public new IPostgreSqlFromCommand<TEntity, T> Union(IQuery<T> subQuery)
        => base.Union(subQuery) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> Union(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
        => base.Union(subQueryExpr) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> UnionAll(IQuery<T> subQuery)
        => base.UnionAll(subQuery) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> UnionAll(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
        => base.UnionAll(subQueryExpr) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> UnionRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr)
        => base.UnionRecursive(subQueryExpr) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> UnionAllRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr)
        => base.UnionAllRecursive(subQueryExpr) as IPostgreSqlFromCommand<TEntity, T>;
    #endregion

    #region WithTable
    public new IPostgreSqlFromCommand<TEntity, T, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlFromCommand<TEntity, T, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlFromCommand<TEntity, T, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlFromCommand<TEntity, T, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlFromCommand<TEntity, T, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlFromCommand<TEntity, T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlFromCommand<TEntity, T, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlFromCommand<TEntity, T, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlFromCommand<TEntity, T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlFromCommand<TEntity, T, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlFromCommand<TEntity, T, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlFromCommand<TEntity, T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlFromCommand<TEntity, T, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlFromCommand<TEntity, T, TOther>;
    #endregion

    #region Where
    public new IPostgreSqlFromCommand<TEntity, T> Where(Expression<Func<T, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> WherePredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T>;
    #endregion

    #region And
    public new IPostgreSqlFromCommand<TEntity, T> And(Expression<Func<T, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T> And(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> AndPredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T>;
    #endregion

    #region Or
    public new IPostgreSqlFromCommand<TEntity, T> Or(Expression<Func<T, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T> Or(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> OrPredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingCommand<TEntity, T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingCommand<TEntity, T, TGrouping>;
    #endregion

    #region OrderBy
    public new IPostgreSqlFromCommand<TEntity, T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlFromCommand<TEntity, T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlFromCommand<TEntity, T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> OrderByDynamic(Func<OrderByBuilder<T>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlFromCommand<TEntity, T>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlFromCommand<TEntity, T> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> Take(int limit)
        => base.Take(limit) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, T> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlFromCommand<TEntity, T>;
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TEntity, T> Select(string fields = "*")
        => base.Select(fields) as IPostgreSqlFromCommand<TEntity, T>;
    public new IPostgreSqlFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlFromCommand<TEntity, TTarget>;
    public new IPostgreSqlFromCommand<TEntity, TTarget> SelectTo<TTarget>(Expression<Func<T, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlFromCommand<TEntity, TTarget>;
    public new IPostgreSqlFromCommand<TEntity, TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
        => base.SelectAggregate(fieldsExpr) as IPostgreSqlFromCommand<TEntity, TTarget>;
    #endregion

    #region Distinct
    public new IPostgreSqlFromCommand<TEntity, T> Distinct()
        => base.Distinct() as IPostgreSqlFromCommand<TEntity, T>;
    #endregion

    #region OnConflict
    public IPostgreSqlFromCommand<TEntity, T> OnConflict<TUpdateFields>(Expression<Func<IPostgreSqlCreateConflictDoUpdate<T>, TUpdateFields>> fieldsAssignment)
    {
        var visitor = this.NewCreateVisitor();
        visitor.VisitSetExpression(fieldsAssignment);
        this.Visitor.IsNeedCommandTableAlias = visitor.IsUseTableAlias;
        visitor.FromSql = this.Visitor.BuildCommandSql(false, out _);
        return this;
    }
    #endregion

    #region Returnning
    public IPostgreSqlBulkCreated<T, TResult> Returning<TResult>(string fieldNames)
    {
        var sql = this.Visitor.BuildCommandSql(true, out _);
        var visitor = this.NewCreateVisitor(sql);
        visitor.Returning(fieldNames);
        return new PostgreSqlBulkCreated<T, TResult>(this.DbContext, visitor);
    }
    public IPostgreSqlBulkCreated<T, TResult> Returning<TResult>(Expression<Func<T, TResult>> fieldsSelector)
    {
        var sql = this.Visitor.BuildCommandSql(true, out _);
        var visitor = this.NewCreateVisitor(sql);
        visitor.Returning(fieldsSelector);
        return new PostgreSqlBulkCreated<T, TResult>(this.DbContext, visitor);
    }
    #endregion

    protected virtual PostgreSqlCreateVisitor NewCreateVisitor(string fromSql = null)
    {
        var createVisiter = new PostgreSqlCreateVisitor(this.DbContext, this.Visitor.TableAsStart);
        createVisiter.Tables = this.Visitor.Tables;
        createVisiter.DbParameters = this.Visitor.DbParameters;
        createVisiter.IsMultiple = this.Visitor.IsMultiple;
        createVisiter.CommandIndex = this.Visitor.CommandIndex;
        createVisiter.RefQueries = this.Visitor.RefQueries;
        createVisiter.ShardingTables = this.Visitor.ShardingTables;
        createVisiter.RefTableAliases = this.Visitor.RefTableAliases;
        createVisiter.IsRecursive = this.Visitor.IsRecursive;
        createVisiter.CteQueryObj = this.Visitor.CteQueryObj;
        createVisiter.RefFrom = this;
        createVisiter.FromSql = fromSql;
        return createVisiter;
    }
}
public class PostgreSqlFromCommand<TEntity, T1, T2> : FromCommand<TEntity, T1, T2>, IPostgreSqlFromCommand<TEntity, T1, T2>
{
    #region Constructor
    public PostgreSqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlFromCommand<TEntity, T1, T2> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlFromCommand<TEntity, T1, T2> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    #endregion

    #region WithTable
    public new IPostgreSqlFromCommand<TEntity, T1, T2, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlFromCommand<TEntity, T1, T2, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlFromCommand<TEntity, T1, T2, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlFromCommand<TEntity, T1, T2, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlFromCommand<TEntity, T1, T2, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlFromCommand<TEntity, T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlFromCommand<TEntity, T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlFromCommand<TEntity, T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, TOther>;
    #endregion

    #region Where
    public new IPostgreSqlFromCommand<TEntity, T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2> WherePredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    #endregion

    #region And
    public new IPostgreSqlFromCommand<TEntity, T1, T2> And(Expression<Func<T1, T2, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2> AndPredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    #endregion

    #region Or
    public new IPostgreSqlFromCommand<TEntity, T1, T2> Or(Expression<Func<T1, T2, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T1, T2> Or(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2> OrPredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingCommand<TEntity, T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingCommand<TEntity, T1, T2, TGrouping>;
    #endregion

    #region OrderBy
    public new IPostgreSqlFromCommand<TEntity, T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlFromCommand<TEntity, T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlFromCommand<TEntity, T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2> OrderByDynamic(Func<OrderByBuilder<T1, T2>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlFromCommand<TEntity, T1, T2> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2> Take(int limit)
        => base.Take(limit) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlFromCommand<TEntity, T1, T2>;
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlFromCommand<TEntity, TTarget>;
    public new IPostgreSqlFromCommand<TEntity, TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlFromCommand<TEntity, TTarget>;
    #endregion
}
public class PostgreSqlFromCommand<TEntity, T1, T2, T3> : FromCommand<TEntity, T1, T2, T3>, IPostgreSqlFromCommand<TEntity, T1, T2, T3>
{
    #region Constructor
    public PostgreSqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    #endregion

    #region WithTable
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, TOther>;
    #endregion

    #region Where
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> WherePredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    #endregion

    #region And
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> AndPredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    #endregion

    #region Or
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> Or(Expression<Func<T1, T2, T3, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> Or(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> OrPredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingCommand<TEntity, T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingCommand<TEntity, T1, T2, T3, TGrouping>;
    #endregion

    #region OrderBy
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> Take(int limit)
        => base.Take(limit) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlFromCommand<TEntity, T1, T2, T3>;
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlFromCommand<TEntity, TTarget>;
    public new IPostgreSqlFromCommand<TEntity, TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlFromCommand<TEntity, TTarget>;
    #endregion
}
public class PostgreSqlFromCommand<TEntity, T1, T2, T3, T4> : FromCommand<TEntity, T1, T2, T3, T4>, IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>
{
    #region Constructor
    public PostgreSqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    #endregion

    #region WithTable
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    #endregion

    #region Where
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    #endregion

    #region And
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    #endregion

    #region Or
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> Or(Expression<Func<T1, T2, T3, T4, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> Or(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping>;
    #endregion

    #region OrderBy
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> Take(int limit)
        => base.Take(limit) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4>;
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlFromCommand<TEntity, TTarget>;
    public new IPostgreSqlFromCommand<TEntity, TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlFromCommand<TEntity, TTarget>;
    #endregion
}
public class PostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> : FromCommand<TEntity, T1, T2, T3, T4, T5>, IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>
{
    #region Constructor
    public PostgreSqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    #endregion

    #region WithTable
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region WithQuery
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region RightJoin
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region Where
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    #endregion

    #region And
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    #endregion

    #region Or
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> Or(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping>;
    #endregion

    #region OrderBy
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> Take(int limit)
        => base.Take(limit) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5>;
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlFromCommand<TEntity, TTarget>;
    public new IPostgreSqlFromCommand<TEntity, TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlFromCommand<TEntity, TTarget>;
    #endregion
}
public class PostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> : FromCommand<TEntity, T1, T2, T3, T4, T5, T6>, IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>
{
    #region Constructor
    public PostgreSqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    #endregion

    #region InnerJoin
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    #endregion

    #region LeftJoin
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    #endregion

    #region RightJoin
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    #endregion

    #region Where
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    #endregion

    #region And
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    #endregion

    #region Or
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Or(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping>;
    #endregion

    #region OrderBy
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    #endregion

    #region Skip/Take/Page
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Skip(int offset)
        => base.Skip(offset) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Take(int limit)
        => base.Take(limit) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IPostgreSqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlFromCommand<TEntity, TTarget>;
    public new IPostgreSqlFromCommand<TEntity, TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IPostgreSqlFromCommand<TEntity, TTarget>;
    #endregion
}