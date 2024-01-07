using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public class MySqlContinuedCreate<TEntity> : ContinuedCreate<TEntity>, IMySqlContinuedCreate<TEntity>
{
    #region Properties
    public MySqlCreateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public MySqlContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as MySqlCreateVisitor;
    }
    #endregion

    #region WithBy
    public new IMySqlContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => this.WithBy(true, insertObj);
    public new IMySqlContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
    {
        base.WithBy(condition, insertObj);
        return this;
    }
    public new IMySqlContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.WithBy(true, fieldSelector, fieldValue);
    public new IMySqlContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        base.WithBy(condition, fieldSelector, fieldValue);
        return this;
    }
    #endregion

    #region Constructor
    public IMySqlContinuedCreate<TEntity> OnDuplicateKeyUpdate<TUpdateFields>(TUpdateFields updateObj)
    {
        this.DialectVisitor.OnDuplicateKeyUpdate(updateObj);
        return this;
    }
    public IMySqlContinuedCreate<TEntity> OnDuplicateKeyUpdate<TUpdateFields>(Expression<Func<IMySqlCreateDuplicateKeyUpdate<TEntity>, TUpdateFields>> fieldsAssignment)
    {
        this.DialectVisitor.OnDuplicateKeyUpdate(fieldsAssignment);
        return this;
    }
    #endregion

    #region Constructor
    public new IMySqlContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames)
    {
        base.IgnoreFields(fieldNames);
        return this;
    }
    public new IMySqlContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        base.IgnoreFields(fieldsSelector);
        return this;
    }
    #endregion

    #region OnlyFields
    public new IMySqlContinuedCreate<TEntity> OnlyFields(params string[] fieldNames)
    {
        base.OnlyFields(fieldNames);
        return this;
    }
    public new IMySqlContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        base.OnlyFields(fieldsSelector);
        return this;
    }
    #endregion 
}
