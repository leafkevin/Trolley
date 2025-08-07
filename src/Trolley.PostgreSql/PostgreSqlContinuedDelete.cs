using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public class PostgreSqlContinuedDelete<TEntity> : ContinuedDelete<TEntity>, IPostgreSqlContinuedDelete<TEntity>
{
    #region Properties
    public PostgreSqlDeleteVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public PostgreSqlContinuedDelete(DbContext dbContext, IDeleteVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as PostgreSqlDeleteVisitor;
    }
    #endregion

    #region And
    public new IPostgreSqlContinuedDelete<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public new IPostgreSqlContinuedDelete<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IPostgreSqlContinuedDelete<TEntity>;
    public new IPostgreSqlContinuedDelete<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IPostgreSqlContinuedDelete<TEntity>;
    #endregion

    #region Or
    public new IPostgreSqlContinuedDelete<TEntity> Or(Expression<Func<TEntity, bool>> predicate)
        => this.Or(true, predicate);
    public new IPostgreSqlContinuedDelete<TEntity> Or(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IPostgreSqlContinuedDelete<TEntity>;
    public new IPostgreSqlContinuedDelete<TEntity> OrPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IPostgreSqlContinuedDelete<TEntity>;
    #endregion

    #region Returnning
    public IPostgreSqlDeleted<TEntity, TResult> Returning<TResult>(string fieldNames)
    {
        this.DialectVisitor.Returning(fieldNames);
        return new PostgreSqlDeleted<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public IPostgreSqlDeleted<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Returning(fieldsSelector);
        return new PostgreSqlDeleted<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}