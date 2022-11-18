using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public interface IIncludableQuery<T, TMember> : IQuery<T>
{
    IIncludableQuery<T, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null);
    IIncludableQuery<T, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
}
public interface IIncludableQuery<T1, T2, TMember> : IQuery<T1, T2>
{
    IIncludableQuery<T1, T2, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null);
    IIncludableQuery<T1, T2, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
}
public interface IIncludableQuery<T1, T2, T3, TMember> : IQuery<T1, T2, T3>
{
    IIncludableQuery<T1, T2, T3, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null);
    IIncludableQuery<T1, T2, T3, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
}
public interface IIncludableQuery<T1, T2, T3, T4, TMember> : IQuery<T1, T2, T3, T4>
{
    IIncludableQuery<T1, T2, T3, T4, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null);
    IIncludableQuery<T1, T2, T3, T4, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
}
public interface IIncludableQuery<T1, T2, T3, T4, T5, TMember> : IQuery<T1, T2, T3, T4, T5>
{
    IIncludableQuery<T1, T2, T3, T4, T5, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null);
    IIncludableQuery<T1, T2, T3, T4, T5, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
}
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> : IQuery<T1, T2, T3, T4, T5, T6>
{
    IIncludableQuery<T1, T2, T3, T4, T5, T6, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
}
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7>
{
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
}
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8>
{
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
}
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
}
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
}
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
}
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
}
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
}
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
}
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
}
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
{
}