using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public class MySqlUpdated<TEntity> : Updated<TEntity>, IMySqlUpdated<TEntity>
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
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        try
        {
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.BulkCopy:
                    {
                        (var updateObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                        Type updateObjType = null;
                        foreach (var updateObj in updateObjs)
                        {
                            updateObjType = updateObj.GetType();
                            break;
                        }
                        if (updateObjType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
                        var fromMapper = this.Visitor.Tables[0].Mapper;
                        var memberMappers = this.Visitor.GetRefMemberMappers(updateObjType, fromMapper, true);
                        var tableName = this.OrmProvider.GetTableName($"{fromMapper.TableName}_{Guid.NewGuid():N}");

                        //添加临时表
                        var builder = new StringBuilder();
                        builder.AppendLine($"CREATE TEMPORARY TABLE {tableName}(");
                        var pkColumns = new List<string>();
                        foreach (var memberMapper in memberMappers)
                        {
                            var refMemberMapper = memberMapper.RefMemberMapper;
                            var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
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
                        if (this.Visitor.IsNeedFetchShardingTables)
                            builder.Append(this.Visitor.BuildTableShardingsSql());
                        var bulkCopySql = builder.ToString();

                        builder.Clear();
                        Action<string, string> sqlExecutor = (target, source) =>
                        {
                            builder.Append($"UPDATE {this.DbContext.OrmProvider.GetTableName(target)} a INNER JOIN {source} b ON ");
                            for (int i = 0; i < pkColumns.Count; i++)
                            {
                                if (i > 0) builder.Append(" AND ");
                                builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                            }
                            builder.Append(" SET ");
                            int setIndex = 0;
                            for (int i = 0; i < memberMappers.Count; i++)
                            {
                                var fieldName = this.Visitor.OrmProvider.GetFieldName(memberMappers[i].RefMemberMapper.FieldName);
                                if (pkColumns.Contains(fieldName)) continue;
                                if (setIndex > 0) builder.Append(',');
                                builder.Append($"a.{fieldName}=b.{fieldName}");
                                setIndex++;
                            }
                        };
                        if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                        {
                            var tableNames = this.Visitor.ShardingTables[0].TableNames;
                            for (int i = 0; i < tableNames.Count; i++)
                            {
                                if (i > 0) builder.Append(';');
                                sqlExecutor.Invoke(tableNames[i], tableName);
                            }
                        }
                        else sqlExecutor.Invoke(this.Visitor.Tables[0].Body ?? fromMapper.TableName, tableName);
                        builder.Append($";DROP TABLE {tableName}");
                        var updateSql = builder.ToString();
                        var dataTable = this.Visitor.ToDataTable(updateObjType, updateObjs, memberMappers, tableName ?? fromMapper.TableName);

                        command.CommandText = bulkCopySql;
                        this.DbContext.Open(connection);
                        eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.BulkCopyUpdate, eventArgs);
                        command.ExecuteNonQuery();

                        var dbConnection = connection.BaseConnection as MySqlConnection;
                        var transaction = this.DbContext.Transaction as MySqlTransaction;
                        var bulkCopy = new MySqlBulkCopy(dbConnection, transaction);
                        if (timeoutSeconds.HasValue) bulkCopy.BulkCopyTimeout = timeoutSeconds.Value;
                        bulkCopy.DestinationTableName = dataTable.TableName;
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, dataTable.Columns[i].ColumnName));
                        }
                        var bulkCopyResult = bulkCopy.WriteToServer(dataTable);
                        result = bulkCopyResult.RowsInserted;

                        command.CommandText = updateSql;
                        eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.BulkCopyUpdate, eventArgs);
                        result = command.ExecuteNonQuery();
                    }
                    break;
                case ActionMode.Bulk:
                    {
                        var builder = new StringBuilder();
                        (var updateObjs, var bulkCount, var tableName, var firstParametersSetter,
                            var firstSqlParametersSetter, var headSqlSetter, var sqlSetter) = this.Visitor.BuildWithBulk(command);
                        Func<int, string> suffixGetter = index => this.Visitor.IsMultiple ? $"_m{this.Visitor.CommandIndex}{index}" : $"{index}";

                        Action<object, int> sqlExecuter = null;
                        if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                        {
                            sqlExecuter = (updateObj, index) =>
                            {
                                if (index > 0) builder.Append(';');
                                var tableNames = this.Visitor.ShardingTables[0].TableNames;
                                headSqlSetter.Invoke(builder, tableNames[0]);
                                firstSqlParametersSetter.Invoke(command.Parameters, builder, this.DbContext.OrmProvider, updateObj, suffixGetter.Invoke(index));

                                for (int i = 1; i < tableNames.Count; i++)
                                {
                                    builder.Append(';');
                                    headSqlSetter.Invoke(builder, tableNames[i]);
                                    sqlSetter.Invoke(builder, this.DbContext.OrmProvider, updateObj, suffixGetter.Invoke(index));
                                }
                            };
                        }
                        else
                        {
                            sqlExecuter = (updateObj, index) =>
                            {
                                if (index > 0) builder.Append(';');
                                headSqlSetter.Invoke(builder, tableName);
                                firstSqlParametersSetter.Invoke(command.Parameters, builder, this.DbContext.OrmProvider, updateObj, suffixGetter.Invoke(index));
                            };
                        }
                        if (this.Visitor.IsNeedFetchShardingTables)
                            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                        int index = 0;
                        firstParametersSetter?.Invoke(command.Parameters);
                        this.DbContext.Open(connection);
                        foreach (var updateObj in updateObjs)
                        {
                            sqlExecuter.Invoke(updateObj, index);
                            if (index >= bulkCount)
                            {
                                command.CommandText = builder.ToString();
                                eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.BulkUpdate, eventArgs);
                                result += command.ExecuteNonQuery();
                                command.Parameters.Clear();
                                firstParametersSetter?.Invoke(command.Parameters);
                                builder.Clear();
                                index = 0;
                                continue;
                            }
                            index++;
                        }
                        if (index > 0)
                        {
                            command.CommandText = builder.ToString();
                            eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.BulkUpdate, eventArgs);
                            result += command.ExecuteNonQuery();
                        }
                        builder.Clear();
                        builder = null;
                    }
                    break;
                default:
                    {
                        if (!this.Visitor.HasWhere)
                            throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");

                        if (this.Visitor.IsNeedFetchShardingTables)
                            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                        command.CommandText = this.Visitor.BuildCommand(this.DbContext, command);
                        this.DbContext.Open(connection);
                        eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.Update);
                        result = command.ExecuteNonQuery();
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            var sqlType = this.Visitor.ActionMode switch
            {
                ActionMode.BulkCopy => CommandSqlType.BulkCopyUpdate,
                ActionMode.Bulk => CommandSqlType.BulkUpdate,
                _ => CommandSqlType.Update
            };
            this.DbContext.AddCommandFailedFilter(connection, command, sqlType, eventArgs, exception);
        }
        finally
        {
            var sqlType = this.Visitor.ActionMode switch
            {
                ActionMode.BulkCopy => CommandSqlType.BulkCopyUpdate,
                ActionMode.Bulk => CommandSqlType.BulkUpdate,
                _ => CommandSqlType.Update
            };
            this.DbContext.AddCommandAfterFilter(connection, command, sqlType, eventArgs, exception == null, exception);
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close(connection);
        }
        if (exception != null) throw exception;
        return result;
    }
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterDbCommand();
        try
        {
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.BulkCopy:
                    {
                        (var updateObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                        Type updateObjType = null;
                        foreach (var updateObj in updateObjs)
                        {
                            updateObjType = updateObj.GetType();
                            break;
                        }
                        if (updateObjType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
                        var fromMapper = this.Visitor.Tables[0].Mapper;
                        var memberMappers = this.Visitor.GetRefMemberMappers(updateObjType, fromMapper, true);
                        var tableName = this.OrmProvider.GetTableName($"{fromMapper.TableName}_{Guid.NewGuid():N}");

                        //添加临时表
                        var builder = new StringBuilder();
                        builder.AppendLine($"CREATE TEMPORARY TABLE {tableName}(");
                        var pkColumns = new List<string>();
                        foreach (var memberMapper in memberMappers)
                        {
                            var refMemberMapper = memberMapper.RefMemberMapper;
                            var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
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
                        if (this.Visitor.IsNeedFetchShardingTables)
                            builder.Append(this.Visitor.BuildTableShardingsSql());
                        var bulkCopySql = builder.ToString();

                        builder.Clear();
                        Action<string, string> sqlExecutor = (target, source) =>
                        {
                            builder.Append($"UPDATE {this.DbContext.OrmProvider.GetTableName(target)} a INNER JOIN {source} b ON ");
                            for (int i = 0; i < pkColumns.Count; i++)
                            {
                                if (i > 0) builder.Append(" AND ");
                                builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                            }
                            builder.Append(" SET ");
                            int setIndex = 0;
                            for (int i = 0; i < memberMappers.Count; i++)
                            {
                                var fieldName = this.Visitor.OrmProvider.GetFieldName(memberMappers[i].RefMemberMapper.FieldName);
                                if (pkColumns.Contains(fieldName)) continue;
                                if (setIndex > 0) builder.Append(',');
                                builder.Append($"a.{fieldName}=b.{fieldName}");
                                setIndex++;
                            }
                        };
                        if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                        {
                            var tableNames = this.Visitor.ShardingTables[0].TableNames;
                            for (int i = 0; i < tableNames.Count; i++)
                            {
                                if (i > 0) builder.Append(';');
                                sqlExecutor.Invoke(tableNames[i], tableName);
                            }
                        }
                        else sqlExecutor.Invoke(this.Visitor.Tables[0].Body ?? fromMapper.TableName, tableName);
                        builder.Append($";DROP TABLE {tableName}");
                        var updateSql = builder.ToString();
                        var dataTable = this.Visitor.ToDataTable(updateObjType, updateObjs, memberMappers, tableName ?? fromMapper.TableName);

                        command.CommandText = bulkCopySql;
                        await this.DbContext.OpenAsync(connection, cancellationToken);
                        eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.BulkCopyUpdate, eventArgs);
                        await command.ExecuteNonQueryAsync(cancellationToken);

                        var dbConnection = connection.BaseConnection as MySqlConnection;
                        var transaction = this.DbContext.Transaction as MySqlTransaction;
                        var bulkCopy = new MySqlBulkCopy(dbConnection, transaction);
                        if (timeoutSeconds.HasValue) bulkCopy.BulkCopyTimeout = timeoutSeconds.Value;
                        bulkCopy.DestinationTableName = dataTable.TableName;
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, dataTable.Columns[i].ColumnName));
                        }
                        var bulkCopyResult = await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
                        result = bulkCopyResult.RowsInserted;

                        command.CommandText = updateSql;
                        eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.BulkCopyUpdate, eventArgs);
                        result = await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    break;
                case ActionMode.Bulk:
                    {
                        var builder = new StringBuilder();
                        (var updateObjs, var bulkCount, var tableName, var firstParametersSetter,
                            var firstSqlParametersSetter, var headSqlSetter, var sqlSetter) = this.Visitor.BuildWithBulk(command);
                        Func<int, string> suffixGetter = index => this.Visitor.IsMultiple ? $"_m{this.Visitor.CommandIndex}{index}" : $"{index}";

                        Action<object, int> sqlExecuter = null;
                        if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                        {
                            sqlExecuter = (updateObj, index) =>
                            {
                                if (index > 0) builder.Append(';');
                                var tableNames = this.Visitor.ShardingTables[0].TableNames;
                                headSqlSetter.Invoke(builder, tableNames[0]);
                                firstSqlParametersSetter.Invoke(command.Parameters, builder, this.DbContext.OrmProvider, updateObj, suffixGetter.Invoke(index));

                                for (int i = 1; i < tableNames.Count; i++)
                                {
                                    builder.Append(';');
                                    headSqlSetter.Invoke(builder, tableNames[i]);
                                    sqlSetter.Invoke(builder, this.DbContext.OrmProvider, updateObj, suffixGetter.Invoke(index));
                                }
                            };
                        }
                        else
                        {
                            sqlExecuter = (updateObj, index) =>
                            {
                                if (index > 0) builder.Append(';');
                                headSqlSetter.Invoke(builder, tableName);
                                firstSqlParametersSetter.Invoke(command.Parameters, builder, this.DbContext.OrmProvider, updateObj, suffixGetter.Invoke(index));
                            };
                        }
                        if (this.Visitor.IsNeedFetchShardingTables)
                            await this.DbContext.FetchShardingTablesAsync(this.Visitor as SqlVisitor, cancellationToken);
                        int index = 0;
                        firstParametersSetter?.Invoke(command.Parameters);
                        await this.DbContext.OpenAsync(connection, cancellationToken);
                        foreach (var updateObj in updateObjs)
                        {
                            sqlExecuter.Invoke(updateObj, index);
                            if (index >= bulkCount)
                            {
                                command.CommandText = builder.ToString();
                                eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.BulkUpdate, eventArgs);
                                result += await command.ExecuteNonQueryAsync(cancellationToken);
                                command.Parameters.Clear();
                                firstParametersSetter?.Invoke(command.Parameters);
                                builder.Clear();
                                index = 0;
                                continue;
                            }
                            index++;
                        }
                        if (index > 0)
                        {
                            command.CommandText = builder.ToString();
                            eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.BulkUpdate, eventArgs);
                            result += await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                        builder.Clear();
                        builder = null;
                    }
                    break;
                default:
                    {
                        if (!this.Visitor.HasWhere)
                            throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");

                        if (this.Visitor.IsNeedFetchShardingTables)
                            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                        command.CommandText = this.Visitor.BuildCommand(this.DbContext, command);
                        await this.DbContext.OpenAsync(connection, cancellationToken);
                        eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.Update);
                        result = await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            var sqlType = this.Visitor.ActionMode switch
            {
                ActionMode.BulkCopy => CommandSqlType.BulkCopyUpdate,
                ActionMode.Bulk => CommandSqlType.BulkUpdate,
                _ => CommandSqlType.Update
            };
            this.DbContext.AddCommandFailedFilter(connection, command, sqlType, eventArgs, exception);
        }
        finally
        {
            var sqlType = this.Visitor.ActionMode switch
            {
                ActionMode.BulkCopy => CommandSqlType.BulkCopyUpdate,
                ActionMode.Bulk => CommandSqlType.BulkUpdate,
                _ => CommandSqlType.Update
            };
            this.DbContext.AddCommandAfterFilter(connection, command, sqlType, eventArgs, exception == null, exception);
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync(connection);
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region ToSql
    public override string ToSql(out List<IDbDataParameter> dbParameters)
    {
        string sql = null;
        dbParameters = null;
        var builder = new StringBuilder();
        if (this.Visitor.ActionMode == ActionMode.BulkCopy)
        {
            (var updateObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
            Type updateObjType = null;
            foreach (var updateObj in updateObjs)
            {
                updateObjType = updateObj.GetType();
                break;
            }
            if (updateObjType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
            var fromMapper = this.Visitor.Tables[0].Mapper;
            var memberMappers = this.Visitor.GetRefMemberMappers(updateObjType, fromMapper, true);
            var ormProvider = this.Visitor.OrmProvider;
            var tableName = ormProvider.GetTableName($"{fromMapper.TableName}_{Guid.NewGuid():N}");

            //添加临时表
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
            if (this.Visitor.IsNeedFetchShardingTables)
            {
                builder.Append(this.Visitor.BuildTableShardingsSql());
                builder.Append(';');
            }

            Action<string, string> sqlExecutor = (target, source) =>
            {
                builder.Append($"UPDATE {this.DbContext.OrmProvider.GetTableName(target)} a INNER JOIN {source} b ON ");
                for (int i = 0; i < pkColumns.Count; i++)
                {
                    if (i > 0) builder.Append(" AND ");
                    builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                }
                builder.Append(" SET ");
                int setIndex = 0;
                for (int i = 0; i < memberMappers.Count; i++)
                {
                    var fieldName = this.Visitor.OrmProvider.GetFieldName(memberMappers[i].RefMemberMapper.FieldName);
                    if (pkColumns.Contains(fieldName)) continue;
                    if (setIndex > 0) builder.Append(',');
                    builder.Append($"a.{fieldName}=b.{fieldName}");
                    setIndex++;
                }
            };
            if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
            {
                var tableNames = this.Visitor.ShardingTables[0].TableNames;
                for (int i = 0; i < tableNames.Count; i++)
                {
                    if (i > 0) builder.Append(';');
                    sqlExecutor.Invoke(this.Visitor.Tables[0].Body ?? tableNames[i], tableName);
                }
            }
            else sqlExecutor.Invoke(fromMapper.TableName, tableName);
            builder.Append($";DROP TABLE {tableName}");
            sql = builder.ToString();
        }
        else
        {
            if (this.Visitor.IsNeedFetchShardingTables)
            {
                this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                builder.Append(this.Visitor.BuildTableShardingsSql());
            }
            (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
            sql = this.Visitor.BuildCommand(this.DbContext, command);
            if (this.Visitor.IsNeedFetchShardingTables)
            {
                builder.Append(sql);
                sql = builder.ToString();
            }
            dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
            command.Dispose();
            if (isNeedClose) connection.Close();
        }
        builder.Clear();
        builder = null;
        return sql;
    }
    #endregion
}
