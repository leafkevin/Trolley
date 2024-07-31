using System;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

public class SqlServerContinuedCreate<TEntity> : ContinuedCreate<TEntity>, ISqlServerContinuedCreate<TEntity>
{
    #region Properties
    public SqlServerCreateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public SqlServerContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as SqlServerCreateVisitor;
    }
    #endregion

    #region WithBy
    public override ISqlServerContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => this.WithBy(true, insertObj);
    public override ISqlServerContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
        => base.WithBy(condition, insertObj) as ISqlServerContinuedCreate<TEntity>;
    public override ISqlServerContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.WithBy(true, fieldSelector, fieldValue);
    public override ISqlServerContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.WithBy(condition, fieldSelector, fieldValue) as ISqlServerContinuedCreate<TEntity>;
    #endregion

    #region IgnoreFields
    public override ISqlServerContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames)
        => base.IgnoreFields(fieldNames) as ISqlServerContinuedCreate<TEntity>;
    public override ISqlServerContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.IgnoreFields(fieldsSelector) as ISqlServerContinuedCreate<TEntity>;
    #endregion

    #region OnlyFields
    public override ISqlServerContinuedCreate<TEntity> OnlyFields(params string[] fieldNames)
        => base.OnlyFields(fieldNames) as ISqlServerContinuedCreate<TEntity>;
    public override ISqlServerContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.OnlyFields(fieldsSelector) as ISqlServerContinuedCreate<TEntity>;
    #endregion

    #region Output
    public ISqlServerCreated<TEntity, TResult> Output<TResult>(params string[] fieldNames)
    {
        this.DialectVisitor.Output(fieldNames);
        return new SqlServerCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public ISqlServerCreated<TEntity, TResult> Output<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Output(fieldsSelector);
        return new SqlServerCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}