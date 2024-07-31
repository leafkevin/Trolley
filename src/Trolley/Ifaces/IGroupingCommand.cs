﻿using System;
using System.Linq.Expressions;

namespace Trolley;

// <summary>
/// 分组查询对象
/// </summary>
/// <typeparam name="TGrouping">分组后的对象类型</typeparam>
public interface IGroupingCommandBase<TGrouping>
{
    /// <summary>
    /// 使用分组后对象直接返回
    /// </summary>
    /// <returns>返回分组后对象</returns>
    IFromCommand<TGrouping> Select();
    /// <summary>
    /// 使用原始字段返回查询结果，用法：Select&lt;Order&gt;("*") 或是 Select&lt;int&gt;("1")
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fields">原始字段字符串，默认值*</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(string fields = "*");
}
/// <summary>
/// 分组查询对象
/// </summary>
/// <typeparam name="T">原始表类型</typeparam>
/// <typeparam name="TGrouping">分组后对象类型</typeparam>
public interface IGroupingCommand<T, TGrouping> : IGroupingCommandBase<TGrouping>
{
    /// <summary>
    /// Having操作，如: .Having((x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500, Sql.Sum(a.Amount) > 500，两者等效</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，执行Having操作，否则不执行Having操作
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500, Sql.Sum(a.Amount) > 500，两者等效</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，用法：
    /// OrderBy(x =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 OrderBy(x =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, f =&gt; new { f.Id, f.OtherId }) 或是 OrderBy(true, x =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，用法：
    /// OrderByDescending(x =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 OrderByDescending(x =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, f =&gt; new { f.Id, f.OtherId }) 或是 OrderByDescending(true, x =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select(x =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select(x =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T, TTarget>> fieldsExpr);
}
/// <summary>
/// 分组查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="TGrouping">分组后对象类型</typeparam>
public interface IGroupingCommand<T1, T2, TGrouping> : IGroupingCommandBase<TGrouping>
{
    /// <summary>
    /// Having操作，如: .Having((x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500, Sql.Sum(a.Amount) > 500，两者等效</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，执行Having操作，否则不执行Having操作
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500, Sql.Sum(a.Amount) > 500，两者等效</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TTarget>> fieldsExpr);
}
/// <summary>
/// 分组查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="TGrouping">分组后对象类型</typeparam>
public interface IGroupingCommand<T1, T2, T3, TGrouping> : IGroupingCommandBase<TGrouping>
{
    /// <summary>
    /// Having操作，如: .Having((x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500, Sql.Sum(a.Amount) > 500，两者等效</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, T3, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，执行Having操作，否则不执行Having操作
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500, Sql.Sum(a.Amount) > 500，两者等效</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, T3, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, T3, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, T3, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, T3, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, T3, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TTarget>> fieldsExpr);
}
/// <summary>
/// 分组查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="TGrouping">分组后对象类型</typeparam>
public interface IGroupingCommand<T1, T2, T3, T4, TGrouping> : IGroupingCommandBase<TGrouping>
{
    /// <summary>
    /// Having操作，如: .Having((x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500, Sql.Sum(a.Amount) > 500，两者等效</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，执行Having操作，否则不执行Having操作
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500, Sql.Sum(a.Amount) > 500，两者等效</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TTarget>> fieldsExpr);
}
/// <summary>
/// 分组查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="TGrouping">分组后对象类型</typeparam>
public interface IGroupingCommand<T1, T2, T3, T4, T5, TGrouping> : IGroupingCommandBase<TGrouping>
{
    /// <summary>
    /// Having操作，如: .Having((x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500, Sql.Sum(a.Amount) > 500，两者等效</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, T5, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，执行Having操作，否则不执行Having操作
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500, Sql.Sum(a.Amount) > 500，两者等效</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, T5, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr);
}
/// <summary>
/// 分组查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="TGrouping">分组后对象类型</typeparam>
public interface IGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> : IGroupingCommandBase<TGrouping>
{
    /// <summary>
    /// Having操作，如: .Having((x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500, Sql.Sum(a.Amount) > 500，两者等效</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，执行Having操作，否则不执行Having操作
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500, Sql.Sum(a.Amount) > 500，两者等效</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，用法：
    /// OrderBy((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 OrderBy((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，用法：
    /// OrderByDescending((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 OrderByDescending((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);
}