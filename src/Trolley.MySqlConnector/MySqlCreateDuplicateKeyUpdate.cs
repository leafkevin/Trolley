using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public class MySqlCreateDuplicateKeyUpdate<TEntity> : MySqlIdentitiedCreated, IMySqlCreateDuplicateKeyUpdate<TEntity>
{
    private MySqlCreateVisitor dialectVisitor;

    public MySqlCreateDuplicateKeyUpdate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.dialectVisitor = visitor as MySqlCreateVisitor;
        this.dialectVisitor.UpdateBuilder = new();
    }
    public TField Values<TField>(TField fieldSelector) => throw new NotImplementedException();
    public IMySqlCreateDuplicateKeyUpdate<TEntity> UseAlias(string aliasName = "newRow")
    {
        this.dialectVisitor.RowAlias = aliasName;
        return this;
    }
    public IMySqlCreateDuplicateKeyUpdate<TEntity> Set(object updateObj)
    {
        this.dialectVisitor.VisitSetObject(updateObj);
        return this;
    }
    public IMySqlCreateDuplicateKeyUpdate<TEntity> Set(bool condition, object updateObj)
    {
        if (condition) this.Set(updateObj);
        return this;
    }
    public IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        this.dialectVisitor.VisitSetExpression(fieldsAssignment);
        return this;
    }
    public IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (condition) this.Set(fieldsAssignment);
        return this;
    }
    public IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        this.dialectVisitor.VisitWithSetField(fieldSelector, fieldValue);
        return this;
    }
    public IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (condition) this.Set(fieldSelector, fieldValue);
        return this;
    }
    public IResultCommand<TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.dialectVisitor.Returning(fieldsSelector);
        return dialectVisitor.OrmProvider.NewResultCreated<TResult>(this.DbContext, this.Visitor);
    }
}
public class MySqlBulkCreateDuplicateKeyUpdate<TEntity> : MySqlCreated, IMySqlBulkCreateDuplicateKeyUpdate<TEntity>
{
    private MySqlCreateVisitor dialectVisitor;

    public MySqlBulkCreateDuplicateKeyUpdate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.dialectVisitor = visitor as MySqlCreateVisitor;
        this.dialectVisitor.UpdateBuilder = new();
    }
    public TField Values<TField>(TField fieldSelector) => throw new NotImplementedException();
    public IMySqlBulkCreateDuplicateKeyUpdate<TEntity> UseAlias(string aliasName = "newRow")
    {
        this.dialectVisitor.RowAlias = aliasName;
        this.dialectVisitor.IsUseSetAlias = true;
        return this;
    }
    public IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set(object updateObj)
    {
        this.dialectVisitor.VisitSetObject(updateObj);
        return this;
    }
    public IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set(bool condition, object updateObj)
    {
        if (condition) this.Set(updateObj);
        return this;
    }
    public IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        this.dialectVisitor.VisitSetExpression(fieldsAssignment);
        return this;
    }
    public IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (condition) this.Set(fieldsAssignment);
        return this;
    }
    public IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        this.dialectVisitor.VisitWithSetField(fieldSelector, fieldValue);
        return this;
    }
    public IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (condition) this.Set(fieldSelector, fieldValue);
        return this;
    }
    public IBulkResultCommand<TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.dialectVisitor.Returning(fieldsSelector);
        return dialectVisitor.OrmProvider.NewBulkResultCreated<TResult>(this.DbContext, this.Visitor);
    }
}
