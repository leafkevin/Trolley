using System;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

public class SqlServerFromContinuedCreate<TEntity> : SqlServerCreated<TEntity>, ISqlServerFromContinuedCreate<TEntity>
{
    #region Constructor
    public SqlServerFromContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Output
    public ISqlServerBulkCreated<TEntity, TResult> Output<TResult>(string fieldNames)
    {
        this.DialectVisitor.Output(fieldNames);
        return new SqlServerBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public ISqlServerBulkCreated<TEntity, TResult> Output<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Output(fieldsSelector);
        return new SqlServerBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}