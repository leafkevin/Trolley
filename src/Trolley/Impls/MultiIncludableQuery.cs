using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

class MultiIncludableQuery<T, TMember> : MultiQuery<T>, IMultiIncludableQuery<T, TMember>
{
    public MultiIncludableQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }

    public IMultiIncludableQuery<T, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.visitor.ThenInclude(member);
        return new MultiIncludableQuery<T, TNavigation>(this.multiQuery, this.ormProvider, this.visitor);
    }
    public IMultiIncludableQuery<T, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, true, filter);
        return new MultiIncludableQuery<T, TElment>(this.multiQuery, this.ormProvider, this.visitor);
    }
}
class MultiIncludableQuery<T1, T2, TMember> : MultiQuery<T1, T2>, IMultiIncludableQuery<T1, T2, TMember>
{
    public MultiIncludableQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }

    public IMultiIncludableQuery<T1, T2, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.visitor.ThenInclude(member);
        return new MultiIncludableQuery<T1, T2, TNavigation>(this.multiQuery, this.ormProvider, this.visitor);
    }
    public IMultiIncludableQuery<T1, T2, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, true, filter);
        return new MultiIncludableQuery<T1, T2, TElment>(this.multiQuery, this.ormProvider, this.visitor);
    }
}
class MultiIncludableQuery<T1, T2, T3, TMember> : MultiQuery<T1, T2, T3>, IMultiIncludableQuery<T1, T2, T3, TMember>
{
    public MultiIncludableQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }

    public IMultiIncludableQuery<T1, T2, T3, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.visitor.ThenInclude(member);
        return new MultiIncludableQuery<T1, T2, T3, TNavigation>(this.multiQuery, this.ormProvider, this.visitor);
    }
    public IMultiIncludableQuery<T1, T2, T3, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, true, filter);
        return new MultiIncludableQuery<T1, T2, T3, TElment>(this.multiQuery, this.ormProvider, this.visitor);
    }
}
class MultiIncludableQuery<T1, T2, T3, T4, TMember> : MultiQuery<T1, T2, T3, T4>, IMultiIncludableQuery<T1, T2, T3, T4, TMember>
{
    public MultiIncludableQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }

    public IMultiIncludableQuery<T1, T2, T3, T4, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.visitor.ThenInclude(member);
        return new MultiIncludableQuery<T1, T2, T3, T4, TNavigation>(this.multiQuery, this.ormProvider, this.visitor);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, true, filter);
        return new MultiIncludableQuery<T1, T2, T3, T4, TElment>(this.multiQuery, this.ormProvider, this.visitor);
    }
}
class MultiIncludableQuery<T1, T2, T3, T4, T5, TMember> : MultiQuery<T1, T2, T3, T4, T5>, IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember>
{
    public MultiIncludableQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }

    public IMultiIncludableQuery<T1, T2, T3, T4, T5, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.visitor.ThenInclude(member);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, TNavigation>(this.multiQuery, this.ormProvider, this.visitor);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, true, filter);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, TElment>(this.multiQuery, this.ormProvider, this.visitor);
    }
}
class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>
{
    public MultiIncludableQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }

    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.visitor.ThenInclude(member);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, TNavigation>(this.multiQuery, this.ormProvider, this.visitor);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, true, filter);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, TElment>(this.multiQuery, this.ormProvider, this.visitor);
    }
}
class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>
{
    public MultiIncludableQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }

    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.visitor.ThenInclude(member);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TNavigation>(this.multiQuery, this.ormProvider, this.visitor);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, true, filter);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElment>(this.multiQuery, this.ormProvider, this.visitor);
    }
}
class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>
{
    public MultiIncludableQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }

    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.visitor.ThenInclude(member);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TNavigation>(this.multiQuery, this.ormProvider, this.visitor);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, true, filter);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElment>(this.multiQuery, this.ormProvider, this.visitor);
    }
}
class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>
{
    public MultiIncludableQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }

    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.visitor.ThenInclude(member);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TNavigation>(this.multiQuery, this.ormProvider, this.visitor);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, true, filter);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElment>(this.multiQuery, this.ormProvider, this.visitor);
    }
}
class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>
{
    public MultiIncludableQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }

    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.visitor.ThenInclude(member);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNavigation>(this.multiQuery, this.ormProvider, this.visitor);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, true, filter);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElment>(this.multiQuery, this.ormProvider, this.visitor);
    }
}
class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>
{
    public MultiIncludableQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }

    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.visitor.ThenInclude(member);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TNavigation>(this.multiQuery, this.ormProvider, this.visitor);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, true, filter);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElment>(this.multiQuery, this.ormProvider, this.visitor);
    }
}
class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>
{
    public MultiIncludableQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }

    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.visitor.ThenInclude(member);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TNavigation>(this.multiQuery, this.ormProvider, this.visitor);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, true, filter);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElment>(this.multiQuery, this.ormProvider, this.visitor);
    }
}
class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>
{
    public MultiIncludableQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }

    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.visitor.ThenInclude(member);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TNavigation>(this.multiQuery, this.ormProvider, this.visitor);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, true, filter);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElment>(this.multiQuery, this.ormProvider, this.visitor);
    }
}
class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>
{
    public MultiIncludableQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }

    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.visitor.ThenInclude(member);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TNavigation>(this.multiQuery, this.ormProvider, this.visitor);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, true, filter);
        return new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElment>(this.multiQuery, this.ormProvider, this.visitor);
    }
}
class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>
{
    public MultiIncludableQuery(MultipleQuery multiQuery, IOrmProvider ormProvider, IQueryVisitor visitor)
        : base(multiQuery, ormProvider, visitor) { }
}