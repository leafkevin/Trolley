using MySqlConnector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public class MySqlCreated<TEntity> : Created<TEntity>
{
    #region Properties
    public MySqlCreateVisitor DialectVisitor { get; protected set; }
    #endregion

    #region Constructor
    public MySqlCreated(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as MySqlCreateVisitor;
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
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
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
                            var data = this.Visitor.ToDataTable(tableName, tabledInsertObjs[tableName], memberMappers, valueGetters);
                            result += dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var data = this.Visitor.ToDataTable(tableName, insertObjs, memberMappers, valueGetters);
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
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
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
                            var data = this.Visitor.ToDataTable(tableName, tabledInsertObjs[tableName], memberMappers, valueGetters);
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var data = this.Visitor.ToDataTable(tableName, insertObjs, memberMappers, valueGetters);
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
public class MySqlCreated<TEntity, TResult> : Created<TEntity>, IMySqlCreated<TEntity, TResult>
{
    #region Constructor
    public MySqlCreated(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Execute
    public new TResult Execute()
    {
        var result = this.DbContext.CreateResult<TResult>(this.Visitor);
        this.Visitor.Dispose();
        this.Visitor = null;
        return result;
    }
    public new async Task<TResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        var result = await this.DbContext.CreateResultAsync<TResult>(this.Visitor, cancellationToken);
        this.Visitor.Dispose();
        this.Visitor = null;
        return result;
    }
    #endregion

    #region ExecuteIdentity
    /// <summary>
    /// 不支持的方法调用，调用Returning方法后此方法无效，请使用Execute方法
    /// </summary>
    /// <returns>返回自增长主键值</returns>
    public override int ExecuteIdentity()
        => throw new NotSupportedException("不支持的方法调用，调用Returning方法后此方法无效，请使用Execute方法");
    /// <summary>
    /// 不支持的方法调用，调用Returning方法后此方法无效，请使用ExecuteAsync方法
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回自增长主键值</returns>
    public override Task<int> ExecuteIdentityAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，调用Returning方法后此方法无效，请使用ExecuteAsync方法");
    /// <summary>
    /// 不支持的方法调用，调用Returning方法后此方法无效，请使用Execute方法
    /// </summary>
    /// <returns>返回自增长主键值</returns>
    public override long ExecuteIdentityLong()
        => throw new NotSupportedException("不支持的方法调用，调用Returning方法后此方法无效，请使用Execute方法");
    /// <summary>
    /// 不支持的方法调用，调用Returning方法后此方法无效，请使用ExecuteAsync方法
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回自增长主键值</returns>
    public override Task<long> ExecuteIdentityLongAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，调用Returning方法后此方法无效，请使用ExecuteAsync方法");
    #endregion
}
public class MySqlBulkCreated<TEntity, TResult> : Created<TEntity>, IMySqlBulkCreated<TEntity, TResult>
{
    #region Constructor
    public MySqlBulkCreated(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Execute
    public new List<TResult> Execute()
    {
        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var dialectVisitor = this.Visitor as MySqlCreateVisitor;

        if (this.Visitor.ActionMode == ActionMode.Bulk)
        {
            (var shardingType, var shardingTables, var insertObjs, var bulkCount, var firstSqlSetter,
                var loopSqlSetter, var tailSql, var readerFields) = this.Visitor.BuildWithBulk(command);

            int index = 0;
            var builder = new StringBuilder();
            Func<ITheaDataReader, object> readerDeserializer = null;
            void TabledExecute(string tableName, IEnumerable insertObjs)
            {
                foreach (var insertObj in insertObjs)
                {
                    loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                    index++;
                    if (index >= bulkCount)
                    {
                        builder.Remove(builder.Length - 1, 1);
                        builder.Append(tailSql);
                        command.CommandText = builder.ToString();
                        using var reader = command.ExecuteReader(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess);
                        readerDeserializer ??= reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);
                        while (reader.Read())
                            result.Add((TResult)readerDeserializer.Invoke(reader));

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
                builder.Append(tailSql);
                command.CommandText = builder.ToString();
                using var reader = command.ExecuteReader(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess);
                while (reader.Read())
                    result.Add((TResult)readerDeserializer.Invoke(reader));
            }
            builder.Clear();
        }
        else
        {
            command.CommandText = dialectVisitor.BuildSql(command, out var readerFields);
            connection.Open();
            using var reader = command.ExecuteReader(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess);
            var readerDeserializer = reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);
            while (reader.Read())
                result.Add((TResult)readerDeserializer.Invoke(reader));
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        this.Visitor.Dispose();
        this.Visitor = null;
        return result;
    }
    public new async Task<List<TResult>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();

        var dialectVisitor = this.Visitor as MySqlCreateVisitor;
        if (this.Visitor.ActionMode == ActionMode.Bulk)
        {
            (var shardingType, var shardingTables, var insertObjs, var bulkCount, var firstSqlSetter,
                var loopSqlSetter, var tailSql, var readerFields) = this.Visitor.BuildWithBulk(command);

            int index = 0;
            var builder = new StringBuilder();
            Func<ITheaDataReader, object> readerDeserializer = null;
            async Task TabledExecute(string tableName, IEnumerable insertObjs, CancellationToken cancellationToken)
            {
                foreach (var insertObj in insertObjs)
                {
                    loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                    index++;
                    if (index >= bulkCount)
                    {
                        builder.Remove(builder.Length - 1, 1);
                        builder.Append(tailSql);
                        command.CommandText = builder.ToString();
                        using var reader = await command.ExecuteReaderAsync(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess, cancellationToken);
                        readerDeserializer ??= reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);
                        while (await reader.ReadAsync(cancellationToken))
                            result.Add((TResult)readerDeserializer.Invoke(reader));

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
                    var tableParameters = tabledInsertObjs[tableName];
                    await TabledExecute(tableName, tableParameters, cancellationToken);
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
                builder.Append(tailSql);
                command.CommandText = builder.ToString();
                using var reader = await command.ExecuteReaderAsync(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess, cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                    result.Add((TResult)readerDeserializer.Invoke(reader));
            }
            builder.Clear();
        }
        else
        {
            command.CommandText = dialectVisitor.BuildSql(command, out var readerFields);
            await connection.OpenAsync(cancellationToken);
            using var reader = await command.ExecuteReaderAsync(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess, cancellationToken);
            var readerDeserializer = reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);
            while (await reader.ReadAsync(cancellationToken))
                result.Add((TResult)readerDeserializer.Invoke(reader));
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        this.Visitor.Dispose();
        return result;
    }
    #endregion

    #region ExecuteIdentity
    /// <summary>
    /// 不支持的方法调用，调用Returning方法后此方法无效，请使用Execute方法
    /// </summary>
    /// <returns>返回自增长主键值</returns>
    public override int ExecuteIdentity()
        => throw new NotSupportedException("不支持的方法调用，调用Returning方法后此方法无效，请使用Execute方法");
    /// <summary>
    /// 不支持的方法调用，调用Returning方法后此方法无效，请使用ExecuteAsync方法
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回自增长主键值</returns>
    public override Task<int> ExecuteIdentityAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，调用Returning方法后此方法无效，请使用ExecuteAsync方法");
    /// <summary>
    /// 不支持的方法调用，调用Returning方法后此方法无效，请使用Execute方法
    /// </summary>
    /// <returns>返回自增长主键值</returns>
    public override long ExecuteIdentityLong()
        => throw new NotSupportedException("不支持的方法调用，调用Returning方法后此方法无效，请使用Execute方法");
    /// <summary>
    /// 不支持的方法调用，调用Returning方法后此方法无效，请使用ExecuteAsync方法
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回自增长主键值</returns>
    public override Task<long> ExecuteIdentityLongAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，调用Returning方法后此方法无效，请使用ExecuteAsync方法");
    #endregion
}