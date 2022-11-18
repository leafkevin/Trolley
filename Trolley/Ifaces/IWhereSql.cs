using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public interface IWhereSql
{
    bool In<TElement>(TElement value, params TElement[] list);
    bool In<TElement>(TElement value, IEnumerable<TElement> list);
    bool In<TElement>(TElement value, IQuery<TElement> subQuery);
    bool In<TElement>(TElement value, Func<IFromQuery, IQuery<TElement>> subQuery);
    bool Exists<T>(Expression<Func<T, bool>> filter);
    bool Exists<T>(Expression<Func<IQuery<T>, bool>> filter);
    bool Exists<T1, T2>(Expression<Func<T1, T2, bool>> filter);
    bool Exists<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> filter);
    bool Exists<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> filter);
    bool Exists<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> filter);
}