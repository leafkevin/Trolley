﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public static class OrmExtensions
{
    #region QueryFirst/Query/QueryDictionary
    /// <summary>
    /// 查询TEntity实体表满足表达式wherePredicate条件的第一条记录，条件表达式wherePredicate可以为null，为null时，查询所有记录的第一条
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="wherePredicate">条件表达式</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    public static TEntity QueryFirst<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> wherePredicate = null)
    {
        var query = repository.From<TEntity>();
        if (wherePredicate != null)
            query.Where(wherePredicate);
        return query.First();
    }
    /// <summary>
    /// 查询TEntity实体表满足表达式wherePredicate条件的第一条记录，条件表达式wherePredicate可以为null，为null时，查询所有记录的第一条
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="wherePredicate">条件表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    public static async Task<TEntity> QueryFirstAsync<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> wherePredicate = null, CancellationToken cancellationToken = default)
    {
        var query = repository.From<TEntity>();
        if (wherePredicate != null)
            query.Where(wherePredicate);
        return await query.FirstAsync(cancellationToken);
    }
    /// <summary>
    /// 查询TEntity实体表满足表达式wherePredicate条件的所有记录，条件表达式wherePredicate可以为null，为null时，查询所有记录
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="wherePredicate">条件表达式</param>
    /// <returns>返回查询结果，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表</returns>
    public static List<TEntity> Query<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> wherePredicate = null)
    {
        var query = repository.From<TEntity>();
        if (wherePredicate != null)
            query.Where(wherePredicate);
        return query.ToList();
    }
    /// <summary>
    /// 查询TEntity实体表满足表达式wherePredicate条件的所有记录，条件表达式wherePredicate可以为null，为null时，查询所有记录，记录不存在时返回没有任何元素的空列表
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="wherePredicate">条件表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表</returns>
    public static async Task<List<TEntity>> QueryAsync<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> wherePredicate = null, CancellationToken cancellationToken = default)
    {
        var query = repository.From<TEntity>();
        if (wherePredicate != null)
            query.Where(wherePredicate);
        return await query.ToListAsync(cancellationToken);
    }
    /// <summary>
    /// 查询TEntity实体表满足表达式wherePredicate条件的所有记录，返回TEntity实体所有字段的记录并转化为Dictionary&lt;TKey, TValue&gt;字典，记录不存在时返回没有任何元素的Dictionary&lt;TKey, TValue&gt;空字典，条件表达式wherePredicate可以为null，为null时，查询所有记录
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TKey">字典Key类型</typeparam>
    /// <typeparam name="TValue">字典Value类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="wherePredicate">条件表达式，条件表达式wherePredicate可以为null，为null时，查询所有记录</param>
    /// <param name="keySelector">字典Key选择委托</param>
    /// <param name="valueSelector">字典Value选择委托</param>
    /// <returns>返回Dictionary&lt;TKey, TValue&gt;字典或没有任何元素的Dictionary&lt;TKey, TValue&gt;空字典</returns>
    public static Dictionary<TKey, TValue> QueryDictionary<TEntity, TKey, TValue>(this IRepository repository, Expression<Func<TEntity, bool>> wherePredicate, Func<TEntity, TKey> keySelector, Func<TEntity, TValue> valueSelector) where TKey : notnull
    {
        var query = repository.From<TEntity>();
        if (wherePredicate != null)
            query.Where(wherePredicate);
        return query.ToDictionary(keySelector, valueSelector);
    }
    /// <summary>
    /// 查询TEntity实体表满足表达式wherePredicate条件的所有记录，返回TEntity实体所有字段的记录并转化为Dictionary&lt;TKey, TValue&gt;字典，记录不存在时返回没有任何元素的Dictionary&lt;TKey, TValue&gt;空字典，条件表达式wherePredicate可以为null，为null时，查询所有记录
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TKey">字典Key类型</typeparam>
    /// <typeparam name="TValue">字典Value类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="wherePredicate">条件表达式，条件表达式wherePredicate可以为null，为null时，查询所有记录</param>
    /// <param name="keySelector">字典Key选择委托</param>
    /// <param name="valueSelector">字典Value选择委托</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回Dictionary&lt;TKey, TValue&gt;字典或没有任何元素的Dictionary&lt;TKey, TValue&gt;空字典</returns>
    public static async Task<Dictionary<TKey, TValue>> QueryDictionaryAsync<TEntity, TKey, TValue>(this IRepository repository, Expression<Func<TEntity, bool>> wherePredicate, Func<TEntity, TKey> keySelector, Func<TEntity, TValue> valueSelector, CancellationToken cancellationToken = default) where TKey : notnull
    {
        var query = repository.From<TEntity>();
        if (wherePredicate != null)
            query.Where(wherePredicate);
        return await query.ToDictionaryAsync(keySelector, valueSelector, cancellationToken);
    }
    #endregion    

    #region Update
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，用法：
    /// <code>
    /// var orderInfo = new OrderInfo { ... };
    /// var tmpObj = new { TotalAmount = 450, ... };
    /// repository.Update&lt;Order&gt;(f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... } //使用updateObjs对象中的参数，实体对象由TypeHandler处理
    /// }, x =&gt; x.Id == 1);
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: UPDATE `sys_order` SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes WHERE `Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段省略赋值字段，如：parameter.TotalAmount</param>
    /// <param name="wherePredicate">条件表达式，条件表达式wherePredicate不能为null</param>
    /// <returns>返回更新行数</returns>
    public static int Update<TEntity>(this IRepository repository, Expression<Func<TEntity, object>> fieldsAssignment, Expression<Func<TEntity, bool>> wherePredicate)
        => repository.Update<TEntity>().Set(fieldsAssignment).Where(wherePredicate).Execute();
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，用法：
    /// <code>
    /// var orderInfo = new OrderInfo { ... };
    /// var tmpObj = new { TotalAmount = 450, ... };
    /// await repository.UpdateAsync&lt;Order&gt;(f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... } //使用updateObjs对象中的参数，实体对象由TypeHandler处理
    /// }, x =&gt; x.Id == 1);
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: UPDATE `sys_order` SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes WHERE `Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段省略赋值字段，如：parameter.TotalAmount</param>
    /// <param name="wherePredicate">条件表达式，条件表达式wherePredicate不能为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回更新行数</returns>
    public static async Task<int> UpdateAsync<TEntity>(this IRepository repository, Expression<Func<TEntity, object>> fieldsAssignment, Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
        => await repository.Update<TEntity>().Set(fieldsAssignment).Where(wherePredicate).ExecuteAsync(cancellationToken);
    #endregion

    #region Delete
    /// <summary>
    /// 删除满足表达式wherePredicate条件的数据，不局限于主键条件，表达式wherePredicate不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="wherePredicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回删除行数</returns>
    public static int Delete<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> wherePredicate)
        => repository.Delete<TEntity>().Where(wherePredicate).Execute();
    /// <summary>
    /// 删除满足表达式wherePredicate条件的数据，不局限于主键条件，表达式wherePredicate不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="wherePredicate">条件表达式，表达式predicate不能为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回删除行数</returns>
    public static async Task<int> DeleteAsync<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
        => await repository.Delete<TEntity>().Where(wherePredicate).ExecuteAsync(cancellationToken);
    #endregion
}