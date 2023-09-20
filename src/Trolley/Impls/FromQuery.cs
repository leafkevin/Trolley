﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class QueryAnonymousObject : IQueryAnonymousObject
{
    #region Fields
    private readonly IQueryVisitor visitor;
    #endregion

    #region Constructor
    public QueryAnonymousObject(IQueryVisitor visitor) => this.visitor = visitor;
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        var sql = this.visitor.BuildSql(out _);
        if (this.visitor.Command.Parameters.Count > 0)
            dbParameters = this.visitor.Command.Parameters.Cast<IDbDataParameter>().ToList();
        return sql;
    }
    #endregion
}
class FromQueryBase
{
    #region Fields
    protected string dbKey;
    protected TheaConnection connection;
    protected IOrmProvider ormProvider;
    protected IEntityMapProvider mapProvider;
    protected IQueryVisitor visitor;
    protected Type insertType;
    #endregion

    #region Constructor
    public FromQueryBase(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
    {
        this.connection = connection;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.visitor = visitor;
        this.dbKey = connection?.DbKey;
        this.insertType = insertType;
    }
    #endregion

    #region Select
    public IQueryAnonymousObject Select(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        var sql = this.visitor.BuildSql(out _);
        if (this.visitor.Command.Parameters.Count > 0)
            dbParameters = this.visitor.Command.Parameters.Cast<IDbDataParameter>().ToList();
        return sql;
    }
    #endregion
}
class FromQuery : IFromQuery
{
    #region Fields
    protected string dbKey;
    protected TheaConnection connection;
    protected IOrmProvider ormProvider;
    protected IEntityMapProvider mapProvider;
    protected IQueryVisitor visitor;
    #endregion

    #region Constructor
    public FromQuery(IQueryVisitor visitor) => this.visitor = visitor;
    public FromQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
    {
        this.connection = connection;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.visitor = visitor;
        this.dbKey = connection?.DbKey;
    }
    #endregion

    #region From
    public IFromQuery<T> From<T>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T));
        return new FromQuery<T>(this.connection, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IFromQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2));
        return new FromQuery<T1, T2>(this.connection, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IFromQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3));
        return new FromQuery<T1, T2, T3>(this.connection, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new FromQuery<T1, T2, T3, T4>(this.connection, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new FromQuery<T1, T2, T3, T4, T5>(this.connection, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return new FromQuery<T1, T2, T3, T4, T5, T6>(this.connection, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7>(this.connection, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8>(this.connection, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this.connection, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this.connection, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this.connection, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this.connection, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this.connection, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this.connection, this.ormProvider, this.mapProvider, this.visitor);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(char tableAsStart = 'a')
    {
        this.visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15));
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this.connection, this.ormProvider, this.mapProvider, this.visitor);
    }
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        var sql = this.visitor.BuildSql(out _);
        if (this.visitor.Command.Parameters.Count > 0)
            dbParameters = this.visitor.Command.Parameters.Cast<IDbDataParameter>().ToList();
        return sql;
    }
    #endregion
}
class FromQuery<T> : IFromQuery<T>
{
    #region Fields
    protected string dbKey;
    protected TheaConnection connection;
    protected IOrmProvider ormProvider;
    protected IEntityMapProvider mapProvider;
    protected IQueryVisitor visitor;
    protected Type insertType;
    #endregion

    #region Constructor
    public FromQuery(IOrmProvider ormProvider, IQueryVisitor visitor)
    {
        this.ormProvider = ormProvider;
        this.visitor = visitor;
    }
    public FromQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
    {
        this.connection = connection;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.visitor = visitor;
        this.dbKey = connection?.DbKey;
        this.insertType = insertType;
    }
    #endregion

    #region Union/UnionAll
    public IFromQuery<T> Union(Func<IFromQuery, IFromQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = " UNION" + Environment.NewLine + newVisitor.BuildSql(out _, true);
        this.visitor.Union(typeof(T), sql);
        return this;
    }
    public IFromQuery<T> UnionAll(Func<IFromQuery, IFromQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = " UNION ALL" + Environment.NewLine + newVisitor.BuildSql(out _, true);
        this.visitor.Union(typeof(T), sql);
        return this;
    }
    public IFromQuery<T> UnionRecursive(Func<IFromQuery, IFromQuery<T>, IFromQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor), null);
        var sql = " UNION" + Environment.NewLine + newVisitor.BuildSql(out _, true);
        this.visitor.Union(typeof(T), sql);
        return this;
    }
    public IFromQuery<T> UnionAllRecursive(Func<IFromQuery, IFromQuery<T>, IFromQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor), null);
        var sql = " UNION ALL" + Environment.NewLine + newVisitor.BuildSql(out _, true);
        this.visitor.Union(typeof(T), sql);
        return this;
    }
    #endregion

    #region Join
    public IFromQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region Where/And
    public IFromQuery<T> Where(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    public IFromQuery<T> And(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T> And(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IFromGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T, TGrouping>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Distinct
    public IFromQuery<T> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    #endregion

    #region Take
    public IFromQuery<T> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<T> Select()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr, true);
        return this;
    }
    public IQueryAnonymousObject Select(string fields)
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new QueryAnonymousObject(this.visitor);
    }
    public IFromQuery<TTarget> Select<TTarget>(string fields)
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.visitor.Select(fields, null, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    //TODO:
    //public IFromQuery<TTarget> Select<TTarget>(TTarget parameters)
    //{
    //    if (parameters == null)
    //        throw new ArgumentNullException(nameof(parameters));
    //    //TODO:
    //    this.visitor.Select(fields, null, true);
    //    var fromQuery = new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    //    //this.visitor.Select()
    //    return fromQuery;
    //}
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region Execute   
    public int Execute()
    {
        if (this.insertType == null)
            throw new NotSupportedException("不支持的操作，只用于INSERT INTO ... SELECT ...");

        using var command = this.visitor.Command;
        command.CommandType = CommandType.Text;
        command.CommandText = this.visitor.BuildSql(out _);
        var result = command.ExecuteNonQuery();
        command.Dispose();
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (this.insertType == null)
            throw new NotSupportedException("不支持的操作，只用于INSERT INTO ... SELECT ...");

        using var cmd = this.visitor.Command;
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = this.visitor.BuildSql(out _);
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        await command.DisposeAsync();
        return result;
    }
    public long ExecuteLong()
    {
        if (this.insertType == null)
            throw new NotSupportedException("不支持的操作，只用于INSERT INTO ... SELECT ...");

        using var command = this.visitor.Command;
        command.CommandType = CommandType.Text;
        command.CommandText = this.visitor.BuildSql(out _);

        var result = command.ExecuteNonQuery();
        command.Dispose();
        return result;
    }
    public async Task<long> ExecuteLongAsync(CancellationToken cancellationToken = default)
    {
        if (this.insertType == null)
            throw new NotSupportedException("不支持的操作，只用于INSERT INTO ... SELECT ...");

        using var cmd = this.visitor.Command;
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = this.visitor.BuildSql(out _);
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        await command.DisposeAsync();
        return result;
    }
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        var sql = this.visitor.BuildSql(out _);
        if (this.visitor.Command.Parameters.Count > 0)
            dbParameters = this.visitor.Command.Parameters.Cast<IDbDataParameter>().ToList();
        return sql;
    }
    #endregion
}
class FromQuery<T1, T2> : FromQueryBase, IFromQuery<T1, T2>
{
    #region Constructor
    public FromQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Join
    public IFromQuery<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region WithTable
    public IFromQuery<T1, T2, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var readerFields);
        this.visitor.WithTable(typeof(TOther), sql, readerFields);
        return new FromQuery<T1, T2, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2> And(Expression<Func<T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IFromGroupingQuery<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, TGrouping>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region OrderBy
    public IFromQuery<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromQuery<T1, T2, T3> : FromQueryBase, IFromQuery<T1, T2, T3>
{
    #region Constructor
    public FromQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Join
    public IFromQuery<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region WithTable
    public IFromQuery<T1, T2, T3, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var readerFields);
        this.visitor.WithTable(typeof(TOther), sql, readerFields);
        return new FromQuery<T1, T2, T3, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IFromGroupingQuery<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, TGrouping>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region OrderBy
    public IFromQuery<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromQuery<T1, T2, T3, T4> : FromQueryBase, IFromQuery<T1, T2, T3, T4>
{
    #region Constructor
    public FromQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Join
    public IFromQuery<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region WithTable
    public IFromQuery<T1, T2, T3, T4, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var readerFields);
        this.visitor.WithTable(typeof(TOther), sql, readerFields);
        return new FromQuery<T1, T2, T3, T4, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IFromGroupingQuery<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, TGrouping>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region OrderBy
    public IFromQuery<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromQuery<T1, T2, T3, T4, T5> : FromQueryBase, IFromQuery<T1, T2, T3, T4, T5>
{
    #region Constructor
    public FromQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region WithTable
    public IFromQuery<T1, T2, T3, T4, T5, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var readerFields);
        this.visitor.WithTable(typeof(TOther), sql, readerFields);
        return new FromQuery<T1, T2, T3, T4, T5, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IFromGroupingQuery<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, TGrouping>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region OrderBy
    public IFromQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromQuery<T1, T2, T3, T4, T5, T6> : FromQueryBase, IFromQuery<T1, T2, T3, T4, T5, T6>
{
    #region Constructor
    public FromQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region WithTable
    public IFromQuery<T1, T2, T3, T4, T5, T6, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var readerFields);
        this.visitor.WithTable(typeof(TOther), sql, readerFields);
        return new FromQuery<T1, T2, T3, T4, T5, T6, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region OrderBy
    public IFromQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7> : FromQueryBase, IFromQuery<T1, T2, T3, T4, T5, T6, T7>
{
    #region Constructor
    public FromQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region WithTable
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var readerFields);
        this.visitor.WithTable(typeof(TOther), sql, readerFields);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region OrderBy
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8> : FromQueryBase, IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8>
{
    #region Constructor
    public FromQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region WithTable
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var readerFields);
        this.visitor.WithTable(typeof(TOther), sql, readerFields);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region OrderBy
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> : FromQueryBase, IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    #region Constructor
    public FromQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region WithTable
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var readerFields);
        this.visitor.WithTable(typeof(TOther), sql, readerFields);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region OrderBy
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : FromQueryBase, IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    #region Constructor
    public FromQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region WithTable
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var readerFields);
        this.visitor.WithTable(typeof(TOther), sql, readerFields);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region OrderBy
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : FromQueryBase, IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    #region Constructor
    public FromQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region WithTable
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var readerFields);
        this.visitor.WithTable(typeof(TOther), sql, readerFields);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region OrderBy
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : FromQueryBase, IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    #region Constructor
    public FromQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region WithTable
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var readerFields);
        this.visitor.WithTable(typeof(TOther), sql, readerFields);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region OrderBy
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : FromQueryBase, IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    #region Constructor
    public FromQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region WithTable
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var readerFields);
        this.visitor.WithTable(typeof(TOther), sql, readerFields);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region OrderBy
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : FromQueryBase, IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    #region Constructor
    public FromQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TTarget), cteTableName, joinOn);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region WithTable
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone();
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var readerFields);
        this.visitor.WithTable(typeof(TOther), sql, readerFields);
        return new FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new FromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion

    #region OrderBy
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}
class FromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : FromQueryBase, IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    #region Constructor
    public FromQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IQueryVisitor visitor, Type insertType = null)
        : base(connection, ormProvider, mapProvider, visitor, insertType) { }
    #endregion

    #region Join
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    #endregion
    #region Where/And
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region OrderBy
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    public IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    public IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr, true);
        return new FromQuery<TTarget>(this.connection, this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    }
    #endregion
}