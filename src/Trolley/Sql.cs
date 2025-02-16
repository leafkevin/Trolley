using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public static partial class Sql
{
    /// <summary>
    /// 使用原始SQL生成名一个字段，如：INSERT INTO XXX (...) VALUES(...) RETURNING myMethod(a.name,a.amount)+upper(a.order_no) as order_info
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    /// <param name="rawSql"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static TField Raw<TField>(string rawSql) => throw new NotImplementedException();
    public static TField Null<TField>() => throw new NotImplementedException();
    /// <summary>
    /// 用在修饰方法调用之后，表示前面的方法不做sql解析，当方法的参数从数据库读取后，再执行方法调用并把返回值赋值到对应的成员上，只做实体赋值解析，不实现
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
    public static bool Exists<TTarget>(Func<IFromQuery, IQuery<TTarget>> subQuery) => throw new NotImplementedException();
    /// <summary>
    /// 使用CTE表构建Exists查询条件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subQuery"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static bool Exists<T>(ICteQuery<T> subQuery, Expression<Func<T, bool>> predicate) => throw new NotImplementedException();
    public static bool Exists<T>(Expression<Func<T, bool>> predicate) => throw new NotImplementedException();
    public static bool Exists<T1, T2>(Expression<Func<T1, T2, bool>> predicate) => throw new NotImplementedException();
    public static bool Exists<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> predicate) => throw new NotImplementedException();
    public static bool Exists<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> predicate) => throw new NotImplementedException();
    public static bool Exists<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate) => throw new NotImplementedException();
    public static bool Exists<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate) => throw new NotImplementedException();


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