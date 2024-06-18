using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.SqlServer;

public class SqlServerBulkContinuedCreate<TEntity> : ContinuedCreate<TEntity>, ISqlServerBulkContinuedCreate<TEntity>
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
    {
        base.WithBy(insertObj);
        return this;
    }
    public override ISqlServerBulkContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
    {
        base.WithBy(condition, insertObj);
        return this;
    }
    public override ISqlServerBulkContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        base.WithBy(true, fieldSelector, fieldValue);
        return this;
    }
    public override ISqlServerBulkContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        base.WithBy(condition, fieldSelector, fieldValue);
        return this;
    }
    #endregion

    #region IgnoreFields
    public override ISqlServerBulkContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames)
    {
        base.IgnoreFields(fieldNames);
        return this;
    }
    public override ISqlServerBulkContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        base.IgnoreFields(fieldsSelector);
        return this;
    }
    #endregion

    #region OnlyFields
    public override ISqlServerBulkContinuedCreate<TEntity> OnlyFields(params string[] fieldNames)
    {
        base.OnlyFields(fieldNames);
        return this;
    }
    public override ISqlServerBulkContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        base.OnlyFields(fieldsSelector);
        return this;
    }
    #endregion

    #region Output
    public ISqlServerBulkContinuedCreate<TEntity, TResult> Output<TResult>(string[] fieldNames)
    {
        this.DialectVisitor.Output(fieldNames);
        return new SqlServerBulkContinuedCreate<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public ISqlServerBulkContinuedCreate<TEntity, TResult> Output<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Output(fieldsSelector);
        return new SqlServerBulkContinuedCreate<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class SqlServerBulkContinuedCreate<TEntity, TResult> : SqlServerBulkContinuedCreate<TEntity>, ISqlServerBulkContinuedCreate<TEntity, TResult>
{
    #region Constructor
    public SqlServerBulkContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Execute
    public new List<TResult> Execute() => this.DbContext.CreateResult<TResult>(this.Visitor);
    public new async Task<List<TResult>> ExecuteAsync(CancellationToken cancellationToken)
        => await this.DbContext.CreateResultAsync<TResult>(this.Visitor, cancellationToken);
    #endregion

    #region ExecuteIdentity
    public new List<int> ExecuteIdentity()
        => this.DbContext.CreateResult<int>(this.Visitor);
    public new async Task<List<int>> ExecuteIdentityAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.CreateResultAsync<int>(this.Visitor, cancellationToken);
    public new List<long> ExecuteIdentityLong()
        => this.DbContext.CreateResult<long>(this.Visitor);
    public new async Task<List<long>> ExecuteIdentityLongAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.CreateResultAsync<long>(this.Visitor, cancellationToken);
    #endregion
}