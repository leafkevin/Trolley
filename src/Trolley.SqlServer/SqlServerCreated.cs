﻿using Microsoft.Data.SqlClient;
using System;
using System.Collections;
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
                                result += this.ExecuteBulkCopy(ref isOpened, entityType, tabledInsertObj.Value, timeoutSeconds, tabledInsertObj.Key);
                            }
                        }
                        else result = this.ExecuteBulkCopy(ref isOpened, entityType, insertObjs, timeoutSeconds, this.Visitor.Tables[0].Body);
                    }
                    else result = this.ExecuteBulkCopy(ref isOpened, entityType, insertObjs, timeoutSeconds);
                    break;
                case ActionMode.Bulk:
                    command = this.DbContext.CreateCommand();
                    var builder = new StringBuilder();
                    (isNeedSplit, var tableName, insertObjs, var bulkCount, var firstInsertObj,
                        var headSqlSetter, var commandInitializer, _) = this.Visitor.BuildWithBulk(command);

                    Action<string, object> clearCommand = (tableName, insertObj) =>
                    {
                        builder.Clear();
                        command.Parameters.Clear();
                        headSqlSetter.Invoke(command.Parameters, builder, tableName, insertObj);
                    };
                    Func<string, IEnumerable, int> executor = (tableName, insertObjs) =>
                    {
                        var isFirst = true;
                        int count = 0, index = 0;
                        foreach (var insertObj in insertObjs)
                        {
                            if (index > 0) builder.Append(',');
                            commandInitializer.Invoke(command.Parameters, builder, insertObj, index.ToString());
                            if (index >= bulkCount)
                            {
                                command.CommandText = builder.ToString();
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
                            command.CommandText = builder.ToString();
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
                            headSqlSetter.Invoke(command.Parameters, builder, tabledInsertObj.Key, tabledInsertObj);
                            result += executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
                        }
                    }
                    else
                    {
                        headSqlSetter.Invoke(command.Parameters, builder, tableName, firstInsertObj);
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
                                result += await this.ExecuteBulkCopyAsync(isOpened, entityType, tabledInsertObj.Value, timeoutSeconds, cancellationToken, tabledInsertObj.Key);
                                if (!isOpened) isOpened = true;
                            }
                        }
                        else result = await this.ExecuteBulkCopyAsync(isOpened, entityType, insertObjs, timeoutSeconds, cancellationToken, this.Visitor.Tables[0].Body);
                    }
                    else result = await this.ExecuteBulkCopyAsync(isOpened, entityType, insertObjs, timeoutSeconds, cancellationToken);
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
    private int ExecuteBulkCopy(ref bool isOpened, Type entityType, IEnumerable insertObjs, int? timeoutSeconds, string tableName = null)
    {
        var entityMapper = this.Visitor.Tables[0].Mapper;
        var dataTable = this.Visitor.ToDataTable(entityType, insertObjs, entityMapper, tableName);
        if (dataTable.Rows.Count == 0) return 0;

        if (!isOpened)
        {
            this.DbContext.Open();
            isOpened = true;
        }
        var connection = this.DbContext.Connection as SqlConnection;
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
    private async Task<int> ExecuteBulkCopyAsync(bool isOpened, Type entityType, IEnumerable insertObjs, int? timeoutSeconds, CancellationToken cancellationToken = default, string tableName = null)
    {
        var entityMapper = this.Visitor.Tables[0].Mapper;
        var dataTable = this.Visitor.ToDataTable(entityType, insertObjs, entityMapper, tableName);
        if (dataTable.Rows.Count == 0) return 0;

        if (!isOpened)
            await this.DbContext.OpenAsync(cancellationToken);
        var connection = this.DbContext.Connection as SqlConnection;
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

