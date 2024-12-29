using System;
using System.Linq.Expressions;

namespace Trolley.Sqlite;

public class SqliteFromContinuedCreate<TEntity> : SqliteCreated<TEntity>, ISqliteFromContinuedCreate<TEntity>
{
    #region Constructor
    public SqliteFromContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Returning
    public ISqliteBulkCreated<TEntity, TResult> Returning<TResult>(string fieldNames)
    {
        this.DialectVisitor.Returning(fieldNames);
        return new SqliteBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public ISqliteBulkCreated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Returning(fieldsSelector);
        return new SqliteBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}