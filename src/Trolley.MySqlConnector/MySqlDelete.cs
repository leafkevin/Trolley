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
    public new IMySqlDelete<TEntity> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> UseTableByRange(params object[] fieldValues)
        => base.UseTableByRange(fieldValues) as IMySqlDelete<TEntity>;
    #endregion

    #region UseTableSchema
    public new IMySqlDelete<TEntity> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IMySqlDelete<TEntity>;
    #endregion

    #region Where
    public new IMySqlDelete<TEntity> Where(object keys)
        => base.WhereBy(keys) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public new IMySqlDelete<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IMySqlDelete<TEntity>;
    #endregion

    #region And
    public new IMySqlDelete<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public new IMySqlDelete<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IMySqlDelete<TEntity>;
    #endregion

    #region Or
    public new IMySqlDelete<TEntity> Or(Expression<Func<TEntity, bool>> predicate)
        => this.Or(true, predicate);
    public new IMySqlDelete<TEntity> Or(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> OrPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IMySqlDelete<TEntity>;
    #endregion

    #region Returnning
    public IMySqlDeleted<TEntity, TResult> Returning<TResult>(string fieldNames)
    {
        this.DialectVisitor.Returning(fieldNames);
        return new MySqlDeleted<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public IMySqlDeleted<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Returning(fieldsSelector);
        return new MySqlDeleted<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}