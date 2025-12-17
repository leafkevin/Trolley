using System;
using System.Collections;
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
    public new IMySqlDelete<TEntity> WhereBy(object whereObj)
        => this.AndBy(whereObj);
    public new IMySqlDelete<TEntity> WhereBy(bool condition, object whereObj)
        => this.AndBy(condition, whereObj);
    public new IMySqlDelete<TEntity> WhereById(object whereKey)
        => this.AndById(whereKey);
    public new IMySqlDelete<TEntity> WhereById(bool condition, object whereKey)
        => this.AndById(condition, whereKey);
    public new IMySqlDelete<TEntity> WhereByIds(IEnumerable whereKeys)
        => this.AndByIds(whereKeys);
    public new IMySqlDelete<TEntity> WhereByIds(bool condition, IEnumerable whereKeys)
        => this.AndByIds(condition, whereKeys);
    public new IMySqlDelete<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public new IMySqlDelete<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public new IMySqlDelete<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public new IMySqlDelete<TEntity> AndBy(object whereObj)
       => base.AndBy(whereObj) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> AndBy(bool condition, object whereObj)
        => base.AndBy(condition, whereObj) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> AndById(object whereKey)
        => base.AndById(whereKey) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> AndById(bool condition, object whereKey)
        => base.AndById(condition, whereKey) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> AndByIds(IEnumerable whereKeys)
        => base.AndByIds(whereKeys) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> AndByIds(bool condition, IEnumerable whereKeys)
        => base.AndByIds(condition, whereKeys) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public new IMySqlDelete<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IMySqlDelete<TEntity>;
    #endregion

    #region Or
    public new IMySqlDelete<TEntity> OrBy(object whereObj)
        => base.OrBy(whereObj) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> OrBy(bool condition, object whereObj)
        => base.OrBy(condition, whereObj) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> OrById(object whereKey)
        => base.OrById(whereKey) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> OrById(bool condition, object whereKey)
        => base.OrById(condition, whereKey) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> OrByIds(IEnumerable whereKeys)
        => base.OrByIds(whereKeys) as IMySqlDelete<TEntity>;
    public new IMySqlDelete<TEntity> OrByIds(bool condition, IEnumerable whereKeys)
        => base.OrByIds(condition, whereKeys) as IMySqlDelete<TEntity>;
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