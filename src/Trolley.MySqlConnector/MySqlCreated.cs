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
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                    Type insertObjType = null;
                    object firstInsertObj = null;
                    foreach (var insertObj in insertObjs)
                    {
                        insertObjType = insertObj.GetType();
                        firstInsertObj = insertObj;
                        break;
                    }
                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var sqlVisitor = this.Visitor as SqlVisitor;
                    if (this.DbContext.TableShardingProvider != null && this.DbContext.TableShardingProvider.TryGetTableSharding(entityType, out var tableShardingInfo))
                    {
                        var isNeedSplit = this.Visitor.Tables[0].Body == null;
                        if (isNeedSplit)
                        {
                            var tabledInsertObjs = this.Visitor.SplitShardingParameters(insertObjType, tableShardingInfo, insertObjs, firstInsertObj);
                            foreach (var tabledInsertObj in tabledInsertObjs)
                            {
                                result += dialectOrmProvider.ExecuteBulkCopy(false, this.DbContext, sqlVisitor, connection, insertObjType, tabledInsertObj.Value, timeoutSeconds, tabledInsertObj.Key);
                            }
                        }
                        else result = dialectOrmProvider.ExecuteBulkCopy(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, timeoutSeconds, this.Visitor.Tables[0].Body);
                    }
                    else result = dialectOrmProvider.ExecuteBulkCopy(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, timeoutSeconds);
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
                    (var insertObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                    Type insertObjType = null;
                    object firstInsertObj = null;
                    foreach (var insertObj in insertObjs)
                    {
                        insertObjType = insertObj.GetType();
                        firstInsertObj = insertObj;
                        break;
                    }
                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var sqlVisitor = this.Visitor as SqlVisitor;
                    if (this.DbContext.TableShardingProvider != null && this.DbContext.TableShardingProvider.TryGetTableSharding(entityType, out var tableShardingInfo))
                    {
                        var isNeedSplit = this.Visitor.Tables[0].Body == null;
                        if (isNeedSplit)
                        {
                            var tabledInsertObjs = this.Visitor.SplitShardingParameters(insertObjType, tableShardingInfo, insertObjs, firstInsertObj);
                            foreach (var tabledInsertObj in tabledInsertObjs)
                            {
                                result += await dialectOrmProvider.ExecuteBulkCopyAsync(false, this.DbContext, sqlVisitor, connection, insertObjType, tabledInsertObj.Value, timeoutSeconds, cancellationToken, tabledInsertObj.Key);
                            }
                        }
                        else result = await dialectOrmProvider.ExecuteBulkCopyAsync(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, timeoutSeconds, cancellationToken, this.Visitor.Tables[0].Body);
                    }
                    else result = await dialectOrmProvider.ExecuteBulkCopyAsync(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, timeoutSeconds, cancellationToken);
                    break;
                }
            case ActionMode.Bulk:
                {
                    var builder = new StringBuilder();
                    (var tableName, var tabledInsertObjs, var insertObjs, var bulkCount,
                        var firstSqlSetter, var loopSqlSetter, _, _) = this.Visitor.BuildWithBulk(command);
                    async Task<int> Executor(string tableName, IEnumerable insertObjs)
                    {
                        int count = 0, index = 0;
                        foreach (var insertObj in insertObjs)
                        {
                            if (index > 0) builder.Append(',');
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                            index++;

                            if (index >= bulkCount)
                            {
                                command.CommandText = builder.ToString();
                                count += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                                builder.Clear();
                                command.Parameters.Clear();
                                firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                                index = 0;
                            }
                        }
                        if (index > 0)
                        {
                            command.CommandText = builder.ToString();
                            count += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                            builder.Clear();
                            command.Parameters.Clear();
                        }
                        return count;
                    }

                    await connection.OpenAsync(cancellationToken);
                    if (tabledInsertObjs != null)
                    {
                        foreach (var tabledInsertObj in tabledInsertObjs)
                        {
                            firstSqlSetter.Invoke(command.Parameters, builder, tabledInsertObj.Key);
                            result += await Executor(tabledInsertObj.Key, tabledInsertObj.Value);
                        }
                    }
                    else
                    {
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        result = await Executor(tableName, insertObjs);
                    }
                    builder.Clear();
                    break;
                }
            default:
                //默认单条
                command.CommandText = this.Visitor.BuildCommand(command, false, out _);
                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(CommandSqlType.Insert, cancellationToken);
                break;
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        this.Visitor.Dispose();
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
    public new TResult Execute() => this.DbContext.CreateResult<TResult>(this.Visitor);
    public new async Task<TResult> ExecuteAsync(CancellationToken cancellationToken)
        => await this.DbContext.CreateResultAsync<TResult>(this.Visitor, cancellationToken);
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
        if (!string.IsNullOrEmpty(dialectVisitor.FromSql))
        {
            command.CommandText = dialectVisitor.BuildCommand(command, false, out var readerFields);
            connection.Open();
            using var reader = command.ExecuteReader(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess);
            var readerDeserializer = reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);
            while (reader.Read())
                result.Add((TResult)readerDeserializer.Invoke(reader));
        }
        else
        {
            (var tableName, var tabledInsertObjs, var insertObjs, var bulkCount, var firstSqlSetter,
                var loopSqlSetter, var tailSql, var readerFields) = this.Visitor.BuildWithBulk(command);

            var entityType = typeof(TEntity);
            var resultType = typeof(TResult);
            var builder = new StringBuilder();
            Func<ITheaDataReader, object> readerDeserializer = null;

            void Execute(string tableName, IEnumerable insertObjs)
            {
                int index = 0;
                foreach (var insertObj in insertObjs)
                {
                    if (index > 0) builder.Append(',');
                    loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                    index++;

                    if (index >= bulkCount)
                    {
                        builder.Append(tailSql);
                        command.CommandText = builder.ToString();
                        using var reader = command.ExecuteReader(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess);
                        readerDeserializer ??= reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);
                        while (reader.Read())
                            result.Add((TResult)readerDeserializer.Invoke(reader));
                        reader.Dispose();
                        builder.Clear();
                        command.Parameters.Clear();
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        index = 0;
                    }
                }
                if (index > 0)
                {
                    builder.Append(tailSql);
                    command.CommandText = builder.ToString();
                    using var reader = command.ExecuteReader(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess);
                    readerDeserializer ??= reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);
                    while (reader.Read())
                        result.Add((TResult)readerDeserializer.Invoke(reader));
                    reader.Dispose();
                    builder.Clear();
                    command.Parameters.Clear();
                }
            }

            connection.Open();
            if (tabledInsertObjs != null)
            {
                foreach (var tabledInsertObj in tabledInsertObjs)
                {
                    firstSqlSetter.Invoke(command.Parameters, builder, tabledInsertObj.Key);
                    Execute(tabledInsertObj.Key, tabledInsertObj.Value);
                }
            }
            else
            {
                firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                Execute(tableName, insertObjs);
            }
            builder.Clear();
        }
        command.Dispose();
        if (isNeedClose) connection.Close();
        this.Visitor.Dispose();
        return result;
    }
    public new async Task<List<TResult>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();

        var dialectVisitor = this.Visitor as MySqlCreateVisitor;
        if (!string.IsNullOrEmpty(dialectVisitor.FromSql))
        {
            command.CommandText = dialectVisitor.BuildCommand(command, false, out var readerFields);
            await connection.OpenAsync(cancellationToken);
            using var reader = await command.ExecuteReaderAsync(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess, cancellationToken);
            var readerDeserializer = reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);
            while (await reader.ReadAsync(cancellationToken))
                result.Add((TResult)readerDeserializer.Invoke(reader));
        }
        else
        {
            (var tableName, var tabledInsertObjs, var insertObjs, var bulkCount, var firstSqlSetter,
                var loopSqlSetter, var tailSql, var readerFields) = this.Visitor.BuildWithBulk(command);

            var entityType = typeof(TEntity);
            var resultType = typeof(TResult);
            var builder = new StringBuilder();
            Func<ITheaDataReader, object> readerDeserializer = null;

            async Task Execute(string tableName, IEnumerable insertObjs)
            {
                int index = 0;
                foreach (var insertObj in insertObjs)
                {
                    if (index > 0) builder.Append(',');
                    loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                    index++;

                    if (index >= bulkCount)
                    {
                        builder.Append(tailSql);
                        command.CommandText = builder.ToString();
                        using var reader = await command.ExecuteReaderAsync(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess, cancellationToken);
                        readerDeserializer ??= reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);
                        while (await reader.ReadAsync(cancellationToken))
                            result.Add((TResult)readerDeserializer.Invoke(reader));
                        await reader.DisposeAsync();
                        builder.Clear();
                        command.Parameters.Clear();
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        index = 0;
                    }
                }
                if (index > 0)
                {
                    builder.Append(tailSql);
                    command.CommandText = builder.ToString();
                    using var reader = await command.ExecuteReaderAsync(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess, cancellationToken);
                    readerDeserializer ??= reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);
                    while (await reader.ReadAsync(cancellationToken))
                        result.Add((TResult)readerDeserializer.Invoke(reader));
                    await reader.DisposeAsync();
                    builder.Clear();
                    command.Parameters.Clear();
                }
            }

            await connection.OpenAsync(cancellationToken);
            if (tabledInsertObjs != null)
            {
                foreach (var tabledInsertObj in tabledInsertObjs)
                {
                    firstSqlSetter.Invoke(command.Parameters, builder, tabledInsertObj.Key);
                    await Execute(tabledInsertObj.Key, tabledInsertObj.Value);
                }
            }
            else
            {
                firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                await Execute(tableName, insertObjs);
            }
            builder.Clear();
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