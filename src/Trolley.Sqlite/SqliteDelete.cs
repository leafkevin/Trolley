using System;
using System.Linq.Expressions;

namespace Trolley.Sqlite;

public class SqliteDelete<TEntity> : Delete<TEntity>, ISqliteDelete<TEntity>
{
    #region Properties
    public SqliteDeleteVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public SqliteDelete(DbContext dbContext) : base(dbContext)
    {
        this.DialectVisitor = this.Visitor as SqliteDeleteVisitor;
    }
    #endregion

    #region Sharding
    public new ISqliteDelete<TEntity> UseTable(params string[] tableNames)
    {
        base.UseTable(tableNames);
        return this;
    }
    public new ISqliteDelete<TEntity> UseTable(Func<string, bool> tableNamePredicate)
    {
        base.UseTable(tableNamePredicate);
        return this;
    }
    public new ISqliteDelete<TEntity> UseTableBy(object field1Value, object field2Value = null)
    {
        base.UseTableBy(field1Value, field2Value);
        return this;
    }
    public new ISqliteDelete<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        base.UseTableByRange(beginFieldValue, endFieldValue);
        return this;
    }
    public new ISqliteDelete<TEntity> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new ISqliteDelete<TEntity> UseTableSchema(string tableSchema)
    {
        base.UseTableSchema(tableSchema);
        return this;
    }
    #endregion

    #region Where
    public new ISqliteContinuedDelete<TEntity> Where(object keys)
        => base.Where(keys) as ISqliteContinuedDelete<TEntity>;
    public new ISqliteContinuedDelete<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public new ISqliteContinuedDelete<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as ISqliteContinuedDelete<TEntity>;
    #endregion
}