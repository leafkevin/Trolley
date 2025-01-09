using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public class PostgreSqlDelete<TEntity> : Delete<TEntity>, IPostgreSqlDelete<TEntity>
{
    #region Properties
    public PostgreSqlDeleteVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public PostgreSqlDelete(DbContext dbContext) : base(dbContext)
    {
        this.DialectVisitor = this.Visitor as PostgreSqlDeleteVisitor;
    }
    #endregion

    #region Sharding
    public new IPostgreSqlDelete<TEntity> UseTable(params string[] tableNames)
    {
        base.UseTable(tableNames);
        return this;
    }
    public new IPostgreSqlDelete<TEntity> UseTable(Func<string, bool> tableNamePredicate)
    {
        base.UseTable(tableNamePredicate);
        return this;
    }
    public new IPostgreSqlDelete<TEntity> UseTableBy(object field1Value, object field2Value = null)
    {
        base.UseTableBy(field1Value, field2Value);
        return this;
    }
    public new IPostgreSqlDelete<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        base.UseTableByRange(beginFieldValue, endFieldValue);
        return this;
    }
    public new IPostgreSqlDelete<TEntity> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IPostgreSqlDelete<TEntity> UseTableSchema(string tableSchema)
    {
        base.UseTableSchema(tableSchema);
        return this;
    }
    #endregion

    #region Where
    public new IPostgreSqlContinuedDelete<TEntity> Where(object keys)
        => base.Where(keys) as IPostgreSqlContinuedDelete<TEntity>;
    public new IPostgreSqlContinuedDelete<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlContinuedDelete<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlContinuedDelete<TEntity>;
    #endregion
}