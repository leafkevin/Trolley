using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class FromCommand : IFromCommand
{
    #region Properties
    public Type EntityType { get; set; }
    public DbContext DbContext { get; set; }
    public IQueryVisitor Visitor { get; set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    public IEntityMapProvider MapProvider => this.DbContext.MapProvider;
    #endregion

    #region Constructor
    public FromCommand(Type entityType, DbContext dbContext, IQueryVisitor visitor)
    {
        this.EntityType = entityType;
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Select
    public IFromCommand<TTarget> Select<TTarget>(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.Visitor.Select(fields);
        return this.OrmProvider.NewFromCommand<TTarget>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region Execute
    public int Execute()
    {
        var sql = this.Visitor.BuildCommandSql(this.EntityType, out var dbParameters);
        var result = this.DbContext.Execute(f =>
        {
            f.CommandText = sql;
            dbParameters.CopyTo(f.Parameters);
        });
        this.Dispose();
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var sql = this.Visitor.BuildCommandSql(this.EntityType, out var dbParameters);
        var result = await this.DbContext.ExecuteAsync(f =>
        {
            f.CommandText = sql;
            dbParameters.CopyTo(f.Parameters);
        }, cancellationToken);
        this.Dispose();
        return result;
    }
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        var sql = this.Visitor.BuildCommandSql(this.EntityType, out var dbDataParameters);
        dbParameters = dbDataParameters.Cast<IDbDataParameter>().ToList();
        this.Dispose();
        return sql;
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        this.EntityType = null;
        //DbContext自己会处理释放，此处什么也不做
        this.DbContext = null;
        this.Visitor.Dispose();
        this.Visitor = null;
    }
    #endregion
}
public class FromCommand<T> : FromCommand, IFromCommand<T>
{
    #region Constructor
    public FromCommand(Type entityType, DbContext dbContext, IQueryVisitor visitor)
        : base(entityType, dbContext, visitor) { }
    #endregion

    #region Union/UnionAll
    public IFromCommand<T> Union(IQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION", typeof(T), subQuery);
        return this;
    }
    public IFromCommand<T> Union(Func<IFromQuery, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION", typeof(T), this.DbContext, subQuery);
        return this;
    }
    public IFromCommand<T> UnionAll(IQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION ALL", typeof(T), subQuery);
        return this;
    }
    public IFromCommand<T> UnionAll(Func<IFromQuery, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION ALL", typeof(T), this.DbContext, subQuery);
        return this;
    }
    #endregion

    #region CTE NextWith
    public IFromCommand<T, TOther> NextWith<TOther>(IQuery<TOther> cteSubQuery)
    {
        if (cteSubQuery == null)
            throw new ArgumentNullException(nameof(cteSubQuery));

        this.Visitor.FromWith(typeof(TOther), false, cteSubQuery);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> NextWith<TOther>(Func<IFromQuery, IQuery<T>, IQuery<TOther>> cteSubQuery)
    {
        if (cteSubQuery == null)
            throw new ArgumentNullException(nameof(cteSubQuery));

        this.Visitor.FromWith(typeof(TOther), false, this.DbContext, cteSubQuery);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region Join   
    public IFromCommand<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region Where/And
    public IFromCommand<T> Where(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IFromCommand<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IFromCommand<T> And(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IFromCommand<T> And(bool condition, Expression<Func<T, bool>> ifPredicate = null, Expression<Func<T, bool>> elsePredicate = null)
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
    public IGroupingCommand<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewGroupCommand<T, TGrouping>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IFromCommand<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
         => this.OrderBy(true, fieldsExpr);
    public IFromCommand<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromCommand<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IFromCommand<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromCommand<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion   

    #region Distinct
    public IFromCommand<T> Distinct()
    {
        this.Visitor.Distinct();
        return this;
    }
    #endregion

    #region Take 
    public IFromCommand<T> Skip(int offset)
    {
        this.Visitor.Skip(offset);
        return this;
    }
    public IFromCommand<T> Take(int limit)
    {
        this.Visitor.Take(limit);
        return this;
    }
    #endregion
}
public class FromCommand<T1, T2> : FromCommand, IFromCommand<T1, T2>
{
    #region Constructor
    public FromCommand(Type entityType, DbContext dbContext, IQueryVisitor visitor)
        : base(entityType, dbContext, visitor) { }
    #endregion

    #region CTE NextWith
    public IFromCommand<T1, T2, TOther> NextWith<TOther>(IQuery<TOther> cteSubQuery)
    {
        if (cteSubQuery == null)
            throw new ArgumentNullException(nameof(cteSubQuery));

        this.Visitor.FromWith(typeof(TOther), false, cteSubQuery);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, TOther> NextWith<TOther>(Func<IFromQuery, IQuery<T1>, IQuery<T2>, IQuery<TOther>> cteSubQuery)
    {
        if (cteSubQuery == null)
            throw new ArgumentNullException(nameof(cteSubQuery));

        this.Visitor.FromWith(typeof(TOther), false, this.DbContext, cteSubQuery);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region Join
    public IFromCommand<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromCommand<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromCommand<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromCommand<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region Where/And
    public IFromCommand<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IFromCommand<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IFromCommand<T1, T2> And(Expression<Func<T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IFromCommand<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null)
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
    public IGroupingCommand<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewGroupCommand<T1, T2, TGrouping>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IFromCommand<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
         => this.OrderBy(true, fieldsExpr);
    public IFromCommand<T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromCommand<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IFromCommand<T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region Take    
    public IFromCommand<T1, T2> Take(int limit)
    {
        throw new NotImplementedException();
    }
    #endregion
}
class FromCommand<T1, T2, T3> : FromCommand, IFromCommand<T1, T2, T3>
{
    #region Constructor
    public FromCommand(Type entityType, DbContext dbContext, IQueryVisitor visitor)
        : base(entityType, dbContext, visitor) { }
    #endregion

    #region CTE NextWith
    public IFromCommand<T1, T2, T3, TOther> NextWith<TOther>(IQuery<TOther> cteSubQuery)
    {
        if (cteSubQuery == null)
            throw new ArgumentNullException(nameof(cteSubQuery));

        this.Visitor.FromWith(typeof(TOther), false, cteSubQuery);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, TOther> NextWith<TOther>(Func<IFromQuery, IQuery<T1>, IQuery<T2>, IQuery<T3>, IQuery<TOther>> cteSubQuery)
    {
        if (cteSubQuery == null)
            throw new ArgumentNullException(nameof(cteSubQuery));

        this.Visitor.FromWith(typeof(TOther), false, this.DbContext, cteSubQuery);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region Join
    public IFromCommand<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromCommand<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromCommand<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region Where/And
    public IFromCommand<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IFromCommand<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IFromCommand<T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IFromCommand<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
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
    public IGroupingCommand<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewGroupCommand<T1, T2, T3, TGrouping>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IFromCommand<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IFromCommand<T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromCommand<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IFromCommand<T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select 
    public IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion
}
class FromCommand<T1, T2, T3, T4> : FromCommand, IFromCommand<T1, T2, T3, T4>
{
    #region Constructor
    public FromCommand(Type entityType, DbContext dbContext, IQueryVisitor visitor)
        : base(entityType, dbContext, visitor) { }
    #endregion

    #region CTE NextWith
    public IFromCommand<T1, T2, T3, T4, TOther> NextWith<TOther>(IQuery<TOther> cteSubQuery)
    {
        if (cteSubQuery == null)
            throw new ArgumentNullException(nameof(cteSubQuery));

        this.Visitor.FromWith(typeof(TOther), false, cteSubQuery);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, TOther> NextWith<TOther>(Func<IFromQuery, IQuery<T1>, IQuery<T2>, IQuery<T3>, IQuery<T4>, IQuery<TOther>> cteSubQuery)
    {
        if (cteSubQuery == null)
            throw new ArgumentNullException(nameof(cteSubQuery));

        this.Visitor.FromWith(typeof(TOther), false, this.DbContext, cteSubQuery);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region Join
    public IFromCommand<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region Where/And
    public IFromCommand<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
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
    public IGroupingCommand<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewGroupCommand<T1, T2, T3, T4, TGrouping>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IFromCommand<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IFromCommand<T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IFromCommand<T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select 
    public IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion
}
class FromCommand<T1, T2, T3, T4, T5> : FromCommand, IFromCommand<T1, T2, T3, T4, T5>
{
    #region Constructor
    public FromCommand(Type entityType, DbContext dbContext, IQueryVisitor visitor)
        : base(entityType, dbContext, visitor) { }
    #endregion

    #region CTE NextWith
    public IFromCommand<T1, T2, T3, T4, T5, TOther> NextWith<TOther>(IQuery<TOther> cteSubQuery)
    {
        if (cteSubQuery == null)
            throw new ArgumentNullException(nameof(cteSubQuery));

        this.Visitor.FromWith(typeof(TOther), false, cteSubQuery);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, T5, TOther> NextWith<TOther>(Func<IFromQuery, IQuery<T1>, IQuery<T2>, IQuery<T3>, IQuery<T4>, IQuery<T5>, IQuery<TOther>> cteSubQuery)
    {
        if (cteSubQuery == null)
            throw new ArgumentNullException(nameof(cteSubQuery));

        this.Visitor.FromWith(typeof(TOther), false, this.DbContext, cteSubQuery);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region Join
    public IFromCommand<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, TOther>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region Where/And
    public IFromCommand<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
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
    public IGroupingCommand<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewGroupCommand<T1, T2, T3, T4, T5, TGrouping>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IFromCommand<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IFromCommand<T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IFromCommand<T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select 
    public IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion
}
class FromCommand<T1, T2, T3, T4, T5, T6> : FromCommand, IFromCommand<T1, T2, T3, T4, T5, T6>
{
    #region Constructor
    public FromCommand(Type entityType, DbContext dbContext, IQueryVisitor visitor)
        : base(entityType, dbContext, visitor) { }
    #endregion

    #region Join
    public IFromCommand<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    #endregion

    #region Where/And
    public IFromCommand<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
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
    public IGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewGroupCommand<T1, T2, T3, T4, T5, T6, TGrouping>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IFromCommand<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IFromCommand<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromCommand<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IFromCommand<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select 
    public IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.EntityType, this.DbContext, this.Visitor);
    }
    public IFromCommand<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.EntityType, this.DbContext, this.Visitor);
    }
    #endregion
}
