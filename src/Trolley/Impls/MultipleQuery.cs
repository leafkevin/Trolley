﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class MultipleQuery : IMultipleQuery, IDisposable
{
    #region Fields
    protected bool isParameterized = false;
    protected TheaConnection connection;
    protected StringBuilder sqlBuilder = new();
    #endregion

    #region Properties
    public string DbKey { get; private set; }
    public IDbConnection Connection => this.connection;
    public IOrmProvider OrmProvider { get; private set; }
    public IEntityMapProvider MapProvider { get; private set; }
    public IDbTransaction Transaction { get; private set; }
    public IDbCommand Command { get; private set; }
    public List<ReaderAfter> ReaderAfters { get; private set; }
    #endregion

    #region Constructor
    public MultipleQuery(string dbKey, IDbConnection connection, IDbCommand command, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized)
    {
        this.DbKey = dbKey;
        this.connection = new TheaConnection { DbKey = dbKey, BaseConnection = connection };
        this.Command = command;
        this.Transaction = this.Command.Transaction;
        this.OrmProvider = ormProvider;
        this.MapProvider = mapProvider;
        this.isParameterized = isParameterized;
        this.ReaderAfters = new();
    }
    public MultipleQuery(TheaConnection connection, IDbCommand command, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized)
    {
        this.DbKey = connection.DbKey;
        this.connection = connection;
        this.Command = command;
        this.Transaction = this.Command.Transaction;
        this.OrmProvider = ormProvider;
        this.MapProvider = mapProvider;
        this.isParameterized = isParameterized;
        this.ReaderAfters = new();
    }
    #endregion

    #region From
    public IMultiQuery<T> From<T>(char tableAsStart = 'a', string suffixRawSql = null)
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T), suffixRawSql);
        return new MultiQuery<T>(this, visitor);
    }
    public IMultiQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2));
        return new MultiQuery<T1, T2>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3));
        return new MultiQuery<T1, T2, T3>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new MultiQuery<T1, T2, T3, T4>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new MultiQuery<T1, T2, T3, T4, T5>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return new MultiQuery<T1, T2, T3, T4, T5, T6>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return new MultiQuery<T1, T2, T3, T4, T5, T6, T7>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, visitor);
    }
    #endregion

    #region From SubQuery
    public IMultiQuery<T> From<T>(Func<IFromQuery, IQuery<T>> subQuery, char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        var fromQuery = new FromQuery(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor);
        var query = subQuery.Invoke(fromQuery);
        var sql = query.Visitor.BuildSql(out var readerFields);
        if (!visitor.Equals(query.Visitor))
            visitor = query.Visitor;
        visitor.WithTable(typeof(T), sql, readerFields);
        return new MultiQuery<T>(this, visitor);
    }
    #endregion

    #region FromWith
    public IMultiQuery<T> FromWith<T>(Func<IFromQuery, IQuery<T>> cteSubQuery, string cteTableName = null, char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart, true);
        var fromQuery = new FromQuery(this.connection, this.Transaction, this.OrmProvider, this.MapProvider, visitor);
        var query = cteSubQuery.Invoke(fromQuery);
        if (!visitor.Equals(query.Visitor))
            visitor = query.Visitor;
        var rawSql = visitor.BuildSql(out var readerFields, false);
        visitor.BuildCteTable(cteTableName, rawSql, readerFields, query, true);
        return new MultiQuery<T>(this, visitor);
    }
    #endregion

    #region QueryFirst/Query
    public IMultipleQuery QueryFirst<TEntity>(string rawSql)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        var targetType = typeof(TEntity);
        Func<IDataReader, object> readerGetter;
        if (targetType.IsEntityType(out _))
            readerGetter = reader => reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider);
        else readerGetter = reader => reader.To<TEntity>();
        this.AddReader(rawSql, readerGetter);
        return this;
    }
    public IMultipleQuery QueryFirst<TEntity>(string rawSql, object parameters)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.DbKey, this.OrmProvider, rawSql, parameters);
        commandInitializer.Invoke(this.Command, this.OrmProvider, parameters);

        var targetType = typeof(TEntity);
        Func<IDataReader, object> readerGetter;
        if (targetType.IsEntityType(out _))
            readerGetter = reader => reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider);
        else readerGetter = reader => reader.To<TEntity>();
        this.AddReader(rawSql, readerGetter);
        return this;
    }
    public IMultipleQuery QueryFirst<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, true);
        var typedCommandInitializer = commandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string>;
        var sql = typedCommandInitializer.Invoke(this.Command, this.OrmProvider, this.MapProvider, $"m{this.ReaderAfters.Count}", whereObj);

        Func<IDataReader, object> readerGetter = reader => reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider);
        this.AddReader(sql, readerGetter);
        return this;
    }
    public IMultipleQuery Query<TEntity>(string rawSql)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        var targetType = typeof(TEntity);
        Func<IDataReader, object> readerGetter;
        if (targetType.IsEntityType(out _))
            readerGetter = reader => reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider);
        else readerGetter = reader => reader.To<TEntity>();
        this.AddReader(rawSql, readerGetter);
        return this;
    }
    public IMultipleQuery Query<TEntity>(string rawSql, object parameters)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.DbKey, this.OrmProvider, rawSql, parameters);
        commandInitializer.Invoke(this.Command, this.OrmProvider, parameters);

        var targetType = typeof(TEntity);
        Func<IDataReader, object> readerGetter;
        if (targetType.IsEntityType(out _))
            readerGetter = reader => reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider);
        else readerGetter = reader => reader.To<TEntity>();
        this.AddReader(rawSql, readerGetter);
        return this;
    }
    public IMultipleQuery Query<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, true);
        var typedCommandInitializer = commandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string>;
        var sql = typedCommandInitializer.Invoke(this.Command, this.OrmProvider, this.MapProvider, $"m{this.ReaderAfters.Count}", whereObj);

        Func<IDataReader, object> readerGetter = reader => reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider);
        this.AddReader(sql, readerGetter);
        return this;
    }
    #endregion

    #region Get
    public IMultipleQuery Get<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildGetSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, true);
        var typedCommandInitializer = commandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string>;
        var sql = typedCommandInitializer.Invoke(this.Command, this.OrmProvider, this.MapProvider, $"m{this.ReaderAfters.Count}", whereObj);

        Func<IDataReader, object> readerGetter = reader => reader.To<TEntity>(this.DbKey, this.OrmProvider, this.MapProvider);
        this.AddReader(sql, readerGetter);
        return this;
    }
    #endregion

    #region Exists
    public IMultipleQuery Exists<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildExistsSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, true);
        var typedCommandInitializer = commandInitializer as Func<IDbCommand, IOrmProvider, IEntityMapProvider, string, object, string>;
        var sql = typedCommandInitializer.Invoke(this.Command, this.OrmProvider, this.MapProvider, $"m{this.ReaderAfters.Count}", whereObj);

        Func<IDataReader, object> readerGetter = reader => reader.To<int>() > 0;
        this.AddReader(sql, readerGetter);
        return this;
    }
    public IMultipleQuery Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate)
    {
        if (wherePredicate == null)
            throw new ArgumentNullException(nameof(wherePredicate));

        var sql = this.From<TEntity>().Where(wherePredicate)
            .Select(f => Sql.Count()).ToSql(out _);
        Func<IDataReader, object> readerGetter = reader => reader.To<int>() > 0;
        this.AddReader(sql, readerGetter);
        return this;
    }
    #endregion

    #region AddReader/BuildSql
    public void AddReader(string sql, Func<IDataReader, object> readerGetter, IQueryVisitor queryVisitor = null, int pageIndex = 0, int pageSize = 0)
    {
        if (this.sqlBuilder.Length > 0)
            this.sqlBuilder.Append(';');
        this.sqlBuilder.Append(sql);
        this.ReaderAfters.Add(new ReaderAfter
        {
            ReaderGetter = readerGetter,
            QueryVisitor = queryVisitor,
            PageIndex = pageIndex,
            PageSize = pageSize
        });
    }
    public string BuildSql(out List<ReaderAfter> readerAfters)
    {
        var sql = this.sqlBuilder.ToString();
        this.sqlBuilder.Clear();
        readerAfters = this.ReaderAfters;
        return sql;
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        this.sqlBuilder.Clear();
        this.ReaderAfters?.Clear();
        this.ReaderAfters = null;
        this.sqlBuilder = null;
        this.OrmProvider = null;
        this.MapProvider = null;
        //command和connection在reader中进行释放，此处只是去掉引用
        this.connection = null;
        this.Command = null;
        this.ReaderAfters.Clear();
        this.ReaderAfters = null;
    }
    #endregion

    private IQueryVisitor CreateQueryVisitor(char tableAsStart, bool isCteQuery = false)
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        visitor.DbParameters = this.Command.Parameters;
        if (isCteQuery)
        {
            visitor.CteTables = new();
            visitor.CteQueries = new();
            visitor.CteTableSegments = new();
        }
        return visitor;
    }
}
public class ReaderAfter
{
    public Func<IDataReader, object> ReaderGetter { get; set; }
    public IQueryVisitor QueryVisitor { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}