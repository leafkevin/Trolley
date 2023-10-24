﻿using System;
using System.Linq;
using System.Linq.Expressions;

namespace Trolley;

/// <summary>
/// 分组查询对象
/// </summary>
/// <typeparam name="TGrouping"></typeparam>
public interface IGroupingAggregate<TGrouping> : IAggregateSelect
{
    /// <summary>
    /// 分组对象，快捷的访问分组的所有字段
    /// </summary>
    TGrouping Grouping { get; set; }
}
/// <summary>
/// 分组查询对象
/// </summary>
/// <typeparam name="TGrouping">分组后的对象类型</typeparam>
public interface IGroupingQueryBase<TGrouping>
{
    /// <summary>
    /// 使用原始字段返回匿名查询结果，主要用在不关注结果类型的地方，比如：Sql.Exists语句，用于判断数据是否存在，用法：SelectAnonymous("*") 或是 SelectAnonymous("1")，
    /// </summary>
    /// <param name="fields">原始字段字符串，默认值*</param>
    /// <returns>返回匿名查询对象</returns>
    IQueryAnonymousObject SelectAnonymous(string fields = "*");
    /// <summary>
    /// 使用分组后对象直接返回
    /// </summary>
    /// <returns>返回分组后对象</returns>
    IQuery<TGrouping> Select();    
}
/// <summary>
/// 分组查询对象
/// </summary>
/// <typeparam name="T">原始表类型</typeparam>
/// <typeparam name="TGrouping">分组后对象类型</typeparam>
public interface IGroupingQuery<T, TGrouping> : IGroupingQueryBase<TGrouping>
{
    /// <summary>
    /// Having操作，如: .Having((x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500, Sql.Sum(a.Amount) > 500，两者等效</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingQuery<T, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，执行Having操作，否则不执行Having操作
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500, Sql.Sum(a.Amount) > 500，两者等效</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingQuery<T, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，用法：
    /// OrderBy(x =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 OrderBy(x =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingQuery<T, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，用法：
    /// OrderByDescending(x =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 OrderByDescending(x =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IGroupingQuery<T, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，可以是一个或多个字段的匿名对象，用法：
    /// <code> ...Select(x =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 ...Select(x =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T, TTarget>> fieldsExpr);
}
public interface IGroupingQuery<T1, T2, TGrouping> : IGroupingQueryBase<TGrouping>
{
    IGroupingQuery<T1, T2, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate);
    IGroupingQuery<T1, T2, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate);
    IGroupingQuery<T1, T2, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr);
    IGroupingQuery<T1, T2, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr);
    IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TTarget>> fieldsExpr);
}
public interface IGroupingQuery<T1, T2, T3, TGrouping> : IGroupingQueryBase<TGrouping>
{
    IGroupingQuery<T1, T2, T3, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate);
    IGroupingQuery<T1, T2, T3, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate);
    IGroupingQuery<T1, T2, T3, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr);
    IGroupingQuery<T1, T2, T3, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr);
    IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TTarget>> fieldsExpr);
}
public interface IGroupingQuery<T1, T2, T3, T4, TGrouping>
{
    IGroupingQuery<T1, T2, T3, T4, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr);
    IGroupingQuery<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr);
    IQuery<TGrouping> Select();
    IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TTarget>> fieldsExpr);
}
public interface IGroupingQuery<T1, T2, T3, T4, T5, TGrouping>
{
    IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IQuery<TGrouping> Select();
    IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr);
}
public interface IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>
{
    IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    IQuery<TGrouping> Select();
    IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);
}
public interface IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>
{
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    IQuery<TGrouping> Select();
    IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr);
}
public interface IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>
{
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    IQuery<TGrouping> Select();
    IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr);
}
public interface IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>
{
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    IQuery<TGrouping> Select();
    IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr);
}
public interface IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>
{
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    IQuery<TGrouping> Select();
    IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr);
}
public interface IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>
{
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    IQuery<TGrouping> Select();
    IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr);
}
public interface IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>
{
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    IQuery<TGrouping> Select();
    IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr);
}
public interface IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>
{
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    IQuery<TGrouping> Select();
    IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr);
}
public interface IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>
{
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    IQuery<TGrouping> Select();
    IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr);
}
public interface IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>
{
    IQuery<TGrouping> Select();
    IQuery<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr);
}