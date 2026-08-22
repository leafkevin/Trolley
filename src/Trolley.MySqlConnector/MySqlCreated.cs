using MySqlConnector;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public class MySqlCreated : Created
{
    protected MySqlCreateVisitor dialectVisitor;

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
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        var entityType = this.Visitor.Tables[0].EntityType;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.dialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.ormProvider as MySqlProvider;
                    var mySqlConnection = connection.DbConnection as MySqlConnection;
                    var mySqlTransaction = this.transaction?.DbTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    connection.Open();
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            using var data = new EnumerableDataReader(tableName, memberMappers, valueGetters);
                            result += dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        using var data = new EnumerableDataReader(insertObjs, memberMappers, valueGetters);
                        result = dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
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
                                if (this.interceptor != null)
                                    command = this.interceptor.CommandInitialized(command);

                                result += command.ExecuteNonQuery();
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
                        if (this.interceptor != null)
                            command = this.interceptor.CommandInitialized(command);

                        result += command.ExecuteNonQuery();
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(out _);
                if (this.interceptor != null)
                    command = this.interceptor.CommandInitialized(command);

                connection.Open();
                result = command.ExecuteNonQuery();
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
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        var entityType = this.Visitor.Tables[0].EntityType;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.dialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.ormProvider as MySqlProvider;
                    var mySqlConnection = connection.DbConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.DbTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    await connection.OpenAsync(cancellationToken);
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            using var data = new EnumerableDataReader(tableName, memberMappers, valueGetters);
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        using var data = new EnumerableDataReader(insertObjs, memberMappers, valueGetters);
                        result = await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
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
                                if (this.interceptor != null)
                                    command = this.interceptor.CommandInitialized(command);

                                result += await command.ExecuteNonQueryAsync(cancellationToken);
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
                        if (this.interceptor != null)
                            command = this.interceptor.CommandInitialized(command);

                        result += await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(out _);
                if (this.interceptor != null)
                    command = this.interceptor.CommandInitialized(command);

                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(cancellationToken);
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
    protected MySqlCreateVisitor dialectVisitor;

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
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        var entityType = this.Visitor.Tables[0].EntityType;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.dialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.ormProvider as MySqlProvider;
                    var mySqlConnection = connection.DbConnection as MySqlConnection;
                    var mySqlTransaction = this.transaction?.DbTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    connection.Open();
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            using var data = new EnumerableDataReader(tableName, memberMappers, valueGetters);
                            result += dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        using var data = new EnumerableDataReader(insertObjs, memberMappers, valueGetters);
                        result = dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
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
                                if (this.interceptor != null)
                                    command = this.interceptor.CommandInitialized(command);

                                result += command.ExecuteNonQuery();
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
                        if (this.interceptor != null)
                            command = this.interceptor.CommandInitialized(command);

                        result += command.ExecuteNonQuery();
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(out _);
                if (this.interceptor != null)
                    command = this.interceptor.CommandInitialized(command);

                connection.Open();
                result = command.ExecuteNonQuery();
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
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        var entityType = this.Visitor.Tables[0].EntityType;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.dialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.ormProvider as MySqlProvider;
                    var mySqlConnection = connection.DbConnection as MySqlConnection;
                    var mySqlTransaction = this.transaction?.DbTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    await connection.OpenAsync(cancellationToken);
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            using var data = new EnumerableDataReader(tableName, memberMappers, valueGetters);
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        using var data = new EnumerableDataReader(insertObjs, memberMappers, valueGetters);
                        result = await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
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
                                if (this.interceptor != null)
                                    command = this.interceptor.CommandInitialized(command);

                                result += await command.ExecuteNonQueryAsync(cancellationToken);
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
                        if (this.interceptor != null)
                            command = this.interceptor.CommandInitialized(command);

                        result += await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(out _);
                if (this.interceptor != null)
                    command = this.interceptor.CommandInitialized(command);

                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(cancellationToken);
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

public class MySqlContinuedCreate : ContinuedCreate
{
    protected MySqlCreateVisitor dialectVisitor;

    #region Constructor
    public MySqlContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.dialectVisitor = this.Visitor as MySqlCreateVisitor;
    }
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        var entityType = this.Visitor.Tables[0].EntityType;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.dialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.ormProvider as MySqlProvider;
                    var mySqlConnection = connection.DbConnection as MySqlConnection;
                    var mySqlTransaction = this.transaction?.DbTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    connection.Open();
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            using var data = new EnumerableDataReader(tableName, memberMappers, valueGetters);
                            result += dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        using var data = new EnumerableDataReader(insertObjs, memberMappers, valueGetters);
                        result = dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
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
                                if (this.interceptor != null)
                                    command = this.interceptor.CommandInitialized(command);

                                result += command.ExecuteNonQuery();
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
                        result += command.ExecuteNonQuery();
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(out _);
                if (this.interceptor != null)
                    command = this.interceptor.CommandInitialized(command);

                connection.Open();
                result = command.ExecuteNonQuery();
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
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        var entityType = this.Visitor.Tables[0].EntityType;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                         var memberMappers, var valueGetters) = this.dialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.ormProvider as MySqlProvider;
                    var mySqlConnection = connection.DbConnection as MySqlConnection;
                    var mySqlTransaction = this.transaction?.DbTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    await connection.OpenAsync(cancellationToken);
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            using var data = new EnumerableDataReader(tableName, memberMappers, valueGetters);
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        using var data = new EnumerableDataReader(insertObjs, memberMappers, valueGetters);
                        result = await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
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
                                if (this.interceptor != null)
                                    command = this.interceptor.CommandInitialized(command);

                                result += await command.ExecuteNonQueryAsync(cancellationToken);
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
                        if (this.interceptor != null)
                            command = this.interceptor.CommandInitialized(command);

                        result += await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(out _);
                if (this.interceptor != null)
                    command = this.interceptor.CommandInitialized(command);

                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(cancellationToken);
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
public class MySqlBulkContinuedCreate : BulkContinuedCreate
{
    protected MySqlCreateVisitor dialectVisitor;

    #region Constructor
    public MySqlBulkContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.dialectVisitor = this.Visitor as MySqlCreateVisitor;
    }
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        var entityType = this.Visitor.Tables[0].EntityType;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.dialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.ormProvider as MySqlProvider;
                    var mySqlConnection = connection.DbConnection as MySqlConnection;
                    var mySqlTransaction = this.transaction?.DbTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    for (int i = 0; i < memberMappers.Count; i++)
                    {
                        bulkCopyObj.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, memberMappers[i].FieldName));
                    }

                    connection.Open();
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            using var data = new EnumerableDataReader(tableName, memberMappers, valueGetters);
                            result += dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        using var data = new EnumerableDataReader(insertObjs, memberMappers, valueGetters);
                        result = dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
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
                                if (this.interceptor != null)
                                    command = this.interceptor.CommandInitialized(command);

                                result += command.ExecuteNonQuery();
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
                        if (this.interceptor != null)
                            command = this.interceptor.CommandInitialized(command);

                        result += command.ExecuteNonQuery();
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(out _);
                if (this.interceptor != null)
                    command = this.interceptor.CommandInitialized(command);

                connection.Open();
                result = command.ExecuteNonQuery();
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
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        var entityType = this.Visitor.Tables[0].EntityType;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                         var memberMappers, var valueGetters) = this.dialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.ormProvider as MySqlProvider;
                    var mySqlConnection = connection.DbConnection as MySqlConnection;
                    var mySqlTransaction = this.transaction?.DbTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    for (int i = 0; i < memberMappers.Count; i++)
                    {
                        bulkCopyObj.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, memberMappers[i].FieldName));
                    }

                    await connection.OpenAsync(cancellationToken);
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            using var data = new EnumerableDataReader(tableName, memberMappers, valueGetters);
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        using var data = new EnumerableDataReader(insertObjs, memberMappers, valueGetters);
                        result = await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
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
                                if (this.interceptor != null)
                                    command = this.interceptor.CommandInitialized(command);

                                result += await command.ExecuteNonQueryAsync(cancellationToken);
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
                        if (this.interceptor != null)
                            command = this.interceptor.CommandInitialized(command);

                        result += await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(out _);
                if (this.interceptor != null)
                    command = this.interceptor.CommandInitialized(command);

                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(cancellationToken);
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
public class MySqlContinuedCreate<TEntity> : ContinuedCreate<TEntity>
{
    protected MySqlCreateVisitor dialectVisitor;

    #region Constructor
    public MySqlContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.dialectVisitor = this.Visitor as MySqlCreateVisitor;
    }
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        var entityType = this.Visitor.Tables[0].EntityType;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.dialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.ormProvider as MySqlProvider;
                    var mySqlConnection = connection.DbConnection as MySqlConnection;
                    var mySqlTransaction = this.transaction?.DbTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    for (int i = 0; i < memberMappers.Count; i++)
                    {
                        bulkCopyObj.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, memberMappers[i].FieldName));
                    }

                    connection.Open();
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            using var data = new EnumerableDataReader(tableName, memberMappers, valueGetters);
                            result += dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        using var data = new EnumerableDataReader(insertObjs, memberMappers, valueGetters);
                        result = dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
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
                                if (this.interceptor != null)
                                    command = this.interceptor.CommandInitialized(command);

                                result += command.ExecuteNonQuery();
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
                        if (this.interceptor != null)
                            command = this.interceptor.CommandInitialized(command);
                        result += command.ExecuteNonQuery();
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(out _);
                if (this.interceptor != null)
                    command = this.interceptor.CommandInitialized(command);

                connection.Open();
                result = command.ExecuteNonQuery();
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
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        var entityType = this.Visitor.Tables[0].EntityType;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                         var memberMappers, var valueGetters) = this.dialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.ormProvider as MySqlProvider;
                    var mySqlConnection = connection.DbConnection as MySqlConnection;
                    var mySqlTransaction = this.transaction?.DbTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    for (int i = 0; i < memberMappers.Count; i++)
                    {
                        bulkCopyObj.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, memberMappers[i].FieldName));
                    }

                    await connection.OpenAsync(cancellationToken);
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            using var data = new EnumerableDataReader(tableName, memberMappers, valueGetters);
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        using var data = new EnumerableDataReader(insertObjs, memberMappers, valueGetters);
                        result = await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
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
                                if (this.interceptor != null)
                                    command = this.interceptor.CommandInitialized(command);

                                result += await command.ExecuteNonQueryAsync(cancellationToken);
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
                        if (this.interceptor != null)
                            command = this.interceptor.CommandInitialized(command);

                        result += await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(out _);
                if (this.interceptor != null)
                    command = this.interceptor.CommandInitialized(command);

                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(cancellationToken);
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
public class MySqlBulkContinuedCreate<TEntity> : BulkContinuedCreate<TEntity>
{
    protected MySqlCreateVisitor dialectVisitor;

    #region Constructor
    public MySqlBulkContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.dialectVisitor = this.Visitor as MySqlCreateVisitor;
    }
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        var entityType = this.Visitor.Tables[0].EntityType;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.dialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.ormProvider as MySqlProvider;
                    var mySqlConnection = connection.DbConnection as MySqlConnection;
                    var mySqlTransaction = this.transaction?.DbTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    for (int i = 0; i < memberMappers.Count; i++)
                    {
                        bulkCopyObj.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, memberMappers[i].FieldName));
                    }

                    connection.Open();
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            using var data = new EnumerableDataReader(tableName, memberMappers, valueGetters);
                            result += dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        using var data = new EnumerableDataReader(insertObjs, memberMappers, valueGetters);
                        result = dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
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
                                if (this.interceptor != null)
                                    command = this.interceptor.CommandInitialized(command);

                                result += command.ExecuteNonQuery();
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
                        if (this.interceptor != null)
                            command = this.interceptor.CommandInitialized(command);

                        result += command.ExecuteNonQuery();
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(out _);
                if (this.interceptor != null)
                    command = this.interceptor.CommandInitialized(command);

                connection.Open();
                result = command.ExecuteNonQuery();
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
        (var isNeedClose, var connection, var command) = this.Visitor.UseCommand();
        var entityType = this.Visitor.Tables[0].EntityType;
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                         var memberMappers, var valueGetters) = this.dialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.ormProvider as MySqlProvider;
                    var mySqlConnection = connection.DbConnection as MySqlConnection;
                    var mySqlTransaction = this.transaction?.DbTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    for (int i = 0; i < memberMappers.Count; i++)
                    {
                        bulkCopyObj.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, memberMappers[i].FieldName));
                    }

                    await connection.OpenAsync(cancellationToken);
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledInsertObjs.Keys)
                        {
                            using var data = new EnumerableDataReader(tableName, memberMappers, valueGetters);
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        using var data = new EnumerableDataReader(insertObjs, memberMappers, valueGetters);
                        result = await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
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
                                if (this.interceptor != null)
                                    command = this.interceptor.CommandInitialized(command);

                                result += await command.ExecuteNonQueryAsync(cancellationToken);
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
                        if (this.interceptor != null)
                            command = this.interceptor.CommandInitialized(command);

                        result += await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    builder.Clear();
                    break;
                }
            default:
                command.CommandText = this.Visitor.BuildSql(out _);
                if (this.interceptor != null)
                    command = this.interceptor.CommandInitialized(command);

                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(cancellationToken);
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