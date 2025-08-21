using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public class MySqlFromCommand<T> : FromCommand<T>, IMySqlFromCommand<T>
{
    #region Constructor
    public MySqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IMySqlFromCommand<T> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<T> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<T> UseTableByRange(params object[] fieldValues)
        => base.UseTableByRange(fieldValues) as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<T> UseUnionShardingTable()
        => base.UseUnionShardingTable() as IMySqlFromCommand<T>;
    #endregion

    #region UseTableSchema
    public new IMySqlFromCommand<T> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IMySqlFromCommand<T>;
    #endregion

    #region Union/UnionAll
    public new IMySqlFromCommand<T> Union(IQuery<T> subQuery)
        => base.Union(subQuery) as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<T> Union(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
        => base.Union(subQueryExpr) as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<T> UnionAll(IQuery<T> subQuery)
        => base.UnionAll(subQuery) as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<T> UnionAll(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
        => base.UnionAll(subQueryExpr) as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<T> UnionRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr)
        => base.UnionRecursive(subQueryExpr) as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<T> UnionAllRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr)
        => base.UnionAllRecursive(subQueryExpr) as IMySqlFromCommand<T>;
    #endregion

    #region WithTable
    public new IMySqlFromCommand<T, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IMySqlFromCommand<T, TOther>;
    #endregion

    #region WithQuery
    public new IMySqlFromCommand<T, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IMySqlFromCommand<T, TOther>;
    public new IMySqlFromCommand<T, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IMySqlFromCommand<T, TOther>;
    #endregion

    #region InnerJoin
    public new IMySqlFromCommand<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IMySqlFromCommand<T, TOther>;
    public new IMySqlFromCommand<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IMySqlFromCommand<T, TOther>;
    public new IMySqlFromCommand<T, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IMySqlFromCommand<T, TOther>;
    #endregion

    #region LeftJoin
    public new IMySqlFromCommand<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IMySqlFromCommand<T, TOther>;
    public new IMySqlFromCommand<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IMySqlFromCommand<T, TOther>;
    public new IMySqlFromCommand<T, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IMySqlFromCommand<T, TOther>;
    #endregion

    #region RightJoin
    public new IMySqlFromCommand<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IMySqlFromCommand<T, TOther>;
    public new IMySqlFromCommand<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IMySqlFromCommand<T, TOther>;
    public new IMySqlFromCommand<T, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IMySqlFromCommand<T, TOther>;
    #endregion

    #region Where
    public new IMySqlFromCommand<T> Where(Expression<Func<T, bool>> predicate)
        => this.Where(true, predicate);
    public new IMySqlFromCommand<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<T> WherePredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IMySqlFromCommand<T>;
    #endregion

    #region And
    public new IMySqlFromCommand<T> And(Expression<Func<T, bool>> predicate)
        => this.And(true, predicate);
    public new IMySqlFromCommand<T> And(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<T> AndPredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IMySqlFromCommand<T>;
    #endregion

    #region Or
    public new IMySqlFromCommand<T> Or(Expression<Func<T, bool>> predicate)
        => this.Or(true, predicate);
    public new IMySqlFromCommand<T> Or(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<T> OrPredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IMySqlFromCommand<T>;
    #endregion

    #region GroupBy
    public new IMySqlGroupingCommand<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IMySqlGroupingCommand<T, TGrouping>;
    #endregion

    #region OrderBy
    public new IMySqlFromCommand<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IMySqlFromCommand<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IMySqlFromCommand<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<T> OrderByDynamic(Func<OrderByBuilder<T>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IMySqlFromCommand<T>;
    #endregion

    #region Skip/Take/Page
    public new IMySqlFromCommand<T> Skip(int offset)
        => base.Skip(offset) as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<T> Take(int limit)
        => base.Take(limit) as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<T> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IMySqlFromCommand<T>;
    #endregion

    #region Select
    public new IMySqlFromCommand<T> Select(string fields = "*")
        => base.Select(fields) as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IMySqlFromCommand<TTarget>;
    public new IMySqlFromCommand<TTarget> SelectTo<TTarget>(Expression<Func<T, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IMySqlFromCommand<TTarget>;
    public new IMySqlFromCommand<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
        => base.SelectAggregate(fieldsExpr) as IMySqlFromCommand<TTarget>;
    #endregion

    #region Distinct
    public new IMySqlFromCommand<T> Distinct()
        => base.Distinct() as IMySqlFromCommand<T>;
    #endregion

    #region OnDuplicateKeyUpdate
    public IMySqlFromCommand<T> OnDuplicateKeyUpdate<TUpdateFields>(TUpdateFields updateObj)
    {
        var sql = this.Visitor.BuildCommandSql(true, out _);
        var visitor = this.NewCreateVisitor(sql);
        visitor.OnDuplicateKeyUpdate(updateObj);
        return this;
    }
    public IMySqlFromCommand<T> OnDuplicateKeyUpdate<TUpdateFields>(Expression<Func<IMySqlCreateDuplicateKeyUpdate<T>, TUpdateFields>> fieldsAssignment)
    {
        var sql = this.Visitor.BuildCommandSql(true, out _);
        var visitor = this.NewCreateVisitor(sql);
        visitor.OnDuplicateKeyUpdate(fieldsAssignment);
        return this;
    }
    #endregion

    #region Returnning
    public IMySqlBulkCreated<T, TResult> Returning<TResult>(string fieldNames)
    {
        var sql = this.Visitor.BuildCommandSql(true, out _);
        var visitor = this.NewCreateVisitor(sql);
        visitor.Returning(fieldNames);
        return new MySqlBulkCreated<T, TResult>(this.DbContext, visitor);
    }
    public IMySqlBulkCreated<T, TResult> Returning<TResult>(Expression<Func<T, TResult>> fieldsSelector)
    {
        var sql = this.Visitor.BuildCommandSql(true, out _);
        var visitor = this.NewCreateVisitor(sql);
        visitor.Returning(fieldsSelector);
        return new MySqlBulkCreated<T, TResult>(this.DbContext, visitor);
    }
    #endregion

    protected virtual MySqlCreateVisitor NewCreateVisitor(string fromSql = null)
    {
        var createVisiter = new MySqlCreateVisitor(this.DbContext, this.Visitor.TableAsStart);
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
public class MySqlFromCommand<T1, T2> : FromCommand<T1, T2>, IMySqlFromCommand<T1, T2>
{
    #region Constructor
    public MySqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IMySqlFromCommand<T1, T2> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IMySqlFromCommand<T1, T2>;
    public new IMySqlFromCommand<T1, T2> UseTableMap(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap(tableNameGetter) as IMySqlFromCommand<T1, T2>;
    public new IMySqlFromCommand<T1, T2> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IMySqlFromCommand<T1, T2>;
    public new IMySqlFromCommand<T1, T2> UseTableByRange(params object[] fieldValues)
        => base.UseTableByRange(fieldValues) as IMySqlFromCommand<T1, T2>;
    public new IMySqlFromCommand<T1, T2> UseUnionShardingTable()
        => base.UseUnionShardingTable() as IMySqlFromCommand<T1, T2>;
    #endregion

    #region UseTableSchema
    public new IMySqlFromCommand<T1, T2> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IMySqlFromCommand<T1, T2>;
    #endregion

    #region WithTable
    public new IMySqlFromCommand<T1, T2, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IMySqlFromCommand<T1, T2, TOther>;
    #endregion

    #region WithQuery
    public new IMySqlFromCommand<T1, T2, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IMySqlFromCommand<T1, T2, TOther>;
    public new IMySqlFromCommand<T1, T2, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IMySqlFromCommand<T1, T2, TOther>;
    #endregion

    #region InnerJoin
    public new IMySqlFromCommand<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn)
        => base.InnerJoin(joinOn) as IMySqlFromCommand<T1, T2>;
    public new IMySqlFromCommand<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IMySqlFromCommand<T1, T2, TOther>;
    public new IMySqlFromCommand<T1, T2, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IMySqlFromCommand<T1, T2, TOther>;
    public new IMySqlFromCommand<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IMySqlFromCommand<T1, T2, TOther>;
    #endregion

    #region LeftJoin
    public new IMySqlFromCommand<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn)
        => base.LeftJoin(joinOn) as IMySqlFromCommand<T1, T2>;
    public new IMySqlFromCommand<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IMySqlFromCommand<T1, T2, TOther>;
    public new IMySqlFromCommand<T1, T2, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IMySqlFromCommand<T1, T2, TOther>;
    public new IMySqlFromCommand<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IMySqlFromCommand<T1, T2, TOther>;
    #endregion

    #region RightJoin
    public new IMySqlFromCommand<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn)
        => base.RightJoin(joinOn) as IMySqlFromCommand<T1, T2>;
    public new IMySqlFromCommand<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IMySqlFromCommand<T1, T2, TOther>;
    public new IMySqlFromCommand<T1, T2, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IMySqlFromCommand<T1, T2, TOther>;
    public new IMySqlFromCommand<T1, T2, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IMySqlFromCommand<T1, T2, TOther>;
    #endregion

    #region Where
    public new IMySqlFromCommand<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
        => this.Where(true, predicate);
    public new IMySqlFromCommand<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T1, T2>;
    public new IMySqlFromCommand<T1, T2> WherePredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IMySqlFromCommand<T1, T2>;
    #endregion

    #region And
    public new IMySqlFromCommand<T1, T2> And(Expression<Func<T1, T2, bool>> predicate)
        => this.And(true, predicate);
    public new IMySqlFromCommand<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T1, T2>;
    public new IMySqlFromCommand<T1, T2> AndPredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IMySqlFromCommand<T1, T2>;
    #endregion

    #region Or
    public new IMySqlFromCommand<T1, T2> Or(Expression<Func<T1, T2, bool>> predicate)
        => this.Or(true, predicate);
    public new IMySqlFromCommand<T1, T2> Or(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T1, T2>;
    public new IMySqlFromCommand<T1, T2> OrPredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IMySqlFromCommand<T1, T2>;
    #endregion

    #region GroupBy
    public new IMySqlGroupingCommand<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IMySqlGroupingCommand<T1, T2, TGrouping>;
    #endregion

    #region OrderBy
    public new IMySqlFromCommand<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IMySqlFromCommand<T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IMySqlFromCommand<T1, T2>;
    public new IMySqlFromCommand<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IMySqlFromCommand<T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IMySqlFromCommand<T1, T2>;
    public new IMySqlFromCommand<T1, T2> OrderByDynamic(Func<OrderByBuilder<T1, T2>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IMySqlFromCommand<T1, T2>;
    #endregion

    #region Skip/Take/Page
    public new IMySqlFromCommand<T1, T2> Skip(int offset)
        => base.Skip(offset) as IMySqlFromCommand<T1, T2>;
    public new IMySqlFromCommand<T1, T2> Take(int limit)
        => base.Take(limit) as IMySqlFromCommand<T1, T2>;
    public new IMySqlFromCommand<T1, T2> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IMySqlFromCommand<T1, T2>;
    #endregion

    #region Select
    public new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IMySqlFromCommand<TTarget>;
    public new IMySqlFromCommand<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IMySqlFromCommand<TTarget>;
    #endregion
}
public class MySqlFromCommand<T1, T2, T3> : FromCommand<T1, T2, T3>, IMySqlFromCommand<T1, T2, T3>
{
    #region Constructor
    public MySqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IMySqlFromCommand<T1, T2, T3> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IMySqlFromCommand<T1, T2, T3>;
    public new IMySqlFromCommand<T1, T2, T3> UseTableMap(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap(tableNameGetter) as IMySqlFromCommand<T1, T2, T3>;
    public new IMySqlFromCommand<T1, T2, T3> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IMySqlFromCommand<T1, T2, T3>;
    public new IMySqlFromCommand<T1, T2, T3> UseTableByRange(params object[] fieldValues)
        => base.UseTableByRange(fieldValues) as IMySqlFromCommand<T1, T2, T3>;
    public new IMySqlFromCommand<T1, T2, T3> UseUnionShardingTable()
        => base.UseUnionShardingTable() as IMySqlFromCommand<T1, T2, T3>;
    #endregion

    #region UseTableSchema
    public new IMySqlFromCommand<T1, T2, T3> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IMySqlFromCommand<T1, T2, T3>;
    #endregion

    #region WithTable
    public new IMySqlFromCommand<T1, T2, T3, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IMySqlFromCommand<T1, T2, T3, TOther>;
    #endregion

    #region WithQuery
    public new IMySqlFromCommand<T1, T2, T3, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IMySqlFromCommand<T1, T2, T3, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IMySqlFromCommand<T1, T2, T3, TOther>;
    #endregion

    #region InnerJoin
    public new IMySqlFromCommand<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
        => base.InnerJoin(joinOn) as IMySqlFromCommand<T1, T2, T3>;
    public new IMySqlFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IMySqlFromCommand<T1, T2, T3, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IMySqlFromCommand<T1, T2, T3, TOther>;
    #endregion

    #region LeftJoin
    public new IMySqlFromCommand<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
        => base.LeftJoin(joinOn) as IMySqlFromCommand<T1, T2, T3>;
    public new IMySqlFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IMySqlFromCommand<T1, T2, T3, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IMySqlFromCommand<T1, T2, T3, TOther>;
    #endregion

    #region RightJoin
    public new IMySqlFromCommand<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
        => base.RightJoin(joinOn) as IMySqlFromCommand<T1, T2, T3>;
    public new IMySqlFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IMySqlFromCommand<T1, T2, T3, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IMySqlFromCommand<T1, T2, T3, TOther>;
    #endregion

    #region Where
    public new IMySqlFromCommand<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
        => this.Where(true, predicate);
    public new IMySqlFromCommand<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T1, T2, T3>;
    public new IMySqlFromCommand<T1, T2, T3> WherePredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IMySqlFromCommand<T1, T2, T3>;
    #endregion

    #region And
    public new IMySqlFromCommand<T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate)
        => this.And(true, predicate);
    public new IMySqlFromCommand<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T1, T2, T3>;
    public new IMySqlFromCommand<T1, T2, T3> AndPredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IMySqlFromCommand<T1, T2, T3>;
    #endregion

    #region Or
    public new IMySqlFromCommand<T1, T2, T3> Or(Expression<Func<T1, T2, T3, bool>> predicate)
        => this.Or(true, predicate);
    public new IMySqlFromCommand<T1, T2, T3> Or(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T1, T2, T3>;
    public new IMySqlFromCommand<T1, T2, T3> OrPredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IMySqlFromCommand<T1, T2, T3>;
    #endregion

    #region GroupBy
    public new IMySqlGroupingCommand<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IMySqlGroupingCommand<T1, T2, T3, TGrouping>;
    #endregion

    #region OrderBy
    public new IMySqlFromCommand<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IMySqlFromCommand<T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IMySqlFromCommand<T1, T2, T3>;
    public new IMySqlFromCommand<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IMySqlFromCommand<T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IMySqlFromCommand<T1, T2, T3>;
    public new IMySqlFromCommand<T1, T2, T3> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IMySqlFromCommand<T1, T2, T3>;
    #endregion

    #region Skip/Take/Page
    public new IMySqlFromCommand<T1, T2, T3> Skip(int offset)
        => base.Skip(offset) as IMySqlFromCommand<T1, T2, T3>;
    public new IMySqlFromCommand<T1, T2, T3> Take(int limit)
        => base.Take(limit) as IMySqlFromCommand<T1, T2, T3>;
    public new IMySqlFromCommand<T1, T2, T3> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IMySqlFromCommand<T1, T2, T3>;
    #endregion

    #region Select
    public new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IMySqlFromCommand<TTarget>;
    public new IMySqlFromCommand<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IMySqlFromCommand<TTarget>;
    #endregion
}
public class MySqlFromCommand<T1, T2, T3, T4> : FromCommand<T1, T2, T3, T4>, IMySqlFromCommand<T1, T2, T3, T4>
{
    #region Constructor
    public MySqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IMySqlFromCommand<T1, T2, T3, T4> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IMySqlFromCommand<T1, T2, T3, T4>;
    public new IMySqlFromCommand<T1, T2, T3, T4> UseTableMap(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap(tableNameGetter) as IMySqlFromCommand<T1, T2, T3, T4>;
    public new IMySqlFromCommand<T1, T2, T3, T4> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IMySqlFromCommand<T1, T2, T3, T4>;
    public new IMySqlFromCommand<T1, T2, T3, T4> UseTableByRange(params object[] fieldValues)
        => base.UseTableByRange(fieldValues) as IMySqlFromCommand<T1, T2, T3, T4>;
    public new IMySqlFromCommand<T1, T2, T3, T4> UseUnionShardingTable()
        => base.UseUnionShardingTable() as IMySqlFromCommand<T1, T2, T3, T4>;
    #endregion

    #region UseTableSchema
    public new IMySqlFromCommand<T1, T2, T3, T4> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IMySqlFromCommand<T1, T2, T3, T4>;
    #endregion

    #region WithTable
    public new IMySqlFromCommand<T1, T2, T3, T4, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IMySqlFromCommand<T1, T2, T3, T4, TOther>;
    #endregion

    #region WithQuery
    public new IMySqlFromCommand<T1, T2, T3, T4, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IMySqlFromCommand<T1, T2, T3, T4, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, T4, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IMySqlFromCommand<T1, T2, T3, T4, TOther>;
    #endregion

    #region InnerJoin
    public new IMySqlFromCommand<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
        => base.InnerJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, T4>;
    public new IMySqlFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, T4, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IMySqlFromCommand<T1, T2, T3, T4, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IMySqlFromCommand<T1, T2, T3, T4, TOther>;
    #endregion

    #region LeftJoin
    public new IMySqlFromCommand<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
        => base.LeftJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, T4>;
    public new IMySqlFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, T4, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IMySqlFromCommand<T1, T2, T3, T4, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IMySqlFromCommand<T1, T2, T3, T4, TOther>;
    #endregion

    #region RightJoin
    public new IMySqlFromCommand<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
        => base.RightJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, T4>;
    public new IMySqlFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, T4, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IMySqlFromCommand<T1, T2, T3, T4, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IMySqlFromCommand<T1, T2, T3, T4, TOther>;
    #endregion

    #region Where
    public new IMySqlFromCommand<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
        => this.Where(true, predicate);
    public new IMySqlFromCommand<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T1, T2, T3, T4>;
    public new IMySqlFromCommand<T1, T2, T3, T4> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IMySqlFromCommand<T1, T2, T3, T4>;
    #endregion

    #region And
    public new IMySqlFromCommand<T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate)
        => this.And(true, predicate);
    public new IMySqlFromCommand<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T1, T2, T3, T4>;
    public new IMySqlFromCommand<T1, T2, T3, T4> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IMySqlFromCommand<T1, T2, T3, T4>;
    #endregion

    #region Or
    public new IMySqlFromCommand<T1, T2, T3, T4> Or(Expression<Func<T1, T2, T3, T4, bool>> predicate)
        => this.Or(true, predicate);
    public new IMySqlFromCommand<T1, T2, T3, T4> Or(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T1, T2, T3, T4>;
    public new IMySqlFromCommand<T1, T2, T3, T4> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IMySqlFromCommand<T1, T2, T3, T4>;
    #endregion

    #region GroupBy
    public new IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping>;
    #endregion

    #region OrderBy
    public new IMySqlFromCommand<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IMySqlFromCommand<T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IMySqlFromCommand<T1, T2, T3, T4>;
    public new IMySqlFromCommand<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IMySqlFromCommand<T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IMySqlFromCommand<T1, T2, T3, T4>;
    public new IMySqlFromCommand<T1, T2, T3, T4> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IMySqlFromCommand<T1, T2, T3, T4>;
    #endregion

    #region Skip/Take/Page
    public new IMySqlFromCommand<T1, T2, T3, T4> Skip(int offset)
        => base.Skip(offset) as IMySqlFromCommand<T1, T2, T3, T4>;
    public new IMySqlFromCommand<T1, T2, T3, T4> Take(int limit)
        => base.Take(limit) as IMySqlFromCommand<T1, T2, T3, T4>;
    public new IMySqlFromCommand<T1, T2, T3, T4> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IMySqlFromCommand<T1, T2, T3, T4>;
    #endregion

    #region Select
    public new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IMySqlFromCommand<TTarget>;
    public new IMySqlFromCommand<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IMySqlFromCommand<TTarget>;
    #endregion
}
public class MySqlFromCommand<T1, T2, T3, T4, T5> : FromCommand<T1, T2, T3, T4, T5>, IMySqlFromCommand<T1, T2, T3, T4, T5>
{
    #region Constructor
    public MySqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> UseTableMap(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap(tableNameGetter) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> UseTableByRange(params object[] fieldValues)
        => base.UseTableByRange(fieldValues) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> UseUnionShardingTable()
        => base.UseUnionShardingTable() as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    #endregion

    #region UseTableSchema
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    #endregion

    #region WithTable
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, TOther> WithTable<TOther>()
        => base.WithTable<TOther>() as IMySqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region WithQuery
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
        => base.WithQuery(subQuery) as IMySqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
        => base.WithQuery(subQueryExpr) as IMySqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region InnerJoin
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
        => base.InnerJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IMySqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.InnerJoin(subQueryExpr, joinOn) as IMySqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region LeftJoin
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
        => base.LeftJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IMySqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.LeftJoin(subQueryExpr, joinOn) as IMySqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region RightJoin
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
        => base.RightJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IMySqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.RightJoin(subQueryExpr, joinOn) as IMySqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region Where
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        => this.Where(true, predicate);
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    #endregion

    #region And
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        => this.And(true, predicate);
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    #endregion

    #region Or
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> Or(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        => this.Or(true, predicate);
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    #endregion

    #region GroupBy
    public new IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping>;
    #endregion

    #region OrderBy
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    #endregion

    #region Skip/Take/Page
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> Skip(int offset)
        => base.Skip(offset) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> Take(int limit)
        => base.Take(limit) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    #endregion

    #region Select
    public new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IMySqlFromCommand<TTarget>;
    public new IMySqlFromCommand<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IMySqlFromCommand<TTarget>;
    #endregion
}
public class MySqlFromCommand<T1, T2, T3, T4, T5, T6> : FromCommand<T1, T2, T3, T4, T5, T6>, IMySqlFromCommand<T1, T2, T3, T4, T5, T6>
{
    #region Constructor
    public MySqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> UseTableMap(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap(tableNameGetter) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> UseTableByRange(params object[] fieldValues)
        => base.UseTableByRange(fieldValues) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> UseUnionShardingTable()
        => base.UseUnionShardingTable() as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region UseTableSchema
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region InnerJoin
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
        => base.InnerJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region LeftJoin
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
        => base.LeftJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region RightJoin
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
        => base.RightJoin(joinOn) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region Where
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
        => this.Where(true, predicate);
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region And
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
        => this.And(true, predicate);
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region Or
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> Or(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
        => this.Or(true, predicate);
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region GroupBy
    public new IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping>;
    #endregion

    #region OrderBy
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => base.OrderBy(condition, fieldsExpr) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => base.OrderByDescending(condition, fieldsExpr) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6>, Expression> fieldsGetter)
        => base.OrderByDynamic(fieldsGetter) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region Skip/Take/Page
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> Skip(int offset)
        => base.Skip(offset) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> Take(int limit)
        => base.Take(limit) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> Page(int pageNumber, int pageSize)
        => base.Page(pageNumber, pageSize) as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region Select
    public new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IMySqlFromCommand<TTarget>;
    public new IMySqlFromCommand<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> specialMemberSelector = null)
        => base.SelectTo(specialMemberSelector) as IMySqlFromCommand<TTarget>;
    #endregion
}