using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public class MySqlCreateDuplicateKeyUpdate : MySqlIdentitiedCreated, IMySqlCreateDuplicateKeyUpdate
{
    public MySqlCreateDuplicateKeyUpdate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.dialectVisitor.UpdateBuilder = new(" ON DUPLICATE KEY UPDATE ");
    }
    public IMySqlCreateDuplicateKeyUpdate UseAlias(string aliasName = "newRow")
    {
        this.dialectVisitor.RowAlias = aliasName;
        this.dialectVisitor.IsUseSetAlias = true;
        this.dialectVisitor.UpdateBuilder.Insert(0, $" AS {aliasName}");
        return this;
    }
    public IMySqlCreateDuplicateKeyUpdate Set(object updateObj)
    {
        this.dialectVisitor.SetObject(updateObj);
        return this;
    }
    public IMySqlCreateDuplicateKeyUpdate Set(bool condition, object updateObj)
    {
        if (condition) this.Set(updateObj);
        return this;
    }
    public IMySqlCreateDuplicateKeyUpdate Set(string fieldName, object fieldValue)
    {
        this.dialectVisitor.SetField(fieldName, fieldValue);
        return this;
    }
    public IMySqlCreateDuplicateKeyUpdate Set(bool condition, string fieldName, object fieldValue)
    {
        if (condition) this.Set(fieldName, fieldValue);
        return this;
    }
    public IResultCommand<TResult> Returning<TResult>(string fieldNames)
    {
        this.dialectVisitor.Returning(fieldNames);
        return dialectVisitor.OrmProvider.NewResultCreated<TResult>(this.DbContext, this.Visitor);
    }
}
public class MySqlBulkCreateDuplicateKeyUpdate : MySqlCreated, IMySqlBulkCreateDuplicateKeyUpdate
{
    public MySqlBulkCreateDuplicateKeyUpdate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.dialectVisitor.UpdateBuilder = new(" ON DUPLICATE KEY UPDATE ");
    }
    public TField Values<TField>(TField fieldSelector) => throw new NotImplementedException();
    public IMySqlBulkCreateDuplicateKeyUpdate UseAlias(string aliasName = "newRow")
    {
        this.dialectVisitor.RowAlias = aliasName;
        return this;
    }
    public IMySqlBulkCreateDuplicateKeyUpdate Set(object updateObj)
    {
        this.dialectVisitor.SetObject(updateObj);
        return this;
    }
    public IMySqlBulkCreateDuplicateKeyUpdate Set(bool condition, object updateObj)
    {
        if (condition) this.Set(updateObj);
        return this;
    }
    public IMySqlBulkCreateDuplicateKeyUpdate Set(string fieldName, object fieldValue)
    {
        this.dialectVisitor.SetField(fieldName, fieldValue);
        return this;
    }
    public IMySqlBulkCreateDuplicateKeyUpdate Set(bool condition, string fieldName, object fieldValue)
    {
        if (condition) this.Set(fieldName, fieldValue);
        return this;
    }
    public IBulkResultCommand<TResult> Returning<TResult>(string fieldNames)
    {
        this.dialectVisitor.Returning(fieldNames);
        return dialectVisitor.OrmProvider.NewBulkResultCreated<TResult>(this.DbContext, this.Visitor);
    }
}

public class MySqlCreateDuplicateKeyUpdate<TEntity> : MySqlCreateDuplicateKeyUpdate, IMySqlCreateDuplicateKeyUpdate<TEntity>
{
    public MySqlCreateDuplicateKeyUpdate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }

    public new IMySqlCreateDuplicateKeyUpdate<TEntity> UseAlias(string aliasName = "newRow")
        => base.UseAlias(aliasName) as IMySqlCreateDuplicateKeyUpdate<TEntity>;
    public new IMySqlCreateDuplicateKeyUpdate<TEntity> Set(object updateObj)
        => base.Set(updateObj) as IMySqlCreateDuplicateKeyUpdate<TEntity>;
    public new IMySqlCreateDuplicateKeyUpdate<TEntity> Set(bool condition, object updateObj)
        => base.Set(condition, updateObj) as IMySqlCreateDuplicateKeyUpdate<TEntity>;
    public new IMySqlCreateDuplicateKeyUpdate<TEntity> Set(string fieldName, object fieldValue)
         => base.Set(fieldName, fieldValue) as IMySqlCreateDuplicateKeyUpdate<TEntity>;
    public new IMySqlCreateDuplicateKeyUpdate<TEntity> Set(bool condition, string fieldName, object fieldValue)
         => base.Set(condition, fieldName, fieldValue) as IMySqlCreateDuplicateKeyUpdate<TEntity>;

    public IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        this.dialectVisitor.SetObjectExpr(fieldsAssignment);
        return this;
    }
    public IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (condition) this.Set(fieldsAssignment);
        return this;
    }
    public IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, object fieldValue)
    {
        this.dialectVisitor.SetFieldExpr(fieldSelector, fieldValue);
        return this;
    }
    public IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, object fieldValue)
    {
        if (condition) this.Set(fieldSelector, fieldValue);
        return this;
    }
    public IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, object>> valueGetter)
    {
        this.dialectVisitor.SetFieldExprs(fieldSelector, valueGetter);
        return this;
    }
    public IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, object>> valueGetter)
    {
        if (condition) this.Set(fieldSelector, valueGetter);
        return this;
    }
    public IResultCommand<TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.dialectVisitor.Returning(fieldsSelector);
        return dialectVisitor.OrmProvider.NewResultCreated<TResult>(this.DbContext, this.Visitor);
    }
}
public class MySqlBulkCreateDuplicateKeyUpdate<TEntity> : MySqlBulkCreateDuplicateKeyUpdate, IMySqlBulkCreateDuplicateKeyUpdate<TEntity>
{
    public MySqlBulkCreateDuplicateKeyUpdate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }

    public new IMySqlBulkCreateDuplicateKeyUpdate<TEntity> UseAlias(string aliasName = "newRow")
        => base.UseAlias(aliasName) as IMySqlBulkCreateDuplicateKeyUpdate<TEntity>;
    public new IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set(object updateObj)
        => base.Set(updateObj) as IMySqlBulkCreateDuplicateKeyUpdate<TEntity>;
    public new IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set(bool condition, object updateObj)
        => base.Set(condition, updateObj) as IMySqlBulkCreateDuplicateKeyUpdate<TEntity>;
    public new IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set(string fieldName, object fieldValue)
         => base.Set(fieldName, fieldValue) as IMySqlBulkCreateDuplicateKeyUpdate<TEntity>;
    public new IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set(bool condition, string fieldName, object fieldValue)
         => base.Set(condition, fieldName, fieldValue) as IMySqlBulkCreateDuplicateKeyUpdate<TEntity>;

    public IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        this.dialectVisitor.SetObjectExpr(fieldsAssignment);
        return this;
    }
    public IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (condition) this.Set(fieldsAssignment);
        return this;
    }
    public IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, object fieldValue)
    {
        this.dialectVisitor.SetFieldExpr(fieldSelector, fieldValue);
        return this;
    }
    public IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, object fieldValue)
    {
        if (condition) this.Set(fieldSelector, fieldValue);
        return this;
    }
    public IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, object>> valueGetter)
    {
        this.dialectVisitor.SetFieldExprs(fieldSelector, valueGetter);
        return this;
    }
    public IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, object>> valueGetter)
    {
        if (condition) this.Set(fieldSelector, valueGetter);
        return this;
    }
    public IBulkResultCommand<TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.dialectVisitor.Returning(fieldsSelector);
        return dialectVisitor.OrmProvider.NewBulkResultCreated<TResult>(this.DbContext, this.Visitor);
    }
}