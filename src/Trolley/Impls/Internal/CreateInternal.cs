using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public class CreateInternal
{
    #region Properties
    public DbContext DbContext { get; protected set; }
    public ICreateVisitor Visitor { get; protected set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region WithBy
    protected virtual void WithByInternal<TInsertObject>(bool condition, TInsertObject insertObj, ActionMode? actionMode = null)
    {
        if (!condition) return;
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        if (insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithBulkBy方法");
        var insertObjType = typeof(TInsertObject);
        if (!insertObjType.IsEntityType(out _))
            throw new NotSupportedException($"方法WithBy只支持类对象参数，不支持基础类型参数, insertObj类型: {insertObjType.FullName}");

        this.Visitor.WithBy(insertObj, actionMode);
    }
    protected virtual void WithByInternal<TField>(bool condition, Expression fieldSelector, TField fieldValue)
    {
        if (!condition) return;
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        this.Visitor.WithByField(fieldSelector, fieldValue);
    }
    #endregion

    #region WithBulk
    protected virtual void WithBulkInternal(IEnumerable insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        if (insertObjs is string || insertObjs is IDictionary<string, object>)
            throw new NotSupportedException("批量插入，单个对象类型只支持命名对象、匿名对象或是字典对象");
        bool isEmpty = true;
        foreach (var insertObj in insertObjs)
        {
            isEmpty = false;
            break;
        }
        if (isEmpty) throw new Exception("批量插入，insertObjs参数至少要有一条数据");

        this.Visitor.WithBulk(insertObjs, bulkCount);
    }
    #endregion

    #region From
    protected virtual IQueryVisitor FromInternal(params Type[] entitiyTypes)
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.AddTable(this.Visitor.Tables[0]);
        queryVisitor.From('a', entitiyTypes);
        queryVisitor.IsFromCommand = true;
        return queryVisitor;
    }
    #endregion

    #region IgnoreFields
    protected virtual void IgnoreFieldsInternal(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.IgnoreFields(fieldNames);
    }
    protected virtual void IgnoreFieldsInternal(LambdaExpression fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));
        if (fieldsSelector.Body.NodeType != ExpressionType.New && fieldsSelector.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelector)},只支持New或MemberInit类型表达式");

        this.Visitor.IgnoreFields(fieldsSelector);
    }
    #endregion

    #region OnlyFields
    protected virtual void OnlyFieldsInternal(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.OnlyFields(fieldNames);
    }
    protected virtual void OnlyFieldsInternal(LambdaExpression fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));
        if (fieldsSelector.Body.NodeType != ExpressionType.New && fieldsSelector.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelector)},只支持New或MemberInit类型表达式");

        this.Visitor.OnlyFields(fieldsSelector);
    }
    #endregion
}
