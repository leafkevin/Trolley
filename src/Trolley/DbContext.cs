using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public sealed class DbContext
{
    #region Properties
    public string DbKey { get; internal set; }
    public string ConnectionString { get; internal set; }
    public TheaDatabase Database { get; internal set; }
    public ITheaConnection Connection { get; set; }
    public ITheaTransaction Transaction { get; set; }
    public string DefaultTableSchema { get; internal set; }
    public IOrmProvider OrmProvider => this.Database.OrmProvider;
    public IEntityMapProvider EntityMapProvider => this.Database.EntityMapProvider;
    public ITableShardingProvider TableShardingProvider => this.Database.TableShardingProvider;
    public DbInterceptors DbInterceptors { get; internal set; }

    public int CommandTimeout { get; internal set; }
    public string UserParameterPrefix { get; internal set; }
    public bool IsConstantParameterized { get; internal set; }
    public Type DefaultEnumMapDbType { get; internal set; }
    public DateTimeKind DefaultDateTimeKind { get; internal set; }
    #endregion

    #region UseMasterCommand/UseSlaveCommand
    public (bool, ITheaConnection, ITheaCommand) UseMasterCommand()
    {
        bool isNeedClose = false;
        ITheaConnection connection;
        ITheaCommand command;
        if (this.Transaction != null)
            connection = this.Connection;
        else
        {
            isNeedClose = true;
            var connString = this.ConnectionString ?? this.Database.Select();
            connection = this.CreateConnection(connString);
        }
        var dbCommand = this.OrmProvider.CreateCommand();
        command = connection.CreateCommand(dbCommand);
        command.CommandType = CommandType.Text;
        command.CommandTimeout = this.CommandTimeout;
        command.Transaction = this.Transaction;
        command.OnExecuting = this.DbInterceptors.OnCommandExecuting;
        command.OnExecuted = this.DbInterceptors.OnCommandExecuted;
        return (isNeedClose, connection, command);
    }
    public (bool, ITheaConnection, ITheaCommand) UseSlaveCommand(IDbCommand dbCommand = null)
    {
        bool isNeedClose = false;
        ITheaConnection connection;
        ITheaCommand command;
        if (this.Transaction != null)
            connection = this.Connection;
        else
        {
            isNeedClose = true;
            var connString = this.ConnectionString ?? this.Database.SelectSlave();
            connection = this.CreateConnection(connString);
        }
        dbCommand ??= this.OrmProvider.CreateCommand();
        command = connection.CreateCommand(dbCommand);
        command.CommandType = CommandType.Text;
        command.CommandTimeout = this.CommandTimeout;
        command.Transaction = this.Transaction;
        command.OnExecuting = this.DbInterceptors.OnCommandExecuting;
        command.OnExecuted = this.DbInterceptors.OnCommandExecuted;
        return (isNeedClose, connection, command);
    }
    private ITheaConnection CreateConnection(string connectionString)
    {
        var connection = this.OrmProvider.CreateConnection(this.DbKey, connectionString);
        connection.OnOpening = this.DbInterceptors.OnConnectionOpening;
        connection.OnOpened = this.DbInterceptors.OnConnectionOpened;
        connection.OnClosing = this.DbInterceptors.OnConnectionClosing;
        connection.OnClosed = this.DbInterceptors.OnConnectionClosed;
        connection.OnTransactionCreated = this.DbInterceptors.OnTransactionCreated;
        connection.OnTransactionCompleted = this.DbInterceptors.OnTransactionCompleted;

        this.DbInterceptors.OnConnectionCreated?.Invoke(new ConectionEventArgs
        {
            DbKey = this.DbKey,
            ConnectionId = connection.ConnectionId,
            ConnectionString = connectionString
        });
        return connection;
    }
    #endregion

    #region QueryScalar
    public TValue QueryScalar<TValue>(string rawSql, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, commandType);

        connection.Open();
        TValue result = default;
        var objResult = command.ExecuteScalar(CommandSqlType.Select);
        if (objResult != null && objResult is not DBNull)
            result = (TValue)Convert.ChangeType(objResult, typeof(TValue));

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TValue> QueryScalarAsync<TValue>(string rawSql, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, commandType);

        await connection.OpenAsync(cancellationToken);
        TValue result = default;
        var objResult = await command.ExecuteScalarAsync(CommandSqlType.Select, cancellationToken);
        if (objResult != null && objResult is not DBNull)
            result = (TValue)Convert.ChangeType(objResult, typeof(TValue));

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public TValue QueryScalar<TValue>(string rawSql, object parameters, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, parameters, commandType);

        connection.Open();
        TValue result = default;
        var objResult = command.ExecuteScalar(CommandSqlType.Select);
        if (objResult != null && objResult is not DBNull)
            result = (TValue)Convert.ChangeType(objResult, typeof(TValue));

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TValue> QueryScalarAsync<TValue>(string rawSql, object parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, parameters, commandType);

        await connection.OpenAsync(cancellationToken);
        TValue result = default;
        var objResult = await command.ExecuteScalarAsync(CommandSqlType.Select, cancellationToken);
        if (objResult != null && objResult is not DBNull)
            result = (TValue)Convert.ChangeType(objResult, typeof(TValue));

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public TValue QueryScalar<TValue>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlParametersCommand(rawSql, parameters, commandType);

        connection.Open();
        TValue result = default;
        var objResult = command.ExecuteScalar(CommandSqlType.Select);
        if (objResult != null && objResult is not DBNull)
            result = (TValue)Convert.ChangeType(objResult, typeof(TValue));

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TValue> QueryScalarAsync<TValue>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlParametersCommand(rawSql, parameters, commandType);

        await connection.OpenAsync(cancellationToken);
        TValue result = default;
        var objResult = await command.ExecuteScalarAsync(CommandSqlType.Select, cancellationToken);
        if (objResult != null && objResult is not DBNull)
            result = (TValue)Convert.ChangeType(objResult, typeof(TValue));

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public TResult QueryScalar<TResult>(IQueryVisitor visitor)
    {
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        (var sql, _) = this.BuildSql(visitor);
        sql = this.BuildScalarShardingSql(visitor, sql);
        command.CommandText = sql;
        visitor.DbParameters.CopyTo(command.Parameters);

        connection.Open();
        TResult result = default;
        var objResult = command.ExecuteScalar(CommandSqlType.Select);
        if (objResult != null && objResult is not DBNull)
            result = (TResult)Convert.ChangeType(objResult, typeof(TResult));

        command.Dispose();
        if (isNeedClose) connection.Close();
        visitor.Dispose();
        return result;
    }
    public async Task<TResult> QueryScalarAsync<TResult>(IQueryVisitor visitor, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        (var sql, var readerFields) = this.BuildSql(visitor);
        sql = this.BuildScalarShardingSql(visitor, sql);
        command.CommandText = sql;
        visitor.DbParameters.CopyTo(command.Parameters);

        await connection.OpenAsync(cancellationToken);
        TResult result = default;
        var objResult = await command.ExecuteScalarAsync(CommandSqlType.Select, cancellationToken);
        if (objResult != null && objResult is not DBNull)
            result = (TResult)Convert.ChangeType(objResult, typeof(TResult));

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        visitor.Dispose();
        return result;
    }
    public bool QueryExists(IQueryVisitor visitor)
    {
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        var sql = visitor.BuildSql(true, out var readerFields);
        command.CommandText = sql;
        visitor.DbParameters.CopyTo(command.Parameters);

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
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        var sql = visitor.BuildSql(true, out var readerFields);
        command.CommandText = sql;
        visitor.DbParameters.CopyTo(command.Parameters);

        await connection.OpenAsync(cancellationToken);
        var objResult = await command.ExecuteScalarAsync(CommandSqlType.Select, cancellationToken);
        var result = objResult != null && objResult is not DBNull;

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        visitor.Dispose();
        return result;
    }
    #endregion

    #region QueryValue
    public List<TTarget> QueryValue<TTarget>(string rawSql, Func<ITheaDataReader, List<TTarget>> readerInitializer, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, commandType);

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        var result = readerInitializer.Invoke(reader);
        reader.Dispose();

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<List<TTarget>> QueryValueAsync<TTarget>(string rawSql, Func<ITheaDataReader, CancellationToken, Task<List<TTarget>>> readerInitializer,
        CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, commandType);

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        var result = await readerInitializer.Invoke(reader, cancellationToken);
        await reader.DisposeAsync();

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public List<TTarget> QueryValue<TTarget>(string rawSql, object parameters,
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
    public async Task<List<TTarget>> QueryValueAsync<TTarget>(string rawSql, object parameters,
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
    public List<TTarget> QueryValue<TTarget>(string rawSql, List<IDbDataParameter> parameters,
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
    public async Task<List<TTarget>> QueryValueAsync<TTarget>(string rawSql, List<IDbDataParameter> parameters,
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
    #endregion

    #region Query
    public TResult Query<TTarget, TResult>(string rawSql, bool isBulk,
        Func<ITheaDataReader, Func<ITheaDataReader, object>, TResult> readerInitializer, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, commandType);

        connection.Open();
        var behavior = isBulk ? CommandBehavior.SequentialAccess : CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        var deserializer = reader.GetReaderDeserializer(typeof(TTarget), this);
        var result = readerInitializer.Invoke(reader, deserializer);
        reader.Dispose();

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TResult> QueryAsync<TTarget, TResult>(string rawSql, bool isBulk,
        Func<ITheaDataReader, Func<ITheaDataReader, object>, CancellationToken, Task<TResult>> readerInitializer, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, commandType);

        await connection.OpenAsync(cancellationToken);
        var behavior = isBulk ? CommandBehavior.SequentialAccess : CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        var deserializer = reader.GetReaderDeserializer(typeof(TTarget), this);
        var result = await readerInitializer.Invoke(reader, deserializer, cancellationToken);
        await reader.DisposeAsync();

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public TResult Query<TTarget, TResult>(string rawSql, bool isBulk, object parameters,
        Func<ITheaDataReader, Func<ITheaDataReader, object>, TResult> readerInitializer, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, parameters, commandType);

        connection.Open();
        var behavior = isBulk ? CommandBehavior.SequentialAccess : CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        var deserializer = reader.GetReaderDeserializer(typeof(TTarget), this);
        var result = readerInitializer.Invoke(reader, deserializer);
        reader.Dispose();

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TResult> QueryAsync<TTarget, TResult>(string rawSql, bool isBulk, object parameters,
        Func<ITheaDataReader, Func<ITheaDataReader, object>, CancellationToken, Task<TResult>> readerInitializer, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, parameters, commandType);

        await connection.OpenAsync(cancellationToken);
        var behavior = isBulk ? CommandBehavior.SequentialAccess : CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        var deserializer = reader.GetReaderDeserializer(typeof(TTarget), this);
        var result = await readerInitializer.Invoke(reader, deserializer, cancellationToken);
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
        var commandInitializer = RepositoryHelper.BuildQueryRawSqlCommandInitializer(this.OrmProvider, rawSql, parameters);
        commandInitializer.Invoke(command.Parameters, this.OrmProvider, parameters);
        command.CommandText = rawSql;
        command.CommandType = commandType;
        return (isNeedClose, connection, command);
    }

    public TResult QueryRaw<TTarget, TResult>(string rawSql, bool isBulk, List<IDbDataParameter> parameters,
        Func<ITheaDataReader, Func<ITheaDataReader, object>, TResult> readerInitializer, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlParametersCommand(rawSql, parameters, commandType);

        connection.Open();
        var behavior = isBulk ? CommandBehavior.SequentialAccess : CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        var deserializer = reader.GetReaderDeserializer(typeof(TTarget), this);
        var result = readerInitializer.Invoke(reader, deserializer);
        reader.Dispose();

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TResult> QueryRawAsync<TTarget, TResult>(string rawSql, bool isBulk, List<IDbDataParameter> parameters,
        Func<ITheaDataReader, Func<ITheaDataReader, object>, CancellationToken, Task<TResult>> readerInitializer, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlParametersCommand(rawSql, parameters, commandType);

        await connection.OpenAsync(cancellationToken);
        var behavior = isBulk ? CommandBehavior.SequentialAccess : CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        var deserializer = reader.GetReaderDeserializer(typeof(TTarget), this);
        var result = await readerInitializer.Invoke(reader, deserializer, cancellationToken);
        await reader.DisposeAsync();

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
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

    public TResult Query<TEntity, TResult>(object whereObjs, bool isUseKey, bool isBulk, Func<ITheaDataReader, Func<ITheaDataReader, object>, TResult> readerInitializer)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryWhereCommand(typeof(TEntity), whereObjs, isUseKey, isBulk);

        connection.Open();
        var behavior = isBulk ? CommandBehavior.SequentialAccess : CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this);
        var result = readerInitializer.Invoke(reader, deserializer);

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TResult> QueryAsync<TEntity, TResult>(object whereObjs, bool isUseKey, bool isBulk, Func<ITheaDataReader, Func<ITheaDataReader, object>, CancellationToken, Task<TResult>> readerInitializer, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryWhereCommand(typeof(TEntity), whereObjs, isUseKey, isBulk);

        await connection.OpenAsync(cancellationToken);
        var behavior = isBulk ? CommandBehavior.SequentialAccess : CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this);
        var result = await readerInitializer.Invoke(reader, deserializer, cancellationToken);

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    private (bool, ITheaConnection, ITheaCommand) CreateQueryWhereCommand(Type entityType, object whereObjs, bool isUseKey, bool isBulk)
    {
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        var commandInitializer = RepositoryHelper.BuildWhereCommandInitializer(this, entityType, whereObjs, 1, isUseKey, false, isBulk);
        command.CommandText = commandInitializer.Invoke(command.Parameters, this, whereObjs);
        return (isNeedClose, connection, command);
    }
    #endregion

    #region QueryVisitor
    public TResult QueryFrom<TEntity, TResult>(IQueryVisitor visitor, bool isBulk, Func<Type, ITheaDataReader, List<SqlFieldSegment>, TResult> readerInitializer)
    {
        var entityType = typeof(TEntity);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        Expression<Func<TEntity, TEntity>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        (var sql, var readerFields) = this.BuildSql(visitor);

        command.CommandText = sql;
        visitor.DbParameters.CopyTo(command.Parameters);

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
    public async Task<TResult> QueryFromAsync<TEntity, TResult>(IQueryVisitor visitor, bool isBulk, Func<Type, ITheaDataReader, List<SqlFieldSegment>, CancellationToken, Task<TResult>> readerInitializer, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();

        Expression<Func<TEntity, TEntity>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        (var sql, var readerFields) = this.BuildSql(visitor);

        command.CommandText = sql;
        visitor.DbParameters.CopyTo(command.Parameters);

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
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        Expression<Func<TResult, TResult>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        visitor.IsNeedPaging = true;
        (var sql, var readerFields) = this.BuildSql(visitor);

        command.CommandText = sql;
        visitor.DbParameters.CopyTo(command.Parameters);

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        if (reader.Read()) result.TotalCount = reader.ToValue<int>(this);
        result.PageNumber = visitor.PageNumber;
        result.PageSize = visitor.PageSize;

        reader.NextResult();
        var entityType = typeof(TResult);
        var deserializer = reader.GetReaderDeserializer(typeof(TResult), this, readerFields);
        while (reader.Read())
            result.Data.Add((TResult)deserializer.Invoke(reader));
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
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();

        Expression<Func<TResult, TResult>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        visitor.IsNeedPaging = true;
        (var sql, var readerFields) = this.BuildSql(visitor);

        command.CommandText = sql;
        visitor.DbParameters.CopyTo(command.Parameters);

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
            result.TotalCount = reader.ToValue<int>(this);
        result.PageNumber = visitor.PageNumber;
        result.PageSize = visitor.PageSize;

        var entityType = typeof(TResult);
        await reader.NextResultAsync(cancellationToken);
        var deserializer = reader.GetReaderDeserializer(typeof(TResult), this, readerFields);
        while (await reader.ReadAsync(cancellationToken))
            result.Data.Add((TResult)deserializer.Invoke(reader));
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
        var commandInitializer = RepositoryHelper.BuildWhereCommandInitializer(this, entityType, whereObjs, 2, isUseKey, false, isBulk);
        command.CommandText = commandInitializer.Invoke(command.Parameters, this, whereObjs);
        return (isNeedClose, connection, command);
    }

    #endregion

    #region Create
    public int Create<TEntity>(object insertObj)
    {
        (var isNeedClose, var connection, var command) = this.CreateInsertCommand(typeof(TEntity), insertObj, false);
        connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.Insert);

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<int> CreateAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateInsertCommand(typeof(TEntity), insertObj, false);
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.Insert, cancellationToken);

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }

    public int Create<TEntity>(IEnumerable insertObjs, int bulkCount)
    {
        (var isNeedClose, var connection, var command, var headSql, var commandInitializer)
            = this.CreateInsertBulkCommand(typeof(TEntity), insertObjs, bulkCount);

        connection.Open();
        int index = 0, result = 0;
        var builder = new StringBuilder(headSql);
        foreach (var insertObj in insertObjs)
        {
            if (index > 0) builder.Append(',');
            commandInitializer.Invoke(command.Parameters, builder, this, insertObj, index.ToString());
            index++;

            if (index >= bulkCount)
            {
                command.CommandText = builder.ToString();
                result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                builder.Clear();
                command.Parameters.Clear();
                builder.Append(headSql);
                index = 0;
            }
        }
        if (index > 0)
        {
            command.CommandText = builder.ToString();
            result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
            builder.Clear();
            command.Parameters.Clear();
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<int> CreateAsync<TEntity>(IEnumerable insertObjs, int bulkCount, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command, var headSql, var commandInitializer)
            = this.CreateInsertBulkCommand(typeof(TEntity), insertObjs, bulkCount);

        await connection.OpenAsync(cancellationToken);
        int index = 0, result = 0;
        var builder = new StringBuilder(headSql);
        foreach (var insertObj in insertObjs)
        {
            if (index > 0) builder.Append(',');
            commandInitializer.Invoke(command.Parameters, builder, this, insertObj, index.ToString());
            index++;

            if (index >= bulkCount)
            {
                command.CommandText = builder.ToString();
                result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                builder.Clear();
                command.Parameters.Clear();
                builder.Append(headSql);
                index = 0;
            }
        }
        if (index > 0)
        {
            command.CommandText = builder.ToString();
            result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
            builder.Clear();
            command.Parameters.Clear();
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }

    public TResult CreateIdentity<TEntity, TResult>(object insertObj)
    {
        TResult result = default;
        (var isNeedClose, var connection, var command) = this.CreateInsertCommand(typeof(TEntity), insertObj, true);

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(CommandSqlType.Insert, behavior);
        if (reader.Read()) result = reader.ToValue<TResult>(this);

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TResult> CreateIdentityAsync<TEntity, TResult>(object insertObj, CancellationToken cancellationToken = default)
    {
        TResult result = default;
        (var isNeedClose, var connection, var command) = this.CreateInsertCommand(typeof(TEntity), insertObj, true);

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Insert, behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken)) result = reader.ToValue<TResult>(this);

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    private (bool, ITheaConnection, ITheaCommand) CreateInsertCommand(Type entityType, object insertObj, bool hasIdentity)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));

        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        if (insertObj is IDictionary<string, object> dict)
        {
            var entityMapper = this.EntityMapProvider.GetEntityMap(entityType);
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
                var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}";
                if (index > 0)
                {
                    fieldsBuilder.Append(',');
                    valuesBuilder.Append(',');
                }
                fieldsBuilder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));
                valuesBuilder.Append(parameterName);
                if (memberMapper.TypeHandler != null)
                    fieldValue = memberMapper.TypeHandler.ToFieldValue(fieldValue);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    var fieldValueType = fieldValue.GetType();
                    if (fieldValueType != targetType)
                    {
                        var myValueGetter = this.OrmProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this);
                        fieldValue = myValueGetter.Invoke(fieldValue);
                    }
                }
                command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                index++;
            }
            command.CommandText = $"INSERT INTO {this.OrmProvider.GetTableName(entityMapper.TableName)} ({fieldsBuilder.ToString()}) VALUES ({valuesBuilder.ToString()})";
            if (hasIdentity)
            {
                var keyFieldName = this.OrmProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName);
                command.CommandText += this.OrmProvider.GetIdentitySql(keyFieldName);
            }
        }
        else
        {
            if (insertObj is IEnumerable && insertObj is not string)
                throw new NotSupportedException("此方法只支持单条数据插入");

            var parameterType = insertObj.GetType();
            var commandInitializer = RepositoryHelper.BuildTypedCommandInitializer(this, entityType, parameterType, 1, true, hasIdentity, null, null)
                as Func<IDataParameterCollection, DbContext, object, string>;
            command.CommandText = commandInitializer.Invoke(command.Parameters, this, insertObj);
        }
        return (isNeedClose, connection, command);
    }
    private (bool, ITheaConnection, ITheaCommand, string, Action<IDataParameterCollection, StringBuilder, DbContext, object, string>)
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
            if (insertObj == null) throw new ArgumentNullException(nameof(insertObj));
            firstInsertObj = insertObj;
            break;
        }
        var insertObjType = firstInsertObj.GetType();

        string headSql = null;
        Action<IDataParameterCollection, StringBuilder, DbContext, object, string> commandInitializer = null;
        var entityMapper = this.EntityMapProvider.GetEntityMap(entityType);
        if (firstInsertObj is IDictionary<string, object> dict)
        {
            int index = 0;
            var builder = new StringBuilder();
            var valueSetters = new List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>();
            builder.Append($"INSERT INTO {this.OrmProvider.GetTableName(entityMapper.TableName)} (");
            foreach (var key in dict.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper))
                    continue;
                if (memberMapper.IsIgnore || memberMapper.IsAutoIncrement
                    || memberMapper.IsNavigation || memberMapper.IsIgnoreInsert || memberMapper.IsRowVersion)
                    continue;

                if (index > 0)
                {
                    builder.Append(',');
                    builder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));
                }
                Func<IDictionary<string, object>, object> valueGetter = null;
                if (memberMapper.TypeHandler != null)
                    valueGetter = insertObj => memberMapper.TypeHandler.ToFieldValue(insertObj[key]);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    var fieldValueType = dict[key].GetType();
                    if (fieldValueType.ToUnderlyingType() != targetType)
                    {
                        var myValueGetter = this.OrmProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this);
                        valueGetter = insertObj => myValueGetter.Invoke(insertObj[key]);
                    }
                    else valueGetter = insertObj => insertObj[key];
                }

                Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string> valueSetter = null;
                if (index > 0)
                {
                    valueSetter = (dbParameters, builder, insertObj, suffix) =>
                    {
                        var fieldValue = valueGetter.Invoke(insertObj);
                        var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                        builder.Append(parameterName);
                        dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                    };
                }
                else
                {
                    valueSetter = (dbParameters, builder, insertObj, suffix) =>
                    {
                        var fieldValue = valueGetter.Invoke(insertObj);
                        var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                        builder.Append(',');
                        builder.Append(parameterName);
                        dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
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
                builder.Append(')');
            };
        }
        else
        {
            (var fieldsSql, commandInitializer) = ((string, Action<IDataParameterCollection, StringBuilder, DbContext, object, string>))
                RepositoryHelper.BuildTypedBulkCommandInitializer(this, entityType, insertObjType, 1, null, null);
            headSql = $"INSERT INTO {this.OrmProvider.GetTableName(entityMapper.TableName)} ({fieldsSql}) VALUES ";
        }
        return (isNeedClose, connection, command, headSql, commandInitializer);
    }

    public TResult CreateIdentity<TResult>(ICreateVisitor visitor)
    {
        TResult result = default;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        visitor.IsReturnIdentity = true;
        command.CommandText = visitor.BuildSql(command, out _);

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(CommandSqlType.Insert, behavior);
        if (reader.Read()) result = reader.ToValue<TResult>(this);

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TResult> CreateIdentityAsync<TResult>(ICreateVisitor visitor, CancellationToken cancellationToken = default)
    {
        TResult result = default;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        visitor.IsReturnIdentity = true;
        command.CommandText = visitor.BuildSql(command, out _);

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Insert, behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken)) result = reader.ToValue<TResult>(this);

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }

    public TResult CreateResult<TResult>(ICreateVisitor visitor)
    {
        TResult result = default;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        command.CommandText = visitor.BuildSql(command, out var readerFields);

        connection.Open();
        using var reader = command.ExecuteReader(CommandSqlType.Insert, CommandBehavior.SequentialAccess);
        var deserializer = reader.GetReaderDeserializer(typeof(TResult), this, readerFields);
        if (reader.Read())
            result = (TResult)deserializer.Invoke(reader);

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TResult> CreateResultAsync<TResult>(ICreateVisitor visitor, CancellationToken cancellationToken = default)
    {
        TResult result = default;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        command.CommandText = visitor.BuildSql(command, out var readerFields);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Insert, CommandBehavior.SequentialAccess, cancellationToken);
        var deserializer = reader.GetReaderDeserializer(typeof(TResult), this, readerFields);
        if (await reader.ReadAsync(cancellationToken))
            result = (TResult)deserializer.Invoke(reader);

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region Update
    public int Update<TEntity>(object updateObj)
    {
        (var isNeedClose, var connection, var command) = this.CreateUpdateCommand(typeof(TEntity), updateObj);
        connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.Update);

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<int> UpdateAsync<TEntity>(object updateObj, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateUpdateCommand(typeof(TEntity), updateObj);
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.Update, cancellationToken);

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    private (bool, ITheaConnection, ITheaCommand) CreateUpdateCommand(Type entityType, object updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        if (updateObj is IDictionary<string, object> dict)
        {
            var entityMapper = this.EntityMapProvider.GetEntityMap(entityType);
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
                var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}";
                if (fieldsBuilder.Length > 0) fieldsBuilder.Append(',');
                if (whereBuilder.Length > 0) whereBuilder.Append(" AND ");
                var sql = $"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}";
                if (memberMapper.IsKey) whereBuilder.Append(sql);
                else fieldsBuilder.Append(sql);

                if (memberMapper.TypeHandler != null)
                    fieldValue = memberMapper.TypeHandler.ToFieldValue(fieldValue);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    var fieldValueType = fieldValue.GetType();
                    if (fieldValueType != targetType)
                    {
                        var myValueGetter = this.OrmProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this);
                        fieldValue = myValueGetter.Invoke(fieldValue);
                    }
                }
                command.Parameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                index++;
            }
            command.CommandText = $"UPDATE {this.OrmProvider.GetTableName(entityMapper.TableName)} SET {fieldsBuilder.ToString()} WHERE ({whereBuilder.ToString()})";
        }
        else
        {
            if (updateObj is IEnumerable && updateObj is not string)
                throw new NotSupportedException("此方法只支持单条数据更新");

            var parameterType = updateObj.GetType();
            var commandInitializer = RepositoryHelper.BuildTypedCommandInitializer(this, entityType, parameterType, 2, true, false, null, null)
                as Func<IDataParameterCollection, DbContext, object, string>;
            command.CommandText = commandInitializer.Invoke(command.Parameters, this, updateObj);
        }
        return (isNeedClose, connection, command);
    }

    public int Update<TEntity>(IEnumerable updateObjs, int bulkCount)
    {
        (var isNeedClose, var connection, var command, var commandInitializer) =
            this.CreateUpdateBulkCommand(typeof(TEntity), updateObjs, bulkCount);
        int index = 0, result = 0;
        var builder = new StringBuilder();

        connection.Open();
        foreach (var updateObj in updateObjs)
        {
            if (index > 0) builder.Append(';');
            commandInitializer.Invoke(command.Parameters, builder, this, updateObj, index.ToString());
            index++;

            if (index >= bulkCount)
            {
                command.CommandText = builder.ToString();
                result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                builder.Clear();
                command.Parameters.Clear();
                index = 0;
            }
        }
        if (index > 0)
        {
            command.CommandText = builder.ToString();
            result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
            builder.Clear();
            command.Parameters.Clear();
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<int> UpdateAsync<TEntity>(IEnumerable updateObjs, int bulkCount, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command, var commandInitializer) =
             this.CreateUpdateBulkCommand(typeof(TEntity), updateObjs, bulkCount);
        int index = 0, result = 0;
        var builder = new StringBuilder();

        await connection.OpenAsync(cancellationToken);
        foreach (var updateObj in updateObjs)
        {
            if (index > 0) builder.Append(';');
            commandInitializer.Invoke(command.Parameters, builder, this, updateObj, index.ToString());
            index++;

            if (index >= bulkCount)
            {
                command.CommandText = builder.ToString();
                result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                builder.Clear();
                command.Parameters.Clear();
                index = 0;
            }
        }
        if (index > 0)
        {
            command.CommandText = builder.ToString();
            result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
            builder.Clear();
            command.Parameters.Clear();
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    private (bool, ITheaConnection, ITheaCommand, Action<IDataParameterCollection, StringBuilder, DbContext, object, string>)
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
            var entityMapper = this.EntityMapProvider.GetEntityMap(entityType);
            var valueSetters = new List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>();
            var whereSetters = new List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>();
            foreach (var key in dict.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper))
                    continue;
                if (memberMapper.IsIgnore || memberMapper.IsAutoIncrement
                    || memberMapper.IsNavigation || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
                    continue;

                Func<IDictionary<string, object>, object> valueGetter = null;
                if (memberMapper.TypeHandler != null)
                    valueGetter = insertObj => memberMapper.TypeHandler.ToFieldValue(insertObj[key]);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    var fieldValueType = dict[key].GetType();
                    if (fieldValueType.ToUnderlyingType() != targetType)
                    {
                        var myValueGetter = this.OrmProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this);
                        valueGetter = insertObj => myValueGetter.Invoke(insertObj[key]);
                    }
                    else valueGetter = insertObj => insertObj[key];
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
                            var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                            builder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                            dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                        };
                    }
                    else
                    {
                        valueSetter = (dbParameters, builder, insertObj, suffix) =>
                        {
                            var fieldValue = valueGetter.Invoke(insertObj);
                            var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                            builder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                            dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
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
                            var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                            builder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                            dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                        };
                    }
                    else
                    {
                        valueSetter = (dbParameters, builder, insertObj, suffix) =>
                        {
                            var fieldValue = valueGetter.Invoke(insertObj);
                            var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                            builder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                            dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                        };
                    }
                    valueSetters.Add(valueSetter);
                }
            }
            commandInitializer = (dbParameters, builder, dbContext, insertObj, suffix) =>
            {
                var dictObj = insertObj as IDictionary<string, object>;
                builder.Append($"UPDATE {this.OrmProvider.GetTableName(entityMapper.TableName)} SET ");
                foreach (var valueSetter in valueSetters)
                    valueSetter.Invoke(dbParameters, builder, dictObj, suffix);
                builder.Append(" WHERE ");
                foreach (var valueSetter in whereSetters)
                    valueSetter.Invoke(dbParameters, builder, dictObj, suffix);
            };
        }
        else commandInitializer = RepositoryHelper.BuildTypedBulkCommandInitializer(this, entityType, updateObjType, 2, null, null)
            as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
        return (isNeedClose, connection, command, commandInitializer);
    }
    #endregion

    #region Delete
    public int Delete<TEntity>(object whereObjs, bool isUseKey, bool isBulk)
    {
        (var isNeedClose, var connection, var command) = this.CreateDeleteCommand(typeof(TEntity), whereObjs, isUseKey, isBulk);
        connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.Delete);

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<int> DeleteAsync<TEntity>(object whereObjs, bool isUseKey, bool isBulk, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateDeleteCommand(typeof(TEntity), whereObjs, isUseKey, isBulk);
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.Delete, cancellationToken);

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    private (bool, ITheaConnection, ITheaCommand) CreateDeleteCommand(Type entityType, object whereObjs, bool isUseKey, bool isBulk)
    {
        if (whereObjs == null)
            throw new ArgumentNullException(nameof(whereObjs));
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        var commandInitializer = RepositoryHelper.BuildWhereCommandInitializer(this, entityType, whereObjs, 3, isUseKey, false, isBulk);
        command.CommandText = commandInitializer.Invoke(command.Parameters, this, whereObjs);
        return (isNeedClose, connection, command);
    }
    #endregion

    #region Execute
    public int Execute(string rawSql, object parameters = null, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, parameters, commandType);
        connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.RawExecute);
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<int> ExecuteAsync(string rawSql, object parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlCommand(rawSql, parameters, commandType);
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.RawExecute, cancellationToken);
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public int Execute(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlParametersCommand(rawSql, parameters, commandType);
        connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.RawExecute);
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<int> ExecuteAsync(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryRawSqlParametersCommand(rawSql, parameters, commandType);
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.RawExecute, cancellationToken);
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region Others   
    public void BeginTransaction()
    {
        if (this.Transaction != null)
            throw new Exception("上一个事务还没有完成，无法开启新事务");
        this.Connection ??= this.CreateConnection(this.Database.Select());
        this.Connection.Open();
        this.Transaction = this.Connection.BeginTransaction();
    }
    public async ValueTask BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (this.Transaction != null)
            throw new Exception("上一个事务还没有完成，无法开启新事务");
        this.Connection ??= this.CreateConnection(this.Database.Select());
        await this.Connection.OpenAsync(cancellationToken);
        this.Transaction = await this.Connection.BeginTransactionAsync(cancellationToken);
    }
    public void Commit()
    {
        if (this.Transaction == null)
            throw new Exception("还没有开启事务，无法完成提交");
        this.Transaction.Commit();
        this.Connection.Close();
        this.Transaction = null;
        this.Connection = null;
    }
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (this.Transaction == null)
            throw new Exception("还没有开启事务，无法完成提交");
        await this.Transaction.CommitAsync(cancellationToken);
        await this.Connection.CloseAsync();
        this.Transaction = null;
        this.Connection = null;
    }
    public void Rollback()
    {
        if (this.Transaction == null)
            throw new Exception("还没有开启事务，无法完成回滚");
        this.Transaction.Rollback();
        this.Connection.Close();
        this.Transaction = null;
        this.Connection = null;
    }
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (this.Transaction == null)
            throw new Exception("还没有开启事务，无法完成回滚");
        await this.Transaction.RollbackAsync(cancellationToken);
        await this.Connection.CloseAsync();
        this.Transaction = null;
        this.Connection = null;
    }
    #endregion

    #region Sharding
    public (string, List<SqlFieldSegment>) BuildSql(IQueryVisitor visitor)
    {
        var sql = visitor.BuildSql(true, out var readerFields);
        if (visitor.IsNeedFormatShardingTables)
            sql = this.BuildShardingTablesSqlByFormat(visitor as SqlVisitor, sql, visitor.ShardingTableJointMark);
        if (visitor.IsNeedUnionShardingTables)
            sql = visitor.BuildShardingSql(sql);
        return (sql, readerFields);
    }
    public string BuildScalarShardingSql(IQueryVisitor visitor, string rawSql)
    {
        if (visitor.IsManyShardingTables && visitor.AggFieldAlias != null)
        {
            string aggFields = null;
            switch (visitor.AggFieldAlias)
            {
                case "COUNT_VALUE":
                    aggFields = "SUM(COUNT_VALUE)";
                    break;
                case "SUM_VALUE":
                    aggFields = "SUM(SUM_VALUE)";
                    break;
                case "AVG_VALUE":
                    aggFields = "SUM(AVG_VALUE)/SUM(AVG_COUNT)";
                    break;
                case "MAX_VALUE":
                    aggFields = "MAX(MAX_VALUE)";
                    break;
                case "MIN_VALUE":
                    aggFields = "MIN(MIN_VALUE)";
                    break;
            }
            return $"SELECT {aggFields} FROM ({rawSql}) AS t";
        }
        return rawSql;
    }
    public string BuildShardingTablesSqlByFormat(SqlVisitor visitor, string formatSql, string jointMark)
    {
        //查询，分表多个表时，都使用表名替换生成分表sql
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
        if (this.TableShardingProvider == null || !this.TableShardingProvider.TryGetTableSharding(entityType, out var shardingTableInfo))
            throw new InvalidOperationException($"实体表{entityType.FullName}没有配置分表，无需调用此方法");
        if (!this.EntityMapProvider.TryGetEntityMap(entityType, out var entityMap))
            throw new InvalidOperationException($"实体表{entityType.FullName}没有配置映射关系，无法获取分表信息");
        return shardingTableInfo.Rule.Invoke(entityMap.TableName, fieldValues) as string;
    }
    #endregion
}