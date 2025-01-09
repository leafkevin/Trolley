using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public class MySqlContinuedDelete<TEntity> : ContinuedDelete<TEntity>, IMySqlContinuedDelete<TEntity>
{
    #region Properties
    public MySqlDeleteVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public MySqlContinuedDelete(DbContext dbContext, IDeleteVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as MySqlDeleteVisitor;
    }
    #endregion

    #region And
    public new IMySqlContinuedDelete<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public new IMySqlContinuedDelete<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        base.And(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region Returnning
    public IMySqlDeleted<TEntity, TResult> Returning<TResult>(string fieldNames)
    {
        this.DialectVisitor.Returning(fieldNames);
        return new MySqlDeleted<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public IMySqlDeleted<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Returning(fieldsSelector);
        return new MySqlDeleted<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}