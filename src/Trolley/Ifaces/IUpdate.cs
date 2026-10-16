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
public interface IUpdate
{
    #region Properties
    DbContext DbContext { get; }
    IUpdateVisitor Visitor { get; }
    #endregion

    #region Sharding
    /// <summary>
    /// 直接指定当前表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回更新对象</returns>
    IUpdate UseTable(params string[] tableNames);
    /// <summary>
    /// 手动指定分表规则参数值，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回更新对象</returns>
    IUpdate UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表名获取委托，执行委托获取分表名，更新对象的值自动更新对应分表中。参数是更新对象的值，返回值是分表名，如：.UseTable(insertObj =&gt; $"sys_user_{((User)insertObj).CreatedAt:yyyyMM}")
    /// </summary>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <returns>返回插入对象</returns>
    IUpdate UseTable(Func<object, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TEntity"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回更新对象</returns>
    IUpdate UseTableByRange(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回更新对象</returns>
    IUpdate UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回更新对象</returns>
    IUpdate WithTableAliasTrailing(string rawSql);
    #endregion

    #region Set
    /// <summary>
    /// 部分字段更新，updateObj对象中除主键、OnlyFields、IgnoreFields方法筛选后的剩下所有字段都将参与更新，匿名、命名对象都可以，需要配合where条件使用，如：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id = 1); .Set(new User { Name = "kevin", SourceType = null });
    /// SQL: SET `Name`=@Name,SourceType=@SourceType WHERE `Id`=@kId</code>
    /// </summary>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate Set(object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中除主键、OnlyFields、IgnoreFields方法筛选后的剩下所有字段都将参与更新，匿名、命名对象都可以，需要配合where条件使用，为false不更新，如：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id = 1); .Set(true, new User { Name = "kevin", SourceType = null });
    /// SQL: SET `Name`=@Name,SourceType=@SourceType WHERE `Id`=@kId</code>
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate Set(bool condition, object updateObj);
    /// <summary>
    /// 固定值单字段更新，如：<code>.Set("OrderNo", "ON_111")</code>
    /// </summary>
    /// <param name="fieldName">字段名称</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate Set(string fieldName, object fieldValue);
    /// <summary>
    /// 固定值单字段更新，如：<code>.Set(true, "OrderNo", "ON_111")</code>
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldName">字段名称</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate Set(bool condition, string fieldName, object fieldValue);
    #endregion

    #region SetBulk
    /// <summary>
    /// 使用集合对象updateObjs部分字段批量更新，根据主键字段更新，updateObjs对象中除主键、OnlyFields、IgnoreFields方法筛选后的剩下所有字段都将参与更新。支持分批次更新，更新条数超过设置的bulkCount值，将在下次更新，直到所有数据更新完毕，bulkCount默认500，
    /// 可以继续使用Set、OnlyFields、IgnoreFields等方法，如：
    /// <code>
    /// var updateObjs = new Order[]{ ... ...};
    /// .SetBulk(updateObjs).Set(new { Name = "kevin"} ...
    /// SQL: UPDATE `sys_order` SET ...Name=@Name0 WHERE `Id`=@kId0;UPDATE `sys_order` SET ...Name=@Name1 WHERE `Id`=@kId1; ...
    /// </code>
    /// </summary>
    /// <param name="updateObjs">更新对象参数集合，包含更新字段和where条件字段</param>
    /// <param name="bulkCount">单次更新的最大数据条数，默认是500</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate SetBulk(IEnumerable updateObjs, int bulkCount = 500);
    #endregion
}
/// <summary>
/// 更新数据
/// </summary>
public interface IUpdated
{
    #region Properties
    DbContext DbContext { get; }
    IUpdateVisitor Visitor { get; }
    #endregion

    #region WithRawSql
    /// <summary>
    /// 在最前面添加原始SQL片段，rawSql可以是任意SQL片段，如：polardb-x数据库，.WithLeadingSql("/*+TDDL:CMD_EXTRA(TTL_FORBID_DROP_TTL_TBL_WITH_ARC_CCI=false)*/")
    /// </summary>
    /// <param name="rawSql">原始SQL片</param>
    /// <returns>返回Repository对象</returns>
    IUpdated WithLeadingSql(string rawSql);
    /// <summary>
    /// 在最后面添加原始SQL片段，rawSql可以是任意SQL片段
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回Repository对象</returns>
    IUpdated WithTrailingSql(string rawSql);
    #endregion

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
public interface IContinuedUpdate : IUpdated
{
    #region Set
    /// <summary>
    /// 固定值单字段更新，如：<code>.Set("OrderNo", "ON_111")</code>
    /// </summary>
    /// <param name="fieldName">字段名称</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate Set(string fieldName, object fieldValue);
    /// <summary>
    /// 固定值单字段更新，如：<code>.Set(true, "OrderNo", "ON_111")</code>
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldName">字段名称</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate Set(bool condition, string fieldName, object fieldValue);
    #endregion

    #region IgnoreFields
    /// <summary>
    /// 不更新指定字段，如：.IgnoreFields("Id","Age")
    /// </summary>
    /// <param name="fieldNames">忽略更新的字段列表</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate IgnoreFields(params string[] fieldNames);
    #endregion

    #region OnlyFields
    /// <summary>
    /// 只更新指定字段，如：.OnlyFields("Name","Gender")
    /// </summary>
    /// <param name="fieldNames">忽略更新的字段列表</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate OnlyFields(params string[] fieldNames);
    #endregion

    #region Where
    /// <summary>
    /// 条件更新，如：.WhereBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate WhereBy(object whereObj);
    /// <summary>
    /// 条件更新，如：.WhereBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    IContinuedUpdate WhereBy(bool condition, object whereObj);
    /// <summary>
    /// 主键条件更新，如：.WhereById(1) 或是 .WhereById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate WhereById(object whereKey);
    /// <summary>
    /// 主键条件更新，如：.WhereById(1) 或是 .WhereById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate WhereById(bool condition, object whereKey);
    /// <summary>
    /// 多主键条件查询，如：.WhereByIds(new int[]{1,2,3}) 或是 .WhereByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate WhereByIds(IEnumerable whereKeys);
    /// <summary>
    /// 多主键条件更新，如：.WhereByIds(new int[]{1,2,3}) 或是 .WhereByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate WhereByIds(bool condition, IEnumerable whereKeys);
    #endregion

    #region And
    /// <summary>
    /// 条件更新，并与已有的条件AND操作，如：.AndBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate AndBy(object whereObj);
    /// <summary>
    /// 条件更新，并与已有的条件AND操作，如：.AndBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    IContinuedUpdate AndBy(bool condition, object whereObj);
    /// <summary>
    /// 主键条件更新，并与已有的条件AND操作，如：.AndById(1) 或是 .AndById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate AndById(object whereKey);
    /// <summary>
    /// 主键条件更新，并与已有的条件AND操作，如：.AndById(1) 或是 .AndById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate AndById(bool condition, object whereKey);
    /// <summary>
    /// 多主键条件更新，并与已有的条件AND操作，如：.AndByIds(new int[]{1,2,3}) 或是 .AndByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate AndByIds(IEnumerable whereKeys);
    /// <summary>
    /// 多主键条件更新，并与已有的条件AND操作，如：.AndByIds(new int[]{1,2,3}) 或是 .AndByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate AndByIds(bool condition, IEnumerable whereKeys);
    #endregion

    #region Or
    /// <summary>
    /// 条件更新，并与已有的条件OR操作，如：.OrBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate OrBy(object whereObj);
    /// <summary>
    /// 条件更新，并与已有的条件OR操作，如：.OrBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    IContinuedUpdate OrBy(bool condition, object whereObj);
    /// <summary>
    /// 主键条件更新，并与已有的条件OR操作，如：.OrById(1) 或是 .OrById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate OrById(object whereKey);
    /// <summary>
    /// 主键条件更新，并与已有的条件OR操作，如：.OrById(1) 或是 .OrById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate OrById(bool condition, object whereKey);
    /// <summary>
    /// 多主键条件更新，并与已有的条件OR操作，如：.OrByIds(new int[]{1,2,3}) 或是 .OrByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate OrByIds(IEnumerable whereKeys);
    /// <summary>
    /// 多主键条件更新，并与已有的条件OR操作，如：.OrByIds(new int[]{1,2,3}) 或是 .OrByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate OrByIds(bool condition, IEnumerable whereKeys);
    #endregion
}
/// <summary>
/// 更新数据
/// </summary>
public interface IBulkContinuedUpdate : IUpdated
{
    #region Set
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，如：
    /// <code>.Set(new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id); .Set(new User { Id = 2, Name = "kevin", SourceType = null }).Where(f =&gt; f.Id);  
    /// SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId </code>
    /// </summary>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate Set(object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，为false不做更新，如：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id);  SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId
    /// .Set(true, new User { Id = 1, ... })  SQL: SET ... //只更新部分字段，可以使用OnlyFields方法，忽略部分字段，可以使用IgnoreFields方法</code>
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate Set(bool condition, object updateObj);
    /// <summary>
    /// 固定值单字段更新，如：<code>.Set("OrderNo", "ON_111")</code>
    /// </summary>
    /// <param name="fieldName">字段名称</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate Set(string fieldName, object fieldValue);
    /// <summary>
    /// 固定值单字段更新，如：<code>.Set(true, "OrderNo", "ON_111")</code>
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldName">字段名称</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate Set(bool condition, string fieldName, object fieldValue);
    #endregion

    #region IgnoreFields
    /// <summary>
    /// 不更新指定字段，如：.IgnoreFields("Id","Age")
    /// </summary>
    /// <param name="fieldNames">忽略更新的字段列表</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate IgnoreFields(params string[] fieldNames);
    #endregion

    #region OnlyFields
    /// <summary>
    /// 只更新指定字段，如：.OnlyFields("Name","Gender")
    /// </summary>
    /// <param name="fieldNames">忽略更新的字段列表</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate OnlyFields(params string[] fieldNames);
    #endregion

    #region Where
    /// <summary>
    /// 条件更新，如：.WhereBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate WhereBy(object whereObj);
    /// <summary>
    /// 条件更新，如：.WhereBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    IBulkContinuedUpdate WhereBy(bool condition, object whereObj);
    #endregion

    #region And
    /// <summary>
    /// 条件更新，并与已有的条件AND操作，如：.AndBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate AndBy(object whereObj);
    /// <summary>
    /// 条件更新，并与已有的条件AND操作，如：.AndBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    IBulkContinuedUpdate AndBy(bool condition, object whereObj);
    #endregion

    #region Or
    /// <summary>
    /// 条件更新，并与已有的条件OR操作，如：.OrBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate OrBy(object whereObj);
    /// <summary>
    /// 条件更新，并与已有的条件OR操作，如：.OrBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    IBulkContinuedUpdate OrBy(bool condition, object whereObj);
    #endregion
}
/// <summary>
/// 更新数据
/// </summary>
/// <typeparam name="TEntity">要更新的实体类型</typeparam>
public interface IBulkCopyContinuedUpdate : IUpdated
{
    #region Where
    /// <summary>
    /// 条件更新，如：.WhereBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    IBulkCopyContinuedUpdate WhereBy(object whereObj);
    /// <summary>
    /// 条件更新，如：.WhereBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    IBulkCopyContinuedUpdate WhereBy(bool condition, object whereObj);
    #endregion

    #region And
    /// <summary>
    /// 条件更新，如：.AndBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    IBulkCopyContinuedUpdate AndBy(object whereObj);
    /// <summary>
    /// 条件更新，如：.AndBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    IBulkCopyContinuedUpdate AndBy(bool condition, object whereObj);
    #endregion

    #region Or
    /// <summary>
    /// 条件更新，如：.OrBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    IBulkCopyContinuedUpdate OrBy(object whereObj);
    /// <summary>
    /// 条件更新，如：.OrBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    IBulkCopyContinuedUpdate OrBy(bool condition, object whereObj);
    #endregion
}

/// <summary>
/// 更新数据
/// </summary>
/// <typeparam name="TEntity">要更新的实体类型</typeparam>
public interface IUpdate<TEntity> : IUpdate
{
    #region Sharding
    /// <summary>
    /// 直接指定当前表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回更新对象</returns>
    new IUpdate<TEntity> UseTable(params string[] tableNames);
    /// <summary>
    /// 手动指定分表规则参数值，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回更新对象</returns>
    new IUpdate<TEntity> UseTableBy(params object[] fieldValues);
    //// <summary>
    /// 手动指定分表名获取委托，执行委托获取分表名，更新对象的值自动更新对应分表中。参数是更新对象的值，返回值是分表名，如：.UseTable(insertObj =&gt; $"sys_user_{((User)insertObj).CreatedAt:yyyyMM}")
    /// </summary>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <returns>返回插入对象</returns>
    new IUpdate<TEntity> UseTable(Func<object, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TEntity"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回更新对象</returns>
    new IUpdate<TEntity> UseTableByRange(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回更新对象</returns>
    new IUpdate<TEntity> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回更新对象</returns>
    new IUpdate<TEntity> WithTableAliasTrailing(string rawSql);
    #endregion

    #region Set
    /// <summary>
    /// 部分字段更新，updateObj对象中除主键、OnlyFields、IgnoreFields方法筛选后的剩下所有字段都将参与更新，匿名、命名对象都可以，需要配合where条件使用，如：
    /// <code>.Set(new { Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id = 1); .Set(new User { Name = "kevin", SourceType = null });
    /// SQL: SET `Name`=@Name,SourceType=@SourceType WHERE `Id`=@kId</code>
    /// </summary>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> Set(object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中除主键、OnlyFields、IgnoreFields方法筛选后的剩下所有字段都将参与更新，匿名、命名对象都可以，需要配合where条件使用，为false不更新，如：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id = 1); .Set(true, new User { Name = "kevin", SourceType = null });
    /// SQL: SET `Name`=@Name,SourceType=@SourceType WHERE `Id`=@kId</code>
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> Set(bool condition, object updateObj);
    /// <summary>
    /// 固定值单字段更新，如：<code>.Set("OrderNo", "ON_111")</code>
    /// </summary>
    /// <param name="fieldName">字段名称</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> Set(string fieldName, object fieldValue);
    /// <summary>
    /// 固定值单字段更新，如：<code>.Set(true, "OrderNo", "ON_111")</code>
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldName">字段名称</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> Set(bool condition, string fieldName, object fieldValue);

    /// <summary>
    /// 部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个字段，支持New、MemberInit表达式访问，如：
    /// <code>.Set(f => new { Description = DBNull.Value, Price = f.Price + 50 ... }) 或.Set(f => new Product { Description = DBNull.Value, Price = f.Price + 50 ... })
    /// SQL: UPDATE `sys_product` SET `Description`=NULL,`Price`=Price+50,`Field`=@Field ...</code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段省略赋值字段，如：parameter.TotalAmount</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，支持New、MemberInit表达式访问，如果为false，则不生成更新语句，如：
    /// <code>.Set(true, f => new { Description = DBNull.Value, Price = f.Price + 50 ... }) 或.Set(true, f => new Product { Description = DBNull.Value, Price = f.Price + 50 ... })
    /// SQL: UPDATE `sys_product` SET `Description`=NULL,`Price`=Price+50,`Field`=@Field ...</code>
    /// </summary>
    /// <typeparam name="TFields">一个或是多个字段</typeparam>
    /// <param name="condition">更新条件</param>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 单字段更新，使用固定值fieldValue进行单字段更新，如：<code>.Set(x =&gt; x.OrderNo, "ON_111")</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用固定值fieldValue进行单字段更新，否则不生成更新语句，如：
    /// <code>.Set(true, x =&gt; x.OrderNo, "ON_111")</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion

    #region SetFrom
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
    IBulkContinuedUpdate<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
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
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldSelector">字段筛选表达式，只能筛选一个字段</param>
    /// <param name="valueSelector">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如：
    /// <code>
    /// var orderInfo = new { ... };
    /// .SetFrom((x, y) =&gt; new
    /// {
    ///     orderInfo.Disputes, //直接赋值，使用同名变量，实体对象由TypeHandler处理
    ///     TotalAmount = x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .SelectAggregate((x, t) =&gt; x.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001",
    ///     BuyerSource = DBNull.Value,
    /// })
    /// SQL: SET a.`Disputes`=@Disputes,a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerSource`=NULL ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，如：
    /// <code>
    /// var orderInfo = new { ... };
    /// .SetFrom(true, (x, y) =&gt; new
    /// {
    ///     orderInfo.Disputes, //直接赋值，使用同名变量，实体对象由TypeHandler处理
    ///     TotalAmount = x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .SelectAggregate((x, t) =&gt; x.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001",
    ///     BuyerSource = DBNull.Value,
    /// })
    /// SQL: SET a.`Disputes`=@Disputes,`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerSource`=NULL ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldsAssignment">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
    #endregion

    #region SetBulk
    /// <summary>
    /// 使用集合对象updateObjs部分字段批量更新，根据主键字段更新，updateObjs对象中除主键、OnlyFields、IgnoreFields方法筛选后的剩下所有字段都将参与更新。支持分批次更新，更新条数超过设置的bulkCount值，将在下次更新，直到所有数据更新完毕，bulkCount默认500，
    /// 可以继续使用Set、OnlyFields、IgnoreFields等方法，如：
    /// <code>
    /// var updateObjs = new Order[]{ ... ...};
    /// .SetBulk(updateObjs).Set(new { Name = "kevin"}).IgnoreFields(...) ...
    /// SQL: UPDATE `sys_order` SET ...Name=@Name0 WHERE `Id`=@kId0;UPDATE `sys_order` SET ...Name=@Name1 WHERE `Id`=@kId1; ...
    /// </code>
    /// </summary>
    /// <param name="updateObjs">更新对象参数集合，包含更新字段和where条件字段</param>
    /// <param name="bulkCount">单次更新的最大数据条数，默认是500</param>
    /// <returns>返回更新对象</returns>
    new IBulkContinuedUpdate<TEntity> SetBulk(IEnumerable updateObjs, int bulkCount = 500);
    #endregion

    #region InnerJoin
    /// <summary>
    /// InnerJoin内连接表TSource部分字段数据，更新当前表TEntity数据
    /// </summary>
    /// <typeparam name="TSource">数据来源表TSource实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新Join对象</returns>
    IUpdateJoin<TEntity, TSource> InnerJoin<TSource>(Expression<Func<TEntity, TSource, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// LeftJoin左连接表TSource部分字段数据，更新当前表TEntity数据
    /// </summary>
    /// <typeparam name="TSource">数据来源表TSource实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新Join对象</returns>
    IUpdateJoin<TEntity, TSource> LeftJoin<TSource>(Expression<Func<TEntity, TSource, bool>> joinOn);
    #endregion
}
/// <summary>
/// 更新数据
/// </summary>
/// <typeparam name="TEntity">要更新的实体类型</typeparam>
public interface IContinuedUpdate<TEntity> : IContinuedUpdate
{
    #region Set
    /// <summary>
    /// 固定值单字段更新，如：<code>.Set("OrderNo", "ON_111")</code>
    /// </summary>
    /// <param name="fieldName">字段名称</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> Set(string fieldName, object fieldValue);
    /// <summary>
    /// 固定值单字段更新，如：<code>.Set(true, "OrderNo", "ON_111")</code>
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldName">字段名称</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> Set(bool condition, string fieldName, object fieldValue);

    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如：
    /// <code>.Set(x =&gt; x.OrderNo, "ON_111")</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，否则不生成更新语句，如：
    /// <code>.Set(true, x =&gt; x.OrderNo, "ON_111")</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion

    #region SetFrom
    /// <summary>
    /// 使用子查询fieldsAssignment表达式捞取值部分栏位更新，表达式fieldsAssignment捞取的字段可以是一个或是多个，如：
    /// <code>
    /// var orderInfo = new { ... };
    /// .SetFrom((x, y) =&gt; new
    /// {
    ///     orderInfo.Disputes, //直接赋值，使用同名变量，实体对象由TypeHandler处理
    ///     TotalAmount = x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .SelectAggregate((x, t) =&gt; x.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001",
    ///     BuyerSource = DBNull.Value,
    /// })
    /// SQL: SET a.`Disputes`=@Disputes,a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerSource`=NULL ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="fieldsAssignment">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用带有子查询的表达式fieldsExpr更新部分栏位TFields，表达式fieldsExpr的字段可以是一个或是多个，如果为false，则不生成更新语句，如：
    /// <code>
    /// var orderInfo = new { ... };
    /// .SetFrom(true, (x, y) =&gt; new
    /// {
    ///     orderInfo.Disputes, //直接赋值，使用同名变量，实体对象由TypeHandler处理
    ///     TotalAmount = x.From&lt;OrderDetail&gt;('c')
    ///         .Where(f =&gt; f.OrderId == y.Id)
    ///         .SelectAggregate((x, t) =&gt; x.Sum(t.Amount)) //子查询
    ///     OrderNo = "ON-001",
    ///     BuyerSource = DBNull.Value,
    /// })
    /// SQL: SET a.`Disputes`=@Disputes,`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`='ON-001',a.`BuyerSource`=NULL ...
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">子查询返回的字段类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldsAssignment">子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
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
    IContinuedUpdate<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
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
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldSelector">字段筛选表达式，只能筛选一个字段</param>
    /// <param name="valueSelector">获取单个字段值的子查询表达式</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
    #endregion

    #region IgnoreFields
    /// <summary>
    /// 不更新指定字段，如：.IgnoreFields("Id","Age")
    /// </summary>
    /// <param name="fieldNames">忽略更新的字段列表</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> IgnoreFields(params string[] fieldNames);
    /// <summary>
    /// 不更新指定字段，如：.OnlyFields("Name","Gender")
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="fieldsSelector">字段选择表达式，支持MemberAccess、New或MemberInit类型表达式</param>
    /// <returns></returns>
    IContinuedUpdate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion

    #region OnlyFields
    /// <summary>
    /// 只更新指定字段，如：.OnlyFields("Name","Gender")
    /// </summary>
    /// <param name="fieldNames">忽略更新的字段列表</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> OnlyFields(params string[] fieldNames);
    /// <summary>
    /// 只更新指定字段，如：.OnlyFields(f =&gt; new { f.Name, f.Gender})
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="fieldsSelector">字段选择表达式，支持MemberAccess、New或MemberInit类型表达式</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion

    #region Where
    /// <summary>
    /// 条件更新，如：.WhereBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> WhereBy(object whereObj);
    /// <summary>
    /// 条件更新，如：.WhereBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    new IContinuedUpdate<TEntity> WhereBy(bool condition, object whereObj);
    /// <summary>
    /// 主键条件更新，如：.WhereById(1) 或是 .WhereById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> WhereById(object whereKey);
    /// <summary>
    /// 主键条件更新，如：.WhereById(1) 或是 .WhereById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> WhereById(bool condition, object whereKey);
    /// <summary>
    /// 多主键条件查询，如：.WhereByIds(new int[]{1,2,3}) 或是 .WhereByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> WhereByIds(IEnumerable whereKeys);
    /// <summary>
    /// 多主键条件更新，如：.WhereByIds(new int[]{1,2,3}) 或是 .WhereByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> WhereByIds(bool condition, IEnumerable whereKeys);

    /// <summary>
    /// 条件更新，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 条件更新，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 条件更新，构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件更新，并与已有的条件AND操作，如：.AndBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> AndBy(object whereObj);
    /// <summary>
    /// 条件更新，并与已有的条件AND操作，如：.AndBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    new IContinuedUpdate<TEntity> AndBy(bool condition, object whereObj);
    /// <summary>
    /// 主键条件更新，并与已有的条件AND操作，如：.AndById(1) 或是 .AndById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> AndById(object whereKey);
    /// <summary>
    /// 主键条件更新，并与已有的条件AND操作，如：.AndById(1) 或是 .AndById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> AndById(bool condition, object whereKey);
    /// <summary>
    /// 多主键条件更新，并与已有的条件AND操作，如：.AndByIds(new int[]{1,2,3}) 或是 .AndByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> AndByIds(IEnumerable whereKeys);
    /// <summary>
    /// 多主键条件更新，并与已有的条件AND操作，如：.AndByIds(new int[]{1,2,3}) 或是 .AndByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> AndByIds(bool condition, IEnumerable whereKeys);
    /// <summary>
    /// 条件更新，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> And(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 条件更新，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 条件更新，构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 条件更新，并与已有的条件OR操作，如：.OrBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> OrBy(object whereObj);
    /// <summary>
    /// 条件更新，并与已有的条件OR操作，如：.OrBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    new IContinuedUpdate<TEntity> OrBy(bool condition, object whereObj);
    /// <summary>
    /// 主键条件更新，并与已有的条件OR操作，如：.OrById(1) 或是 .OrById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> OrById(object whereKey);
    /// <summary>
    /// 主键条件更新，并与已有的条件OR操作，如：.OrById(1) 或是 .OrById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> OrById(bool condition, object whereKey);
    /// <summary>
    /// 多主键条件更新，并与已有的条件OR操作，如：.OrByIds(new int[]{1,2,3}) 或是 .OrByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> OrByIds(IEnumerable whereKeys);
    /// <summary>
    /// 多主键条件更新，并与已有的条件OR操作，如：.OrByIds(new int[]{1,2,3}) 或是 .OrByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回更新对象</returns>
    new IContinuedUpdate<TEntity> OrByIds(bool condition, IEnumerable whereKeys);
    /// <summary>
    /// 条件更新，并与已有的条件OR操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> Or(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 条件更新，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> Or(bool condition, Expression<Func<TEntity, bool>> ifPredicate = null, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 条件更新，构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IContinuedUpdate<TEntity> OrPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion
}
/// <summary>
/// 更新数据
/// </summary>
/// <typeparam name="TEntity">要更新的实体类型</typeparam>
public interface IBulkContinuedUpdate<TEntity> : IBulkContinuedUpdate
{
    #region Set
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，如：
    /// <code>.Set(new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id); .Set(new User { Id = 2, Name = "kevin", SourceType = null }).Where(f =&gt; f.Id);  
    /// SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId </code>
    /// </summary>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    new IBulkContinuedUpdate<TEntity> Set(object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，为false不做更新，如：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id);  SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId
    /// .Set(true, new User { Id = 1, ... })  SQL: SET ... //只更新部分字段，可以使用OnlyFields方法，忽略部分字段，可以使用IgnoreFields方法</code>
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    new IBulkContinuedUpdate<TEntity> Set(bool condition, object updateObj);
    /// <summary>
    /// 固定值单字段更新，如：<code>.Set("OrderNo", "ON_111")</code>
    /// </summary>
    /// <param name="fieldName">字段名称</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    new IBulkContinuedUpdate<TEntity> Set(string fieldName, object fieldValue);
    /// <summary>
    /// 固定值单字段更新，如：<code>.Set(true, "OrderNo", "ON_111")</code>
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldName">字段名称</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    new IBulkContinuedUpdate<TEntity> Set(bool condition, string fieldName, object fieldValue);

    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个字段，如：
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
    IBulkContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如果为false，则不生成更新语句，如：
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
    IBulkContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，如：
    /// <code>.Set(x =&gt; x.OrderNo, "ON_111")</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式fieldSelector筛选单个字段，使用固定值fieldValue进行单字段更新，否则不生成更新语句，如：
    /// <code>.Set(true, x =&gt; x.OrderNo, "ON_111")</code>
    /// </summary>
    /// <typeparam name="TField">更新字段类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldSelector">筛选单个字段表达式，只能筛选一个字段</param>
    /// <param name="fieldValue">字段值，固定值</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion

    #region IgnoreFields
    /// <summary>
    /// 不更新指定字段，如：.IgnoreFields("Id","Age")
    /// </summary>
    /// <param name="fieldNames">忽略更新的字段列表</param>
    /// <returns>返回更新对象</returns>
    new IBulkContinuedUpdate<TEntity> IgnoreFields(params string[] fieldNames);
    /// <summary>
    /// 不更新指定字段，如：.OnlyFields("Name","Gender"); .IgnoreFields(f =&gt; f.Id);
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="fieldsSelector">字段选择表达式，支持MemberAccess、New或MemberInit类型表达式</param>
    /// <returns></returns>
    IBulkContinuedUpdate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion

    #region OnlyFields
    /// <summary>
    /// 只更新指定字段，如：.OnlyFields("Name","Gender")
    /// </summary>
    /// <param name="fieldNames">忽略更新的字段列表</param>
    /// <returns>返回更新对象</returns>
    new IBulkContinuedUpdate<TEntity> OnlyFields(params string[] fieldNames);
    /// <summary>
    /// 只更新指定字段，如：.OnlyFields(f =&gt; new { f.Name, f.Gender}); .OnlyFields(f =&gt; f.Name);
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="fieldsSelector">字段选择表达式，支持MemberAccess、New或MemberInit类型表达式</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion

    #region Where
    /// <summary>
    /// 条件更新，如：.WhereBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    new IBulkContinuedUpdate<TEntity> WhereBy(object whereObj);
    /// <summary>
    /// 条件更新，如：.WhereBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    new IBulkContinuedUpdate<TEntity> WhereBy(bool condition, object whereObj);

    /// <summary>
    /// 条件更新，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 条件更新，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 条件更新，构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件更新，并与已有的条件AND操作，如：.AndBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    new IBulkContinuedUpdate<TEntity> AndBy(object whereObj);
    /// <summary>
    /// 条件更新，并与已有的条件AND操作，如：.AndBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    new IBulkContinuedUpdate<TEntity> AndBy(bool condition, object whereObj);

    /// <summary>
    /// 条件更新，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate<TEntity> And(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 条件更新，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 条件更新，构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 条件更新，并与已有的条件OR操作，如：.OrBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    new IBulkContinuedUpdate<TEntity> OrBy(object whereObj);
    /// <summary>
    /// 条件更新，并与已有的条件OR操作，如：.OrBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    new IBulkContinuedUpdate<TEntity> OrBy(bool condition, object whereObj);

    /// <summary>
    /// 条件更新，并与已有的条件OR操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate<TEntity> Or(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 条件更新，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate<TEntity> Or(bool condition, Expression<Func<TEntity, bool>> ifPredicate = null, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 条件更新，构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IBulkContinuedUpdate<TEntity> OrPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion
}
/// <summary>
/// 更新数据
/// </summary>
/// <typeparam name="TEntity">要更新的实体类型</typeparam>
public interface IBulkCopyContinuedUpdate<TEntity> : IBulkCopyContinuedUpdate
{
    #region Where
    /// <summary>
    /// 条件更新，如：.WhereBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    new IBulkCopyContinuedUpdate<TEntity> WhereBy(object whereObj);
    /// <summary>
    /// 条件更新，如：.WhereBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    new IBulkCopyContinuedUpdate<TEntity> WhereBy(bool condition, object whereObj);
    /// <summary>
    /// 条件更新，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IBulkCopyContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 条件更新，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IBulkCopyContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 条件更新，构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IBulkCopyContinuedUpdate<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件更新，如：.AndBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    new IBulkCopyContinuedUpdate<TEntity> AndBy(object whereObj);
    /// <summary>
    /// 条件更新，如：.AndBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    new IBulkCopyContinuedUpdate<TEntity> AndBy(bool condition, object whereObj);
    /// <summary>
    /// 条件更新，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IBulkCopyContinuedUpdate<TEntity> And(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 条件更新，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IBulkCopyContinuedUpdate<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 条件更新，构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IBulkCopyContinuedUpdate<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 条件更新，如：.OrBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回更新对象</returns>
    new IBulkCopyContinuedUpdate<TEntity> OrBy(object whereObj);
    /// <summary>
    /// 条件更新，如：.OrBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    new IBulkCopyContinuedUpdate<TEntity> OrBy(bool condition, object whereObj);
    /// <summary>
    /// 条件更新，并与已有的条件OR操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IBulkCopyContinuedUpdate<TEntity> Or(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 条件更新，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回更新对象</returns>
    IBulkCopyContinuedUpdate<TEntity> Or(bool condition, Expression<Func<TEntity, bool>> ifPredicate = null, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 条件更新，构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回更新对象</returns>
    IBulkCopyContinuedUpdate<TEntity> OrPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion
}
/// <summary>
/// 使用表T1部分字段数据，更新当前表TEntity数据
/// </summary>
/// <typeparam name="TEntity">要更新数据表TEntity实体类型</typeparam>
/// <typeparam name="T1">更新值来源表T1实体类型</typeparam>
public interface IUpdateJoin<TEntity, T1> : IUpdated
{
    #region Sharding
    /// <summary>
    /// 直接指定当前表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> UseTable(params string[] tableNames);
    /// <summary>
    /// 手动指定分表规则参数值，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表名获取委托，执行委托获取分表名，更新对象的值自动更新对应分表中。参数是更新对象的值，返回值是分表名，如：.UseTable(insertObj =&gt; $"sys_user_{((User)insertObj).CreatedAt:yyyyMM}")
    /// </summary>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <returns>返回插入对象</returns>
    IUpdateJoin<TEntity, T1> UseTable(Func<object, string> tableNameGetter);
    /// <summary>
    /// 根据更新实体<typeparamref name="TEntity"/>表与当前<typeparamref name="T1"/>表名的映射关系，指定当前<typeparamref name="T1"/>表分表名获取委托，执行委托获取分表名。委托第一个参数是首个分表原始表名，第二个参数是当前<typeparamref name="TEntity"/>表原始表名，第三个参数是首个分表当前分表名，返回值是当前<typeparamref name="T1"/>表分表名称，如：
    /// <code>
    /// .Update&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// }) ...
    /// SQL:
    /// UPDATE `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...;UPDATE `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter"><typeparamref name="T1"/>表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IUpdateJoin<TEntity, T1> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TEntity"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> UseTableByRange(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> WithTableAliasTrailing(string rawSql);
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
    /// <param name="condition">判断条件，为true时生效</param>
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
    /// <param name="condition">判断条件，为true时生效</param>
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
    /// <param name="condition">判断条件，为true时生效</param>
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
    /// <param name="condition">判断条件，为true时生效</param>
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
public interface IUpdateJoin<TEntity, T1, T2> : IUpdated
{
    #region Sharding
    /// <summary>
    /// 直接指定当前表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> UseTable(params string[] tableNames);
    /// <summary>
    /// 手动指定分表规则参数值，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表名获取委托，执行委托获取分表名，更新对象的值自动更新对应分表中。参数是更新对象的值，返回值是分表名，如：.UseTable(insertObj =&gt; $"sys_user_{((User)insertObj).CreatedAt:yyyyMM}")
    /// </summary>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <returns>返回插入对象</returns>
    IUpdateJoin<TEntity, T1, T2> UseTable(Func<object, string> tableNameGetter);
    /// <summary>
    /// 根据更新实体<typeparamref name="TEntity"/>表与当前<typeparamref name="T2"/>表名的映射关系，指定当前<typeparamref name="T2"/>表分表名获取委托，执行委托获取分表名。委托第一个参数是首个分表原始表名，第二个参数是当前<typeparamref name="TEntity"/>表原始表名，第三个参数是首个分表当前分表名，返回值是当前<typeparamref name="T2"/>表分表名称，如：
    /// <code>
    /// .Update&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// }) ...
    /// SQL:
    /// UPDATE `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...;UPDATE `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter"><typeparamref name="T2"/>表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IUpdateJoin<TEntity, T1, T2> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TEntity"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> UseTableByRange(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> WithTableAliasTrailing(string rawSql);
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
    /// <param name="condition">判断条件，为true时生效</param>
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
    /// <param name="condition">判断条件，为true时生效</param>
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
    /// <param name="condition">判断条件，为true时生效</param>
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
    /// <param name="condition">判断条件，为true时生效</param>
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
public interface IUpdateJoin<TEntity, T1, T2, T3> : IUpdated
{
    #region Sharding
    /// <summary>
    /// 直接指定当前表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> UseTable(params string[] tableNames);
    /// <summary>
    /// 手动指定分表规则参数值，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表名获取委托，执行委托获取分表名，更新对象的值自动更新对应分表中。参数是更新对象的值，返回值是分表名，如：.UseTable(insertObj =&gt; $"sys_user_{((User)insertObj).CreatedAt:yyyyMM}")
    /// </summary>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <returns>返回插入对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> UseTable(Func<object, string> tableNameGetter);
    /// <summary>
    /// 根据更新实体<typeparamref name="TEntity"/>表与当前<typeparamref name="T3"/>表名的映射关系，指定当前<typeparamref name="T3"/>表分表名获取委托，执行委托获取分表名。委托第一个参数是首个分表原始表名，第二个参数是当前<typeparamref name="TEntity"/>表原始表名，第三个参数是首个分表当前分表名，返回值是当前<typeparamref name="T3"/>表分表名称，如：
    /// <code>
    /// .Update&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// }) ...
    /// SQL:
    /// UPDATE `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...;UPDATE `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter"><typeparamref name="T3"/>表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TEntity"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> UseTableByRange(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> WithTableAliasTrailing(string rawSql);
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
    /// <param name="condition">判断条件，为true时生效</param>
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
    /// <param name="condition">判断条件，为true时生效</param>
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
    /// <param name="condition">判断条件，为true时生效</param>
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
    /// <param name="condition">判断条件，为true时生效</param>
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
public interface IUpdateJoin<TEntity, T1, T2, T3, T4> : IUpdated
{
    #region Sharding
    /// <summary>
    /// 直接指定当前表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> UseTable(params string[] tableNames);
    /// <summary>
    /// 手动指定分表规则参数值，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表名获取委托，执行委托获取分表名，更新对象的值自动更新对应分表中。参数是更新对象的值，返回值是分表名，如：.UseTable(insertObj =&gt; $"sys_user_{((User)insertObj).CreatedAt:yyyyMM}")
    /// </summary>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <returns>返回插入对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> UseTable(Func<object, string> tableNameGetter);
    /// <summary>
    /// 根据更新实体<typeparamref name="TEntity"/>表与当前<typeparamref name="T4"/>表名的映射关系，指定当前<typeparamref name="T4"/>表分表名获取委托，执行委托获取分表名。委托第一个参数是首个分表原始表名，第二个参数是当前<typeparamref name="TEntity"/>表原始表名，第三个参数是首个分表当前分表名，返回值是当前<typeparamref name="T4"/>表分表名称，如：
    /// <code>
    /// .Update&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// }) ...
    /// SQL:
    /// UPDATE `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...;UPDATE `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter"><typeparamref name="T4"/>表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TEntity"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> UseTableByRange(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> WithTableAliasTrailing(string rawSql);
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
    /// <param name="condition">判断条件，为true时生效</param>
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
    /// <param name="condition">判断条件，为true时生效</param>
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
    /// <param name="condition">判断条件，为true时生效</param>
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
    /// <param name="condition">判断条件，为true时生效</param>
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
public interface IUpdateJoin<TEntity, T1, T2, T3, T4, T5> : IUpdated
{
    #region Sharding
    /// <summary>
    /// 直接指定当前表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTable(params string[] tableNames);
    /// <summary>
    /// 手动指定分表规则参数值，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表名获取委托，执行委托获取分表名，更新对象的值自动更新对应分表中。参数是更新对象的值，返回值是分表名，如：.UseTable(insertObj =&gt; $"sys_user_{((User)insertObj).CreatedAt:yyyyMM}")
    /// </summary>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <returns>返回插入对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTable(Func<object, string> tableNameGetter);
    /// <summary>
    /// 根据更新实体<typeparamref name="TEntity"/>表与当前<typeparamref name="T5"/>表名的映射关系，指定当前<typeparamref name="T5"/>表分表名获取委托，执行委托获取分表名。委托第一个参数是首个分表原始表名，第二个参数是当前<typeparamref name="TEntity"/>表原始表名，第三个参数是首个分表当前分表名，返回值是当前<typeparamref name="T5"/>表分表名称，如：
    /// <code>
    /// .Update&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// }) ...
    /// SQL:
    /// UPDATE `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...;UPDATE `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter"><typeparamref name="T5"/>表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TEntity"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTableByRange(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> WithTableAliasTrailing(string rawSql);
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
    /// <param name="condition">判断条件，为true时生效</param>
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
    /// <param name="condition">判断条件，为true时生效</param>
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
    /// <param name="condition">判断条件，为true时生效</param>
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
    /// <param name="condition">判断条件，为true时生效</param>
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