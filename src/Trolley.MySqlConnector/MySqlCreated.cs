using MySqlConnector;
using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public class MySqlCreated<TEntity> : Created<TEntity>, IMySqlCreated<TEntity>
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
        IDbCommand command = null;
        Exception exception = null;
        IEnumerable insertObjs = null;
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
                    (insertObjs, var bulkCount, var headSqlSetter, var commandInitializer) = this.Visitor.BuildWithBulk(command);
                    headSqlSetter.Invoke(sqlBuilder);

                    foreach (var insertObj in insertObjs)
                    {
                        if (index > 0) sqlBuilder.Append(',');
                        commandInitializer.Invoke(sqlBuilder, insertObj, index.ToString());
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
                            sqlBuilder.Clear();
                            headSqlSetter.Invoke(sqlBuilder);
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
                    break;
                case ActionMode.BulkCopy:
                    (insertObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                    Type entityType = null;
                    foreach (var insertObj in insertObjs)
                    {
                        entityType = insertObj.GetType();
                        break;
                    }
                    var entityMapper = this.Visitor.Tables[0].Mapper;
                    var dataTable = this.Visitor.ToDataTable(entityType, insertObjs, entityMapper);
                    if (dataTable.Rows.Count == 0) return 0;

                    this.DbContext.Open();
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
                    break;
                default:
                    //默认单条
                    command = this.DbContext.CreateCommand();
                    command.CommandText = this.Visitor.BuildCommand(command, false);
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
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.Bulk:
                    int index = 0;
                    bool isFirst = true;
                    var sqlBuilder = new StringBuilder();
                    command = this.DbContext.CreateDbCommand();
                    (insertObjs, var bulkCount, var headSqlSetter, var commandInitializer) = this.Visitor.BuildWithBulk(command);
                    headSqlSetter.Invoke(sqlBuilder);
                    foreach (var insertObj in insertObjs)
                    {
                        if (index > 0) sqlBuilder.Append(',');
                        commandInitializer.Invoke(sqlBuilder, insertObj, index.ToString());
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
                            sqlBuilder.Clear();
                            headSqlSetter.Invoke(sqlBuilder);
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
                    break;
                case ActionMode.BulkCopy:
                    (insertObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                    Type entityType = null;
                    foreach (var insertObj in insertObjs)
                    {
                        entityType = insertObj.GetType();
                        break;
                    }
                    var entityMapper = this.Visitor.Tables[0].Mapper;
                    var dataTable = this.Visitor.ToDataTable(entityType, insertObjs, entityMapper);
                    if (dataTable.Rows.Count == 0) return 0;

                    await this.DbContext.OpenAsync(cancellationToken);
                    var connection = this.DbContext.Connection as MySqlConnection;
                    var transaction = this.DbContext.Transaction as MySqlTransaction;
                    var bulkCopy = new MySqlBulkCopy(connection, transaction);
                    if (timeoutSeconds.HasValue) bulkCopy.BulkCopyTimeout = timeoutSeconds.Value;
                    bulkCopy.DestinationTableName = dataTable.TableName;
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, dataTable.Columns[i].ColumnName));
                    }
                    var bulkCopyResult = await bulkCopy.WriteToServerAsync(dataTable);
                    result = bulkCopyResult.RowsInserted;
                    break;
                default:
                    //默认单条
                    command = this.DbContext.CreateDbCommand();
                    command.CommandText = this.Visitor.BuildCommand(command, false);
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
