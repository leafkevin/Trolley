using MySqlConnector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public class MySqlContinuedCreate<TEntity> : ContinuedCreate<TEntity>, IMySqlContinuedCreate<TEntity>
{
    #region Properties
    public MySqlCreateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public MySqlContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as MySqlCreateVisitor;
    }
    #endregion

    #region WithBy
    public new IMySqlContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => this.WithBy(true, insertObj);
    public new IMySqlContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
        => base.WithBy(condition, insertObj) as IMySqlContinuedCreate<TEntity>;
    public new IMySqlContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.WithBy(true, fieldSelector, fieldValue);
    public new IMySqlContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.WithBy(condition, fieldSelector, fieldValue) as IMySqlContinuedCreate<TEntity>;
    #endregion

    #region IgnoreFields
    public new IMySqlContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames)
        => base.IgnoreFields(fieldNames) as IMySqlContinuedCreate<TEntity>;
    public new IMySqlContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.IgnoreFields(fieldsSelector) as IMySqlContinuedCreate<TEntity>;
    #endregion

    #region OnlyFields
    public new IMySqlContinuedCreate<TEntity> OnlyFields(params string[] fieldNames)
        => base.OnlyFields(fieldNames) as IMySqlContinuedCreate<TEntity>;
    public new IMySqlContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.OnlyFields(fieldsSelector) as IMySqlContinuedCreate<TEntity>;
    #endregion

    #region OnDuplicateKeyUpdate
    public IMySqlContinuedCreate<TEntity> OnDuplicateKeyUpdate<TUpdateFields>(Expression<Func<IMySqlCreateDuplicateKeyUpdate<TEntity>, TUpdateFields>> fieldsAssignment)
    {
        this.DialectVisitor.OnDuplicateKeyUpdate(fieldsAssignment);
        return this;
    }
    #endregion

    #region Returnning
    public IMySqlCreated<TEntity, TResult> Returning<TResult>(string fieldNames)
    {
        this.DialectVisitor.Returning(fieldNames);
        return new MySqlCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public IMySqlCreated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Returning(fieldsSelector);
        return new MySqlCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var entityType = typeof(TEntity);
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjType, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.DialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var mySqlConnection = connection.BaseConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.BaseTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            bulkCopyObj.DestinationTableName = tableName;
                            var data = this.Visitor.ToDataTable(tableName, insertObjType, tabledInsertObjs[tableName], memberMappers, valueGetters);
                            result += dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var data = this.Visitor.ToDataTable(tableName, insertObjType, insertObjs, memberMappers, valueGetters);
                        result = dialectOrmProvider.ExecuteBulkCopy(shardingTables as string, bulkCopyObj, connection, this.DbContext, data);
                    }
                    break;
                }
            case ActionMode.Bulk:
                {
                    (var shardingType, var shardingTables, var insertObjs, var bulkCount, var firstSqlSetter,
                    var loopSqlSetter, _, var readerFields) = this.Visitor.BuildWithBulk(command);

                    int index = 0;
                    var builder = new StringBuilder();
                    void TabledExecute(string tableName, IEnumerable insertObjs)
                    {
                        foreach (var insertObj in insertObjs)
                        {
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                            index++;
                            if (index >= bulkCount)
                            {
                                builder.Remove(builder.Length - 1, 1);
                                command.CommandText = builder.ToString();
                                result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                                builder.Clear();
                                command.Parameters.Clear();
                                firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                                index = 0;
                            }
                        }
                    }
                    connection.Open();
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                            var tableParameters = tabledInsertObjs[tableName];
                            TabledExecute(tableName, tableParameters);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        TabledExecute(tableName, insertObjs);
                    }
                    if (index > 0)
                    {
                        builder.Remove(builder.Length - 1, 1);
                        command.CommandText = builder.ToString();
                        result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(command, out _);
                connection.Open();
                result = command.ExecuteNonQuery(CommandSqlType.Insert);
                break;
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        this.Visitor.Dispose();
        this.Visitor = null;
        return result;
    }
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var entityType = typeof(TEntity);
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjType, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.DialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var mySqlConnection = connection.BaseConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.BaseTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            bulkCopyObj.DestinationTableName = tableName;
                            var data = this.Visitor.ToDataTable(tableName, insertObjType, tabledInsertObjs[tableName], memberMappers, valueGetters);
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var data = this.Visitor.ToDataTable(tableName, insertObjType, insertObjs, memberMappers, valueGetters);
                        result = await dialectOrmProvider.ExecuteBulkCopyAsync(shardingTables as string, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                    }
                    break;
                }
            case ActionMode.Bulk:
                {
                    (var shardingType, var shardingTables, var insertObjs, var bulkCount, var firstSqlSetter,
                        var loopSqlSetter, _, var readerFields) = this.Visitor.BuildWithBulk(command);

                    int index = 0;
                    var builder = new StringBuilder();
                    async Task TabledExecute(string tableName, IEnumerable insertObjs, CancellationToken cancellationToken)
                    {
                        foreach (var insertObj in insertObjs)
                        {
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                            index++;
                            if (index >= bulkCount)
                            {
                                builder.Remove(builder.Length - 1, 1);
                                command.CommandText = builder.ToString();
                                result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                                builder.Clear();
                                command.Parameters.Clear();
                                firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                                index = 0;
                            }
                        }
                    }
                    await connection.OpenAsync(cancellationToken);
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                            await TabledExecute(tableName, tabledInsertObjs[tableName], cancellationToken);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        await TabledExecute(tableName, insertObjs, cancellationToken);
                    }
                    if (index > 0)
                    {
                        builder.Remove(builder.Length - 1, 1);
                        command.CommandText = builder.ToString();
                        result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(command, out _);
                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(CommandSqlType.Insert, cancellationToken);
                break;
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        this.Visitor.Dispose();
        this.Visitor = null;
        return result;
    }
    #endregion
}
public class MySqlBulkContinuedCreate<TEntity> : ContinuedCreate<TEntity>, IMySqlBulkContinuedCreate<TEntity>
{
    #region Properties
    public MySqlCreateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public MySqlBulkContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as MySqlCreateVisitor;
    }
    #endregion

    #region WithBy
    public new IMySqlBulkContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => this.WithBy(true, insertObj);
    public new IMySqlBulkContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
        => base.WithBy(condition, insertObj) as IMySqlBulkContinuedCreate<TEntity>;
    public new IMySqlBulkContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.WithBy(true, fieldSelector, fieldValue);
    public new IMySqlBulkContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.WithBy(condition, fieldSelector, fieldValue) as IMySqlBulkContinuedCreate<TEntity>;
    #endregion

    #region IgnoreFields
    public new IMySqlBulkContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames)
        => base.IgnoreFields(fieldNames) as IMySqlBulkContinuedCreate<TEntity>;
    public new IMySqlBulkContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.IgnoreFields(fieldsSelector) as IMySqlBulkContinuedCreate<TEntity>;
    #endregion

    #region OnlyFields
    public new IMySqlBulkContinuedCreate<TEntity> OnlyFields(params string[] fieldNames)
        => base.OnlyFields(fieldNames) as IMySqlBulkContinuedCreate<TEntity>;
    public new IMySqlBulkContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.OnlyFields(fieldsSelector) as IMySqlBulkContinuedCreate<TEntity>;
    #endregion

    #region OnDuplicateKeyUpdate
    public IMySqlBulkContinuedCreate<TEntity> OnDuplicateKeyUpdate<TUpdateFields>(Expression<Func<IMySqlCreateDuplicateKeyUpdate<TEntity>, TUpdateFields>> fieldsAssignment)
    {
        this.DialectVisitor.OnDuplicateKeyUpdate(fieldsAssignment);
        return this;
    }
    #endregion

    #region Returnning
    public IMySqlBulkCreated<TEntity, TResult> Returning<TResult>(string fieldNames)
    {
        this.DialectVisitor.Returning(fieldNames);
        return new MySqlBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public IMySqlBulkCreated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Returning(fieldsSelector);
        return new MySqlBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var entityType = typeof(TEntity);
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjType, var insertObjs, var timeoutSeconds,
                         var memberMappers, var valueGetters) = this.DialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var mySqlConnection = connection.BaseConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.BaseTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            bulkCopyObj.DestinationTableName = tableName;
                            var data = this.Visitor.ToDataTable(tableName, insertObjType, tabledInsertObjs[tableName], memberMappers, valueGetters);
                            result += dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var data = this.Visitor.ToDataTable(tableName, insertObjType, insertObjs, memberMappers, valueGetters);
                        result = dialectOrmProvider.ExecuteBulkCopy(shardingTables as string, bulkCopyObj, connection, this.DbContext, data);
                    }
                    break;
                }
            case ActionMode.Bulk:
                {
                    (var shardingType, var shardingTables, var insertObjs, var bulkCount, var firstSqlSetter,
                    var loopSqlSetter, _, var readerFields) = this.Visitor.BuildWithBulk(command);

                    int index = 0;
                    var builder = new StringBuilder();
                    void TabledExecute(string tableName, IEnumerable insertObjs)
                    {
                        foreach (var insertObj in insertObjs)
                        {
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                            index++;
                            if (index >= bulkCount)
                            {
                                builder.Remove(builder.Length - 1, 1);
                                command.CommandText = builder.ToString();
                                result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                                builder.Clear();
                                command.Parameters.Clear();
                                firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                                index = 0;
                            }
                        }
                    }
                    connection.Open();
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                            var tableParameters = tabledInsertObjs[tableName];
                            TabledExecute(tableName, tableParameters);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        TabledExecute(tableName, insertObjs);
                    }
                    if (index > 0)
                    {
                        builder.Remove(builder.Length - 1, 1);
                        command.CommandText = builder.ToString();
                        result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(command, out _);
                connection.Open();
                result = command.ExecuteNonQuery(CommandSqlType.Insert);
                break;
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        this.Visitor.Dispose();
        this.Visitor = null;
        return result;
    }
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var entityType = typeof(TEntity);
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjType, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.DialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var mySqlConnection = connection.BaseConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.BaseTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            bulkCopyObj.DestinationTableName = tableName;
                            var data = this.Visitor.ToDataTable(tableName, insertObjType, tabledInsertObjs[tableName], memberMappers, valueGetters);
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var data = this.Visitor.ToDataTable(tableName, insertObjType, insertObjs, memberMappers, valueGetters);
                        result = await dialectOrmProvider.ExecuteBulkCopyAsync(shardingTables as string, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                    }
                    break;
                }
            case ActionMode.Bulk:
                {
                    (var shardingType, var shardingTables, var insertObjs, var bulkCount, var firstSqlSetter,
                        var loopSqlSetter, _, var readerFields) = this.Visitor.BuildWithBulk(command);

                    int index = 0;
                    var builder = new StringBuilder();
                    async Task TabledExecute(string tableName, IEnumerable insertObjs, CancellationToken cancellationToken)
                    {
                        foreach (var insertObj in insertObjs)
                        {
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                            index++;
                            if (index >= bulkCount)
                            {
                                builder.Remove(builder.Length - 1, 1);
                                command.CommandText = builder.ToString();
                                result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                                builder.Clear();
                                command.Parameters.Clear();
                                firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                                index = 0;
                            }
                        }
                    }
                    await connection.OpenAsync(cancellationToken);
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                            await TabledExecute(tableName, tabledInsertObjs[tableName], cancellationToken);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        await TabledExecute(tableName, insertObjs, cancellationToken);
                    }
                    if (index > 0)
                    {
                        builder.Remove(builder.Length - 1, 1);
                        command.CommandText = builder.ToString();
                        result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(command, out _);
                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(CommandSqlType.Insert, cancellationToken);
                break;
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        this.Visitor.Dispose();
        this.Visitor = null;
        return result;
    }
    #endregion
}