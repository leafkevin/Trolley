using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

/// <summary>
/// 更新数据
/// </summary>
/// <typeparam name="TEntity">要更新的实体类型</typeparam>
public interface IUpdate<TEntity>
{
    #region Set/SetWith/SetFrom
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个字段，用法：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    /// repository.Update&lt;Order&gt;()
    ///     .Set(f => new
    ///     {
    ///         parameter.TotalAmount, //直接赋值，使用同名变量
    ///         Products = this.GetProducts(), //直接赋值，使用本地函数
    ///         BuyerId = DBNull.Value, //直接赋值 NULL
    ///         Disputes = new Dispute { ... } //直接赋值，实体对象由TypeHandler处理
    ///     }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: UPDATE `sys_order` SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段省略赋值字段，如：parameter.TotalAmount</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    /// repository.Update&lt;Order&gt;()
    ///     .Set(true, f => new
    ///     {
    ///         parameter.TotalAmount, //直接赋值，使用同名变量
    ///         Products = this.GetProducts(), //直接赋值，使用本地函数
    ///         BuyerId = DBNull.Value, //直接赋值 NULL
    ///         Disputes = new Dispute { ... } //直接赋值，实体对象由TypeHandler处理
    ///     }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: UPDATE `sys_order` SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段省略赋值字段，如：parameter.TotalAmount</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，用法：
    /// <code>.Set(x =&gt; x.OrderNo, "ON_111")</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，否则不生成更新语句，用法：
    /// <code>.Set(true, x =&gt; x.OrderNo, "ON_111")</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相同的栏位都将参与更新，单对象更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相当的栏位都将参与更新，单对象更新，为false不做更新，用法：
    /// <code>.Set(true, new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var orderInfo = new { ... };
    /// var tmpObj = new { TotalAmount = 450, ... };
    /// repository.Update&lt;Order&gt;(f => new
    /// {
    ///     tmpObj.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     f.ProductCount, //使用updateObjs对象中的参数
    ///     f.Disputes //使用updateObjs对象中的参数，实体对象由TypeHandler处理
    /// }, orderInfo);
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: UPDATE `sys_order` SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`ProductCount`=@ProductCount,`Disputes`=@Disputes WHERE `Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> SetWith<TFields>(Expression<Func<TEntity, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var orderInfo = new { ... };
    /// var tmpObj = new { TotalAmount = 450, ... };
    /// repository.Update&lt;Order&gt;(true, f => new
    /// {
    ///     tmpObj.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     f.ProductCount, //使用updateObjs对象中的参数
    ///     f.Disputes //使用updateObjs对象中的参数，实体对象由TypeHandler处理
    /// }, orderInfo);
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: UPDATE `sys_order` SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`ProductCount`=@ProductCount,`Disputes`=@Disputes WHERE `Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> SetWith<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsSelectorOrAssignment, object updateObj);
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
    IUpdateSetting<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
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
    IUpdateSetting<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
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
    IUpdateSetting<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
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
    IUpdateSetting<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    #endregion

    #region WithBulk
    /// <summary>
    /// 使用表达式fieldsSelectorOrAssignment字段筛选和集合对象updateObjs部分字段批量更新，集合对象updateObjs中的单个元素实体中必须包含主键字段，支持分批次更新，更新条数超过设置的bulkCount值，将在下次更新，直到所有数据更新完毕，bulkCount默认500，用法：
    /// <code>
    /// var tmpObj = new { Products = new int[]{ 1, 2 ,3 }, ... };
    /// var updateObjs = new Order[]{ ... ...};
    /// .WithBulk(f =&gt; new
    /// {
    ///     tmpObj.Products, //直接参数赋值，实体对象由TypeHandler处理
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     OrderNo = "ON_" + f.OrderNo, //直接表达式赋值
    ///     f.TotalAmount //使用updateObjs对象中的参数
    /// }, updateObjs) ...
    /// SQL: UPDATE `sys_order` SET `Products`=@Products,`TotalAmount`=@TotalAmount`BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount0 WHERE `Id`=@kId0;UPDATE `sys_order` SET `Products`=@Products,`BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount1 WHERE `Id`=@kId1; ...
    /// </code>
    /// 执行后的结果，栏位BuyerId将被更新为固定值NULL，栏位OrderNo将被更新为ON_+数据库中原值，栏位TotalAmount将被更新为参数orders中提供的值
    /// </summary>
    /// <typeparam name="TFields">要更新的字段</typeparam>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObjs">更新对象参数集合，包含想要更新的必需栏位和主键字段</param>
    /// <param name="bulkCount">单次更新的最大数据条数，默认是500</param>
    /// <returns>返回更新对象</returns>
    IUpdateSet WithBulk<TFields>(Expression<Func<TEntity, TFields>> fieldsSelectorOrAssignment, IEnumerable updateObjs, int bulkCount = 500);
    #endregion

    #region From
    /// <summary>
    /// 连接表TSource获取更新数据
    /// </summary>
    /// <typeparam name="TSource">数据来源表TSource实体类型</typeparam>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateFrom<TEntity, TSource> From<TSource>();
    /// <summary>
    /// 使用表T1, T2部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
    /// </summary>
    /// <typeparam name="T1">数据来源表T1实体类型</typeparam>
    /// <typeparam name="T2">数据来源表T2实体类型</typeparam>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateFrom<TEntity, T1, T2> From<T1, T2>();
    /// <summary>
    /// 使用表T1, T2, T3部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
    /// </summary>
    /// <typeparam name="T1">数据来源表T1实体类型</typeparam>
    /// <typeparam name="T2">数据来源表T2实体类型</typeparam>
    /// <typeparam name="T3">数据来源表T3实体类型</typeparam>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> From<T1, T2, T3>();
    /// <summary>
    /// 使用表T1, T2, T3, T4部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
    /// </summary>
    /// <typeparam name="T1">数据来源表T1实体类型</typeparam>
    /// <typeparam name="T2">数据来源表T2实体类型</typeparam>
    /// <typeparam name="T3">数据来源表T3实体类型</typeparam>
    /// <typeparam name="T4">数据来源表T4实体类型</typeparam>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>();
    /// <summary>
    /// 使用表T1, T2, T3, T4, T5部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
    /// </summary>
    /// <typeparam name="T1">数据来源表T1实体类型</typeparam>
    /// <typeparam name="T2">数据来源表T2实体类型</typeparam>
    /// <typeparam name="T3">数据来源表T3实体类型</typeparam>
    /// <typeparam name="T4">数据来源表T4实体类型</typeparam>
    /// <typeparam name="T5">数据来源表T5实体类型</typeparam>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>();
    #endregion

    #region Join
    /// <summary>
    /// InnerJoin内连接表TSource部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
    /// </summary>
    /// <typeparam name="TSource">数据来源表TSource实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateJoin<TEntity, TSource> InnerJoin<TSource>(Expression<Func<TEntity, TSource, bool>> joinOn);
    /// <summary>
    /// LeftJoin左连接表TSource部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
    /// </summary>
    /// <typeparam name="TSource">数据来源表TSource实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateJoin<TEntity, TSource> LeftJoin<TSource>(Expression<Func<TEntity, TSource, bool>> joinOn);
    #endregion
}
/// <summary>
/// 更新数据
/// </summary>
public interface IUpdateSet
{
    #region Execute
    /// <summary>
    /// 执行更新操作，并返回更新行数
    /// </summary>
    /// <returns>返回更新行数</returns>
    int Execute();
    /// <summary>
    /// 执行更新操作，并返回更新行数
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回更新行数</returns>
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    #endregion

    #region ToSql
    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
    #endregion
}
/// <summary>
/// 更新数据
/// </summary>
/// <typeparam name="TEntity">要更新的实体类型</typeparam>
public interface IUpdateSetting<TEntity> : IUpdateSet
{
    #region Set/SetWith/SetFrom
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，用法：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    /// .Set(f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... } //直接赋值，实体对象由TypeHandler处理
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    /// .Set(true, f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... } //直接赋值，实体对象由TypeHandler处理
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，否则不生成更新语句，用法：
    /// <code>.Set(x =&gt; x.OrderNo, "ON_111")</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，否则不生成更新语句，用法：
    /// <code>.Set(true, x =&gt; x.OrderNo, "ON_111")</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相同的栏位都将参与更新，单对象更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相当的栏位都将参与更新，单对象更新，为false不做更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObjs部分字段根据主键进行更新，可单条也可多条数据更新，多条可分批次完成，每次更新bulkCount条数。
    /// updateObjs对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，updateObjs对象中必须包含主键栏位。
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，用法：
    /// <code>
    /// var orderInfo = new OrderInfo { ... };
    /// var tmpObj = new { TotalAmount = 450, ... };
    /// .SetWith(f => new
    /// {
    ///     tmpObj.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     f.ProductCount, //使用updateObjs对象中的参数
    ///     f.Disputes //使用updateObjs对象中的参数，实体对象由TypeHandler处理
    /// }, orderInfo);
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`ProductCount`=@ProductCount,`Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> SetWith<TFields>(Expression<Func<TEntity, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObjs部分字段根据主键进行更新，可单条也可多条数据更新，多条可分批次完成，每次更新bulkCount条数。
    /// updateObjs对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，updateObjs对象中必须包含主键栏位。
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，用法：
    /// <code>
    /// var orderInfo = new OrderInfo { ... };
    /// var tmpObj = new { TotalAmount = 450, ... };
    /// .SetWith(true, f => new
    /// {
    ///     tmpObj.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     f.ProductCount, //使用updateObjs对象中的参数
    ///     f.Disputes //使用updateObjs对象中的参数，实体对象由TypeHandler处理
    /// }, orderInfo);
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: UPDATE `sys_order` SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`ProductCount`=@ProductCount,`Disputes`=@Disputes WHERE `Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> SetWith<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom((x, y) =&gt; new
    /// {
    ///     TotalAmount = x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts() //直接赋值，使用本地函数
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom(true, (x, y) =&gt; new
    /// {
    ///     TotalAmount = x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts() //直接赋值，使用本地函数
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
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
    IUpdateSetting<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
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
    IUpdateSetting<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用whereObj生成Where条件，whereObj对象内所有与当前实体表TEntity名称相同的栏位都将参与where条件过滤，whereObj对象可以是匿名对象或是已有命名对象或是字典，推荐使用匿名对象，不能为null
    /// </summary>
    /// <typeparam name="TFields">where条件对象类型</typeparam>
    /// <param name="whereObj">where条件对象，whereObj对象内所有与当前实体表TEntity名称相同的栏位都将参与where条件过滤，可以是匿名对象或是已有命名对象或是字典，推荐使用匿名对象，不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateSet Where<TFields>(TFields whereObj);
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> And(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate = null, Expression<Func<TEntity, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
/// Update ..From语句，在Where语句中一定要包含Update表与From表之间的关联条件
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
public interface IUpdateFrom<TEntity, T1> : IUpdateSet
{
    #region Set/SetWith/SetFrom
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，WHERE语句中要包含Update表与From表的关联條件，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .Set((a, b) =&gt; new { BuyerSource = b.SourceType, parameter.TotalAmount, BuyerSource = DBNull.Value, ... })
    ///     .Where((a, b) =&gt; a.Id == b.OrderId && a.BuyerId == 1 ...)
    /// SQL: UPDATE [sys_order] SET [BuyerSource]=b.[SourceType],TotalAmount=@TotalAmount,BuyerSource=@BuyerSource ... FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] ... AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，WHERE语句中要包含Update表与From表的关联條件，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .Set(true, (a, b) =&gt; new { BuyerSource = b.SourceType, parameter.TotalAmount, BuyerSource = DBNull.Value, ... })
    ///     .Where((a, b) =&gt; a.Id == b.OrderId && a.BuyerId == 1 ...)
    /// SQL: UPDATE [sys_order] SET [BuyerSource]=b.[SourceType],TotalAmount=@TotalAmount,BuyerSource=@BuyerSource ... FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] ... AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> Set<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，用法：
    /// <code>.Set(x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如果为false，则不生成更新语句，用法：
    /// <code>.Set(condition, x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">字段筛选表达式，筛选单个栏位</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相同的栏位都将参与更新，单对象更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相当的栏位都将参与更新，单对象更新，为false不做更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var orderInfo = new OrderInfo { Id = 1, ... };
    /// var tmpObj = new { TotalAmount = 450, ... };
    /// ...
    /// .SetWith(true, (a, b) => new
    /// {
    ///     tmpObj.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     Remark = DBNull.Value, //直接赋值 NULL
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.BuyerId, //使用updateObjs对象中的参数
    ///     a.Disputes //使用updateObjs对象中的参数，实体对象由TypeHandler处理
    /// }, orderInfo) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET [TotalAmount`=@TotalAmount,[Products`=@Products,[Remark`=@Remark,[BuyerId`=NULL,BuyerSource]=b.[SourceType],[BuyerId`=@BuyerId,[Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> SetWith<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var orderInfo = new OrderInfo { Id = 1, ... };
    /// var tmpObj = new { TotalAmount = 450, ... };
    /// ...
    /// .SetWith(true, (a, b) => new
    /// {
    ///     tmpObj.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     Remark = DBNull.Value, //直接赋值 NULL
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.BuyerId, //使用updateObjs对象中的参数
    ///     a.Disputes //使用updateObjs对象中的参数，实体对象由TypeHandler处理
    /// }, orderInfo) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET [TotalAmount`=@TotalAmount,[Products`=@Products,[Remark`=@Remark,[BuyerId`=NULL,BuyerSource]=b.[SourceType],[BuyerId`=@BuyerId,[Disputes`=@Disputes ...
    /// </code>
    /// 执行后的结果，TotalAmount，Products，BuyerId被更新为对应的值，ProductCount，Disputes被更新为orderInfo中对应的值，UpdatedAt栏位没有更新
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom((a, b) =&gt; new
    /// {
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom(true, (a, b) =&gt; new
    /// {
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsAssignment);
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
    IUpdateFrom<TEntity, T1> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
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
    IUpdateFrom<TEntity, T1> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate = null, Expression<Func<TEntity, T1, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1T2部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
/// Update ..From语句，在Where语句中一定要包含Update表与From表之间的关联条件
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表T2实体类型</typeparam>
public interface IUpdateFrom<TEntity, T1, T2> : IUpdateSet
{
    #region Set/SetWith/SetFrom
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，WHERE语句中要包含Update表与From表的关联條件，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .Set((a, b, c) =&gt; new { BuyerSource = b.SourceType, parameter.TotalAmount, BuyerSource = DBNull.Value, ... })
    ///     .Where((a, b, c) =&gt; a.Id == b.OrderId && a.BuyerId == 1 ...)
    /// SQL: UPDATE [sys_order] SET [BuyerSource]=b.[SourceType],TotalAmount=@TotalAmount,BuyerSource=@BuyerSource ... FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] ... AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，WHERE语句中要包含Update表与From表的关联條件，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .Set(true, (a, b, c) =&gt; new { BuyerSource = b.SourceType, parameter.TotalAmount, BuyerSource = DBNull.Value, ... })
    ///     .Where((a, b, c) =&gt; a.Id == b.OrderId && a.BuyerId == 1 ...)
    /// SQL: UPDATE [sys_order] SET [BuyerSource]=b.[SourceType],TotalAmount=@TotalAmount,BuyerSource=@BuyerSource ... FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] ... AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，用法：
    /// <code>.Set(x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如果为false，则不生成更新语句，用法：
    /// <code>.Set(condition, x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">字段筛选表达式，筛选单个栏位</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相同的栏位都将参与更新，单对象更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相当的栏位都将参与更新，单对象更新，为false不做更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var orderInfo = new OrderInfo { Id = 1, ... };
    /// var tmpObj = new { TotalAmount = 450, ... };
    /// ...
    /// .SetWith(true, (a, b, c) => new
    /// {
    ///     tmpObj.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     Remark = DBNull.Value, //直接赋值 NULL
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.BuyerId, //使用updateObjs对象中的参数
    ///     a.Disputes //使用updateObjs对象中的参数，实体对象由TypeHandler处理
    /// }, orderInfo) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET [TotalAmount`=@TotalAmount,[Products`=@Products,[Remark`=@Remark,[BuyerId`=NULL,BuyerSource]=b.[SourceType],[BuyerId`=@BuyerId,[Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> SetWith<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var orderInfo = new OrderInfo { Id = 1, ... };
    /// var tmpObj = new { TotalAmount = 450, ... };
    /// ...
    /// .SetWith(true, (a, b, c) => new
    /// {
    ///     tmpObj.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     Remark = DBNull.Value, //直接赋值 NULL
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.BuyerId, //使用updateObjs对象中的参数
    ///     a.Disputes //使用updateObjs对象中的参数，实体对象由TypeHandler处理
    /// }, orderInfo) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET [TotalAmount`=@TotalAmount,[Products`=@Products,[Remark`=@Remark,[BuyerId`=NULL,BuyerSource]=b.[SourceType],[BuyerId`=@BuyerId,[Disputes`=@Disputes ...
    /// </code>
    /// 执行后的结果，TotalAmount，Products，BuyerId被更新为对应的值，ProductCount，Disputes被更新为orderInfo中对应的值，UpdatedAt栏位没有更新
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom((a, b, c) =&gt; new
    /// {
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom(true, (a, b, c) =&gt; new
    /// {
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsAssignment);
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
    IUpdateFrom<TEntity, T1, T2> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
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
    IUpdateFrom<TEntity, T1, T2> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1T2T3部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
/// Update ..From语句，在Where语句中一定要包含Update表与From表之间的关联条件
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表T2实体类型</typeparam>
/// <typeparam name="T3">更新值来源表T3实体类型</typeparam>
public interface IUpdateFrom<TEntity, T1, T2, T3> : IUpdateSet
{
    #region Set/SetWith/SetFrom
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，WHERE语句中要包含Update表与From表的关联條件，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .Set((a, b, c, d) =&gt; new { BuyerSource = b.SourceType, parameter.TotalAmount, BuyerSource = DBNull.Value, ... })
    ///     .Where((a, b, c, d) =&gt; a.Id == b.OrderId && a.BuyerId == 1 ...)
    /// SQL: UPDATE [sys_order] SET [BuyerSource]=b.[SourceType],TotalAmount=@TotalAmount,BuyerSource=@BuyerSource ... FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] ... AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，WHERE语句中要包含Update表与From表的关联條件，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .Set(true, (a, b, c, d) =&gt; new { BuyerSource = b.SourceType, parameter.TotalAmount, BuyerSource = DBNull.Value, ... })
    ///     .Where((a, b, c, d) =&gt; a.Id == b.OrderId && a.BuyerId == 1 ...)
    /// SQL: UPDATE [sys_order] SET [BuyerSource]=b.[SourceType],TotalAmount=@TotalAmount,BuyerSource=@BuyerSource ... FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] ... AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，用法：
    /// <code>.Set(x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如果为false，则不生成更新语句，用法：
    /// <code>.Set(condition, x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">字段筛选表达式，筛选单个栏位</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相同的栏位都将参与更新，单对象更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相当的栏位都将参与更新，单对象更新，为false不做更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var orderInfo = new OrderInfo { Id = 1, ... };
    /// var tmpObj = new { TotalAmount = 450, ... };
    /// ...
    /// .SetWith(true, (a, b, c, d) => new
    /// {
    ///     tmpObj.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     Remark = DBNull.Value, //直接赋值 NULL
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.BuyerId, //使用updateObjs对象中的参数
    ///     a.Disputes //使用updateObjs对象中的参数，实体对象由TypeHandler处理
    /// }, orderInfo) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET [TotalAmount`=@TotalAmount,[Products`=@Products,[Remark`=@Remark,[BuyerId`=NULL,BuyerSource]=b.[SourceType],[BuyerId`=@BuyerId,[Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var orderInfo = new OrderInfo { Id = 1, ... };
    /// var tmpObj = new { TotalAmount = 450, ... };
    /// ...
    /// .SetWith(true, (a, b, c, d) => new
    /// {
    ///     tmpObj.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     Remark = DBNull.Value, //直接赋值 NULL
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.BuyerId, //使用updateObjs对象中的参数
    ///     a.Disputes //使用updateObjs对象中的参数，实体对象由TypeHandler处理
    /// }, orderInfo) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET [TotalAmount`=@TotalAmount,[Products`=@Products,[Remark`=@Remark,[BuyerId`=NULL,BuyerSource]=b.[SourceType],[BuyerId`=@BuyerId,[Disputes`=@Disputes ...
    /// </code>
    /// 执行后的结果，TotalAmount，Products，BuyerId被更新为对应的值，ProductCount，Disputes被更新为orderInfo中对应的值，UpdatedAt栏位没有更新
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom((a, b, c, d) =&gt; new
    /// {
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom(true, (a, b, c, d) =&gt; new
    /// {
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsAssignment);
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
    IUpdateFrom<TEntity, T1, T2, T3> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
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
    IUpdateFrom<TEntity, T1, T2, T3> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1T2T3T4部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
/// Update ..From语句，在Where语句中一定要包含Update表与From表之间的关联条件
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表T2实体类型</typeparam>
/// <typeparam name="T3">更新值来源表T3实体类型</typeparam>
/// <typeparam name="T4">更新值来源表T4实体类型</typeparam>
public interface IUpdateFrom<TEntity, T1, T2, T3, T4> : IUpdateSet
{
    #region Set/SetWith/SetFrom
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，WHERE语句中要包含Update表与From表的关联條件，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .Set((a, b, c, d, e) =&gt; new { BuyerSource = b.SourceType, parameter.TotalAmount, BuyerSource = DBNull.Value, ... })
    ///     .Where((a, b, c, d, e) =&gt; a.Id == b.OrderId && a.BuyerId == 1 ...)
    /// SQL: UPDATE [sys_order] SET [BuyerSource]=b.[SourceType],TotalAmount=@TotalAmount,BuyerSource=@BuyerSource ... FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] ... AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，WHERE语句中要包含Update表与From表的关联條件，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .Set(true, (a, b, c, d, e) =&gt; new { BuyerSource = b.SourceType, parameter.TotalAmount, BuyerSource = DBNull.Value, ... })
    ///     .Where((a, b, c, d, e) =&gt; a.Id == b.OrderId && a.BuyerId == 1 ...)
    /// SQL: UPDATE [sys_order] SET [BuyerSource]=b.[SourceType],TotalAmount=@TotalAmount,BuyerSource=@BuyerSource ... FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] ... AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，用法：
    /// <code>.Set(x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如果为false，则不生成更新语句，用法：
    /// <code>.Set(condition, x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">字段筛选表达式，筛选单个栏位</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相同的栏位都将参与更新，单对象更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相当的栏位都将参与更新，单对象更新，为false不做更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var orderInfo = new OrderInfo { Id = 1, ... };
    /// var tmpObj = new { TotalAmount = 450, ... };
    /// ...
    /// .SetWith(true, (a, b, c, d, e) => new
    /// {
    ///     tmpObj.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     Remark = DBNull.Value, //直接赋值 NULL
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.BuyerId, //使用updateObjs对象中的参数
    ///     a.Disputes //使用updateObjs对象中的参数，实体对象由TypeHandler处理
    /// }, orderInfo) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET [TotalAmount`=@TotalAmount,[Products`=@Products,[Remark`=@Remark,[BuyerId`=NULL,BuyerSource]=b.[SourceType],[BuyerId`=@BuyerId,[Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var orderInfo = new OrderInfo { Id = 1, ... };
    /// var tmpObj = new { TotalAmount = 450, ... };
    /// ...
    /// .SetWith(true, (a, b, c, d, e) => new
    /// {
    ///     tmpObj.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     Remark = DBNull.Value, //直接赋值 NULL
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.BuyerId, //使用updateObjs对象中的参数
    ///     a.Disputes //使用updateObjs对象中的参数，实体对象由TypeHandler处理
    /// }, orderInfo) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET [TotalAmount`=@TotalAmount,[Products`=@Products,[Remark`=@Remark,[BuyerId`=NULL,BuyerSource]=b.[SourceType],[BuyerId`=@BuyerId,[Disputes`=@Disputes ...
    /// </code>
    /// 执行后的结果，TotalAmount，Products，BuyerId被更新为对应的值，ProductCount，Disputes被更新为orderInfo中对应的值，UpdatedAt栏位没有更新
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom((a, b, c, d, e) =&gt; new
    /// {
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom(true, (a, b, c, d, e) =&gt; new
    /// {
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment);
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
    IUpdateFrom<TEntity, T1, T2, T3, T4> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
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
    IUpdateFrom<TEntity, T1, T2, T3, T4> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1T2T3T4T5部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
/// Update ..From语句，在Where语句中一定要包含Update表与From表之间的关联条件
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表T2实体类型</typeparam>
/// <typeparam name="T3">更新值来源表T3实体类型</typeparam>
/// <typeparam name="T4">更新值来源表T4实体类型</typeparam>
/// <typeparam name="T5">更新值来源表T5实体类型</typeparam>
public interface IUpdateFrom<TEntity, T1, T2, T3, T4, T5> : IUpdateSet
{
    #region Set/SetWith/SetFrom
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，WHERE语句中要包含Update表与From表的关联條件，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .Set((a, b, c, d, e, f) =&gt; new { BuyerSource = b.SourceType, parameter.TotalAmount, BuyerSource = DBNull.Value, ... })
    ///     .Where((a, b, c, d, e, f) =&gt; a.Id == b.OrderId && a.BuyerId == 1 ...)
    /// SQL: UPDATE [sys_order] SET [BuyerSource]=b.[SourceType],TotalAmount=@TotalAmount,BuyerSource=@BuyerSource ... FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] ... AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，WHERE语句中要包含Update表与From表的关联條件，用法：
    /// <code>
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .Set(true, (a, b, c, d, e, f) =&gt; new { BuyerSource = b.SourceType, parameter.TotalAmount, BuyerSource = DBNull.Value, ... })
    ///     .Where((a, b, c, d, e, f) =&gt; a.Id == b.OrderId && a.BuyerId == 1 ...)
    /// SQL: UPDATE [sys_order] SET [BuyerSource]=b.[SourceType],TotalAmount=@TotalAmount,BuyerSource=@BuyerSource ... FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] ... AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，用法：
    /// <code>.Set(x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如果为false，则不生成更新语句，用法：
    /// <code>.Set(condition, x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">字段筛选表达式，筛选单个栏位</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相同的栏位都将参与更新，单对象更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相当的栏位都将参与更新，单对象更新，为false不做更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var orderInfo = new OrderInfo { Id = 1, ... };
    /// var tmpObj = new { TotalAmount = 450, ... };
    /// ...
    /// .SetWith(true, (a, b, c, d, e, f) => new
    /// {
    ///     tmpObj.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     Remark = DBNull.Value, //直接赋值 NULL
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.BuyerId, //使用updateObjs对象中的参数
    ///     a.Disputes //使用updateObjs对象中的参数，实体对象由TypeHandler处理
    /// }, orderInfo) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET [TotalAmount`=@TotalAmount,[Products`=@Products,[Remark`=@Remark,[BuyerId`=NULL,BuyerSource]=b.[SourceType],[BuyerId`=@BuyerId,[Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var orderInfo = new OrderInfo { Id = 1, ... };
    /// var tmpObj = new { TotalAmount = 450, ... };
    /// ...
    /// .SetWith(true, (a, b, c, d, e, f) => new
    /// {
    ///     tmpObj.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     Remark = DBNull.Value, //直接赋值 NULL
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.BuyerId, //使用updateObjs对象中的参数
    ///     a.Disputes //使用updateObjs对象中的参数，实体对象由TypeHandler处理
    /// }, orderInfo) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET [TotalAmount`=@TotalAmount,[Products`=@Products,[Remark`=@Remark,[BuyerId`=NULL,BuyerSource]=b.[SourceType],[BuyerId`=@BuyerId,[Disputes`=@Disputes ...
    /// </code>
    /// 执行后的结果，TotalAmount，Products，BuyerId被更新为对应的值，ProductCount，Disputes被更新为orderInfo中对应的值，UpdatedAt栏位没有更新
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom((a, b, c, d, e, f) =&gt; new
    /// {
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom(true, (a, b, c, d, e, f) =&gt; new
    /// {
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment);
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
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
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
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
public interface IUpdateJoin<TEntity, T1> : IUpdateSet
{
    #region Join
    /// <summary>
    /// 追加表T2字段数据InnerJoin内连接更新表TEntity数据
    /// </summary>
    /// <typeparam name="T2">数据来源表T2实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateJoin<TEntity, T1, T2> InnerJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn);
    /// <summary>
    /// 追加表T2字段数据LeftJoin左连接更新表TEntity数据
    /// </summary>
    /// <typeparam name="T2">数据来源表T2实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateJoin<TEntity, T1, T2> LeftJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn);
    #endregion

    #region Set/SetWith/SetFrom
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，用法：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    /// .Set(f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    /// .Set(true, f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Set<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，用法：
    /// <code>.Set(x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如果为false，则不生成更新语句，用法：
    /// <code>.Set(true, x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">字段筛选表达式，筛选单个栏位</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相同的栏位都将参与更新，单对象更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相当的栏位都将参与更新，单对象更新，为false不做更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)
    /// 用法：
    /// <code>
    /// var updateObj = new OrderInfo { ... };
    /// .SetWith(a, b, c, d, e, f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.Disputes //使用updateObj对象中的参数，实体对象由TypeHandler处理
    /// }, updateObj) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET a.`TotalAmount`=@TotalAmount,a.`Products`=@Products,a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`BuyerSource`=b.BuyerSource,a.`Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> SetWith<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var updateObj = new OrderInfo { ... };
    /// .SetWith(a, b, c, d, e, f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.Disputes //使用updateObj对象中的参数，实体对象由TypeHandler处理
    /// }, updateObj) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET a.`TotalAmount`=@TotalAmount,a.`Products`=@Products,a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`BuyerSource`=b.BuyerSource,a.`Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom(true, (a, b, c, d, e, f) =&gt; new
    /// {
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetFrom(condition, (x, y, z) =&gt; new
    ///     {
    ///         TotalAmount = x.From&lt;OrderDetail&gt;('a')
    ///             .Where(f =&gt; f.OrderId == y.Id)
    ///             .Select(t =&gt; Sql.Sum(t.Amount)),
    ///         OrderNo = y.OrderNo + "-111",
    ///         BuyerSource = z.SourceType
    ///     })
    ///     .Set(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(a.[Amount]) FROM [sys_order_detail] a WHERE a.[OrderId]=[sys_order].[Id]),[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsAssignment);
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
    IUpdateJoin<TEntity, T1> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
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
    IUpdateJoin<TEntity, T1> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate = null, Expression<Func<TEntity, T1, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1T2部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表T2实体类型</typeparam>
public interface IUpdateJoin<TEntity, T1, T2> : IUpdateSet
{
    #region Join
    /// <summary>
    /// 追加表T3字段数据InnerJoin内连接更新表TEntity数据
    /// </summary>
    /// <typeparam name="T3">数据来源表T3实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> InnerJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn);
    /// <summary>
    /// 追加表T3字段数据LeftJoin左连接更新表TEntity数据
    /// </summary>
    /// <typeparam name="T3">数据来源表T3实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> LeftJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn);
    #endregion

    #region Set/SetWith/SetFrom
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，用法：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    /// .Set(f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    /// .Set(true, f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，用法：
    /// <code>.Set(x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如果为false，则不生成更新语句，用法：
    /// <code>.Set(true, x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">字段筛选表达式，筛选单个栏位</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相同的栏位都将参与更新，单对象更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相当的栏位都将参与更新，单对象更新，为false不做更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)
    /// 用法：
    /// <code>
    /// var updateObj = new OrderInfo { ... };
    /// .SetWith(a, b, c, d, e, f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.Disputes //使用updateObj对象中的参数，实体对象由TypeHandler处理
    /// }, updateObj) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET a.`TotalAmount`=@TotalAmount,a.`Products`=@Products,a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`BuyerSource`=b.BuyerSource,a.`Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> SetWith<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var updateObj = new OrderInfo { ... };
    /// .SetWith(a, b, c, d, e, f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.Disputes //使用updateObj对象中的参数，实体对象由TypeHandler处理
    /// }, updateObj) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET a.`TotalAmount`=@TotalAmount,a.`Products`=@Products,a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`BuyerSource`=b.BuyerSource,a.`Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom(true, (a, b, c, d, e, f) =&gt; new
    /// {
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetFrom(condition, (x, y, z) =&gt; new
    ///     {
    ///         TotalAmount = x.From&lt;OrderDetail&gt;('a')
    ///             .Where(f =&gt; f.OrderId == y.Id)
    ///             .Select(t =&gt; Sql.Sum(t.Amount)),
    ///         OrderNo = y.OrderNo + "-111",
    ///         BuyerSource = z.SourceType
    ///     })
    ///     .Set(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(a.[Amount]) FROM [sys_order_detail] a WHERE a.[OrderId]=[sys_order].[Id]),[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsAssignment);
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
    IUpdateJoin<TEntity, T1, T2> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
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
    IUpdateJoin<TEntity, T1, T2> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1T2T3部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表T2实体类型</typeparam>
/// <typeparam name="T3">更新值来源表T3实体类型</typeparam>
public interface IUpdateJoin<TEntity, T1, T2, T3> : IUpdateSet
{
    #region Join
    /// <summary>
    /// 追加表T4字段数据InnerJoin内连接更新表TEntity数据
    /// </summary>
    /// <typeparam name="T4">数据来源表T4实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> InnerJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn);
    /// <summary>
    /// 追加表T4字段数据LeftJoin左连接更新表TEntity数据
    /// </summary>
    /// <typeparam name="T4">数据来源表T4实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> LeftJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn);
    #endregion

    #region Set/SetWith/SetFrom
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，用法：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    /// .Set(f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    /// .Set(true, f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，用法：
    /// <code>.Set(x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如果为false，则不生成更新语句，用法：
    /// <code>.Set(true, x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">字段筛选表达式，筛选单个栏位</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相同的栏位都将参与更新，单对象更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相当的栏位都将参与更新，单对象更新，为false不做更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)
    /// 用法：
    /// <code>
    /// var updateObj = new OrderInfo { ... };
    /// .SetWith(a, b, c, d, e, f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.Disputes //使用updateObj对象中的参数，实体对象由TypeHandler处理
    /// }, updateObj) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET a.`TotalAmount`=@TotalAmount,a.`Products`=@Products,a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`BuyerSource`=b.BuyerSource,a.`Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var updateObj = new OrderInfo { ... };
    /// .SetWith(a, b, c, d, e, f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.Disputes //使用updateObj对象中的参数，实体对象由TypeHandler处理
    /// }, updateObj) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET a.`TotalAmount`=@TotalAmount,a.`Products`=@Products,a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`BuyerSource`=b.BuyerSource,a.`Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom(true, (a, b, c, d, e, f) =&gt; new
    /// {
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetFrom(condition, (x, y, z) =&gt; new
    ///     {
    ///         TotalAmount = x.From&lt;OrderDetail&gt;('a')
    ///             .Where(f =&gt; f.OrderId == y.Id)
    ///             .Select(t =&gt; Sql.Sum(t.Amount)),
    ///         OrderNo = y.OrderNo + "-111",
    ///         BuyerSource = z.SourceType
    ///     })
    ///     .Set(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(a.[Amount]) FROM [sys_order_detail] a WHERE a.[OrderId]=[sys_order].[Id]),[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsAssignment);
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
    IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
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
    IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1T2T3T4部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表T2实体类型</typeparam>
/// <typeparam name="T3">更新值来源表T3实体类型</typeparam>
/// <typeparam name="T4">更新值来源表T4实体类型</typeparam>
public interface IUpdateJoin<TEntity, T1, T2, T3, T4> : IUpdateSet
{
    #region Join
    /// <summary>
    /// 追加表T5字段数据InnerJoin内连接更新表TEntity数据
    /// </summary>
    /// <typeparam name="T5">数据来源表T5实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn);
    /// <summary>
    /// 追加表T5字段数据LeftJoin左连接更新表TEntity数据
    /// </summary>
    /// <typeparam name="T5">数据来源表T5实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn);
    #endregion

    #region Set/SetWith/SetFrom
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，用法：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    /// .Set(f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    /// .Set(true, f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，用法：
    /// <code>.Set(x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如果为false，则不生成更新语句，用法：
    /// <code>.Set(true, x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">字段筛选表达式，筛选单个栏位</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相同的栏位都将参与更新，单对象更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相当的栏位都将参与更新，单对象更新，为false不做更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)
    /// 用法：
    /// <code>
    /// var updateObj = new OrderInfo { ... };
    /// .SetWith(a, b, c, d, e, f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.Disputes //使用updateObj对象中的参数，实体对象由TypeHandler处理
    /// }, updateObj) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET a.`TotalAmount`=@TotalAmount,a.`Products`=@Products,a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`BuyerSource`=b.BuyerSource,a.`Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var updateObj = new OrderInfo { ... };
    /// .SetWith(a, b, c, d, e, f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.Disputes //使用updateObj对象中的参数，实体对象由TypeHandler处理
    /// }, updateObj) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET a.`TotalAmount`=@TotalAmount,a.`Products`=@Products,a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`BuyerSource`=b.BuyerSource,a.`Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom(true, (a, b, c, d, e, f) =&gt; new
    /// {
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetFrom(condition, (x, y, z) =&gt; new
    ///     {
    ///         TotalAmount = x.From&lt;OrderDetail&gt;('a')
    ///             .Where(f =&gt; f.OrderId == y.Id)
    ///             .Select(t =&gt; Sql.Sum(t.Amount)),
    ///         OrderNo = y.OrderNo + "-111",
    ///         BuyerSource = z.SourceType
    ///     })
    ///     .Set(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(a.[Amount]) FROM [sys_order_detail] a WHERE a.[OrderId]=[sys_order].[Id]),[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment);
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
    IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
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
    IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null);
    #endregion
}
/// <summary>
/// 使用表T1T2T3T4T5部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表T2实体类型</typeparam>
/// <typeparam name="T3">更新值来源表T3实体类型</typeparam>
/// <typeparam name="T4">更新值来源表T4实体类型</typeparam>
/// <typeparam name="T5">更新值来源表T5实体类型</typeparam>
public interface IUpdateJoin<TEntity, T1, T2, T3, T4, T5> : IUpdateSet
{
    #region Set/SetWith/SetFrom
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，用法：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    /// .Set(f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    /// .Set(true, f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，用法：
    /// <code>.Set(x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如果为false，则不生成更新语句，用法：
    /// <code>.Set(true, x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">字段筛选表达式，筛选单个栏位</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相同的栏位都将参与更新，单对象更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象内所有与当前实体表TEntity名称相当的栏位都将参与更新，单对象更新，为false不做更新，用法：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value })  SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)
    /// 用法：
    /// <code>
    /// var updateObj = new OrderInfo { ... };
    /// .SetWith(a, b, c, d, e, f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.Disputes //使用updateObj对象中的参数，实体对象由TypeHandler处理
    /// }, updateObj) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET a.`TotalAmount`=@TotalAmount,a.`Products`=@Products,a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`BuyerSource`=b.BuyerSource,a.`Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsSelectorOrAssignment字段筛选和更新对象updateObj部分字段更新，updateObj对象内所有与表达式fieldsSelectorOrAssignment筛选字段名称相同的栏位都将参与更新，同时表达式fieldsSelectorOrAssignment也可以直接给字段赋值，单对象更新，
    /// fieldsSelectorOrAssignment字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)，用法：
    /// <code>
    /// var updateObj = new OrderInfo { ... };
    /// .SetWith(a, b, c, d, e, f => new
    /// {
    ///     parameter.TotalAmount, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///     BuyerSource = b.BuyerSource, //使用其他表栏位赋值
    ///     a.Disputes //使用updateObj对象中的参数，实体对象由TypeHandler处理
    /// }, updateObj) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET a.`TotalAmount`=@TotalAmount,a.`Products`=@Products,a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`BuyerSource`=b.BuyerSource,a.`Disputes`=@Disputes ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段筛选表达式要更新的所有字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObj">部分字段更新对象，包含想要更新的所需栏位值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsSelectorOrAssignment, object updateObj);
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，用法：
    /// <code>
    /// var tmpObj = new { Disputes = new Disputes{ ... }, ... };
    /// .SetFrom(true, (a, b, c, d, e, f) =&gt; new
    /// {
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .Select(t =&gt; Sql.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如果为false，则不生成更新语句，用法：
    /// <code>
    /// var condition = true;
    /// repository.Update&lt;Order&gt;()
    ///     .From&lt;User&gt;()
    ///     .SetFrom(condition, (x, y, z) =&gt; new
    ///     {
    ///         TotalAmount = x.From&lt;OrderDetail&gt;('a')
    ///             .Where(f =&gt; f.OrderId == y.Id)
    ///             .Select(t =&gt; Sql.Sum(t.Amount)),
    ///         OrderNo = y.OrderNo + "-111",
    ///         BuyerSource = z.SourceType
    ///     })
    ///     .Set(x =&gt; x.Products, new List&lt;int&gt; { 1, 2, 3 })
    ///     .Where((a, b) =&gt; a.BuyerId == b.Id && a.BuyerId == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(a.[Amount]) FROM [sys_order_detail] a WHERE a.[OrderId]=[sys_order].[Id]),[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式，可以一个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment);
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
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
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
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    #endregion
}