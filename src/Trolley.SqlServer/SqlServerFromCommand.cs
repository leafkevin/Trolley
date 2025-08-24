using System;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

public class SqlServerFromCommand<TEntity, T> : FromCommand<TEntity, T>, ISqlServerFromCommand<TEntity, T>
{
    #region Constructor
    public SqlServerFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new ISqlServerFromCommand<TEntity, T> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as ISqlServerFromCommand<TEntity, T>;
    public new ISqlServerFromCommand<TEntity, T> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as ISqlServerFromCommand<TEntity, T>;
    public new ISqlServerFromCommand<TEntity, T> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as ISqlServerFromCommand<TEntity, T>;
    public new ISqlServerFromCommand<TEntity, T> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as ISqlServerFromCommand<TEntity, T>;
    public new ISqlServerFromCommand<TEntity, T> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as ISqlServerFromCommand<TEntity, T>;
    public new ISqlServerFromCommand<TEntity, T> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as ISqlServerFromCommand<TEntity, T>;
    #endregion

    #region UseTableSchema
    public new ISqlServerFromCommand<TEntity, T> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as ISqlServerFromCommand<TEntity, T>;
    #endregion

    #region Union/UnionAll
    public new ISqlServerFromCommand<TEntity, T> Union(IQuery<T> subQuery)
    {
        base.UnionInternal(subQuery);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T> Union(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        base.UnionInternal(subQuery);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T> UnionAll(IQuery<T> subQuery)
    {
        base.UnionAllInternal(subQuery);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T> UnionAll(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        base.UnionAllInternal(subQuery);
        return this;
    }
    #endregion

    #region Join   
    public new ISqlServerFromCommand<TEntity, T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as ISqlServerFromCommand<TEntity, T, TOther>;
    public new ISqlServerFromCommand<TEntity, T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as ISqlServerFromCommand<TEntity, T, TOther>;
    public new ISqlServerFromCommand<TEntity, T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as ISqlServerFromCommand<TEntity, T, TOther>;
    public new ISqlServerFromCommand<TEntity, T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as ISqlServerFromCommand<TEntity, T, TOther>;
    public new ISqlServerFromCommand<TEntity, T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as ISqlServerFromCommand<TEntity, T, TOther>;
    public new ISqlServerFromCommand<TEntity, T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as ISqlServerFromCommand<TEntity, T, TOther>;
    public new ISqlServerFromCommand<TEntity, T, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as ISqlServerFromCommand<TEntity, T, TOther>;
    public new ISqlServerFromCommand<TEntity, T, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as ISqlServerFromCommand<TEntity, T, TOther>;
    public new ISqlServerFromCommand<TEntity, T, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as ISqlServerFromCommand<TEntity, T, TOther>;
    #endregion

    #region Where/And
    public new ISqlServerFromCommand<TEntity, T> Where(Expression<Func<T, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T> And(Expression<Func<T, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T> And(bool condition, Expression<Func<T, bool>> ifPredicate = null, Expression<Func<T, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public new ISqlServerGroupingCommand<TEntity, T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as ISqlServerGroupingCommand<TEntity, T, TGrouping>;
    #endregion

    #region OrderBy
    public new ISqlServerFromCommand<TEntity, T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
         => this.OrderBy(true, fieldsExpr);
    public new ISqlServerFromCommand<TEntity, T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new ISqlServerFromCommand<TEntity, T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new ISqlServerFromCommand<TEntity, TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as ISqlServerFromCommand<TEntity, TTarget>;
    public new ISqlServerFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as ISqlServerFromCommand<TEntity, TTarget>;
    public new ISqlServerFromCommand<TEntity, TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
        => base.SelectAggregate(fieldsExpr) as ISqlServerFromCommand<TEntity, TTarget>;
    #endregion

    #region Distinct
    public new ISqlServerFromCommand<TEntity, T> Distinct()
    {
        this.Visitor.Distinct();
        return this;
    }
    #endregion

    #region Skip/Take
    public new ISqlServerFromCommand<TEntity, T> Skip(int offset)
    {
        this.Visitor.Skip(offset);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T> Take(int limit)
    {
        this.Visitor.Take(limit);
        return this;
    }
    #endregion

    #region Output
    public ISqlServerBulkCreated<T, TResult> Output<TResult>(string fieldNames)
    {
        var visitor = this.NewCreateVisitor();
        var dialectVisitor = this.Visitor as SqlServerQueryVisitor;
        dialectVisitor.OutputSql = visitor.VisitOutputFields(fieldNames);
        var sql = this.Visitor.BuildCommandSql(out _);
        visitor.FromSql = this.Visitor.BuildCommandSql(out _);
        return new SqlServerBulkCreated<T, TResult>(this.DbContext, visitor);
    }
    public ISqlServerBulkCreated<T, TResult> Output<TResult>(Expression<Func<T, TResult>> fieldsSelector)
    {
        var visitor = this.NewCreateVisitor();
        var dialectVisitor = this.Visitor as SqlServerQueryVisitor;
        dialectVisitor.OutputSql = visitor.VisitOutputExpression(fieldsSelector);
        visitor.FromSql = this.Visitor.BuildCommandSql(out _);
        return new SqlServerBulkCreated<T, TResult>(this.DbContext, visitor);
    }
    #endregion

    protected virtual SqlServerCreateVisitor NewCreateVisitor()
    {
        var createVisiter = new SqlServerCreateVisitor(this.DbContext, this.Visitor.TableAsStart);
        createVisiter.Tables = this.Visitor.Tables;
        createVisiter.IsMultiple = this.Visitor.IsMultiple;
        createVisiter.CommandIndex = this.Visitor.CommandIndex;
        createVisiter.RefQueries = this.Visitor.RefQueries;
        createVisiter.ShardingTables = this.Visitor.ShardingTables;
        createVisiter.DbParameters = this.Visitor.DbParameters;
        return createVisiter;
    }
}
public class SqlServerFromCommand<TEntity, T1, T2> : FromCommand<TEntity, T1, T2>, ISqlServerFromCommand<TEntity, T1, T2>
{
    #region Constructor
    public SqlServerFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new ISqlServerFromCommand<TEntity, T1, T2> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as ISqlServerFromCommand<TEntity, T1, T2>;
    public new ISqlServerFromCommand<TEntity, T1, T2> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as ISqlServerFromCommand<TEntity, T1, T2>;
    public new ISqlServerFromCommand<TEntity, T1, T2> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as ISqlServerFromCommand<TEntity, T1, T2>;
    public new ISqlServerFromCommand<TEntity, T1, T2> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as ISqlServerFromCommand<TEntity, T1, T2>;
    public new ISqlServerFromCommand<TEntity, T1, T2> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as ISqlServerFromCommand<TEntity, T1, T2>;
    public new ISqlServerFromCommand<TEntity, T1, T2> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as ISqlServerFromCommand<TEntity, T1, T2>;
    #endregion

    #region UseTableSchema
    public new ISqlServerFromCommand<TEntity, T1, T2> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as ISqlServerFromCommand<TEntity, T1, T2>;
    #endregion

    #region Join
    public new ISqlServerFromCommand<TEntity, T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn)
        => base.InnerJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2>;
    public new ISqlServerFromCommand<TEntity, T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn)
        => base.LeftJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2>;
    public new ISqlServerFromCommand<TEntity, T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn)
        => base.RightJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2>;
    public new ISqlServerFromCommand<TEntity, T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, TOther>;
    #endregion

    #region Where/And
    public new ISqlServerFromCommand<TEntity, T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2> And(Expression<Func<T1, T2, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public new ISqlServerGroupingCommand<TEntity, T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as ISqlServerGroupingCommand<TEntity, T1, T2, TGrouping>;
    #endregion

    #region OrderBy
    public new ISqlServerFromCommand<TEntity, T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new ISqlServerFromCommand<TEntity, T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new ISqlServerFromCommand<TEntity, T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new ISqlServerFromCommand<TEntity, TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as ISqlServerFromCommand<TEntity, TTarget>;
    public new ISqlServerFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as ISqlServerFromCommand<TEntity, TTarget>;
    #endregion

    #region Skip/Take
    public new ISqlServerFromCommand<TEntity, T1, T2> Skip(int offset)
    {
        this.Visitor.Skip(offset);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2> Take(int limit)
    {
        this.Visitor.Take(limit);
        return this;
    }
    #endregion
}
public class SqlServerFromCommand<TEntity, T1, T2, T3> : FromCommand<TEntity, T1, T2, T3>, ISqlServerFromCommand<TEntity, T1, T2, T3>
{
    #region Constructor
    public SqlServerFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as ISqlServerFromCommand<TEntity, T1, T2, T3>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as ISqlServerFromCommand<TEntity, T1, T2, T3>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as ISqlServerFromCommand<TEntity, T1, T2, T3>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as ISqlServerFromCommand<TEntity, T1, T2, T3>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as ISqlServerFromCommand<TEntity, T1, T2, T3>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as ISqlServerFromCommand<TEntity, T1, T2, T3>;
    #endregion

    #region UseTableSchema
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as ISqlServerFromCommand<TEntity, T1, T2, T3>;
    #endregion

    #region Join
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
        => base.InnerJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
        => base.LeftJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
        => base.RightJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, TOther>;
    #endregion

    #region Where/And
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public new ISqlServerGroupingCommand<TEntity, T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as ISqlServerGroupingCommand<TEntity, T1, T2, T3, TGrouping>;
    #endregion

    #region OrderBy
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new ISqlServerFromCommand<TEntity, TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as ISqlServerFromCommand<TEntity, TTarget>;
    public new ISqlServerFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as ISqlServerFromCommand<TEntity, TTarget>;
    #endregion

    #region Skip/Take
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> Skip(int offset)
    {
        this.Visitor.Skip(offset);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3> Take(int limit)
    {
        this.Visitor.Take(limit);
        return this;
    }
    #endregion
}
public class SqlServerFromCommand<TEntity, T1, T2, T3, T4> : FromCommand<TEntity, T1, T2, T3, T4>, ISqlServerFromCommand<TEntity, T1, T2, T3, T4>
{
    #region Constructor
    public SqlServerFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4>;
    #endregion

    #region UseTableSchema
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4>;
    #endregion

    #region Join
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
        => base.InnerJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
        => base.LeftJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
        => base.RightJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, TOther>;
    #endregion

    #region Where/And
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public new ISqlServerGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as ISqlServerGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping>;
    #endregion

    #region OrderBy
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new ISqlServerFromCommand<TEntity, TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as ISqlServerFromCommand<TEntity, TTarget>;
    public new ISqlServerFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as ISqlServerFromCommand<TEntity, TTarget>;
    #endregion

    #region Skip/Take
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> Skip(int offset)
    {
        this.Visitor.Skip(offset);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4> Take(int limit)
    {
        this.Visitor.Take(limit);
        return this;
    }
    #endregion
}
public class SqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> : FromCommand<TEntity, T1, T2, T3, T4, T5>, ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5>
{
    #region Constructor
    public SqlServerFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5>;
    #endregion

    #region UseTableSchema
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5>;
    #endregion

    #region Join
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
        => base.InnerJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
        => base.LeftJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
        => base.RightJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region Where/And
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public new ISqlServerGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as ISqlServerGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping>;
    #endregion

    #region OrderBy
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new ISqlServerFromCommand<TEntity, TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as ISqlServerFromCommand<TEntity, TTarget>;
    public new ISqlServerFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as ISqlServerFromCommand<TEntity, TTarget>;
    #endregion

    #region Skip/Take
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> Skip(int offset)
    {
        this.Visitor.Skip(offset);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5> Take(int limit)
    {
        this.Visitor.Take(limit);
        return this;
    }
    #endregion
}
public class SqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> : FromCommand<TEntity, T1, T2, T3, T4, T5, T6>, ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6>
{
    #region Constructor
    public SqlServerFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    #endregion

    #region UseTableSchema
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    #endregion

    #region Join
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
        => base.InnerJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
        => base.LeftJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
        => base.RightJoin(joinOn) as ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6>;
    #endregion

    #region Where/And
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public new ISqlServerGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as ISqlServerGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping>;
    #endregion

    #region OrderBy
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new ISqlServerFromCommand<TEntity, TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as ISqlServerFromCommand<TEntity, TTarget>;
    public new ISqlServerFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as ISqlServerFromCommand<TEntity, TTarget>;
    #endregion

    #region Skip/Take
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Skip(int offset)
    {
        this.Visitor.Skip(offset);
        return this;
    }
    public new ISqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Take(int limit)
    {
        this.Visitor.Take(limit);
        return this;
    }
    #endregion
}