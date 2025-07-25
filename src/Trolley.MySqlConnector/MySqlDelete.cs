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
        => base.UseTable(tableNames) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IMySqlDelete<TEntity>;
    #endregion

    #region UseTableSchema
    public new IMySqlDelete<TEntity> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IMySqlDelete<TEntity>;
    #endregion

    #region Where
    public new IMySqlContinuedDelete<TEntity> Where(object keys)
        => base.Where(keys) as IMySqlContinuedDelete<TEntity>;
    public new IMySqlContinuedDelete<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public new IMySqlContinuedDelete<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IMySqlContinuedDelete<TEntity>;
    public new IMySqlContinuedDelete<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IMySqlContinuedDelete<TEntity>;
    #endregion
}