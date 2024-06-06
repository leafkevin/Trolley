using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class MultipleQuery : IMultipleQuery, IDisposable
{
    #region Fields
    protected StringBuilder sqlBuilder = new();
    #endregion

    #region Properties
    public DbContext DbContext { get; protected set; }
    public string DbKey => this.DbContext.DbKey;
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    public IEntityMapProvider MapProvider => this.DbContext.MapProvider;
    public IDbCommand Command { get; private set; }
    public List<ReaderAfter> ReaderAfters { get; private set; }
    #endregion

    #region Constructor
    public MultipleQuery(DbContext dbContext, IDbCommand command)
    {
        this.DbContext = dbContext;
        this.Command = command;
        this.ReaderAfters = new();
    }
    #endregion

    #region From
    public IMultiQuery<T> From<T>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T));
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
        visitor.From(typeof(T), this.DbContext, subQuery);
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
            readerGetter = reader => reader.To<TEntity>(this.DbContext);
        else readerGetter = reader => reader.To<TEntity>(this.OrmProvider);
        this.AddReader(targetType, rawSql, readerGetter);
        return this;
    }
    public IMultipleQuery QueryFirst<TEntity>(string rawSql, object parameters)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
        commandInitializer.Invoke(this.Command.Parameters, this.OrmProvider, parameters);

        var targetType = typeof(TEntity);
        Func<IDataReader, object> readerGetter;
        if (targetType.IsEntityType(out _))
            readerGetter = reader => reader.To<TEntity>(this.DbContext);
        else readerGetter = reader => reader.To<TEntity>(this.OrmProvider);
        this.AddReader(targetType, rawSql, readerGetter);
        return this;
    }
    public IMultipleQuery QueryFirst<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var targetType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.OrmProvider, this.MapProvider, targetType, whereObj, true);
        var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string, string>;
        var sql = typedCommandInitializer.Invoke(this.Command.Parameters, this.OrmProvider, whereObj, $"_m{this.ReaderAfters.Count}");

        Func<IDataReader, object> readerGetter = reader => reader.To<TEntity>(this.DbContext);
        this.AddReader(targetType, sql, readerGetter);
        return this;
    }
    public IMultipleQuery Query<TEntity>(string rawSql)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        var targetType = typeof(TEntity);
        Func<IDataReader, object> readerGetter;
        if (targetType.IsEntityType(out _))
            readerGetter = reader => reader.To<TEntity>(this.DbContext);
        else readerGetter = reader => reader.To<TEntity>(this.OrmProvider);
        this.AddReader(targetType, rawSql, readerGetter);
        return this;
    }
    public IMultipleQuery Query<TEntity>(string rawSql, object parameters)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
        commandInitializer.Invoke(this.Command.Parameters, this.OrmProvider, parameters);

        var targetType = typeof(TEntity);
        Func<IDataReader, object> readerGetter;
        if (targetType.IsEntityType(out _))
            readerGetter = reader => reader.To<TEntity>(this.DbContext);
        else readerGetter = reader => reader.To<TEntity>(this.OrmProvider);
        this.AddReader(targetType, rawSql, readerGetter);
        return this;
    }
    public IMultipleQuery Query<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var targetType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.OrmProvider, this.MapProvider, targetType, whereObj, true);
        var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string, string>;
        var sql = typedCommandInitializer.Invoke(this.Command.Parameters, this.OrmProvider, whereObj, $"_m{this.ReaderAfters.Count}");

        Func<IDataReader, object> readerGetter = reader => reader.To<TEntity>(this.DbContext);
        this.AddReader(targetType, sql, readerGetter);
        return this;
    }
    #endregion

    #region Get
    public IMultipleQuery Get<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var targetType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildGetSqlParameters(this.OrmProvider, this.MapProvider, targetType, whereObj, true);
        var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string, string>;
        var sql = typedCommandInitializer.Invoke(this.Command.Parameters, this.OrmProvider, whereObj, $"_m{this.ReaderAfters.Count}");

        Func<IDataReader, object> readerGetter = reader => reader.To<TEntity>(this.DbContext);
        this.AddReader(targetType, sql, readerGetter);
        return this;
    }
    #endregion

    #region Exists
    public IMultipleQuery Exists<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildExistsSqlParameters(this.OrmProvider, this.MapProvider, entityType, whereObj, true);
        var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string, string>;
        var sql = typedCommandInitializer.Invoke(this.Command.Parameters, this.OrmProvider, whereObj, $"_m{this.ReaderAfters.Count}");

        Func<IDataReader, object> readerGetter = reader => reader.To<int>(this.OrmProvider) > 0;
        this.AddReader(typeof(int), sql, readerGetter);
        return this;
    }
    public IMultipleQuery Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate)
    {
        if (wherePredicate == null)
            throw new ArgumentNullException(nameof(wherePredicate));

        var sql = this.From<TEntity>().Where(wherePredicate)
            .Select(f => Sql.Count()).ToSql(out _);
        Func<IDataReader, object> readerGetter = reader => reader.To<int>(this.OrmProvider) > 0;
        this.AddReader(typeof(int), sql, readerGetter);
        return this;
    }
    #endregion

    #region AddReader/BuildSql
    public void AddReader(Type targetType, string sql, Func<IDataReader, object> readerGetter, IQueryVisitor queryVisitor = null, int pageNumber = 0, int pageSize = 0)
    {
        if (this.sqlBuilder.Length > 0)
            this.sqlBuilder.Append(';');
        this.sqlBuilder.Append(sql);
        this.ReaderAfters.Add(new ReaderAfter
        {
            TargetType = targetType,
            ReaderGetter = readerGetter,
            QueryVisitor = queryVisitor,
            PageNumber = pageNumber,
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
        this.sqlBuilder = null;
        this.ReaderAfters = null;
        this.Command = null;
    }
    #endregion

    private IQueryVisitor CreateQueryVisitor(char tableAsStart)
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.DbContext.ShardingProvider, this.DbContext.IsParameterized, tableAsStart);
        visitor.DbParameters = this.Command.Parameters;
        return visitor;
    }
}
public class ReaderAfter
{
    public Type TargetType { get; set; }
    public Func<IDataReader, object> ReaderGetter { get; set; }
    public IQueryVisitor QueryVisitor { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
