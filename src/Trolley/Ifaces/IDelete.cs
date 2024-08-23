using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

/// <summary>
/// 删除数据
/// </summary>
/// <typeparam name="TEntity">要删除的实体类型</typeparam>
public interface IDelete<TEntity>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TEntity表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回删除对象</returns>
    IDelete<TEntity> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TEntity表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回删除对象</returns>
    IDelete<TEntity> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 根据字段值确定TEntity表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致，可多次调用，可多次调用
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回删除对象</returns>
    IDelete<TEntity> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TEntity表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回删除对象</returns>
    IDelete<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TEntity表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回删除对象</returns>
    IDelete<TEntity> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回删除对象</returns>
    IDelete<TEntity> UseTableSchema(string tableSchema);
    #endregion

    /// <summary>
    /// 根据主键删除数据，可以删除一条也可以删除多条记录，keys可以是主键值也可以是包含主键值的匿名对象，用法：
    /// <code>
    /// 单个删除,下面两个方法等效
    /// repository.Delete&lt;User&gt;(1);
    /// repository.Delete&lt;User&gt;(new { Id = 1});
    /// 批量删除,下面两个方法等效
    /// repository.Delete&lt;User&gt;(new[] { 1, 2 });
    /// repository.Delete&lt;User&gt;(new[] { new { Id = 1 }, new { Id = 2 } });
    /// </code>
    /// </summary>
    /// <param name="keys">主键值，可以是一个值或是一个匿名对象，也可以是多个值或是多个匿名对象</param>
    /// <returns>返回删除对象</returns>
    IDeleted<TEntity> Where(object keys);
    /// <summary>
    /// 删除满足表达式predicate条件的数据，不局限于主键条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回删除对象</returns>
    IContinuedDelete<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 删除满足表达式ifPredicate或elsePredicate条件的数据，不局限于主键条件，表达式ifPredicate不能为null。
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回删除对象</returns>
    IContinuedDelete<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
}
/// <summary>
/// 删除数据
/// </summary>
/// <typeparam name="TEntity">要删除的实体类型</typeparam>
public interface IDeleted<TEntity>
{
    /// <summary>
    /// 执行删除操作，并返回删除行数
    /// </summary>
    /// <returns>返回删除行数</returns>
    int Execute();
    /// <summary>
    /// 执行删除操作，并返回删除行数
    /// </summary>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回删除行数</returns>
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    MultipleCommand ToMultipleCommand();
    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 删除数据
/// </summary>
/// <typeparam name="TEntity">要删除的实体类型</typeparam>
public interface IContinuedDelete<TEntity> : IDeleted<TEntity>
{
    /// <summary>
    /// 删除满足表达式predicate条件的数据，不局限于主键条件，predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回删除对象</returns>
    IContinuedDelete<TEntity> And(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 删除满足表达式ifPredicate或elsePredicate条件的数据，不局限于主键条件，表达式ifPredicate不能为null。
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回删除对象</returns>
    IContinuedDelete<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate = null, Expression<Func<TEntity, bool>> elsePredicate = null);
}