using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public class PostgreSqlCreateConflictDoUpdate<TEntity> : PostgreSqlIdentitiedCreated, IPostgreSqlCreateConflictDoUpdate<TEntity>
{
    private PostgreSqlCreateVisitor dialectVisitor;

    public PostgreSqlCreateConflictDoUpdate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.dialectVisitor = visitor as PostgreSqlCreateVisitor;
        this.dialectVisitor.UpdateBuilder = new(" ON CONFLICT");
    }

    #region DoNothing
    public virtual IIdentitiedCreated DoNothing()
    {
        this.dialectVisitor.DoNothing();
        return this;
    }
    #endregion

    #region UseKeys
    public virtual IPostgreSqlCreateConflictDoUpdate<TEntity> UseKeys()
    {
        this.dialectVisitor.UseKeys();
        return this;
    }
    #endregion

    #region UseConstraint
    public virtual IPostgreSqlCreateConflictDoUpdate<TEntity> UseConstraint(string constraintName)
    {
        this.dialectVisitor.UseConstraint(constraintName);
        return this;
    }
    #endregion

    #region Set
    public virtual IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        this.dialectVisitor.SetObjectExpr(fieldsAssignment);
        return this;
    }
    public virtual IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (!condition) return this;
        return this.Set(fieldsAssignment);
    }
    public IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, object fieldValue)
    {
        this.dialectVisitor.SetFieldExpr(fieldSelector, fieldValue);
        return this;
    }
    public IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, object fieldValue)
    {
        if (condition) this.Set(fieldSelector, fieldValue);
        return this;
    }
    public IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, object>> valueGetter)
    {
        this.dialectVisitor.SetFieldExprs(fieldSelector, valueGetter);
        return this;
    }
    public IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, object>> valueGetter)
    {
        if (condition) this.Set(fieldSelector, valueGetter);
        return this;
    }
    #endregion

    #region Where
    public virtual IPostgreSqlCreateConflictDoUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
        this.dialectVisitor.VisitSetWhere(predicate);
        return this;
    }
    #endregion

    #region Returning
    public virtual IResultCommand<TResult> Returning<TResult>(string fieldNames)
    {
        if (string.IsNullOrEmpty(fieldNames))
            throw new ArgumentNullException(nameof(fieldNames));
        this.dialectVisitor.Returning(fieldNames);
        return this.OrmProvider.NewResultCreated<TResult>(this.DbContext, this.Visitor);
    }
    public virtual IResultCommand<TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));
        this.dialectVisitor.Returning(fieldsSelector);
        return this.OrmProvider.NewResultCreated<TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class PostgreSqlBulkCreateConflictDoUpdate<TEntity> : PostgreSqlCreated, IPostgreSqlBulkCreateConflictDoUpdate<TEntity>
{
    private PostgreSqlCreateVisitor dialectVisitor;

    public PostgreSqlBulkCreateConflictDoUpdate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.dialectVisitor = visitor as PostgreSqlCreateVisitor;
        this.dialectVisitor.UpdateBuilder = new(" ON CONFLICT");
    }

    #region DoNothing
    public virtual ICreated DoNothing()
    {
        this.dialectVisitor.DoNothing();
        return this;
    }
    #endregion

    #region UseKeys
    public virtual IPostgreSqlBulkCreateConflictDoUpdate<TEntity> UseKeys()
    {
        this.dialectVisitor.UseKeys();
        return this;
    }
    #endregion

    #region UseConstraint
    public virtual IPostgreSqlBulkCreateConflictDoUpdate<TEntity> UseConstraint(string constraintName)
    {
        this.dialectVisitor.UseConstraint(constraintName);
        return this;
    }
    #endregion

    #region Set
    public virtual IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        this.dialectVisitor.SetObjectExpr(fieldsAssignment);
        return this;
    }
    public virtual IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (!condition) return this;
        return this.Set(fieldsAssignment);
    }
    public IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, object fieldValue)
    {
        this.dialectVisitor.SetFieldExpr(fieldSelector, fieldValue);
        return this;
    }
    public IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, object fieldValue)
    {
        if (condition) this.Set(fieldSelector, fieldValue);
        return this;
    }
    public IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, object>> valueGetter)
    {
        this.dialectVisitor.SetFieldExprs(fieldSelector, valueGetter);
        return this;
    }
    public IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, object>> valueGetter)
    {
        if (condition) this.Set(fieldSelector, valueGetter);
        return this;
    }
    #endregion

    #region Where
    public virtual IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
        this.dialectVisitor.SetWhere(predicate);
        return this;
    }
    #endregion

    #region Returning
    public virtual IBulkResultCommand<TResult> Returning<TResult>(string fieldNames)
    {
        if (string.IsNullOrEmpty(fieldNames))
            throw new ArgumentNullException(nameof(fieldNames));
        this.dialectVisitor.Returning(fieldNames);
        return this.OrmProvider.NewBulkResultCreated<TResult>(this.DbContext, this.Visitor);
    }
    public virtual IBulkResultCommand<TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));
        this.dialectVisitor.Returning(fieldsSelector);
        return this.OrmProvider.NewBulkResultCreated<TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}