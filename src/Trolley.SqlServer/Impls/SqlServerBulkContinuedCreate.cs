using System;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

public class SqlServerBulkContinuedCreate<TEntity> : ContinuedCreate<TEntity>, ISqlServerCreated<TEntity>, ISqlServerBulkContinuedCreate<TEntity>
{
    #region Properties
    public SqlServerCreateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public SqlServerBulkContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as SqlServerCreateVisitor;
    }
    #endregion

    #region WithBy
    public override ISqlServerBulkContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => this.WithBy(true, insertObj);
    public override ISqlServerBulkContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
        => base.WithBy(condition, insertObj) as ISqlServerBulkContinuedCreate<TEntity>;
    public override ISqlServerBulkContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.WithBy(true, fieldSelector, fieldValue);
    public override ISqlServerBulkContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.WithBy(condition, fieldSelector, fieldValue) as ISqlServerBulkContinuedCreate<TEntity>;
    #endregion

    #region IgnoreFields
    public override ISqlServerBulkContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames)
        => base.IgnoreFields(fieldNames) as ISqlServerBulkContinuedCreate<TEntity>;
    public override ISqlServerBulkContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.IgnoreFields(fieldsSelector) as ISqlServerBulkContinuedCreate<TEntity>;
    #endregion

    #region OnlyFields
    public override ISqlServerBulkContinuedCreate<TEntity> OnlyFields(params string[] fieldNames)
        => base.OnlyFields(fieldNames) as ISqlServerBulkContinuedCreate<TEntity>;
    public override ISqlServerBulkContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.OnlyFields(fieldsSelector) as ISqlServerBulkContinuedCreate<TEntity>;
    #endregion

    #region Output
    public ISqlServerBulkCreated<TEntity, TResult> Output<TResult>(params string[] fieldNames)
    {
        this.DialectVisitor.Output(fieldNames);
        return new SqlServerBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public ISqlServerBulkCreated<TEntity, TResult> Output<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Output(fieldsSelector);
        return new SqlServerBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}