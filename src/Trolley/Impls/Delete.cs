using System;
using System.Collections;
using System.Collections.Concurrent;
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
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly bool isParameterized;

    public Delete(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.isParameterized = isParameterized;
    }

    public IDeleted<TEntity> Where(object keys)
    {
        if (keys == null)
            throw new ArgumentNullException(nameof(keys));

        return new Deleted<TEntity>(this.connection, this.transaction, this.ormProvider, this.mapProvider, keys);
    }
    public IDeleting<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var visitor = this.ormProvider.NewDeleteVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        visitor.Where(predicate);
        return new Deleting<TEntity>(this.connection, this.transaction, visitor);
    }
    public IDeleting<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        var visitor = this.ormProvider.NewDeleteVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        if (condition) visitor.Where(ifPredicate);
        else if (elsePredicate != null) visitor.Where(elsePredicate);
        return new Deleting<TEntity>(this.connection, this.transaction, visitor);
    }
}
class Deleted<TEntity> : IDeleted<TEntity>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private object parameters = null;

    public Deleted(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, object parameters)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.parameters = parameters;
    }
    public int Execute()
    {
        string sql = null;
        var entityType = typeof(TEntity);
        bool isMulti = this.parameters is IEnumerable && this.parameters is not string && this.parameters is not Dictionary<string, object>;
        using var command = this.connection.CreateCommand();
        if (isMulti)
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
        bool isMulti = this.parameters is IEnumerable && this.parameters is not string && this.parameters is not Dictionary<string, object>;
        using var cmd = this.connection.CreateCommand();
        if (isMulti)
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
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        string sql = null;
        var entityType = typeof(TEntity);
        bool isMulti = this.parameters is IEnumerable && this.parameters is not string && this.parameters is not Dictionary<string, object>;
        using var command = this.connection.CreateCommand();
        if (isMulti)
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
}
class Deleting<TEntity> : IDeleting<TEntity>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly IDeleteVisitor visitor;

    public Deleting(TheaConnection connection, IDbTransaction transaction, IDeleteVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }

    public IDeleting<TEntity> And(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IDeleting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition) this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
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
    public string ToSql(out List<IDbDataParameter> dbParameters) => this.visitor.BuildSql(out dbParameters);
}