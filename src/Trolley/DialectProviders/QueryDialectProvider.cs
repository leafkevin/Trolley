using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class QueryDialectProvider : DialectProvider
{
    #region QueryScalar
    public TValue QueryScalar<TValue>(string rawSql, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, commandType);
        return this.QueryScalarInternal<TValue>(isNeedClose, connection, command);
    }
    public async Task<TValue> QueryScalarAsync<TValue>(string rawSql, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, commandType);
        return await this.QueryScalarInternalAsync<TValue>(isNeedClose, connection, command, cancellationToken);
    }
    public TValue QueryScalar<TValue>(string rawSql, object parameters, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, parameters, commandType);
        return this.QueryScalarInternal<TValue>(isNeedClose, connection, command);
    }
    public async Task<TValue> QueryScalarAsync<TValue>(string rawSql, object parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, parameters, commandType);
        return await this.QueryScalarInternalAsync<TValue>(isNeedClose, connection, command, cancellationToken);
    }
    public TValue QueryScalar<TValue>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlParametersCommand(rawSql, parameters, commandType);
        return this.QueryScalarInternal<TValue>(isNeedClose, connection, command);
    }
    public async Task<TValue> QueryScalarAsync<TValue>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlParametersCommand(rawSql, parameters, commandType);
        return await this.QueryScalarInternalAsync<TValue>(isNeedClose, connection, command, cancellationToken);
    }
    public TResult QueryScalar<TResult>(IQueryVisitor visitor)
    {
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor);
        command.CommandText = this.BuildScalarSql(visitor);
        var result = this.QueryScalarInternal<TResult>(isNeedClose, connection, command);
        visitor.Dispose();
        return result;
    }
    public async Task<TResult> QueryScalarAsync<TResult>(IQueryVisitor visitor, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor);
        command.CommandText = this.BuildScalarSql(visitor);
        var result = await this.QueryScalarInternalAsync<TResult>(isNeedClose, connection, command, cancellationToken);
        visitor.Dispose();
        return result;
    }
    private TResult QueryScalarInternal<TResult>(bool isNeedClose, ITheaConnection connection, ITheaCommand command)
    {
        connection.Open();
        TResult result = default;
        var objResult = command.ExecuteScalar(CommandSqlType.Select);
        if (objResult != null && objResult is not DBNull)
            result = (TResult)Convert.ChangeType(objResult, typeof(TResult));

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    private async Task<TResult> QueryScalarInternalAsync<TResult>(bool isNeedClose, ITheaConnection connection, ITheaCommand command, CancellationToken cancellationToken = default)
    {
        await connection.OpenAsync(cancellationToken);
        TResult result = default;
        var objResult = await command.ExecuteScalarAsync(CommandSqlType.Select, cancellationToken);
        if (objResult != null && objResult is not DBNull)
            result = (TResult)Convert.ChangeType(objResult, typeof(TResult));

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public bool QueryExists(IQueryVisitor visitor)
    {
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor);
        var sql = visitor.BuildSql(true, out var readerFields);
        command.CommandText = sql;

        connection.Open();
        var objResult = command.ExecuteScalar(CommandSqlType.Select);
        var result = objResult != null && objResult is not DBNull;

        command.Dispose();
        if (isNeedClose) connection.Close();
        visitor.Dispose();
        return result;
    }
    public async Task<bool> QueryExistsAsync(IQueryVisitor visitor, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor);
        var sql = visitor.BuildSql(true, out var readerFields);
        command.CommandText = sql;

        await connection.OpenAsync(cancellationToken);
        var objResult = await command.ExecuteScalarAsync(CommandSqlType.Select, cancellationToken);
        var result = objResult != null && objResult is not DBNull;

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        visitor.Dispose();
        return result;
    }
    #endregion

    #region QueryRaw
    public TResult QueryRaw<TTarget, TResult>(string rawSql, bool isBulk, Func<ITheaDataReader, TResult> readerInitializer, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, commandType);
        return this.QueryRawInternal<TTarget, TResult>(isBulk, isNeedClose, connection, command, readerInitializer);
    }
    public async Task<TResult> QueryRawAsync<TTarget, TResult>(string rawSql, bool isBulk, Func<ITheaDataReader, CancellationToken, Task<TResult>> readerInitializer, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, commandType);
        return await this.QueryRawInternalAsync<TTarget, TResult>(isBulk, isNeedClose, connection, command, readerInitializer, cancellationToken);
    }
    public TResult QueryRaw<TTarget, TResult>(string rawSql, bool isBulk, object parameter, Func<ITheaDataReader, TResult> readerInitializer, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, parameter, commandType);
        return this.QueryRawInternal<TTarget, TResult>(isBulk, isNeedClose, connection, command, readerInitializer);
    }
    public async Task<TResult> QueryRawAsync<TTarget, TResult>(string rawSql, bool isBulk, object parameter, Func<ITheaDataReader, CancellationToken, Task<TResult>> readerInitializer, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, parameter, commandType);
        return await this.QueryRawInternalAsync<TTarget, TResult>(isBulk, isNeedClose, connection, command, readerInitializer, cancellationToken);
    }
    public TResult QueryRaw<TTarget, TResult>(string rawSql, bool isBulk, List<IDbDataParameter> parameters, Func<ITheaDataReader, TResult> readerInitializer, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlParametersCommand(rawSql, parameters, commandType);
        return this.QueryRawInternal<TTarget, TResult>(isBulk, isNeedClose, connection, command, readerInitializer);
    }
    public async Task<TResult> QueryRawAsync<TTarget, TResult>(string rawSql, bool isBulk, List<IDbDataParameter> parameters, Func<ITheaDataReader, CancellationToken, Task<TResult>> readerInitializer, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlParametersCommand(rawSql, parameters, commandType);
        return await this.QueryRawInternalAsync<TTarget, TResult>(isBulk, isNeedClose, connection, command, readerInitializer, cancellationToken);
    }
    private (bool, ITheaConnection, ITheaCommand) CreateQueryRawSqlParametersCommand(string rawSql, List<IDbDataParameter> parameters, CommandType commandType)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null || parameters.Count == 0)
            throw new ArgumentNullException(nameof(parameters));

        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        command.CommandText = rawSql;
        command.CommandType = commandType;
        parameters.ForEach(f => command.Parameters.Add(f));
        return (isNeedClose, connection, command);
    }
    public List<TTarget> QueryRaw<TTarget>(string rawSql, List<IDbDataParameter> parameters,
         Func<ITheaDataReader, List<TTarget>> readerInitializer, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, parameters, commandType);

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        var result = readerInitializer.Invoke(reader);
        reader.Dispose();

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<List<TTarget>> QueryRawAsync<TTarget>(string rawSql, List<IDbDataParameter> parameters,
        Func<ITheaDataReader, CancellationToken, Task<List<TTarget>>> readerInitializer, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, parameters, commandType);

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        var result = await readerInitializer.Invoke(reader, cancellationToken);
        await reader.DisposeAsync();

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }


    private (bool, ITheaConnection, ITheaCommand) CreateQueryRawSqlCommand(string rawSql, CommandType commandType)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        command.CommandText = rawSql;
        command.CommandType = commandType;
        return (isNeedClose, connection, command);
    }
    private (bool, ITheaConnection, ITheaCommand) CreateQueryRawSqlCommand(string rawSql, object parameters, CommandType commandType)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));
        var whereObjType = parameters.GetType();
        if (!whereObjType.IsEntityType(out _))
            throw new NotSupportedException("不支持的参数类型，此方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");

        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        var commandInitializer = RepositoryHelper.BuildQueryRawSqlCommandInitializer(this.ormProvider, rawSql, parameters);
        commandInitializer.Invoke(command.Parameters, this.ormProvider, parameters);
        command.CommandText = rawSql;
        command.CommandType = commandType;
        return (isNeedClose, connection, command);
    }
    private TResult QueryRawInternal<TTarget, TResult>(bool isBulk, bool isNeedClose, ITheaConnection connection, ITheaCommand command, Func<ITheaDataReader, TResult> readerInitializer)
    {
        connection.Open();
        var behavior = isBulk ? CommandBehavior.SequentialAccess : CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        var result = readerInitializer.Invoke(reader);
        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    private async Task<TResult> QueryRawInternalAsync<TTarget, TResult>(bool isBulk, bool isNeedClose, ITheaConnection connection, ITheaCommand command, Func<ITheaDataReader, CancellationToken, Task<TResult>> readerInitializer, CancellationToken cancellationToken = default)
    {
        await connection.OpenAsync(cancellationToken);
        var behavior = isBulk ? CommandBehavior.SequentialAccess : CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        var result = await readerInitializer.Invoke(reader, cancellationToken);
        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region Query
    public TResult Query<TEntity, TResult>(object whereObjs, bool isUseKey, bool isBulk, Func<ITheaDataReader, Func<ITheaDataReader, object>, TResult> readerInitializer)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryWhereCommand(typeof(TEntity), whereObjs, isUseKey, isBulk);

        connection.Open();
        var behavior = isBulk ? CommandBehavior.SequentialAccess : CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this.DbContext);
        var result = readerInitializer.Invoke(reader, deserializer);

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TResult> QueryAsync<TEntity, TResult>(object whereObjs, bool isUseKey, bool isBulk, Func<ITheaDataReader,
        Func<ITheaDataReader, object>, CancellationToken, Task<TResult>> readerInitializer, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryWhereCommand(typeof(TEntity), whereObjs, isUseKey, isBulk);

        await connection.OpenAsync(cancellationToken);
        var behavior = isBulk ? CommandBehavior.SequentialAccess : CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this.DbContext);
        var result = await readerInitializer.Invoke(reader, deserializer, cancellationToken);

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    private (bool, ITheaConnection, ITheaCommand) CreateQueryWhereCommand(Type entityType, object whereObjs, bool isUseKey, bool isBulk)
    {
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        var commandInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, whereObjs, 1, isUseKey, false, isBulk);
        command.CommandText = commandInitializer.Invoke(command.Parameters, this.DbContext, whereObjs);
        return (isNeedClose, connection, command);
    }
    #endregion

    #region QueryVisitor
    public TResult QueryFrom<TEntity, TResult>(IQueryVisitor visitor, bool isBulk, Func<Type, ITheaDataReader, List<ReaderField>, TResult> readerInitializer)
    {
        var entityType = typeof(TEntity);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor);
        Expression<Func<TEntity, TEntity>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        (var sql, var readerFields) = this.BuildSql(visitor);
        command.CommandText = sql;

        connection.Open();
        var behavior = isBulk ? CommandBehavior.SequentialAccess : CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        var result = readerInitializer.Invoke(entityType, reader, readerFields);
        if (visitor.BuildIncludeSql(entityType, result, isBulk, out sql))
        {
            reader.Dispose();
            command.CommandText = sql;
            command.Parameters.Clear();
            visitor.NextDbParameters.CopyTo(command.Parameters);
            reader = command.ExecuteReader(CommandSqlType.Select, CommandBehavior.SequentialAccess);
            visitor.SetIncludeValues(entityType, result, reader, isBulk);
        }

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        visitor.Dispose();
        return result;
    }
    public async Task<TResult> QueryFromAsync<TEntity, TResult>(IQueryVisitor visitor, bool isBulk, Func<Type, ITheaDataReader, List<ReaderField>, CancellationToken, Task<TResult>> readerInitializer, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor);

        Expression<Func<TEntity, TEntity>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        (var sql, var readerFields) = this.BuildSql(visitor);
        command.CommandText = sql;

        await connection.OpenAsync(cancellationToken);
        var behavior = isBulk ? CommandBehavior.SequentialAccess : CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        var result = await readerInitializer.Invoke(entityType, reader, readerFields, cancellationToken);
        if (visitor.BuildIncludeSql(entityType, result, isBulk, out sql))
        {
            await reader.DisposeAsync();
            command.CommandText = sql;
            command.Parameters.Clear();
            visitor.NextDbParameters.CopyTo(command.Parameters);
            reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
            await visitor.SetIncludeValuesAsync(entityType, result, reader, isBulk, cancellationToken);
        }

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        visitor.Dispose();
        return result;
    }
    public IPagedList<TResult> QueryPage<TResult>(IQueryVisitor visitor)
    {
        var result = new PagedList<TResult> { Data = new List<TResult>() };
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor);
        Expression<Func<TResult, TResult>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        visitor.IsNeedPaging = true;
        (var sql, var readerFields) = this.BuildSql(visitor);
        command.CommandText = sql;

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        if (reader.Read()) result.TotalCount = reader.ToValue<int>(this.DbContext);
        result.PageNumber = visitor.PageNumber;
        result.PageSize = visitor.PageSize;

        reader.NextResult();
        var entityType = typeof(TResult);
        var deserializer = reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);
        while (reader.Read())
            result.Data.Add((TResult)deserializer.Invoke(reader, readerFields));
        result.Count = result.Data.Count;
        if (visitor.BuildIncludeSql(entityType, result.Data, false, out sql))
        {
            reader.Dispose();
            command.CommandText = sql;
            command.Parameters.Clear();
            visitor.NextDbParameters.CopyTo(command.Parameters);
            reader = command.ExecuteReader(CommandSqlType.Select, behavior);
            visitor.SetIncludeValues(entityType, result.Data, reader, false);
        }

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        visitor.Dispose();
        return result;
    }
    public async Task<IPagedList<TResult>> QueryPageAsync<TResult>(IQueryVisitor visitor, CancellationToken cancellationToken = default)
    {
        var result = new PagedList<TResult> { Data = new List<TResult>() };
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor);

        Expression<Func<TResult, TResult>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        visitor.IsNeedPaging = true;
        (var sql, var readerFields) = this.BuildSql(visitor);
        command.CommandText = sql;

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
            result.TotalCount = reader.ToValue<int>(this.DbContext);
        result.PageNumber = visitor.PageNumber;
        result.PageSize = visitor.PageSize;

        var entityType = typeof(TResult);
        await reader.NextResultAsync(cancellationToken);
        var deserializer = reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);
        while (await reader.ReadAsync(cancellationToken))
            result.Data.Add((TResult)deserializer.Invoke(reader, readerFields));
        result.Count = result.Data.Count;
        if (visitor.BuildIncludeSql(entityType, result.Data, false, out sql))
        {
            await reader.DisposeAsync();
            command.CommandText = sql;
            command.Parameters.Clear();
            visitor.NextDbParameters.CopyTo(command.Parameters);
            reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
            await visitor.SetIncludeValuesAsync(entityType, result.Data, reader, false, cancellationToken);
        }

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        visitor.Dispose();
        return result;
    }
    #endregion

    #region Exists
    public bool Exists<TEntity>(object whereObj, bool isUseKey, bool isBulk)
    {
        (var isNeedClose, var connection, var command) = this.CreateExistsCommand(typeof(TEntity), whereObj, isUseKey, isBulk);

        connection.Open();
        var objResult = command.ExecuteScalar(CommandSqlType.Select);
        var result = objResult != null && objResult is not DBNull;

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<bool> ExistsAsync<TEntity>(object whereObj, bool isUseKey, bool isBulk, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateExistsCommand(typeof(TEntity), whereObj, isUseKey, isBulk);

        await connection.OpenAsync(cancellationToken);
        var objResult = await command.ExecuteScalarAsync(CommandSqlType.Select, cancellationToken);
        var result = objResult != null && objResult is not DBNull;

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    private (bool, ITheaConnection, ITheaCommand) CreateExistsCommand(Type entityType, object whereObjs, bool isUseKey, bool isBulk)
    {
        if (whereObjs == null)
            throw new ArgumentNullException(nameof(whereObjs));
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        var commandInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, whereObjs, 2, isUseKey, false, isBulk);
        command.CommandText = commandInitializer.Invoke(command.Parameters, this.DbContext, whereObjs);
        return (isNeedClose, connection, command);
    }
    #endregion

    #region BuildSql
    public (string, List<ReaderField>) BuildSql(IQueryVisitor visitor)
    {
        var sql = visitor.BuildSql(true, out var readerFields);
        if (visitor.IsManyShardingTables)
        {
            sql = this.BuildShardingTablesSqlByFormat(visitor as SqlVisitor, sql, visitor.ShardingTableJointMark);
            if (visitor.IsNeedChangeUnionShardingTables)
                sql = visitor.BuildShardingSql(sql);
        }
        return (sql, readerFields);
    }
    public string BuildScalarSql(IQueryVisitor visitor)
    {
        var sql = visitor.BuildSql(true, out _);
        if (visitor.IsManyShardingTables)
        {
            sql = this.BuildShardingTablesSqlByFormat(visitor as SqlVisitor, sql, visitor.ShardingTableJointMark);
            sql = visitor.BuildShardingScalarSql(sql);
        }
        return sql;
    }
    public string BuildShardingTablesSqlByFormat(SqlVisitor visitor, string formatSql, string jointMark)
    {
        //查询，多分表时，都使用表名替换生成分表sql
        var builder = new StringBuilder();
        if (visitor.ShardingTables.Count > 1)
        {
            var masterTableSegment = visitor.ShardingTables[0];
            var loopCount = masterTableSegment.TableNames.Count;
            var origMasterName = masterTableSegment.Mapper.TableName;
            for (int i = 0; i < loopCount; i++)
            {
                var masterTableName = masterTableSegment.TableNames[i];
                var sql = formatSql.Replace($"__SHARDING_{masterTableSegment.ShardingId}_{origMasterName}", masterTableName);
                for (int j = 1; j < visitor.ShardingTables.Count; j++)
                {
                    var tableSegment = visitor.ShardingTables[j];
                    if (tableSegment.IsIncludeManySharding) continue;

                    var origTableName = tableSegment.Mapper.TableName;
                    //如果主表分表名不存在，直接忽略本次关联
                    var tableName = tableSegment.ShardingMapGetter.Invoke(origMasterName, origTableName, masterTableName);
                    sql = sql.Replace($"__SHARDING_{tableSegment.ShardingId}_{origTableName}", tableName);
                    //1:N include表，需要统计一下表名，后续会用到
                    if (visitor.IncludeTables != null && visitor.IncludeTables.Contains(tableSegment))
                    {
                        tableSegment.TableNames ??= new();
                        if (!tableSegment.TableNames.Contains(tableName))
                            tableSegment.TableNames.Add(tableName);
                    }
                }
                if (builder.Length > 0) builder.Append(jointMark);
                builder.Append(sql);
            }
        }
        else
        {
            var tableSegment = visitor.ShardingTables[0];
            var origTableName = tableSegment.Mapper.TableName;
            if (tableSegment.TableNames != null)
            {
                for (int i = 0; i < tableSegment.TableNames.Count; i++)
                {
                    if (i > 0) builder.Append(jointMark);
                    var tableName = tableSegment.TableNames[i];
                    var sql = formatSql.Replace($"__SHARDING_{tableSegment.ShardingId}_{origTableName}", tableName);
                    builder.Append(sql);
                }
            }
            else
            {
                var sql = formatSql.Replace($"__SHARDING_{tableSegment.ShardingId}_{origTableName}", tableSegment.Body);
                builder.Append(sql);
            }
        }
        var result = builder.ToString();
        builder.Clear();
        return result;
    }
    public string GetShardingTable(Type entityType, params object[] fieldValues)
    {
        if (fieldValues == null || fieldValues.Length == 0)
            throw new ArgumentNullException(nameof(fieldValues), "参数fieldValues不能为null或是空元素");
        if (this.tableShardingProvider == null || !this.tableShardingProvider.TryGetTableSharding(entityType, out var shardingTableInfo))
            throw new InvalidOperationException($"实体表{entityType.FullName}没有配置分表，无需调用此方法");
        if (!this.entityMapProvider.TryGetEntityMap(entityType, out var entityMap))
            throw new InvalidOperationException($"实体表{entityType.FullName}没有配置映射关系，无法获取分表信息");
        return shardingTableInfo.Rule.Invoke(entityMap.TableName, fieldValues) as string;
    }
    #endregion
}