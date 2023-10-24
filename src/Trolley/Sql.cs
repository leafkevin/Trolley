using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public static class Sql
{
    public static T Null<T>()
        => throw new NotImplementedException();
    /// <summary>
    /// Select语句从数据库执行后，进行方法调用并把返回值赋值到对应的字段上，只做实体赋值解析，不实现
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static T Deferred<T>(this T obj)
        => throw new NotImplementedException();
    /// <summary>
    /// 当前字段或是表达式是否为NULL，只做条件解析，不实现
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    /// <param name="field">字段访问</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static bool IsNull<TField>(this TField field)
        => throw new NotImplementedException();
    /// <summary>
    /// 把当前对象类型转换为TTarget类型，相同名字的成员直接赋值，不存在的成员不做处理，只做实体赋值解析，不实现
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static TTarget FlattenTo<TTarget>()
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// 把当前对象类型转换为TTarget类型，除了specialMemberInitializer表达式中的成员做特殊处理外，其他相同的成员名字直接赋值，不存在的成员不做处理，只做实体赋值解析，不实现
    /// </summary>
    /// <typeparam name="TTarget">类型</typeparam>
    /// <param name="specialMemberInitializer">做特殊处理的成员赋值表达式</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static TTarget FlattenTo<TTarget>(Func<object> specialMemberInitializer)
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
    public static int RowNumber()
    {
        throw new NotImplementedException();
    }
    public static int RowNumberOver(Func<RowNumberOver, RowNumberOver> overExpr)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// 参数化当前变量，常量不做参数化，当前值都将被参数化如：@p0,@p1等，本函数只用来解析，并不实现，用法：
    /// <code>
    /// string productNo="xxx";
    /// using var repository = dbFactory.Create();
    /// var result1 = await repository.QueryAsync&lt;Product&gt;(f =&gt; f.ProductNo.Contains(productNo.ToParameter()));
    /// var result2 = await repository.QueryAsync&lt;Product&gt;(f =&gt; f.ProductNo.Contains("PN-001".ToParameter()));
    /// </code>
    /// </summary>
    /// <typeparam name="T">原值类型</typeparam>
    /// <param name="value">原值</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static T ToParameter<T>(this T value)
    {
        throw new NotImplementedException();
    }
    public static bool In<TElement>(TElement value, params TElement[] list)
    {
        throw new NotImplementedException();
    }
    public static bool In<TElement>(TElement value, IEnumerable<TElement> list)
    {
        throw new NotImplementedException();
    }
    public static bool In<TElement>(TElement value, Func<IFromQuery, IQuery<TElement>> subQuery)
    {
        throw new NotImplementedException();
    }
    public static bool Exists(Func<IFromQuery, IQueryAnonymousObject> subQuery)
    {
        throw new NotImplementedException();
    }
    public static bool Exists<T>(Expression<Func<T, bool>> filter)
    {
        throw new NotImplementedException();
    }
    public static bool Exists<T1, T2>(Expression<Func<T1, T2, bool>> filter)
    {
        throw new NotImplementedException();
    }
    public static bool Exists<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> filter)
    {
        throw new NotImplementedException();
    }
    public static bool Exists<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> filter)
    {
        throw new NotImplementedException();
    }
    public static bool Exists<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> filter)
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
