﻿using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.PostgreSql;

public class PostgreSqlCreated<TEntity> : Created<TEntity>, IPostgreSqlCreated<TEntity>
{
    #region Properties
    public PostgreSqlCreateVisitor DialectVisitor { get; protected set; }
    public IOrmProvider OrmProvider => this.Visitor.OrmProvider;
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
        IDbCommand command = null;
        Exception exception = null;
        IEnumerable insertObjs = null;
        bool isNeedClose = this.DbContext.IsNeedClose;
        try
        {
            bool isNeedSplit = false;
            var entityType = typeof(TEntity);
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.BulkCopy:
                    (insertObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();

                    bool isOpened = false;
                    if (this.DbContext.ShardingProvider.TryGetShardingTable(entityType, out var shardingTable))
                    {
                        isNeedSplit = this.Visitor.Tables[0].Body == null;
                        if (isNeedSplit)
                        {
                            var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                            foreach (var tabledInsertObj in tabledInsertObjs)
                            {
                                result += this.ExecuteBulkCopy(ref isOpened, tabledInsertObj.Value, timeoutSeconds, tabledInsertObj.Key);
                            }
                        }
                        else result = this.ExecuteBulkCopy(ref isOpened, insertObjs, timeoutSeconds, this.Visitor.Tables[0].Body);
                    }
                    else result = this.ExecuteBulkCopy(ref isOpened, insertObjs, timeoutSeconds);
                    break;
                case ActionMode.Bulk:
                    command = this.DbContext.CreateCommand();
                    var sqlBuilder = new StringBuilder();
                    (isNeedSplit, var tableName, insertObjs, var bulkCount, var firstInsertObj,
                        var headSqlSetter, var commandInitializer, _) = this.Visitor.BuildWithBulk(command);

                    Action<string, object> clearCommand = (tableName, insertObj) =>
                    {
                        sqlBuilder.Clear();
                        command.Parameters.Clear();
                        headSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName, insertObj);
                    };
                    Func<string, IEnumerable, int> executor = (tableName, insertObjs) =>
                    {
                        var isFirst = true;
                        int count = 0, index = 0;
                        foreach (var insertObj in insertObjs)
                        {
                            if (index > 0) sqlBuilder.Append(',');
                            commandInitializer.Invoke(command.Parameters, sqlBuilder, insertObj, index.ToString());
                            if (index >= bulkCount)
                            {
                                command.CommandText = sqlBuilder.ToString();
                                if (isFirst)
                                {
                                    this.DbContext.Open();
                                    isFirst = false;
                                }
                                count += command.ExecuteNonQuery();
                                clearCommand.Invoke(tableName, insertObj);
                                index = 0;
                                continue;
                            }
                            index++;
                        }
                        if (index > 0)
                        {
                            command.CommandText = sqlBuilder.ToString();
                            if (isFirst) this.DbContext.Open();
                            count += command.ExecuteNonQuery();
                        }
                        return count;
                    };

                    if (isNeedSplit)
                    {
                        var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                        foreach (var tabledInsertObj in tabledInsertObjs)
                        {
                            headSqlSetter.Invoke(command.Parameters, sqlBuilder, tabledInsertObj.Key, tabledInsertObj);
                            result += executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
                        }
                    }
                    else
                    {
                        headSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName, firstInsertObj);
                        result = executor.Invoke(tableName, insertObjs);
                    }
                    sqlBuilder.Clear();
                    sqlBuilder = null;
                    break;
                default:
                    //默认单条
                    command = this.DbContext.CreateCommand();
                    command.CommandText = this.Visitor.BuildCommand(command, false, out _);
                    this.DbContext.Open();
                    result = command.ExecuteNonQuery();
                    break;
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            command?.Dispose();
            if (isNeedClose) this.Close();
        }
        if (exception != null) throw exception;
        return result;
    }
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        DbCommand command = null;
        Exception exception = null;
        IEnumerable insertObjs = null;
        bool isNeedClose = this.DbContext.IsNeedClose;
        try
        {
            bool isNeedSplit = false;
            var entityType = typeof(TEntity);
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.BulkCopy:
                    (insertObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                    bool isOpened = false;

                    if (this.DbContext.ShardingProvider.TryGetShardingTable(entityType, out _))
                    {
                        isNeedSplit = this.Visitor.Tables[0].Body == null;
                        if (isNeedSplit)
                        {
                            var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                            foreach (var tabledInsertObj in tabledInsertObjs)
                            {
                                result += await this.ExecuteBulkCopyAsync(isOpened, tabledInsertObj.Value, timeoutSeconds, cancellationToken, tabledInsertObj.Key);
                                if (!isOpened) isOpened = true;
                            }
                        }
                        else result = await this.ExecuteBulkCopyAsync(isOpened, insertObjs, timeoutSeconds, cancellationToken, this.Visitor.Tables[0].Body);
                    }
                    else result = await this.ExecuteBulkCopyAsync(isOpened, insertObjs, timeoutSeconds, cancellationToken);
                    break;
                case ActionMode.Bulk:
                    command = this.DbContext.CreateDbCommand();
                    var sqlBuilder = new StringBuilder();
                    (isNeedSplit, var tableName, insertObjs, var bulkCount, var firstInsertObj,
                        var headSqlSetter, var commandInitializer, _) = this.Visitor.BuildWithBulk(command);

                    Action<string, object> clearCommand = (tableName, insertObj) =>
                    {
                        sqlBuilder.Clear();
                        command.Parameters.Clear();
                        headSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName, insertObj);
                    };
                    Func<string, IEnumerable, Task<int>> executor = async (tableName, insertObjs) =>
                    {
                        var isFirst = true;
                        int count = 0, index = 0;
                        foreach (var insertObj in insertObjs)
                        {
                            if (index > 0) sqlBuilder.Append(',');
                            commandInitializer.Invoke(command.Parameters, sqlBuilder, insertObj, index.ToString());
                            if (index >= bulkCount)
                            {
                                command.CommandText = sqlBuilder.ToString();
                                if (isFirst)
                                {
                                    await this.DbContext.OpenAsync(cancellationToken);
                                    isFirst = false;
                                }
                                count += await command.ExecuteNonQueryAsync(cancellationToken);
                                clearCommand.Invoke(tableName, insertObj);
                                index = 0;
                                continue;
                            }
                            index++;
                        }
                        if (index > 0)
                        {
                            command.CommandText = sqlBuilder.ToString();
                            if (isFirst) await this.DbContext.OpenAsync(cancellationToken);
                            count += await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                        return count;
                    };

                    if (isNeedSplit)
                    {
                        var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                        foreach (var tabledInsertObj in tabledInsertObjs)
                        {
                            headSqlSetter.Invoke(command.Parameters, sqlBuilder, tabledInsertObj.Key, tabledInsertObj);
                            result += await executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
                        }
                    }
                    else
                    {
                        headSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName, firstInsertObj);
                        result = await executor.Invoke(tableName, insertObjs);
                    }
                    sqlBuilder.Clear();
                    sqlBuilder = null;
                    break;
                default:
                    //默认单条
                    command = this.DbContext.CreateDbCommand();
                    command.CommandText = this.Visitor.BuildCommand(command, false, out _);
                    await this.DbContext.OpenAsync(cancellationToken);
                    result = await command.ExecuteNonQueryAsync(cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            if (command != null)
            {
                command.Parameters.Clear();
                await command.DisposeAsync();
            }
            if (isNeedClose) await this.CloseAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    private int ExecuteBulkCopy(ref bool isOpened, IEnumerable insertObjs, int? timeoutSeconds, string tableName = null)
    {
        if (!isOpened)
        {
            this.DbContext.Open();
            isOpened = true;
        }
        var entityMapper = this.Visitor.Tables[0].Mapper;
        tableName ??= entityMapper.TableName;
        var builder = new StringBuilder($"COPY {this.OrmProvider.GetTableName(tableName)}(");

        for (int i = 0; i < entityMapper.MemberMaps.Count; i++)
        {
            if (i > 0) builder.Append(',');
            var memberMapper = entityMapper.MemberMaps[i];
            builder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));
        }
        builder.Append(") FROM STDIN BINARY");
        var connection = this.DbContext.Connection as NpgsqlConnection;
        using var writer = connection.BeginBinaryImport(builder.ToString());
        int result = 0;
        foreach (var insertObj in insertObjs)
        {
            writer.StartRow();
            foreach (var memberMapper in entityMapper.MemberMaps)
                writer.Write(insertObj, (NpgsqlDbType)memberMapper.NativeDbType);
            result++;
        }
        writer.Complete();
        builder.Clear();
        builder = null;
        return result;
    }
    private async Task<int> ExecuteBulkCopyAsync(bool isOpened, IEnumerable insertObjs, int? timeoutSeconds, CancellationToken cancellationToken = default, string tableName = null)
    {
        if (!isOpened)
        {
            await this.DbContext.OpenAsync(cancellationToken);
            isOpened = true;
        }
        var entityMapper = this.Visitor.Tables[0].Mapper;
        tableName ??= entityMapper.TableName;
        var builder = new StringBuilder($"COPY {this.OrmProvider.GetTableName(tableName)}(");

        for (int i = 0; i < entityMapper.MemberMaps.Count; i++)
        {
            if (i > 0) builder.Append(',');
            var memberMapper = entityMapper.MemberMaps[i];
            builder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));
        }
        builder.Append(") FROM STDIN BINARY");
        var connection = this.DbContext.Connection as NpgsqlConnection;
        using var writer = await connection.BeginBinaryImportAsync(builder.ToString(), cancellationToken);
        int result = 0;
        foreach (var insertObj in insertObjs)
        {
            await writer.StartRowAsync(cancellationToken);
            foreach (var memberMapper in entityMapper.MemberMaps)
                await writer.WriteAsync(insertObj, (NpgsqlDbType)memberMapper.NativeDbType, cancellationToken);
            result++;
        }
        await writer.CompleteAsync(cancellationToken);
        builder.Clear();
        builder = null;
        return result;
    }
    #endregion
}
public class PostgreSqlCreated<TEntity, TResult> : PostgreSqlContinuedCreate<TEntity>, IPostgreSqlCreated<TEntity, TResult>
{
    #region Constructor
    public PostgreSqlCreated(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Execute
    public new TResult Execute() => this.DbContext.CreateResult<TResult>((command, dbContext) =>
    {
        command.CommandText = this.Visitor.BuildCommand(command, false, out var readerFields);
        return readerFields;
    });
    public new async Task<TResult> ExecuteAsync(CancellationToken cancellationToken) => await this.DbContext.CreateResultAsync<TResult>((command, dbContext) =>
    {
        command.CommandText = this.Visitor.BuildCommand(command, false, out var readerFields);
        return readerFields;
    }, cancellationToken);
    #endregion

    #region ExecuteIdentity
    /// <summary>
    /// 不支持的方法调用，调用Outpt方法后此方法无效，请使用Execute方法
    /// </summary>
    /// <returns>返回自增长主键值</returns>
    public override int ExecuteIdentity()
        => throw new NotSupportedException("不支持的方法调用，调用Outpt方法后此方法无效，请使用Execute方法");
    /// <summary>
    /// 不支持的方法调用，调用Outpt方法后此方法无效，请使用ExecuteAsync方法
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回自增长主键值</returns>
    public override Task<int> ExecuteIdentityAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，调用Outpt方法后此方法无效，请使用ExecuteAsync方法");
    /// <summary>
    /// 不支持的方法调用，调用Outpt方法后此方法无效，请使用Execute方法
    /// </summary>
    /// <returns>返回自增长主键值</returns>   
    public override long ExecuteIdentityLong()
        => throw new NotSupportedException("不支持的方法调用，调用Outpt方法后此方法无效，请使用Execute方法");
    /// <summary>
    /// 不支持的方法调用，调用Outpt方法后此方法无效，请使用ExecuteAsync方法
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回自增长主键值</returns>
    public override Task<long> ExecuteIdentityLongAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，调用Outpt方法后此方法无效，请使用ExecuteAsync方法");
    #endregion
}
public class PostgreSqlBulkCreated<TEntity, TResult> : PostgreSqlBulkContinuedCreate<TEntity>, IPostgreSqlBulkCreated<TEntity, TResult>
{
    #region Constructor
    public PostgreSqlBulkCreated(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Execute
    public new List<TResult> Execute() => this.DbContext.CreateResult<TResult>(this.Visitor);
    public new async Task<List<TResult>> ExecuteAsync(CancellationToken cancellationToken)
        => await this.DbContext.CreateResultAsync<TResult>(this.Visitor, cancellationToken);
    #endregion

    #region ExecuteIdentity
    /// <summary>
    /// 不支持的方法调用，调用Outpt方法后此方法无效，请使用Execute方法
    /// </summary>
    /// <returns>返回自增长主键值</returns>
    public override int ExecuteIdentity()
        => throw new NotSupportedException("不支持的方法调用，调用Outpt方法后此方法无效，请使用Execute方法");
    /// <summary>
    /// 不支持的方法调用，调用Outpt方法后此方法无效，请使用ExecuteAsync方法
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回自增长主键值</returns>
    public override Task<int> ExecuteIdentityAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，调用Outpt方法后此方法无效，请使用ExecuteAsync方法");
    /// <summary>
    /// 不支持的方法调用，调用Outpt方法后此方法无效，请使用Execute方法
    /// </summary>
    /// <returns>返回自增长主键值</returns>
    public override long ExecuteIdentityLong()
        => throw new NotSupportedException("不支持的方法调用，调用Outpt方法后此方法无效，请使用Execute方法");
    /// <summary>
    /// 不支持的方法调用，调用Outpt方法后此方法无效，请使用ExecuteAsync方法
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回自增长主键值</returns>
    public override Task<long> ExecuteIdentityLongAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("不支持的方法调用，调用Outpt方法后此方法无效，请使用ExecuteAsync方法");
    #endregion
}
