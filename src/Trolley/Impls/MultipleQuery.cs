using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

class MultipleQuery : IMultipleQuery
{
    #region Fields
    private StringBuilder sqlBuilder = new();
    #endregion

    #region Properties
    public string DbKey { get; private set; }
    public IOrmProvider OrmProvider { get; private set; }
    public IEntityMapProvider MapProvider { get; private set; }
    public bool IsParameterized { get; private set; }
    public TheaConnection Connection { get; private set; }
    public IDbCommand Command { get; private set; }
    public List<ReaderAfter> ReaderAfters { get; private set; }
    #endregion

    #region Constructor
    public MultipleQuery(string dbKey, IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IDbCommand command, bool isParameterized)
    {
        this.DbKey = dbKey;
        this.Connection = new TheaConnection { DbKey = dbKey, BaseConnection = connection };
        this.OrmProvider = ormProvider;
        this.MapProvider = mapProvider;
        this.Command = command;
        this.IsParameterized = isParameterized;
        this.ReaderAfters = new();
    }
    public MultipleQuery(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IDbCommand command, bool isParameterized)
    {
        this.DbKey = connection.DbKey;
        this.Connection = connection;
        this.OrmProvider = ormProvider;
        this.MapProvider = mapProvider;
        this.Command = command;
        this.IsParameterized = isParameterized;
        this.ReaderAfters = new();
    }
    #endregion

    #region From/FromWith/FromWithRecursive
    public IMultiQuery<T> From<T>(char tableAsStart = 'a', string suffixRawSql = null)
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, tableAsStart, multiParameterPrefix: $"m{this.ReaderAfters.Count}");
        visitor.From(tableAsStart, typeof(T), suffixRawSql);
        return new MultiQuery<T>(this, visitor);
    }
    public IMultiQuery<T> From<T>(Func<IFromQuery, IFromQuery<T>> subQuery, char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, tableAsStart, "p1w", $"m{this.ReaderAfters.Count}");
        subQuery.Invoke(new FromQuery(visitor));
        var sql = visitor.BuildSql(out var dbDataParameters, out var readerFields);
        var newVisitor = visitor.Clone(tableAsStart);
        newVisitor.WithTable(typeof(T), sql, dbDataParameters, readerFields);
        return new MultiQuery<T>(this, newVisitor);
    }
    public IMultiQuery<T> FromWith<T>(Func<IFromQuery, IFromQuery<T>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, tableAsStart, "p1w", $"m{this.ReaderAfters.Count}");
        cteSubQuery.Invoke(new FromQuery(visitor));
        var rawSql = visitor.BuildSql(out var dbDataParameters, out var readerFields);
        var newVisitor = visitor.Clone(tableAsStart);
        newVisitor.WithCteTable(typeof(T), cteTableName, false, rawSql, dbDataParameters, readerFields);
        return new MultiQuery<T>(this, newVisitor);
    }
    public IMultiQuery<T> FromWithRecursive<T>(Func<IFromQuery, string, IFromQuery<T>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, tableAsStart, "p1w", $"m{this.ReaderAfters.Count}");
        cteSubQuery.Invoke(new FromQuery(visitor), cteTableName);
        var rawSql = visitor.BuildSql(out var dbDataParameters, out var readerFields);
        var newVisitor = visitor.Clone(tableAsStart);
        newVisitor.WithCteTable(typeof(T), cteTableName, true, rawSql, dbDataParameters, readerFields);
        return new MultiQuery<T>(this, newVisitor);
    }
    public IMultiQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, tableAsStart, multiParameterPrefix: $"m{this.ReaderAfters.Count}");
        visitor.From(tableAsStart, typeof(T1), typeof(T2));
        return new MultiQuery<T1, T2>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, tableAsStart, multiParameterPrefix: $"m{this.ReaderAfters.Count}");
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3));
        return new MultiQuery<T1, T2, T3>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, tableAsStart, multiParameterPrefix: $"m{this.ReaderAfters.Count}");
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new MultiQuery<T1, T2, T3, T4>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, tableAsStart, multiParameterPrefix: $"m{this.ReaderAfters.Count}");
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new MultiQuery<T1, T2, T3, T4, T5>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, tableAsStart, multiParameterPrefix: $"m{this.ReaderAfters.Count}");
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return new MultiQuery<T1, T2, T3, T4, T5, T6>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, tableAsStart, multiParameterPrefix: $"m{this.ReaderAfters.Count}");
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return new MultiQuery<T1, T2, T3, T4, T5, T6, T7>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, tableAsStart, multiParameterPrefix: $"m{this.ReaderAfters.Count}");
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, tableAsStart, multiParameterPrefix: $"m{this.ReaderAfters.Count}");
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, tableAsStart, multiParameterPrefix: $"m{this.ReaderAfters.Count}");
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, visitor);
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

        var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.Connection, this.OrmProvider, rawSql, parameters);
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
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.Connection, this.OrmProvider, this.MapProvider, entityType, whereObj);
        var sql = commandInitializer.Invoke(this.Command, this.OrmProvider, this.MapProvider, $"m{this.ReaderAfters.Count}", whereObj);

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

        var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.Connection, this.OrmProvider, rawSql, parameters);
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
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.Connection, this.OrmProvider, this.MapProvider, entityType, whereObj);
        var sql = commandInitializer.Invoke(this.Command, this.OrmProvider, this.MapProvider, $"m{this.ReaderAfters.Count}", whereObj);

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
        var commandInitializer = RepositoryHelper.BuildGetSqlParameters(this.Connection, this.OrmProvider, this.MapProvider, entityType, whereObj);
        var sql = commandInitializer.Invoke(this.Command, this.OrmProvider, this.MapProvider, $"m{this.ReaderAfters.Count}", whereObj);

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
        var commandInitializer = RepositoryHelper.BuildExistsSqlParameters(this.Connection, this.OrmProvider, this.MapProvider, entityType, whereObj);
        var sql = commandInitializer.Invoke(this.Command, this.OrmProvider, this.MapProvider, $"m{this.ReaderAfters.Count}", whereObj);
        Func<IDataReader, object> readerGetter = reader => reader.To<int>() > 0;
        this.AddReader(sql, readerGetter);
        return this;
    }
    public IMultipleQuery Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate)
    {
        if (wherePredicate == null)
            throw new ArgumentNullException(nameof(wherePredicate));

        var sql = this.From<TEntity>().Where(wherePredicate)
            .Select(f => Sql.Count()).ToSql(out var dbParameters);
        Func<IDataReader, object> readerGetter = reader => reader.To<int>() > 0;
        this.AddReader(sql, readerGetter, dbParameters);
        return this;
    }
    public IMultipleQuery Execute(string rawSql)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        Func<IDataReader, object> readerGetter = reader => reader.To<int>();
        this.AddReader(rawSql, readerGetter);
        return this;
    }
    public IMultipleQuery Execute(string rawSql, object parameters)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.Connection, this.OrmProvider, rawSql, parameters);
        commandInitializer.Invoke(this.Command, this.OrmProvider, parameters);

        Func<IDataReader, object> readerGetter = reader => reader.To<int>();
        this.AddReader(rawSql, readerGetter);
        return this;
    }
    #endregion

    #region AddReader/BuildSql
    public void AddReader(string sql, Func<IDataReader, object> readerGetter, List<IDbDataParameter> dbParameters = null, IQueryVisitor queryVisitor = null, int pageIndex = 0, int pageSize = 0)
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
        if (dbParameters != null && dbParameters.Count > 0)
        {
            dbParameters.ForEach(f =>
            {
                if (this.Command.Parameters.Contains(f.ParameterName)
                    && this.Command.Parameters[f.ParameterName] is IDbDataParameter dbParameter
                    && dbParameter.Value != f.Value)
                    throw new Exception($"名为{f.ParameterName}的参数已存在并与当前参数值不同，Value1:{dbParameter.Value},Value2:{f.Value}");
                this.Command.Parameters.Add(f);
            });
        }
    }
    public string BuildSql()
    {
        var sql = this.sqlBuilder.ToString();
        this.sqlBuilder.Clear();
        return sql;
    }
    #endregion
}
public class ReaderAfter
{
    /// <summary>
    /// 明确的类型可以使用dynamic类型
    /// </summary>
    public bool IsTyped { get; set; } = true;
    public Func<IDataReader, object> ReaderGetter { get; set; }
    public IQueryVisitor QueryVisitor { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}