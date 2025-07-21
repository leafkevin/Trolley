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
    public string DefaultTableSchema { get; internal set; }
    public IOrmProvider OrmProvider { get; internal set; }
    public IEntityMapProvider MapProvider { get; internal set; }
    public ITableShardingProvider ShardingProvider { get; internal set; }
    public DbInterceptors DbInterceptors { get; internal set; }

    public ITheaConnection Connection { get; set; }
    public ITheaTransaction Transaction { get; set; }

    public bool IsConstantParameterized { get; set; }
    public string UserParameterPrefix { get; set; }
    public int CommandTimeout { get; set; }
    public Type DefaultEnumMapDbType { get; set; }
    public DateTimeKind DefaultDateTimeKind { get; set; }
    public Delegate CommandShardingTableGetter { get; set; }
    public ITypeHandler JsonTypeHandler { get; set; }
    public ITypeHandler ToStringTypeHandler { get; set; }
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
            var connString = this.ConnectionString ?? this.Database.UseMaster();
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
            var connString = this.ConnectionString ?? this.Database.UseSlave();
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

    #region Query
    public List<TEntity> Query<TEntity>(string rawSql, object parameters = null, CommandType commandType = CommandType.Text)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters != null)
        {
            var whereObjType = parameters.GetType();
            if (!whereObjType.IsEntityType(out _))
                throw new NotSupportedException("不支持的参数类型，Query方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");
        }

        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        if (parameters != null)
        {
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
            commandInitializer.Invoke(command.Parameters, this.OrmProvider, parameters);
        }
        command.CommandText = rawSql;
        command.CommandType = commandType;

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        var result = new List<TEntity>();
        var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this);
        while (reader.Read())
            result.Add((TEntity)deserializer.Invoke(reader));

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, object parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters != null)
        {
            var whereObjType = parameters.GetType();
            if (!whereObjType.IsEntityType(out _))
                throw new NotSupportedException("不支持的参数类型，QueryAsync方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");
        }

        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        if (parameters != null)
        {
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
            commandInitializer.Invoke(command.Parameters, this.OrmProvider, parameters);
        }
        command.CommandText = rawSql;
        command.CommandType = commandType;

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        var result = new List<TEntity>();
        var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this);
        while (await reader.ReadAsync(cancellationToken))
            result.Add((TEntity)deserializer.Invoke(reader));

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }

    public TResult Query<TEntity, TResult>(object whereObj, bool isSingle, Func<ITheaDataReader, TResult> readerInitializer)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        var whereObjType = whereObj.GetType();
        if (!whereObjType.IsEntityType(out _))
            throw new NotSupportedException("不支持的参数类型，Query方法的whereObj参数，支持实体类型参数，命名、匿名对象或是字典对象");
        bool isBulk = whereObj is IEnumerable && whereObj is not string && whereObj is not IDictionary<string, object>;

        var entityType = typeof(TEntity);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        if (isBulk)
        {
            (var isInExpr, var headSql, var commandInitializer) = ((bool, string, object))RepositoryHelper.BuildQueryWhereObjSqlParameters(this, entityType, whereObjType, whereObj, false, isBulk);
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            var parameters = whereObj as IEnumerable;
            int index = 0;
            var builder = new StringBuilder(headSql);
            var jointMark = isInExpr ? "," : " OR ";
            foreach (var parameter in parameters)
            {
                if (index > 0) builder.Append(jointMark);
                typedCommandInitializer.Invoke(command.Parameters, builder, this, parameter, index.ToString());
                index++;
            }
            if (isInExpr) builder.Append(')');
            command.CommandText = builder.ToString();
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this, entityType, whereObjType, whereObj, false, isBulk);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
            command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this, whereObj);
        }

        connection.Open();
        var behavior = isSingle ? CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow : CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        var result = readerInitializer.Invoke(reader);

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TResult> QueryAsync<TEntity, TResult>(object whereObj, bool isSingle, Func<ITheaDataReader, CancellationToken, Task<TResult>> readerInitializer, CancellationToken cancellationToken = default)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        var whereObjType = whereObj.GetType();
        if (!whereObjType.IsEntityType(out _))
            throw new NotSupportedException("不支持的参数类型，Query方法的whereObj参数，支持实体类型参数，命名、匿名对象或是字典对象");
        bool isBulk = whereObj is IEnumerable && whereObj is not string && whereObj is not IDictionary<string, object>;

        var entityType = typeof(TEntity);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();

        if (isBulk)
        {
            (var isInExpr, var headSql, var commandInitializer) = ((bool, string, object))RepositoryHelper.BuildQueryWhereObjSqlParameters(this, entityType, whereObjType, whereObj, false, isBulk);
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            var parameters = whereObj as IEnumerable;
            int index = 0;
            var builder = new StringBuilder(headSql);
            var jointMark = isInExpr ? "," : " OR ";
            foreach (var parameter in parameters)
            {
                if (index > 0) builder.Append(jointMark);
                typedCommandInitializer.Invoke(command.Parameters, builder, this, parameter, index.ToString());
                index++;
            }
            if (isInExpr) builder.Append(')');
            command.CommandText = builder.ToString();
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this, entityType, whereObjType, whereObj, false, isBulk);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
            command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this, whereObj);
        }

        await connection.OpenAsync(cancellationToken);
        var behavior = isSingle ? CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow : CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        var result = await readerInitializer.Invoke(reader, cancellationToken);

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public TResult QueryById<TEntity, TResult>(object whereKeys, bool isSingle, Func<ITheaDataReader, TResult> readerInitializer)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));
        var entityType = typeof(TEntity);
        bool isBulk = whereKeys is IEnumerable && whereKeys is not string && whereKeys is not IDictionary<string, object>;
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();

        if (isBulk)
        {
            (var isInExpr, var headSql, var commandInitializer) = ((bool, string, object))RepositoryHelper.BuildQueryWhereObjByKeySqlParameters(this, entityType, whereKeys, false, isBulk);
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            var parameters = whereKeys as IEnumerable;
            int index = 0;
            var builder = new StringBuilder(headSql);
            var jointMark = isInExpr ? "," : " OR ";
            foreach (var parameter in parameters)
            {
                if (index > 0) builder.Append(jointMark);
                typedCommandInitializer.Invoke(command.Parameters, builder, this, parameter, index.ToString());
                index++;
            }
            if (isInExpr) builder.Append(')');
            command.CommandText = builder.ToString();
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjByKeySqlParameters(this, entityType, whereKeys, false, isBulk);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
            command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this, whereKeys);
        }

        connection.Open();
        var behavior = isSingle ? CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow : CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        var result = readerInitializer.Invoke(reader);

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TResult> QueryByIdAsync<TEntity, TResult>(object whereKeys, bool isSingle, Func<ITheaDataReader, CancellationToken, Task<TResult>> readerInitializer, CancellationToken cancellationToken = default)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));
        bool isBulk = whereKeys is IEnumerable && whereKeys is not string && whereKeys is not IDictionary<string, object>;
        var entityType = typeof(TEntity);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();

        if (isBulk)
        {
            (var isInExpr, var headSql, var commandInitializer) = ((bool, string, object))RepositoryHelper.BuildQueryWhereObjByKeySqlParameters(this, entityType, whereKeys, false, isBulk);
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            var parameters = whereKeys as IEnumerable;
            int index = 0;
            var builder = new StringBuilder(headSql);
            var jointMark = isInExpr ? "," : " OR ";
            foreach (var parameter in parameters)
            {
                if (index > 0) builder.Append(jointMark);
                typedCommandInitializer.Invoke(command.Parameters, builder, this, parameter, index.ToString());
                index++;
            }
            if (isInExpr) builder.Append(')');
            command.CommandText = builder.ToString();
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjByKeySqlParameters(this, entityType, whereKeys, false, isBulk);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
            command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this, whereKeys);
        }

        await connection.OpenAsync(cancellationToken);
        var behavior = isSingle ? CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow : CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        var result = await readerInitializer.Invoke(reader, cancellationToken);

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public TResult QueryScalar<TResult>(IQueryVisitor visitor)
    {
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        (var isSuccess, var sql, _) = this.BuildSql(visitor, " UNION ALL ");
        if (!isSuccess) return default;

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
        (var isSuccess, var sql, var readerFields) = await this.BuildSqlAsync(visitor, " UNION ALL ", cancellationToken);
        if (!isSuccess) return default;

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
    public TResult QueryFrom<TEntity, TResult>(IQueryVisitor visitor, bool isSingle, Func<Type, ITheaDataReader, List<SqlFieldSegment>, TResult> readerInitializer)
    {
        var entityType = typeof(TEntity);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        Expression<Func<TEntity, TEntity>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        (var isSuccess, var sql, var readerFields) = this.BuildSql(visitor, " UNION ALL ");
        if (!isSuccess) return default;

        command.CommandText = sql;
        visitor.DbParameters.CopyTo(command.Parameters);

        connection.Open();
        var behavior = isSingle ? CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow : CommandBehavior.SequentialAccess;
        var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        var result = readerInitializer.Invoke(entityType, reader, readerFields);
        if (visitor.BuildIncludeSql(entityType, result, isSingle, out sql))
        {
            reader.Dispose();
            command.CommandText = sql;
            command.Parameters.Clear();
            visitor.NextDbParameters.CopyTo(command.Parameters);
            reader = command.ExecuteReader(CommandSqlType.Select, CommandBehavior.SequentialAccess);
            visitor.SetIncludeValues(entityType, result, reader, isSingle);
        }

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        visitor.Dispose();
        return result;
    }
    public async Task<TResult> QueryFromAsync<TEntity, TResult>(IQueryVisitor visitor, bool isSingle, Func<Type, ITheaDataReader, List<SqlFieldSegment>, CancellationToken, Task<TResult>> readerInitializer, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();

        Expression<Func<TEntity, TEntity>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        (var isSuccess, var sql, var readerFields) = await this.BuildSqlAsync(visitor, " UNION ALL ", cancellationToken);
        if (!isSuccess) return default;

        command.CommandText = sql;
        visitor.DbParameters.CopyTo(command.Parameters);

        await connection.OpenAsync(cancellationToken);
        var behavior = isSingle ? CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow : CommandBehavior.SequentialAccess;
        var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        var result = await readerInitializer.Invoke(entityType, reader, readerFields, cancellationToken);
        if (visitor.BuildIncludeSql(entityType, result, isSingle, out sql))
        {
            await reader.DisposeAsync();
            command.CommandText = sql;
            command.Parameters.Clear();
            visitor.NextDbParameters.CopyTo(command.Parameters);
            reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
            await visitor.SetIncludeValuesAsync(entityType, result, reader, isSingle, cancellationToken);
        }

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        visitor.Dispose();
        return result;
    }
    #endregion

    #region QueryPage
    public IPagedList<TResult> QueryPage<TResult>(IQueryVisitor visitor)
    {
        var result = new PagedList<TResult> { Data = new List<TResult>() };
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        Expression<Func<TResult, TResult>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        visitor.IsNeedPaging = true;
        (var isSuccess, var sql, var readerFields) = this.BuildSql(visitor, " UNION ALL ");
        if (!isSuccess) return default;

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
        (var isSuccess, var sql, var readerFields) = await this.BuildSqlAsync(visitor, " UNION ALL ", cancellationToken);
        if (!isSuccess) return result;

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

    #region Create
    public TResult CreateIdentity<TEntity, TResult>(object insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        bool isBulk = insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>;
        if (isBulk) throw new NotSupportedException("CreateIdentity方法只支持单条数据插入，不支持批量插入返回Identity");

        TResult result = default;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        var entityType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildCreateCommandInitializer(this, entityType, insertObj, true);
        commandInitializer.Invoke(this, command, insertObj);

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
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        bool isBulk = insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>;
        if (isBulk) throw new NotSupportedException("CreateIdentityAsync方法只支持单条数据插入，不支持批量插入返回Identity");

        TResult result = default;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        var entityType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildCreateCommandInitializer(this, entityType, insertObj, true);
        commandInitializer.Invoke(this, command, insertObj);

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Insert, behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken)) result = reader.ToValue<TResult>(this);

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public TResult CreateIdentity<TResult>(ICreateVisitor visitor)
    {
        TResult result = default;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        command.CommandText = visitor.BuildCommand(command, true, out _);

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(CommandSqlType.Insert, behavior);
        if (reader.Read()) result = reader.ToValue<TResult>(this);

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        visitor.Dispose();
        return result;
    }
    public async Task<TResult> CreateIdentityAsync<TResult>(ICreateVisitor visitor, CancellationToken cancellationToken = default)
    {
        TResult result = default;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        command.CommandText = visitor.BuildCommand(command, true, out _);

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Insert, behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken)) result = reader.ToValue<TResult>(this);

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        visitor.Dispose();
        return result;
    }

    public TResult CreateResult<TResult>(ICreateVisitor visitor)
    {
        TResult result = default;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        command.CommandText = visitor.BuildCommand(command, false, out var readerFields);

        connection.Open();
        using var reader = command.ExecuteReader(CommandSqlType.Insert, CommandBehavior.SequentialAccess);
        var deserializer = reader.GetReaderDeserializer(typeof(TResult), this, readerFields);
        if (reader.Read())
            result = (TResult)deserializer.Invoke(reader);

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        visitor.Dispose();
        return result;
    }
    public async Task<TResult> CreateResultAsync<TResult>(ICreateVisitor visitor, CancellationToken cancellationToken = default)
    {
        TResult result = default;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        command.CommandText = visitor.BuildCommand(command, false, out var readerFields);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Insert, CommandBehavior.SequentialAccess, cancellationToken);
        var deserializer = reader.GetReaderDeserializer(typeof(TResult), this, readerFields);
        if (await reader.ReadAsync(cancellationToken))
            result = (TResult)deserializer.Invoke(reader);

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        visitor.Dispose();
        return result;
    }
    #endregion

    #region Others   
    public void BeginTransaction()
    {
        if (this.Transaction != null)
            throw new Exception("上一个事务还没有完成，无法开启新事务");
        this.Connection ??= this.CreateConnection(this.Database.UseMaster());
        this.Connection.Open();
        this.Transaction = this.Connection.BeginTransaction();
    }
    public async ValueTask BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (this.Transaction != null)
            throw new Exception("上一个事务还没有完成，无法开启新事务");
        this.Connection ??= this.CreateConnection(this.Database.UseMaster());
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
    public (bool, string, List<SqlFieldSegment>) BuildSql(IQueryVisitor visitor, string jointMark)
    {
        bool isSuccess = true;
        string sql = null;
        if (visitor.IsNeedFetchShardingTables)
            isSuccess = this.FetchShardingTables(visitor as SqlVisitor);
        if (!isSuccess) return (isSuccess, sql, null);
        sql = visitor.BuildSql(true, out var readerFields);
        if (visitor.IsNeedFormatShardingTables)
            sql = this.BuildShardingTablesSqlByFormat(visitor as SqlVisitor, sql, jointMark);
        if (visitor.IsNeedUnionShardingTables)
            sql = visitor.BuildShardingSql(sql);
        return (isSuccess, sql, readerFields);
    }
    public async Task<(bool, string, List<SqlFieldSegment>)> BuildSqlAsync(IQueryVisitor visitor, string jointMark, CancellationToken cancellationToken = default)
    {
        bool isSuccess = true;
        string sql = null;
        if (visitor.IsNeedFetchShardingTables)
            isSuccess = await this.FetchShardingTablesAsync(visitor as SqlVisitor, cancellationToken);
        if (!isSuccess) return (isSuccess, sql, null);
        sql = visitor.BuildSql(true, out var readerFields);
        if (visitor.IsNeedFormatShardingTables)
            sql = this.BuildShardingTablesSqlByFormat(visitor as SqlVisitor, sql, jointMark);
        if (visitor.IsNeedUnionShardingTables)
            sql = visitor.BuildShardingSql(sql);
        return (isSuccess, sql, readerFields);
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
    public bool FetchShardingTables(SqlVisitor visitor)
    {
        var fetchSql = visitor.BuildTableShardingsSql();
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        command.CommandText = fetchSql;
        connection.Open();
        using var reader = command.ExecuteReader(CommandSqlType.Select, CommandBehavior.SequentialAccess);
        var shardingTables = new List<string>();
        while (reader.Read())
        {
            shardingTables.Add(reader.ToValue<string>(this));
        }
        var hasTables = visitor.SetShardingTables(shardingTables);

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return hasTables;
    }
    public async Task<bool> FetchShardingTablesAsync(SqlVisitor visitor, CancellationToken cancellationToken = default)
    {
        var fetchSql = visitor.BuildTableShardingsSql();
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        command.CommandText = fetchSql;
        command.Parameters.Clear();
        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, CommandBehavior.SequentialAccess, cancellationToken);
        var shardingTables = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            shardingTables.Add(reader.ToValue<string>(this));
        }
        var hasTables = visitor.SetShardingTables(shardingTables);

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return hasTables;
    }
    public string BuildShardingTablesSqlByFormat(SqlVisitor visitor, string formatSql, string jointMark)
    {
        //查询，分表多个表时，都使用表名替换生成分表sql
        var builder = new StringBuilder();
        if (visitor.ShardingTables.Count > 1)
        {
            var masterTableSegment = visitor.ShardingTables[0];
            var loopCount = masterTableSegment.TableNames.Count;
            if (loopCount > 1) masterTableSegment.TableNames.Sort((x, y) => x.CompareTo(y));
            var origMasterName = masterTableSegment.Mapper.TableName;
            Dictionary<TableSegment, List<string>> tableShardings = new();
            for (int i = 0; i < loopCount; i++)
            {
                var masterTableName = masterTableSegment.TableNames[i];
                var sql = formatSql.Replace($"__SHARDING_{masterTableSegment.ShardingId}_{origMasterName}", masterTableName);
                if (this.GetdShardingMapTableName(visitor, origMasterName, masterTableName, sql, tableShardings, out sql))
                {
                    if (builder.Length > 0) builder.Append(jointMark);
                    builder.Append(sql);
                }
            }
            if (tableShardings.Count > 0)
            {
                foreach (var tableSharding in tableShardings)
                {
                    tableSharding.Key.TableNames = tableSharding.Value;
                }
            }
        }
        else
        {
            var tableSegment = visitor.ShardingTables[0];
            var origName = tableSegment.Mapper.TableName;
            if (tableSegment.TableNames != null)
            {
                for (int i = 0; i < tableSegment.TableNames.Count; i++)
                {
                    if (i > 0) builder.Append(jointMark);
                    var tableName = tableSegment.TableNames[i];
                    var sql = formatSql.Replace($"__SHARDING_{tableSegment.ShardingId}_{origName}", tableName);
                    builder.Append(sql);
                }
            }
            else
            {
                var sql = formatSql.Replace($"__SHARDING_{tableSegment.ShardingId}_{origName}", tableSegment.Body);
                builder.Append(sql);
            }
        }
        var result = builder.ToString();
        builder.Clear();
        return result;
    }
    private bool GetdShardingMapTableName(SqlVisitor visitor, string origMasterName, string masterTableName, string formatSql, Dictionary<TableSegment, List<string>> tableShardingNames, out string sql)
    {
        sql = formatSql;
        for (int j = 1; j < visitor.ShardingTables.Count; j++)
        {
            var tableSegment = visitor.ShardingTables[j];
            var origName = tableSegment.Mapper.TableName;

            //如果主表分表名不存在，直接忽略本次关联
            var tableName = tableSegment.ShardingMapGetter.Invoke(origMasterName, origName, masterTableName);
            //主表存在分表，但从表不存在分表，直接忽略本次关联
            if (!tableSegment.TableNames.Exists(f => f == tableName))
                return false;
            sql = sql.Replace($"__SHARDING_{tableSegment.ShardingId}_{origName}", tableName);
            //1:N include表，需要统计一下表名，后续会用到
            if (visitor.IncludeTables != null && visitor.IncludeTables.Contains(tableSegment))
            {
                if (!tableShardingNames.TryGetValue(tableSegment, out var tableNames))
                    tableShardingNames.Add(tableSegment, tableNames = new List<string>());
                tableNames.Add(tableName);
            }
        }
        return true;
    }
    public string GetShardingTableBy(EntityMap entityMappper, object field1Value, object field2Value = null)
    {
        var entityType = entityMappper.EntityType;
        if (this.ShardingProvider == null || !this.ShardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            throw new Exception($"实体表{entityType.FullName}没有配置分表，无需调用此方法");
        if (shardingTable.DependOnMembers == null || shardingTable.DependOnMembers.Count == 0)
            throw new Exception($"实体表{entityType.FullName}没有配置分表依赖的字段");

        var origTableName = entityMappper.TableName;
        string tableName = null;
        if (field1Value == null)
            throw new ArgumentNullException($"实体{entityType.FullName}的分表规则依赖字段，字段值field1Value不能为null");

        if (shardingTable.DependOnMembers.Count > 1)
        {
            if (field2Value == null)
                throw new ArgumentNullException($"实体{entityType.FullName}的分表规则依赖2个字段，字段值field2Value不能为null");
            var shardingRule = shardingTable.Rule as Func<string, object, object, string>;
            tableName = shardingRule.Invoke(origTableName, field1Value, field2Value);
        }
        else
        {
            var shardingRule = shardingTable.Rule as Func<string, object, string>;
            tableName = shardingRule.Invoke(origTableName, field1Value);
        }
        return tableName;
    }
    #endregion
}
