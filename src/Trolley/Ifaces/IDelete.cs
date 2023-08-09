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
    /// 使用predicate表达式删除数据，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回删除对象</returns>
    IDeleting<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IDeleting<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
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
public interface IDeleting<TEntity> : IDeleted<TEntity>
{
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IDeleting<TEntity> And(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IDeleting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate = null, Expression<Func<TEntity, bool>> elsePredicate = null);
}