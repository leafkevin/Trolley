using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public class PostgreSqlFromCommand<T> : FromCommand<T>, IPostgreSqlFromCommand<T>
{
    #region Constructor
    public PostgreSqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlFromCommand<T> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlFromCommand<T>;
    public new IPostgreSqlFromCommand<T> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlFromCommand<T>;
    public new IPostgreSqlFromCommand<T> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlFromCommand<T>;
    public new IPostgreSqlFromCommand<T> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlFromCommand<T>;
    public new IPostgreSqlFromCommand<T> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlFromCommand<T>;
    public new IPostgreSqlFromCommand<T> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlFromCommand<T>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlFromCommand<T> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlFromCommand<T>;
    #endregion

    #region Union/UnionAll
    public new IPostgreSqlFromCommand<T> Union(IQuery<T> subQuery)
    {
        base.UnionInternal(subQuery);
        return this;
    }
    public new IPostgreSqlFromCommand<T> Union(Func<IFromQuery, IQuery<T>> subQueryGetter)
    {
        base.UnionInternal(subQuery);
        return this;
    }
    public new IPostgreSqlFromCommand<T> UnionAll(IQuery<T> subQuery)
    {
        base.UnionAllInternal(subQuery);
        return this;
    }
    public new IPostgreSqlFromCommand<T> UnionAll(Func<IFromQuery, IQuery<T>> subQueryGetter)
    {
        base.UnionAllInternal(subQuery);
        return this;
    }
    #endregion

    #region Join   
    public new IPostgreSqlFromCommand<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<T, TOther>;
    public new IPostgreSqlFromCommand<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<T, TOther>;
    public new IPostgreSqlFromCommand<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<T, TOther>;
    public new IPostgreSqlFromCommand<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<T, TOther>;
    public new IPostgreSqlFromCommand<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<T, TOther>;
    public new IPostgreSqlFromCommand<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<T, TOther>;
    public new IPostgreSqlFromCommand<T, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression<Func<T, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<T, TOther>;
    public new IPostgreSqlFromCommand<T, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression<Func<T, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<T, TOther>;
    public new IPostgreSqlFromCommand<T, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression<Func<T, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<T, TOther>;
    #endregion

    #region Where/And
    public new IPostgreSqlFromCommand<T> Where(Expression<Func<T, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T> And(Expression<Func<T, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T> And(bool condition, Expression<Func<T, bool>> ifPredicate = null, Expression<Func<T, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingCommand<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingCommand<T, TGrouping>;
    #endregion

    #region OrderBy
    public new IPostgreSqlFromCommand<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
         => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlFromCommand<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new IPostgreSqlFromCommand<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlFromCommand<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as IPostgreSqlFromCommand<TTarget>;
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlFromCommand<TTarget>;
    public new IPostgreSqlFromCommand<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
        => base.SelectAggregate(fieldsExpr) as IPostgreSqlFromCommand<TTarget>;
    #endregion

    #region Distinct
    public new IPostgreSqlFromCommand<T> Distinct()
    {
        this.Visitor.Distinct();
        return this;
    }
    #endregion

    #region Skip/Take
    public new IPostgreSqlFromCommand<T> Skip(int offset)
    {
        this.Visitor.Skip(offset);
        return this;
    }
    public new IPostgreSqlFromCommand<T> Take(int limit)
    {
        this.Visitor.Take(limit);
        return this;
    }
    #endregion

    #region OnConflict
    public IPostgreSqlFromContinuedCreate<T> OnConflict<TUpdateFields>(Expression<Func<IPostgreSqlCreateConflictDoUpdate<T>, TUpdateFields>> fieldsAssignment)
    {
        var visitor = this.NewCreateVisitor();
        visitor.VisitSetExpression(fieldsAssignment);
        this.Visitor.IsNeedCommandTableAlias = visitor.IsUseTableAlias;
        visitor.FromSql = this.Visitor.BuildCommandSql(out _);
        return new PostgreSqlFromContinuedCreate<T>(this.DbContext, visitor);
    }
    #endregion

    #region Returnning
    public IPostgreSqlBulkCreated<T, TResult> Returning<TResult>(string fieldNames)
    {
        var sql = this.Visitor.BuildCommandSql(out _);
        var visitor = this.NewCreateVisitor();
        visitor.FromSql = sql;
        visitor.Returning(fieldNames);
        return new PostgreSqlBulkCreated<T, TResult>(this.DbContext, visitor);
    }
    public IPostgreSqlBulkCreated<T, TResult> Returning<TResult>(Expression<Func<T, TResult>> fieldsSelector)
    {
        var sql = this.Visitor.BuildCommandSql(out _);
        var visitor = this.NewCreateVisitor();
        visitor.FromSql = sql;
        visitor.Returning(fieldsSelector);
        return new PostgreSqlBulkCreated<T, TResult>(this.DbContext, visitor);
    }
    #endregion

    protected virtual PostgreSqlCreateVisitor NewCreateVisitor()
    {
        var createVisiter = new PostgreSqlCreateVisitor(this.DbContext, this.Visitor.TableAsStart);
        createVisiter.Tables = this.Visitor.Tables;
        createVisiter.IsMultiple = this.Visitor.IsMultiple;
        createVisiter.CommandIndex = this.Visitor.CommandIndex;
        createVisiter.RefQueries = this.Visitor.RefQueries;
        createVisiter.ShardingTables = this.Visitor.ShardingTables;
        createVisiter.DbParameters = this.Visitor.DbParameters;
        return createVisiter;
    }
}
public class PostgreSqlFromCommand<T1, T2> : FromCommand<T1, T2>, IPostgreSqlFromCommand<T1, T2>
{
    #region Constructor
    public PostgreSqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlFromCommand<T1, T2> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlFromCommand<T1, T2>;
    public new IPostgreSqlFromCommand<T1, T2> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlFromCommand<T1, T2>;
    public new IPostgreSqlFromCommand<T1, T2> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlFromCommand<T1, T2>;
    public new IPostgreSqlFromCommand<T1, T2> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlFromCommand<T1, T2>;
    public new IPostgreSqlFromCommand<T1, T2> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlFromCommand<T1, T2>;
    public new IPostgreSqlFromCommand<T1, T2> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlFromCommand<T1, T2>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlFromCommand<T1, T2> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlFromCommand<T1, T2>;
    #endregion

    #region Join
    public new IPostgreSqlFromCommand<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<T1, T2>;
    public new IPostgreSqlFromCommand<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<T1, T2>;
    public new IPostgreSqlFromCommand<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<T1, T2>;
    public new IPostgreSqlFromCommand<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression<Func<T1, T2, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, TOther>;
    #endregion

    #region Where/And
    public new IPostgreSqlFromCommand<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2> And(Expression<Func<T1, T2, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingCommand<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingCommand<T1, T2, TGrouping>;
    #endregion

    #region OrderBy
    public new IPostgreSqlFromCommand<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlFromCommand<T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlFromCommand<T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as IPostgreSqlFromCommand<TTarget>;
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlFromCommand<TTarget>;
    #endregion

    #region Skip/Take
    public new IPostgreSqlFromCommand<T1, T2> Skip(int offset)
    {
        this.Visitor.Skip(offset);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2> Take(int limit)
    {
        this.Visitor.Take(limit);
        return this;
    }
    #endregion
}
public class PostgreSqlFromCommand<T1, T2, T3> : FromCommand<T1, T2, T3>, IPostgreSqlFromCommand<T1, T2, T3>
{
    #region Constructor
    public PostgreSqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlFromCommand<T1, T2, T3> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlFromCommand<T1, T2, T3>;
    public new IPostgreSqlFromCommand<T1, T2, T3> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlFromCommand<T1, T2, T3>;
    public new IPostgreSqlFromCommand<T1, T2, T3> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlFromCommand<T1, T2, T3>;
    public new IPostgreSqlFromCommand<T1, T2, T3> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlFromCommand<T1, T2, T3>;
    public new IPostgreSqlFromCommand<T1, T2, T3> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlFromCommand<T1, T2, T3>;
    public new IPostgreSqlFromCommand<T1, T2, T3> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlFromCommand<T1, T2, T3>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlFromCommand<T1, T2, T3> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlFromCommand<T1, T2, T3>;
    #endregion

    #region Join
    public new IPostgreSqlFromCommand<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3>;
    public new IPostgreSqlFromCommand<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3>;
    public new IPostgreSqlFromCommand<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3>;
    public new IPostgreSqlFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, TOther>;
    #endregion

    #region Where/And
    public new IPostgreSqlFromCommand<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingCommand<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingCommand<T1, T2, T3, TGrouping>;
    #endregion

    #region OrderBy
    public new IPostgreSqlFromCommand<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlFromCommand<T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlFromCommand<T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as IPostgreSqlFromCommand<TTarget>;
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlFromCommand<TTarget>;
    #endregion

    #region Skip/Take
    public new IPostgreSqlFromCommand<T1, T2, T3> Skip(int offset)
    {
        this.Visitor.Skip(offset);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3> Take(int limit)
    {
        this.Visitor.Take(limit);
        return this;
    }
    #endregion
}
public class PostgreSqlFromCommand<T1, T2, T3, T4> : FromCommand<T1, T2, T3, T4>, IPostgreSqlFromCommand<T1, T2, T3, T4>
{
    #region Constructor
    public PostgreSqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlFromCommand<T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlFromCommand<T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlFromCommand<T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlFromCommand<T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlFromCommand<T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlFromCommand<T1, T2, T3, T4>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlFromCommand<T1, T2, T3, T4>;
    #endregion

    #region Join
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, TOther>;
    #endregion

    #region Where/And
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingCommand<T1, T2, T3, T4, TGrouping>;
    #endregion

    #region OrderBy
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as IPostgreSqlFromCommand<TTarget>;
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlFromCommand<TTarget>;
    #endregion

    #region Skip/Take
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> Skip(int offset)
    {
        this.Visitor.Skip(offset);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> Take(int limit)
    {
        this.Visitor.Take(limit);
        return this;
    }
    #endregion
}
public class PostgreSqlFromCommand<T1, T2, T3, T4, T5> : FromCommand<T1, T2, T3, T4, T5>, IPostgreSqlFromCommand<T1, T2, T3, T4, T5>
{
    #region Constructor
    public PostgreSqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5>;
    #endregion

    #region Join
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.InnerJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.LeftJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
        => base.RightJoin(subQuery, joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, TOther>;
    #endregion

    #region Where/And
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping>;
    #endregion

    #region OrderBy
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as IPostgreSqlFromCommand<TTarget>;
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlFromCommand<TTarget>;
    #endregion

    #region Skip/Take
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> Skip(int offset)
    {
        this.Visitor.Skip(offset);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> Take(int limit)
    {
        this.Visitor.Take(limit);
        return this;
    }
    #endregion
}
public class PostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> : FromCommand<T1, T2, T3, T4, T5, T6>, IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6>
{
    #region Constructor
    public PostgreSqlFromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region Join
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
        => base.InnerJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
        => base.LeftJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
        => base.RightJoin(joinOn) as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6>;
    #endregion

    #region Where/And
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public new IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr)
        => base.GroupBy(groupingExpr) as IPostgreSqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping>;
    #endregion

    #region OrderBy
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(string fields = "*")
        => base.Select<TTarget>(fields) as IPostgreSqlFromCommand<TTarget>;
    public new IPostgreSqlFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
        => base.Select(fieldsExpr) as IPostgreSqlFromCommand<TTarget>;
    #endregion

    #region Skip/Take
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> Skip(int offset)
    {
        this.Visitor.Skip(offset);
        return this;
    }
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> Take(int limit)
    {
        this.Visitor.Take(limit);
        return this;
    }
    #endregion
}