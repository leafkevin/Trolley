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
        => base.UseTable(tableNames) as ISqlServerDelete<TEntity>;
    public new ISqlServerDelete<TEntity> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as ISqlServerDelete<TEntity>;
    public new ISqlServerDelete<TEntity> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as ISqlServerDelete<TEntity>;
    public new ISqlServerDelete<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as ISqlServerDelete<TEntity>;
    public new ISqlServerDelete<TEntity> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as ISqlServerDelete<TEntity>;
    public new ISqlServerDelete<TEntity> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as ISqlServerDelete<TEntity>;
    #endregion

    #region UseTableSchema
    public new ISqlServerDelete<TEntity> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as ISqlServerDelete<TEntity>;
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