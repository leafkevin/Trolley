using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

/// <summary>
/// 去重分组对象
/// </summary>
/// <typeparam name="TDistinctOn">去重分组对象类型</typeparam>
public interface IDistinctOnObject<TDistinctOn>
{
    /// <summary>
    /// 去重分组对象字段集合
    /// </summary>
    TDistinctOn DistinctOn { get; set; }
}
/// <summary>
/// 去重分组查询对象
/// </summary>
/// <typeparam name="TDistinctOn">分组去重后的对象类型</typeparam>
public interface IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    /// <summary>
    /// 使用分组去重后对象直接返回
    /// </summary>
    /// <returns>返回分组后对象</returns>
    IPostgreSqlQuery<TDistinctOn> Select();
    /// <summary>
    /// 使用原始字段返回查询结果，用法：Select&lt;Order&gt;("*") 或是 Select&lt;int&gt;("1")
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fields">原始字段字符串，默认值*</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlQuery<TTarget> Select<TTarget>(string fields = "*");
}
/// <summary>
/// 去重分组查询对象
/// </summary>
/// <typeparam name="T">原始表类型</typeparam>
/// <typeparam name="TDistinctOn">分组后对象类型</typeparam>
public interface IPostgreSqlDistinctOnQuery<T, TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderBy(x =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderBy(x =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T, TDistinctOn> OrderBy<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, f =&gt; new { f.Id, f.OtherId }) 或是 OrderBy(true, x =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderByDescending(x =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderByDescending(x =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, f =&gt; new { f.Id, f.OtherId }) 或是 OrderByDescending(true, x =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select(x =&gt; new { x.DistinctOn, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select(x =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IDistinctOnObject<TDistinctOn>, T, TTarget>> fieldsExpr);
    #endregion
}
/// <summary>
/// 分组去重查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="TDistinctOn">分组后对象类型</typeparam>
public interface IPostgreSqlDistinctOnQuery<T1, T2, TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, TDistinctOn> OrderBy<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, TTarget>> fieldsExpr);
    #endregion
}
/// <summary>
/// 分组去重查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="TDistinctOn">分组后对象类型</typeparam>
public interface IPostgreSqlDistinctOnQuery<T1, T2, T3, TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, TDistinctOn> OrderBy<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, TTarget>> fieldsExpr);
    #endregion
}
/// <summary>
/// 分组去重查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="TDistinctOn">分组后对象类型</typeparam>
public interface IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, TDistinctOn> OrderBy<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, TTarget>> fieldsExpr);
    #endregion
}
/// <summary>
/// 分组去重查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="TDistinctOn">分组后对象类型</typeparam>
public interface IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, TDistinctOn> OrderBy<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr);
    #endregion
}
/// <summary>
/// 分组去重查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="TDistinctOn">分组后对象类型</typeparam>
public interface IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, TDistinctOn> OrderBy<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);
    #endregion
}
/// <summary>
/// 分组去重查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="TDistinctOn">分组后对象类型</typeparam>
public interface IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, TDistinctOn> OrderBy<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr);
    #endregion
}
/// <summary>
/// 分组去重查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="TDistinctOn">分组后对象类型</typeparam>
public interface IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, TDistinctOn> OrderBy<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr);
    #endregion
}
/// <summary>
/// 分组去重查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="TDistinctOn">分组后对象类型</typeparam>
public interface IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDistinctOn> OrderBy<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr);
    #endregion
}
/// <summary>
/// 分组去重查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="TDistinctOn">分组后对象类型</typeparam>
public interface IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDistinctOn> OrderBy<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr);
    #endregion
}
/// <summary>
/// 分组去重查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="TDistinctOn">分组后对象类型</typeparam>
public interface IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDistinctOn> OrderBy<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr);
    #endregion
}
/// <summary>
/// 分组去重查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="T12">表T12实体类型</typeparam>
/// <typeparam name="TDistinctOn">分组后对象类型</typeparam>
public interface IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDistinctOn> OrderBy<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr);
    #endregion
}
/// <summary>
/// 分组去重查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="T12">表T12实体类型</typeparam>
/// <typeparam name="T13">表T13实体类型</typeparam>
/// <typeparam name="TDistinctOn">分组后对象类型</typeparam>
public interface IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDistinctOn> OrderBy<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr);
    #endregion
}
/// <summary>
/// 分组去重查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="T12">表T12实体类型</typeparam>
/// <typeparam name="T13">表T13实体类型</typeparam>
/// <typeparam name="T14">表T14实体类型</typeparam>
/// <typeparam name="TDistinctOn">分组后对象类型</typeparam>
public interface IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDistinctOn> OrderBy<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr);
    #endregion
}
/// <summary>
/// 分组去重查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="T12">表T12实体类型</typeparam>
/// <typeparam name="T13">表T13实体类型</typeparam>
/// <typeparam name="T14">表T14实体类型</typeparam>
/// <typeparam name="T15">表T15实体类型</typeparam>
/// <typeparam name="TDistinctOn">分组后对象类型</typeparam>
public interface IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDistinctOn> OrderBy<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDistinctOn> OrderBy<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象DistinctOn，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.DistinctOn.Id, x.DistinctOn.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.DistinctOn.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDistinctOn> OrderByDescending<TFields>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDistinctOn> OrderByDescending<TFields>(bool condition, Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IPostgreSqlQuery<TTarget> Select<TTarget>(Expression<Func<IDistinctOnObject<TDistinctOn>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr);
    #endregion
}
/// <summary>
/// 分组去重查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="T12">表T12实体类型</typeparam>
/// <typeparam name="T13">表T13实体类型</typeparam>
/// <typeparam name="T14">表T14实体类型</typeparam>
/// <typeparam name="T15">表T15实体类型</typeparam>
/// <typeparam name="T16">表T16实体类型</typeparam>
/// <typeparam name="TDistinctOn">分组后对象类型</typeparam>
public interface IPostgreSqlDistinctOnQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TDistinctOn> : IPostgreSqlDistinctOnQueryBase<TDistinctOn>
{
}