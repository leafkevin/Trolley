using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
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
        Exception exception = null;
        IEnumerable insertObjs = null;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
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
                    if (this.DbContext.ShardingProvider != null && this.DbContext.ShardingProvider.TryGetTableSharding(entityType, out var shardingTable))
                    {
                        isNeedSplit = this.Visitor.Tables[0].Body == null;
                        if (isNeedSplit)
                        {
                            var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                            foreach (var tabledInsertObj in tabledInsertObjs)
                            {
                                eventArgs = this.DbContext.AddCommandBeforeFilter(connection, CommandSqlType.BulkCopyInsert, eventArgs);
                                result += this.ExecuteBulkCopy(connection, insertObjType, tabledInsertObj.Value, timeoutSeconds, tabledInsertObj.Key);
                            }
                        }
                        else
                        {
                            eventArgs = this.DbContext.AddCommandBeforeFilter(connection, CommandSqlType.BulkCopyInsert, eventArgs);
                            result = this.ExecuteBulkCopy(connection, insertObjType, insertObjs, timeoutSeconds, this.Visitor.Tables[0].Body);
                        }
                    }
                    else
                    {
                        eventArgs = this.DbContext.AddCommandBeforeFilter(connection, CommandSqlType.BulkCopyInsert, eventArgs);
                        result = this.ExecuteBulkCopy(connection, insertObjType, insertObjs, timeoutSeconds);
                    }
                    break;
                case ActionMode.Bulk:
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
                                eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.BulkInsert, eventArgs);
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
                            eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.BulkInsert, eventArgs);
                            count += command.ExecuteNonQuery();
                        }
                        return count;
                    };
                    this.DbContext.Open(connection);
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
                    command.CommandText = this.Visitor.BuildCommand(command, false, out _);
                    this.DbContext.Open(connection);
                    eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.Insert);
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
            this.DbContext.AddCommandFailedFilter(connection, command, sqlType, eventArgs, exception);
        }
        finally
        {
            var sqlType = this.Visitor.ActionMode switch
            {
                ActionMode.BulkCopy => CommandSqlType.BulkCopyInsert,
                ActionMode.Bulk => CommandSqlType.BulkInsert,
                _ => CommandSqlType.Insert
            };
            this.DbContext.AddCommandAfterFilter(connection, command, sqlType, eventArgs, exception == null, exception);
            command?.Dispose();
            if (isNeedClose) this.Close(connection);
        }
        if (exception != null) throw exception;
        return result;
    }
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        Exception exception = null;
        IEnumerable insertObjs = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterDbCommand();
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
                    if (this.DbContext.ShardingProvider != null && this.DbContext.ShardingProvider.TryGetTableSharding(entityType, out _))
                    {
                        isNeedSplit = this.Visitor.Tables[0].Body == null;
                        if (isNeedSplit)
                        {
                            var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                            foreach (var tabledInsertObj in tabledInsertObjs)
                            {
                                eventArgs = this.DbContext.AddCommandBeforeFilter(connection, CommandSqlType.BulkCopyInsert, eventArgs);
                                result += await this.ExecuteBulkCopyAsync(connection, insertObjType, tabledInsertObj.Value, timeoutSeconds, cancellationToken, tabledInsertObj.Key);
                                if (!isOpened) isOpened = true;
                            }
                        }
                        else
                        {
                            eventArgs = this.DbContext.AddCommandBeforeFilter(connection, CommandSqlType.BulkCopyInsert, eventArgs);
                            result = await this.ExecuteBulkCopyAsync(connection, insertObjType, insertObjs, timeoutSeconds, cancellationToken, this.Visitor.Tables[0].Body);
                        }
                    }
                    else
                    {
                        eventArgs = this.DbContext.AddCommandBeforeFilter(connection, CommandSqlType.BulkCopyInsert, eventArgs);
                        result = await this.ExecuteBulkCopyAsync(connection, insertObjType, insertObjs, timeoutSeconds, cancellationToken);
                    }
                    break;
                case ActionMode.Bulk:
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
                                eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.BulkInsert, eventArgs);
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
                            eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.BulkInsert, eventArgs);
                            count += await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                        return count;
                    };
                    await this.DbContext.OpenAsync(connection, cancellationToken);
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
                    command.CommandText = this.Visitor.BuildCommand(command, false, out _);
                    await this.DbContext.OpenAsync(connection, cancellationToken);
                    eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.Insert);
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
            this.DbContext.AddCommandFailedFilter(connection, command, sqlType, eventArgs, exception);
        }
        finally
        {
            var sqlType = this.Visitor.ActionMode switch
            {
                ActionMode.BulkCopy => CommandSqlType.BulkCopyInsert,
                ActionMode.Bulk => CommandSqlType.BulkInsert,
                _ => CommandSqlType.Insert
            };
            this.DbContext.AddCommandAfterFilter(connection, command, sqlType, eventArgs, exception == null, exception);
            if (command != null)
            {
                command.Parameters.Clear();
                await command.DisposeAsync();
            }
            if (isNeedClose) await this.CloseAsync(connection);
        }
        if (exception != null) throw exception;
        return result;
    }
    private int ExecuteBulkCopy(TheaConnection connection, Type insertObjType, IEnumerable insertObjs, int? timeoutSeconds, string tableName = null)
    {
        var entityMapper = this.Visitor.Tables[0].Mapper;
        var memberMappers = this.Visitor.GetRefMemberMappers(insertObjType, entityMapper);
        var dataTable = this.Visitor.ToDataTable(insertObjType, insertObjs, memberMappers, tableName ?? entityMapper.TableName);
        if (dataTable.Rows.Count == 0) return 0;

        this.DbContext.Open(connection);
        var dbConnection = connection.BaseConnection as SqlConnection;
        var transaction = this.DbContext.Transaction as SqlTransaction;
        var bulkCopy = new SqlBulkCopy(dbConnection, SqlBulkCopyOptions.Default, transaction);
        if (timeoutSeconds.HasValue) bulkCopy.BulkCopyTimeout = timeoutSeconds.Value;
        bulkCopy.DestinationTableName = dataTable.TableName;
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, dataTable.Columns[i].ColumnName));
        }
        bulkCopy.WriteToServer(dataTable);
        return dataTable.Rows.Count;
    }
    private async Task<int> ExecuteBulkCopyAsync(TheaConnection connection, Type insertObjType, IEnumerable insertObjs, int? timeoutSeconds, CancellationToken cancellationToken = default, string tableName = null)
    {
        var entityMapper = this.Visitor.Tables[0].Mapper;
        var memberMappers = this.Visitor.GetRefMemberMappers(insertObjType, entityMapper);
        var dataTable = this.Visitor.ToDataTable(insertObjType, insertObjs, memberMappers, tableName ?? entityMapper.TableName);
        if (dataTable.Rows.Count == 0) return 0;

        await this.DbContext.OpenAsync(connection, cancellationToken);
        var dbConnection = connection.BaseConnection as SqlConnection;
        var transaction = this.DbContext.Transaction as SqlTransaction;
        var bulkCopy = new SqlBulkCopy(dbConnection, SqlBulkCopyOptions.Default, transaction);
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
