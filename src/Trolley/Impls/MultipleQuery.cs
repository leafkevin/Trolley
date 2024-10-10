using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class MultipleQuery : IMultipleQuery, IDisposable
{
    #region Fields
    protected internal bool isUseMaster = false;
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
    public MultipleQuery(DbContext dbContext)
    {
        this.DbContext = dbContext;
        this.ReaderAfters = new();
        this.Command = this.OrmProvider.CreateCommand();
    }
    #endregion

    #region UseMaster
    public IMultipleQuery UseMaster(bool isUseMaster = true)
    {
        this.isUseMaster = isUseMaster;
        return this;
    }
    #endregion

    #region From
    public IMultiQuery<T> From<T>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T));
        return this.OrmProvider.NewMultiQuery<T>(this, visitor);
    }
    public IMultiQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2));
        return this.OrmProvider.NewMultiQuery<T1, T2>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, visitor);
    }
    #endregion

    #region From SubQuery
    public IMultiQuery<T> From<T>(Func<IFromQuery, IQuery<T>> subQuery, char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(typeof(T), this.DbContext, subQuery);
        return this.OrmProvider.NewMultiQuery<T>(this, visitor);
    }
    #endregion

    #region QueryFirst/Query
    public IMultipleQuery QueryFirst<TEntity>(string rawSql)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        var targetType = typeof(TEntity);
        Func<ITheaDataReader, object> readerGetter;
        if (targetType.IsEntityType(out _))
            readerGetter = reader => reader.ToEntity<TEntity>(this.DbContext);
        else readerGetter = reader => reader.ToValue<TEntity>(this.DbContext);
        this.AddReader(targetType, rawSql, readerGetter, true);
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
        Func<ITheaDataReader, object> readerGetter;
        if (targetType.IsEntityType(out _))
            readerGetter = reader => reader.ToEntity<TEntity>(this.DbContext);
        else readerGetter = reader => reader.ToValue<TEntity>(this.DbContext);
        this.AddReader(targetType, rawSql, readerGetter, true);
        return this;
    }
    public IMultipleQuery QueryFirst<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var targetType = typeof(TEntity);
        var whereObjType = whereObj.GetType();
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbContext, targetType, whereObjType, true);
        var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string, string>;
        var sql = typedCommandInitializer.Invoke(this.Command.Parameters, this.DbContext, whereObj, $"_m{this.ReaderAfters.Count}");

        Func<ITheaDataReader, object> readerGetter = reader => reader.ToEntity<TEntity>(this.DbContext);
        this.AddReader(targetType, sql, readerGetter, true);
        return this;
    }
    public IMultipleQuery Query<TEntity>(string rawSql)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        var targetType = typeof(TEntity);
        Func<ITheaDataReader, object> readerGetter;
        if (targetType.IsEntityType(out _))
            readerGetter = reader => reader.ToEntity<TEntity>(this.DbContext);
        else readerGetter = reader => reader.ToValue<TEntity>(this.DbContext);
        this.AddReader(targetType, rawSql, readerGetter, false);
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
        Func<ITheaDataReader, object> readerGetter;
        if (targetType.IsEntityType(out _))
            readerGetter = reader => reader.ToEntity<TEntity>(this.DbContext);
        else readerGetter = reader => reader.ToValue<TEntity>(this.DbContext);
        this.AddReader(targetType, rawSql, readerGetter, false);
        return this;
    }
    public IMultipleQuery Query<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var targetType = typeof(TEntity);
        var whereObjType = whereObj.GetType();
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbContext, targetType, whereObjType, true);
        var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string, string>;
        var sql = typedCommandInitializer.Invoke(this.Command.Parameters, this.DbContext, whereObj, $"_m{this.ReaderAfters.Count}");

        Func<ITheaDataReader, object> readerGetter = reader => reader.ToEntity<TEntity>(this.DbContext);
        this.AddReader(targetType, sql, readerGetter, false);
        return this;
    }
    #endregion

    #region Get
    public IMultipleQuery Get<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var targetType = typeof(TEntity);
        var whereObjType = whereObj.GetType();
        var commandInitializer = RepositoryHelper.BuildGetSqlParameters(this.DbContext, targetType, whereObjType, true);
        var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string, string>;
        var sql = typedCommandInitializer.Invoke(this.Command.Parameters, this.DbContext, whereObj, $"_m{this.ReaderAfters.Count}");

        Func<ITheaDataReader, object> readerGetter = reader => reader.ToEntity<TEntity>(this.DbContext);
        this.AddReader(targetType, sql, readerGetter, false);
        return this;
    }
    #endregion

    #region Exists
    public IMultipleQuery Exists<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        var whereObjType = whereObj.GetType();
        var commandInitializer = RepositoryHelper.BuildExistsSqlParameters(this.DbContext, entityType, whereObjType, true);
        var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string, string>;
        var sql = typedCommandInitializer.Invoke(this.Command.Parameters, this.DbContext, whereObj, $"_m{this.ReaderAfters.Count}");

        Func<ITheaDataReader, object> readerGetter = reader => reader.ToValue<int>(this.DbContext) > 0;
        this.AddReader(typeof(int), sql, readerGetter, true);
        return this;
    }
    public IMultipleQuery Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate = null)
    {
        if (wherePredicate == null)
            throw new ArgumentNullException(nameof(wherePredicate));

        var sql = this.From<TEntity>().Where(wherePredicate)
            .Select(f => Sql.Count()).ToSql(out _);

        if (wherePredicate != null)
            sql = this.From<TEntity>().Where(wherePredicate).Select(f => Sql.Count()).ToSql(out _);
        else sql = this.From<TEntity>().Select(f => Sql.Count()).ToSql(out _);

        Func<ITheaDataReader, object> readerGetter = reader => reader.ToValue<int>(this.DbContext) > 0;
        this.AddReader(typeof(int), sql, readerGetter, true);
        return this;
    }
    #endregion

    #region AddReader/BuildSql
    public void AddReader(Type targetType, string sql, Func<ITheaDataReader, object> readerGetter, bool isSingle, IQueryVisitor queryVisitor = null, int pageNumber = 0, int pageSize = 0)
    {
        if (this.sqlBuilder.Length > 0)
            this.sqlBuilder.Append(';');
        this.sqlBuilder.Append(sql);
        this.ReaderAfters.Add(new ReaderAfter
        {
            TargetType = targetType,
            ReaderGetter = readerGetter,
            QueryVisitor = queryVisitor,
            IsSingle = isSingle,
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
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbContext);
        visitor.TableAsStart = tableAsStart;
        visitor.DbParameters = this.Command.Parameters;
        return visitor;
    }
}
public class ReaderAfter
{
    public Type TargetType { get; set; }
    public Func<ITheaDataReader, object> ReaderGetter { get; set; }
    public IQueryVisitor QueryVisitor { get; set; }
    public bool IsSingle { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
