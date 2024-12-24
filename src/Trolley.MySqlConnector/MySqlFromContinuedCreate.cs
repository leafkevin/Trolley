using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public class MySqlFromContinuedCreate<TEntity> : MySqlCreated<TEntity>, IMySqlFromContinuedCreate<TEntity>
{
    #region Constructor
    public MySqlFromContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Returning
    public IMySqlBulkCreated<TEntity, TResult> Returning<TResult>(string fieldNames)
    {
        this.DialectVisitor.Returning(fieldNames);
        return new MySqlBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public IMySqlBulkCreated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Returning(fieldsSelector);
        return new MySqlBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}