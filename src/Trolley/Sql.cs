using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public static class Sql
{
    public static T Null<T>()
        => throw new NotImplementedException();
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
    public static T ToParameter<T>(this T value, string parameterName)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// 数据分组后，把字段field的多行数据，用separator字符分割拼接在一起，行转列操作
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="field">字段名称</param>
    /// <param name="separator">连接符</param>
    /// <returns>返回连接后的字符串表达式</returns>
    /// <exception cref="NotImplementedException"></exception>
    //public static string GroupConcat<TField>(this TField field, string separator)
    //{
    //    throw new NotImplementedException();
    //}
    //public static List<TTable> GroupInto<TTable, TField>(this TTable table, Expression<Func<TTable, TField>> fieldSelector)
    //{
    //    throw new NotImplementedException();
    //}
    //public static int RowNumber()
    //{
    //    throw new NotImplementedException();
    //}
    //public static int RowNumberOver(Func<RowNumberOver, RowNumberOver> overExpr)
    //{
    //    throw new NotImplementedException();
    //}
    public static bool In<TElement>(TElement value, params TElement[] list)
    {
        throw new NotImplementedException();
    }
    public static bool In<TElement>(TElement value, IEnumerable<TElement> list)
    {
        throw new NotImplementedException();
    }
    public static bool In<TElement>(TElement value, IQuery<TElement> subQuery)
    {
        throw new NotImplementedException();
    }
    public static bool In<TElement>(TElement value, Func<IFromQuery, IQuery<TElement>> subQuery)
    {
        throw new NotImplementedException();
    }
    public static bool Exists<TTarget>(Func<IFromQuery, IQuery<TTarget>> subQuery)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// 使用CTE表构建Exists查询条件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subQuery"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static bool Exists<T>(ICteQuery<T> subQuery, Expression<Func<T, bool>> predicate)
    {
        throw new NotImplementedException();
    }
    public static bool Exists<T>(Expression<Func<T, bool>> predicate)
    {
        throw new NotImplementedException();
    }
    public static bool Exists<T1, T2>(Expression<Func<T1, T2, bool>> predicate)
    {
        throw new NotImplementedException();
    }
    public static bool Exists<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        throw new NotImplementedException();
    }
    public static bool Exists<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        throw new NotImplementedException();
    }
    public static bool Exists<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        throw new NotImplementedException();
    }
    public static bool Exists<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        throw new NotImplementedException();
    }
    public static int Count()
    {
        throw new NotImplementedException();
    }
    public static long LongCount()
    {
        throw new NotImplementedException();
    }
    public static int Count<TField>(TField field)
    {
        throw new NotImplementedException();
    }
    public static int CountDistinct<TField>(TField field)
    {
        throw new NotImplementedException();
    }
    public static long LongCount<TField>(TField field)
    {
        throw new NotImplementedException();
    }
    public static long LongCountDistinct<TField>(TField field)
    {
        throw new NotImplementedException();
    }
    public static TField Sum<TField>(TField field)
    {
        throw new NotImplementedException();
    }
    public static TField Avg<TField>(TField field)
    {
        throw new NotImplementedException();
    }
    public static TField Max<TField>(TField field)
    {
        throw new NotImplementedException();
    }
    public static TField Min<TField>(TField field)
    {
        throw new NotImplementedException();
    }
}
public class RowNumberOver
{
    public RowNumberOver PartitionBy<Fields>(Func<Fields> fields)
    {
        throw new NotImplementedException();
    }
    public RowNumberOver OrderBy<Fields>(Func<Fields> fields)
    {
        throw new NotImplementedException();
    }
}
