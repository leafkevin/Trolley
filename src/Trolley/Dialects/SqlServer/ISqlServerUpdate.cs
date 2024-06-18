using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

public interface ISqlServerUpdate<TEntity> : IUpdate<TEntity>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TEntity表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回更新对象</returns>
    new ISqlServerUpdate<TEntity> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TEntity表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回更新对象</returns>
    new ISqlServerUpdate<TEntity> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 根据字段值确定TEntity表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致，可多次调用
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回更新对象</returns>
    new ISqlServerUpdate<TEntity> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TEntity表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回更新对象</returns>
    new ISqlServerUpdate<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TEntity表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回更新对象</returns>
    new ISqlServerUpdate<TEntity> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region Set
    /// <summary>
    /// 部分字段更新，updateObj对象中除主键、OnlyFields、IgnoreFields方法筛选后的剩下所有字段都将参与更新，匿名、命名对象都可以，需要配合where条件使用，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id = 1); .Set(new User { Name = "kevin", SourceType = null });
    /// SQL: SET `Name`=@Name,SourceType=@SourceType WHERE `Id`=@kId</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    new ISqlServerContinuedUpdate<TEntity> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中除主键、OnlyFields、IgnoreFields方法筛选后的剩下所有字段都将参与更新，匿名、命名对象都可以，需要配合where条件使用，为false不更新，用法：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id = 1); .Set(true, new User { Name = "kevin", SourceType = null });
    /// SQL: SET `Name`=@Name,SourceType=@SourceType WHERE `Id`=@kId</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    new ISqlServerContinuedUpdate<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个字段，支持New、MemberInit表达式访问，用法：
    /// <code>.Set(f => new { Description = DBNull.Value, Price = f.Price + 50 ... }) 或.Set(f => new Product { Description = DBNull.Value, Price = f.Price + 50 ... })
    /// SQL: UPDATE `sys_product` SET `Description`=NULL,`Price`=Price+50,`Field`=@Field ...</code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段省略赋值字段，如：parameter.TotalAmount</param>
    /// <returns>返回更新对象</returns>
    new ISqlServerContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，支持New、MemberInit表达式访问，如果为false，则不生成更新语句，用法：
    /// <code>.Set(true, f => new { Description = DBNull.Value, Price = f.Price + 50 ... }) 或.Set(true, f => new Product { Description = DBNull.Value, Price = f.Price + 50 ... })
    /// SQL: UPDATE `sys_product` SET `Description`=NULL,`Price`=Price+50,`Field`=@Field ...</code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式</param>
    /// <returns>返回更新对象</returns>
    new ISqlServerContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 单字段更新，使用固定值fieldValue进行单字段更新，用法：<code>.Set(x =&gt; x.OrderNo, "ON_111")</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    new ISqlServerContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用固定值fieldValue进行单字段更新，否则不生成更新语句，用法：
    /// <code>.Set(true, x =&gt; x.OrderNo, "ON_111")</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    new ISqlServerContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion

    #region SetFrom
    /// <summary>
    /// 使用表达式fieldSelector筛选单个栏位，子查询表达式valueSelector捞取更新值，部分栏位更新，表达式fieldSelector只能筛选一个栏位，用法：
    /// <code>
    /// .SetFrom(f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///     .From&lt;OrderDetail&gt;('c')
    ///     .Where(f =&gt; f.OrderId == y.Id)
    ///     .Select(t =&gt; Sql.Sum(t.Amount))) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldSelector">单个字段筛选表达式</param>
    /// <param name="valueSelector">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    new ISqlServerContinuedUpdate<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个栏位，子查询表达式valueSelector捞取更新值，部分栏位更新，如果为false，则不生成更新语句，表达式fieldSelector只能筛选一个栏位，用法：
    /// <code>
    /// .SetFrom(true, f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///     .From&lt;OrderDetail&gt;('c')
    ///     .Where(f =&gt; f.OrderId == y.Id)
    ///     .Select(t =&gt; Sql.Sum(t.Amount))) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">字段筛选表达式，只能筛选一个字段</param>
    /// <param name="valueSelector">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    new ISqlServerContinuedUpdate<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，用法：
    /// <code>
    /// var orderInfo = new { ... };
    /// .SetFrom((x, y) =&gt; new
    /// {
    ///     orderInfo.Disputes, //直接赋值，使用同名变量，实体对象由TypeHandler处理
    ///     TotalAmount = x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001",
    ///     BuyerSource = DBNull.Value,
    /// })
    /// SQL: SET a.`Disputes`=@Disputes,a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerSource`=NULL ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    new ISqlServerContinuedUpdate<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var orderInfo = new { ... };
    /// .SetFrom(true, (x, y) =&gt; new
    /// {
    ///     orderInfo.Disputes, //直接赋值，使用同名变量，实体对象由TypeHandler处理
    ///     TotalAmount = x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001",
    ///     BuyerSource = DBNull.Value,
    /// })
    /// SQL: SET a.`Disputes`=@Disputes,`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerSource`=NULL ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    new ISqlServerContinuedUpdate<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
    #endregion

    #region SetBulk
    /// <summary>
    /// 使用集合对象updateObjs部分字段批量更新，根据主键字段更新，updateObjs对象中除主键、OnlyFields、IgnoreFields方法筛选后的剩下所有字段都将参与更新。支持分批次更新，更新条数超过设置的bulkCount值，将在下次更新，直到所有数据更新完毕，bulkCount默认500，
    /// 可以继续使用Set、OnlyFields、IgnoreFields等方法，用法：
    /// <code>
    /// var updateObjs = new Order[]{ ... ...};
    /// .SetBulk(updateObjs).Set(new { Name = "kevin"}).IgnoreFields(...) ...
    /// SQL: UPDATE `sys_order` SET ...Name=@Name0 WHERE `Id`=@kId0;UPDATE `sys_order` SET ...Name=@Name1 WHERE `Id`=@kId1; ...
    /// </code>
    /// </summary>
    /// <typeparam name="TUpdateObj">要更新的字段类型</typeparam>
    /// <param name="updateObjs">更新对象参数集合，包含更新字段和where条件字段</param>
    /// <param name="bulkCount">单次更新的最大数据条数，默认是500</param>
    /// <returns>返回更新对象</returns>
    new ISqlServerContinuedUpdate<TEntity> SetBulk<TUpdateObj>(IEnumerable<TUpdateObj> updateObjs, int bulkCount = 500);
    #endregion

    #region InnerJoin
    /// <summary>
    /// InnerJoin内连接表TSource部分字段数据，更新当前表TEntity数据
    /// </summary>
    /// <typeparam name="TSource">数据来源表TSource实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateJoin<TEntity, TSource> InnerJoin<TSource>(Expression<Func<TEntity, TSource, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// LeftJoin左连接表TSource部分字段数据，更新当前表TEntity数据
    /// </summary>
    /// <typeparam name="TSource">数据来源表TSource实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateJoin<TEntity, TSource> LeftJoin<TSource>(Expression<Func<TEntity, TSource, bool>> joinOn);
    #endregion    

    #region SetBulkCopy
    /// <summary>
    /// 批量更新，采用SqlBulkCopy方式先插入数据到临时表，再Join临时表数据更新，不生成SQL，updateObjs可以不是TEntity类型，只要包含更新字段和主键栏位即可
    /// </summary>
    /// <param name="updateObjs">更新的对象集合，可以不是TEntity类型，包含更新字段和主键栏位即可</param>
    /// <param name="timeoutSeconds">超时时间，单位秒</param>
    /// <returns>返更新对象</returns>
    ISqlServerUpdated<TEntity> SetBulkCopy(IEnumerable updateObjs, int? timeoutSeconds = null);
    #endregion
}