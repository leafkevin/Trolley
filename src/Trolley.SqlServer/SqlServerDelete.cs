using System;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

public class SqlServerDelete<TEntity> : Delete<TEntity>, ISqlServerDelete<TEntity>
{
    #region Properties
    public SqlServerDeleteVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public SqlServerDelete(DbContext dbContext) : base(dbContext)
    {
        this.DialectVisitor = this.Visitor as SqlServerDeleteVisitor;
    }
    #endregion

    #region Sharding
    public new ISqlServerDelete<TEntity> UseTable(params string[] tableNames)
    {
        base.UseTable(tableNames);
        return this;
    }
    public new ISqlServerDelete<TEntity> UseTable(Func<string, bool> tableNamePredicate)
    {
        base.UseTable(tableNamePredicate);
        return this;
    }
    public new ISqlServerDelete<TEntity> UseTableBy(object field1Value, object field2Value = null)
    {
        base.UseTableBy(field1Value, field2Value);
        return this;
    }
    public new ISqlServerDelete<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        base.UseTableByRange(beginFieldValue, endFieldValue);
        return this;
    }
    public new ISqlServerDelete<TEntity> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new ISqlServerDelete<TEntity> UseTableSchema(string tableSchema)
    {
        base.UseTableSchema(tableSchema);
        return this;
    }
    #endregion

    #region Where
    public new ISqlServerContinuedDelete<TEntity> Where(object keys)
        => base.Where(keys) as ISqlServerContinuedDelete<TEntity>;
    public new ISqlServerContinuedDelete<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public new ISqlServerContinuedDelete<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as ISqlServerContinuedDelete<TEntity>;
    #endregion
}