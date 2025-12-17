using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.PostgreSql;

public class PostgreSqlUpdated<TEntity> : Updated<TEntity>
{
    #region Properties
    public PostgreSqlUpdateVisitor DialectVisitor { get; protected set; }
    public IOrmProvider OrmProvider => this.Visitor.OrmProvider;
    #endregion

    #region Constructor
    public PostgreSqlUpdated(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as PostgreSqlUpdateVisitor;
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
                    var updateObjs = this.DialectVisitor.BuildWithBulkCopy();
                    Type updateObjType = null;
                    foreach (var updateObj in updateObjs)
                    {
                        updateObjType = updateObj.GetType();
                        break;
                    }
                    if (updateObjType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
                    var fromMapper = this.Visitor.Tables[0].Mapper;
                    var memberMappers = this.Visitor.GetRefMemberMappers(updateObjType, fromMapper, true);
                    var tableName = $"{fromMapper.TableName}_{Guid.NewGuid():N}";

                    //添加临时表
                    var builder = new StringBuilder();
                    builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName(tableName)}(");
                    var pkColumns = new List<string>();
                    foreach ((var refMemberMapper, _) in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                        builder.Append($"{fieldName} {refMemberMapper.DbColumnType}");
                        if (refMemberMapper.IsKey)
                        {
                            builder.Append(" NOT NULL");
                            pkColumns.Add(fieldName);
                        }
                        builder.AppendLine(",");
                    }
                    builder.AppendLine($"PRIMARY KEY({string.Join(",", pkColumns)})");
                    builder.AppendLine(");");
                    if (this.Visitor.IsNeedFetchShardingTables)
                        builder.Append(this.Visitor.BuildTableShardingsSql());
                    var bulkCopySql = builder.ToString();

                    builder.Clear();
                    void Execute(string target, string source)
                    {
                        builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a SET ");
                        int setIndex = 0;
                        foreach ((var refMemberMapper, _) in memberMappers)
                        {
                            var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                            if (pkColumns.Contains(fieldName)) continue;
                            if (setIndex > 0) builder.Append(',');
                            builder.Append($"{fieldName}=b.{fieldName}");
                            setIndex++;
                        }
                        builder.Append($" FROM {this.OrmProvider.GetTableName(source)} b WHERE ");
                        for (int i = 0; i < pkColumns.Count; i++)
                        {
                            if (i > 0) builder.Append(" AND ");
                            builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                        }
                    }
                    if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                    {
                        var tableNames = this.Visitor.ShardingTables[0].TableNames;
                        for (int i = 0; i < tableNames.Count; i++)
                        {
                            if (i > 0) builder.Append(';');
                            Execute(tableNames[i], tableName);
                        }
                    }
                    else Execute(this.Visitor.Tables[0].Body ?? fromMapper.TableName, tableName);
                    builder.Append($";DROP TABLE {this.OrmProvider.GetTableName(tableName)}");
                    var updateSql = builder.ToString();

                    command.CommandText = bulkCopySql;
                    connection.Open();
                    command.ExecuteNonQuery(CommandSqlType.BulkCopyUpdate);

                    var dialectOrmProvider = this.OrmProvider as PostgreSqlProvider;
                    var sqlVisitor = this.Visitor as SqlVisitor;
                    result = dialectOrmProvider.ExecuteBulkCopy(true, this.DbContext, sqlVisitor, connection, updateObjType, updateObjs, tableName);
                    if (result == 0) updateSql = $"DROP TABLE {this.OrmProvider.GetTableName(tableName)}";
                    command.CommandText = updateSql;
                    result = command.ExecuteNonQuery(CommandSqlType.BulkCopyUpdate);
                    builder.Clear();
                }
                break;
            case ActionMode.Bulk:
                {
                    var builder = new StringBuilder();
                    (var updateObjs, var bulkCount, var tableName, var fixedParameterSetter, var firstSqlSetter, var sqlSetter, _) = this.Visitor.BuildWithBulk(command);
                    Func<int, string> suffixGetter = index => this.Visitor.IsMultiple ? $"_m{this.Visitor.CommandIndex}{index}" : $"{index}";

                    Action<object, int> sqlExecuter = null;
                    if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                    {
                        sqlExecuter = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            var tableNames = this.Visitor.ShardingTables[0].TableNames;
                            firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableNames[0], updateObj, suffixGetter.Invoke(index));
                            for (int i = 1; i < tableNames.Count; i++)
                            {
                                builder.Append(';');
                                sqlSetter.Invoke(builder, this.DbContext, tableNames[i], updateObj, suffixGetter.Invoke(index));
                            }
                        };
                    }
                    else
                    {
                        sqlExecuter = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, suffixGetter.Invoke(index));
                        };
                    }
                    if (this.Visitor.IsNeedFetchShardingTables)
                        this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                    int index = 0;
                    fixedParameterSetter?.Invoke(command.Parameters);
                    connection.Open();
                    foreach (var updateObj in updateObjs)
                    {
                        sqlExecuter.Invoke(updateObj, index);
                        index++;

                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                            command.Parameters.Clear();
                            fixedParameterSetter?.Invoke(command.Parameters);
                            builder.Clear();
                            index = 0;
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

                    if (this.Visitor.IsNeedFetchShardingTables)
                        this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
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
                    var updateObjs = this.DialectVisitor.BuildWithBulkCopy();
                    Type updateObjType = null;
                    foreach (var updateObj in updateObjs)
                    {
                        updateObjType = updateObj.GetType();
                        break;
                    }
                    if (updateObjType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
                    var fromMapper = this.Visitor.Tables[0].Mapper;
                    var memberMappers = this.Visitor.GetRefMemberMappers(updateObjType, fromMapper, true);
                    var tableName = $"{fromMapper.TableName}_{Guid.NewGuid():N}";

                    //添加临时表
                    var builder = new StringBuilder();
                    builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName(tableName)}(");
                    var pkColumns = new List<string>();
                    foreach ((var refMemberMapper, _) in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                        builder.Append($"{fieldName} {refMemberMapper.DbColumnType}");
                        if (refMemberMapper.IsKey)
                        {
                            builder.Append(" NOT NULL");
                            pkColumns.Add(fieldName);
                        }
                        builder.AppendLine(",");
                    }
                    builder.AppendLine($"PRIMARY KEY({string.Join(",", pkColumns)})");
                    builder.AppendLine(");");
                    if (this.Visitor.IsNeedFetchShardingTables)
                        builder.Append(this.Visitor.BuildTableShardingsSql());
                    var bulkCopySql = builder.ToString();

                    builder.Clear();
                    void Execute(string target, string source)
                    {
                        builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a SET ");
                        int setIndex = 0;
                        foreach ((var refMemberMapper, _) in memberMappers)
                        {
                            var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                            if (pkColumns.Contains(fieldName)) continue;
                            if (setIndex > 0) builder.Append(',');
                            builder.Append($"{fieldName}=b.{fieldName}");
                            setIndex++;
                        }
                        builder.Append($" FROM {this.OrmProvider.GetTableName(source)} b WHERE ");
                        for (int i = 0; i < pkColumns.Count; i++)
                        {
                            if (i > 0) builder.Append(" AND ");
                            builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                        }
                    }
                    if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                    {
                        var tableNames = this.Visitor.ShardingTables[0].TableNames;
                        for (int i = 0; i < tableNames.Count; i++)
                        {
                            if (i > 0) builder.Append(';');
                            Execute(tableNames[i], tableName);
                        }
                    }
                    else Execute(this.Visitor.Tables[0].Body ?? fromMapper.TableName, tableName);
                    builder.Append($";DROP TABLE {this.OrmProvider.GetTableName(tableName)}");
                    var updateSql = builder.ToString();

                    command.CommandText = bulkCopySql;
                    await connection.OpenAsync(cancellationToken);
                    await command.ExecuteNonQueryAsync(CommandSqlType.BulkCopyUpdate, cancellationToken);

                    var dialectOrmProvider = this.OrmProvider as PostgreSqlProvider;
                    var sqlVisitor = this.Visitor as SqlVisitor;
                    result = await dialectOrmProvider.ExecuteBulkCopyAsync(true, this.DbContext, sqlVisitor, connection, updateObjType, updateObjs, cancellationToken, tableName);
                    if (result == 0) updateSql = $"DROP TABLE {this.OrmProvider.GetTableName(tableName)}";
                    command.CommandText = updateSql;
                    result = await command.ExecuteNonQueryAsync(CommandSqlType.BulkCopyUpdate, cancellationToken);
                    builder.Clear();
                }
                break;
            case ActionMode.Bulk:
                {
                    var builder = new StringBuilder();
                    (var updateObjs, var bulkCount, var tableName, var fixedParameterSetter, var firstSqlSetter, var sqlSetter, _) = this.Visitor.BuildWithBulk(command);
                    Func<int, string> suffixGetter = index => this.Visitor.IsMultiple ? $"_m{this.Visitor.CommandIndex}{index}" : $"{index}";

                    Action<object, int> sqlExecuter = null;
                    if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                    {
                        sqlExecuter = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            var tableNames = this.Visitor.ShardingTables[0].TableNames;
                            firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableNames[0], updateObj, suffixGetter.Invoke(index));
                            for (int i = 1; i < tableNames.Count; i++)
                            {
                                builder.Append(';');
                                sqlSetter.Invoke(builder, this.DbContext, tableNames[i], updateObj, suffixGetter.Invoke(index));
                            }
                        };
                    }
                    else
                    {
                        sqlExecuter = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, suffixGetter.Invoke(index));
                        };
                    }

                    int index = 0;
                    if (this.Visitor.IsNeedFetchShardingTables)
                        await this.DbContext.FetchShardingTablesAsync(this.Visitor as SqlVisitor, cancellationToken);
                    fixedParameterSetter?.Invoke(command.Parameters);
                    await connection.OpenAsync(cancellationToken);

                    foreach (var updateObj in updateObjs)
                    {
                        sqlExecuter.Invoke(updateObj, index);
                        index++;

                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                            command.Parameters.Clear();
                            fixedParameterSetter?.Invoke(command.Parameters);
                            builder.Clear();
                            index = 0;
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

                    if (this.Visitor.IsNeedFetchShardingTables)
                        this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
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
        if (this.Visitor.ActionMode == ActionMode.BulkCopy)
        {
            var updateObjs = this.DialectVisitor.BuildWithBulkCopy();
            Type updateObjType = null;
            foreach (var updateObj in updateObjs)
            {
                updateObjType = updateObj.GetType();
                break;
            }
            if (updateObjType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
            var fromMapper = this.Visitor.Tables[0].Mapper;
            var memberMappers = this.Visitor.GetRefMemberMappers(updateObjType, fromMapper, true);
            var tableName = $"{fromMapper.TableName}_{Guid.NewGuid():N}";

            //添加临时表
            builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName(tableName)}(");
            var pkColumns = new List<string>();
            foreach ((var refMemberMapper, _) in memberMappers)
            {
                var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                builder.Append($"{fieldName} {refMemberMapper.DbColumnType}");
                if (refMemberMapper.IsKey)
                {
                    builder.Append(" NOT NULL");
                    pkColumns.Add(fieldName);
                }
                builder.AppendLine(",");
            }
            builder.AppendLine($"PRIMARY KEY({string.Join(",", pkColumns)})");
            builder.AppendLine(");");
            if (this.Visitor.IsNeedFetchShardingTables)
            {
                builder.Append(this.Visitor.BuildTableShardingsSql());
                builder.Append(';');
            }
            void Execute(string target, string source)
            {
                builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a SET ");
                int setIndex = 0;
                foreach ((var refMemberMapper, _) in memberMappers)
                {
                    var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                    if (pkColumns.Contains(fieldName)) continue;
                    if (setIndex > 0) builder.Append(',');
                    builder.Append($"{fieldName}=b.{fieldName}");
                    setIndex++;
                }
                builder.Append($" FROM {this.OrmProvider.GetTableName(source)} b WHERE ");
                for (int i = 0; i < pkColumns.Count; i++)
                {
                    if (i > 0) builder.Append(" AND ");
                    builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                }
            }
            if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
            {
                var tableNames = this.Visitor.ShardingTables[0].TableNames;
                for (int i = 0; i < tableNames.Count; i++)
                {
                    if (i > 0) builder.Append(';');
                    Execute(tableNames[i], tableName);
                }
            }
            else Execute(this.Visitor.Tables[0].Body ?? fromMapper.TableName, tableName);
            builder.Append($";DROP TABLE {this.OrmProvider.GetTableName(tableName)}");
            sql = builder.ToString();
        }
        else
        {
            if (this.Visitor.IsNeedFetchShardingTables)
            {
                this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                builder.Append(this.Visitor.BuildTableShardingsSql());
                builder.Append(';');
            }
            (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
            sql = this.Visitor.BuildSql(command, out _);
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
        return sql;
    }
    #endregion
}
public class PostgreSqlUpdated<TEntity, TResult> : Updated<TEntity>, IPostgreSqlUpdated<TEntity, TResult>
{
    #region Properties
    public PostgreSqlUpdateVisitor DialectVisitor { get; protected set; }
    public IOrmProvider OrmProvider => this.Visitor.OrmProvider;
    #endregion

    #region Constructor
    public PostgreSqlUpdated(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as PostgreSqlUpdateVisitor;
    }
    #endregion

    #region Execute
    public new List<TResult> Execute()
    {
        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();

        Func<ITheaDataReader, object> readerDeserializer = null;
        Action<CommandSqlType, string, List<SqlFieldSegment>> readerExecuter = (sqlType, sql, readerFields) =>
        {
            command.CommandText = sql;
            using var reader = command.ExecuteReader(sqlType, CommandBehavior.SequentialAccess);
            readerDeserializer ??= reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);
            while (reader.Read())
                result.Add((TResult)readerDeserializer.Invoke(reader));
            while (reader.NextResult())
            {
                while (reader.Read())
                    result.Add((TResult)readerDeserializer.Invoke(reader));
            }
            reader.Dispose();
            command.Parameters.Clear();
        };
        if (this.Visitor.IsNeedFetchShardingTables)
            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);

        if (this.Visitor.ActionMode == ActionMode.Bulk)
        {
            (var updateObjs, var bulkCount, var tableName, var fixedParameterSetter, var firstSqlSetter, var sqlSetter, var readerFields) = this.Visitor.BuildWithBulk(command);
            Func<int, string> suffixGetter = index => this.Visitor.IsMultiple ? $"_m{this.Visitor.CommandIndex}{index}" : $"{index}";

            var builder = new StringBuilder();
            Action<object, int> sqlExecuter = null;
            if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
            {
                sqlExecuter = (updateObj, index) =>
                {
                    if (index > 0) builder.Append(';');
                    var tableNames = this.Visitor.ShardingTables[0].TableNames;
                    firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableNames[0], updateObj, suffixGetter.Invoke(index));
                    for (int i = 1; i < tableNames.Count; i++)
                    {
                        builder.Append(';');
                        sqlSetter.Invoke(builder, this.DbContext, tableNames[i], updateObj, suffixGetter.Invoke(index));
                    }
                };
            }
            else
            {
                sqlExecuter = (updateObj, index) =>
                {
                    if (index > 0) builder.Append(';');
                    firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, suffixGetter.Invoke(index));
                };
            }

            int index = 0;
            fixedParameterSetter?.Invoke(command.Parameters);
            connection.Open();

            foreach (var updateObj in updateObjs)
            {
                sqlExecuter.Invoke(updateObj, index);
                index++;

                if (index >= bulkCount)
                {
                    readerExecuter.Invoke(CommandSqlType.BulkUpdate, builder.ToString(), readerFields);
                    fixedParameterSetter?.Invoke(command.Parameters);
                    index = 0;
                    builder.Clear();
                }
            }
            if (index > 0) readerExecuter.Invoke(CommandSqlType.BulkUpdate, builder.ToString(), readerFields);
        }
        else
        {
            if (!this.Visitor.HasWhere)
                throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

            connection.Open();
            var sql = this.Visitor.BuildSql(this.DbContext, command, out var readerFields);
            readerExecuter.Invoke(CommandSqlType.Update, sql, readerFields);
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        this.Visitor.Dispose();
        return result;
    }
    public new async Task<List<TResult>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();

        Func<ITheaDataReader, object> readerDeserializer = null;
        Func<CommandSqlType, string, List<SqlFieldSegment>, Task> readerExecuter = async (sqlType, sql, readerFields) =>
        {
            command.CommandText = sql;
            using var reader = await command.ExecuteReaderAsync(sqlType, CommandBehavior.SequentialAccess, cancellationToken);
            readerDeserializer ??= reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);
            while (await reader.ReadAsync(cancellationToken))
                result.Add((TResult)readerDeserializer.Invoke(reader));
            while (await reader.NextResultAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                    result.Add((TResult)readerDeserializer.Invoke(reader));
            }
            await reader.DisposeAsync();
            command.Parameters.Clear();
        };
        if (this.Visitor.IsNeedFetchShardingTables)
            await this.DbContext.FetchShardingTablesAsync(this.Visitor as SqlVisitor, cancellationToken);

        if (this.Visitor.ActionMode == ActionMode.Bulk)
        {
            (var updateObjs, var bulkCount, var tableName, var fixedParameterSetter, var firstSqlSetter, var sqlSetter, var readerFields) = this.Visitor.BuildWithBulk(command);
            Func<int, string> suffixGetter = index => this.Visitor.IsMultiple ? $"_m{this.Visitor.CommandIndex}{index}" : $"{index}";

            var builder = new StringBuilder();
            Action<object, int> sqlExecuter = null;
            if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
            {
                sqlExecuter = (updateObj, index) =>
                {
                    if (index > 0) builder.Append(';');
                    var tableNames = this.Visitor.ShardingTables[0].TableNames;
                    firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableNames[0], updateObj, suffixGetter.Invoke(index));
                    for (int i = 1; i < tableNames.Count; i++)
                    {
                        builder.Append(';');
                        sqlSetter.Invoke(builder, this.DbContext, tableNames[i], updateObj, suffixGetter.Invoke(index));
                    }
                };
            }
            else
            {
                sqlExecuter = (updateObj, index) =>
                {
                    if (index > 0) builder.Append(';');
                    firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, suffixGetter.Invoke(index));
                };
            }

            int index = 0;
            fixedParameterSetter?.Invoke(command.Parameters);
            await connection.OpenAsync(cancellationToken);

            foreach (var updateObj in updateObjs)
            {
                sqlExecuter.Invoke(updateObj, index);
                index++;

                if (index >= bulkCount)
                {
                    await readerExecuter.Invoke(CommandSqlType.BulkUpdate, builder.ToString(), readerFields);
                    fixedParameterSetter?.Invoke(command.Parameters);
                    index = 0;
                    builder.Clear();
                }
            }
            if (index > 0) await readerExecuter.Invoke(CommandSqlType.BulkUpdate, builder.ToString(), readerFields);
        }
        else
        {
            if (!this.Visitor.HasWhere)
                throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

            await connection.OpenAsync(cancellationToken);
            var sql = this.Visitor.BuildSql(this.DbContext, command, out var readerFields);
            await readerExecuter.Invoke(CommandSqlType.Update, sql, readerFields);
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        this.Visitor.Dispose();
        return result;
    }
    #endregion

    #region ToSql
    public override string ToSql(out List<IDbDataParameter> dbParameters)
    {
        string sql;
        dbParameters = null;
        var builder = new StringBuilder();
        if (this.Visitor.IsNeedFetchShardingTables)
        {
            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
            builder.Append(this.Visitor.BuildTableShardingsSql());
            builder.Append(';');
        }
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        sql = this.Visitor.BuildSql(command, out _);
        if (this.Visitor.IsNeedFetchShardingTables)
        {
            builder.Append(sql);
            sql = builder.ToString();
        }
        dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        builder.Clear();
        return sql;
    }
    #endregion
}