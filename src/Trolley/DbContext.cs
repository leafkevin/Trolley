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
    public string DbKey { get; set; }
    public ITheaConnection Connection { get; set; }
    public TheaDatabase Database { get; set; }
    public string DefaultTableSchema { get; set; }
    public IOrmProvider OrmProvider { get; set; }
    public IEntityMapProvider MapProvider { get; set; }
    public ITableShardingProvider ShardingProvider { get; set; }
    public ITheaTransaction Transaction { get; set; }
    public bool IsParameterized { get; set; }
    public int CommandTimeout { get; set; }
    public string ParameterPrefix { get; set; } = "p";
    public Type DefaultEnumMapDbType { get; set; }
    public DbInterceptors DbInterceptors { get; set; }
    #endregion   

    #region UseMasterCommand
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
            var connString = this.Database.ConnectionString;
            connection = this.OrmProvider.CreateConnection(this.DbKey, connString);
            connection.OnOpening = this.DbInterceptors.OnConnectionOpening;
            connection.OnOpened = this.DbInterceptors.OnConnectionOpened;
            connection.OnClosing = this.DbInterceptors.OnConnectionClosing;
            connection.OnClosed = this.DbInterceptors.OnConnectionClosed;

            this.DbInterceptors.OnConnectionCreated?.Invoke(new ConectionEventArgs
            {
                DbKey = this.DbKey,
                ConnectionId = connection.ConnectionId,
                ConnectionString = connString,
                CreatedAt = DateTime.Now
            });
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
    #endregion

    #region UseSlaveCommand
    public (bool, ITheaConnection, ITheaCommand) UseSlaveCommand(bool isUseMaster)
        => this.UseSlaveCommand(isUseMaster, null);
    public (bool, ITheaConnection, ITheaCommand) UseSlaveCommand(bool isUseMaster, IDbCommand dbCommand)
    {
        bool isNeedClose = false;
        ITheaConnection connection;
        ITheaCommand command;
        if (this.Transaction != null)
            connection = this.Connection;
        else
        {
            isNeedClose = true;
            var connString = isUseMaster ? this.Database.ConnectionString : this.Database.UseSlave();
            connection = this.OrmProvider.CreateConnection(this.DbKey, connString);
            connection.OnOpening = this.DbInterceptors.OnConnectionOpening;
            connection.OnOpened = this.DbInterceptors.OnConnectionOpened;
            connection.OnClosing = this.DbInterceptors.OnConnectionClosing;
            connection.OnClosed = this.DbInterceptors.OnConnectionClosed;

            this.DbInterceptors.OnConnectionCreated?.Invoke(new ConectionEventArgs
            {
                DbKey = this.DbKey,
                ConnectionId = connection.ConnectionId,
                ConnectionString = connString,
                CreatedAt = DateTime.Now
            });
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
    #endregion

    #region QueryFirst
    public TResult QueryFirst<TResult>(Action<IDbCommand> commandInitializer)
    {
        TResult result = default;
        var entityType = typeof(TResult);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(false);
        commandInitializer.Invoke(command.BaseCommand);

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        if (reader.Read())
        {
            if (entityType.IsEntityType(out _))
                result = reader.To<TResult>(this.OrmProvider, this.MapProvider);
            else result = reader.To<TResult>(this.OrmProvider);
        }

        reader.Dispose();
        command.Parameters.Clear();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TResult> QueryFirstAsync<TResult>(Action<IDbCommand> commandInitializer, CancellationToken cancellationToken = default)
    {
        TResult result = default;
        var entityType = typeof(TResult);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(false);
        commandInitializer.Invoke(command.BaseCommand);

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            if (entityType.IsEntityType(out _))
                result = reader.To<TResult>(this.OrmProvider, this.MapProvider);
            else result = reader.To<TResult>(this.OrmProvider);
        }

        await reader.DisposeAsync();
        command.Parameters.Clear();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public TResult QueryFirst<TResult>(IQueryVisitor visitor)
    {
        TResult result = default;
        var entityType = typeof(TResult);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor.IsUseMaster);
        if (visitor.IsNeedFetchShardingTables)
            this.FetchShardingTables(visitor as SqlVisitor);

        Expression<Func<TResult, TResult>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        var sql = visitor.BuildSql(out var readerFields);
        sql = this.BuildSql(visitor, sql, " UNION ALL ");
        command.CommandText = sql;
        visitor.DbParameters.CopyTo(command.Parameters);

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        if (reader.Read())
        {
            if (entityType.IsEntityType(out _))
                result = reader.To<TResult>(this.OrmProvider, readerFields);
            else result = reader.To<TResult>(this.OrmProvider);
        }
        if (visitor.BuildIncludeSql(entityType, result, true, out sql))
        {
            reader.Dispose();
            command.CommandText = sql;
            command.Parameters.Clear();
            visitor.NextDbParameters.CopyTo(command.Parameters);
            reader = command.ExecuteReader(CommandSqlType.Select, CommandBehavior.SequentialAccess);
            visitor.SetIncludeValues(entityType, result, reader, true);
        }

        reader.Close();
        command.Parameters.Clear();
        command.Dispose();
        if (isNeedClose) connection.Close();
        visitor.Dispose();
        return result;
    }
    public async Task<TResult> QueryFirstAsync<TResult>(IQueryVisitor visitor, CancellationToken cancellationToken = default)
    {
        TResult result = default;
        var entityType = typeof(TResult);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor.IsUseMaster);
        if (visitor.IsNeedFetchShardingTables)
            await this.FetchShardingTablesAsync(visitor as SqlVisitor, cancellationToken);
        Expression<Func<TResult, TResult>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        var sql = visitor.BuildSql(out var readerFields);
        sql = this.BuildSql(visitor, sql, " UNION ALL ");
        command.CommandText = sql;
        visitor.DbParameters.CopyTo(command.Parameters);

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            if (entityType.IsEntityType(out _))
                result = reader.To<TResult>(this.OrmProvider, readerFields);
            else result = reader.To<TResult>(this.OrmProvider);
        }
        if (visitor.BuildIncludeSql(entityType, result, true, out sql))
        {
            await reader.DisposeAsync();
            command.CommandText = sql;
            command.Parameters.Clear();
            visitor.NextDbParameters.CopyTo(command.Parameters);
            reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
            await visitor.SetIncludeValuesAsync(entityType, result, reader, true, cancellationToken);
        }

        await reader.CloseAsync();
        command.Parameters.Clear();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        visitor.Dispose();
        return result;
    }
    #endregion

    #region Query
    public List<TResult> Query<TResult>(Action<IDbCommand> commandInitializer)
    {
        var result = new List<TResult>();
        var entityType = typeof(TResult);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(false);
        commandInitializer.Invoke(command.BaseCommand);

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        if (entityType.IsEntityType(out _))
        {
            while (reader.Read())
            {
                result.Add(reader.To<TResult>(this.OrmProvider, this.MapProvider));
            }
        }
        else
        {
            while (reader.Read())
            {
                result.Add(reader.To<TResult>(this.OrmProvider));
            }
        }

        reader.Dispose();
        command.Parameters.Clear();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<List<TResult>> QueryAsync<TResult>(Action<IDbCommand> commandInitializer, CancellationToken cancellationToken = default)
    {
        var result = new List<TResult>();
        var entityType = typeof(TResult);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(false);
        commandInitializer.Invoke(command.BaseCommand);

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        if (entityType.IsEntityType(out _))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<TResult>(this.OrmProvider, this.MapProvider));
            }
        }
        else
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<TResult>(this.OrmProvider));
            }
        }

        await reader.DisposeAsync();
        command.Parameters.Clear();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public List<TResult> Query<TResult>(IQueryVisitor visitor)
    {
        var result = new List<TResult>();
        var entityType = typeof(TResult);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor.IsUseMaster);
        if (visitor.IsNeedFetchShardingTables)
            this.FetchShardingTables(visitor as SqlVisitor);

        Expression<Func<TResult, TResult>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        var sql = visitor.BuildSql(out var readerFields);
        sql = this.BuildSql(visitor, sql, " UNION ALL ");
        command.CommandText = sql;
        visitor.DbParameters.CopyTo(command.Parameters);

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        if (entityType.IsEntityType(out _))
        {
            while (reader.Read())
            {
                result.Add(reader.To<TResult>(this.OrmProvider, readerFields));
            }
        }
        else
        {
            while (reader.Read())
            {
                result.Add(reader.To<TResult>(this.OrmProvider));
            }
        }
        if (visitor.BuildIncludeSql(entityType, result, false, out sql))
        {
            reader.Dispose();
            command.CommandText = sql;
            command.Parameters.Clear();
            visitor.NextDbParameters.CopyTo(command.Parameters);
            reader = command.ExecuteReader(CommandSqlType.Select, behavior);
            visitor.SetIncludeValues(entityType, result, reader, false);
        }

        reader.Dispose();
        command.Parameters.Clear();
        command.Dispose();
        if (isNeedClose) connection.Close();
        visitor.Dispose();
        return result;
    }
    public async Task<List<TResult>> QueryAsync<TResult>(IQueryVisitor visitor, CancellationToken cancellationToken = default)
    {
        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor.IsUseMaster);
        if (visitor.IsNeedFetchShardingTables)
            await this.FetchShardingTablesAsync(visitor as SqlVisitor, cancellationToken);

        Expression<Func<TResult, TResult>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        var sql = visitor.BuildSql(out var readerFields);
        sql = this.BuildSql(visitor, sql, " UNION ALL ");
        command.CommandText = sql;
        visitor.DbParameters.CopyTo(command.Parameters);

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);

        var entityType = typeof(TResult);
        if (entityType.IsEntityType(out _))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<TResult>(this.OrmProvider, readerFields));
            }
        }
        else
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<TResult>(this.OrmProvider));
            }
        }
        if (visitor.BuildIncludeSql(entityType, result, false, out sql))
        {
            await reader.DisposeAsync();
            command.CommandText = sql;
            command.Parameters.Clear();
            visitor.NextDbParameters.CopyTo(command.Parameters);
            reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
            await visitor.SetIncludeValuesAsync(entityType, result, reader, false, cancellationToken);
        }

        await reader.DisposeAsync();
        command.Parameters.Clear();
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
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor.IsUseMaster);
        Expression<Func<TResult, TResult>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        var sql = visitor.BuildSql(out var readerFields);
        sql = this.BuildSql(visitor, sql, " UNION ALL ");
        command.CommandText = sql;
        visitor.DbParameters.CopyTo(command.Parameters);

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        if (reader.Read()) result.TotalCount = reader.To<int>(this.OrmProvider);
        result.PageNumber = visitor.PageNumber;
        result.PageSize = visitor.PageSize;

        reader.NextResult();
        var entityType = typeof(TResult);
        if (entityType.IsEntityType(out _))
        {
            while (reader.Read())
            {
                result.Data.Add(reader.To<TResult>(this.OrmProvider, readerFields));
            }
        }
        else
        {
            while (reader.Read())
            {
                result.Data.Add(reader.To<TResult>(this.OrmProvider));
            }
        }
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
        command.Parameters.Clear();
        command.Dispose();
        if (isNeedClose) connection.Close();
        visitor.Dispose();
        return result;
    }
    public async Task<IPagedList<TResult>> QueryPageAsync<TResult>(IQueryVisitor visitor, CancellationToken cancellationToken = default)
    {
        var result = new PagedList<TResult> { Data = new List<TResult>() };
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor.IsUseMaster);

        Expression<Func<TResult, TResult>> defaultExpr = f => f;
        visitor.SelectDefault(defaultExpr);
        var sql = visitor.BuildSql(out var readerFields);
        sql = this.BuildSql(visitor, sql, " UNION ALL ");
        command.CommandText = sql;
        visitor.DbParameters.CopyTo(command.Parameters);

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
            result.TotalCount = reader.To<int>(this.OrmProvider);
        result.PageNumber = visitor.PageNumber;
        result.PageSize = visitor.PageSize;

        await reader.NextResultAsync(cancellationToken);
        var entityType = typeof(TResult);
        if (entityType.IsEntityType(out _))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Data.Add(reader.To<TResult>(this.OrmProvider, readerFields));
            }
        }
        else
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Data.Add(reader.To<TResult>(this.OrmProvider));
            }
        }
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
        command.Parameters.Clear();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        visitor.Dispose();
        return result;
    }
    #endregion

    #region Get
    public TEntity Get<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        TEntity result = default;
        var entityType = typeof(TEntity);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(false);
        var whereObjType = whereObj.GetType();
        var commandInitializer = RepositoryHelper.BuildGetSqlParameters(this.OrmProvider, this.MapProvider, entityType, whereObjType, false);
        var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
        command.CommandText = typedCommandInitializer.Invoke(command.BaseCommand.Parameters, this.OrmProvider, whereObj);

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        if (reader.Read())
            result = reader.To<TEntity>(this.OrmProvider, this.MapProvider);

        reader.Dispose();
        command.Parameters.Clear();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TEntity> GetAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        TEntity result = default;
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(false);
        var entityType = typeof(TEntity);
        var whereObjType = whereObj.GetType();
        var commandInitializer = RepositoryHelper.BuildGetSqlParameters(this.OrmProvider, this.MapProvider, entityType, whereObjType, false);
        var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
        command.CommandText = typedCommandInitializer.Invoke(command.BaseCommand.Parameters, this.OrmProvider, whereObj);

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
            result = reader.To<TEntity>(this.OrmProvider, this.MapProvider);

        await reader.DisposeAsync();
        command.Parameters.Clear();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region Create
    public TResult CreateResult<TResult>(Func<IDbCommand, DbContext, List<SqlFieldSegment>> commandInitializer)
    {
        TResult result = default;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        var readerFields = commandInitializer.Invoke(command.BaseCommand, this);
        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(CommandSqlType.Insert, behavior);
        if (reader.Read())
        {
            if (readerFields != null && readerFields.Count > 0)
                result = reader.To<TResult>(this.OrmProvider, readerFields, true);
            else result = reader.To<TResult>(this.OrmProvider);
        }

        reader.Dispose();
        command.Parameters.Clear();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TResult> CreateResultAsync<TResult>(Func<IDbCommand, DbContext, List<SqlFieldSegment>> commandInitializer, CancellationToken cancellationToken = default)
    {
        TResult result = default;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        var readerFields = commandInitializer.Invoke(command.BaseCommand, this);
        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Insert, behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            if (readerFields != null && readerFields.Count > 0)
                result = reader.To<TResult>(this.OrmProvider, readerFields, true);
            else result = reader.To<TResult>(this.OrmProvider);
        }

        await reader.DisposeAsync();
        command.Parameters.Clear();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public List<TResult> CreateResult<TResult>(ICreateVisitor visitor)
    {
        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        command.CommandText = visitor.BuildCommand(command.BaseCommand, false, out var readerFields);
        connection.Open();
        using var reader = command.ExecuteReader(CommandSqlType.Insert, CommandBehavior.SequentialAccess);
        while (reader.Read())
        {
            result.Add(reader.To<TResult>(this.OrmProvider, readerFields, true));
        }
        reader.Dispose();
        command.Parameters.Clear();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<List<TResult>> CreateResultAsync<TResult>(ICreateVisitor visitor, CancellationToken cancellationToken = default)
    {
        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        command.CommandText = visitor.BuildCommand(command.BaseCommand, false, out var readerFields);
        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Insert, CommandBehavior.SequentialAccess, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(reader.To<TResult>(this.OrmProvider, readerFields, true));
        }
        await reader.DisposeAsync();
        command.Parameters.Clear();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        visitor.Dispose();
        return result;
    }
    public void BuildCreateCommand(IDbCommand command, Type entityType, object insertObj, bool isReturnIdentity)
    {
        var insertObjType = insertObj.GetType();
        var fieldsSqlPartSetter = RepositoryHelper.BuildCreateFieldsSqlPart(this.OrmProvider, this.MapProvider, entityType, insertObjType, null, null);
        var valuesSqlPartSetter = RepositoryHelper.BuildCreateValuesSqlParametes(this.OrmProvider, this.MapProvider, entityType, insertObjType, null, null, false);
        bool isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(insertObjType);

        Action<IDataParameterCollection, StringBuilder, string> firstSqlSetter = null;
        Action<IDataParameterCollection, StringBuilder, object> loopSqlSetter = null;

        var entityMapper = this.MapProvider.GetEntityMap(entityType);
        var tableName = entityMapper.TableName;
        if (isDictionary)
        {
            var typedFieldsSqlPartSetter = fieldsSqlPartSetter as Func<StringBuilder, object, List<MemberMap>>;
            var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, List<MemberMap>, object>;

            var builder = new StringBuilder();
            var memberMappers = typedFieldsSqlPartSetter.Invoke(builder, insertObj);
            builder.Append(") VALUES ");
            var firstHeadSql = builder.ToString();
            builder.Clear();

            firstSqlSetter = (dbParameters, builder, tableName) =>
            {
                builder.Append($"INSERT INTO {this.OrmProvider.GetTableName(tableName)} (");
                builder.Append(firstHeadSql);
            };
            loopSqlSetter = (dbParameters, builder, insertObj) =>
            {
                builder.Append('(');
                typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.OrmProvider, memberMappers, insertObj);
                builder.Append(')');
            };
        }
        else
        {
            var typedFieldsSqlPartSetter = fieldsSqlPartSetter as Action<StringBuilder>;
            var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object>;

            firstSqlSetter = (dbParameters, builder, tableName) =>
            {
                builder.Append($"INSERT INTO {this.OrmProvider.GetTableName(tableName)} (");
                typedFieldsSqlPartSetter.Invoke(builder);
                builder.Append(") VALUES ");
            };
            loopSqlSetter = (dbParameters, builder, insertObj) =>
            {
                builder.Append('(');
                typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.OrmProvider, insertObj);
                builder.Append(')');
            };
        }
        var sqlBuilder = new StringBuilder();
        if (this.ShardingProvider != null && this.ShardingProvider.TryGetTableSharding(entityType, out _))
            tableName = this.GetShardingTableName(entityType, insertObjType, insertObj);
        firstSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName);
        loopSqlSetter.Invoke(command.Parameters, sqlBuilder, insertObj);
        if (isReturnIdentity)
        {
            var keyField = entityMapper.KeyMembers[0].FieldName;
            keyField = this.OrmProvider.GetFieldName(keyField);
            sqlBuilder.Append(this.OrmProvider.GetIdentitySql(keyField));
        }
        command.CommandText = sqlBuilder.ToString();
    }
    #endregion

    #region Execute
    public int Execute(Func<IDbCommand, bool> commandInitializer)
    {
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        if (!commandInitializer.Invoke(command.BaseCommand))
            connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.RawExecute);
        command.Parameters.Clear();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<int> ExecuteAsync(Func<IDbCommand, bool> commandInitializer, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        if (!commandInitializer.Invoke(command.BaseCommand))
            await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.RawExecute, cancellationToken);
        command.Parameters.Clear();
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
        this.Connection ??= this.OrmProvider.CreateConnection(this.DbKey, this.Database.ConnectionString);
        this.Connection.Open();
        this.Transaction = this.Connection.BeginTransaction();
    }
    public async ValueTask BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (this.Transaction != null)
            throw new Exception("上一个事务还没有完成，无法开启新事务");
        this.Connection = this.OrmProvider.CreateConnection(this.DbKey, this.Database.ConnectionString);
        await this.Connection.OpenAsync(cancellationToken);
        this.Transaction = await this.Connection.BeginTransactionAsync(IsolationLevel.Unspecified, cancellationToken);
    }
    public void Commit()
    {
        this.Transaction?.Commit();
        this.Connection.Close();
        this.Transaction = null;
        this.Connection = null;
    }
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (this.Transaction != null)
            await this.Transaction.CommitAsync(cancellationToken);
        await this.Connection.CloseAsync();
        this.Transaction = null;
        this.Connection = null;
    }
    public void Rollback()
    {
        this.Transaction?.Rollback();
        this.Connection.Close();
        this.Transaction = null;
        this.Connection = null;
    }
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (this.Transaction != null)
            await this.Transaction.RollbackAsync(null, cancellationToken);
        await this.Connection.CloseAsync();
        this.Transaction = null;
        this.Connection = null;
    }
    #endregion

    public string BuildSql(IQueryVisitor visitor, string formatSql, string jointMark)
    {
        var sql = formatSql;
        if (visitor.ShardingTables != null && visitor.ShardingTables.Count > 0)
            sql = this.BuildShardingTablesSqlByFormat(visitor as SqlVisitor, formatSql, jointMark);
        return sql;
    }
    public void FetchShardingTables(SqlVisitor visitor)
    {
        var fetchSql = visitor.BuildTableShardingsSql();
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor.IsUseMaster);
        command.CommandText = fetchSql;
        connection.Open();
        using var reader = command.ExecuteReader(CommandSqlType.Select, CommandBehavior.SequentialAccess);
        var shardingTables = new List<string>();
        while (reader.Read())
        {
            shardingTables.Add(reader.To<string>(this.OrmProvider));
        }
        visitor.SetShardingTables(shardingTables);

        reader.Dispose();
        command.Parameters.Clear();
        command.Dispose();
        if (isNeedClose) connection.Close();
    }
    public async Task FetchShardingTablesAsync(SqlVisitor visitor, CancellationToken cancellationToken = default)
    {
        var fetchSql = visitor.BuildTableShardingsSql();
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand(visitor.IsUseMaster);
        command.CommandText = fetchSql;
        command.Parameters.Clear();
        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, CommandBehavior.SequentialAccess, cancellationToken);
        var shardingTables = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            shardingTables.Add(reader.To<string>(this.OrmProvider));
        }
        visitor.SetShardingTables(shardingTables);

        await reader.DisposeAsync();
        command.Parameters.Clear();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
    }
    public Dictionary<string, List<object>> SplitShardingParameters(Type entityType, IEnumerable parameters)
        => RepositoryHelper.SplitShardingParameters(this.MapProvider, this.ShardingProvider, entityType, parameters);
    public string GetShardingTableName(Type entityType, Type parameterType, object parameter)
        => RepositoryHelper.GetShardingTableName(this.MapProvider, this.ShardingProvider, entityType, parameterType, parameter);
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
                if (builder.Length > 0) builder.Append(jointMark);
                var masterTableName = masterTableSegment.TableNames[i];
                var sql = formatSql.Replace($"__SHARDING_{masterTableSegment.ShardingId}_{origMasterName}", masterTableName);

                if (this.GetdShardingMapTableName(visitor, origMasterName, masterTableName, sql, tableShardings, out sql))
                    builder.Append(sql);
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
            //TOTO:此处需要记录日志
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
}