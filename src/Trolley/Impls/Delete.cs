using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class Delete<TEntity> : IDelete<TEntity>
{
    #region Fields
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly bool isParameterized;
    #endregion

    #region Constructor
    public Delete(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.isParameterized = isParameterized;
    }
    #endregion

    #region Where
    public IDeleted<TEntity> Where(object keys)
    {
        if (keys == null)
            throw new ArgumentNullException(nameof(keys));

        var visitor = this.ormProvider.NewDeleteVisitor(this.connection.DbKey, this.mapProvider, this.isParameterized);
        visitor.Initialize(typeof(TEntity));
        return new Deleted<TEntity>(this.connection, this.transaction, this.ormProvider, this.mapProvider);
    }
    public IDeleting<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public IDeleting<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        var visitor = this.ormProvider.NewDeleteVisitor(this.connection.DbKey, this.mapProvider, this.isParameterized);
        visitor.Initialize(typeof(TEntity));
        if (condition) visitor.Where(ifPredicate);
        else if (elsePredicate != null) visitor.Where(elsePredicate);
        return new Deleting<TEntity>(this.connection, this.transaction, visitor);
    }
    #endregion
}
class Deleted<TEntity> : IDeleted<TEntity>
{
    #region Fields
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private bool isBulk = false;
    private object parameters = null;
    #endregion

    #region Constructor
    public Deleted(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, object parameters)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.parameters = parameters;
    }
    #endregion

    #region WhereKeys
    public void Where(object keys)
    {
        string sql = null;
        var entityType = typeof(TEntity);
        this. isBulk = this.parameters is IEnumerable && this.parameters is not string && this.parameters is not IDictionary<string, object>;
       
       
    } 
    #endregion

    #region Execute
    public int Execute()
    {
        string sql = null;
        var entityType = typeof(TEntity);
        bool isBulk = this.parameters is IEnumerable && this.parameters is not string && this.parameters is not IDictionary<string, object>;
        using var command = this.connection.CreateCommand();
        if (isBulk)
        {
            var commandInitializer = RepositoryHelper.BuildDeleteBatchCommandInitializer(this.connection,
                 this.ormProvider, this.mapProvider, entityType, this.parameters, out var isNeedEndParenthesis);
            int index = 0;
            var sqlBuilder = new StringBuilder();
            var entities = this.parameters as IEnumerable;
            foreach (var entity in entities)
            {
                commandInitializer.Invoke(command, this.ormProvider, this.mapProvider, sqlBuilder, index, entity);
                index++;
            }
            if (isNeedEndParenthesis) sqlBuilder.Append(')');
            sql = sqlBuilder.ToString();
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildDeleteCommandInitializer(
               this.connection, this.ormProvider, this.mapProvider, entityType, parameters);
            sql = commandInitializer.Invoke(command, this.ormProvider, this.parameters);
        }
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;
        this.connection.Open();
        var result = command.ExecuteNonQuery();
        command.Dispose();
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        string sql = null;
        var entityType = typeof(TEntity);
        bool isBulk = this.parameters is IEnumerable && this.parameters is not string && this.parameters is not IDictionary<string, object>;
        using var cmd = this.connection.CreateCommand();
        if (isBulk)
        {
            var commandInitializer = RepositoryHelper.BuildDeleteBatchCommandInitializer(this.connection,
                this.ormProvider, this.mapProvider, entityType, this.parameters, out var isNeedEndParenthesis);
            int index = 0;
            var sqlBuilder = new StringBuilder();
            var entities = this.parameters as IEnumerable;
            foreach (var entity in entities)
            {
                commandInitializer.Invoke(cmd, this.ormProvider, this.mapProvider, sqlBuilder, index, entity);
                index++;
            }
            if (isNeedEndParenthesis) sqlBuilder.Append(')');
            sql = sqlBuilder.ToString();
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildDeleteCommandInitializer(
               this.connection, this.ormProvider, this.mapProvider, entityType, parameters);
            sql = commandInitializer.Invoke(cmd, this.ormProvider, this.parameters);
        }

        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        command.Dispose();
        return result;
    }
    #endregion

    #region ToMultipleCommand
    public MultipleCommand ToMultipleCommand() => this.visitor.CreateMultipleCommand();
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        string sql = null;
        var entityType = typeof(TEntity);
        bool isBulk = this.parameters is IEnumerable && this.parameters is not string && this.parameters is not IDictionary<string, object>;
        using var command = this.connection.CreateCommand();
        if (isBulk)
        {
            var commandInitializer = RepositoryHelper.BuildDeleteBatchCommandInitializer(this.connection,
                 this.ormProvider, this.mapProvider, entityType, this.parameters, out var isNeedEndParenthesis);
            int index = 0;
            var sqlBuilder = new StringBuilder();
            var entities = this.parameters as IEnumerable;
            foreach (var entity in entities)
            {
                commandInitializer.Invoke(command, this.ormProvider, this.mapProvider, sqlBuilder, index, entity);
                index++;
            }
            if (isNeedEndParenthesis) sqlBuilder.Append(')');
            sql = sqlBuilder.ToString();
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildDeleteCommandInitializer(
               this.connection, this.ormProvider, this.mapProvider, entityType, parameters);
            sql = commandInitializer.Invoke(command, this.ormProvider, this.parameters);
        }
        if (command.Parameters != null && command.Parameters.Count > 0)
            dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        return sql;
    }
    #endregion
}
class Deleting<TEntity> : IDeleting<TEntity>
{
    #region Fields
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly IDeleteVisitor visitor;
    #endregion

    #region Constructor
    public Deleting(TheaConnection connection, IDbTransaction transaction, IDeleteVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    #endregion

    #region And
    public IDeleting<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public IDeleting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition) this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region Execute
    public int Execute()
    {
        var sql = this.visitor.BuildSql(out var dbParameters);
        using var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));
        this.connection.Open();
        var result = command.ExecuteNonQuery();
        command.Dispose();
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var sql = this.visitor.BuildSql(out var dbParameters);
        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => cmd.Parameters.Add(f));
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        await command.DisposeAsync();
        return result;
    }
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters) => this.visitor.BuildSql(out dbParameters);
    #endregion
}