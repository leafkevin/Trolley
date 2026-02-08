using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public static class PostgreSqlExtensions
{
    #region Excluded
    /// <summary>
    /// 获取插入字段原值
    /// </summary>
    /// <typeparam name="TInsertObj">插入对象</typeparam>
    /// <typeparam name="TField">插入字段类型</typeparam>
    /// <param name="insertObj">插入对象</param>
    /// <param name="field">插入字段值</param>
    /// <returns>插入对象原值</returns>
    /// <exception cref="NotImplementedException"></exception>
    public static TField Excluded<TInsertObj, TField>(this TInsertObj insertObj, TField field) => throw new NotImplementedException();
    #endregion

    #region DistictOn
    public static IQuery<TEntity> DistictOn<TEntity>(this IQuery<TEntity> instance, Expression<Func<TEntity, object>> fieldsSelector)
    {
        var dialectVisitor = instance.Visitor as PostgreSqlQueryVisitor;
        dialectVisitor.DistinctOn(fieldsSelector);
        return instance;
    }
    #endregion

    #region WithBulkCopy
    public static IBulkContinuedCreate WithBulkCopy(this ICreate instance, IEnumerable insertObjs)
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
        var dialectVisitor = instance.Visitor as PostgreSqlCreateVisitor;
        dialectVisitor.WithBulkCopy(insertObjs);
        return dialectVisitor.OrmProvider.NewBulkContinuedCreate(instance.DbContext, dialectVisitor);
    }
    public static IBulkContinuedCreate<TEntity> WithBulkCopy<TEntity>(this ICreate<TEntity> instance, IEnumerable insertObjs)
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
        var dialectVisitor = instance.Visitor as PostgreSqlCreateVisitor;
        dialectVisitor.WithBulkCopy(insertObjs);
        return dialectVisitor.OrmProvider.NewBulkContinuedCreate<TEntity>(instance.DbContext, dialectVisitor);
    }
    #endregion

    #region SetBulkCopy
    public static IBulkContinuedUpdate SetBulkCopy(this IUpdate instance, IEnumerable updateObjs)
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
        var dialectVisitor = instance.Visitor as PostgreSqlUpdateVisitor;
        dialectVisitor.SetBulkCopy(updateObjs);
        return dialectVisitor.OrmProvider.NewBulkContinuedUpdate(instance.DbContext, dialectVisitor);
    }
    public static IBulkContinuedUpdate<TEntity> SetBulkCopy<TEntity>(this IUpdate<TEntity> instance, IEnumerable updateObjs)
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
        var dialectVisitor = instance.Visitor as PostgreSqlUpdateVisitor;
        dialectVisitor.SetBulkCopy(updateObjs);
        return dialectVisitor.OrmProvider.NewBulkContinuedUpdate<TEntity>(instance.DbContext, dialectVisitor);
    }
    #endregion

    #region OnConflict
    public static IPostgreSqlCreateConflictDoUpdate<TEntity> OnConflict<TEntity>(this IContinuedCreate<TEntity> instance)
        => new PostgreSqlCreateConflictDoUpdate<TEntity>(instance.DbContext, instance.Visitor);
    public static IPostgreSqlBulkCreateConflictDoUpdate<TEntity> OnConflict<TEntity>(this IBulkContinuedCreate<TEntity> instance)
        => new PostgreSqlBulkCreateConflictDoUpdate<TEntity>(instance.DbContext, instance.Visitor);
    public static IPostgreSqlBulkCreateConflictDoUpdate<TEntity> OnConflict<TEntity, T>(this IFromCommand<TEntity, T> instance)
    {
        var fromSql = instance.Visitor.BuildCommandSql(false, out _);
        var visitor = instance.NewCreateVisitor(fromSql);
        return new PostgreSqlBulkCreateConflictDoUpdate<TEntity>(instance.DbContext, visitor);
    }
    #endregion

    #region Returnning
    public static IResultCommand<TResult> Returning<TResult>(this IContinuedCreate instance, string fieldNames)
    {
        var dialectVisitor = instance.Visitor as PostgreSqlCreateVisitor;
        dialectVisitor.Returning(fieldNames);
        return dialectVisitor.OrmProvider.NewResultCreated<TResult>(instance.DbContext, instance.Visitor);
    }
    public static IResultCommand<TResult> Returning<TEntity, TResult>(this IContinuedCreate<TEntity> instance, Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        var dialectVisitor = instance.Visitor as PostgreSqlCreateVisitor;
        dialectVisitor.Returning(fieldsSelector);
        return dialectVisitor.OrmProvider.NewResultCreated<TResult>(instance.DbContext, instance.Visitor);
    }
    public static IBulkResultCommand<TResult> Returning<TResult>(this IBulkContinuedCreate instance, string fieldNames)
    {
        var dialectVisitor = instance.Visitor as PostgreSqlCreateVisitor;
        dialectVisitor.Returning(fieldNames);
        return dialectVisitor.OrmProvider.NewBulkResultCreated<TResult>(instance.DbContext, instance.Visitor);
    }
    public static IBulkResultCommand<TResult> Returning<TEntity, TResult>(this IBulkContinuedCreate<TEntity> instance, Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        var dialectVisitor = instance.Visitor as PostgreSqlCreateVisitor;
        dialectVisitor.Returning(fieldsSelector);
        return dialectVisitor.OrmProvider.NewBulkResultCreated<TResult>(instance.DbContext, instance.Visitor);
    }

    public static IBulkResultCommand<TResult> Returning<TTarget, TResult>(this IFromCommand<TTarget> instance, string fieldNames)
    {
        var sql = instance.Visitor.BuildCommandSql(false, out _);
        var visitor = instance.NewCreateVisitor(sql) as PostgreSqlCreateVisitor;
        visitor.Returning(fieldNames);
        return visitor.OrmProvider.NewBulkResultCreated<TResult>(instance.DbContext, visitor);
    }
    public static IBulkResultCommand<TResult> Returning<TTarget, TResult>(this IFromCommand<TTarget> instance, Expression<Func<TTarget, TResult>> fieldsSelector)
    {
        var sql = instance.Visitor.BuildCommandSql(false, out _);
        var visitor = instance.NewCreateVisitor(sql) as PostgreSqlCreateVisitor;
        visitor.Returning(fieldsSelector);
        return visitor.OrmProvider.NewBulkResultCreated<TResult>(instance.DbContext, visitor);
    }
    public static IBulkResultCommand<TResult> Returning<TResult>(this IContinuedUpdate instance, string fieldNames)
    {
        var dialectVisitor = instance.Visitor as PostgreSqlCreateVisitor;
        dialectVisitor.Returning(fieldNames);
        return dialectVisitor.OrmProvider.NewResultUpdated<TResult>(instance.DbContext, instance.Visitor);
    }

    public static IBulkResultCommand<TResult> Returning<TResult>(this IDelete instance, string fieldNames)
    {
        var dialectVisitor = instance.Visitor as PostgreSqlDeleteVisitor;
        dialectVisitor.Returning(fieldNames);
        return dialectVisitor.OrmProvider.NewResultDeleted<TResult>(instance.DbContext, instance.Visitor);
    }
    #endregion
}