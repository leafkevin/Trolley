using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.PostgreSql;

public class PostgreSqlCreated<TEntity> : Created<TEntity>, IPostgreSqlCreated<TEntity>
{
    #region Properties
    public PostgreSqlCreateVisitor DialectVisitor { get; protected set; }
    #endregion

    #region Constructor
    public PostgreSqlCreated(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as PostgreSqlCreateVisitor;
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
                    var insertObjs = this.DialectVisitor.BuildWithBulkCopy();
                    Type insertObjType = null;
                    foreach (var insertObj in insertObjs)
                    {
                        insertObjType = insertObj.GetType();
                        break;
                    }
                    var dialectOrmProvider = this.OrmProvider as PostgreSqlProvider;
                    var sqlVisitor = this.Visitor as SqlVisitor;
                    if (this.DbContext.ShardingProvider != null && this.DbContext.ShardingProvider.TryGetTableSharding(entityType, out var shardingTable))
                    {
                        var isNeedSplit = this.Visitor.Tables[0].Body == null;
                        if (isNeedSplit)
                        {
                            var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                            foreach (var tabledInsertObj in tabledInsertObjs)
                            {
                                result += dialectOrmProvider.ExecuteBulkCopy(false, this.DbContext, sqlVisitor, connection, insertObjType, tabledInsertObj.Value, tabledInsertObj.Key);
                            }
                        }
                        else result = dialectOrmProvider.ExecuteBulkCopy(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, this.Visitor.Tables[0].Body);
                    }
                    else result = dialectOrmProvider.ExecuteBulkCopy(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs);
                    break;
                }
            case ActionMode.Bulk:
                {
                    var builder = new StringBuilder();
                    (var isNeedSplit, var tableName, var insertObjs, var bulkCount,
                        var firstSqlSetter, var loopSqlSetter, _, _) = this.Visitor.BuildWithBulk(command);
                    int Execute(string tableName, IEnumerable insertObjs)
                    {
                        int count = 0, index = 0;
                        foreach (var insertObj in insertObjs)
                        {
                            if (index > 0) builder.Append(',');
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                            if (index >= bulkCount)
                            {
                                command.CommandText = builder.ToString();
                                count += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                                builder.Clear();
                                command.Parameters.Clear();
                                firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                                index = 0;
                                continue;
                            }
                            index++;
                        }
                        if (index > 0)
                        {
                            command.CommandText = builder.ToString();
                            count += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                            builder.Clear();
                            command.Parameters.Clear();
                        }
                        return count;
                    };
                    connection.Open();
                    if (isNeedSplit)
                    {
                        var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                        foreach (var tabledInsertObj in tabledInsertObjs)
                        {
                            firstSqlSetter.Invoke(command.Parameters, builder, tabledInsertObj.Key);
                            result += Execute(tabledInsertObj.Key, tabledInsertObj.Value);
                        }
                    }
                    else
                    {
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        result = Execute(tableName, insertObjs);
                    }
                    builder.Clear();
                    break;
                }
            default:
                //默认单条
                command.CommandText = this.Visitor.BuildCommand(command, false, out _);
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
                    var insertObjs = this.DialectVisitor.BuildWithBulkCopy();
                    Type insertObjType = null;
                    foreach (var insertObj in insertObjs)
                    {
                        insertObjType = insertObj.GetType();
                        break;
                    }
                    var dialectOrmProvider = this.OrmProvider as PostgreSqlProvider;
                    var sqlVisitor = this.Visitor as SqlVisitor;
                    if (this.DbContext.ShardingProvider != null && this.DbContext.ShardingProvider.TryGetTableSharding(entityType, out var shardingTable))
                    {
                        var isNeedSplit = this.Visitor.Tables[0].Body == null;
                        if (isNeedSplit)
                        {
                            var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                            foreach (var tabledInsertObj in tabledInsertObjs)
                            {
                                result += await dialectOrmProvider.ExecuteBulkCopyAsync(false, this.DbContext, sqlVisitor, connection, insertObjType, tabledInsertObj.Value, cancellationToken, tabledInsertObj.Key);
                            }
                        }
                        else result = await dialectOrmProvider.ExecuteBulkCopyAsync(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, cancellationToken, this.Visitor.Tables[0].Body);
                    }
                    else result = await dialectOrmProvider.ExecuteBulkCopyAsync(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, cancellationToken);
                    break;
                }
            case ActionMode.Bulk:
                {
                    var builder = new StringBuilder();
                    (var isNeedSplit, var tableName, var insertObjs, var bulkCount,
                        var firstSqlSetter, var loopSqlSetter, _, _) = this.Visitor.BuildWithBulk(command);
                    async Task<int> Executor(string tableName, IEnumerable insertObjs)
                    {
                        int count = 0, index = 0;
                        foreach (var insertObj in insertObjs)
                        {
                            if (index > 0) builder.Append(',');
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                            if (index >= bulkCount)
                            {
                                command.CommandText = builder.ToString();
                                count += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                                builder.Clear();
                                command.Parameters.Clear();
                                firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                                index = 0;
                                continue;
                            }
                            index++;
                        }
                        if (index > 0)
                        {
                            command.CommandText = builder.ToString();
                            count += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                            builder.Clear();
                            command.Parameters.Clear();
                        }
                        return count;
                    };
                    await connection.OpenAsync(cancellationToken);
                    if (isNeedSplit)
                    {
                        var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
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
public class PostgreSqlCreated<TEntity, TResult> : Created<TEntity>, IPostgreSqlCreated<TEntity, TResult>
{
    #region Constructor
    public PostgreSqlCreated(DbContext dbContext, ICreateVisitor visitor)
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
public class PostgreSqlBulkCreated<TEntity, TResult> : Created<TEntity>, IPostgreSqlBulkCreated<TEntity, TResult>
{
    #region Constructor
    public PostgreSqlBulkCreated(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Execute
    public new List<TResult> Execute()
    {
        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var dialectVisitor = this.Visitor as PostgreSqlCreateVisitor;
        if (!string.IsNullOrEmpty(dialectVisitor.FromSql))
        {
            if (dialectVisitor.IsNeedFetchShardingTables)
                this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);

            command.CommandText = dialectVisitor.BuildCommand(command, false, out var readerFields);
            connection.Open();
            using var reader = command.ExecuteReader(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess);
            while (reader.Read())
            {
                result.Add(reader.ToEntity<TResult>(this.DbContext, readerFields));
            }
        }
        else
        {
            (var isNeedSplit, var tableName, var insertObjs, var bulkCount, var firstSqlSetter,
                var loopSqlSetter, var tailSql, var readerFields) = this.Visitor.BuildWithBulk(command);

            var entityType = typeof(TEntity);
            var resultType = typeof(TResult);
            var builder = new StringBuilder();
            Action<DbContext, List<TResult>, ITheaDataReader> initializer = null;
            if (resultType.IsEntityType(out _))
            {
                initializer = (dbContext, result, reader) =>
                {
                    while (reader.Read())
                    {
                        result.Add(reader.ToEntity<TResult>(dbContext, readerFields));
                    }
                };
            }
            else
            {
                initializer = (dbContext, result, reader) =>
                {
                    while (reader.Read())
                    {
                        result.Add(reader.ToValue<TResult>(dbContext));
                    }
                };
            }

            void Execute(string tableName, IEnumerable insertObjs)
            {
                int index = 0;
                foreach (var insertObj in insertObjs)
                {
                    if (index > 0) builder.Append(',');
                    loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                    if (index >= bulkCount)
                    {
                        builder.Append(tailSql);
                        command.CommandText = builder.ToString();
                        using var reader = command.ExecuteReader(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess);
                        initializer.Invoke(this.DbContext, result, reader);
                        reader.Dispose();
                        builder.Clear();
                        command.Parameters.Clear();
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    builder.Append(tailSql);
                    command.CommandText = builder.ToString();
                    using var reader = command.ExecuteReader(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess);
                    initializer.Invoke(this.DbContext, result, reader);
                    reader.Dispose();
                    builder.Clear();
                    command.Parameters.Clear();
                }
            };
            connection.Open();
            if (isNeedSplit)
            {
                var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
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

        var dialectVisitor = this.Visitor as PostgreSqlCreateVisitor;
        if (!string.IsNullOrEmpty(dialectVisitor.FromSql))
        {
            if (dialectVisitor.IsNeedFetchShardingTables)
                await this.DbContext.FetchShardingTablesAsync(this.Visitor as SqlVisitor, cancellationToken);

            command.CommandText = dialectVisitor.BuildCommand(command, false, out var readerFields);
            await connection.OpenAsync(cancellationToken);
            using var reader = await command.ExecuteReaderAsync(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess, cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.ToEntity<TResult>(this.DbContext, readerFields));
            }
        }
        else
        {
            (var isNeedSplit, var tableName, var insertObjs, var bulkCount, var firstSqlSetter,
                var loopSqlSetter, var tailSql, var readerFields) = this.Visitor.BuildWithBulk(command);

            var entityType = typeof(TEntity);
            var resultType = typeof(TResult);
            var builder = new StringBuilder();
            Func<DbContext, List<TResult>, ITheaDataReader, CancellationToken, Task> initializer = null;
            if (resultType.IsEntityType(out _))
            {
                initializer = async (dbContext, result, reader, cancellationToken) =>
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        result.Add(reader.ToEntity<TResult>(dbContext, readerFields));
                    }
                };
            }
            else
            {
                initializer = async (dbContext, result, reader, cancellationToken) =>
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        result.Add(reader.ToValue<TResult>(dbContext));
                    }
                };
            }

            async Task Execute(string tableName, IEnumerable insertObjs)
            {
                int index = 0;
                foreach (var insertObj in insertObjs)
                {
                    if (index > 0) builder.Append(',');
                    loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                    if (index >= bulkCount)
                    {
                        builder.Append(tailSql);
                        command.CommandText = builder.ToString();
                        using var reader = await command.ExecuteReaderAsync(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess, cancellationToken);
                        await initializer.Invoke(this.DbContext, result, reader, cancellationToken);
                        await reader.DisposeAsync();
                        builder.Clear();
                        command.Parameters.Clear();
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    builder.Append(tailSql);
                    command.CommandText = builder.ToString();
                    using var reader = await command.ExecuteReaderAsync(CommandSqlType.BulkInsert, CommandBehavior.SequentialAccess, cancellationToken);
                    await initializer.Invoke(this.DbContext, result, reader, cancellationToken);
                    await reader.DisposeAsync();
                    builder.Clear();
                    command.Parameters.Clear();
                }
            };
            await connection.OpenAsync(cancellationToken);
            if (isNeedSplit)
            {
                var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
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