using MySqlConnector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public class MySqlUpdated<TEntity> : Updated<TEntity>, IMySqlUpdated<TEntity>
{
    #region Properties
    public MySqlUpdateVisitor DialectVisitor { get; protected set; }
    #endregion

    #region Constructor
    public MySqlUpdated(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as MySqlUpdateVisitor;
    }
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        IDbCommand command = null;
        Exception exception = null;
        IEnumerable updateObjs = null;
        bool isNeedClose = this.DbContext.IsNeedClose;
        try
        {
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.Bulk:
                    int index = 0;
                    bool isFirst = true;
                    var sqlBuilder = new StringBuilder();
                    command = this.DbContext.CreateCommand();
                    (updateObjs, var bulkCount, var commandInitializer, var firstCommandInitializer) = this.Visitor.BuildSetBulk(command);
                    firstCommandInitializer?.Invoke(command.Parameters);
                    foreach (var updateObj in updateObjs)
                    {
                        if (index > 0) sqlBuilder.Append(';');
                        commandInitializer.Invoke(sqlBuilder, updateObj, index.ToString());
                        if (index >= bulkCount)
                        {
                            command.CommandText = sqlBuilder.ToString();
                            if (isFirst)
                            {
                                this.DbContext.Open();
                                isFirst = false;
                            }
                            result += command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            firstCommandInitializer?.Invoke(command.Parameters);
                            sqlBuilder.Clear();
                            index = 0;
                            continue;
                        }
                        index++;
                    }
                    if (index > 0)
                    {
                        command.CommandText = sqlBuilder.ToString();
                        if (isFirst) this.DbContext.Open();
                        result += command.ExecuteNonQuery();
                    }
                    sqlBuilder.Clear();
                    sqlBuilder = null;
                    break;
                case ActionMode.BulkCopy:
                    (updateObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                    Type entityType = null;
                    foreach (var updateObj in updateObjs)
                    {
                        entityType = updateObj.GetType();
                        break;
                    }
                    if (entityType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
                    var fromMapper = this.Visitor.Tables[0].Mapper;
                    var memberMappers = this.Visitor.GetRefMemberMappers(entityType, fromMapper);
                    var ormProvider = this.Visitor.OrmProvider;
                    var tableName = ormProvider.GetTableName($"{fromMapper.TableName}_{Guid.NewGuid():N}");
                    //添加临时表
                    var builder = new StringBuilder();
                    builder.AppendLine($"CREATE TEMPORARY TABLE {tableName}(");
                    var pkColumns = new List<string>();
                    foreach (var memberMapper in memberMappers)
                    {
                        var refMemberMapper = memberMapper.RefMemberMapper;
                        var fieldName = ormProvider.GetFieldName(refMemberMapper.FieldName);
                        builder.Append($"{fieldName} {refMemberMapper.DbColumnType}");
                        if (refMemberMapper.IsKey)
                        {
                            builder.Append(" NOT NULL");
                            pkColumns.Add(fieldName);
                        }
                        builder.AppendLine(",");
                    }
                    builder.AppendLine($"PRIMARY KEY({string.Join(',', pkColumns)})");
                    builder.AppendLine(");");
                    command = this.DbContext.CreateCommand();
                    command.CommandText = builder.ToString();
                    this.DbContext.Open();
                    command.ExecuteNonQuery();
                    command.Dispose();

                    var dataTable = this.Visitor.ToDataTable(entityType, updateObjs, fromMapper, tableName);
                    if (dataTable.Rows.Count == 0) return 0;

                    var connection = this.DbContext.Connection as MySqlConnection;
                    var transaction = this.DbContext.Transaction as MySqlTransaction;
                    var bulkCopy = new MySqlBulkCopy(connection, transaction);
                    if (timeoutSeconds.HasValue) bulkCopy.BulkCopyTimeout = timeoutSeconds.Value;
                    bulkCopy.DestinationTableName = dataTable.TableName;
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, dataTable.Columns[i].ColumnName));
                    }
                    var bulkCopyResult = bulkCopy.WriteToServer(dataTable);
                    result = bulkCopyResult.RowsInserted;

                    builder.Clear();
                    builder.Append($"UPDATE {ormProvider.GetTableName(fromMapper.TableName)} a INNER JOIN {tableName} b ON ");
                    for (int i = 0; i < pkColumns.Count; i++)
                    {
                        if (i > 0) builder.Append(" AND ");
                        builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                    }
                    builder.Append(" SET ");
                    int setIndex = 0;
                    for (int i = 0; i < memberMappers.Count; i++)
                    {
                        var fieldName = ormProvider.GetFieldName(memberMappers[i].RefMemberMapper.FieldName);
                        if (pkColumns.Contains(fieldName)) continue;
                        if (setIndex > 0) builder.Append(',');
                        builder.Append($"a.{fieldName}=b.{fieldName}");
                        setIndex++;
                    }

                    command = this.DbContext.CreateCommand();
                    command.CommandText = builder.ToString();
                    result = command.ExecuteNonQuery();
                    command.Dispose();

                    command = this.DbContext.CreateDbCommand();
                    command.CommandText = $"DROP TABLE {tableName}";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    command = null;
                    break;
                default:
                    if (!hasWhere)
                        throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");

                    command = this.DbContext.CreateCommand();
                    command.CommandText = this.Visitor.BuildCommand(command);
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
        IEnumerable updateObjs = null;
        bool isNeedClose = this.DbContext.IsNeedClose;
        try
        {
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.Bulk:
                    int index = 0;
                    bool isFirst = true;
                    var sqlBuilder = new StringBuilder();
                    command = this.DbContext.CreateDbCommand();
                    (updateObjs, var bulkCount, var commandInitializer, var firstCommandInitializer) = this.Visitor.BuildSetBulk(command);
                    firstCommandInitializer?.Invoke(command.Parameters);
                    foreach (var updateObj in updateObjs)
                    {
                        if (index > 0) sqlBuilder.Append(';');
                        commandInitializer.Invoke(sqlBuilder, updateObj, index.ToString());
                        if (index >= bulkCount)
                        {
                            command.CommandText = sqlBuilder.ToString();
                            if (isFirst)
                            {
                                await this.DbContext.OpenAsync(cancellationToken);
                                isFirst = false;
                            }
                            result += await command.ExecuteNonQueryAsync(cancellationToken);
                            command.Parameters.Clear();
                            firstCommandInitializer?.Invoke(command.Parameters);
                            sqlBuilder.Clear();
                            index = 0;
                            continue;
                        }
                        index++;
                    }
                    if (index > 0)
                    {
                        command.CommandText = sqlBuilder.ToString();
                        if (isFirst) await this.DbContext.OpenAsync(cancellationToken);
                        result += await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    sqlBuilder.Clear();
                    sqlBuilder = null;
                    break;
                case ActionMode.BulkCopy:
                    (updateObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                    Type entityType = null;
                    foreach (var updateObj in updateObjs)
                    {
                        entityType = updateObj.GetType();
                        break;
                    }
                    if (entityType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
                    var fromMapper = this.Visitor.Tables[0].Mapper;
                    var memberMappers = this.Visitor.GetRefMemberMappers(entityType, fromMapper);
                    var ormProvider = this.Visitor.OrmProvider;
                    var tableName = ormProvider.GetTableName($"{fromMapper.TableName}_{Guid.NewGuid():N}");
                    //添加临时表
                    var builder = new StringBuilder();
                    builder.AppendLine($"CREATE TEMPORARY TABLE {tableName}(");
                    var pkColumns = new List<string>();
                    foreach (var memberMapper in memberMappers)
                    {
                        var refMemberMapper = memberMapper.RefMemberMapper;
                        var fieldName = ormProvider.GetFieldName(refMemberMapper.FieldName);
                        builder.Append($"{fieldName} {refMemberMapper.DbColumnType}");
                        if (refMemberMapper.IsKey)
                        {
                            builder.Append(" NOT NULL");
                            pkColumns.Add(fieldName);
                        }
                        builder.AppendLine(",");
                    }
                    builder.AppendLine($"PRIMARY KEY({string.Join(',', pkColumns)})");
                    builder.AppendLine(");");
                    command = this.DbContext.CreateDbCommand();
                    command.CommandText = builder.ToString();
                    await this.DbContext.OpenAsync(cancellationToken);
                    await command.ExecuteNonQueryAsync(cancellationToken);
                    await command.DisposeAsync();

                    var dataTable = this.Visitor.ToDataTable(entityType, updateObjs, fromMapper, tableName);
                    if (dataTable.Rows.Count == 0) return 0;

                    var connection = this.DbContext.Connection as MySqlConnection;
                    var transaction = this.DbContext.Transaction as MySqlTransaction;
                    var bulkCopy = new MySqlBulkCopy(connection, transaction);
                    if (timeoutSeconds.HasValue) bulkCopy.BulkCopyTimeout = timeoutSeconds.Value;
                    bulkCopy.DestinationTableName = dataTable.TableName;
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, dataTable.Columns[i].ColumnName));
                    }
                    var bulkCopyResult = await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
                    result = bulkCopyResult.RowsInserted;

                    builder.Clear();
                    builder.Append($"UPDATE {ormProvider.GetTableName(fromMapper.TableName)} a INNER JOIN {tableName} b ON ");
                    for (int i = 0; i < pkColumns.Count; i++)
                    {
                        if (i > 0) builder.Append(" AND ");
                        builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                    }
                    builder.Append(" SET ");
                    int setIndex = 0;
                    for (int i = 0; i < memberMappers.Count; i++)
                    {
                        var fieldName = ormProvider.GetFieldName(memberMappers[i].RefMemberMapper.FieldName);
                        if (pkColumns.Contains(fieldName)) continue;
                        if (setIndex > 0) builder.Append(',');
                        builder.Append($"a.{fieldName}=b.{fieldName}");
                        setIndex++;
                    }

                    command = this.DbContext.CreateDbCommand();
                    command.CommandText = builder.ToString();
                    result = await command.ExecuteNonQueryAsync(cancellationToken);
                    await command.DisposeAsync();

                    command = this.DbContext.CreateDbCommand();
                    command.CommandText = $"DROP TABLE {tableName}";
                    await command.ExecuteNonQueryAsync(cancellationToken);
                    await command.DisposeAsync();
                    command = null;
                    break;
                default:
                    if (!hasWhere)
                        throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");

                    command = this.DbContext.CreateDbCommand();
                    command.CommandText = this.Visitor.BuildCommand(command);
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
                await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion
}
