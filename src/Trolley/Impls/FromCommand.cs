using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
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

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        var sql = this.Visitor.BuildCommandSql(true, out var dbDataParameters);
        dbParameters = dbDataParameters.Cast<IDbDataParameter>().ToList();
        this.Dispose();
        return sql;
    }
    #endregion

    #region Dispose
    public void Dispose() => this.Visitor.Dispose();
    #endregion
}
public class FromCommand<TEntity, T> : FromCommand, IFromCommand<TEntity, T>
{
    #region Constructor
    public FromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IFromCommand<TEntity, T> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableNames);
        return this;
    }
    public virtual IFromCommand<TEntity, T> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IFromCommand<TEntity, T> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IFromCommand<TEntity, T> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IFromCommand<TEntity, T> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region Union/UnionAll
    public virtual IFromCommand<TEntity, T> Union(IQuery<T> subQuery)
    {
        base.UnionInternal(subQuery);
        return this;
    }
    public virtual IFromCommand<TEntity, T> Union(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        base.UnionInternal(subQueryExpr);
        return this;
    }
    public virtual IFromCommand<TEntity, T> UnionAll(IQuery<T> subQuery)
    {
        base.UnionAllInternal(subQuery);
        return this;
    }
    public virtual IFromCommand<TEntity, T> UnionAll(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        base.UnionAllInternal(subQueryExpr);
        return this;
    }
    public virtual IFromCommand<TEntity, T> UnionRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr)
    {
        base.UnionRecursiveInternal(subQueryExpr);
        return this;
    }
    public virtual IFromCommand<TEntity, T> UnionAllRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr)
    {
        base.UnionAllRecursiveInternal(subQueryExpr);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IFromCommand<TEntity, T, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewFromCommand<TEntity, T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IFromCommand<TEntity, T, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewFromCommand<TEntity, T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewFromCommand<TEntity, T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IFromCommand<TEntity, T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IFromCommand<TEntity, T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IFromCommand<TEntity, T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IFromCommand<TEntity, T> Where(Expression<Func<T, bool>> predicate) => this.And(predicate);
    public virtual IFromCommand<TEntity, T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IFromCommand<TEntity, T> WherePredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IFromCommand<TEntity, T> And(Expression<Func<T, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T> And(bool condition, Expression<Func<T, bool>> ifPredicate = null, Expression<Func<T, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T> AndPredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IFromCommand<TEntity, T> Or(Expression<Func<T, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T> Or(bool condition, Expression<Func<T, bool>> ifPredicate = null, Expression<Func<T, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T> OrPredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingCommand<TEntity, T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
    {
        this.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupCommand<TEntity, T, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IFromCommand<TEntity, T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
         => this.OrderBy(true, fieldsExpr);
    public virtual IFromCommand<TEntity, T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<TEntity, T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IFromCommand<TEntity, T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<TEntity, T> OrderByDynamic(Func<OrderByBuilder<T>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IFromCommand<TEntity, T> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IFromCommand<TEntity, T> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IFromCommand<TEntity, T> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TEntity, T> Select(string fields = "*")
    {
        this.SelectInternal(fields);
        return this;
    }
    public virtual IFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, TTarget> SelectTo<TTarget>(Expression<Func<T, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Distinct
    public virtual IFromCommand<TEntity, T> Distinct()
    {
        this.Visitor.Distinct();
        return this;
    }
    #endregion    

    #region Execute
    public virtual int Execute()
    {
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
}
public class FromCommand<TEntity, T1, T2> : FromCommand, IFromCommand<TEntity, T1, T2>
{
    #region Constructor
    public FromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IFromCommand<TEntity, T1, T2> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableNames);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(TableShardingUsageMode.WriteOnly, false, tableNameGetter);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IFromCommand<TEntity, T1, T2> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IFromCommand<TEntity, T1, T2, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IFromCommand<TEntity, T1, T2, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, TOther>(this.DbContext, this.Visitor);
    }

    #endregion

    #region InnerJoin
    public virtual IFromCommand<TEntity, T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IFromCommand<TEntity, T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IFromCommand<TEntity, T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IFromCommand<TEntity, T1, T2> Where(Expression<Func<T1, T2, bool>> predicate) => this.And(predicate);
    public virtual IFromCommand<TEntity, T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IFromCommand<TEntity, T1, T2> WherePredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IFromCommand<TEntity, T1, T2> And(Expression<Func<T1, T2, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2> AndPredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IFromCommand<TEntity, T1, T2> Or(Expression<Func<T1, T2, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2> Or(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2> OrPredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingCommand<TEntity, T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupCommand<TEntity, T1, T2, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IFromCommand<TEntity, T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IFromCommand<TEntity, T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IFromCommand<TEntity, T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2> OrderByDynamic(Func<OrderByBuilder<T1, T2>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IFromCommand<TEntity, T1, T2> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class FromCommand<TEntity, T1, T2, T3> : FromCommand, IFromCommand<TEntity, T1, T2, T3>
{
    #region Constructor
    public FromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IFromCommand<TEntity, T1, T2, T3> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableNames);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(TableShardingUsageMode.WriteOnly, false, tableNameGetter);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IFromCommand<TEntity, T1, T2, T3> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IFromCommand<TEntity, T1, T2, T3, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IFromCommand<TEntity, T1, T2, T3, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }

    #endregion

    #region InnerJoin
    public virtual IFromCommand<TEntity, T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IFromCommand<TEntity, T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IFromCommand<TEntity, T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IFromCommand<TEntity, T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate) => this.And(predicate);
    public virtual IFromCommand<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IFromCommand<TEntity, T1, T2, T3> WherePredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IFromCommand<TEntity, T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3> AndPredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IFromCommand<TEntity, T1, T2, T3> Or(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3> Or(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3> OrPredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingCommand<TEntity, T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupCommand<TEntity, T1, T2, T3, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IFromCommand<TEntity, T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IFromCommand<TEntity, T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IFromCommand<TEntity, T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IFromCommand<TEntity, T1, T2, T3> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class FromCommand<TEntity, T1, T2, T3, T4> : FromCommand, IFromCommand<TEntity, T1, T2, T3, T4>
{
    #region Constructor
    public FromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableNames);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(TableShardingUsageMode.WriteOnly, false, tableNameGetter);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }

    #endregion

    #region InnerJoin
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate) => this.And(predicate);
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> Or(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> Or(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupCommand<TEntity, T1, T2, T3, T4, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class FromCommand<TEntity, T1, T2, T3, T4, T5> : FromCommand, IFromCommand<TEntity, T1, T2, T3, T4, T5>
{
    #region Constructor
    public FromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableNames);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(TableShardingUsageMode.WriteOnly, false, tableNameGetter);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }

    #endregion

    #region InnerJoin
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, T5, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate) => this.And(predicate);
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> Or(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupCommand<TEntity, T1, T2, T3, T4, T5, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class FromCommand<TEntity, T1, T2, T3, T4, T5, T6> : FromCommand, IFromCommand<TEntity, T1, T2, T3, T4, T5, T6>
{
    #region Constructor
    public FromCommand(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableNames);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(TableShardingUsageMode.WriteOnly, false, tableNameGetter);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region InnerJoin
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    #endregion

    #region LeftJoin
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    #endregion

    #region RightJoin
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    #endregion

    #region Where
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate) => this.And(predicate);
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Or(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewGroupCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select
    public virtual IFromCommand<TEntity, TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    public virtual IFromCommand<TEntity, TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewFromCommand<TEntity, TTarget>(this.DbContext, this.Visitor);
    }
    #endregion
}