using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public class Sql
{
    public static string Null = "NULL";
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
    public static bool Exists<T>(Expression<Func<T, bool>> filter)
    {
        throw new NotImplementedException();
    }
    public static bool Exists<T>(Expression<Func<IQuery<T>, bool>> filter)
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
