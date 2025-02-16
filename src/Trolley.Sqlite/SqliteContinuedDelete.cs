using System;
using System.Linq.Expressions;

namespace Trolley.Sqlite;

public class SqliteContinuedDelete<TEntity> : ContinuedDelete<TEntity>, ISqliteContinuedDelete<TEntity>
{
    #region Properties
    public SqliteDeleteVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public SqliteContinuedDelete(DbContext dbContext, IDeleteVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as SqliteDeleteVisitor;
    }
    #endregion

    #region And
    public new ISqliteContinuedDelete<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public new ISqliteContinuedDelete<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        base.And(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region Returnning
    public ISqliteDeleted<TEntity, TResult> Returning<TResult>(string fieldNames)
    {
        this.DialectVisitor.Returning(fieldNames);
        return new SqliteDeleted<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public ISqliteDeleted<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Returning(fieldsSelector);
        return new SqliteDeleted<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}