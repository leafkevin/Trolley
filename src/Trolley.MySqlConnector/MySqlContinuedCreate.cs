using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public class MySqlContinuedCreate<TEntity> : MySqlCreated<TEntity>, IMySqlContinuedCreate<TEntity>
{
    #region Constructor
    public MySqlContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as MySqlCreateVisitor;
    }
    #endregion

    #region WithBy
    public IMySqlContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => this.WithBy(true, insertObj);
    public IMySqlContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        if (insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithBulkBy方法");
        if (!typeof(TInsertObject).IsEntityType(out _))
            throw new NotSupportedException("方法WithBy<TInsertObject>(TInsertObject insertObj)只支持类对象参数，不支持基础类型参数");

        if (condition) this.Visitor.WithBy(insertObj);
        return this;
    }
    public IMySqlContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.WithBy(true, fieldSelector, fieldValue);
    public IMySqlContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        if (condition) this.Visitor.WithByField(fieldSelector, fieldValue);
        return this;
    }
    #endregion

    #region IgnoreFields
    public IMySqlContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.IgnoreFields(fieldNames);
        return this;
    }
    public IMySqlContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));
        if (fieldsSelector.Body.NodeType != ExpressionType.New && fieldsSelector.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelector)},只支持New或MemberInit类型表达式");

        this.Visitor.IgnoreFields(fieldsSelector);
        return this;
    }
    #endregion

    #region OnlyFields
    public IMySqlContinuedCreate<TEntity> OnlyFields(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.OnlyFields(fieldNames);
        return this;
    }
    public IMySqlContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));
        if (fieldsSelector.Body.NodeType != ExpressionType.New && fieldsSelector.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelector)},只支持New或MemberInit类型表达式");

        this.Visitor.OnlyFields(fieldsSelector);
        return this;
    }
    #endregion

    #region OnDuplicateKeyUpdate
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

    #region IContinuedCreate
    IContinuedCreate<TEntity> IContinuedCreate<TEntity>.WithBy<TInsertObject>(TInsertObject insertObj)
        => this.WithBy(insertObj);
    IContinuedCreate<TEntity> IContinuedCreate<TEntity>.WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
        => this.WithBy(condition, insertObj);
    IContinuedCreate<TEntity> IContinuedCreate<TEntity>.WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.WithBy(condition, fieldSelector, fieldValue);
    IContinuedCreate<TEntity> IContinuedCreate<TEntity>.IgnoreFields(params string[] fieldNames)
        => this.IgnoreFields(fieldNames);
    IContinuedCreate<TEntity> IContinuedCreate<TEntity>.IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => this.IgnoreFields(fieldsSelector);
    IContinuedCreate<TEntity> IContinuedCreate<TEntity>.OnlyFields(params string[] fieldNames)
        => this.OnlyFields(fieldNames);
    IContinuedCreate<TEntity> IContinuedCreate<TEntity>.OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => this.OnlyFields(fieldsSelector);
    #endregion
}
