using System;
using System.Collections.Generic;

namespace Trolley;

public static class Sql
{
    /// <summary>
    /// 将当前对象转换为指定类型T，只做字段映射解析，不实现
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fields"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static T As<T>(this object fields) => throw new NotImplementedException();
    /// <summary>
    /// 原始SQL，可以做任何代码片段，只能单个字段，不支持实体类型，如：
    /// Sql.Raw&lt;int&gt;("ROW_NUMBER() OVER(ORDER BY e.CREATE_TIME DESC) AS RowNumber")})
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static T Raw<T>(string rawSql) => throw new NotImplementedException();
    /// <summary>
    /// 原始SQL，可以做任何代码片段，支持实体类型，必须指定fieldsCount值，如：
    /// Sql.Raw&lt;int&gt;("ROW_NUMBER() OVER(ORDER BY e.CREATE_TIME DESC) AS RowNumber", 1)})，Sql.Raw&lt;Order&gt;("id,order_no", 2)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="fieldsCount">字段个数</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static T Raw<T>(string rawSql, int fieldsCount) => throw new NotImplementedException();
    public static TField Null<TField>() => throw new NotImplementedException();
    /// <summary>
    /// 用在表达式之后，表示前面的表达式不做sql解析，当引用的字段从数据库读取后，再执行进行解析，如：
    /// f.TotalAmount.ToString("C").Deferred()，DateTimeOffset.FromUnixTimeMilliseconds(f.CreatedAt).UtcDateTime.Deferred()
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static T Deferred<T>(this T obj)
        => throw new NotImplementedException();
    /// <summary>
    /// 当前字段或是表达式是否为NULL，只做条件解析，不实现
    /// <code>x.BuyerId.IsNull()</code>
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    /// <param name="field">字段访问</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static bool IsNull<TField>(this TField field)
        => throw new NotImplementedException();
    /// <summary>
    /// 当栏位field为null值时，取nullVaueExpr的值，可以是常量、变量、或是字段表达式等
    /// <code>x.Max(f.Balance.IsNull(0)),a.Balance.IsNull(b.EndBalance)</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="field">字段表达式</param>
    /// <param name="nullVaueExpr">代替表达式</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static TField IsNull<TField>(this TField field, TField nullVaueExpr)
    {
        if (field.Equals(default(TField)))
            return nullVaueExpr;
        return field;
    }
    /// <summary>
    /// 更改参数名称，在子查询或是CTE子句中使用参数会有与主查询SQL中参数名相同，可以使用此方法更改参数名，避免参数名重复
    /// </summary>
    /// <typeparam name="T">变量类型</typeparam>
    /// <param name="value">变量值</param>
    /// <param name="parameterName">参数名称</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static T ToParameter<T>(this T value, string parameterName) => throw new NotImplementedException();

    public static bool In<TElement>(TElement value, params TElement[] list) => throw new NotImplementedException();
    public static bool In<TElement>(TElement value, IEnumerable<TElement> list) => throw new NotImplementedException();
    public static bool In<TElement>(TElement value, IQuery<TElement> subQuery) => throw new NotImplementedException();
    public static bool In<TElement>(TElement value, Func<IFromQuery, IQuery<TElement>> subQuery) => throw new NotImplementedException();

    /// <summary>
    /// 判断数据是否存在现有的子查询中，这个子查询对象必须是直接引用，不能做任何的引用外部变量的操作，如：
    /// <code>
    /// var myOrders = repository.From&lt;Order&gt;() ...
    /// .Where(x =&gt; Sql.Exists&lt;Order&gt;(myOrders))
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    /// <param name="subQuery">直接引用的子查询对象</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static bool Exists<TTarget>(IQuery<TTarget> subQuery) => throw new NotImplementedException();
    /// <summary>
    /// 判断数据是否存在子查询中，这个子查询对象可以是现有的子查询对象，也可以是新构造的子查询对象，如果引用现有子查询对象，请使用f =&gt; f.UseQuery()方法，如：
    /// <code>
    /// var myOrders = repository.From&lt;Order&gt;() ...
    /// .Where(x =&gt; Sql.Exists&lt;Order&gt;(f =&lt; f.UseQuery(myOrders)))
    /// .Where(x =&gt; Sql.Exists&lt;Order&gt;(f =&lt; f.UseQuery(myOrders).Where(t =&lt; t.Id == x.OrderId)))
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    /// <param name="subQuery"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static bool Exists<TTarget>(Func<IFromQuery, IQuery<TTarget>> subQuery) => throw new NotImplementedException();
    public static bool Exists<T>(Func<T, bool> predicate) => throw new NotImplementedException();
    public static bool Exists<T1, T2>(Func<T1, T2, bool> predicate) => throw new NotImplementedException();
    public static bool Exists<T1, T2, T3>(Func<T1, T2, T3, bool> predicate) => throw new NotImplementedException();
    public static bool Exists<T1, T2, T3, T4>(Func<T1, T2, T3, T4, bool> predicate) => throw new NotImplementedException();
    public static bool Exists<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, bool> predicate) => throw new NotImplementedException();
    public static bool Exists<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, bool> predicate) => throw new NotImplementedException();

    public static PredicateBuilder<T> Where<T>() => new PredicateBuilder<T>();
    public static PredicateBuilder<T1, T2> Where<T1, T2>() => new PredicateBuilder<T1, T2>();
    public static PredicateBuilder<T1, T2, T3> Where<T1, T2, T3>() => new PredicateBuilder<T1, T2, T3>();
    public static PredicateBuilder<T1, T2, T3, T4> Where<T1, T2, T3, T4>() => new PredicateBuilder<T1, T2, T3, T4>();
    public static PredicateBuilder<T1, T2, T3, T4, T5> Where<T1, T2, T3, T4, T5>() => new PredicateBuilder<T1, T2, T3, T4, T5>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6> Where<T1, T2, T3, T4, T5, T6>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7> Where<T1, T2, T3, T4, T5, T6, T7>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8> Where<T1, T2, T3, T4, T5, T6, T7, T8>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where<T1, T2, T3, T4, T5, T6, T7, T8, T9>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>();


    public static IWindowFunction<int> Rank() => throw new NotImplementedException();
    public static IWindowFunction<long> LongRank() => throw new NotImplementedException();
    public static IWindowFunction<int> DenseRank() => throw new NotImplementedException();
    public static IWindowFunction<long> LongDenseRank() => throw new NotImplementedException();
    public static IWindowFunction<int> RowNumber() => throw new NotImplementedException();
    public static IWindowFunction<int> LongRowNumber() => throw new NotImplementedException();
    public static IWindowFunction<int> Count() => throw new NotImplementedException();
    public static IWindowFunction<long> LongCount() => throw new NotImplementedException();
    public static IWindowFunction<int> Count<TField>(TField field) => throw new NotImplementedException();
    public static IWindowFunction<long> CountDistinct<TField>(TField field) => throw new NotImplementedException();
    public static IWindowFunction<long> LongCount<TField>(TField field) => throw new NotImplementedException();
    public static IWindowFunction<long> LongCountDistinct<TField>(TField field) => throw new NotImplementedException();
    public static IWindowFunction<TField> Sum<TField>(TField field) => throw new NotImplementedException();
    public static IWindowFunction<TField> Avg<TField>(TField field) => throw new NotImplementedException();
    public static IWindowFunction<TField> Max<TField>(TField field) => throw new NotImplementedException();
    public static IWindowFunction<TField> Min<TField>(TField field) => throw new NotImplementedException();

    /// <summary>
    /// 数据分组后，把字段field的多行数据，用,字符分割拼接在一起，行转列操作，仅支持MySql,Mariadb数据库
    /// </summary>
    /// <typeparam name="TFields"></typeparam>
    /// <param name="fields"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static IGroupConcat GroupConcat<TFields>(TFields fields) => throw new NotImplementedException();
    /// <summary>
    /// 数据分组后，把字段field的多行数据，用separator字符分割拼接在一起，行转列操作，仅支持MySql,Mariadb数据库
    /// </summary>
    /// <typeparam name="TFields"></typeparam>
    /// <param name="fields"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static IGroupConcat GroupConcat<TFields>(TFields fields, string separator) => throw new NotImplementedException();
    /// <summary>
    /// 数据分组后，字符串连接，仅支持Postgresql,SqlServer数据库，Postgresql: STRING_AGG (expression, separator [order_by_clause] ) ,Sql Server: STRING_AGG(expression, separator )[WITHIN GROUP(ORDER BY <order_by_expression_list> [ASC|DESC])]
    /// </summary>
    /// <typeparam name="TFields"></typeparam>
    /// <param name="fields"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static IStringAgg StringAgg<TFields>(TFields fields, string separator) => throw new NotImplementedException();

}
public interface IPartitionByOver<TValue>
{
    IPartitionByOver<TValue> OrderBy<TFields>(TFields fields);
    IPartitionByOver<TValue> OrderByDescending<TFields>(TFields fields);
    TValue ToValue();
}
public interface IWindowFunction<TValue>
{
    ISqlOver<TValue> Over();
}
public interface ISqlOver<TValue>
{
    ISqlOver<TValue> OrderBy<TFields>(TFields fields);
    ISqlOver<TValue> OrderByDescending<TFields>(TFields fields);
    IPartitionByOver<TValue> PartitionBy<TFields>(TFields fields);
    TValue ToValue();
}
public interface IGroupConcat
{
    IGroupConcat OrderBy<TFields>(TFields fields);
    IGroupConcat OrderByDescending<TFields>(TFields fields);
    IGroupConcat Distinct();
    string ToValue();
}
public interface IStringAgg
{
    IStringAgg OrderBy<TFields>(TFields fields);
    IStringAgg OrderByDescending<TFields>(TFields fields);
    string ToValue();
}