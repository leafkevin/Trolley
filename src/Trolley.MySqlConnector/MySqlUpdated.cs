using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public class MySqlUpdated : Updated
{
    #region Properties
    public MySqlUpdateVisitor DialectVisitor { get; protected set; }
    public IOrmProvider OrmProvider => this.Visitor.OrmProvider;
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
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var updateObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.DialectVisitor.BuildSetBulkCopy();

                    var tableId = $"{Guid.NewGuid():N}";
                    var pkFields = memberMappers.Where(f => f.IsKey).Select(f => this.OrmProvider.GetFieldName(f.FieldName)).ToList();
                    var builder = new StringBuilder();
                    builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName("{0}_" + tableId)}(");
                    foreach (var memberMapper in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        builder.Append($"{fieldName} {memberMapper.DbColumnType}");
                        if (memberMapper.IsKey)
                            builder.Append(" NOT NULL");
                        builder.AppendLine(",");
                    }
                    builder.AppendLine($"PRIMARY KEY({string.Join(",", pkFields)})");
                    builder.AppendLine(");");
                    var createFormatSql = builder.ToString();

                    //添加临时表
                    builder.Clear();
                    builder.Append($"UPDATE {this.OrmProvider.GetTableName("{0}")} a INNER JOIN {this.OrmProvider.GetTableName("{0}_" + tableId)} b ON ");
                    for (int i = 0; i < pkFields.Count; i++)
                    {
                        if (i > 0) builder.Append(" AND ");
                        builder.Append($"a.{pkFields[i]}=b.{pkFields[i]}");
                    }
                    builder.Append(" SET ");
                    int setIndex = 0;
                    foreach (var memberMapper in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        if (pkFields.Contains(fieldName)) continue;
                        if (setIndex > 0) builder.Append(',');
                        builder.Append($"a.{fieldName}=b.{fieldName}");
                        setIndex++;
                    }
                    builder.Append($";DROP TABLE {this.OrmProvider.GetTableName("{0}_" + tableId)};");
                    var updateFormatSql = builder.ToString();
                    builder.Clear();

                    string createSql = null, updateSql = null;
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            builder.Append(string.Format(createFormatSql, tableName));
                        }
                        builder.Clear();
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            builder.Append(string.Format(updateFormatSql, tableName));
                        }
                        updateSql = builder.ToString();
                    }
                    else
                    {
                        createSql = string.Format(createFormatSql, shardingTables as string);
                        updateSql = string.Format(updateFormatSql, shardingTables as string);
                    }

                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var mySqlConnection = connection.BaseConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.BaseTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    //创建临时表
                    connection.Open();
                    command.CommandText = createSql;
                    command.ExecuteNonQuery(CommandSqlType.BulkCopyUpdate);

                    //插入数据到临时表
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var data = this.Visitor.ToDataTable(tableName, tabledUpdateObjs[tableName], memberMappers, valueGetters);
                            result += dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var data = this.Visitor.ToDataTable(tableName, updateObjs, memberMappers, valueGetters);
                        result = dialectOrmProvider.ExecuteBulkCopy(tableName, bulkCopyObj, connection, this.DbContext, data);
                    }
                    //执行更新
                    command.CommandText = updateSql;
                    command.ExecuteNonQuery(CommandSqlType.BulkCopyUpdate);
                    builder.Clear();
                }
                break;
            case ActionMode.Bulk:
                {
                    (var shardingType, var shardingTables, var updateObjs, var bulkCount,
                        var fixedSqlSetter, var loopSqlSetter, _) = this.Visitor.BuildSetBulk(command);

                    int index = 0;
                    var builder = new StringBuilder();
                    fixedSqlSetter?.Invoke(command.Parameters);

                    connection.Open();
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var tableParameters = tabledUpdateObjs[tableName];
                            foreach (var updateObj in tableParameters)
                            {
                                loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                            }
                            index++;

                            if (index >= bulkCount)
                            {
                                command.CommandText = builder.ToString();
                                result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                                command.Parameters.Clear();
                                fixedSqlSetter?.Invoke(command.Parameters);
                                builder.Clear();
                                index = 0;
                            }
                        }
                    }
                    else
                    {
                        foreach (var updateObj in updateObjs)
                        {
                            switch (shardingType)
                            {
                                case ShardingTableType.None:
                                case ShardingTableType.SingleTable:
                                    loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, shardingTables as string, updateObj, index.ToString());
                                    break;
                                case ShardingTableType.MultiTable:
                                case ShardingTableType.ShardingTableMap:
                                    var tableNames = shardingTables as List<string>;
                                    foreach (var tableName in tableNames)
                                    {
                                        loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                                    }
                                    break;
                            }
                            index++;
                            if (index >= bulkCount)
                            {
                                command.CommandText = builder.ToString();
                                result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                                command.Parameters.Clear();
                                fixedSqlSetter?.Invoke(command.Parameters);
                                builder.Clear();
                                index = 0;
                            }
                        }
                    }

                    if (index > 0)
                    {
                        command.CommandText = builder.ToString();
                        result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                    }
                    builder.Clear();
                }
                break;
            default:
                {
                    if (!this.Visitor.HasWhere)
                        throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

                    command.CommandText = this.Visitor.BuildSql(command, out _);
                    connection.Open();
                    result = command.ExecuteNonQuery(CommandSqlType.Update);
                }
                break;
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var updateObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.DialectVisitor.BuildSetBulkCopy();

                    var tableId = $"{Guid.NewGuid():N}";
                    var pkFields = memberMappers.Where(f => f.IsKey).Select(f => this.OrmProvider.GetFieldName(f.FieldName)).ToList();

                    var builder = new StringBuilder();
                    builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName("{0}_" + tableId)}(");
                    foreach (var memberMapper in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        builder.Append($"{fieldName} {memberMapper.DbColumnType}");
                        if (memberMapper.IsKey)
                            builder.Append(" NOT NULL");
                        builder.AppendLine(",");
                    }
                    builder.AppendLine($"PRIMARY KEY({string.Join(",", pkFields)})");
                    builder.AppendLine(");");
                    var createFormatSql = builder.ToString();

                    //添加临时表
                    builder.Clear();
                    builder.Append($"UPDATE {this.OrmProvider.GetTableName("{0}")} a INNER JOIN {this.OrmProvider.GetTableName("{0}_" + tableId)} b ON ");
                    for (int i = 0; i < pkFields.Count; i++)
                    {
                        if (i > 0) builder.Append(" AND ");
                        builder.Append($"a.{pkFields[i]}=b.{pkFields[i]}");
                    }
                    builder.Append(" SET ");
                    int setIndex = 0;
                    foreach (var memberMapper in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        if (pkFields.Contains(fieldName)) continue;
                        if (setIndex > 0) builder.Append(',');
                        builder.Append($"a.{fieldName}=b.{fieldName}");
                        setIndex++;
                    }
                    builder.Append($";DROP TABLE {this.OrmProvider.GetTableName("{0}_" + tableId)};");
                    var updateFormatSql = builder.ToString();
                    builder.Clear();

                    string createSql = null, updateSql = null;
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            builder.Append(string.Format(createFormatSql, tableName));
                        }
                        builder.Clear();
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            builder.Append(string.Format(updateFormatSql, tableName));
                        }
                        updateSql = builder.ToString();
                    }
                    else
                    {
                        createSql = string.Format(createFormatSql, shardingTables as string);
                        updateSql = string.Format(updateFormatSql, shardingTables as string);
                    }

                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var mySqlConnection = connection.BaseConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.BaseTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    //创建临时表
                    await connection.OpenAsync(cancellationToken);
                    command.CommandText = createSql;
                    await command.ExecuteNonQueryAsync(CommandSqlType.BulkCopyUpdate, cancellationToken);

                    //插入数据到临时表
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var data = this.Visitor.ToDataTable(tableName, tabledUpdateObjs[tableName], memberMappers, valueGetters);
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var data = this.Visitor.ToDataTable(tableName, updateObjs, memberMappers, valueGetters);
                        result = await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                    }
                    //执行更新
                    command.CommandText = updateSql;
                    await command.ExecuteNonQueryAsync(CommandSqlType.BulkCopyUpdate, cancellationToken);
                    builder.Clear();
                }
                break;
            case ActionMode.Bulk:
                {
                    (var shardingType, var shardingTables, var updateObjs, var bulkCount,
                        var fixedSqlSetter, var loopSqlSetter, _) = this.Visitor.BuildSetBulk(command);

                    int index = 0;
                    var builder = new StringBuilder();
                    fixedSqlSetter?.Invoke(command.Parameters);

                    connection.Open();
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var tableParameters = tabledUpdateObjs[tableName];
                            foreach (var updateObj in tableParameters)
                            {
                                loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                            }
                            index++;

                            if (index >= bulkCount)
                            {
                                command.CommandText = builder.ToString();
                                result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                                command.Parameters.Clear();
                                fixedSqlSetter?.Invoke(command.Parameters);
                                builder.Clear();
                                index = 0;
                            }
                        }
                    }
                    else
                    {
                        foreach (var updateObj in updateObjs)
                        {
                            switch (shardingType)
                            {
                                case ShardingTableType.None:
                                case ShardingTableType.SingleTable:
                                    loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, shardingTables as string, updateObj, index.ToString());
                                    break;
                                case ShardingTableType.MultiTable:
                                case ShardingTableType.ShardingTableMap:
                                    var tableNames = shardingTables as List<string>;
                                    foreach (var tableName in tableNames)
                                    {
                                        loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                                    }
                                    break;
                            }
                            index++;
                            if (index >= bulkCount)
                            {
                                command.CommandText = builder.ToString();
                                result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                                command.Parameters.Clear();
                                fixedSqlSetter?.Invoke(command.Parameters);
                                builder.Clear();
                                index = 0;
                            }
                        }
                    }
                    if (index > 0)
                    {
                        command.CommandText = builder.ToString();
                        result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                    }
                    builder.Clear();
                }
                break;
            default:
                {
                    if (!this.Visitor.HasWhere)
                        throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

                    command.CommandText = this.Visitor.BuildSql(command, out _);
                    await connection.OpenAsync(cancellationToken);
                    result = await command.ExecuteNonQueryAsync(CommandSqlType.Update, cancellationToken);
                }
                break;
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region ToSql
    public override string ToSql(out List<IDbDataParameter> dbParameters)
    {
        string sql;
        dbParameters = null;
        var builder = new StringBuilder();
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        if (this.Visitor.ActionMode == ActionMode.BulkCopy)
        {
            (var shardingType, var shardingTables, var updateObjs, var timeoutSeconds,
                var memberMappers, var valueGetters) = this.DialectVisitor.BuildSetBulkCopy();

            var tableId = $"{Guid.NewGuid():N}";
            var pkFields = memberMappers.Where(f => f.IsKey).Select(f => this.OrmProvider.GetFieldName(f.FieldName)).ToList();
            builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName("{0}_" + tableId)}(");
            foreach (var memberMapper in memberMappers)
            {
                var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                builder.Append($"{fieldName} {memberMapper.DbColumnType}");
                if (memberMapper.IsKey)
                    builder.Append(" NOT NULL");
                builder.AppendLine(",");
            }
            builder.AppendLine($"PRIMARY KEY({string.Join(",", pkFields)})");
            builder.AppendLine(");");
            var createFormatSql = builder.ToString();

            //添加临时表
            builder.Clear();
            builder.Append($"UPDATE {this.OrmProvider.GetTableName("{0}")} a INNER JOIN {this.OrmProvider.GetTableName("{0}_" + tableId)} b ON ");
            for (int i = 0; i < pkFields.Count; i++)
            {
                if (i > 0) builder.Append(" AND ");
                builder.Append($"a.{pkFields[i]}=b.{pkFields[i]}");
            }
            builder.Append(" SET ");
            int setIndex = 0;
            foreach (var memberMapper in memberMappers)
            {
                var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                if (pkFields.Contains(fieldName)) continue;
                if (setIndex > 0) builder.Append(',');
                builder.Append($"a.{fieldName}=b.{fieldName}");
                setIndex++;
            }
            builder.Append($";DROP TABLE {this.OrmProvider.GetTableName("{0}_" + tableId)};");
            var updateFormatSql = builder.ToString();
            builder.Clear();

            if (shardingType == ShardingTableType.SplitTables)
            {
                var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                foreach (var tableName in tabledUpdateObjs.Keys)
                {
                    builder.Append(string.Format(createFormatSql, tableName));
                }
                foreach (var tableName in tabledUpdateObjs.Keys)
                {
                    builder.Append(string.Format(updateFormatSql, tableName));
                }
            }
            else
            {
                builder.Append(string.Format(createFormatSql, shardingTables as string));
                builder.Append(string.Format(updateFormatSql, shardingTables as string));
            }
            sql = builder.ToString();
        }
        else
        {
            sql = this.Visitor.BuildSql(command, out _);
            dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
            command.Dispose();
            if (isNeedClose) connection.Close();
        }
        builder.Clear();
        return sql;
    }
    #endregion
}