using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public static class MySqlExtensions
{
    #region IgnoreInto
    public static ICreate IgnoreInto(this ICreate instance)
    {
        var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
        dialectVisitor.IsUseIgnoreInto = true;
        return instance;
    }
    public static ICreate<TEntity> IgnoreInto<TEntity>(this ICreate<TEntity> instance)
    {
        var baseInstance = instance as ICreate;
        return baseInstance.IgnoreInto() as ICreate<TEntity>;
    }
    #endregion

    #region WithBulkCopy
    public static IBulkContinuedCreate WithBulkCopy(this ICreate instance, IEnumerable insertObjs, int? timeoutSeconds = null)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));
        bool isEmpty = true;
        foreach (var insertObj in insertObjs)
        {
            isEmpty = false;
            break;
        }
        if (isEmpty) throw new Exception("批量更新，insertObjs参数至少要有一条数据");
        var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
        dialectVisitor.WithBulkCopy(insertObjs, timeoutSeconds);
        return dialectVisitor.OrmProvider.NewBulkContinuedCreate(instance.DbContext, dialectVisitor);
    }
    public static IBulkContinuedCreate<TEntity> WithBulkCopy<TEntity>(this ICreate<TEntity> instance, IEnumerable insertObjs, int? timeoutSeconds = null)
    {
        var baseInstance = instance as ICreate;
        return baseInstance.WithBulkCopy(insertObjs, timeoutSeconds) as BulkContinuedCreate<TEntity>;
    }
    #endregion

    #region SetBulkCopy
    public static IBulkContinuedUpdate SetBulkCopy(this IUpdate instance, IEnumerable updateObjs, int? timeoutSeconds = null)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));

        if (updateObjs is IDictionary<string, object>)
            throw new NotSupportedException("批量更新，单个对象类型只支持命名对象、匿名对象或是字典对象");

        bool isEmpty = true;
        foreach (var updateObj in updateObjs)
        {
            isEmpty = false;
            break;
        }
        if (isEmpty) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
        var dialectVisitor = instance.Visitor as MySqlUpdateVisitor;
        dialectVisitor.SetBulkCopy(updateObjs, timeoutSeconds);
        return dialectVisitor.OrmProvider.NewBulkContinuedUpdate(instance.DbContext, dialectVisitor);
    }
    public static IBulkContinuedUpdate<TEntity> SetBulkCopy<TEntity>(this IUpdate<TEntity> instance, IEnumerable updateObjs, int? timeoutSeconds = null)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));

        if (updateObjs is IDictionary<string, object>)
            throw new NotSupportedException("批量更新，单个对象类型只支持命名对象、匿名对象或是字典对象");

        bool isEmpty = true;
        foreach (var updateObj in updateObjs)
        {
            isEmpty = false;
            break;
        }
        if (isEmpty) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
        var dialectVisitor = instance.Visitor as MySqlUpdateVisitor;
        dialectVisitor.SetBulkCopy(updateObjs, timeoutSeconds);
        return dialectVisitor.OrmProvider.NewBulkContinuedUpdate<TEntity>(instance.DbContext, dialectVisitor);
    }
    #endregion

    #region Values
    public static TFields Values<TEntity, TFields>(this TEntity entity, TFields fields) => throw new NotImplementedException();
    #endregion

    #region OnDuplicateKeyUpdate
    public static IMySqlCreateDuplicateKeyUpdate<TEntity> OnDuplicateKeyUpdate<TEntity>(this IContinuedCreate<TEntity> instance)
        => new MySqlCreateDuplicateKeyUpdate<TEntity>(instance.DbContext, instance.Visitor);
    public static IMySqlBulkCreateDuplicateKeyUpdate<TEntity> OnDuplicateKeyUpdate<TEntity>(this IBulkContinuedCreate<TEntity> instance)
        => new MySqlBulkCreateDuplicateKeyUpdate<TEntity>(instance.DbContext, instance.Visitor);
    public static IMySqlBulkCreateDuplicateKeyUpdate<TEntity> OnDuplicateKeyUpdate<TEntity, T>(this IFromCommand<TEntity, T> instance)
    {
        var fromSql = instance.Visitor.BuildCommandSql(false, out _);
        var visitor = instance.NewCreateVisitor(fromSql);
        return new MySqlBulkCreateDuplicateKeyUpdate<TEntity>(instance.DbContext, visitor);
    }
    #endregion

    #region Join
    public static IUpdateJoin<TEntity, T> InnerJoin<TEntity, T>(this IUpdate<TEntity> instance, Expression<Func<TEntity, T, bool>> joinOn)
    {
        if (joinOn == null) throw new ArgumentNullException(nameof(joinOn));
        instance.Visitor.Join("INNER JOIN", typeof(T), joinOn);
        return instance.DbContext.OrmProvider.NewUpdateJoin<TEntity, T>(instance.DbContext, instance.Visitor);
    }
    public static IUpdateJoin<TEntity, T> LeftJoin<TEntity, T>(this IUpdate<TEntity> instance, Expression<Func<TEntity, T, bool>> joinOn)
    {
        if (joinOn == null) throw new ArgumentNullException(nameof(joinOn));
        instance.Visitor.Join("LEFT JOIN", typeof(T), joinOn);
        return instance.DbContext.OrmProvider.NewUpdateJoin<TEntity, T>(instance.DbContext, instance.Visitor);
    }
    #endregion

    #region Returnning
    public static IResultCommand<TResult> Returning<TResult>(this IContinuedCreate instance, string fieldNames)
    {
        var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
        dialectVisitor.Returning(fieldNames);
        return dialectVisitor.OrmProvider.NewResultCreated<TResult>(instance.DbContext, instance.Visitor);
    }
    public static IResultCommand<TResult> Returning<TEntity, TResult>(this IContinuedCreate<TEntity> instance, Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
        dialectVisitor.Returning(fieldsSelector);
        return dialectVisitor.OrmProvider.NewResultCreated<TResult>(instance.DbContext, instance.Visitor);
    }
    public static IBulkResultCommand<TResult> Returning<TResult>(this IBulkContinuedCreate instance, string fieldNames)
    {
        var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
        dialectVisitor.Returning(fieldNames);
        return dialectVisitor.OrmProvider.NewBulkResultCreated<TResult>(instance.DbContext, instance.Visitor);
    }
    public static IBulkResultCommand<TResult> Returning<TEntity, TResult>(this IBulkContinuedCreate instance, Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
        dialectVisitor.Returning(fieldsSelector);
        return dialectVisitor.OrmProvider.NewBulkResultCreated<TResult>(instance.DbContext, instance.Visitor);
    }

    public static IBulkResultCommand<TResult> Returning<TEntity, T, TResult>(this IFromCommand<TEntity, T> instance, string fieldNames)
    {
        var sql = instance.Visitor.BuildCommandSql(false, out _);
        var visitor = instance.NewCreateVisitor(sql) as MySqlCreateVisitor;
        visitor.Returning(fieldNames);
        return visitor.OrmProvider.NewBulkResultCreated<TResult>(instance.DbContext, visitor);
    }

    public static IBulkResultCommand<TResult> Returning<TResult>(this IDelete instance, string fieldNames)
    {
        var dialectVisitor = instance.Visitor as MySqlDeleteVisitor;
        dialectVisitor.Returning(fieldNames);
        return dialectVisitor.OrmProvider.NewResultDeleted<TResult>(instance.DbContext, instance.Visitor);
    }
    #endregion     
}