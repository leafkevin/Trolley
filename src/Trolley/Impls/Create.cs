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
    protected readonly ICreateVisitor visitor;
    protected readonly Type entityType;
    protected readonly bool isParameterized;
    #endregion

    #region Constructor
    public Create(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.isParameterized = isParameterized;
        this.entityType = typeof(TEntity);
        this.visitor = ormProvider.NewCreateVisitor(connection.DbKey, mapProvider, isParameterized);
        this.visitor.Initialize(entityType);
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
        return new ContinuedCreate<TEntity>(this.connection, this.ormProvider, this.mapProvider, this.visitor);
    }
    #endregion

    #region WithBulk
    public ICreated<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        return new Created<TEntity>(this.connection, this.ormProvider, this.mapProvider, this.visitor).WithBulk(insertObjs, bulkCount);
    }
    #endregion

    #region UseIgnore/IfNotExists
    public ICreate<TEntity> UseIgnore()
    {
        this.visitor.UseIgnore();
        return this;
    }
    public ICreate<TEntity> IfNotExists<TFields>(TFields keys)
    {
        this.visitor.IfNotExists(keys);
        return this;
    }
    public ICreate<TEntity> IfNotExists(Expression<Func<TEntity, bool>> keysPredicate)
    {
        this.visitor.IfNotExists(keysPredicate);
        return this;
    }
    #endregion
}
class Created<TEntity> : ICreated<TEntity>
{
    #region Fields
    protected readonly TheaConnection connection;
    protected readonly IOrmProvider ormProvider;
    protected readonly ICreateVisitor visitor;
    protected readonly IEntityMapProvider mapProvider;
    private IEnumerable parameters = null;
    private int? bulkCount;
    private bool isBulk = false;
    #endregion

    #region Constructor
    public Created(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, ICreateVisitor visitor)
    {
        this.connection = connection;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.visitor = visitor;
    }
    #endregion   

    #region WithBulk
    public ICreated<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        this.visitor.WithBulk(insertObjs);
        this.parameters = insertObjs;
        this.bulkCount = bulkCount;
        this.isBulk = true;
        return this;
    }
    #endregion

    #region OrUpdate  
    public ICreated<TEntity> OrUpdate<TUpdateFields>(TUpdateFields updateObj)
    {
        //this.visitor.Set(updateObj);
        return this;
    }
    public ICreated<TEntity> OrUpdate<TUpdateFields>(Expression<Func<ICreateOrUpdate, TEntity, TUpdateFields>> fieldsAssignment)
    {
        this.visitor.Set(fieldsAssignment);
        return this;
    }
    #endregion

    #region Execute
    public int Execute() => (int)this.ExecuteLong();
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
        => (int)await this.ExecuteLongAsync(cancellationToken);
    public long ExecuteLong()
    {
        long result = 0;
        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        if (this.isBulk)
        {
            int index = 0;
            this.bulkCount ??= 500;
            var sqlBuilder = new StringBuilder();
            var headSql = this.visitor.BuildBulkHeadSql(sqlBuilder, out var commandInitializer);
            var typedCommandInitializer = commandInitializer as Action<IDbCommand, StringBuilder, object, int>;
            foreach (var entity in this.parameters)
            {
                this.visitor.WithBulk(command, sqlBuilder, typedCommandInitializer, entity, index);
                if (index >= this.bulkCount)
                {
                    this.visitor.WithBulkTail(sqlBuilder);
                    command.CommandText = sqlBuilder.ToString();
                    this.connection.Open();
                    result += command.ExecuteNonQuery();
                    command.Parameters.Clear();
                    sqlBuilder.Clear();
                    index = 0;
                    sqlBuilder.Append(headSql);
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
            sqlBuilder.Clear();
        }
        else
        {
            var entityType = typeof(TEntity);
            var entityMapper = this.mapProvider.GetEntityMap(entityType);
            this.visitor.BuildCommand(command);
            if (entityMapper.IsAutoIncrement)
            {
                using var reader = command.ExecuteReader();
                if (reader.Read()) result = reader.To<long>();
                reader.Dispose();
            }
            else result = command.ExecuteNonQuery();
        }
        command.Dispose();
        return result;
    }
    public async Task<long> ExecuteLongAsync(CancellationToken cancellationToken = default)
    {
        long result = 0;
        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        if (this.isBulk)
        {
            int index = 0;
            this.bulkCount ??= 500;
            var sqlBuilder = new StringBuilder();
            var headSql = this.visitor.BuildBulkHeadSql(sqlBuilder, out var commandInitializer);
            var myCommandInitializer = commandInitializer as Action<IDbCommand, StringBuilder, object, int>;
            foreach (var entity in this.parameters)
            {
                this.visitor.WithBulk(command, sqlBuilder, myCommandInitializer, entity, index);
                if (index >= this.bulkCount)
                {
                    this.visitor.WithBulkTail(sqlBuilder);
                    command.CommandText = sqlBuilder.ToString();
                    await this.connection.OpenAsync(cancellationToken);
                    result += await command.ExecuteNonQueryAsync(cancellationToken);
                    command.Parameters.Clear();
                    sqlBuilder.Clear();
                    index = 0;
                    sqlBuilder.Append(headSql);
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
            sqlBuilder.Clear();
        }
        else
        {
            var entityType = typeof(TEntity);
            var entityMapper = this.mapProvider.GetEntityMap(entityType);
            this.visitor.BuildCommand(command);
            await this.connection.OpenAsync(cancellationToken);
            if (entityMapper.IsAutoIncrement)
            {
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                    result = reader.To<long>();
                await reader.DisposeAsync();
            }
            else result = await command.ExecuteNonQueryAsync(cancellationToken);
        }
        await command.DisposeAsync();
        return result;
    }
    #endregion

    //#region ToMultipleCommand
    //public MultipleCommand ToMultipleCommand() => this.visitor.CreateMultipleCommand();
    //#endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        string sql = null;
        using var command = this.connection.CreateCommand();
        if (this.isBulk)
        {
            int index = 0;
            var sqlBuilder = new StringBuilder();
            var headSql = this.visitor.BuildBulkHeadSql(sqlBuilder, out var commandInitializer);
            var myCommandInitializer = commandInitializer as Action<IDbCommand, StringBuilder, object, int>;
            foreach (var entity in this.parameters)
            {
                this.visitor.WithBulk(command, sqlBuilder, myCommandInitializer, entity, index);
                if (index >= this.bulkCount)
                {
                    this.visitor.WithBulkTail(sqlBuilder);
                    break;
                }
                index++;
            }
            if (index > 0)
                sql = sqlBuilder.ToString();
        }
        else sql = this.visitor.BuildSql();
        dbParameters = this.visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        return sql;
    }
    #endregion
}
class ContinuedCreate<TEntity> : Created<TEntity>, IContinuedCreate<TEntity>
{
    #region Constructor
    public ContinuedCreate(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, ICreateVisitor visitor)
        : base(connection, ormProvider, mapProvider, visitor) { }
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

        if (condition) this.visitor.WithByField(new FieldObject { FieldSelector = fieldSelector, FieldValue = fieldValue });
        return this;
    }
    #endregion
}