using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

class MultiDelete<TEntity> : IMultiDelete<TEntity>
{
    #region Fields
    private readonly MultipleQuery multiQuery;
    private readonly TheaConnection connection;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly bool isParameterized;
    #endregion

    #region Constructor
    public MultiDelete(MultipleQuery multiQuery)
    {
        this.multiQuery = multiQuery;
        this.connection = multiQuery.Connection;
        this.ormProvider = multiQuery.OrmProvider;
        this.mapProvider = multiQuery.MapProvider;
        this.isParameterized = multiQuery.IsParameterized;
    }
    #endregion

    #region Where
    public IMultiDeleted<TEntity> Where(object keys)
    {
        if (keys == null)
            throw new ArgumentNullException(nameof(keys));

        return new MultiDeleted<TEntity>(this.multiQuery, keys);
    }
    public IMultiDeleting<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var visitor = this.ormProvider.NewDeleteVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        visitor.Where(predicate);
        return new MultiDeleting<TEntity>(this.multiQuery, visitor);
    }
    public IMultiDeleting<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        var visitor = this.ormProvider.NewDeleteVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        if (condition) visitor.Where(ifPredicate);
        else if (elsePredicate != null) visitor.Where(elsePredicate);
        return new MultiDeleting<TEntity>(this.multiQuery, visitor);
    }
    #endregion
}
class MultiDeleted<TEntity> : IMultiDeleted<TEntity>
{
    #region Fields
    private readonly MultipleQuery multiQuery;
    private readonly TheaConnection connection;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly IDbCommand command;
    private object parameters = null;
    #endregion

    #region Constructor
    public MultiDeleted(MultipleQuery multiQuery, object parameters)
    {
        this.multiQuery = multiQuery;
        this.connection = multiQuery.Connection;
        this.ormProvider = multiQuery.OrmProvider;
        this.mapProvider = multiQuery.MapProvider;
        this.command = multiQuery.Command;
        this.parameters = parameters;
    }
    #endregion

    #region Execute
    public IMultipleQuery Execute()
    {
        string sql = null;
        var entityType = typeof(TEntity);
        bool isMulti = this.parameters is IEnumerable && this.parameters is not string && this.parameters is not Dictionary<string, object>;
        if (isMulti)
        {
            var commandInitializer = RepositoryHelper.BuildDeleteBatchCommandInitializer(this.connection,
                this.ormProvider, this.mapProvider, entityType, parameters, out var isNeedEndParenthesis);
            int index = 0;
            var sqlBuilder = new StringBuilder();
            var entities = this.parameters as IEnumerable;
            foreach (var entity in entities)
            {
                commandInitializer.Invoke(this.command, this.ormProvider, this.mapProvider, sqlBuilder, index, entity);
                index++;
            }
            if (isNeedEndParenthesis) sqlBuilder.Append(')');
            sql = sqlBuilder.ToString();
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildDeleteCommandInitializer(
                this.connection, this.ormProvider, this.mapProvider, entityType, parameters);
            sql = commandInitializer.Invoke(this.command, this.ormProvider, this.parameters);
        }
        Func<IDataReader, object> readerGetter = reader => reader.To<int>();
        this.multiQuery.AddReader(sql, readerGetter);
        return this.multiQuery;
    }
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        string sql = null;
        var entityType = typeof(TEntity);
        bool isMulti = this.parameters is IEnumerable && this.parameters is not string && this.parameters is not Dictionary<string, object>;
        using var sqlCommand = this.connection.CreateCommand();
        if (isMulti)
        {
            var commandInitializer = RepositoryHelper.BuildDeleteBatchCommandInitializer(this.connection,
                this.ormProvider, this.mapProvider, entityType, parameters, out var isNeedEndParenthesis);
            int index = 0;
            var sqlBuilder = new StringBuilder();
            var entities = this.parameters as IEnumerable;
            foreach (var entity in entities)
            {
                commandInitializer.Invoke(sqlCommand, this.ormProvider, this.mapProvider, sqlBuilder, index, entity);
                index++;
            }
            if (isNeedEndParenthesis) sqlBuilder.Append(')');
            sql = sqlBuilder.ToString();
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildDeleteCommandInitializer(
                this.connection, this.ormProvider, this.mapProvider, entityType, parameters);
            sql = commandInitializer.Invoke(sqlCommand, this.ormProvider, this.parameters);
        }
        if (sqlCommand.Parameters != null && sqlCommand.Parameters.Count > 0)
            dbParameters = sqlCommand.Parameters.Cast<IDbDataParameter>().ToList();
        sqlCommand.Dispose();
        return sql;
    }
    #endregion
}
class MultiDeleting<TEntity> : IMultiDeleting<TEntity>
{
    #region Fields
    private readonly MultipleQuery multiQuery;
    private readonly IDeleteVisitor visitor;
    #endregion

    #region Constructor
    public MultiDeleting(MultipleQuery multiQuery, IDeleteVisitor visitor)
    {
        this.multiQuery = multiQuery;
        this.visitor = visitor;
    }
    #endregion

    #region And
    public IMultiDeleting<TEntity> And(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IMultiDeleting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition) this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region Execute
    public IMultipleQuery Execute()
    {
        var sql = this.visitor.BuildSql(out var dbParameters);
        Func<IDataReader, object> readerGetter = reader => reader.To<int>();
        this.multiQuery.AddReader(sql, readerGetter, dbParameters);
        return this.multiQuery;
    }
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters);
    #endregion
}