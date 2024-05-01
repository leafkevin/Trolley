using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using System.Data;
using System.Collections.Generic;
using MySqlConnector;
using System;

namespace Trolley.MySqlConnector;

public class MySqlCreate<TEntity> : Create<TEntity>, IMySqlCreate<TEntity>
{
    #region Properties
    public MySqlCreateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public MySqlCreate(DbContext dbContext) : base(dbContext)
    {
        this.DialectVisitor = this.Visitor as MySqlCreateVisitor;
    }
    #endregion

    #region IgnoreInto
    public IMySqlCreate<TEntity> IgnoreInto()
    {
        this.DialectVisitor.IsUseIgnoreInto = true;
        return this;
    }
    #endregion

    #region WithBy
    public new IMySqlContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
    {
        base.WithBy(insertObj);
        return new MySqlContinuedCreate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithBulk
    public new IMySqlContinuedCreate<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount)
    {
        base.WithBulk(insertObjs, bulkCount);
        return new MySqlContinuedCreate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region ExecuteBulkCopy
    public override int ExecuteBulkCopy(IEnumerable<TEntity> insertObjs, int? bulkCopyTimeout = null)
    {
        var dataTable = this.ToDataTable(insertObjs);
        if (dataTable.Rows.Count == 0) return 0;

        int result = 0;
        bool isNeedClose = false;
        Exception exception = null;
        try
        {
            this.DbContext.Open();
            var connection = this.DbContext.Connection as MySqlConnection;
            var transaction = this.DbContext.Transaction as MySqlTransaction;
            var bulkCopy = new MySqlBulkCopy(connection, transaction);
            if (bulkCopyTimeout.HasValue) bulkCopy.BulkCopyTimeout = bulkCopyTimeout.Value;
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
        return result;
    }
    public async override Task<int> ExecuteBulkCopyAsync(IEnumerable<TEntity> insertObjs, int? bulkCopyTimeout = null, CancellationToken cancellationToken = default)
    {
        var dataTable = this.ToDataTable(insertObjs);
        if (dataTable.Rows.Count == 0) return 0;

        int result = 0;
        Exception exception = null;
        bool isNeedClose = this.DbContext.Transaction == null;
        try
        {
            await this.DbContext.OpenAsync(cancellationToken);
            var connection = this.DbContext.Connection as MySqlConnection;
            var transaction = this.DbContext.Transaction as MySqlTransaction;
            var bulkCopy = new MySqlBulkCopy(connection, transaction);
            if (bulkCopyTimeout.HasValue) bulkCopy.BulkCopyTimeout = bulkCopyTimeout.Value;
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
        return result;
    }
    #endregion
}
