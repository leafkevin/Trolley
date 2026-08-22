using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.PostgreSql;

public class PostgreSqlUpdated : Updated
{
    private PostgreSqlUpdateVisitor dialectVisitor;

    #region Properties
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public PostgreSqlUpdated(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.dialectVisitor = this.Visitor as PostgreSqlUpdateVisitor;
    }
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var updateObjs, var memberMappers,
                        var valueGetters) = this.dialectVisitor.BuildSetBulkCopy();

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

                    builder.Append($"UPDATE {this.OrmProvider.GetTableName("{0}")} a SET ");
                    int setIndex = 0;
                    foreach (var refMemberMapper in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                        if (pkFields.Contains(fieldName)) continue;
                        if (setIndex > 0) builder.Append(',');
                        builder.Append($"{fieldName}=b.{fieldName}");
                        setIndex++;
                    }
                    builder.Append($" FROM {this.OrmProvider.GetTableName("{0}_" + tableId)} b WHERE ");
                    for (int i = 0; i < pkFields.Count; i++)
                    {
                        if (i > 0) builder.Append(" AND ");
                        builder.Append($"a.{pkFields[i]}=b.{pkFields[i]}");
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

                    //创建临时表
                    connection.Open();
                    command.CommandText = createSql;
                    command.ExecuteNonQuery(CommandSqlType.BulkCopyUpdate);

                    //插入数据到临时表
                    var dialectOrmProvider = this.OrmProvider as PostgreSqlProvider;
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            result += dialectOrmProvider.ExecuteBulkCopy(tableName, this.DbContext,
                                connection, tabledUpdateObjs[tableName], memberMappers, valueGetters);
                        }
                    }
                    else
                    {
                        result = dialectOrmProvider.ExecuteBulkCopy(shardingTables as string,
                            this.DbContext, connection, updateObjs, memberMappers, valueGetters);
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

                    if (this.Visitor.IsNeedFetchShardingTables)
                        this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                    command.CommandText = this.Visitor.BuildSql(out _);
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
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var updateObjs, var memberMappers,
                        var valueGetters) = this.dialectVisitor.BuildSetBulkCopy();

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

                    builder.Append($"UPDATE {this.OrmProvider.GetTableName("{0}")} a SET ");
                    int setIndex = 0;
                    foreach (var refMemberMapper in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                        if (pkFields.Contains(fieldName)) continue;
                        if (setIndex > 0) builder.Append(',');
                        builder.Append($"{fieldName}=b.{fieldName}");
                        setIndex++;
                    }
                    builder.Append($" FROM {this.OrmProvider.GetTableName("{0}_" + tableId)} b WHERE ");
                    for (int i = 0; i < pkFields.Count; i++)
                    {
                        if (i > 0) builder.Append(" AND ");
                        builder.Append($"a.{pkFields[i]}=b.{pkFields[i]}");
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

                    //创建临时表
                    await connection.OpenAsync(cancellationToken);
                    command.CommandText = createSql;
                    await command.ExecuteNonQueryAsync(CommandSqlType.BulkCopyUpdate, cancellationToken);

                    //插入数据到临时表
                    var dialectOrmProvider = this.OrmProvider as PostgreSqlProvider;
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(tableName, this.DbContext,
                                connection, tabledUpdateObjs[tableName], memberMappers, valueGetters, cancellationToken);
                        }
                    }
                    else
                    {
                        result = await dialectOrmProvider.ExecuteBulkCopyAsync(shardingTables as string,
                            this.DbContext, connection, updateObjs, memberMappers, valueGetters, cancellationToken);
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

                    await connection.OpenAsync(cancellationToken);
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

                    if (this.Visitor.IsNeedFetchShardingTables)
                        this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                    command.CommandText = this.Visitor.BuildSql(out _);
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
            var updateObjs = this.dialectVisitor.BuildWithBulkCopy();
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
            (var isNeedClose, var connection, var command) = this.UseMasterCommand();
            sql = this.Visitor.BuildSql(out _);
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
public class PostgreSqlResultUpdated<TResult> : IBulkResultCommand<TResult>
{
    private PostgreSqlUpdateVisitor dialectVisitor;

    #region Properties
    public DbContext DbContext { get; set; }
    public IUpdateVisitor Visitor { get; set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public PostgreSqlResultUpdated(DbContext dbContext, IUpdateVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
        this.dialectVisitor = visitor as PostgreSqlUpdateVisitor;
    }
    #endregion

    #region Execute
    public List<TResult> Execute()
    {
        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();

        Func<ITheaDataReader, object> readerDeserializer = null;
        Action<CommandSqlType, string, List<SqlSegment>> readerExecuter = (sqlType, sql, readerFields) =>
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
    public async Task<List<TResult>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();

        Func<ITheaDataReader, object> readerDeserializer = null;
        Func<CommandSqlType, string, List<SqlSegment>, Task> readerExecuter = async (sqlType, sql, readerFields) =>
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
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        sql = this.Visitor.BuildSql(out _);
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