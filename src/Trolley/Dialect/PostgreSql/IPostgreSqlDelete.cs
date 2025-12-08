using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

/// <summary>
/// 删除数据
/// </summary>
/// <typeparam name="TEntity">要删除的实体类型</typeparam>
public interface IPostgreSqlDelete<TEntity> : IDelete<TEntity>
{
    #region Sharding
    /// <summary>
    /// 手动指定分表名，如：.UseTable("sys_order_202001")
    /// </summary>
    /// <param name="tableName">完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> UseTable(string tableName);
    /// <summary>
    /// 手动指定分表规则参数值，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> UseTableByRange(params object[] fieldValues);
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
    /// 条件删除，如：.WhereBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> WhereBy(object whereObj);
    /// <summary>
    /// 条件删除，如：.WhereBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    new IPostgreSqlDelete<TEntity> WhereBy(bool condition, object whereObj);
    /// <summary>
    /// 主键条件删除，如：.WhereById(1) 或是 .WhereById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> WhereById(object whereKey);
    /// <summary>
    /// 主键条件删除，如：.WhereById(1) 或是 .WhereById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> WhereById(bool condition, object whereKey);
    /// <summary>
    /// 多主键条件查询，如：.WhereByIds(new int[]{1,2,3}) 或是 .WhereByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> WhereByIds(IEnumerable whereKeys);
    /// <summary>
    /// 多主键条件删除，如：.WhereByIds(new int[]{1,2,3}) 或是 .WhereByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> WhereByIds(bool condition, IEnumerable whereKeys);
    /// <summary>
    /// 条件删除，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 条件删除，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 条件删除，构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件删除，并与已有的条件AND操作，如：.AndBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> AndBy(object whereObj);
    /// <summary>
    /// 条件删除，并与已有的条件AND操作，如：.AndBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    new IPostgreSqlDelete<TEntity> AndBy(bool condition, object whereObj);
    /// <summary>
    /// 主键条件删除，并与已有的条件AND操作，如：.AndById(1) 或是 .AndById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> AndById(object whereKey);
    /// <summary>
    /// 主键条件删除，并与已有的条件AND操作，如：.AndById(1) 或是 .AndById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> AndById(bool condition, object whereKey);
    /// <summary>
    /// 多主键条件删除，并与已有的条件AND操作，如：.AndByIds(new int[]{1,2,3}) 或是 .AndByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> AndByIds(IEnumerable whereKeys);
    /// <summary>
    /// 多主键条件删除，并与已有的条件AND操作，如：.AndByIds(new int[]{1,2,3}) 或是 .AndByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> AndByIds(bool condition, IEnumerable whereKeys);
    /// <summary>
    /// 条件删除，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> And(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 条件删除，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 条件删除，构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 条件删除，并与已有的条件OR操作，如：.OrBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> OrBy(object whereObj);
    /// <summary>
    /// 条件删除，并与已有的条件OR操作，如：.OrBy(new { IsEnabled = true})，whereObj不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，whereObj不能为null</param>
    /// <returns></returns>
    new IPostgreSqlDelete<TEntity> OrBy(bool condition, object whereObj);
    /// <summary>
    /// 主键条件删除，并与已有的条件OR操作，如：.OrById(1) 或是 .OrById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> OrById(object whereKey);
    /// <summary>
    /// 主键条件删除，并与已有的条件OR操作，如：.OrById(1) 或是 .OrById(new { Id = 1})，whereKey不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKey">主键值或是包含主键的对象</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> OrById(bool condition, object whereKey);
    /// <summary>
    /// 多主键条件删除，并与已有的条件OR操作，如：.OrByIds(new int[]{1,2,3}) 或是 .OrByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> OrByIds(IEnumerable whereKeys);
    /// <summary>
    /// 多主键条件删除，并与已有的条件OR操作，如：.OrByIds(new int[]{1,2,3}) 或是 .OrByIds(new []{new { Id = 1}, new { Id = 2}, new { Id = 3} })，whereKeys不能为null
    /// </summary>
    /// <param name="condition">判断条件，为true时条件生效</param>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> OrByIds(bool condition, IEnumerable whereKeys);
    /// <summary>
    /// 条件删除，并与已有的条件OR操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> Or(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 条件删除，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> Or(bool condition, Expression<Func<TEntity, bool>> ifPredicate = null, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 条件删除，构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回删除对象</returns>
    new IPostgreSqlDelete<TEntity> OrPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion

    #region Returning
    /// <summary>
    /// 返回删除数据，如：.Returning("*")
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldNames">返回字段名称列表, 如果有函数调用、表达式或是常量值需要带有AS子句</param>
    /// <returns>返回已删除的选择字段值</returns>
    IPostgreSqlDeleted<TEntity, TResult> Returning<TResult>(string fieldNames);
    /// <summary>
    /// 返回删除数据
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldsSelector">返回字段名称列表</param>
    /// <returns>返回已删除的选择字段值</returns>
    IPostgreSqlDeleted<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}