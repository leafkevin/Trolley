using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Trolley;

public class FromQuery : IFromQuery
{
    #region Fields
    protected string dbKey;
    protected TheaConnection connection;
    protected IDbTransaction transaction;
    protected IOrmProvider ormProvider;
    protected IEntityMapProvider mapProvider;
    protected IQueryVisitor visitor;
    #endregion

    #region Constructor
    public FromQuery(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.visitor = visitor;
        this.dbKey = connection?.DbKey;
    }
    public FromQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
    {
        this.dbKey = multiQuery.DbKey;
        this.connection = multiQuery.Connection as TheaConnection;
        this.transaction = multiQuery.Transaction;
        this.ormProvider = multiQuery.OrmProvider;
        this.mapProvider = multiQuery.MapProvider;
        this.visitor = visitor;
    }
    #endregion

    #region From
    public IQuery<T> From<T>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T));
        return new Query<T>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T> From<T>(char tableAsStart, string suffixRawSql)
    {
        this.visitor.From(tableAsStart, typeof(T), suffixRawSql);
        return new Query<T>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2));
        return new Query<T1, T2>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3));
        return new Query<T1, T2, T3>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new Query<T1, T2, T3, T4>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new Query<T1, T2, T3, T4, T5>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return new Query<T1, T2, T3, T4, T5, T6>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return new Query<T1, T2, T3, T4, T5, T6, T7>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this.connection, this.transaction, this.ormProvider, this.mapProvider, this.visitor);
    }
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = this.visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        return this.visitor.BuildSql(out _);
    }
    #endregion
}