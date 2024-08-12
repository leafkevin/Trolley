using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.SqlServer;

public class SqlServerCreated<TEntity> : Created<TEntity>, ISqlServerCreated<TEntity>
{
    #region Properties
    public SqlServerCreateVisitor DialectVisitor { get; protected set; }
    #endregion

    #region Constructor
    public SqlServerCreated(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as SqlServerCreateVisitor;
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
        CommandEventArgs eventArgs = null;
        try
        {
            bool isNeedSplit = false;
            var entityType = typeof(TEntity);
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.BulkCopy:
                    (insertObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();

                    Type insertObjType = null;
                    foreach (var insertObj in insertObjs)
                    {
                        insertObjType = insertObj.GetType();
                        break;
                    }
                    if (this.DbContext.ShardingProvider.TryGetShardingTable(entityType, out var shardingTable))
                    {
                        isNeedSplit = this.Visitor.Tables[0].Body == null;
                        if (isNeedSplit)
                        {
                            var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                            foreach (var tabledInsertObj in tabledInsertObjs)
                            {
                                eventArgs = this.DbContext.AddCommandBeforeFilter(CommandSqlType.BulkCopyInsert, eventArgs);
                                result += this.ExecuteBulkCopy(insertObjType, tabledInsertObj.Value, timeoutSeconds, tabledInsertObj.Key);
                            }
                        }
                        else
                        {
                            eventArgs = this.DbContext.AddCommandBeforeFilter(CommandSqlType.BulkCopyInsert, eventArgs);
                            result = this.ExecuteBulkCopy(insertObjType, insertObjs, timeoutSeconds, this.Visitor.Tables[0].Body);
                        }
                    }
                    else
                    {
                        eventArgs = this.DbContext.AddCommandBeforeFilter(CommandSqlType.BulkCopyInsert, eventArgs);
                        result = this.ExecuteBulkCopy(insertObjType, insertObjs, timeoutSeconds);
                    }
                    break;
                case ActionMode.Bulk:
                    command = this.DbContext.CreateCommand();
                    var builder = new StringBuilder();
                    (isNeedSplit, var tableName, insertObjs, var bulkCount,
                        var firstSqlSetter, var loopSqlSetter, _) = this.Visitor.BuildWithBulk(command);

                    Action<string> clearCommand = tableName =>
                    {
                        builder.Clear();
                        command.Parameters.Clear();
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                    };
                    Func<string, IEnumerable, int> executor = (tableName, insertObjs) =>
                    {
                        int count = 0, index = 0;
                        foreach (var insertObj in insertObjs)
                        {
                            if (index > 0) builder.Append(',');
                            loopSqlSetter.Invoke(command.Parameters, builder, insertObj, index.ToString());
                            if (index >= bulkCount)
                            {
                                command.CommandText = builder.ToString();
                                eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.BulkInsert, eventArgs);
                                count += command.ExecuteNonQuery();
                                clearCommand.Invoke(tableName);
                                index = 0;
                                continue;
                            }
                            index++;
                        }
                        if (index > 0)
                        {
                            command.CommandText = builder.ToString();
                            eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.BulkInsert, eventArgs);
                            count += command.ExecuteNonQuery();
                        }
                        return count;
                    };
                    this.DbContext.Open();
                    if (isNeedSplit)
                    {
                        var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                        foreach (var tabledInsertObj in tabledInsertObjs)
                        {
                            firstSqlSetter.Invoke(command.Parameters, builder, tabledInsertObj.Key);
                            result += executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
                        }
                    }
                    else
                    {
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        result = executor.Invoke(tableName, insertObjs);
                    }
                    builder.Clear();
                    builder = null;
                    break;
                default:
                    //默认单条
                    command = this.DbContext.CreateCommand();
                    command.CommandText = this.Visitor.BuildCommand(command, false, out _);
                    this.DbContext.Open();
                    eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.Insert);
                    result = command.ExecuteNonQuery();
                    break;
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            var sqlType = this.Visitor.ActionMode switch
            {
                ActionMode.BulkCopy => CommandSqlType.BulkCopyInsert,
                ActionMode.Bulk => CommandSqlType.BulkInsert,
                _ => CommandSqlType.Insert
            };
            this.DbContext.AddCommandFailedFilter(command, sqlType, eventArgs, exception);
        }
        finally
        {
            var sqlType = this.Visitor.ActionMode switch
            {
                ActionMode.BulkCopy => CommandSqlType.BulkCopyInsert,
                ActionMode.Bulk => CommandSqlType.BulkInsert,
                _ => CommandSqlType.Insert
            };
            this.DbContext.AddCommandAfterFilter(command, sqlType, eventArgs, exception == null, exception);
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
        CommandEventArgs eventArgs = null;
        try
        {
            bool isNeedSplit = false;
            var entityType = typeof(TEntity);
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.BulkCopy:
                    (insertObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                    bool isOpened = false;
                    Type insertObjType = null;
                    foreach (var insertObj in insertObjs)
                    {
                        insertObjType = insertObj.GetType();
                        break;
                    }
                    if (this.DbContext.ShardingProvider.TryGetShardingTable(entityType, out _))
                    {
                        isNeedSplit = this.Visitor.Tables[0].Body == null;
                        if (isNeedSplit)
                        {
                            var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                            foreach (var tabledInsertObj in tabledInsertObjs)
                            {
                                eventArgs = this.DbContext.AddCommandBeforeFilter(CommandSqlType.BulkCopyInsert, eventArgs);
                                result += await this.ExecuteBulkCopyAsync(insertObjType, tabledInsertObj.Value, timeoutSeconds, cancellationToken, tabledInsertObj.Key);
                                if (!isOpened) isOpened = true;
                            }
                        }
                        else
                        {
                            eventArgs = this.DbContext.AddCommandBeforeFilter(CommandSqlType.BulkCopyInsert, eventArgs);
                            result = await this.ExecuteBulkCopyAsync(insertObjType, insertObjs, timeoutSeconds, cancellationToken, this.Visitor.Tables[0].Body);
                        }
                    }
                    else
                    {
                        eventArgs = this.DbContext.AddCommandBeforeFilter(CommandSqlType.BulkCopyInsert, eventArgs);
                        result = await this.ExecuteBulkCopyAsync(insertObjType, insertObjs, timeoutSeconds, cancellationToken);
                    }
                    break;
                case ActionMode.Bulk:
                    command = this.DbContext.CreateDbCommand();
                    var sqlBuilder = new StringBuilder();
                    (isNeedSplit, var tableName, insertObjs, var bulkCount,
                        var firstSqlSetter, var loopSqlSetter, _) = this.Visitor.BuildWithBulk(command);

                    Action<string> clearCommand = tableName =>
                    {
                        sqlBuilder.Clear();
                        command.Parameters.Clear();
                        firstSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName);
                    };
                    Func<string, IEnumerable, Task<int>> executor = async (tableName, insertObjs) =>
                    {
                        int count = 0, index = 0;
                        foreach (var insertObj in insertObjs)
                        {
                            if (index > 0) sqlBuilder.Append(',');
                            loopSqlSetter.Invoke(command.Parameters, sqlBuilder, insertObj, index.ToString());
                            if (index >= bulkCount)
                            {
                                command.CommandText = sqlBuilder.ToString();
                                eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.BulkInsert, eventArgs);
                                count += await command.ExecuteNonQueryAsync(cancellationToken);
                                clearCommand.Invoke(tableName);
                                index = 0;
                                continue;
                            }
                            index++;
                        }
                        if (index > 0)
                        {
                            command.CommandText = sqlBuilder.ToString();
                            eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.BulkInsert, eventArgs);
                            count += await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                        return count;
                    };
                    await this.DbContext.OpenAsync(cancellationToken);
                    if (isNeedSplit)
                    {
                        var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                        foreach (var tabledInsertObj in tabledInsertObjs)
                        {
                            firstSqlSetter.Invoke(command.Parameters, sqlBuilder, tabledInsertObj.Key);
                            result += await executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
                        }
                    }
                    else
                    {
                        firstSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName);
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
                    eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.Insert);
                    result = await command.ExecuteNonQueryAsync(cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            var sqlType = this.Visitor.ActionMode switch
            {
                ActionMode.BulkCopy => CommandSqlType.BulkCopyInsert,
                ActionMode.Bulk => CommandSqlType.BulkInsert,
                _ => CommandSqlType.Insert
            };
            this.DbContext.AddCommandFailedFilter(command, sqlType, eventArgs, exception);
        }
        finally
        {
            var sqlType = this.Visitor.ActionMode switch
            {
                ActionMode.BulkCopy => CommandSqlType.BulkCopyInsert,
                ActionMode.Bulk => CommandSqlType.BulkInsert,
                _ => CommandSqlType.Insert
            };
            this.DbContext.AddCommandAfterFilter(command, sqlType, eventArgs, exception == null, exception);
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
    private int ExecuteBulkCopy(Type insertObjType, IEnumerable insertObjs, int? timeoutSeconds, string tableName = null)
    {
        var entityMapper = this.Visitor.Tables[0].Mapper;
        var memberMappers = this.Visitor.GetRefMemberMappers(insertObjType, entityMapper);
        var dataTable = this.Visitor.ToDataTable(insertObjType, insertObjs, memberMappers, tableName ?? entityMapper.TableName);
        if (dataTable.Rows.Count == 0) return 0;

        this.DbContext.Open();
        var connection = this.DbContext.Connection.BaseConnection as SqlConnection;
        var transaction = this.DbContext.Transaction as SqlTransaction;
        var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);
        if (timeoutSeconds.HasValue) bulkCopy.BulkCopyTimeout = timeoutSeconds.Value;
        bulkCopy.DestinationTableName = dataTable.TableName;
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, dataTable.Columns[i].ColumnName));
        }
        bulkCopy.WriteToServer(dataTable);
        return dataTable.Rows.Count;
    }
    private async Task<int> ExecuteBulkCopyAsync(Type insertObjType, IEnumerable insertObjs, int? timeoutSeconds, CancellationToken cancellationToken = default, string tableName = null)
    {
        var entityMapper = this.Visitor.Tables[0].Mapper;
        var memberMappers = this.Visitor.GetRefMemberMappers(insertObjType, entityMapper);
        var dataTable = this.Visitor.ToDataTable(insertObjType, insertObjs, memberMappers, tableName ?? entityMapper.TableName);
        if (dataTable.Rows.Count == 0) return 0;

        await this.DbContext.OpenAsync(cancellationToken);
        var connection = this.DbContext.Connection.BaseConnection as SqlConnection;
        var transaction = this.DbContext.Transaction as SqlTransaction;
        var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);
        if (timeoutSeconds.HasValue) bulkCopy.BulkCopyTimeout = timeoutSeconds.Value;
        bulkCopy.DestinationTableName = dataTable.TableName;
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, dataTable.Columns[i].ColumnName));
        }
        await bulkCopy.WriteToServerAsync(dataTable);
        return dataTable.Rows.Count;
    }
    #endregion
}
public class SqlServerCreated<TEntity, TResult> : Created<TEntity>, ISqlServerCreated<TEntity, TResult>
{
    #region Constructor
    public SqlServerCreated(DbContext dbContext, ICreateVisitor visitor)
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
public class SqlServerBulkCreated<TEntity, TResult> : Created<TEntity>, ISqlServerBulkCreated<TEntity, TResult>
{
    #region Constructor
    public SqlServerBulkCreated(DbContext dbContext, ICreateVisitor visitor)
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
