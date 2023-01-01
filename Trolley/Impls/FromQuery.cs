using System.Data;

namespace Trolley;

class FromQuery : IFromQuery
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly string parameterPrefix = "p";

    public FromQuery(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, string parameterPrefix = "p")
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.parameterPrefix = parameterPrefix;
    }

    public IQuery<T> From<T>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        visitor.From(typeof(T));
        return new Query<T>(visitor);
    }
    public IQuery<T1, T2> From<T1, T2>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        visitor.From(typeof(T1), typeof(T2));
        return new Query<T1, T2>(visitor);
    }
    public IQuery<T1, T2, T3> From<T1, T2, T3>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        visitor.From(typeof(T1), typeof(T2), typeof(T3));
        return new Query<T1, T2, T3>(visitor);
    }
    public IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new Query<T1, T2, T3, T4>(visitor);
    }
    public IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new Query<T1, T2, T3, T4, T5>(visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return new Query<T1, T2, T3, T4, T5, T6>(visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return new Query<T1, T2, T3, T4, T5, T6, T7>(visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8>(visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>(visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableStartAs = 'a')
    {
        var visitor = new QueryVisitor(this.dbFactory, this.connection, this.transaction, tableStartAs, this.parameterPrefix);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(visitor);
    }
}
