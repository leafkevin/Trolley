using MySqlConnector;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public class MySqlCreated : Created
{
    private MySqlCreateVisitor dialectVisitor;

    #region Constructor
    public MySqlCreated(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.dialectVisitor = this.Visitor as MySqlCreateVisitor;
    }
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var entityType = this.Visitor.Tables[0].EntityType;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.dialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var mySqlConnection = connection.BaseConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.BaseTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            var data = this.Visitor.ToDataTable(tableName, tabledInsertObjs[tableName], memberMappers, valueGetters);
                            result += dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var data = this.Visitor.ToDataTable(tableName, insertObjs, memberMappers, valueGetters);
                        result = dialectOrmProvider.ExecuteBulkCopy(shardingTables as string, bulkCopyObj, connection, this.DbContext, data);
                    }
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
                        builder.Remove(builder.Length - 1, 1);
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
        this.Visitor = null;
        return result;
    }
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var entityType = this.Visitor.Tables[0].EntityType;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.dialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var mySqlConnection = connection.BaseConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.BaseTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            var data = this.Visitor.ToDataTable(tableName, tabledInsertObjs[tableName], memberMappers, valueGetters);
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var data = this.Visitor.ToDataTable(tableName, insertObjs, memberMappers, valueGetters);
                        result = await dialectOrmProvider.ExecuteBulkCopyAsync(shardingTables as string, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                    }
                    break;
                }
            case ActionMode.Bulk:
                {
                    (var shardingType, var shardingTables, var insertObjs, var bulkCount, var firstSqlSetter,
                        var loopSqlSetter, _, var readerFields) = this.Visitor.BuildWithBulk(command);

                    int index = 0;
                    var builder = new StringBuilder();
                    async Task TabledExecute(string tableName, IEnumerable insertObjs, CancellationToken cancellationToken)
                    {
                        foreach (var insertObj in insertObjs)
                        {
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                            index++;
                            if (index >= bulkCount)
                            {
                                builder.Remove(builder.Length - 1, 1);
                                command.CommandText = builder.ToString();
                                result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                                builder.Clear();
                                command.Parameters.Clear();
                                firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                                index = 0;
                            }
                        }
                    }
                    await connection.OpenAsync(cancellationToken);
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                            await TabledExecute(tableName, tabledInsertObjs[tableName], cancellationToken);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        await TabledExecute(tableName, insertObjs, cancellationToken);
                    }
                    if (index > 0)
                    {
                        builder.Remove(builder.Length - 1, 1);
                        command.CommandText = builder.ToString();
                        result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(command, out _);
                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(CommandSqlType.Insert, cancellationToken);
                break;
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        this.Visitor.Dispose();
        this.Visitor = null;
        return result;
    }
    #endregion
}
public class MySqlIdentitiedCreated : IdentitiedCreated
{
    private MySqlCreateVisitor dialectVisitor;

    #region Constructor
    public MySqlIdentitiedCreated(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.dialectVisitor = this.Visitor as MySqlCreateVisitor;
    }
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var entityType = this.Visitor.Tables[0].EntityType;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.dialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var mySqlConnection = connection.BaseConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.BaseTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            var data = this.Visitor.ToDataTable(tableName, tabledInsertObjs[tableName], memberMappers, valueGetters);
                            result += dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var data = this.Visitor.ToDataTable(tableName, insertObjs, memberMappers, valueGetters);
                        result = dialectOrmProvider.ExecuteBulkCopy(shardingTables as string, bulkCopyObj, connection, this.DbContext, data);
                    }
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
                        builder.Remove(builder.Length - 1, 1);
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
        this.Visitor = null;
        return result;
    }
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var entityType = this.Visitor.Tables[0].EntityType;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                         var memberMappers, var valueGetters) = this.dialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var mySqlConnection = connection.BaseConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.BaseTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            var data = this.Visitor.ToDataTable(tableName, tabledInsertObjs[tableName], memberMappers, valueGetters);
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var data = this.Visitor.ToDataTable(tableName, insertObjs, memberMappers, valueGetters);
                        result = await dialectOrmProvider.ExecuteBulkCopyAsync(shardingTables as string, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                    }
                    break;
                }
            case ActionMode.Bulk:
                {
                    (var shardingType, var shardingTables, var insertObjs, var bulkCount, var firstSqlSetter,
                        var loopSqlSetter, _, var readerFields) = this.Visitor.BuildWithBulk(command);

                    int index = 0;
                    var builder = new StringBuilder();
                    async Task TabledExecute(string tableName, IEnumerable insertObjs, CancellationToken cancellationToken)
                    {
                        foreach (var insertObj in insertObjs)
                        {
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                            index++;
                            if (index >= bulkCount)
                            {
                                builder.Remove(builder.Length - 1, 1);
                                command.CommandText = builder.ToString();
                                result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                                builder.Clear();
                                command.Parameters.Clear();
                                firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                                index = 0;
                            }
                        }
                    }
                    await connection.OpenAsync(cancellationToken);
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                            await TabledExecute(tableName, tabledInsertObjs[tableName], cancellationToken);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        await TabledExecute(tableName, insertObjs, cancellationToken);
                    }
                    if (index > 0)
                    {
                        builder.Remove(builder.Length - 1, 1);
                        command.CommandText = builder.ToString();
                        result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(command, out _);
                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(CommandSqlType.Insert, cancellationToken);
                break;
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        this.Visitor.Dispose();
        this.Visitor = null;
        return result;
    }
    #endregion
}