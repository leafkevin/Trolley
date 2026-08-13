using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class DialectProvider
{
    protected internal string dbKey => this.DbContext.DbKey;
    protected internal string connectionString => this.DbContext.ConnectionString;
    protected internal TheaDatabase database => this.DbContext.Database;
    protected internal ITheaConnection connection => this.DbContext.Connection;
    protected internal ITheaTransaction transaction => this.DbContext.Transaction;
    protected IOrmProvider ormProvider => this.DbContext.OrmProvider;
    protected IEntityMapProvider entityMapProvider => this.DbContext.EntityMapProvider;
    protected internal ITableShardingProvider tableShardingProvider => this.database.TableShardingProvider;
    protected internal IDbInterceptor interceptor => this.DbContext.Interceptor;
    protected internal OrmDbFactoryOptions options => this.DbContext.Options;

    #region Properties
    public DbContext DbContext { get; set; }
    #endregion

    #region UseMasterCommand/UseSlaveCommand
    public (bool, ITheaConnection, ITheaCommand) UseMasterCommand(ICommandContext commandContext = null)
    {
        bool isNeedClose = false;
        ITheaConnection connection;
        ITheaCommand command;
        if (this.transaction != null)
            connection = this.connection;
        else
        {
            isNeedClose = true;
            var connString = this.connectionString ?? this.database.Select();
            connection = this.CreateConnection(connString);
        }
        if (commandContext == null)
        {
            this.interceptor?.CommandCreating(connection);
            command = this.ormProvider.CreateCommand();
            this.interceptor?.CommandCreated(command);
        }
        else command = commandContext.Command;
        command.Connection = connection;
        command.CommandType = CommandType.Text;
        command.CommandTimeout = this.options.CommandTimeout;
        command.Transaction = this.transaction;
        command.Interceptor = this.interceptor;
        return (isNeedClose, connection, command);
    }
    public (bool, ITheaConnection, ITheaCommand) UseSlaveCommand(ICommandContext commandContext = null)
    {
        bool isNeedClose = false;
        ITheaConnection connection;
        ITheaCommand command;
        if (this.transaction != null)
            connection = this.connection;
        else
        {
            isNeedClose = true;
            var connString = this.connectionString ?? this.database.SelectSlave();
            connection = this.CreateConnection(connString);
        }
        if (commandContext == null)
        {
            this.interceptor?.CommandCreating(connection);
            command = this.ormProvider.CreateCommand();
            this.interceptor?.CommandCreated(command);
        }
        else command = commandContext.Command;
        command.Connection = connection;
        command.CommandType = CommandType.Text;
        command.CommandTimeout = this.options.CommandTimeout;
        command.Transaction = this.transaction;
        command.Interceptor = this.interceptor;
        return (isNeedClose, connection, command);
    }
    private ITheaConnection CreateConnection(string connectionString)
    {
        this.interceptor?.ConnectionCreating();
        var connection = this.ormProvider.CreateConnection(this.dbKey, connectionString);
        connection = this.interceptor?.ConnectionCreated(connection);
        connection.Interceptor = this.interceptor;
        return connection;
    }
    #endregion

    #region CreateQueryCommand
    public (bool, ITheaConnection, ITheaCommand) CreateQueryCommand(string rawSql, CommandType commandType)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        command.CommandText = rawSql;
        command.CommandType = commandType;
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);
        return (isNeedClose, connection, command);
    }
    public (bool, ITheaConnection, ITheaCommand) CreateQueryCommand(string rawSql, object parameters, CommandType commandType)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));
        var whereObjType = parameters.GetType();
        if (parameters is List<IDbDataParameter> dbParameters)
            return CreateQueryCommand(rawSql, dbParameters, commandType);
        if (!whereObjType.IsEntityType(out _))
            throw new NotSupportedException("不支持的参数类型，此方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");

        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        var commandInitializer = RepositoryHelper.BuildRawSqlCommandInitializer(this.ormProvider, rawSql, parameters);
        commandInitializer.Invoke(command.Parameters, this.ormProvider, parameters);
        command.CommandText = rawSql;
        command.CommandType = commandType;
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);
        return (isNeedClose, connection, command);
    }
    public (bool, ITheaConnection, ITheaCommand) CreateQueryCommand(string rawSql, List<IDbDataParameter> parameters, CommandType commandType)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null || parameters.Count == 0)
            throw new ArgumentNullException(nameof(parameters));
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        command.CommandText = rawSql;
        command.CommandType = commandType;
        parameters.ForEach(f => command.Parameters.Add(f));
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);
        return (isNeedClose, connection, command);
    }
    public (bool, ITheaConnection, ITheaCommand) CreateQueryByCommand(Type entityType, object whereObjs, bool isUseKey, bool isBulk)
    {
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        if (whereObjs == null)
        {
            var entityMapper = this.entityMapProvider.GetEntityMap(entityType);
            command.CommandText = $"SELECT * FROM {this.ormProvider.GetTableName(entityMapper.TableName)}";
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, whereObjs, 1, isUseKey, false, isBulk);
            command.CommandText = commandInitializer.Invoke(command.Parameters, this.DbContext, whereObjs);
        }
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);
        return (isNeedClose, connection, command);
    }
    public (bool, ITheaConnection, ITheaCommand) CreateExistsCommand(Type entityType, object whereObjs, bool isUseKey, bool isBulk)
    {
        if (whereObjs == null)
            throw new ArgumentNullException(nameof(whereObjs));
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        var commandInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, whereObjs, 2, isUseKey, false, isBulk);
        command.CommandText = commandInitializer.Invoke(command.Parameters, this.DbContext, whereObjs);
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);
        return (isNeedClose, connection, command);
    }
    #endregion

    #region QueryScalar
    public TResult QueryScalar<TResult>(bool isNeedClose, ITheaConnection connection, ITheaCommand command)
    {
        connection.Open();
        TResult result = default;
        var objResult = command.ExecuteScalar();
        if (objResult != null && objResult is not DBNull)
            result = (TResult)Convert.ChangeType(objResult, typeof(TResult));

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TResult> QueryScalarAsync<TResult>(bool isNeedClose, ITheaConnection connection, ITheaCommand command, CancellationToken cancellationToken = default)
    {
        await connection.OpenAsync(cancellationToken);
        TResult result = default;
        var objResult = await command.ExecuteScalarAsync(cancellationToken);
        if (objResult != null && objResult is not DBNull)
            result = (TResult)Convert.ChangeType(objResult, typeof(TResult));

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region QuerySingle
    public TTarget QuerySingle<TTarget>(bool isNeedClose, ITheaConnection connection, ITheaCommand command)
    {
        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(behavior);
        var deserializer = reader.GetReaderDeserializer(typeof(TTarget), this.DbContext);
        TTarget result = default;
        if (reader.Read())
            result = (TTarget)deserializer.Invoke(reader);
        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TTarget> QuerySingleAsync<TTarget>(bool isNeedClose, ITheaConnection connection, ITheaCommand command, CancellationToken cancellationToken = default)
    {
        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        var deserializer = reader.GetReaderDeserializer(typeof(TTarget), this.DbContext);
        TTarget result = default;
        if (await reader.ReadAsync(cancellationToken))
            result = (TTarget)deserializer.Invoke(reader);
        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public TTarget QuerySingle<TTarget>(IQueryVisitor visitor)
    {
        var entityType = typeof(TTarget);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor);
        Expression<Func<TTarget, TTarget>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        (var sql, var readerFields) = this.BuildSql(visitor);
        command.CommandText = sql;

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        TTarget result = default;
        var deserializer = reader.GetReaderDeserializer(entityType, this.DbContext, readerFields);
        if (reader.Read()) result = (TTarget)deserializer.Invoke(reader, readerFields);
        if (visitor.BuildIncludeSql(entityType, result, false, out sql))
        {
            reader.Dispose();
            command.CommandText = sql;
            command.Parameters.Clear();
            visitor.NextDbParameters.CopyTo(command.Parameters);
            reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
            visitor.SetIncludeValues(entityType, result, reader, false);
        }

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        visitor.Dispose();
        return result;
    }
    public async Task<TTarget> QuerySingleAsync<TTarget>(IQueryVisitor visitor, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TTarget);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor);
        Expression<Func<TTarget, TTarget>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        (var sql, var readerFields) = this.BuildSql(visitor);
        command.CommandText = sql;

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        TTarget result = default;
        var deserializer = reader.GetReaderDeserializer(entityType, this.DbContext, readerFields);
        if (await reader.ReadAsync(cancellationToken))
            result = (TTarget)deserializer.Invoke(reader, readerFields);
        if (visitor.BuildIncludeSql(entityType, result, false, out sql))
        {
            await reader.DisposeAsync();
            command.CommandText = sql;
            command.Parameters.Clear();
            visitor.NextDbParameters.CopyTo(command.Parameters);
            reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
            visitor.SetIncludeValues(entityType, result, reader, false);
        }
        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        visitor.Dispose();
        return result;
    }
    #endregion

    #region Query
    public List<TTarget> Query<TTarget>(bool isNeedClose, ITheaConnection connection, ITheaCommand command)
    {
        connection.Open();
        using var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
        var deserializer = reader.GetReaderDeserializer(typeof(TTarget), this.DbContext);
        var result = new List<TTarget>();
        while (reader.Read())
            result.Add((TTarget)deserializer.Invoke(reader));
        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<List<TTarget>> QueryAsync<TTarget>(bool isNeedClose, ITheaConnection connection, ITheaCommand command, CancellationToken cancellationToken = default)
    {
        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
        var deserializer = reader.GetReaderDeserializer(typeof(TTarget), this.DbContext);
        var result = new List<TTarget>();
        while (await reader.ReadAsync(cancellationToken))
            result.Add((TTarget)deserializer.Invoke(reader));
        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public List<TTarget> Query<TTarget>(IQueryVisitor visitor)
    {
        var entityType = typeof(TTarget);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor);
        Expression<Func<TTarget, TTarget>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        (var sql, var readerFields) = this.BuildSql(visitor);
        command.CommandText = sql;

        connection.Open();
        var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
        var deserializer = reader.GetReaderDeserializer(entityType, this.DbContext, readerFields);
        var result = new List<TTarget>();
        while (reader.Read())
            result.Add((TTarget)deserializer.Invoke(reader, readerFields));

        if (visitor.BuildIncludeSql(entityType, result, false, out sql))
        {
            reader.Dispose();
            command.CommandText = sql;
            command.Parameters.Clear();
            visitor.NextDbParameters.CopyTo(command.Parameters);
            reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
            visitor.SetIncludeValues(entityType, result, reader, false);
        }

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        visitor.Dispose();
        return result;
    }
    public async Task<List<TTarget>> QueryAsync<TTarget>(IQueryVisitor visitor, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TTarget);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor);
        Expression<Func<TTarget, TTarget>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        (var sql, var readerFields) = this.BuildSql(visitor);
        command.CommandText = sql;

        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
        var deserializer = reader.GetReaderDeserializer(entityType, this.DbContext, readerFields);
        var result = new List<TTarget>();
        while (await reader.ReadAsync(cancellationToken))
            result.Add((TTarget)deserializer.Invoke(reader, readerFields));

        if (visitor.BuildIncludeSql(entityType, result, false, out sql))
        {
            await reader.DisposeAsync();
            command.CommandText = sql;
            command.Parameters.Clear();
            visitor.NextDbParameters.CopyTo(command.Parameters);
            reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
            visitor.SetIncludeValues(entityType, result, reader, false);
        }

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        visitor.Dispose();
        return result;
    }
    #endregion

    #region QueryPage
    public IPagedList<TTarget> QueryPage<TTarget>(IQueryVisitor visitor)
    {
        var result = new PagedList<TTarget> { Data = new List<TTarget>() };
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor);
        Expression<Func<TTarget, TTarget>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        visitor.IsNeedPaging = true;
        (var sql, var readerFields) = this.BuildSql(visitor);
        command.CommandText = sql;
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);

        connection.Open();
        var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
        if (reader.Read()) result.TotalCount = reader.ToValue<int>(this.DbContext);
        result.PageNumber = visitor.PageNumber;
        result.PageSize = visitor.PageSize;

        reader.NextResult();
        var entityType = typeof(TTarget);
        var deserializer = reader.GetReaderDeserializer(typeof(TTarget), this.DbContext, readerFields);
        while (reader.Read())
            result.Data.Add((TTarget)deserializer.Invoke(reader, readerFields));
        result.Count = result.Data.Count;
        if (visitor.BuildIncludeSql(entityType, result.Data, false, out sql))
        {
            reader.Dispose();
            command.CommandText = sql;
            command.Parameters.Clear();
            visitor.NextDbParameters.CopyTo(command.Parameters);
            reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
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
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);

        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
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
            reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
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
    public bool Exists(bool isNeedClose, ITheaConnection connection, ITheaCommand command)
    {
        connection.Open();
        var objResult = command.ExecuteScalar();
        var result = objResult != null && objResult is not DBNull;
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<bool> ExistsAsync(bool isNeedClose, ITheaConnection connection, ITheaCommand command, CancellationToken cancellationToken = default)
    {
        await connection.OpenAsync(cancellationToken);
        var objResult = await command.ExecuteScalarAsync(cancellationToken);
        var result = objResult != null && objResult is not DBNull;
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region Build Query Sql
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

    #region CreateInsertCommand
    public (bool, ITheaConnection, ITheaCommand) CreateInsertCommand(Type entityType, object insertObj, bool hasIdentity)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));

        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        if (insertObj is IDictionary<string, object> dict)
        {
            var entityMapper = this.entityMapProvider.GetEntityMap(entityType);
            int index = 0;
            var fieldsBuilder = new StringBuilder();
            var valuesBuilder = new StringBuilder();
            foreach (var key in dict.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper) || memberMapper.IsIgnore
                    || memberMapper.IsAutoIncrement || memberMapper.IsNavigation
                    || memberMapper.IsIgnoreInsert || memberMapper.IsRowVersion)
                    continue;

                var fieldValue = dict[key];
                var parameterName = $"{this.ormProvider.ParameterPrefix}{memberMapper.MemberName}";
                if (index > 0)
                {
                    fieldsBuilder.Append(',');
                    valuesBuilder.Append(',');
                }
                fieldsBuilder.Append(this.ormProvider.GetFieldName(memberMapper.FieldName));
                valuesBuilder.Append(parameterName);
                if (fieldValue == null)
                    fieldValue = DBNull.Value;
                else if (memberMapper.TypeHandler != null)
                    fieldValue = memberMapper.TypeHandler.ToFieldValue(fieldValue);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    var fieldValueType = fieldValue.GetType();
                    if (fieldValueType != targetType)
                    {
                        var myValueGetter = this.ormProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.options);
                        fieldValue = myValueGetter.Invoke(fieldValue);
                    }
                }
                command.Parameters.Add(this.ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                index++;
            }
            command.CommandText = $"INSERT INTO {this.ormProvider.GetTableName(entityMapper.TableName)} ({fieldsBuilder.ToString()}) VALUES ({valuesBuilder.ToString()})";
            if (hasIdentity)
            {
                var keyFieldName = this.ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName);
                command.CommandText += this.ormProvider.GetIdentitySql(keyFieldName);
            }
        }
        else
        {
            if (insertObj is IEnumerable && insertObj is not string)
                throw new NotSupportedException("此方法只支持单条数据插入");

            var parameterType = insertObj.GetType();
            var commandInitializer = RepositoryHelper.BuildTypedCommandInitializer(this.DbContext, entityType, parameterType, 1, true, hasIdentity, null, null)
                as Func<IDataParameterCollection, DbContext, object, string>;
            command.CommandText = commandInitializer.Invoke(command.Parameters, this.DbContext, insertObj);
        }
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);
        return (isNeedClose, connection, command);
    }
    public (bool, ITheaConnection, ITheaCommand, string, Action<IDataParameterCollection, StringBuilder, DbContext, object, string>)
        CreateInsertBulkCommand(Type entityType, IEnumerable insertObjs, int bulkCount)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));
        if (bulkCount <= 0)
            throw new ArgumentOutOfRangeException("bulkCount必须大于0");

        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        object firstInsertObj = null;
        foreach (var insertObj in insertObjs)
        {
            firstInsertObj = insertObj;
            break;
        }
        var insertObjType = firstInsertObj.GetType();

        string headSql = null;
        Action<IDataParameterCollection, StringBuilder, DbContext, object, string> commandInitializer = null;
        var entityMapper = this.entityMapProvider.GetEntityMap(entityType);
        if (firstInsertObj is IDictionary<string, object> dict)
        {
            int index = 0;
            var builder = new StringBuilder();
            var valueSetters = new List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>();
            builder.Append($"INSERT INTO {this.ormProvider.GetTableName(entityMapper.TableName)} (");
            foreach (var key in dict.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper) || memberMapper.IsIgnore
                    || memberMapper.IsAutoIncrement || memberMapper.IsNavigation
                    || memberMapper.IsIgnoreInsert || memberMapper.IsRowVersion)
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(this.ormProvider.GetFieldName(memberMapper.FieldName));
                Func<IDictionary<string, object>, object> valueGetter = null;

                if (memberMapper.TypeHandler != null)
                    valueGetter = insertObj => memberMapper.TypeHandler.ToFieldValue(insertObj[key]);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    var fieldValue = dict[key];
                    if (memberMapper.IsRequired)
                    {
                        if (fieldValue == null)
                            throw new Exception($"实体{entityMapper.EntityType.FullName}表，字段{memberMapper.FieldName}为必填，值不能为空");

                        var fieldValueType = fieldValue.GetType();
                        if (fieldValueType.ToUnderlyingType() != targetType)
                        {
                            var myValueGetter = this.ormProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.options);
                            valueGetter = insertObj => myValueGetter.Invoke(insertObj[key]);
                        }
                        else valueGetter = insertObj => insertObj[key];
                    }
                    else
                    {
                        if (fieldValue != null)
                        {
                            var fieldValueType = dict[key].GetType();
                            if (fieldValueType.ToUnderlyingType() != targetType)
                            {
                                var myValueGetter = this.ormProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.options);
                                valueGetter = insertObj =>
                                {
                                    var fieldValue = insertObj[key];
                                    return fieldValue == null ? memberMapper.DefaultValue : myValueGetter.Invoke(fieldValue);
                                };
                            }
                            else valueGetter = insertObj => insertObj[key] ?? memberMapper.DefaultValue;
                        }
                        else valueGetter = insertObj => insertObj[key] ?? memberMapper.DefaultValue;
                    }
                }

                Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string> valueSetter = null;
                if (index > 0)
                {
                    valueSetter = (dbParameters, builder, insertObj, suffix) =>
                    {
                        var fieldValue = valueGetter.Invoke(insertObj);
                        var parameterName = $"{this.ormProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                        builder.Append(',');
                        builder.Append(parameterName);
                        dbParameters.Add(this.ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                    };
                }
                else
                {
                    valueSetter = (dbParameters, builder, insertObj, suffix) =>
                    {
                        var fieldValue = valueGetter.Invoke(insertObj);
                        var parameterName = $"{this.ormProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                        builder.Append(parameterName);
                        dbParameters.Add(this.ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                    };
                }
                valueSetters.Add(valueSetter);
                index++;
            }
            builder.Append(") VALUES ");
            headSql = builder.ToString();
            builder.Clear();
            commandInitializer = (dbParameters, builder, dbContext, insertObj, suffix) =>
            {
                var dictObj = insertObj as IDictionary<string, object>;
                builder.Append('(');
                foreach (var valueSetter in valueSetters)
                    valueSetter.Invoke(dbParameters, builder, dictObj, suffix);
                builder.Append("),");
            };
        }
        else
        {
            (var fieldsSql, var typedCommandInitializer) = ((string, Action<IDataParameterCollection, StringBuilder, DbContext, string, string, object, string>))
                RepositoryHelper.BuildTypedBulkCommandInitializer(this.DbContext, entityType, insertObjType, 1, null, null);
            headSql = $"INSERT INTO {this.ormProvider.GetTableName(entityMapper.TableName)} ({fieldsSql}) VALUES ";
            commandInitializer = (dbParameters, builder, dbContext, insertObj, suffix) =>
                typedCommandInitializer.Invoke(dbParameters, builder, dbContext, "(", "),", insertObj, suffix);
        }
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);
        return (isNeedClose, connection, command, headSql, commandInitializer);
    }
    public int CreateBulk<TEntity>(bool isNeedClose, ITheaConnection connection, ITheaCommand command, IEnumerable insertObjs, int bulkCount,
        string headSql, Action<IDataParameterCollection, StringBuilder, DbContext, object, string> commandInitializer)
    {
        connection.Open();
        int index = 0, result = 0;
        var builder = new StringBuilder(headSql);
        foreach (var insertObj in insertObjs)
        {
            commandInitializer.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
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
                builder.Append(headSql);
                index = 0;
            }
        }
        if (index > 0)
        {
            builder.Remove(builder.Length - 1, 1);
            command.CommandText = builder.ToString();
            if (this.interceptor != null)
                command = this.interceptor.CommandInitialized(command);

            result += command.ExecuteNonQuery();
            builder.Clear();
            command.Parameters.Clear();
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<int> CreateBulkAsync<TEntity>(bool isNeedClose, ITheaConnection connection, ITheaCommand command, IEnumerable insertObjs, int bulkCount,
        string headSql, Action<IDataParameterCollection, StringBuilder, DbContext, object, string> commandInitializer, CancellationToken cancellationToken = default)
    {
        await connection.OpenAsync(cancellationToken);
        int index = 0, result = 0;
        var builder = new StringBuilder(headSql);
        foreach (var insertObj in insertObjs)
        {
            commandInitializer.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
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
                builder.Append(headSql);
                index = 0;
            }
        }
        if (index > 0)
        {
            builder.Remove(builder.Length - 1, 1);
            command.CommandText = builder.ToString();
            if (this.interceptor != null)
                command = this.interceptor.CommandInitialized(command);

            result += await command.ExecuteNonQueryAsync(cancellationToken);
            builder.Clear();
            command.Parameters.Clear();
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region CreateUpdateCommand
    public (bool, ITheaConnection, ITheaCommand) CreateUpdateCommand(Type entityType, object updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        if (updateObj is IDictionary<string, object> dict)
        {
            var entityMapper = this.entityMapProvider.GetEntityMap(entityType);
            int index = 0;
            var fieldsBuilder = new StringBuilder();
            var whereBuilder = new StringBuilder();
            foreach (var key in dict.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper)
                    || memberMapper.IsIgnore || memberMapper.IsNavigation
                    || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
                    continue;

                var fieldValue = dict[key];
                var parameterName = $"{this.ormProvider.ParameterPrefix}{memberMapper.MemberName}";
                if (fieldsBuilder.Length > 0) fieldsBuilder.Append(',');
                if (whereBuilder.Length > 0) whereBuilder.Append(" AND ");
                var sql = $"{this.ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}";
                if (memberMapper.IsKey) whereBuilder.Append(sql);
                else fieldsBuilder.Append(sql);

                if (fieldValue == null)
                    fieldValue = DBNull.Value;
                else if (memberMapper.TypeHandler != null)
                    fieldValue = memberMapper.TypeHandler.ToFieldValue(fieldValue);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    var fieldValueType = fieldValue.GetType();
                    if (fieldValueType != targetType)
                    {
                        var myValueGetter = this.ormProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.options);
                        fieldValue = myValueGetter.Invoke(fieldValue);
                    }
                }
                command.Parameters.Add(this.ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                index++;
            }
            command.CommandText = $"UPDATE {this.ormProvider.GetTableName(entityMapper.TableName)} SET {fieldsBuilder.ToString()} WHERE ({whereBuilder.ToString()})";
        }
        else
        {
            if (updateObj is IEnumerable && updateObj is not string)
                throw new NotSupportedException("此方法只支持单条数据更新");

            var parameterType = updateObj.GetType();
            var commandInitializer = RepositoryHelper.BuildTypedCommandInitializer(this.DbContext, entityType, parameterType, 2, true, false, null, null)
                as Func<IDataParameterCollection, DbContext, object, string>;
            command.CommandText = commandInitializer.Invoke(command.Parameters, this.DbContext, updateObj);
        }
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);
        return (isNeedClose, connection, command);
    }
    public (bool, ITheaConnection, ITheaCommand, Action<IDataParameterCollection, StringBuilder, DbContext, object, string>)
        CreateUpdateBulkCommand(Type entityType, IEnumerable updateObjs, int bulkCount)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));
        if (bulkCount <= 0)
            throw new ArgumentOutOfRangeException("bulkCount必须大于0");

        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        object firstUpdateObj = null;
        foreach (var updateObj in updateObjs)
        {
            if (updateObj == null) throw new ArgumentNullException(nameof(updateObj));
            firstUpdateObj = updateObj;
            break;
        }
        var updateObjType = firstUpdateObj.GetType();

        Action<IDataParameterCollection, StringBuilder, DbContext, object, string> commandInitializer = null;
        if (firstUpdateObj is IDictionary<string, object> dict)
        {
            var entityMapper = this.entityMapProvider.GetEntityMap(entityType);
            var valueSetters = new List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>();
            var whereSetters = new List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>();
            foreach (var key in dict.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper)
                    || memberMapper.IsIgnore || memberMapper.IsNavigation
                    || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
                    continue;

                Func<IDictionary<string, object>, object> valueGetter = null;
                if (memberMapper.TypeHandler != null)
                    valueGetter = updateObj => memberMapper.TypeHandler.ToFieldValue(updateObj[key]);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    var fieldValue = dict[key];
                    if (memberMapper.IsRequired)
                    {
                        if (fieldValue == null)
                            throw new Exception($"实体{entityMapper.EntityType.FullName}表，字段{memberMapper.FieldName}为必填，值不能为空");

                        var fieldValueType = fieldValue.GetType();
                        if (fieldValueType.ToUnderlyingType() != targetType)
                        {
                            var myValueGetter = this.ormProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.options);
                            valueGetter = updateObj => myValueGetter.Invoke(updateObj[key]);
                        }
                        else valueGetter = updateObj => updateObj[key];
                    }
                    else
                    {
                        if (fieldValue != null)
                        {
                            var fieldValueType = dict[key].GetType();
                            if (fieldValueType.ToUnderlyingType() != targetType)
                            {
                                var myValueGetter = this.ormProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.options);
                                valueGetter = updateObj =>
                                {
                                    var fieldValue = updateObj[key];
                                    return fieldValue == null ? memberMapper.DefaultValue : myValueGetter.Invoke(fieldValue);
                                };
                            }
                            else valueGetter = updateObj => updateObj[key] ?? memberMapper.DefaultValue;
                        }
                        else valueGetter = updateObj => updateObj[key] ?? memberMapper.DefaultValue;
                    }
                }

                Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string> valueSetter = null;
                if (memberMapper.IsKey)
                {
                    if (whereSetters.Count > 0)
                    {
                        valueSetter = (dbParameters, builder, insertObj, suffix) =>
                        {
                            var fieldValue = valueGetter.Invoke(insertObj);
                            builder.Append(" AND ");
                            var parameterName = $"{this.ormProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                            builder.Append($"{this.ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                            dbParameters.Add(this.ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                        };
                    }
                    else
                    {
                        valueSetter = (dbParameters, builder, insertObj, suffix) =>
                        {
                            var fieldValue = valueGetter.Invoke(insertObj);
                            var parameterName = $"{this.ormProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                            builder.Append($"{this.ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                            dbParameters.Add(this.ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                        };
                    }
                    whereSetters.Add(valueSetter);
                }
                else
                {
                    if (valueSetters.Count > 0)
                    {
                        valueSetter = (dbParameters, builder, insertObj, suffix) =>
                        {
                            var fieldValue = valueGetter.Invoke(insertObj);
                            builder.Append(',');
                            var parameterName = $"{this.ormProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                            builder.Append($"{this.ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                            dbParameters.Add(this.ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                        };
                    }
                    else
                    {
                        valueSetter = (dbParameters, builder, insertObj, suffix) =>
                        {
                            var fieldValue = valueGetter.Invoke(insertObj);
                            var parameterName = $"{this.ormProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                            builder.Append($"{this.ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                            dbParameters.Add(this.ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                        };
                    }
                    valueSetters.Add(valueSetter);
                }
            }
            commandInitializer = (dbParameters, builder, dbContext, insertObj, suffix) =>
            {
                var dictObj = insertObj as IDictionary<string, object>;
                builder.Append($"UPDATE {this.ormProvider.GetTableName(entityMapper.TableName)} SET ");
                foreach (var valueSetter in valueSetters)
                    valueSetter.Invoke(dbParameters, builder, dictObj, suffix);
                builder.Append(" WHERE ");
                foreach (var valueSetter in whereSetters)
                    valueSetter.Invoke(dbParameters, builder, dictObj, suffix);
            };
        }
        else commandInitializer = RepositoryHelper.BuildTypedBulkCommandInitializer(this.DbContext, entityType, updateObjType, 2, null, null)
            as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
        return (isNeedClose, connection, command, commandInitializer);
    }
    public int UpdateBulk<TEntity>(bool isNeedClose, ITheaConnection connection, ITheaCommand command, IEnumerable updateObjs,
        int bulkCount, Action<IDataParameterCollection, StringBuilder, DbContext, object, string> commandInitializer)
    {
        int index = 0, result = 0;
        var builder = new StringBuilder();

        connection.Open();
        foreach (var updateObj in updateObjs)
        {
            if (index > 0) builder.Append(';');
            commandInitializer.Invoke(command.Parameters, builder, this.DbContext, updateObj, index.ToString());
            index++;

            if (index >= bulkCount)
            {
                command.CommandText = builder.ToString();
                if (this.interceptor != null)
                    command = this.interceptor.CommandInitialized(command);

                result += command.ExecuteNonQuery();
                builder.Clear();
                command.Parameters.Clear();
                index = 0;
            }
        }
        if (index > 0)
        {
            command.CommandText = builder.ToString();
            if (this.interceptor != null)
                command = this.interceptor.CommandInitialized(command);

            result += command.ExecuteNonQuery();
            builder.Clear();
            command.Parameters.Clear();
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<int> UpdateBulkAsync<TEntity>(bool isNeedClose, ITheaConnection connection, ITheaCommand command, IEnumerable updateObjs,
        int bulkCount, Action<IDataParameterCollection, StringBuilder, DbContext, object, string> commandInitializer, CancellationToken cancellationToken = default)
    {
        int index = 0, result = 0;
        var builder = new StringBuilder();

        await connection.OpenAsync(cancellationToken);
        foreach (var updateObj in updateObjs)
        {
            if (index > 0) builder.Append(';');
            commandInitializer.Invoke(command.Parameters, builder, this.DbContext, updateObj, index.ToString());
            index++;

            if (index >= bulkCount)
            {
                command.CommandText = builder.ToString();
                if (this.interceptor != null)
                    command = this.interceptor.CommandInitialized(command);

                result += await command.ExecuteNonQueryAsync(cancellationToken);
                builder.Clear();
                command.Parameters.Clear();
                index = 0;
            }
        }
        if (index > 0)
        {
            command.CommandText = builder.ToString();
            if (this.interceptor != null)
                command = this.interceptor.CommandInitialized(command);

            result += await command.ExecuteNonQueryAsync(cancellationToken);
            builder.Clear();
            command.Parameters.Clear();
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region CreateDeleteCommand
    public (bool, ITheaConnection, ITheaCommand) CreateDeleteCommand(Type entityType, object whereObjs, bool isUseKey, bool isBulk)
    {
        if (whereObjs == null)
            throw new ArgumentNullException(nameof(whereObjs));
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        var commandInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, whereObjs, 3, isUseKey, false, isBulk);
        command.CommandText = commandInitializer.Invoke(command.Parameters, this.DbContext, whereObjs);
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);
        return (isNeedClose, connection, command);
    }
    #endregion

    #region CreateExecuteCommand
    public (bool, ITheaConnection, ITheaCommand) CreateExecuteCommand(string rawSql, CommandType commandType)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        command.CommandText = rawSql;
        command.CommandType = commandType;
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);
        return (isNeedClose, connection, command);
    }
    public (bool, ITheaConnection, ITheaCommand) CreateExecuteCommand(string rawSql, object parameters, CommandType commandType)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));
        var whereObjType = parameters.GetType();
        if (parameters is List<IDbDataParameter> dbParameters)
            return CreateExecuteCommand(rawSql, dbParameters, commandType);
        if (!whereObjType.IsEntityType(out _))
            throw new NotSupportedException("不支持的参数类型，此方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");

        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        var commandInitializer = RepositoryHelper.BuildRawSqlCommandInitializer(this.ormProvider, rawSql, parameters);
        commandInitializer.Invoke(command.Parameters, this.ormProvider, parameters);
        command.CommandText = rawSql;
        command.CommandType = commandType;
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);
        return (isNeedClose, connection, command);
    }
    public (bool, ITheaConnection, ITheaCommand) CreateExecuteCommand(string rawSql, List<IDbDataParameter> parameters, CommandType commandType)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null || parameters.Count == 0)
            throw new ArgumentNullException(nameof(parameters));
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        command.CommandText = rawSql;
        command.CommandType = commandType;
        parameters.ForEach(f => command.Parameters.Add(f));
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);
        return (isNeedClose, connection, command);
    }
    #endregion

    #region Execute
    public int Execute(bool isNeedClose, ITheaConnection connection, ITheaCommand command)
    {
        connection.Open();
        var result = command.ExecuteNonQuery();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<int> ExecuteAsync(bool isNeedClose, ITheaConnection connection, ITheaCommand command, CancellationToken cancellationToken = default)
    {
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region Others
    public void Close() => this.connection.Close();
    public async Task CloseAsync() => await this.connection.CloseAsync();
    public void BeginTransaction()
    {
        if (this.transaction != null)
            throw new Exception("上一个事务还没有完成，无法开启新事务");
        this.DbContext.Connection ??= this.CreateConnection(this.database.Select());
        this.connection.Open();
        this.DbContext.Transaction = this.connection.BeginTransaction();
    }
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (this.transaction != null)
            throw new Exception("上一个事务还没有完成，无法开启新事务");
        this.DbContext.Connection ??= this.CreateConnection(this.database.Select());
        await this.connection.OpenAsync(cancellationToken);
        this.DbContext.Transaction = await this.connection.BeginTransactionAsync(cancellationToken);
    }
    public void Commit()
    {
        if (this.transaction == null)
            throw new Exception("还没有开启事务，无法完成提交");
        this.transaction.Commit();
        this.connection.Close();
        this.DbContext.Transaction = null;
        this.DbContext.Connection = null;
    }
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (this.transaction == null)
            throw new Exception("还没有开启事务，无法完成提交");
        await this.transaction.CommitAsync(cancellationToken);
        await this.connection.CloseAsync();
        this.DbContext.Transaction = null;
        this.DbContext.Connection = null;
    }
    public void Rollback()
    {
        if (this.transaction == null)
            throw new Exception("还没有开启事务，无法完成回滚");
        this.transaction.Rollback();
        this.connection.Close();
        this.DbContext.Transaction = null;
        this.DbContext.Connection = null;
    }
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (this.transaction == null)
            throw new Exception("还没有开启事务，无法完成回滚");
        await this.transaction.RollbackAsync(cancellationToken);
        await this.connection.CloseAsync();
        this.DbContext.Transaction = null;
        this.DbContext.Connection = null;
    }
    #endregion
}