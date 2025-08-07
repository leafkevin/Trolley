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
        => base.UseTable(tableNames) as IPostgreSqlDelete<TEntity>;
    public new IPostgreSqlDelete<TEntity> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlDelete<TEntity>;
    public new IPostgreSqlDelete<TEntity> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlDelete<TEntity>;
    public new IPostgreSqlDelete<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlDelete<TEntity>;
    public new IPostgreSqlDelete<TEntity> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlDelete<TEntity>;
    public new IPostgreSqlDelete<TEntity> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlDelete<TEntity>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlDelete<TEntity> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlDelete<TEntity>;
    #endregion

    #region Where
    public new IPostgreSqlContinuedDelete<TEntity> Where(object keys)
        => base.Where(keys) as IPostgreSqlContinuedDelete<TEntity>;
    public new IPostgreSqlContinuedDelete<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public new IPostgreSqlContinuedDelete<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IPostgreSqlContinuedDelete<TEntity>;
    public new IPostgreSqlContinuedDelete<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IPostgreSqlContinuedDelete<TEntity>;
    #endregion
}