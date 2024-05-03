using MySqlConnector;
using System;
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
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.Bulk:
                {
                    using var command = this.DbContext.CreateCommand();
                    bool isNeedClose = this.DbContext.IsNeedClose;
                    Exception exception = null;
                    try
                    {
                        int index = 0;
                        bool isFirst = true;
                        var sqlBuilder = new StringBuilder();
                        (var insertObjs, var bulkCount, var headSqlSetter, var commandInitializer) = this.Visitor.BuildWithBulk(command);
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
                    }
                    catch (Exception ex)
                    {
                        isNeedClose = true;
                        exception = ex;
                    }
                    finally
                    {
                        command.Dispose();
                        if (isNeedClose) this.Close();
                    }
                    if (exception != null) throw exception;
                }
                break;
            case ActionMode.BulkCopy:
                {
                    (var insertObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                    var dataTable = this.Visitor.ToDataTable(insertObjs);
                    if (dataTable.Rows.Count == 0) return 0;

                    bool isNeedClose = false;
                    Exception exception = null;
                    try
                    {
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
                    }
                    catch (Exception ex)
                    {
                        isNeedClose = true;
                        exception = ex;
                    }
                    finally
                    {
                        if (isNeedClose) this.DbContext.Close();
                    }
                    if (exception != null) throw exception;
                }
                break;
            default:
                //默认单条
                result = this.DbContext.Execute(f => f.CommandText = this.Visitor.BuildCommand(f, false));
                this.Visitor.Dispose();
                this.Visitor = null;
                break;
        }
        return result;
    }
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.Bulk:
                {
                    using var command = this.DbContext.CreateDbCommand();
                    bool isNeedClose = this.DbContext.IsNeedClose;
                    Exception exception = null;
                    try
                    {
                        int index = 0;
                        bool isFirst = true;
                        var sqlBuilder = new StringBuilder();
                        (var insertObjs, var bulkCount, var headSqlSetter, var commandInitializer) = this.Visitor.BuildWithBulk(command);
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
                    }
                    catch (Exception ex)
                    {
                        isNeedClose = true;
                        exception = ex;
                    }
                    finally
                    {
                        await command.DisposeAsync();
                        if (isNeedClose) await this.CloseAsync();
                    }
                    if (exception != null) throw exception;
                }
                break;
            case ActionMode.BulkCopy:
                {
                    (var insertObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                    var dataTable = this.Visitor.ToDataTable(insertObjs);
                    if (dataTable.Rows.Count == 0) return 0;

                    Exception exception = null;
                    bool isNeedClose = this.DbContext.Transaction == null;
                    try
                    {
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
                    }
                    catch (Exception ex)
                    {
                        isNeedClose = true;
                        exception = ex;
                    }
                    finally
                    {
                        if (isNeedClose) await this.DbContext.CloseAsync();
                    }
                    if (exception != null) throw exception;
                }
                break;
            default:
                result = await this.DbContext.ExecuteAsync(f => f.CommandText = this.Visitor.BuildCommand(f, false), cancellationToken);
                this.Visitor.Dispose();
                this.Visitor = null;
                break;
        }
        return result;
    }
    #endregion
}
