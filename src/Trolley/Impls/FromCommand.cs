using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class FromCommand : QueryInternal, IFromCommand
{
    #region Constructor
    public FromCommand(DbContext dbContext, IQueryVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TTarget> Select<TTarget>(string fields = "*")
    {
        this.SelectInternal(fields);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        if (this.Visitor.IsNeedFetchShardingTables)
            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
        var sql = this.Visitor.BuildCommandSql(true, out var dbDataParameters);
        if (this.Visitor.IsNeedFetchShardingTables)
        {
            var builder = new StringBuilder(this.Visitor.BuildTableShardingsSql());
            builder.Append(';');
            builder.Append(sql);
            sql = builder.ToString();
        }
        dbParameters = dbDataParameters.Cast<IDbDataParameter>().ToList();
        this.Dispose();
        return sql;
    }
    #endregion

    #region Dispose
    public void Dispose() => this.Visitor.Dispose();
    #endregion
}
public class FromCommand<T> : QueryInternal, IFromCommand<T>
{
    #region Constructor
    public FromCommand(DbContext dbContext, IQueryVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Sharding
    public virtual IFromCommand<T> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IFromCommand<T> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IFromCommand<T> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IFromCommand<T> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IFromCommand<T> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public virtual IFromCommand<T> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IFromCommand<T> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region Union/UnionAll
    public virtual IFromCommand<T> Union(IQuery<T> subQuery)
    {
        base.UnionInternal(subQuery);
        return this;
    }
    public virtual IFromCommand<T> Union(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        base.UnionInternal(subQueryExpr);
        return this;
    }
    public virtual IFromCommand<T> UnionAll(IQuery<T> subQuery)
    {
        base.UnionAllInternal(subQuery);
        return this;
    }
    public virtual IFromCommand<T> UnionAll(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        base.UnionAllInternal(subQueryExpr);
        return this;
    }
    public virtual IFromCommand<T> UnionRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr)
    {
        base.UnionRecursiveInternal(subQueryExpr);
        return this;
    }
    public virtual IFromCommand<T> UnionAllRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr)
    {
        base.UnionAllRecursiveInternal(subQueryExpr);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IFromCommand<T, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IFromCommand<T, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IFromCommand<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IFromCommand<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IFromCommand<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IFromCommand<T> Where(Expression<Func<T, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region And
    public virtual IFromCommand<T> And(Expression<Func<T, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T> And(bool condition, Expression<Func<T, bool>> ifPredicate = null, Expression<Func<T, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region Or
    public virtual IFromCommand<T> Or(Expression<Func<T, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T> Or(bool condition, Expression<Func<T, bool>> ifPredicate = null, Expression<Func<T, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public virtual IGroupingCommand<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
    {
        this.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupCommand<T, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IFromCommand<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
         => this.OrderBy(true, fieldsExpr);
    public virtual IFromCommand<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IFromCommand<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IFromCommand<T> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IFromCommand<T> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IFromCommand<T> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<T> Select(string fields = "*")
    {
        this.SelectInternal(fields);
        return this;
    }
    public virtual IFromCommand<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TTarget> SelectFlattenTo<TTarget>(Expression<Func<T, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion   

    #region Distinct
    public virtual IFromCommand<T> Distinct()
    {
        this.Visitor.Distinct();
        return this;
    }
    #endregion    

    #region Execute
    public virtual int Execute()
    {
        if (this.Visitor.IsNeedFetchShardingTables)
            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        command.CommandText = this.Visitor.BuildCommandSql(true, out var dbParameters);
        dbParameters.CopyTo(command.Parameters);
        connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.Insert);
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (this.Visitor.IsNeedFetchShardingTables)
            await this.DbContext.FetchShardingTablesAsync(this.Visitor as SqlVisitor, cancellationToken);

        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        command.CommandText = this.Visitor.BuildCommandSql(true, out var dbParameters);
        dbParameters.CopyTo(command.Parameters);
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.Insert, cancellationToken);
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        if (this.Visitor.IsNeedFetchShardingTables)
            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
        var sql = this.Visitor.BuildCommandSql(true, out var dbDataParameters);
        if (this.Visitor.IsNeedFetchShardingTables)
        {
            var builder = new StringBuilder(this.Visitor.BuildTableShardingsSql());
            builder.Append(';');
            builder.Append(sql);
            sql = builder.ToString();
        }
        dbParameters = dbDataParameters.Cast<IDbDataParameter>().ToList();
        this.Dispose();
        return sql;
    }
    #endregion

    #region Dispose
    public void Dispose() => this.Visitor.Dispose();
    #endregion
}
public class FromCommand<T1, T2> : FromCommand, IFromCommand<T1, T2>
{
    #region Constructor
    public FromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IFromCommand<T1, T2> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IFromCommand<T1, T2> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IFromCommand<T1, T2> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IFromCommand<T1, T2> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IFromCommand<T1, T2> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public virtual IFromCommand<T1, T2> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IFromCommand<T1, T2> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion
    #region WithTable
    public virtual IFromCommand<T1, T2, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IFromCommand<T1, T2, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.DbContext, this.Visitor);
    }

    #endregion

    #region InnerJoin
    public virtual IFromCommand<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IFromCommand<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IFromCommand<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IFromCommand<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region And
    public virtual IFromCommand<T1, T2> And(Expression<Func<T1, T2, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region Or
    public virtual IFromCommand<T1, T2> Or(Expression<Func<T1, T2, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T1, T2> Or(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public virtual IGroupingCommand<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupCommand<T1, T2, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IFromCommand<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IFromCommand<T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IFromCommand<T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IFromCommand<T1, T2> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IFromCommand<T1, T2> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IFromCommand<T1, T2> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TTarget> SelectFlattenTo<TTarget>(Expression<Func<T1, T2, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class FromCommand<T1, T2, T3> : FromCommand, IFromCommand<T1, T2, T3>
{
    #region Constructor
    public FromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IFromCommand<T1, T2, T3> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IFromCommand<T1, T2, T3> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion
    #region WithTable
    public virtual IFromCommand<T1, T2, T3, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IFromCommand<T1, T2, T3, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }

    #endregion

    #region InnerJoin
    public virtual IFromCommand<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IFromCommand<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IFromCommand<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IFromCommand<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region And
    public virtual IFromCommand<T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region Or
    public virtual IFromCommand<T1, T2, T3> Or(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3> Or(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public virtual IGroupingCommand<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupCommand<T1, T2, T3, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IFromCommand<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IFromCommand<T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IFromCommand<T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IFromCommand<T1, T2, T3> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TTarget> SelectFlattenTo<TTarget>(Expression<Func<T1, T2, T3, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class FromCommand<T1, T2, T3, T4> : FromCommand, IFromCommand<T1, T2, T3, T4>
{
    #region Constructor
    public FromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IFromCommand<T1, T2, T3, T4> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IFromCommand<T1, T2, T3, T4> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion
    #region WithTable
    public virtual IFromCommand<T1, T2, T3, T4, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IFromCommand<T1, T2, T3, T4, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }

    #endregion

    #region InnerJoin
    public virtual IFromCommand<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IFromCommand<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IFromCommand<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IFromCommand<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region And
    public virtual IFromCommand<T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region Or
    public virtual IFromCommand<T1, T2, T3, T4> Or(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4> Or(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public virtual IGroupingCommand<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupCommand<T1, T2, T3, T4, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IFromCommand<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IFromCommand<T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IFromCommand<T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IFromCommand<T1, T2, T3, T4> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TTarget> SelectFlattenTo<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class FromCommand<T1, T2, T3, T4, T5> : FromCommand, IFromCommand<T1, T2, T3, T4, T5>
{
    #region Constructor
    public FromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IFromCommand<T1, T2, T3, T4, T5> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IFromCommand<T1, T2, T3, T4, T5> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion
    #region WithTable
    public virtual IFromCommand<T1, T2, T3, T4, T5, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IFromCommand<T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }

    #endregion

    #region InnerJoin
    public virtual IFromCommand<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IFromCommand<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IFromCommand<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IFromCommand<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region And
    public virtual IFromCommand<T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region Or
    public virtual IFromCommand<T1, T2, T3, T4, T5> Or(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public virtual IGroupingCommand<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupCommand<T1, T2, T3, T4, T5, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IFromCommand<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IFromCommand<T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IFromCommand<T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IFromCommand<T1, T2, T3, T4, T5> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TTarget> SelectFlattenTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class FromCommand<T1, T2, T3, T4, T5, T6> : FromCommand, IFromCommand<T1, T2, T3, T4, T5, T6>
{
    #region Constructor
    public FromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region InnerJoin
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    #endregion

    #region LeftJoin
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    #endregion

    #region RightJoin
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    #endregion

    #region Where
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region And
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region Or
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> Or(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public virtual IGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupCommand<T1, T2, T3, T4, T5, T6, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TTarget> SelectFlattenTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectFlattenTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}