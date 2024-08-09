using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public class PostgreSqlContinuedCreate<TEntity> : ContinuedCreate<TEntity>, IPostgreSqlCreated<TEntity>, IPostgreSqlContinuedCreate<TEntity>
{
    #region Properties
    public PostgreSqlCreateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public PostgreSqlContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as PostgreSqlCreateVisitor;
    }
    #endregion

    #region WithBy
    public override IPostgreSqlContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => this.WithBy(true, insertObj);
    public override IPostgreSqlContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
        => base.WithBy(condition, insertObj) as IPostgreSqlContinuedCreate<TEntity>;
    public override IPostgreSqlContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.WithBy(true, fieldSelector, fieldValue);
    public override IPostgreSqlContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.WithBy(condition, fieldSelector, fieldValue) as IPostgreSqlContinuedCreate<TEntity>;
    #endregion

    #region IgnoreFields
    public override IPostgreSqlContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames)
        => base.IgnoreFields(fieldNames) as IPostgreSqlContinuedCreate<TEntity>;
    public override IPostgreSqlContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.IgnoreFields(fieldsSelector) as IPostgreSqlContinuedCreate<TEntity>;
    #endregion

    #region OnlyFields
    public override IPostgreSqlContinuedCreate<TEntity> OnlyFields(params string[] fieldNames)
        => base.OnlyFields(fieldNames) as IPostgreSqlContinuedCreate<TEntity>;
    public override IPostgreSqlContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.OnlyFields(fieldsSelector) as IPostgreSqlContinuedCreate<TEntity>;
    #endregion

    #region Returnning
    public IPostgreSqlCreated<TEntity, TResult> Returning<TResult>(params string[] fieldNames)
    {
        this.DialectVisitor.Returning(fieldNames);
        return new PostgreSqlCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public IPostgreSqlCreated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Returning(fieldsSelector);
        return new PostgreSqlCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OnConflict
    public IPostgreSqlCreated<TEntity> OnConflict<TUpdateFields>(Expression<Func<IPostgreSqlCreateConflictDoUpdate<TEntity>, TUpdateFields>> fieldsAssignment)
    {
        this.DialectVisitor.OnConflict(fieldsAssignment);
        return this;
    }
    #endregion     
}
public class PostgreSqlBulkContinuedCreate<TEntity> : ContinuedCreate<TEntity>, IPostgreSqlCreated<TEntity>, IPostgreSqlBulkContinuedCreate<TEntity>
{
    #region Properties
    public PostgreSqlCreateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public PostgreSqlBulkContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as PostgreSqlCreateVisitor;
    }
    #endregion

    #region WithBy
    public override IPostgreSqlBulkContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => this.WithBy(true, insertObj);
    public override IPostgreSqlBulkContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
        => base.WithBy(condition, insertObj) as IPostgreSqlBulkContinuedCreate<TEntity>;
    public override IPostgreSqlBulkContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.WithBy(true, fieldSelector, fieldValue);
    public override IPostgreSqlBulkContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.WithBy(condition, fieldSelector, fieldValue) as IPostgreSqlBulkContinuedCreate<TEntity>;
    #endregion

    #region IgnoreFields
    public override IPostgreSqlBulkContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames)
        => base.IgnoreFields(fieldNames) as IPostgreSqlBulkContinuedCreate<TEntity>;
    public override IPostgreSqlBulkContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.IgnoreFields(fieldsSelector) as IPostgreSqlBulkContinuedCreate<TEntity>;
    #endregion

    #region OnlyFields
    public override IPostgreSqlBulkContinuedCreate<TEntity> OnlyFields(params string[] fieldNames)
        => base.OnlyFields(fieldNames) as IPostgreSqlBulkContinuedCreate<TEntity>;
    public override IPostgreSqlBulkContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.OnlyFields(fieldsSelector) as IPostgreSqlBulkContinuedCreate<TEntity>;
    #endregion

    #region Returnning
    public IPostgreSqlBulkCreated<TEntity, TResult> Returning<TResult>(params string[] fieldNames)
    {
        this.DialectVisitor.Returning(fieldNames);
        return new PostgreSqlBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public IPostgreSqlBulkCreated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Returning(fieldsSelector);
        return new PostgreSqlBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OnConflict
    public IPostgreSqlCreated<TEntity> OnConflict<TUpdateFields>(Expression<Func<IPostgreSqlCreateConflictDoUpdate<TEntity>, TUpdateFields>> fieldsAssignment)
    {
        this.DialectVisitor.OnConflict(fieldsAssignment);
        return this;
    }
    #endregion
}