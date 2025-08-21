using System;
using System.Linq.Expressions;

namespace Trolley;

/// <summary>
/// 使用表T1部分字段数据，更新当前表TEntity数据
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
public interface IUpdateJoin<TEntity, T1> : IUpdated<TEntity>
{
    #region Sharding
    /// <summary>
    /// 直接指定T1表分表名，完整的表名，如：.UseTable("sys_order_202001")
    /// </summary>
    /// <param name="tableName">分表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> UseTable(string tableName);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T1表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> UseTableBy(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> UseTableSchema(string tableSchema);
    #endregion

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

    #region Set
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象中所有字段都将参与更新，如：
    /// <code>.Set(new { Id = 1, Name = "kevin", SourceType = DBNull.Value }) SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中所有字段都将参与更新，为false不做更新，如：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }) SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个字段，如：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    ///     .Set((a, b ...) => new
    ///     {
    ///         parameter.TotalAmount, //直接赋值，使用同名变量
    ///         Products = this.GetProducts(), //直接赋值，使用本地函数
    ///         BuyerId = DBNull.Value, //直接赋值 NULL
    ///         Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///         BuyerSource = b.BuyerSource //使用其他表栏位赋值
    ///     }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段可省略赋值字段，如：parameter.TotalAmount</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，如：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    ///     .Set(true, (a, b ...) => new
    ///     {
    ///         parameter.TotalAmount, //直接赋值，使用同名变量
    ///         Products = this.GetProducts(), //直接赋值，使用本地函数
    ///         BuyerId = DBNull.Value, //直接赋值 NULL
    ///         Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///         BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段省略赋值字段，如：parameter.TotalAmount</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Set<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如：
    /// <code>.Set(x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，否则不生成更新语句，如：
    /// <code>.Set(true, x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion

    #region SetFrom
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如：
    /// <code>
    /// var orderInfo = new { ... };
    /// .SetFrom((a, b) =&gt; new
    /// {
    ///     orderInfo.Disputes, //直接赋值，使用同名变量，实体对象由TypeHandler处理
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == b.Id)
    ///         .SelectAggregate((x, t) =&gt; x.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// })
    /// SQL: SET a.`Disputes`=@Disputes,a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式，可以单个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，如：
    /// <code>
    /// var orderInfo = new { ... };
    /// .SetFrom(true, (a, b) =&gt; new
    /// {
    ///     orderInfo.Disputes, //直接赋值，使用同名变量，实体对象由TypeHandler处理
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == b.Id)
    ///         .SelectAggregate((x, t) =&gt; x.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// })
    /// SQL: SET a.`Disputes`=@Disputes,a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式，可以单个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个栏位，子查询表达式valueSelector捞取更新值，部分栏位更新，表达式fieldSelector只能筛选一个栏位，如：
    /// <code>
    /// .SetFrom(f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///     .From&lt;OrderDetail&gt;('c')
    ///     .Where(f =&gt; f.OrderId == y.Id)
    ///     .SelectAggregate((x, t) =&gt; x.Sum(t.Amount))) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldSelector">单个字段筛选表达式</param>
    /// <param name="valueSelector">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个栏位，子查询表达式valueSelector捞取更新值，部分栏位更新，如果为false，则不生成更新语句，表达式fieldSelector只能筛选一个栏位，如：
    /// <code>
    /// .SetFrom(true, f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///     .From&lt;OrderDetail&gt;('c')
    ///     .Where(f =&gt; f.OrderId == y.Id)
    ///     .SelectAggregate((x, t) =&gt; x.Sum(t.Amount))) ...
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

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> WherePredicate(Func<PredicateBuilder<TEntity, T1>, Expression<Func<TEntity, T1, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate = null, Expression<Func<TEntity, T1, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> AndPredicate(Func<PredicateBuilder<TEntity, T1>, Expression<Func<TEntity, T1, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Or(Expression<Func<TEntity, T1, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Or(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate = null, Expression<Func<TEntity, T1, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> OrPredicate(Func<PredicateBuilder<TEntity, T1>, Expression<Func<TEntity, T1, bool>>> predicateInitializer);
    #endregion
}
/// <summary>
/// 使用表T1,T2部分字段数据，更新当前表TEntity数据
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表T2实体类型</typeparam>
public interface IUpdateJoin<TEntity, T1, T2> : IUpdated<TEntity>
{
    #region Sharding
    /// <summary>
    /// 直接指定T2表分表名，完整的表名，如：.UseTable("sys_order_202001")
    /// </summary>
    /// <param name="tableName">分表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> UseTable(string tableName);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T2表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> UseTableBy(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> UseTableSchema(string tableSchema);
    #endregion

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

    #region Set
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象中所有字段都将参与更新，如：
    /// <code>.Set(new { Id = 1, Name = "kevin", SourceType = DBNull.Value }) SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中所有字段都将参与更新，为false不做更新，如：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }) SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个字段，如：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    ///     .Set((a, b ...) => new
    ///     {
    ///         parameter.TotalAmount, //直接赋值，使用同名变量
    ///         Products = this.GetProducts(), //直接赋值，使用本地函数
    ///         BuyerId = DBNull.Value, //直接赋值 NULL
    ///         Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///         BuyerSource = b.BuyerSource //使用其他表栏位赋值
    ///     }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段可省略赋值字段，如：parameter.TotalAmount</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，如：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    ///     .Set(true, (a, b ...) => new
    ///     {
    ///         parameter.TotalAmount, //直接赋值，使用同名变量
    ///         Products = this.GetProducts(), //直接赋值，使用本地函数
    ///         BuyerId = DBNull.Value, //直接赋值 NULL
    ///         Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///         BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段省略赋值字段，如：parameter.TotalAmount</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如：
    /// <code>.Set(x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，否则不生成更新语句，如：
    /// <code>.Set(true, x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion

    #region SetFrom
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如：
    /// <code>
    /// var orderInfo = new { ... };
    /// .SetFrom((a, b) =&gt; new
    /// {
    ///     orderInfo.Disputes, //直接赋值，使用同名变量，实体对象由TypeHandler处理
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == b.Id)
    ///         .SelectAggregate((x, t) =&gt; x.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// })
    /// SQL: SET a.`Disputes`=@Disputes,a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式，可以单个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，如：
    /// <code>
    /// var orderInfo = new { ... };
    /// .SetFrom(true, (a, b) =&gt; new
    /// {
    ///     orderInfo.Disputes, //直接赋值，使用同名变量，实体对象由TypeHandler处理
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == b.Id)
    ///         .SelectAggregate((x, t) =&gt; x.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// })
    /// SQL: SET a.`Disputes`=@Disputes,a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式，可以单个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个栏位，子查询表达式valueSelector捞取更新值，部分栏位更新，表达式fieldSelector只能筛选一个栏位，如：
    /// <code>
    /// .SetFrom(f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///     .From&lt;OrderDetail&gt;('c')
    ///     .Where(f =&gt; f.OrderId == y.Id)
    ///     .SelectAggregate((x, t) =&gt; x.Sum(t.Amount))) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldSelector">单个字段筛选表达式</param>
    /// <param name="valueSelector">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个栏位，子查询表达式valueSelector捞取更新值，部分栏位更新，如果为false，则不生成更新语句，表达式fieldSelector只能筛选一个栏位，如：
    /// <code>
    /// .SetFrom(true, f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///     .From&lt;OrderDetail&gt;('c')
    ///     .Where(f =&gt; f.OrderId == y.Id)
    ///     .SelectAggregate((x, t) =&gt; x.Sum(t.Amount))) ...
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

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> WherePredicate(Func<PredicateBuilder<TEntity, T1, T2>, Expression<Func<TEntity, T1, T2, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> AndPredicate(Func<PredicateBuilder<TEntity, T1, T2>, Expression<Func<TEntity, T1, T2, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Or(Expression<Func<TEntity, T1, T2, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Or(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> OrPredicate(Func<PredicateBuilder<TEntity, T1, T2>, Expression<Func<TEntity, T1, T2, bool>>> predicateInitializer);
    #endregion
}
/// <summary>
/// 使用表T1,T2,T3部分字段数据，更新当前表TEntity数据
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表T2实体类型</typeparam>
/// <typeparam name="T3">更新值来源表T3实体类型</typeparam>
public interface IUpdateJoin<TEntity, T1, T2, T3> : IUpdated<TEntity>
{
    #region Sharding
    /// <summary>
    /// 直接指定T3表分表名，完整的表名，如：.UseTable("sys_order_202001")
    /// </summary>
    /// <param name="tableName">分表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> UseTable(string tableName);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T3表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> UseTableBy(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> UseTableSchema(string tableSchema);
    #endregion

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

    #region Set
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象中所有字段都将参与更新，如：
    /// <code>.Set(new { Id = 1, Name = "kevin", SourceType = DBNull.Value }) SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中所有字段都将参与更新，为false不做更新，如：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }) SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个字段，如：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    ///     .Set((a, b ...) => new
    ///     {
    ///         parameter.TotalAmount, //直接赋值，使用同名变量
    ///         Products = this.GetProducts(), //直接赋值，使用本地函数
    ///         BuyerId = DBNull.Value, //直接赋值 NULL
    ///         Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///         BuyerSource = b.BuyerSource //使用其他表栏位赋值
    ///     }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段可省略赋值字段，如：parameter.TotalAmount</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，如：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    ///     .Set(true, (a, b ...) => new
    ///     {
    ///         parameter.TotalAmount, //直接赋值，使用同名变量
    ///         Products = this.GetProducts(), //直接赋值，使用本地函数
    ///         BuyerId = DBNull.Value, //直接赋值 NULL
    ///         Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///         BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段省略赋值字段，如：parameter.TotalAmount</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如：
    /// <code>.Set(x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，否则不生成更新语句，如：
    /// <code>.Set(true, x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion

    #region SetFrom
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如：
    /// <code>
    /// var orderInfo = new { ... };
    /// .SetFrom((a, b) =&gt; new
    /// {
    ///     orderInfo.Disputes, //直接赋值，使用同名变量，实体对象由TypeHandler处理
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == b.Id)
    ///         .SelectAggregate((x, t) =&gt; x.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// })
    /// SQL: SET a.`Disputes`=@Disputes,a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式，可以单个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，如：
    /// <code>
    /// var orderInfo = new { ... };
    /// .SetFrom(true, (a, b) =&gt; new
    /// {
    ///     orderInfo.Disputes, //直接赋值，使用同名变量，实体对象由TypeHandler处理
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == b.Id)
    ///         .SelectAggregate((x, t) =&gt; x.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// })
    /// SQL: SET a.`Disputes`=@Disputes,a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式，可以单个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个栏位，子查询表达式valueSelector捞取更新值，部分栏位更新，表达式fieldSelector只能筛选一个栏位，如：
    /// <code>
    /// .SetFrom(f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///     .From&lt;OrderDetail&gt;('c')
    ///     .Where(f =&gt; f.OrderId == y.Id)
    ///     .SelectAggregate((x, t) =&gt; x.Sum(t.Amount))) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldSelector">单个字段筛选表达式</param>
    /// <param name="valueSelector">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个栏位，子查询表达式valueSelector捞取更新值，部分栏位更新，如果为false，则不生成更新语句，表达式fieldSelector只能筛选一个栏位，如：
    /// <code>
    /// .SetFrom(true, f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///     .From&lt;OrderDetail&gt;('c')
    ///     .Where(f =&gt; f.OrderId == y.Id)
    ///     .SelectAggregate((x, t) =&gt; x.Sum(t.Amount))) ...
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

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> WherePredicate(Func<PredicateBuilder<TEntity, T1, T2, T3>, Expression<Func<TEntity, T1, T2, T3, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> AndPredicate(Func<PredicateBuilder<TEntity, T1, T2, T3>, Expression<Func<TEntity, T1, T2, T3, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Or(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Or(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> OrPredicate(Func<PredicateBuilder<TEntity, T1, T2, T3>, Expression<Func<TEntity, T1, T2, T3, bool>>> predicateInitializer);
    #endregion
}
/// <summary>
/// 使用表T1,T2,T3,T4部分字段数据，更新当前表TEntity数据
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表T2实体类型</typeparam>
/// <typeparam name="T3">更新值来源表T3实体类型</typeparam>
/// <typeparam name="T4">更新值来源表T4实体类型</typeparam>
public interface IUpdateJoin<TEntity, T1, T2, T3, T4> : IUpdated<TEntity>
{
    #region Sharding
    /// <summary>
    /// 直接指定T4表分表名，完整的表名，如：.UseTable("sys_order_202001")
    /// </summary>
    /// <param name="tableName">分表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> UseTable(string tableName);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T4表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> UseTableBy(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> UseTableSchema(string tableSchema);
    #endregion

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

    #region Set
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象中所有字段都将参与更新，如：
    /// <code>.Set(new { Id = 1, Name = "kevin", SourceType = DBNull.Value }) SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中所有字段都将参与更新，为false不做更新，如：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }) SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个字段，如：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    ///     .Set((a, b ...) => new
    ///     {
    ///         parameter.TotalAmount, //直接赋值，使用同名变量
    ///         Products = this.GetProducts(), //直接赋值，使用本地函数
    ///         BuyerId = DBNull.Value, //直接赋值 NULL
    ///         Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///         BuyerSource = b.BuyerSource //使用其他表栏位赋值
    ///     }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段可省略赋值字段，如：parameter.TotalAmount</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，如：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    ///     .Set(true, (a, b ...) => new
    ///     {
    ///         parameter.TotalAmount, //直接赋值，使用同名变量
    ///         Products = this.GetProducts(), //直接赋值，使用本地函数
    ///         BuyerId = DBNull.Value, //直接赋值 NULL
    ///         Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///         BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段省略赋值字段，如：parameter.TotalAmount</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如：
    /// <code>.Set(x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，否则不生成更新语句，如：
    /// <code>.Set(true, x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion

    #region SetFrom
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如：
    /// <code>
    /// var orderInfo = new { ... };
    /// .SetFrom((a, b) =&gt; new
    /// {
    ///     orderInfo.Disputes, //直接赋值，使用同名变量，实体对象由TypeHandler处理
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == b.Id)
    ///         .SelectAggregate((x, t) =&gt; x.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// })
    /// SQL: SET a.`Disputes`=@Disputes,a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式，可以单个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，如：
    /// <code>
    /// var orderInfo = new { ... };
    /// .SetFrom(true, (a, b) =&gt; new
    /// {
    ///     orderInfo.Disputes, //直接赋值，使用同名变量，实体对象由TypeHandler处理
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == b.Id)
    ///         .SelectAggregate((x, t) =&gt; x.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// })
    /// SQL: SET a.`Disputes`=@Disputes,a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式，可以单个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个栏位，子查询表达式valueSelector捞取更新值，部分栏位更新，表达式fieldSelector只能筛选一个栏位，如：
    /// <code>
    /// .SetFrom(f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///     .From&lt;OrderDetail&gt;('c')
    ///     .Where(f =&gt; f.OrderId == y.Id)
    ///     .SelectAggregate((x, t) =&gt; x.Sum(t.Amount))) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldSelector">单个字段筛选表达式</param>
    /// <param name="valueSelector">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个栏位，子查询表达式valueSelector捞取更新值，部分栏位更新，如果为false，则不生成更新语句，表达式fieldSelector只能筛选一个栏位，如：
    /// <code>
    /// .SetFrom(true, f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///     .From&lt;OrderDetail&gt;('c')
    ///     .Where(f =&gt; f.OrderId == y.Id)
    ///     .SelectAggregate((x, t) =&gt; x.Sum(t.Amount))) ...
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

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> WherePredicate(Func<PredicateBuilder<TEntity, T1, T2, T3, T4>, Expression<Func<TEntity, T1, T2, T3, T4, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> AndPredicate(Func<PredicateBuilder<TEntity, T1, T2, T3, T4>, Expression<Func<TEntity, T1, T2, T3, T4, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Or(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Or(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> OrPredicate(Func<PredicateBuilder<TEntity, T1, T2, T3, T4>, Expression<Func<TEntity, T1, T2, T3, T4, bool>>> predicateInitializer);
    #endregion
}
/// <summary>
/// 使用表T1,T2,T3,T4,T5部分字段数据，更新当前表TEntity数据
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
/// <typeparam name="T2">更新值来源表T2实体类型</typeparam>
/// <typeparam name="T3">更新值来源表T3实体类型</typeparam>
/// <typeparam name="T4">更新值来源表T4实体类型</typeparam>
/// <typeparam name="T5">更新值来源表T5实体类型</typeparam>
public interface IUpdateJoin<TEntity, T1, T2, T3, T4, T5> : IUpdated<TEntity>
{
    #region Sharding
    /// <summary>
    /// 直接指定T5表分表名，完整的表名，如：.UseTable("sys_order_202001")
    /// </summary>
    /// <param name="tableName">分表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTable(string tableName);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T5表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTableBy(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTableSchema(string tableSchema);
    #endregion

    #region Set
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象中所有字段都将参与更新，如：
    /// <code>.Set(new { Id = 1, Name = "kevin", SourceType = DBNull.Value }) SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中所有字段都将参与更新，为false不做更新，如：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }) SQL: SET `Name`=@Name,SourceType=@SourceType</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个字段，如：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    ///     .Set((a, b ...) => new
    ///     {
    ///         parameter.TotalAmount, //直接赋值，使用同名变量
    ///         Products = this.GetProducts(), //直接赋值，使用本地函数
    ///         BuyerId = DBNull.Value, //直接赋值 NULL
    ///         Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///         BuyerSource = b.BuyerSource //使用其他表栏位赋值
    ///     }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段可省略赋值字段，如：parameter.TotalAmount</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，如：
    /// <code>
    /// var parameter = new OrderInfo { ... };
    ///     .Set(true, (a, b ...) => new
    ///     {
    ///         parameter.TotalAmount, //直接赋值，使用同名变量
    ///         Products = this.GetProducts(), //直接赋值，使用本地函数
    ///         BuyerId = DBNull.Value, //直接赋值 NULL
    ///         Disputes = new Dispute { ... }, //直接赋值，实体对象由TypeHandler处理
    ///         BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// }) ...
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// SQL: SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes,`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段省略赋值字段，如：parameter.TotalAmount</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如：
    /// <code>.Set(x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，否则不生成更新语句，如：
    /// <code>.Set(true, x =&gt; x.TotalAmount, 200.56)</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion

    #region SetFrom
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如：
    /// <code>
    /// var orderInfo = new { ... };
    /// .SetFrom((a, b) =&gt; new
    /// {
    ///     orderInfo.Disputes, //直接赋值，使用同名变量，实体对象由TypeHandler处理
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == b.Id)
    ///         .SelectAggregate((x, t) =&gt; x.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// })
    /// SQL: SET a.`Disputes`=@Disputes,a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式，可以单个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，如：
    /// <code>
    /// var orderInfo = new { ... };
    /// .SetFrom(true, (a, b) =&gt; new
    /// {
    ///     orderInfo.Disputes, //直接赋值，使用同名变量，实体对象由TypeHandler处理
    ///     TotalAmount = a.From&lt;OrderDetail&gt;('x')
    ///         .Where(f =&gt; f.OrderId == b.Id)
    ///         .SelectAggregate((x, t) =&gt; x.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001", //直接赋值
    ///     BuyerId = DBNull.Value, //直接赋值 NULL
    ///     tmpObj.Disputes, //直接赋值，使用同名变量
    ///     Products = this.GetProducts(), //直接赋值，使用本地函数
    ///     BuyerSource = b.BuyerSource //使用其他表栏位赋值
    /// })
    /// SQL: SET a.`Disputes`=@Disputes,a.`TotalAmount`=(SELECT SUM(x.`Amount`) FROM `sys_order_detail` x WHERE x.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerId`=NULL,a.`Disputes`=@Disputes,a.`Products`=@Products,a.`BuyerSource`=b.BuyerSource ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">子查询表达式，可以单个或多个字段赋值</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个栏位，子查询表达式valueSelector捞取更新值，部分栏位更新，表达式fieldSelector只能筛选一个栏位，如：
    /// <code>
    /// .SetFrom(f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///     .From&lt;OrderDetail&gt;('c')
    ///     .Where(f =&gt; f.OrderId == y.Id)
    ///     .SelectAggregate((x, t) =&gt; x.Sum(t.Amount))) ...
    /// SQL: SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TField">子查询返回的单个字段类型</typeparam>
    /// <param name="fieldSelector">单个字段筛选表达式</param>
    /// <param name="valueSelector">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个栏位，子查询表达式valueSelector捞取更新值，部分栏位更新，如果为false，则不生成更新语句，表达式fieldSelector只能筛选一个栏位，如：
    /// <code>
    /// .SetFrom(true, f =&gt; f.TotalAmount, (x, y) =&gt; x
    ///     .From&lt;OrderDetail&gt;('c')
    ///     .Where(f =&gt; f.OrderId == y.Id)
    ///     .SelectAggregate((x, t) =&gt; x.Sum(t.Amount))) ...
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

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> WherePredicate(Func<PredicateBuilder<TEntity, T1, T2, T3, T4, T5>, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> AndPredicate(Func<PredicateBuilder<TEntity, T1, T2, T3, T4, T5>, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Or(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Or(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> OrPredicate(Func<PredicateBuilder<TEntity, T1, T2, T3, T4, T5>, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>>> predicateInitializer);
    #endregion
}