using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public class MySqlDelete<TEntity> : Delete<TEntity>, IMySqlDelete<TEntity>
{
    #region Properties
    public MySqlDeleteVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public MySqlDelete(DbContext dbContext) : base(dbContext)
    {
        this.DialectVisitor = this.Visitor as MySqlDeleteVisitor;
    }
    #endregion

    #region Sharding
    public new IMySqlDelete<TEntity> UseTable(params string[] tableNames)
    {
        base.UseTable(tableNames);
        return this;
    }
    public new IMySqlDelete<TEntity> UseTable(Func<string, bool> tableNamePredicate)
    {
        base.UseTable(tableNamePredicate);
        return this;
    }
    public new IMySqlDelete<TEntity> UseTableBy(object field1Value, object field2Value = null)
    {
        base.UseTableBy(field1Value, field2Value);
        return this;
    }
    public new IMySqlDelete<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        base.UseTableByRange(beginFieldValue, endFieldValue);
        return this;
    }
    public new IMySqlDelete<TEntity> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMySqlDelete<TEntity> UseTableSchema(string tableSchema)
    {
        base.UseTableSchema(tableSchema);
        return this;
    }
    #endregion

    #region Where
    public new IMySqlContinuedDelete<TEntity> Where(object keys)
        => base.Where(keys) as IMySqlContinuedDelete<TEntity>;
    public new IMySqlContinuedDelete<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public new IMySqlContinuedDelete<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IMySqlContinuedDelete<TEntity>;
    #endregion
}