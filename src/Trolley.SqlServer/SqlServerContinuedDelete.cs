using System;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

public class SqlServerContinuedDelete<TEntity> : ContinuedDelete<TEntity>, ISqlServerContinuedDelete<TEntity>
{
    #region Properties
    public SqlServerDeleteVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public SqlServerContinuedDelete(DbContext dbContext, IDeleteVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as SqlServerDeleteVisitor;
    }
    #endregion

    #region And
    public new ISqlServerContinuedDelete<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public new ISqlServerContinuedDelete<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        base.And(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region Output
    public ISqlServerDeleted<TEntity, TResult> Output<TResult>(string fieldNames)
    {
        this.DialectVisitor.Output(fieldNames);
        return new SqlServerDeleted<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public ISqlServerDeleted<TEntity, TResult> Output<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Output(fieldsSelector);
        return new SqlServerDeleted<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}