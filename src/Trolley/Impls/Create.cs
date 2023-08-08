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

class Create<TEntity> : ICreate<TEntity>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly bool isParameterized;

    public Create(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.isParameterized = isParameterized;
    }
    public ICreated<TEntity> RawSql(string rawSql, object parameters)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        return new Created<TEntity>(this.connection, this.transaction, this.ormProvider, this.mapProvider).RawSql(rawSql, parameters);
    }
    public IContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        if (insertObj is IEnumerable && insertObj is not string && insertObj is not Dictionary<string, object>)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithBulkBy方法");

        return new ContinuedCreate<TEntity>(this.connection, this.transaction, this.ormProvider, this.mapProvider).WithBy(insertObj);
    }
    public ICreated<TEntity> WithBulkBy(IEnumerable insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        return new Created<TEntity>(this.connection, this.transaction, this.ormProvider, this.mapProvider).WithBulkBy(insertObjs, bulkCount);
    }
    public IContinuedCreate<TEntity, TSource> From<TSource>(Expression<Func<TSource, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = this.ormProvider.NewCreateVisitor(this.connection.DbKey, this.mapProvider, entityType, this.isParameterized).From(fieldSelector);
        return new ContinuedCreate<TEntity, TSource>(this.connection, this.transaction, visitor);
    }
    public IContinuedCreate<TEntity, T1, T2> From<T1, T2>(Expression<Func<T1, T2, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = this.ormProvider.NewCreateVisitor(this.connection.DbKey, this.mapProvider, entityType, this.isParameterized).From(fieldSelector);
        return new ContinuedCreate<TEntity, T1, T2>(this.connection, this.transaction, visitor);
    }
    public IContinuedCreate<TEntity, T1, T2, T3> From<T1, T2, T3>(Expression<Func<T1, T2, T3, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = this.ormProvider.NewCreateVisitor(this.connection.DbKey, this.mapProvider, entityType, this.isParameterized).From(fieldSelector);
        return new ContinuedCreate<TEntity, T1, T2, T3>(this.connection, this.transaction, visitor);
    }
    public IContinuedCreate<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = this.ormProvider.NewCreateVisitor(this.connection.DbKey, this.mapProvider, entityType, this.isParameterized).From(fieldSelector);
        return new ContinuedCreate<TEntity, T1, T2, T3, T4>(this.connection, this.transaction, visitor);
    }
    public IContinuedCreate<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = this.ormProvider.NewCreateVisitor(this.connection.DbKey, this.mapProvider, entityType, this.isParameterized).From(fieldSelector);
        return new ContinuedCreate<TEntity, T1, T2, T3, T4, T5>(this.connection, this.transaction, visitor);
    }
}
class ContinuedCreate<TEntity> : IContinuedCreate<TEntity>
{
    private readonly List<WithByBuilderCache> builders = new();
    private readonly TheaConnection connection;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly IDbTransaction transaction;

    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
    }
    public IContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        if (insertObj is IEnumerable && insertObj is not string && insertObj is not Dictionary<string, object>)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithBulkBy方法");

        var entityType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildCreateWithBiesCommandInitializer(
            this.connection, this.ormProvider, this.mapProvider, entityType, insertObj);

        this.builders.Add(new WithByBuilderCache
        {
            CommandInitializer = commandInitializer,
            Parameters = insertObj
        });
        return this;
    }
    public IContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
    {
        if (condition)
        {
            if (insertObj == null)
                throw new ArgumentNullException(nameof(insertObj));

            return this.WithBy(insertObj);
        }
        return this;
    }
    public int Execute()
    {
        int result = 0;
        var entityType = typeof(TEntity);
        var entityMapper = this.mapProvider.GetEntityMap(entityType);
        using var command = this.connection.CreateCommand();
        var sql = this.BuildSql(entityMapper, command);
        this.builders.Clear();

        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;
        this.connection.Open();

        if (entityMapper.IsAutoIncrement)
        {
            using var reader = command.ExecuteReader();
            if (reader.Read()) result = reader.To<int>();
            reader.Close();
            reader.Dispose();
            command.Dispose();
            return result;
        }
        result = command.ExecuteNonQuery();
        command.Dispose();
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        var entityType = typeof(TEntity);
        var entityMapper = this.mapProvider.GetEntityMap(entityType);
        using var cmd = this.connection.CreateCommand();
        var sql = this.BuildSql(entityMapper, cmd);
        this.builders.Clear();

        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        if (entityMapper.IsAutoIncrement)
        {
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
                result = reader.To<int>();
            await reader.CloseAsync();
            await reader.DisposeAsync();
            await command.DisposeAsync();
            return result;
        }
        result = await command.ExecuteNonQueryAsync(cancellationToken);
        await command.DisposeAsync();
        return result;
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        var entityType = typeof(TEntity);
        var entityMapper = this.mapProvider.GetEntityMap(entityType);
        using var command = this.connection.CreateCommand();
        var sql = this.BuildSql(entityMapper, command);

        dbParameters = null;
        if (command.Parameters != null && command.Parameters.Count > 0)
            dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Cancel();
        command.Dispose();
        return sql;
    }
    private string BuildSql(EntityMap entityMapper, IDbCommand command)
    {
        var insertBuilder = new StringBuilder($"INSERT INTO {this.ormProvider.GetTableName(entityMapper.TableName)} (");
        var valuesBuilder = new StringBuilder(" VALUES(");
        int index = 0;
        foreach (var builder in this.builders)
        {
            if (index > 0)
            {
                insertBuilder.Append(',');
                valuesBuilder.Append(',');
            }
            builder.CommandInitializer.Invoke(command, this.ormProvider, builder.Parameters, insertBuilder, valuesBuilder);
            index++;
        }
        insertBuilder.Append(')');
        valuesBuilder.Append(')');

        if (entityMapper.IsAutoIncrement)
            valuesBuilder.AppendFormat(this.ormProvider.SelectIdentitySql, entityMapper.AutoIncrementField);
        return insertBuilder.ToString() + valuesBuilder.ToString();
    }
    struct WithByBuilderCache
    {
        public object Parameters { get; set; }
        public Action<IDbCommand, IOrmProvider, object, StringBuilder, StringBuilder> CommandInitializer { get; set; }
    }
}
class Created<TEntity> : ICreated<TEntity>
{
    private readonly TheaConnection connection;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly IDbTransaction transaction;
    private string rawSql = null;
    private object parameters = null;
    private int? bulkCount;

    public Created(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
    }
    public ICreated<TEntity> RawSql(string rawSql, object parameters)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        this.rawSql = rawSql;
        this.parameters = parameters;
        return this;
    }
    public ICreated<TEntity> WithBulkBy(IEnumerable insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));
        this.parameters = insertObjs;
        this.bulkCount = bulkCount;
        return this;
    }
    public int Execute()
    {
        var entityType = typeof(TEntity);
        if (!string.IsNullOrEmpty(this.rawSql))
        {
            int result = 0;
            using var command = this.connection.CreateCommand();
            if (this.parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildCreateRawSqlParameters(this.connection,
                    this.ormProvider, this.mapProvider, entityType, this.rawSql, this.parameters);
                commandInitializer.Invoke(command, this.ormProvider, this.parameters);
            }
            command.CommandText = this.rawSql;
            command.CommandType = CommandType.Text;
            command.Transaction = this.transaction;
            this.connection.Open();
            var entityMapper = this.mapProvider.GetEntityMap(entityType);
            if (entityMapper.IsAutoIncrement)
            {
                using var reader = command.ExecuteReader();
                if (reader.Read()) result = reader.To<int>();
                reader.Close();
                reader.Dispose();
                command.Dispose();
                return result;
            }
            result = command.ExecuteNonQuery();
            command.Dispose();
            return result;
        }
        else
        {
            int result = 0;
            bool isMulti = this.parameters is IEnumerable && this.parameters is not string && this.parameters is not Dictionary<string, object>;
            if (isMulti)
            {
                var commandInitializer = RepositoryHelper.BuildCreateBatchCommandInitializer(
                    this.connection, this.ormProvider, this.mapProvider, entityType, this.parameters);

                int index = 0;
                this.bulkCount ??= 500;
                using var command = this.connection.CreateCommand();
                command.Transaction = this.transaction;
                var entities = this.parameters as IEnumerable;
                var sqlBuilder = new StringBuilder();

                foreach (var entity in entities)
                {
                    commandInitializer.Invoke(command, this.ormProvider, sqlBuilder, index, entity);
                    if (index >= this.bulkCount)
                    {
                        command.CommandText = sqlBuilder.ToString();
                        command.CommandType = CommandType.Text;
                        this.connection.Open();
                        result += command.ExecuteNonQuery();
                        command.Parameters.Clear();
                        sqlBuilder.Clear();
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    command.CommandText = sqlBuilder.ToString();
                    command.CommandType = CommandType.Text;
                    this.connection.Open();
                    result += command.ExecuteNonQuery();
                }
                command.Dispose();
            }
            else
            {
                var commandInitializer = RepositoryHelper.BuildCreateCommandInitializer(
                    this.connection, this.ormProvider, this.mapProvider, entityType, this.parameters);
                using var command = this.connection.CreateCommand();
                var sql = commandInitializer.Invoke(command, this.ormProvider, this.parameters);

                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                command.Transaction = this.transaction;
                this.connection.Open();
                var entityMapper = this.mapProvider.GetEntityMap(entityType);
                if (entityMapper.IsAutoIncrement)
                {
                    using var reader = command.ExecuteReader();
                    if (reader.Read()) result = reader.To<int>();
                    reader.Close();
                    reader.Dispose();
                    command.Dispose();
                    return result;
                }
                result = command.ExecuteNonQuery();
                command.Dispose();
            }
            return result;
        }
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        if (!string.IsNullOrEmpty(this.rawSql))
        {
            int result = 0;
            using var cmd = this.connection.CreateCommand();
            if (this.parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildCreateRawSqlParameters(
                    this.connection, this.ormProvider, this.mapProvider, entityType, this.rawSql, this.parameters);
                commandInitializer.Invoke(cmd, this.ormProvider, this.parameters);
            }

            cmd.CommandText = this.rawSql;
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = this.transaction;
            if (cmd is not DbCommand command)
                throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

            await this.connection.OpenAsync(cancellationToken);
            var entityMapper = this.mapProvider.GetEntityMap(entityType);
            if (entityMapper.IsAutoIncrement)
            {
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                    result = reader.To<int>();
                await reader.CloseAsync();
                await reader.DisposeAsync();
                await command.DisposeAsync();
                return result;
            }
            result = await command.ExecuteNonQueryAsync(cancellationToken);
            await command.DisposeAsync();
            return result;
        }
        else
        {
            int result = 0;
            bool isMulti = this.parameters is IEnumerable && this.parameters is not string && this.parameters is not Dictionary<string, object>;
            if (isMulti)
            {
                var commandInitializer = RepositoryHelper.BuildCreateBatchCommandInitializer(
                    this.connection, this.ormProvider, this.mapProvider, entityType, this.parameters);

                int index = 0;
                this.bulkCount ??= 500;
                using var cmd = this.connection.CreateCommand();
                cmd.Transaction = this.transaction;
                if (cmd is not DbCommand command)
                    throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");
                var entities = this.parameters as IEnumerable;
                var sqlBuilder = new StringBuilder();

                foreach (var entity in entities)
                {
                    commandInitializer.Invoke(command, this.ormProvider, sqlBuilder, index, entity);
                    if (index >= this.bulkCount)
                    {
                        command.CommandText = sqlBuilder.ToString();
                        command.CommandType = CommandType.Text;
                        await this.connection.OpenAsync(cancellationToken);
                        result += await command.ExecuteNonQueryAsync(cancellationToken);
                        command.Parameters.Clear();
                        sqlBuilder.Clear();
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    command.CommandText = sqlBuilder.ToString();
                    command.CommandType = CommandType.Text;
                    await this.connection.OpenAsync(cancellationToken);
                    result += await command.ExecuteNonQueryAsync(cancellationToken);
                }
                await command.DisposeAsync();
            }
            else
            {
                var commandInitializer = RepositoryHelper.BuildCreateCommandInitializer(
                    this.connection, this.ormProvider, this.mapProvider, entityType, this.parameters);
                using var cmd = this.connection.CreateCommand();
                var sql = commandInitializer.Invoke(cmd, this.ormProvider, this.parameters);
                cmd.Transaction = this.transaction;
                if (cmd is not DbCommand command)
                    throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                await this.connection.OpenAsync(cancellationToken);
                var entityMapper = this.mapProvider.GetEntityMap(entityType);
                if (entityMapper.IsAutoIncrement)
                {
                    using var reader = await command.ExecuteReaderAsync(cancellationToken);
                    if (await reader.ReadAsync(cancellationToken))
                        result = reader.To<int>();
                    await reader.CloseAsync();
                    await reader.DisposeAsync();
                    await command.DisposeAsync();
                    return result;
                }
                result = await command.ExecuteNonQueryAsync(cancellationToken);
                await command.DisposeAsync();
            }
            return result;
        }
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        string sql = null;
        dbParameters = null;
        var entityType = typeof(TEntity);
        using var command = this.connection.CreateCommand();
        if (!string.IsNullOrEmpty(this.rawSql))
        {
            sql = this.rawSql;
            if (this.parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildCreateRawSqlParameters(
                    this.connection, this.ormProvider, this.mapProvider, entityType, this.rawSql, this.parameters);
                commandInitializer.Invoke(command, this.ormProvider, this.parameters);
            }
        }
        else
        {
            bool isMulti = this.parameters is IEnumerable && this.parameters is not string && this.parameters is not Dictionary<string, object>;
            if (isMulti)
            {
                var commandInitializer = RepositoryHelper.BuildCreateBatchCommandInitializer(
                    this.connection, this.ormProvider, this.mapProvider, entityType, this.parameters);

                int index = 0;
                var entities = this.parameters as IEnumerable;
                var sqlBuilder = new StringBuilder();
                foreach (var entity in entities)
                {
                    commandInitializer.Invoke(command, this.ormProvider, sqlBuilder, index, entity);
                    if (index >= this.bulkCount)
                        break;
                    index++;
                }
                if (index > 0)
                    sql = sqlBuilder.ToString();
            }
            else
            {
                var commandInitializer = RepositoryHelper.BuildCreateCommandInitializer(
                    this.connection, this.ormProvider, this.mapProvider, entityType, this.parameters);
                sql = commandInitializer.Invoke(command, this.ormProvider, this.parameters);
            }
        }
        if (command.Parameters != null && command.Parameters.Count > 0)
            dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Cancel();
        command.Dispose();
        return sql;
    }
}
class ContinuedCreateBase
{
    protected readonly TheaConnection connection;
    protected readonly IDbTransaction transaction;
    protected readonly ICreateVisitor visitor;

    public ContinuedCreateBase(TheaConnection connection, IDbTransaction transaction, ICreateVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
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

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");
        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));

        await this.connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        await command.DisposeAsync();
        return result;
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters);
}
class ContinuedCreate<TEntity, TSource> : ContinuedCreateBase, IContinuedCreate<TEntity, TSource>
{
    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, ICreateVisitor visitor)
        : base(connection, transaction, visitor) { }
    public IContinuedCreate<TEntity, TSource> Where(Expression<Func<TSource, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IContinuedCreate<TEntity, TSource> Where(bool condition, Expression<Func<TSource, bool>> ifPredicate, Expression<Func<TSource, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IContinuedCreate<TEntity, TSource> And(Expression<Func<TSource, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IContinuedCreate<TEntity, TSource> And(bool condition, Expression<Func<TSource, bool>> ifPredicate, Expression<Func<TSource, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
}
class ContinuedCreate<TEntity, T1, T2> : ContinuedCreateBase, IContinuedCreate<TEntity, T1, T2>
{
    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, ICreateVisitor visitor)
        : base(connection, transaction, visitor) { }
    public IContinuedCreate<TEntity, T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1, T2> And(Expression<Func<T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
}
class ContinuedCreate<TEntity, T1, T2, T3> : ContinuedCreateBase, IContinuedCreate<TEntity, T1, T2, T3>
{
    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, ICreateVisitor visitor)
        : base(connection, transaction, visitor) { }
    public IContinuedCreate<TEntity, T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
}
class ContinuedCreate<TEntity, T1, T2, T3, T4> : ContinuedCreateBase, IContinuedCreate<TEntity, T1, T2, T3, T4>
{
    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, ICreateVisitor visitor)
        : base(connection, transaction, visitor) { }
    public IContinuedCreate<TEntity, T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
}
class ContinuedCreate<TEntity, T1, T2, T3, T4, T5> : ContinuedCreateBase, IContinuedCreate<TEntity, T1, T2, T3, T4, T5>
{
    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, ICreateVisitor visitor)
        : base(connection, transaction, visitor) { }
    public IContinuedCreate<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
}