using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public class PostgreSqlFromContinuedCreate<TEntity> : PostgreSqlCreated<TEntity>, IPostgreSqlFromContinuedCreate<TEntity>
{
    #region Constructor
    public PostgreSqlFromContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Returning
    public IPostgreSqlBulkCreated<TEntity, TResult> Returning<TResult>(string fieldNames)
    {
        this.DialectVisitor.Returning(fieldNames);
        return new PostgreSqlBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public IPostgreSqlBulkCreated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Returning(fieldsSelector);
        return new PostgreSqlBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}