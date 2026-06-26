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
    //public static T As<T>(this object fields) => throw new NotImplementedException();
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
    public static bool In<TElement>(this TElement value, IQuery subQuery) => throw new NotImplementedException();

    public static bool Exists<T>(Func<T, bool> predicate) => throw new NotImplementedException();
    public static bool Exists<T1, T2>(Func<T1, T2, bool> predicate) => throw new NotImplementedException();
    public static bool Exists<T1, T2, T3>(Func<T1, T2, T3, bool> predicate) => throw new NotImplementedException();
    public static bool Exists<T1, T2, T3, T4>(Func<T1, T2, T3, T4, bool> predicate) => throw new NotImplementedException();
    public static bool Exists<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, bool> predicate) => throw new NotImplementedException();
    public static bool Exists<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, bool> predicate) => throw new NotImplementedException();

    public static IQuery<T> From<T>() => throw new NotImplementedException();
    public static IQuery<T1, T2> From<T1, T2>() => throw new NotImplementedException();
    public static IQuery<T1, T2, T3> From<T1, T2, T3>() => throw new NotImplementedException();
    public static IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>() => throw new NotImplementedException();
    public static IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>() => throw new NotImplementedException();
    public static IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>() => throw new NotImplementedException();
    public static IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>() => throw new NotImplementedException();
    public static IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>() => throw new NotImplementedException();
    public static IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>() => throw new NotImplementedException();
    public static IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>() => throw new NotImplementedException();
    public static IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>() => throw new NotImplementedException();
    public static IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>() => throw new NotImplementedException();
    public static IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>() => throw new NotImplementedException();
    public static IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>() => throw new NotImplementedException();
    public static IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>() => throw new NotImplementedException();
    public static IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>() => throw new NotImplementedException();



    public static ISqlWindowFunction Over() => throw new NotImplementedException();

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

public interface ISqlWindowFunction
{
    int Rank();
    long LongRank();
    int DenseRank();
    long LongDenseRank();
    int RowNumber();
    int LongRowNumber();
    int Count();
    long LongCount();
    int Count<TField>(TField field);
    long CountDistinct<TField>(TField field);
    long LongCount<TField>(TField field);
    long LongCountDistinct<TField>(TField field);
    TField Sum<TField>(TField field);
    TField Avg<TField>(TField field);
    TField Max<TField>(TField field);
    TField Min<TField>(TField field);

    ISqlWindowFunction PartitionBy<TFields>(TFields fields);
    ISqlWindowFunction OrderBy<TFields>(TFields fields);
    ISqlWindowFunction OrderByDescending<TFields>(TFields fields);
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