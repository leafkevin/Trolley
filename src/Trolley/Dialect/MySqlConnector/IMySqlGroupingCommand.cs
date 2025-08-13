using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

// <summary>
/// 分组查询对象
/// </summary>
/// <typeparam name="TGrouping">分组后的对象类型</typeparam>
public interface IMySqlGroupingCommandBase<TGrouping> : IGroupingCommandBase<TGrouping>
{
    /// <summary>
    /// 使用分组后对象直接返回
    /// </summary>
    /// <returns>返回分组后对象</returns>
    new IMySqlFromCommand<TGrouping> Select();
}
/// <summary>
/// 分组查询对象
/// </summary>
/// <typeparam name="T">原始表类型</typeparam>
/// <typeparam name="TGrouping">分组后对象类型</typeparam>
public interface IMySqlGroupingCommand<T, TGrouping> : IGroupingCommand<T, TGrouping>, IMySqlGroupingCommandBase<TGrouping>
{
    /// <summary>
    /// Having操作，如: .Having((x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate);
    /// <summary>
    /// Having操作，condition为true时生效，如: .Having(true, (x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, bool>> predicate);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，如：
    /// .OrderBy(x =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 .OrderBy(x =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, f =&gt; new { f.Id, f.OtherId }) 或是 .OrderBy(true, x =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，如：
    /// .OrderByDescending(x =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 .OrderByDescending(x =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, f =&gt; new { f.Id, f.OtherId }) 或是 .OrderByDescending(true, x =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Amount";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("BuyerId", (x, f) =&lt; f.BuyerId).When("Amount", (x, f) =&lt; x.Sum(f.Amount)).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T>, Expression> fieldsGetter);
    /// <summary>
    /// 选择指定字段返回，可以是单个或多个字段的匿名对象，如：
    /// <code> .Select(x =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 .Select(x =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T, TTarget>> fieldsExpr);
}
/// <summary>
/// 分组查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="TGrouping">分组后对象类型</typeparam>
public interface IMySqlGroupingCommand<T1, T2, TGrouping> : IGroupingCommand<T1, T2, TGrouping>, IGroupingCommandBase<TGrouping>
{
    /// <summary>
    /// Having操作，如: .Having((x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate);
    /// <summary>
    /// Having操作，condition为true时生效，如: .Having(true, (x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, bool>> predicate);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，如：
    /// .OrderBy((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 .OrderBy((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，如：
    /// .OrderByDescending((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 .OrderByDescending((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Amount";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("BuyerId", (x, a, b, ...) =&lt; a.BuyerId).When("Amount", (x, a, b, ...) =&lt; x.Sum(a.Amount)).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2>, Expression> fieldsGetter);
    /// <summary>
    /// 选择指定字段返回，可以是单个或多个字段的匿名对象，如：
    /// <code> .Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 .Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, TTarget>> fieldsExpr);
}
/// <summary>
/// 分组查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="TGrouping">分组后对象类型</typeparam>
public interface IMySqlGroupingCommand<T1, T2, T3, TGrouping> : IGroupingCommand<T1, T2, T3, TGrouping>, IGroupingCommandBase<TGrouping>
{
    /// <summary>
    /// Having操作，如: .Having((x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// Having操作，condition为true时生效，如: .Having(true, (x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，如：
    /// .OrderBy((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 .OrderBy((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，如：
    /// .OrderByDescending((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 .OrderByDescending((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Amount";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("BuyerId", (x, a, b, ...) =&lt; a.BuyerId).When("Amount", (x, a, b, ...) =&lt; x.Sum(a.Amount)).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2, T3>, Expression> fieldsGetter);
    /// <summary>
    /// 选择指定字段返回，可以是单个或多个字段的匿名对象，如：
    /// <code> .Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 .Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, TTarget>> fieldsExpr);
}
/// <summary>
/// 分组查询对象
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="TGrouping">分组后对象类型</typeparam>
public interface IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping> : IGroupingCommand<T1, T2, T3, T4, TGrouping>, IGroupingCommandBase<TGrouping>
{
    /// <summary>
    /// Having操作，如: .Having((x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// Having操作，condition为true时生效，如: .Having(true, (x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，如：
    /// .OrderBy((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 .OrderBy((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，如：
    /// .OrderByDescending((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 .OrderByDescending((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Amount";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("BuyerId", (x, a, b, ...) =&lt; a.BuyerId).When("Amount", (x, a, b, ...) =&lt; x.Sum(a.Amount)).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2, T3, T4>, Expression> fieldsGetter);
    /// <summary>
    /// 选择指定字段返回，可以是单个或多个字段的匿名对象，如：
    /// <code> .Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 .Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, TTarget>> fieldsExpr);
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
public interface IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> : IGroupingCommand<T1, T2, T3, T4, T5, TGrouping>, IGroupingCommandBase<TGrouping>
{
    /// <summary>
    /// Having操作，如: .Having((x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// Having操作，condition为true时生效，如: .Having(true, (x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，如：
    /// .OrderBy((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 .OrderBy((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，如：
    /// .OrderByDescending((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 .OrderByDescending((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Amount";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("BuyerId", (x, a, b, ...) =&lt; a.BuyerId).When("Amount", (x, a, b, ...) =&lt; x.Sum(a.Amount)).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, T5, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5>, Expression> fieldsGetter);
    /// <summary>
    /// 选择指定字段返回，可以是单个或多个字段的匿名对象，如：
    /// <code> .Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 .Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, TTarget>> fieldsExpr);
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
public interface IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> : IGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping>, IGroupingCommandBase<TGrouping>
{
    /// <summary>
    /// Having操作，如: .Having((x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> Having(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate);
    /// <summary>
    /// Having操作，condition为true时生效，如: .Having(true, (x, a, ...) => x.Sum(a.Amount) > 500)
    /// </summary>
    /// <param name="condition">判断条件</param>
    /// <param name="predicate">Having条件表达式，如：x.Sum(a.Amount) > 500</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> Having(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, bool>> predicate);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，如：
    /// .OrderBy((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 .OrderBy((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderBy<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，fieldsExpr可以是单个或多个字段的匿名对象，可以使用分组对象Grouping，也可以使用原始表字段，如：
    /// .OrderByDescending((x, a, ...) =&gt; new { x.Grouping.Id, x.Grouping.OrderId }) 或是 .OrderByDescending((x, a, ...) =&gt; x.Grouping.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDescending<TFields>(bool condition, Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Amount";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("BuyerId", (x, a, b, ...) =&lt; a.BuyerId).When("Amount", (x, a, b, ...) =&lt; x.Sum(a.Amount)).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    new IMySqlGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> OrderByDynamic(Func<OrderByBuilder<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6>, Expression> fieldsGetter);
    /// <summary>
    /// 选择指定字段返回，可以是单个或多个字段的匿名对象，如：
    /// <code> .Select((x, a, ...) =&gt; new { x.Grouping, TotalAmount = x.Sum(a.Amount) }) 或是 .Select((x, a, ...) =&gt; a.Id)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回分组查询对象</returns>
    new IMySqlFromCommand<TTarget> Select<TTarget>(Expression<Func<IGroupingAggregate<TGrouping>, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);
}