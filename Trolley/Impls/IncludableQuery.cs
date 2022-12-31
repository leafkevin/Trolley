using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

class IncludableQuery<T, TMember> : Query<T>, IIncludableQuery<T, TMember>
{
    public IncludableQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
        : base(dbFactory, connection, transaction, visitor) { }

    public IIncludableQuery<T, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T, TNavigation>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
}
class IncludableQuery<T1, T2, TMember> : Query<T1, T2>, IIncludableQuery<T1, T2, TMember>
{
    public IncludableQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
        : base(dbFactory, connection, transaction, visitor) { }

    public IIncludableQuery<T1, T2, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, TNavigation>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
}
class IncludableQuery<T1, T2, T3, TMember> : Query<T1, T2, T3>, IIncludableQuery<T1, T2, T3, TMember>
{
    public IncludableQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
        : base(dbFactory, connection, transaction, visitor) { }

    public IIncludableQuery<T1, T2, T3, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, TNavigation>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
}
class IncludableQuery<T1, T2, T3, T4, TMember> : Query<T1, T2, T3, T4>, IIncludableQuery<T1, T2, T3, T4, TMember>
{
    public IncludableQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
        : base(dbFactory, connection, transaction, visitor) { }

    public IIncludableQuery<T1, T2, T3, T4, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, TNavigation>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
}
class IncludableQuery<T1, T2, T3, T4, T5, TMember> : Query<T1, T2, T3, T4, T5>, IIncludableQuery<T1, T2, T3, T4, T5, TMember>
{
    public IncludableQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
        : base(dbFactory, connection, transaction, visitor) { }

    public IIncludableQuery<T1, T2, T3, T4, T5, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, TNavigation>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
}
class IncludableQuery<T1, T2, T3, T4, T5, T6, TMember> : Query<T1, T2, T3, T4, T5, T6>, IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>
{
    public IncludableQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
        : base(dbFactory, connection, transaction, visitor) { }

    public IIncludableQuery<T1, T2, T3, T4, T5, T6, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, TNavigation>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
}
class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> : Query<T1, T2, T3, T4, T5, T6, T7>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>
{
    public IncludableQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
        : base(dbFactory, connection, transaction, visitor) { }

    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, TNavigation>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
}
class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>
{
    public IncludableQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
        : base(dbFactory, connection, transaction, visitor) { }

    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TNavigation>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
}
class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>
{
    public IncludableQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
        : base(dbFactory, connection, transaction, visitor) { }

    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TNavigation>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
}
class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>
{
    public IncludableQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
        : base(dbFactory, connection, transaction, visitor) { }

    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNavigation>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
}
class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>
{
    public IncludableQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
        : base(dbFactory, connection, transaction, visitor) { }

    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TNavigation>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
}
class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>
{
    public IncludableQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
        : base(dbFactory, connection, transaction, visitor) { }

    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TNavigation>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
}
class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>
{
    public IncludableQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
        : base(dbFactory, connection, transaction, visitor) { }

    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TNavigation>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
}
class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>
{
    public IncludableQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
        : base(dbFactory, connection, transaction, visitor) { }

    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TNavigation>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
}
class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>
{
    public IncludableQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
        : base(dbFactory, connection, transaction, visitor) { }

    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member, Expression<Func<TNavigation, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TNavigation>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.ThenInclude(member, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
}
class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>
{
    public IncludableQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
        : base(dbFactory, connection, transaction, visitor) { }
}