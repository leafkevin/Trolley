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
    #region Fields
    protected readonly TheaConnection connection;
    protected readonly IDbTransaction transaction;
    protected readonly IOrmProvider ormProvider;
    protected readonly IEntityMapProvider mapProvider;
    protected readonly bool isParameterized;
    protected readonly ICreateVisitor visitor;
    #endregion

    #region Constructor
    public Create(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.isParameterized = isParameterized;
        var entityType = typeof(TEntity);
        this.visitor = ormProvider.NewCreateVisitor(connection.DbKey, mapProvider, entityType, isParameterized);
    }
    #endregion

    #region WithBy
    public IContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        if (insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithBulk方法");

        this.visitor.WithBy(insertObj);
        return new ContinuedCreate<TEntity>(this.connection, this.transaction, this.mapProvider, this.visitor);
    }
    #endregion

    #region WithBulk
    public ICreated<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        this.visitor.WithBulkFirst(insertObjs);
        return new Created<TEntity>(this.connection, this.transaction, this.mapProvider, this.visitor).WithBulk(insertObjs, bulkCount);
    }
    #endregion

    #region From
    public IContinuedCreate<TEntity, TSource> From<TSource>(Expression<Func<TSource, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        this.visitor.From(fieldSelector);
        return new ContinuedCreate<TEntity, TSource>(this.connection, this.transaction, this.mapProvider, this.visitor);
    }
    public IContinuedCreate<TEntity, T1, T2> From<T1, T2>(Expression<Func<T1, T2, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        this.visitor.From(fieldSelector);
        return new ContinuedCreate<TEntity, T1, T2>(this.connection, this.transaction, this.mapProvider, this.visitor);
    }
    public IContinuedCreate<TEntity, T1, T2, T3> From<T1, T2, T3>(Expression<Func<T1, T2, T3, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        this.visitor.From(fieldSelector);
        return new ContinuedCreate<TEntity, T1, T2, T3>(this.connection, this.transaction, this.mapProvider, this.visitor);
    }
    public IContinuedCreate<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        this.visitor.From(fieldSelector);
        return new ContinuedCreate<TEntity, T1, T2, T3, T4>(this.connection, this.transaction, this.mapProvider, this.visitor);
    }
    public IContinuedCreate<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        this.visitor.From(fieldSelector);
        return new ContinuedCreate<TEntity, T1, T2, T3, T4, T5>(this.connection, this.transaction, this.mapProvider, this.visitor);
    }
    #endregion
}
class Created<TEntity> : ICreated<TEntity>
{
    #region Fields
    protected readonly TheaConnection connection;
    protected readonly IDbTransaction transaction;
    protected readonly ICreateVisitor visitor;
    protected readonly IEntityMapProvider mapProvider;
    private IEnumerable parameters = null;
    private int? bulkCount;
    private bool isBulk = false;
    #endregion

    #region Constructor
    public Created(TheaConnection connection, IDbTransaction transaction, IEntityMapProvider mapProvider, ICreateVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.mapProvider = mapProvider;
        this.visitor = visitor;
    }
    #endregion

    #region UseIgnore
    public ICreated<TEntity> UseIgnore()
    {
        this.visitor.UseIgnore();
        return this;
    }
    #endregion

    #region WithBulk
    public ICreated<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        this.parameters = insertObjs;
        this.bulkCount = bulkCount;
        this.isBulk = true;
        return this;
    }
    #endregion

    #region Execute
    public int Execute()
    {
        int result = 0;
        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        if (this.isBulk)
        {
            int index = 0;
            this.bulkCount ??= 500;
            var sqlBuilder = new StringBuilder();
            this.visitor.WithBulkFirst(this.parameters);

            foreach (var entity in this.parameters)
            {
                this.visitor.WithBulk(command, sqlBuilder, index, entity);
                if (index >= this.bulkCount)
                {
                    command.CommandText = sqlBuilder.ToString();
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
                this.connection.Open();
                result += command.ExecuteNonQuery();
            }
        }
        else
        {
            var sql = this.visitor.BuildSql(out var dbParameters);
            command.CommandText = sql;
            if (dbParameters != null && dbParameters.Count > 0)
                dbParameters.ForEach(f => command.Parameters.Add(f));

            var entityType = typeof(TEntity);
            var entityMapper = this.mapProvider.GetEntityMap(entityType);
            if (entityMapper.IsAutoIncrement)
            {
                using var reader = command.ExecuteReader();
                if (reader.Read()) result = reader.To<int>();
                reader.Dispose();
                command.Dispose();
                return result;
            }
            result = command.ExecuteNonQuery();
        }
        command.Dispose();
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        if (this.isBulk)
        {
            int index = 0;
            this.bulkCount ??= 500;
            var sqlBuilder = new StringBuilder();
            this.visitor.WithBulkFirst(this.parameters);

            foreach (var entity in this.parameters)
            {
                this.visitor.WithBulk(cmd, sqlBuilder, index, entity);
                if (index >= this.bulkCount)
                {
                    command.CommandText = sqlBuilder.ToString();
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
                await this.connection.OpenAsync(cancellationToken);
                result += await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        else
        {
            var sql = this.visitor.BuildSql(out var dbParameters);
            cmd.CommandText = sql;
            if (dbParameters != null && dbParameters.Count > 0)
                dbParameters.ForEach(f => command.Parameters.Add(f));

            var entityType = typeof(TEntity);
            var entityMapper = this.mapProvider.GetEntityMap(entityType);
            await this.connection.OpenAsync(cancellationToken);
            if (entityMapper.IsAutoIncrement)
            {
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                    result = reader.To<int>();
                await reader.DisposeAsync();
                await command.DisposeAsync();
                return result;
            }
            result = await command.ExecuteNonQueryAsync(cancellationToken);
            await command.DisposeAsync();
        }
        await command.DisposeAsync();
        return result;
    }
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        string sql = null;
        if (this.isBulk)
        {
            int index = 0;
            var sqlBuilder = new StringBuilder();
            using var command = this.connection.CreateCommand();
            this.visitor.WithBulkFirst(this.parameters);

            foreach (var entity in this.parameters)
            {
                this.visitor.WithBulk(command, sqlBuilder, index, entity);
                if (index >= this.bulkCount)
                    break;
                index++;
            }
            if (index > 0)
                sql = sqlBuilder.ToString();
            if (command.Parameters != null && command.Parameters.Count > 0)
                dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
            command.Dispose();
        }
        else sql = this.visitor.BuildSql(out dbParameters);
        return sql;
    }
    #endregion
}
class ContinuedCreate<TEntity> : Created<TEntity>, IContinuedCreate<TEntity>
{
    #region Constructor
    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, IEntityMapProvider mapProvider, ICreateVisitor visitor)
        : base(connection, transaction, mapProvider, visitor) { }
    #endregion

    #region WithBy
    public IContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        if (insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithBulkBy方法");

        this.visitor.WithBy(insertObj);
        return this;
    }
    public IContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
    {
        if (condition) this.WithBy(insertObj);
        return this;
    }
    public IContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        if (condition) this.visitor.WithBy(fieldSelector, fieldValue);
        return this;
    }
    #endregion
}

class ContinuedCreate<TEntity, TSource> : Created<TEntity>, IContinuedCreate<TEntity, TSource>
{
    #region Constructor
    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, IEntityMapProvider mapProvider, ICreateVisitor visitor)
        : base(connection, transaction, mapProvider, visitor) { }
    #endregion

    #region Where/And
    public IContinuedCreate<TEntity, TSource> Where(Expression<Func<TSource, bool>> predicate)
        => this.Where(true, predicate);
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
        => this.And(true, predicate);

    public IContinuedCreate<TEntity, TSource> And(bool condition, Expression<Func<TSource, bool>> ifPredicate, Expression<Func<TSource, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
class ContinuedCreate<TEntity, T1, T2> : Created<TEntity>, IContinuedCreate<TEntity, T1, T2>
{
    #region Constructor
    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, IEntityMapProvider mapProvider, ICreateVisitor visitor)
        : base(connection, transaction, mapProvider, visitor) { }
    #endregion

    #region Where/And
    public IContinuedCreate<TEntity, T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
        => this.Where(true, predicate);
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
        => this.And(true, predicate);
    public IContinuedCreate<TEntity, T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
class ContinuedCreate<TEntity, T1, T2, T3> : Created<TEntity>, IContinuedCreate<TEntity, T1, T2, T3>
{
    #region Constructor
    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, IEntityMapProvider mapProvider, ICreateVisitor visitor)
        : base(connection, transaction, mapProvider, visitor) { }
    #endregion

    #region Where/And
    public IContinuedCreate<TEntity, T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
        => this.Where(true, predicate);
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
        => this.And(true, predicate);
    public IContinuedCreate<TEntity, T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
class ContinuedCreate<TEntity, T1, T2, T3, T4> : Created<TEntity>, IContinuedCreate<TEntity, T1, T2, T3, T4>
{
    #region Constructor
    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, IEntityMapProvider mapProvider, ICreateVisitor visitor)
        : base(connection, transaction, mapProvider, visitor) { }
    #endregion

    #region Where/And
    public IContinuedCreate<TEntity, T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
        => this.Where(true, predicate);
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
        => this.And(true, predicate);
    public IContinuedCreate<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
class ContinuedCreate<TEntity, T1, T2, T3, T4, T5> : Created<TEntity>, IContinuedCreate<TEntity, T1, T2, T3, T4, T5>
{
    #region Constructor
    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, IEntityMapProvider mapProvider, ICreateVisitor visitor)
        : base(connection, transaction, mapProvider, visitor) { }
    #endregion

    #region Where/And
    public IContinuedCreate<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        => this.Where(true, predicate);
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
        => this.And(true, predicate);
    public IContinuedCreate<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion
}