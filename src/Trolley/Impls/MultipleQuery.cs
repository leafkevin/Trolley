using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class MultipleQuery : DialectProvider, IMultipleQuery
{
    #region Fields
    protected StringBuilder sqlBuilder = new();
    #endregion

    #region Properties
    public ITheaCommand Command { get; set; }
    public IDataParameterCollection DbParameters { get; set; }
    public List<ReaderAfter> ReaderAfters { get; private set; }
    public bool IsNeedClose { get; private set; }
    #endregion

    #region Constructor
    public MultipleQuery(DbContext dbContext)
    {
        this.DbContext = dbContext;
        this.ReaderAfters = new();
        (this.IsNeedClose, var connection, var command) = this.UseSlaveCommand();
        this.DbContext.Connection = connection;
        this.Command = command;
        this.DbParameters = this.Command.Parameters;
    }
    #endregion

    #region GetShardingTableName
    public virtual IMultipleQuery GetShardingTableName<TEntity>(params object[] fieldValues)
    {
        this.GetShardingTable(typeof(TEntity), fieldValues);
        return this;
    }
    #endregion

    #region From
    public virtual IMultiQuery<T> From<T>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T));
        return this.ormProvider.NewMultiQuery<T>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2));
        return this.ormProvider.NewMultiQuery<T1, T2>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3));
        return this.ormProvider.NewMultiQuery<T1, T2, T3>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return this.ormProvider.NewMultiQuery<T1, T2, T3, T4>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return this.ormProvider.NewMultiQuery<T1, T2, T3, T4, T5>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return this.ormProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return this.ormProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return this.ormProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return this.ormProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this, visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return this.ormProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, visitor);
    }
    #endregion

    #region FromQuery
    public virtual IMultiQuery<T> FromQuery<T>(IQuery<T> subQuery)
    {
        var visitor = this.CreateQueryVisitor();
        visitor.UseQuery(typeof(T), subQuery, false);
        return this.ormProvider.NewMultiQuery<T>(this, visitor);
    }
    public virtual IMultiQuery<T> FromQuery<T>(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        var visitor = this.CreateQueryVisitor();
        visitor.UseNewQuery(typeof(T), subQueryExpr, false);
        return this.ormProvider.NewMultiQuery<T>(this, visitor);
    }
    #endregion

    #region QueryScalar
    public virtual IMultipleQuery QueryScalar<TValue>(string rawSql)
    {
        this.CreateRawReader(rawSql, typeof(TValue), ReaderResultType.Value, false);
        return this;
    }
    public virtual IMultipleQuery QueryScalar<TValue>(string rawSql, object parameters)
    {
        this.CreateRawReader(rawSql, parameters, typeof(TValue), ReaderResultType.Value, false);
        return this;
    }
    public virtual IMultipleQuery QueryScalar<TValue>(string rawSql, List<IDbDataParameter> parameters)
    {
        this.CreateRawParametersReader(rawSql, parameters, typeof(TValue), ReaderResultType.Value, false);
        return this;
    }
    #endregion

    #region QueryById
    public virtual IMultipleQuery QueryById<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        var entityType = typeof(TEntity);
        this.CreateQueryReader(whereObj, entityType, entityType, 1, true, false, ReaderResultType.Entity);
        return this;
    }
    #endregion     

    #region QueryByIds
    public virtual IMultipleQuery QueryByIds<TEntity>(IEnumerable whereObjs)
    {
        if (whereObjs == null)
            throw new ArgumentNullException(nameof(whereObjs));
        var entityType = typeof(TEntity);
        this.CreateQueryReader(whereObjs, entityType, entityType, 1, true, true, ReaderResultType.List);
        return this;
    }
    #endregion

    #region QueryFirst
    public virtual IMultipleQuery QueryFirst<TEntity>(string rawSql)
    {
        this.CreateRawReader(rawSql, typeof(TEntity), ReaderResultType.Entity, false);
        return this;
    }
    public IMultipleQuery QueryFirst<TEntity>(string rawSql, object parameters)
    {
        this.CreateRawReader(rawSql, parameters, typeof(TEntity), ReaderResultType.Entity, false);
        return this;
    }
    public IMultipleQuery QueryFirst<TEntity>(string rawSql, List<IDbDataParameter> parameters)
    {
        this.CreateRawParametersReader(rawSql, parameters, typeof(TEntity), ReaderResultType.Entity, false);
        return this;
    }
    public virtual IMultipleQuery QueryFirst<TEntity>(object whereObj)
    {
        var entityType = typeof(TEntity);
        this.CreateQueryReader(whereObj, entityType, entityType, 1, false, false, ReaderResultType.Entity);
        return this;
    }
    #endregion

    #region Query
    public virtual IMultipleQuery Query<TEntity>(string rawSql)
    {
        this.CreateRawReader(rawSql, typeof(TEntity), ReaderResultType.List, false);
        return this;
    }
    public IMultipleQuery Query<TEntity>(string rawSql, object parameters)
    {
        this.CreateRawReader(rawSql, parameters, typeof(TEntity), ReaderResultType.List, false);
        return this;
    }
    public virtual IMultipleQuery Query<TEntity>(string rawSql, List<IDbDataParameter> parameters)
    {
        this.CreateRawParametersReader(rawSql, parameters, typeof(TEntity), ReaderResultType.List, false);
        return this;
    }
    public virtual IMultipleQuery Query<TEntity>(object whereObj)
    {
        var entityType = typeof(TEntity);
        this.CreateQueryReader(whereObj, entityType, entityType, 1, false, false, ReaderResultType.List);
        return this;
    }
    #endregion

    #region Exists
    public virtual IMultipleQuery ExistsBy<TEntity>(object whereObj)
    {
        this.CreateQueryReader(whereObj, typeof(TEntity), typeof(bool), 2, false, false, ReaderResultType.Value);
        return this;
    }
    public virtual IMultipleQuery ExistsById<TEntity>(object whereKey)
    {
        if (whereKey == null)
            throw new ArgumentNullException(nameof(whereKey));
        this.CreateQueryReader(whereKey, typeof(TEntity), typeof(bool), 2, true, false, ReaderResultType.Value);
        return this;
    }
    public virtual IMultipleQuery ExistsByIds<TEntity>(IEnumerable whereKeys)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));
        this.CreateQueryReader(whereKeys, typeof(TEntity), typeof(bool), 2, true, true, ReaderResultType.List);
        return this;
    }
    public virtual IMultipleQuery Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate)
    {
        if (wherePredicate == null)
            throw new ArgumentNullException(nameof(wherePredicate));
        var sql = this.From<TEntity>().Where(wherePredicate)
            .Select<int>("1").Take(1).ToSql(out _);
        this.AddReader(typeof(bool), sql, ReaderResultType.Value, true);
        return this;
    }
    #endregion

    #region AddReader/BuildSql
    public virtual void AddReader(Type targetType, string sql, ReaderResultType resultType, bool isExists = false, IQueryVisitor visitor = null)
    {
        if (this.sqlBuilder.Length > 0)
            this.sqlBuilder.Append(';');
        this.sqlBuilder.Append(sql);
        this.ReaderAfters.Add(new ReaderAfter
        {
            TargetType = targetType,
            Visitor = visitor,
            ResultType = resultType,
            IsExists = isExists
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
        => this.ormProvider.NewQueryVisitor(this.DbContext, tableAsStart, this.Command);
    private void CreateRawReader(string rawSql, Type targetType, ReaderResultType resultType, bool isExists)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        this.AddReader(targetType, rawSql, resultType, isExists);
    }
    private void CreateRawReader(string rawSql, object parameters, Type targetType, ReaderResultType resultType, bool isExists)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));
        var whereObjType = parameters.GetType();
        if (!whereObjType.IsEntityType(out _))
            throw new NotSupportedException("不支持的参数类型，此方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");
        var commandInitializer = RepositoryHelper.BuildRawSqlCommandInitializer(this.ormProvider, rawSql, parameters);
        commandInitializer.Invoke(this.Command.Parameters, this.ormProvider, parameters);
        this.AddReader(targetType, rawSql, resultType, isExists);
    }
    private void CreateRawParametersReader(string rawSql, List<IDbDataParameter> parameters, Type targetType, ReaderResultType resultType, bool isExists)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));
        parameters.ForEach(f => this.Command.Parameters.Add(f));
        this.AddReader(targetType, rawSql, resultType, isExists);
    }
    private void CreateQueryReader(object whereObjs, Type entityType, Type targetType, int commandType, bool isUseKey, bool isBulk, ReaderResultType resultType)
    {
        var commandInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, whereObjs, commandType, isUseKey, true, isBulk);
        var sql = commandInitializer.Invoke(this.Command.Parameters, this.DbContext, whereObjs);
        this.AddReader(targetType, sql, resultType, commandType == 2);
    }
    #endregion
}