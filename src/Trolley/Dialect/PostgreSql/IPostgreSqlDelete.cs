using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public interface IPostgreSqlDelete<TEntity> : IDelete<TEntity>
{
    #region Sharding
    /// <summary>
    /// 直接指定1个或多个TEntity表分表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TEntity表1个或多个分表名，如：.UseTable(f =&gt; f.Contains("202001"))
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定TEntity表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 根据1个字段范围值，手动指定TEntity表分表名，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)，//时间分表，最近一周的订单
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段范围值，手动指定TEntity表分表名，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// .UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)//商户+时间分表，商户1最近一周的订单
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 根据2个固定字段值和1个字段范围值，手动指定TEntity表分表名，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// .UseTableByRange(1, 6, DateTime.Now.AddDays(-7), DateTime.Now)//商户+产品+时间分表，商户1，产品6，最近一周的订单
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3开始起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> UseTableSchema(string tableSchema);
    #endregion

    #region Where
    /// <summary>
    /// 删除满足表达式predicate条件的数据，不局限于主键条件，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlContinuedDelete<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 删除满足表达式ifPredicate或elsePredicate条件的数据，不局限于主键条件，表达式ifPredicate不可为null。
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlContinuedDelete<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlContinuedDelete<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion
}