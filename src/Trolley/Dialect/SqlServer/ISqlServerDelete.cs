using System;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

/// <summary>
/// 删除数据
/// </summary>
/// <typeparam name="TEntity">要删除的实体类型</typeparam>
public interface ISqlServerDelete<TEntity> : IDelete<TEntity>
{
    #region Sharding
    /// <summary>
    /// 手动指定分表名，如：.UseTable("sys_order_202001")
    /// </summary>
    /// <param name="tableName">完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回删除对象</returns>
    new ISqlServerDelete<TEntity> UseTable(string tableName);
    /// <summary>
    /// 手动指定分表规则参数值，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回删除对象</returns>
    new ISqlServerDelete<TEntity> UseTableBy(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回删除对象</returns>
    new ISqlServerDelete<TEntity> UseTableSchema(string tableSchema);
    #endregion

    #region Where
    /// <summary>
    /// 主键删除，单条也可多条，keys可以是主键值也可以是包含主键值的匿名对象，也可以是对应的集合，如：
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
    new ISqlServerDelete<TEntity> Where(object keys);
    /// <summary>
    /// 条件删除，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回删除对象</returns>
    new ISqlServerDelete<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 条件删除，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回删除对象</returns>
    new ISqlServerDelete<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 条件删除，构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回删除对象</returns>
    new ISqlServerDelete<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件删除，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回删除对象</returns>
    new ISqlServerDelete<TEntity> And(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 条件删除，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回删除对象</returns>
    new ISqlServerDelete<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 条件删除，构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回删除对象</returns>
    new ISqlServerDelete<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 条件删除，并与已有的条件OR操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回删除对象</returns>
    new ISqlServerDelete<TEntity> Or(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 条件删除，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回删除对象</returns>
    new ISqlServerDelete<TEntity> Or(bool condition, Expression<Func<TEntity, bool>> ifPredicate = null, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 条件删除，构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回删除对象</returns>
    new ISqlServerDelete<TEntity> OrPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion

    #region Output
    /// <summary>
    /// 返回删除数据，如：.Output("DELETED.[Id]") 或 .Output("DELETED.*")
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldNames">返回字段名称列表, 如果有函数调用、表达式或是常量值需要带有AS子句</param>
    /// <returns>返回已删除的选择字段值</returns>
    ISqlServerDeleted<TEntity, TResult> Output<TResult>(string fieldNames);
    /// <summary>
    /// 返回删除数据
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldsSelector">返回字段名称列表</param>
    /// <returns>返回已删除的选择字段值</returns>
    ISqlServerDeleted<TEntity, TResult> Output<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}