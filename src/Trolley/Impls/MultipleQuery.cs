using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class MultipleQuery : IMultipleQuery
{
    #region Fields
    protected StringBuilder sqlBuilder = new();
    #endregion

    #region Properties
    public DbContext DbContext { get; protected set; }
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

    #region GetShardingTableNames
    public virtual IMultipleQuery GetShardingTableNames<TEntity>(Func<string, bool> tableNameSelector, string tableSchema = null) => this;
    #endregion

    #region From
    public virtual IMultiQuery<T> From<T>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T));
        return this.OrmProvider.NewMultiQuery<T>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2));
        return this.OrmProvider.NewMultiQuery<T1, T2>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, visitor);
    }
    #endregion

    #region FromQuery
    public virtual IMultiQuery<T> FromQuery<T>(IQuery<T> subQuery)
    {
        var visitor = this.CreateQueryVisitor();
        visitor.UseQuery(typeof(T), subQuery, false);
        return this.OrmProvider.NewMultiQuery<T>(this, visitor);
    }
    public virtual IMultiQuery<T> FromQuery<T>(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        var visitor = this.CreateQueryVisitor();
        visitor.UseNewQuery(typeof(T), subQueryExpr, true);
        return this.OrmProvider.NewMultiQuery<T>(this, visitor);
    }
    #endregion

    #region QueryScalar
    public virtual IMultipleQuery QueryScalar<TValue>(string rawSql)
    {
        this.CreateReader(rawSql, typeof(TValue), false);
        return this;
    }
    public virtual IMultipleQuery QueryScalar<TValue>(string rawSql, object parameters)
    {
        this.CreateReader(rawSql, parameters, typeof(TValue), false);
        return this;
    }
    public virtual IMultipleQuery QueryScalar<TValue>(string rawSql, List<IDbDataParameter> parameters)
    {
        this.CreateReader(rawSql, parameters, typeof(TValue), false);
        return this;
    }
    #endregion

    #region GetById
    public virtual IMultipleQuery GetById<TEntity>(object whereObj)
    {
        this.CreateReader(whereObj, typeof(TEntity), false);
        return this;
    }
    #endregion

    #region GetByIds
    public virtual IMultipleQuery GetByIds<TEntity>(IEnumerable whereObjs)
    {
        if (whereObjs == null)
            throw new ArgumentNullException(nameof(whereObjs));
        var entityType = typeof(TEntity);
        bool isEmpty = true;
        foreach (var whereObj in whereObjs)
        {
            isEmpty = false;
            break;
        }
        if (isEmpty) throw new Exception("多主键ID查询，whereObjs参数至少要有一条数据");

        (var isInExpr, var headSql, var commandInitializer) = ((bool, string, object))RepositoryHelper.BuildQueryWhereObjByKeySqlParameters(this.DbContext, entityType, whereObjs, true, true);
        var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
        int index = 0;
        var builder = new StringBuilder(headSql);
        var jointMark = isInExpr ? "," : " OR ";
        foreach (var whereObj in whereObjs)
        {
            if (index > 0) builder.Append(jointMark);
            typedCommandInitializer.Invoke(this.Command.Parameters, builder, this.DbContext, whereObj, $"_m{this.ReaderAfters.Count}{index}");
            index++;
        }
        if (isInExpr) builder.Append(')');
        var sql = builder.ToString();
        this.AddReader(entityType, sql, false);
        return this;
    }
    #endregion

    #region QueryFirst
    public virtual IMultipleQuery QueryFirst<TEntity>(string rawSql)
    {
        this.CreateReader(rawSql, typeof(TEntity), true);
        return this;
    }
    public IMultipleQuery QueryFirst<TEntity>(string rawSql, object parameters)
    {
        this.CreateReader(rawSql, parameters, typeof(TEntity), true);
        return this;
    }
    public IMultipleQuery QueryFirst<TEntity>(string rawSql, List<IDbDataParameter> parameters)
    {
        this.CreateReader(rawSql, parameters, typeof(TEntity), true);
        return this;
    }
    public virtual IMultipleQuery QueryFirst<TEntity>(object whereObj)
    {
        this.CreateReader(whereObj, typeof(TEntity), true);
        return this;
    }
    #endregion

    #region Query
    public virtual IMultipleQuery Query<TEntity>(string rawSql)
    {
        this.CreateReader(rawSql, typeof(TEntity), false);
        return this;
    }
    public IMultipleQuery Query<TEntity>(string rawSql, object parameters)
    {
        this.CreateReader(rawSql, parameters, typeof(TEntity), false);
        return this;
    }
    public virtual IMultipleQuery Query<TEntity>(string rawSql, List<IDbDataParameter> parameters)
    {
        this.CreateReader(rawSql, parameters, typeof(TEntity), false);
        return this;
    }
    public virtual IMultipleQuery Query<TEntity>(object whereObj)
    {
        this.CreateReader(whereObj, typeof(TEntity), false);
        return this;
    }
    #endregion

    #region Exists
    public virtual IMultipleQuery Exists<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        bool isBulk = whereObj is IEnumerable && whereObj is not string && whereObj is not IDictionary<string, object>;
        var commandInitializer = RepositoryHelper.BuildExistsSqlParameters(this.DbContext, entityType, whereObj, true, isBulk);
        var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string, string>;
        var sql = typedCommandInitializer.Invoke(this.Command.Parameters, this.DbContext, whereObj, $"_m{this.ReaderAfters.Count}");
        this.AddReader(typeof(bool), sql, false);
        return this;
    }
    public virtual IMultipleQuery Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate)
    {
        if (wherePredicate == null)
            throw new ArgumentNullException(nameof(wherePredicate));

        var sql = this.From<TEntity>().Where(wherePredicate)
            .SelectAggregate((x, f) => x.Count()).ToSql(out _);
        this.AddReader(typeof(bool), sql, false);
        return this;
    }
    #endregion

    #region AddReader/BuildSql
    public virtual void AddReader(Type targetType, string sql, bool isSingle, IQueryVisitor queryVisitor = null, int pageNumber = 0, int pageSize = 0)
    {
        if (this.sqlBuilder.Length > 0)
            this.sqlBuilder.Append(';');
        this.sqlBuilder.Append(sql);
        this.ReaderAfters.Add(new ReaderAfter
        {
            TargetType = targetType,
            QueryVisitor = queryVisitor,
            IsSingle = isSingle,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }
    public virtual string BuildSql(out List<ReaderAfter> readerAfters)
    {
        var sql = this.sqlBuilder.ToString();
        this.sqlBuilder.Clear();
        readerAfters = this.ReaderAfters;
        return sql;
    }
    #endregion

    #region Dispose
    public virtual void Dispose()
    {
        this.sqlBuilder = null;
        this.ReaderAfters = null;
        this.Command = null;
    }
    #endregion

    #region Others
    private IQueryVisitor CreateQueryVisitor(char tableAsStart = 'a')
        => this.OrmProvider.NewQueryVisitor(this.DbContext, tableAsStart, this.Command.Parameters);
    private void CreateReader(string rawSql, Type targetType, bool isSingle)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        this.AddReader(targetType, rawSql, isSingle);
    }
    private void CreateReader(string rawSql, object parameters, Type targetType, bool isSingle)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));
        var whereObjType = parameters.GetType();
        if (!whereObjType.IsEntityType(out _))
            throw new NotSupportedException("不支持的参数类型，此方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");
        var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
        commandInitializer.Invoke(this.Command.Parameters, this.OrmProvider, parameters);
        this.AddReader(targetType, rawSql, isSingle);
    }
    private void CreateReader(string rawSql, List<IDbDataParameter> parameters, Type targetType, bool isSingle)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));
        parameters.ForEach(f => this.Command.Parameters.Add(f));
        this.AddReader(targetType, rawSql, isSingle);
    }
    private void CreateReader(object whereObj, Type entityType, bool isSingle)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        bool isBulk = whereObj is IEnumerable && whereObj is not string && whereObj is not IDictionary<string, object>;
        var commandInitializer = RepositoryHelper.BuildQueryWhereObjByKeySqlParameters(this.DbContext, entityType, whereObj, true, isBulk);
        var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string, string>;
        var sql = typedCommandInitializer.Invoke(this.Command.Parameters, this.DbContext, whereObj, $"_m{this.ReaderAfters.Count}");
        this.AddReader(entityType, sql, isSingle);
    }
    #endregion
}