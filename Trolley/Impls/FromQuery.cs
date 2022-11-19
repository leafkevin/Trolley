using System.Data;

namespace Trolley;

class FromQuery : IFromQuery
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private IDbTransaction transaction;
    private string parameterPrefix = "p";

    public FromQuery(IOrmDbFactory dbFactory, TheaConnection connection, string parameterPrefix = "p")
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.parameterPrefix = parameterPrefix;
    }

    public IQuery<T> From<T>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection.OrmProvider);
        visitor.From(typeof(T));
        return new Query<T>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2> From<T1, T2>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection.OrmProvider);
        visitor.From(typeof(T1), typeof(T2));
        return new Query<T1, T2>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3> From<T1, T2, T3>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3));
        return new Query<T1, T2, T3>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new Query<T1, T2, T3, T4>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new Query<T1, T2, T3, T4, T5>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return new Query<T1, T2, T3, T4, T5, T6>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return new Query<T1, T2, T3, T4, T5, T6, T7>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this.dbFactory, this.connection, this.transaction, visitor);
    }
}
