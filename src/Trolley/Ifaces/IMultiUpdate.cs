﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

/// <summary>
/// 更新数据
/// </summary>
/// <typeparam name="TEntity">要更新的实体类型</typeparam>
public interface IMultiUpdate<TEntity>
{
    /// <summary>
    /// 使用更新对象parameters部分字段更新，单对象更新，更新对象parameters必须包含主键字段，用法：
    /// <code>
    /// repository.Update&lt;User&gt;()
    ///     .WithBy(new
    ///     {
    ///         Id = 1,
    ///         Name = "leafkevin"
    ///     })
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_user` SET `Name`=@Name WHERE `Id`=@kId
    /// </code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="parameters">部分字段更新对象参数，包含想要更新的必需栏位值和主键字段值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSet<TEntity> WithBy<TUpdateObj>(TUpdateObj parameters);
    /// <summary>
    /// 使用表达式fieldsExpr筛选更新字段，更新对象parameters部分字段更新，单对象更新，更新对象parameters必须包含主键字段，
    /// 表达式中，栏位固定赋值则更新为固定值，只是成员访问的字段将被更新为更新对象parameters中对应的字段值，用法：
    /// <code>
    /// repository.Update&lt;OrderDetail&gt;()
    ///     .WithBy(f =&gt; new
    ///     {
    ///         Price = 200,
    ///         f.Quantity,
    ///         UpdatedBy = 2,
    ///         f.Amount,
    ///         Remark = DBNull.Value
    ///     }, parameters)
    ///     .Execute();
    /// </code>
    /// 数据库三个栏位Price，UpdatedBy，Remark将被更新为固定值，栏位Remark将被更新为NULL，栏位Quantity，Amount将被更新为更新对象parameters中对应的字段值。
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order_detail` SET `Price`=@Price,`Quantity`=@Quantity,`UpdatedBy`=@UpdatedBy,`Amount`=@Amount,`Remark`=NULL WHERE `Id`=@kId
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">更新对象类型</typeparam>
    /// <param name="fieldsExpr">要更新的字段筛选表达式</param>
    /// <param name="parameters">部分字段更新对象参数，包含想要更新的必需栏位值和主键字段值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSet<TEntity> WithBy<TFields>(Expression<Func<TEntity, TFields>> fieldsExpr, object parameters);
    /// <summary>
    /// 使用集合对象parameters部分字段批量更新，集合对象parameters中的单个实体中必须包含主键字段，
    /// 支持分批次更新，更新条数超过设置的bulkCount值，将在下次更新，直到所有数据更新完毕，bulkCount默认500
    /// 用法：
    /// <code>
    /// var parameters = new []{ new { Id = 1, Name = "Name1" }, new { Id = 2, Name = "Name2" }};
    /// repository.Update&lt;User&gt;()
    ///     .WithBulkBy(parameters)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_user` SET `Name`=@Name0 WHERE `Id`=@kId0;UPDATE `sys_user` SET `Name`=@Name1 WHERE `Id`=@kId1;
    /// </code>
    /// </summary>
    /// <typeparam name="TUpdateObj"></typeparam>
    /// <param name="parameters">更新对象参数集合，包含想要更新的必需栏位值和主键字段值</param>
    /// <param name="bulkCount">单次更新的最大数据条数</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSet<TEntity> WithBulkBy<TUpdateObj>(IEnumerable<TUpdateObj> parameters, int bulkCount = 500);
    /// <summary>
    /// 使用表达式fieldsExpr筛选更新字段，集合对象parameters部分字段批量更新，集合对象parameters中的单个实体中必须包含主键字段，
    /// 支持分批次更新，更新条数超过设置的bulkCount值，将在下次更新，直到所有数据更新完毕，bulkCount默认500
    /// 用法：
    /// <code>
    /// var orders = await repository.From&lt;Order&gt;()
    ///     .Where(f =&gt; new int[] { 1, 2, 3 }.Contains(f.Id))
    ///     .ToListAsync();
    /// repository.Update&lt;Order&gt;()
    ///     .WithBulkBy(f =&gt; new
    ///     {
    ///         BuyerId = DBNull.Value,
    ///         OrderNo = "ON_" + f.OrderNo,
    ///         f.TotalAmount
    ///     }, orders)
    ///     .Execute();
    /// </code>
    /// 数据库栏位BuyerId将被更新为固定值NULL，栏位OrderNo将被更新为ON_+数据库中原值。栏位TotalAmount将被更新为参数orders中提供的值
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` SET `BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount0 WHERE `Id`=@kId0;UPDATE `sys_order` SET `BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount1 WHERE `Id`=@kId1;UPDATE `sys_order` SET `BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount2 WHERE `Id`=@kId2
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">要更新的字段</typeparam>
    /// <param name="fieldsExpr">要更新的字段筛选表达式</param>
    /// <param name="parameters">更新对象参数集合，包含想要更新的必需栏位值和主键字段值</param>
    /// <param name="bulkCount">单次更新的最大数据条数</param>
    /// <returns></returns>
    IMultiUpdateSet<TEntity> WithBulkBy<TFields>(Expression<Func<TEntity, TFields>> fieldsExpr, IEnumerable parameters, int bulkCount = 500);
    /// <summary>
    /// 使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;User&gt;()
    ///     .Set(f =&gt; new { SomeTimes = TimeSpan.FromMinutes(1455) })
    ///     .Where(x =&gt; x.Id == 1)
    ///     .Execute();
    /// var parameter = repository.Get&lt;Order&gt;(1);
    /// parameter.TotalAmount += 50;
    /// repository.Update&lt;Order&gt;()
    ///     .Set(f => new
    ///     {
    ///         parameter.TotalAmount,
    ///         Products = this.GetProducts(),
    ///         BuyerId = DBNull.Value,
    ///         Disputes = new Dispute
    ///         {
    ///             Id = 1,
    ///             Content = "43dss",
    ///             Users = "1,2",
    ///             Result = "OK",
    ///             CreatedAt = DateTime.Now
    ///         }
    ///     })
    ///     .Where(x =&gt; x.Id == 1)
    ///     .Execute();
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// </code>
    /// 生成的SQL:
    /// SQL1:UPDATE `sys_user` SET `SomeTimes`=@SomeTimes WHERE `Id`=1
    /// SQL2:UPDATE `sys_order` SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes WHERE `Id`=1
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，
    /// 如果为false，则不生成更新语句，用法：
    /// var condition = true;
    /// repository.Update&lt;User&gt;()
    ///     .SetIf(condition, f =&gt; new { Gender = Gender.Male })
    ///     .Where(x =&gt; x.Id == 1)
    ///     .Execute();
    /// var parameter = repository.Get&lt;Order&gt;(1);
    /// parameter.TotalAmount += 50;
    /// repository.Update&lt;Order&gt;()
    ///     .SetIf(condition, f =&gt; new
    ///     {
    ///         parameter.TotalAmount,
    ///         Products = this.GetProducts(),
    ///         BuyerId = DBNull.Value,
    ///         Disputes = new Dispute
    ///         {
    ///             Id = 1,
    ///             Content = "43dss",
    ///             Users = "1,2",
    ///             Result = "OK",
    ///             CreatedAt = DateTime.Now
    ///         }
    ///     })
    ///     .Where(x =&gt; x.Id == 1)
    ///     .Execute();
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> SetIf<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsExpr);
    /// <summary>
    /// 使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;OrderDetail&gt;((x, y) =&gt; x.Id == y.OrderId)
    ///     .Set((x, y, z) =&gt; new
    ///     {
    ///         TotalAmount = x.From&lt;OrderDetail&gt;('c')
    ///             .Where(f =&gt; f.OrderId == y.Id)
    ///             .Select(t =&gt; Sql.Sum(t.Amount))
    ///     })
    ///     .Set((a, b) =&gt; new { OrderNo = a.OrderNo + b.ProductId.ToString() })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((a, b) =&gt; a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,CAST(b.`ProductId` AS CHAR)),a.`BuyerId`=NULL WHERE a.`BuyerId`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> Set<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;OrderDetail&gt;((x, y) =&gt; x.Id == y.OrderId)
    ///     .SetIf(true, (x, y, z) =&gt; new
    ///     {
    ///         TotalAmount = x.From&lt;OrderDetail&gt;('c')
    ///             .Where(f =&gt; f.OrderId == y.Id)
    ///             .Select(t =&gt; Sql.Sum(t.Amount))
    ///     })
    ///     .Set((a, b) =&gt; new { OrderNo = a.OrderNo + b.ProductId.ToString() })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((a, b) =&gt; a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,CAST(b.`ProductId` AS CHAR)),a.`BuyerId`=NULL WHERE a.`BuyerId`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新单个栏位，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set(f =&gt; new { OrderNo = f.OrderNo + "-001" })
    ///     .Set(f =&gt; new { BuyerId = DBNull.Value })
    ///     .Where(x =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'-001'),a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新指定栏位，如果为false，则不生成更新语句，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .Set(true, f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set(f =&gt; new { OrderNo = f.OrderNo + "-001" })
    ///     .Set(f =&gt; new { BuyerId = DBNull.Value })
    ///     .Where(x =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'-001'),a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         TotalAmount = a.From&lt;OrderDetail&gt;('b')
    ///         .Where(f =&gt; f.OrderId == b.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount))
    ///     })
    ///     //单个字段+值方式
    ///     .SetValue(x =&gt; x.OrderNo, "ON_111")
    ///     //单个字段、多个字段 表达式方式
    ///     .Set(f =&gt; new { BuyerId = DBNull.Value })
    ///     .Where(a =&gt; a.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，否则不生成更新语句，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         TotalAmount = a.From&lt;OrderDetail&gt;('b')
    ///         .Where(f =&gt; f.OrderId == b.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount))
    ///     })
    ///     //单个字段+值方式
    ///     .SetValue(x =&gt; x.OrderNo, "ON_111")
    ///     //单个字段、多个字段 表达式方式
    ///     .Set(f =&gt; new { BuyerId = DBNull.Value })
    ///     .Where(a =&gt; a.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    /// <summary>
    /// 连接表TSource获取更新数据
    /// </summary>
    /// <typeparam name="TSource">数据来源表TSource实体类型</typeparam>
    /// <returns>返回数据更新来源对象</returns>
    IMultiUpdateFrom<TEntity, TSource> From<TSource>();
    /// <summary>
    /// 使用表T1, T2部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
    /// </summary>
    /// <typeparam name="T1">数据来源表T1实体类型</typeparam>
    /// <typeparam name="T2">数据来源表T2实体类型</typeparam>
    /// <returns>返回数据更新来源对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2> From<T1, T2>();
    /// <summary>
    /// 使用表T1, T2, T3部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
    /// </summary>
    /// <typeparam name="T1">数据来源表T1实体类型</typeparam>
    /// <typeparam name="T2">数据来源表T2实体类型</typeparam>
    /// <typeparam name="T3">数据来源表T3实体类型</typeparam>
    /// <returns>返回数据更新来源对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3> From<T1, T2, T3>();
    /// <summary>
    /// 使用表T1, T2, T3, T4部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
    /// </summary>
    /// <typeparam name="T1">数据来源表T1实体类型</typeparam>
    /// <typeparam name="T2">数据来源表T2实体类型</typeparam>
    /// <typeparam name="T3">数据来源表T3实体类型</typeparam>
    /// <typeparam name="T4">数据来源表T4实体类型</typeparam>
    /// <returns>返回数据更新来源对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>();
    /// <summary>
    /// 使用表T1, T2, T3, T4, T5部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
    /// </summary>
    /// <typeparam name="T1">数据来源表T1实体类型</typeparam>
    /// <typeparam name="T2">数据来源表T2实体类型</typeparam>
    /// <typeparam name="T3">数据来源表T3实体类型</typeparam>
    /// <typeparam name="T4">数据来源表T4实体类型</typeparam>
    /// <typeparam name="T5">数据来源表T5实体类型</typeparam>
    /// <returns>返回数据更新来源对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>();
    /// <summary>
    /// InnerJoin内连接表TSource部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
    /// </summary>
    /// <typeparam name="TSource">数据来源表TSource实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IMultiUpdateJoin<TEntity, TSource> InnerJoin<TSource>(Expression<Func<TEntity, TSource, bool>> joinOn);
    /// <summary>
    /// LeftJoin左连接表TSource部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
    /// </summary>
    /// <typeparam name="TSource">数据来源表TSource实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IMultiUpdateJoin<TEntity, TSource> LeftJoin<TSource>(Expression<Func<TEntity, TSource, bool>> joinOn);
}
/// <summary>
/// 更新数据
/// </summary>
/// <typeparam name="TEntity">要更新的实体类型</typeparam>
public interface IMultiUpdateSet<TEntity>
{
    /// <summary>
    /// 执行更新操作，并返回更新行数
    /// </summary>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Execute();
    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 更新数据
/// </summary>
/// <typeparam name="TEntity">要更新的实体类型</typeparam>
public interface IMultiUpdateSetting<TEntity> : IMultiUpdateSet<TEntity>
{
    #region Set/SetIf/SetValue/SetValueIf
    /// <summary>
    /// 使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;User&gt;()
    ///     .Set(f =&gt; new { SomeTimes = TimeSpan.FromMinutes(1455) })
    ///     .Where(x =&gt; x.Id == 1)
    ///     .Execute();
    /// var parameter = repository.Get&lt;Order&gt;(1);
    /// parameter.TotalAmount += 50;
    /// repository.Update&lt;Order&gt;()
    ///     .Set(f => new
    ///     {
    ///         parameter.TotalAmount,
    ///         Products = this.GetProducts(),
    ///         BuyerId = DBNull.Value,
    ///         Disputes = new Dispute
    ///         {
    ///             Id = 1,
    ///             Content = "43dss",
    ///             Users = "1,2",
    ///             Result = "OK",
    ///             CreatedAt = DateTime.Now
    ///         }
    ///     })
    ///     .Where(x =&gt; x.Id == 1)
    ///     .Execute();
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// </code>
    /// 生成的SQL:
    /// SQL1:UPDATE `sys_user` SET `SomeTimes`=@SomeTimes WHERE `Id`=1
    /// SQL2:UPDATE `sys_order` SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes WHERE `Id`=1
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，
    /// 如果为false，则不生成更新语句，用法：
    /// var condition = true;
    /// repository.Update&lt;User&gt;()
    ///     .SetIf(condition, f =&gt; new { Gender = Gender.Male })
    ///     .Where(x =&gt; x.Id == 1)
    ///     .Execute();
    /// var parameter = repository.Get&lt;Order&gt;(1);
    /// parameter.TotalAmount += 50;
    /// repository.Update&lt;Order&gt;()
    ///     .SetIf(condition, f =&gt; new
    ///     {
    ///         parameter.TotalAmount,
    ///         Products = this.GetProducts(),
    ///         BuyerId = DBNull.Value,
    ///         Disputes = new Dispute
    ///         {
    ///             Id = 1,
    ///             Content = "43dss",
    ///             Users = "1,2",
    ///             Result = "OK",
    ///             CreatedAt = DateTime.Now
    ///         }
    ///     })
    ///     .Where(x =&gt; x.Id == 1)
    ///     .Execute();
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> SetIf<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsExpr);
    /// <summary>
    /// 使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;OrderDetail&gt;((x, y) =&gt; x.Id == y.OrderId)
    ///     .Set((x, y, z) =&gt; new
    ///     {
    ///         TotalAmount = x.From&lt;OrderDetail&gt;('c')
    ///             .Where(f =&gt; f.OrderId == y.Id)
    ///             .Select(t =&gt; Sql.Sum(t.Amount))
    ///     })
    ///     .Set((a, b) =&gt; new { OrderNo = a.OrderNo + b.ProductId.ToString() })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((a, b) =&gt; a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,CAST(b.`ProductId` AS CHAR)),a.`BuyerId`=NULL WHERE a.`BuyerId`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> Set<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;OrderDetail&gt;((x, y) =&gt; x.Id == y.OrderId)
    ///     .SetIf(true, (x, y, z) =&gt; new
    ///     {
    ///         TotalAmount = x.From&lt;OrderDetail&gt;('c')
    ///             .Where(f =&gt; f.OrderId == y.Id)
    ///             .Select(t =&gt; Sql.Sum(t.Amount))
    ///     })
    ///     .Set((a, b) =&gt; new { OrderNo = a.OrderNo + b.ProductId.ToString() })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((a, b) =&gt; a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,CAST(b.`ProductId` AS CHAR)),a.`BuyerId`=NULL WHERE a.`BuyerId`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新单个栏位，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set(f =&gt; new { OrderNo = f.OrderNo + "-001" })
    ///     .Set(f =&gt; new { BuyerId = DBNull.Value })
    ///     .Where(x =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'-001'),a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新指定栏位，如果为false，则不生成更新语句，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .Set(true, f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set(f =&gt; new { OrderNo = f.OrderNo + "-001" })
    ///     .Set(f =&gt; new { BuyerId = DBNull.Value })
    ///     .Where(x =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'-001'),a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         TotalAmount = a.From&lt;OrderDetail&gt;('b')
    ///         .Where(f =&gt; f.OrderId == b.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount))
    ///     })
    ///     //单个字段+值方式
    ///     .SetValue(x =&gt; x.OrderNo, "ON_111")
    ///     //单个字段、多个字段 表达式方式
    ///     .Set(f =&gt; new { BuyerId = DBNull.Value })
    ///     .Where(a =&gt; a.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，否则不生成更新语句，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         TotalAmount = a.From&lt;OrderDetail&gt;('b')
    ///         .Where(f =&gt; f.OrderId == b.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount))
    ///     })
    ///     //单个字段+值方式
    ///     .SetValue(x =&gt; x.OrderNo, "ON_111")
    ///     //单个字段、多个字段 表达式方式
    ///     .Set(f =&gt; new { BuyerId = DBNull.Value })
    ///     .Where(a =&gt; a.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> And(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateSetting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate = null, Expression<Func<TEntity, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
public interface IMultiUpdateFrom<TEntity, T1> : IMultiUpdateSet<TEntity>
{
    #region Set/SetIf/SetValue/SetValueIf
    /// <summary>
    /// 使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((a, b) =&gt; new 
    ///     {
    ///         TotalAmount = y.Amount,
    ///         OrderNo = x.OrderNo + "_111"
    ///     })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=b.[Amount],[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，
    /// 如果为false，则不生成更新语句，用法：   
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set(condition, (a, b) =&gt; new 
    ///     {
    ///         TotalAmount = y.Amount,
    ///         OrderNo = x.OrderNo + "_111"
    ///     })
    ///     .Set(condition, (x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=b.[Amount],[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsExpr);
    /// <summary>
    /// 使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(true, (x, y) =&gt; new
    ///     {
    ///         TotalAmount = 200.56,
    ///         OrderNo = x.OrderNo + "-111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .SetValue(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(condition, (x, y, z) =&gt; new
    ///     {
    ///         TotalAmount = x.From&lt;OrderDetail&gt;('a')
    ///             .Where(f =&gt; f.OrderId == y.Id)
    ///             .Select(t =&gt; Sql.Sum(t.Amount)),
    ///         OrderNo = y.OrderNo + "-111",
    ///         BuyerSource = z.SourceType
    ///     })
    ///     .SetValue(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(a.[Amount]) FROM [sys_order_detail] a WHERE a.[OrderId]=[sys_order].[Id]),[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新单个栏位，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///         .From&lt;OrderDetail&gt;('b')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new { BuyerSource = y.SourceType })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(b.[Amount]) FROM [sys_order_detail] b WHERE b.[OrderId]=[sys_order].[Id]),[BuyerSource]=b.[SourceType] FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新指定栏位，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(condition, f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///         .From&lt;OrderDetail&gt;('b')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new { BuyerSource = y.SourceType })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(b.[Amount]) FROM [sys_order_detail] b WHERE b.[OrderId]=[sys_order].[Id]),[BuyerSource]=b.[SourceType] FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .SetValue(x =&gt; x.TotalAmount, 200.56)
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，否则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .SetValueIf(condition, x =&gt; x.TotalAmount, 200.56)
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate = null, Expression<Func<TEntity, T1, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1, T2部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表TT1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表TT2实体类型</typeparam>
public interface IMultiUpdateFrom<TEntity, T1, T2> : IMultiUpdateSet<TEntity>
{
    #region Set/SetIf/SetValue/SetValueIf
    /// <summary>
    /// 使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((a, b) =&gt; new 
    ///     {
    ///         TotalAmount = y.Amount,
    ///         OrderNo = x.OrderNo + "_111"
    ///     })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=b.[Amount],[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，
    /// 如果为false，则不生成更新语句，用法：   
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set(condition, (a, b) =&gt; new 
    ///     {
    ///         TotalAmount = y.Amount,
    ///         OrderNo = x.OrderNo + "_111"
    ///     })
    ///     .Set(condition, (x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=b.[Amount],[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(true, (x, y) =&gt; new
    ///     {
    ///         TotalAmount = 200.56,
    ///         OrderNo = x.OrderNo + "-111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .SetValue(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(condition, (x, y, z) =&gt; new
    ///     {
    ///         TotalAmount = x.From&lt;OrderDetail&gt;('a')
    ///             .Where(f =&gt; f.OrderId == y.Id)
    ///             .Select(t =&gt; Sql.Sum(t.Amount)),
    ///         OrderNo = y.OrderNo + "-111",
    ///         BuyerSource = z.SourceType
    ///     })
    ///     .SetValue(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(a.[Amount]) FROM [sys_order_detail] a WHERE a.[OrderId]=[sys_order].[Id]),[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新单个栏位，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///         .From&lt;OrderDetail&gt;('b')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new { BuyerSource = y.SourceType })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(b.[Amount]) FROM [sys_order_detail] b WHERE b.[OrderId]=[sys_order].[Id]),[BuyerSource]=b.[SourceType] FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新指定栏位，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(condition, f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///         .From&lt;OrderDetail&gt;('b')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new { BuyerSource = y.SourceType })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(b.[Amount]) FROM [sys_order_detail] b WHERE b.[OrderId]=[sys_order].[Id]),[BuyerSource]=b.[SourceType] FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .SetValue(x =&gt; x.TotalAmount, 200.56)
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，否则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .SetValueIf(condition, x =&gt; x.TotalAmount, 200.56)
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1, T2, T3部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表TT1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表TT2实体类型</typeparam>
/// <typeparam name="T3">更新值来源表TT3实体类型</typeparam>
public interface IMultiUpdateFrom<TEntity, T1, T2, T3> : IMultiUpdateSet<TEntity>
{
    #region Set/SetIf/SetValue/SetValueIf
    /// <summary>
    /// 使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((a, b) =&gt; new 
    ///     {
    ///         TotalAmount = y.Amount,
    ///         OrderNo = x.OrderNo + "_111"
    ///     })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=b.[Amount],[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，
    /// 如果为false，则不生成更新语句，用法：   
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set(condition, (a, b) =&gt; new 
    ///     {
    ///         TotalAmount = y.Amount,
    ///         OrderNo = x.OrderNo + "_111"
    ///     })
    ///     .Set(condition, (x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=b.[Amount],[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(true, (x, y) =&gt; new
    ///     {
    ///         TotalAmount = 200.56,
    ///         OrderNo = x.OrderNo + "-111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .SetValue(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(condition, (x, y, z) =&gt; new
    ///     {
    ///         TotalAmount = x.From&lt;OrderDetail&gt;('a')
    ///             .Where(f =&gt; f.OrderId == y.Id)
    ///             .Select(t =&gt; Sql.Sum(t.Amount)),
    ///         OrderNo = y.OrderNo + "-111",
    ///         BuyerSource = z.SourceType
    ///     })
    ///     .SetValue(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(a.[Amount]) FROM [sys_order_detail] a WHERE a.[OrderId]=[sys_order].[Id]),[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新单个栏位，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///         .From&lt;OrderDetail&gt;('b')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new { BuyerSource = y.SourceType })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(b.[Amount]) FROM [sys_order_detail] b WHERE b.[OrderId]=[sys_order].[Id]),[BuyerSource]=b.[SourceType] FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新指定栏位，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(condition, f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///         .From&lt;OrderDetail&gt;('b')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new { BuyerSource = y.SourceType })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(b.[Amount]) FROM [sys_order_detail] b WHERE b.[OrderId]=[sys_order].[Id]),[BuyerSource]=b.[SourceType] FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .SetValue(x =&gt; x.TotalAmount, 200.56)
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，否则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .SetValueIf(condition, x =&gt; x.TotalAmount, 200.56)
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1, T2, T3, T4部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表TT1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表TT2实体类型</typeparam>
/// <typeparam name="T3">更新值来源表TT3实体类型</typeparam>
/// <typeparam name="T4">更新值来源表TT4实体类型</typeparam>
public interface IMultiUpdateFrom<TEntity, T1, T2, T3, T4> : IMultiUpdateSet<TEntity>
{
    #region Set/SetIf/SetValue/SetValueIf
    /// <summary>
    /// 使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((a, b) =&gt; new 
    ///     {
    ///         TotalAmount = y.Amount,
    ///         OrderNo = x.OrderNo + "_111"
    ///     })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=b.[Amount],[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，
    /// 如果为false，则不生成更新语句，用法：   
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set(condition, (a, b) =&gt; new 
    ///     {
    ///         TotalAmount = y.Amount,
    ///         OrderNo = x.OrderNo + "_111"
    ///     })
    ///     .Set(condition, (x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=b.[Amount],[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(true, (x, y) =&gt; new
    ///     {
    ///         TotalAmount = 200.56,
    ///         OrderNo = x.OrderNo + "-111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .SetValue(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(condition, (x, y, z) =&gt; new
    ///     {
    ///         TotalAmount = x.From&lt;OrderDetail&gt;('a')
    ///             .Where(f =&gt; f.OrderId == y.Id)
    ///             .Select(t =&gt; Sql.Sum(t.Amount)),
    ///         OrderNo = y.OrderNo + "-111",
    ///         BuyerSource = z.SourceType
    ///     })
    ///     .SetValue(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(a.[Amount]) FROM [sys_order_detail] a WHERE a.[OrderId]=[sys_order].[Id]),[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新单个栏位，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///         .From&lt;OrderDetail&gt;('b')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new { BuyerSource = y.SourceType })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(b.[Amount]) FROM [sys_order_detail] b WHERE b.[OrderId]=[sys_order].[Id]),[BuyerSource]=b.[SourceType] FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新指定栏位，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(condition, f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///         .From&lt;OrderDetail&gt;('b')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new { BuyerSource = y.SourceType })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(b.[Amount]) FROM [sys_order_detail] b WHERE b.[OrderId]=[sys_order].[Id]),[BuyerSource]=b.[SourceType] FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .SetValue(x =&gt; x.TotalAmount, 200.56)
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，否则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .SetValueIf(condition, x =&gt; x.TotalAmount, 200.56)
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1, T2, T3, T4, T5部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表TT1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表TT2实体类型</typeparam>
/// <typeparam name="T3">更新值来源表TT3实体类型</typeparam>
/// <typeparam name="T4">更新值来源表TT4实体类型</typeparam>
/// <typeparam name="T5">更新值来源表TT5实体类型</typeparam>
public interface IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> : IMultiUpdateSet<TEntity>
{
    #region Set/SetIf/SetValue/SetValueIf
    /// <summary>
    /// 使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((a, b) =&gt; new 
    ///     {
    ///         TotalAmount = y.Amount,
    ///         OrderNo = x.OrderNo + "_111"
    ///     })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=b.[Amount],[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，
    /// 如果为false，则不生成更新语句，用法：   
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set(condition, (a, b) =&gt; new 
    ///     {
    ///         TotalAmount = y.Amount,
    ///         OrderNo = x.OrderNo + "_111"
    ///     })
    ///     .Set(condition, (x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=b.[Amount],[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(true, (x, y) =&gt; new
    ///     {
    ///         TotalAmount = 200.56,
    ///         OrderNo = x.OrderNo + "-111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .SetValue(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(condition, (x, y, z) =&gt; new
    ///     {
    ///         TotalAmount = x.From&lt;OrderDetail&gt;('a')
    ///             .Where(f =&gt; f.OrderId == y.Id)
    ///             .Select(t =&gt; Sql.Sum(t.Amount)),
    ///         OrderNo = y.OrderNo + "-111",
    ///         BuyerSource = z.SourceType
    ///     })
    ///     .SetValue(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(a.[Amount]) FROM [sys_order_detail] a WHERE a.[OrderId]=[sys_order].[Id]),[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新单个栏位，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///         .From&lt;OrderDetail&gt;('b')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new { BuyerSource = y.SourceType })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(b.[Amount]) FROM [sys_order_detail] b WHERE b.[OrderId]=[sys_order].[Id]),[BuyerSource]=b.[SourceType] FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新指定栏位，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetIf(condition, f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///         .From&lt;OrderDetail&gt;('b')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new { BuyerSource = y.SourceType })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(b.[Amount]) FROM [sys_order_detail] b WHERE b.[OrderId]=[sys_order].[Id]),[BuyerSource]=b.[SourceType] FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .SetValue(x =&gt; x.TotalAmount, 200.56)
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，否则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;OrderDetail&gt;()
    ///     .SetValueIf(condition, x =&gt; x.TotalAmount, 200.56)
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Set((x, y) =&gt; new { BuyerId = DBNull.Value })
    ///     .Where((x, y) =&gt; x.Id == y.OrderId && x.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">要更新数据表T1实体类型</typeparam>
public interface IMultiUpdateJoin<TEntity, T1> : IMultiUpdateSet<TEntity>
{
    #region Join
    /// <summary>
    /// 追加表T2字段数据InnerJoin内连接更新表TEntity数据
    /// </summary>
    /// <typeparam name="T2">数据来源表T2实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2> InnerJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn);
    /// <summary>
    /// 追加表T2字段数据LeftJoin左连接更新表TEntity数据
    /// </summary>
    /// <typeparam name="T2">数据来源表T实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2> LeftJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn);
    #endregion

    #region Set/SetIf/SetValue/SetValueIf
    /// <summary>
    /// 使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;OrderDetail&gt;((x, y) =&gt; x.Id == y.OrderId)
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，
    /// 如果为false，则不生成更新语句，用法：   
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;OrderDetail&gt;((x, y) =&gt; x.Id == y.OrderId)
    ///     .SetIf(condition, f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .SetIf(condition, (x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsExpr);
    /// <summary>
    /// 使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((a, b) =&gt; a.BuyerId == b.Id)
    ///     .Set((x, y) =&gt; new
    ///     {
    ///         TotalAmount = 200.56,
    ///         OrderNo = x.OrderNo + "-111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .SetValue(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((a, b) =&gt; a.BuyerId == b.Id)
    ///     .SetIf(condition, (x, y) =&gt; new
    ///     {
    ///         TotalAmount = 200.56,
    ///         OrderNo = x.OrderNo + "-111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .SetValueIf(condition, x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新单个栏位，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新指定栏位，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .SetIf(condition, f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .SetIf(condition, (x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .SetValue(x =&gt; x.TotalAmount, 200.56)
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，否则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .SetValueIf(condition, x =&gt; x.TotalAmount, 200.56)
    ///     .SetIf(condition, (a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate = null, Expression<Func<TEntity, T1, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1, T2部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表TT1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表TT2实体类型</typeparam>
public interface IMultiUpdateJoin<TEntity, T1, T2> : IMultiUpdateSet<TEntity>
{
    #region Join
    /// <summary>
    /// InnerJoin内连接表T3部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
    /// </summary>
    /// <typeparam name="T3">数据来源表T3实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3> InnerJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn);
    /// <summary>
    /// LeftJoin左连接表T3部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
    /// </summary>
    /// <typeparam name="T3">数据来源表T3实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3> LeftJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn);
    #endregion

    #region Set/SetIf/SetValue/SetValueIf
    /// <summary>
    /// 使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;OrderDetail&gt;((x, y) =&gt; x.Id == y.OrderId)
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，
    /// 如果为false，则不生成更新语句，用法：   
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;OrderDetail&gt;((x, y) =&gt; x.Id == y.OrderId)
    ///     .SetIf(condition, f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .SetIf(condition, (x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((a, b) =&gt; a.BuyerId == b.Id)
    ///     .Set((x, y) =&gt; new
    ///     {
    ///         TotalAmount = 200.56,
    ///         OrderNo = x.OrderNo + "-111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .SetValue(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((a, b) =&gt; a.BuyerId == b.Id)
    ///     .SetIf(condition, (x, y) =&gt; new
    ///     {
    ///         TotalAmount = 200.56,
    ///         OrderNo = x.OrderNo + "-111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .SetValueIf(condition, x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新单个栏位，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新指定栏位，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .SetIf(condition, f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .SetIf(condition, (x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .SetValue(x =&gt; x.TotalAmount, 200.56)
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，否则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .SetValueIf(condition, x =&gt; x.TotalAmount, 200.56)
    ///     .SetIf(condition, (a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1, T2, T3部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表TT1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表TT2实体类型</typeparam>
/// <typeparam name="T3">更新值来源表TT3实体类型</typeparam>
public interface IMultiUpdateJoin<TEntity, T1, T2, T3> : IMultiUpdateSet<TEntity>
{
    #region Join
    /// <summary>
    /// InnerJoin内连接表T4部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
    /// </summary>
    /// <typeparam name="T4">数据来源表T4实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4> InnerJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn);
    /// <summary>
    /// LeftJoin左连接表T4部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
    /// </summary>
    /// <typeparam name="T4">数据来源表T4实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4> LeftJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn);
    #endregion

    #region Set/SetIf/SetValue/SetValueIf
    /// <summary>
    /// 使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;OrderDetail&gt;((x, y) =&gt; x.Id == y.OrderId)
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，
    /// 如果为false，则不生成更新语句，用法：   
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;OrderDetail&gt;((x, y) =&gt; x.Id == y.OrderId)
    ///     .SetIf(condition, f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .SetIf(condition, (x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((a, b) =&gt; a.BuyerId == b.Id)
    ///     .Set((x, y) =&gt; new
    ///     {
    ///         TotalAmount = 200.56,
    ///         OrderNo = x.OrderNo + "-111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .SetValue(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((a, b) =&gt; a.BuyerId == b.Id)
    ///     .SetIf(condition, (x, y) =&gt; new
    ///     {
    ///         TotalAmount = 200.56,
    ///         OrderNo = x.OrderNo + "-111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .SetValueIf(condition, x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新单个栏位，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新指定栏位，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .SetIf(condition, f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .SetIf(condition, (x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .SetValue(x =&gt; x.TotalAmount, 200.56)
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，否则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .SetValueIf(condition, x =&gt; x.TotalAmount, 200.56)
    ///     .SetIf(condition, (a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1, T2, T3, T4部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表TT1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表TT2实体类型</typeparam>
/// <typeparam name="T3">更新值来源表TT3实体类型</typeparam>
/// <typeparam name="T4">更新值来源表TT4实体类型</typeparam>
public interface IMultiUpdateJoin<TEntity, T1, T2, T3, T4> : IMultiUpdateSet<TEntity>
{
    #region Join
    /// <summary>
    /// InnerJoin内连接表T5部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
    /// </summary>
    /// <typeparam name="T5">数据来源表T5实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn);
    /// <summary>
    /// LeftJoin左连接表T5部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
    /// </summary>
    /// <typeparam name="T5">数据来源表T5实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn);
    #endregion

    #region Set/SetIf/SetValue/SetValueIf
    /// <summary>
    /// 使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;OrderDetail&gt;((x, y) =&gt; x.Id == y.OrderId)
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，
    /// 如果为false，则不生成更新语句，用法：   
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;OrderDetail&gt;((x, y) =&gt; x.Id == y.OrderId)
    ///     .SetIf(condition, f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .SetIf(condition, (x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((a, b) =&gt; a.BuyerId == b.Id)
    ///     .Set((x, y) =&gt; new
    ///     {
    ///         TotalAmount = 200.56,
    ///         OrderNo = x.OrderNo + "-111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .SetValue(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((a, b) =&gt; a.BuyerId == b.Id)
    ///     .SetIf(condition, (x, y) =&gt; new
    ///     {
    ///         TotalAmount = 200.56,
    ///         OrderNo = x.OrderNo + "-111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .SetValueIf(condition, x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新单个栏位，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新指定栏位，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .SetIf(condition, f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .SetIf(condition, (x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .SetValue(x =&gt; x.TotalAmount, 200.56)
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，否则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .SetValueIf(condition, x =&gt; x.TotalAmount, 200.56)
    ///     .SetIf(condition, (a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1, T2, T3, T4, T5部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表TT1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表TT2实体类型</typeparam>
/// <typeparam name="T3">更新值来源表TT3实体类型</typeparam>
/// <typeparam name="T4">更新值来源表TT4实体类型</typeparam>
/// <typeparam name="T5">更新值来源表TT5实体类型</typeparam>
public interface IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> : IMultiUpdateSet<TEntity>
{
    #region Set/SetIf/SetValue/SetValueIf
    /// <summary>
    /// 使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;OrderDetail&gt;((x, y) =&gt; x.Id == y.OrderId)
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，
    /// 如果为false，则不生成更新语句，用法：   
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;OrderDetail&gt;((x, y) =&gt; x.Id == y.OrderId)
    ///     .SetIf(condition, f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .SetIf(condition, (x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerId = DBNull.Value
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsExpr">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((a, b) =&gt; a.BuyerId == b.Id)
    ///     .Set((x, y) =&gt; new
    ///     {
    ///         TotalAmount = 200.56,
    ///         OrderNo = x.OrderNo + "-111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .SetValue(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((a, b) =&gt; a.BuyerId == b.Id)
    ///     .SetIf(condition, (x, y) =&gt; new
    ///     {
    ///         TotalAmount = 200.56,
    ///         OrderNo = x.OrderNo + "-111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .SetValueIf(condition, x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsExpr">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新单个栏位，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .Set(f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .Set((x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsExpr筛选单个栏位，子查询表达式fieldValueExpr作为更新值，更新指定栏位，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .SetIf(condition, f =&gt; f.TotalAmount, (x, y) =&gt; x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)))
    ///     .SetIf(condition, (x, y) =&gt; new
    ///     {
    ///         OrderNo = x.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">单个字段筛选表达式</param>
    /// <param name="fieldValueExpr">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    /// <summary>
    /// 使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .SetValue(x =&gt; x.TotalAmount, 200.56)
    ///     .Set((a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldExpr筛选单个字段，使用固定值fieldValue进行更新，否则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .SetValueIf(condition, x =&gt; x.TotalAmount, 200.56)
    ///     .SetIf(condition, (a, b) =&gt; new
    ///     {
    ///         OrderNo = a.OrderNo + "_111",
    ///         BuyerSource = y.SourceType
    ///     })
    ///     .Where((x, y) =&gt; x.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerSource`=b.`SourceType` WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldExpr">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    #endregion
}