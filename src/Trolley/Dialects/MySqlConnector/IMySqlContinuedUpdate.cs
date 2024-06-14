using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

/// <summary>
/// 更新数据
/// </summary>
/// <typeparam name="TEntity">要更新的实体类型</typeparam>
public interface IMySqlContinuedUpdate<TEntity> : IContinuedUpdate<TEntity>
{
    #region Set
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，用法：
    /// <code>.Set(new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id); .Set(new User { Id = 2, Name = "kevin", SourceType = null }).Where(f =&gt; f.Id);  
    /// SQL: SET `Name`=@Name,SourceType=@SourceType WHERE `Id`=@kId </code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    new IMySqlContinuedUpdate<TEntity> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，为false不做更新，用法：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id);  SQL: SET `Name`=@Name,SourceType=@SourceType WHERE `Id`=@kId
    /// .Set(true, new User { Id = 1, ... })  SQL: SET ... //只更新部分字段，可以使用OnlyFields方法，忽略部分字段，可以使用IgnoreFields方法</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    new IMySqlContinuedUpdate<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
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
    new IMySqlContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment);
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
    new IMySqlContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，用法：
    /// <code>.Set(x =&gt; x.OrderNo, "ON_111")</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    new IMySqlContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，否则不生成更新语句，用法：
    /// <code>.Set(true, x =&gt; x.OrderNo, "ON_111")</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    new IMySqlContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion

    #region SetFrom
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
    new IMySqlContinuedUpdate<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
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
    new IMySqlContinuedUpdate<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
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
    new IMySqlContinuedUpdate<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
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
    new IMySqlContinuedUpdate<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    #endregion

    #region IgnoreFields
    /// <summary>
    /// 不更新指定字段，如：IgnoreFields("Id","Age");IgnoreFields("Id");
    /// </summary>
    /// <param name="fieldNames">忽略更新的字段列表</param>
    /// <returns>返回更新对象</returns>
    new IMySqlContinuedUpdate<TEntity> IgnoreFields(params string[] fieldNames);
    /// <summary>
    /// 不更新指定字段，如：.IgnoreFields(f =&gt; new { f.Id, f.Age}); .IgnoreFields(f =&gt; f.Id);
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="fieldsSelector">字段选择表达式，支持MemberAccess、New或MemberInit类型表达式</param>
    /// <returns></returns>
    new IMySqlContinuedUpdate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion

    #region OnlyFields
    /// <summary>
    /// 只更新指定字段，如：.OnlyFields("Name","Gender"); .OnlyFields("Name");
    /// </summary>
    /// <param name="fieldNames">忽略更新的字段列表</param>
    /// <returns>返回更新对象</returns>
    new IMySqlContinuedUpdate<TEntity> OnlyFields(params string[] fieldNames);
    /// <summary>
    /// 只更新指定字段，如：.OnlyFields(f =&gt; new { f.Name, f.Gender}); .OnlyFields(f =&gt; f.Name);
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="fieldsSelector">字段选择表达式，支持MemberAccess、New或MemberInit类型表达式</param>
    /// <returns>返回更新对象</returns>
    new IMySqlContinuedUpdate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用whereObj对象生成Where条件，whereObj对象内所有与当前实体表TEntity名称相同的栏位都将参与where条件过滤，whereObj对象可以是匿名对象、命名对象或是字典，推荐使用匿名对象，不能为null
    /// </summary>
    /// <typeparam name="TWhereObj">where条件对象类型</typeparam>
    /// <param name="whereObj">where条件对象，whereObj对象内所有与当前实体表TEntity名称相同的栏位都将参与where条件过滤，可以是匿名对象、命名对象或是字典，推荐使用匿名对象，不能为null</param>
    /// <returns>返回更新对象</returns>
    new IMySqlUpdated<TEntity> Where<TWhereObj>(TWhereObj whereObj);
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    new IMySqlContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    new IMySqlContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    new IMySqlContinuedUpdate<TEntity> And(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    new IMySqlContinuedUpdate<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate = null, Expression<Func<TEntity, bool>> elsePredicate = null);
    #endregion
}