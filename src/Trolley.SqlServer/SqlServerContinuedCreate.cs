using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

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
    {
        base.WithBy(insertObj);
        return this;
    }
    public override ISqlServerContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
    {
        base.WithBy(condition, insertObj);
        return this;
    }
    public override ISqlServerContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        base.WithBy(true, fieldSelector, fieldValue);
        return this;
    }
    public override ISqlServerContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        base.WithBy(condition, fieldSelector, fieldValue);
        return this;
    }
    #endregion

    #region IgnoreFields
    public override ISqlServerContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames)
    {
        base.IgnoreFields(fieldNames);
        return this;
    }
    public override ISqlServerContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        base.IgnoreFields(fieldsSelector);
        return this;
    }
    #endregion

    #region OnlyFields
    public override ISqlServerContinuedCreate<TEntity> OnlyFields(params string[] fieldNames)
    {
        base.OnlyFields(fieldNames);
        return this;
    }
    public override ISqlServerContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        base.OnlyFields(fieldsSelector);
        return this;
    }
    #endregion

    #region Output
    public ISqlServerContinuedCreate<TEntity, TResult> Output<TResult>(string[] fieldNames)
    {
        this.DialectVisitor.Output(fieldNames);
        return new SqlServerContinuedCreate<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public ISqlServerContinuedCreate<TEntity, TResult> Output<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Output(fieldsSelector);
        return new SqlServerContinuedCreate<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class SqlServerContinuedCreate<TEntity, TResult> : SqlServerContinuedCreate<TEntity>, ISqlServerContinuedCreate<TEntity, TResult>
{
    #region Constructor
    public SqlServerContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Execute
    public new TResult Execute()
        => this.DbContext.CreateResult<TResult>((command, dbContext) => this.Visitor.BuildCommand(command, true));
    public new async Task<TResult> ExecuteAsync(CancellationToken cancellationToken)
        => await this.DbContext.CreateResultAsync<TResult>((command, dbContext) => this.Visitor.BuildCommand(command, true), cancellationToken);
    #endregion
}