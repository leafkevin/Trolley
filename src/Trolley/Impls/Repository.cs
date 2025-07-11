using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class Repository : IRepository
{
    #region Properties
    public DbContext DbContext { get; set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    public IEntityMapProvider MapProvider => this.DbContext.MapProvider;
    public ITableShardingProvider ShardingProvider => this.DbContext.ShardingProvider;
    public bool IsParameterized => this.DbContext.IsConstantParameterized;
    #endregion

    #region Constructor
    public Repository(DbContext dbContext) => this.DbContext = dbContext;
    #endregion

    #region ShardingDatabase
    public IRepository UseMaster()
    {
        this.DbContext.ConnectionString = this.DbContext.Database.UseMaster();
        return this;
    }
    public IRepository UseMasterBy(params object[] fieldValues)
    {
        this.DbContext.ConnectionString = this.DbContext.Database.UseMasterBy(fieldValues);
        return this;
    }
    public IRepository UseSlaveBy(params object[] fieldValues)
    {
        this.DbContext.ConnectionString = this.DbContext.Database.UseSlaveBy(fieldValues);
        return this;
    }
    #endregion

    #region ShardingTable
    public virtual List<string> GetShardingTableNames<TEntity>(Func<string, bool> tableNameSelector, string tableSchema = null) => null;
    public virtual Task<List<string>> GetShardingTableNamesAsync<TEntity>(Func<string, bool> tableNameSelector, string tableSchema = null, CancellationToken cancellationToken = default) => null;
    public virtual void CreateShardingTable<TEntity>(string tableName, string tableSchema = null, string fromTableSchema = null) { }
    public virtual Task CreateShardingTableAsync<TEntity>(string tableName, string tableSchema = null, string fromTableSchema = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public virtual string GetShardingTableNameBy<TEntity>(object field1Value, object field2Value = null)
    {
        var entityMapper = this.MapProvider.GetEntityMap(typeof(TEntity));
        return this.DbContext.GetShardingTableBy(entityMapper, field1Value, field2Value);
    }
    public virtual void CreateShardingTableBy<TEntity>(object field1Value, object field2Value = null, string tableSchema = null, string fromTableSchema = null)
    {
        var entityMapper = this.MapProvider.GetEntityMap(typeof(TEntity));
        var tableName = this.DbContext.GetShardingTableBy(entityMapper, field1Value, field2Value);
        this.CreateShardingTable<TEntity>(tableName, tableSchema, fromTableSchema);
    }
    public virtual async Task CreateShardingTableByAsync<TEntity>(object field1Value, object field2Value = null, string tableSchema = null, string fromTableSchema = null, CancellationToken cancellationToken = default)
    {
        var entityMapper = this.MapProvider.GetEntityMap(typeof(TEntity));
        var tableName = this.DbContext.GetShardingTableBy(entityMapper, field1Value, field2Value);
        await this.CreateShardingTableAsync<TEntity>(tableName, tableSchema, fromTableSchema, cancellationToken);
    }
    #endregion  

    #region WithOptions
    public IRepository WithTimeout(int seconds)
    {
        this.DbContext.CommandTimeout = seconds;
        return this;
    }
    #endregion

    #region From
    public virtual IQuery<T> From<T>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T));
        return this.OrmProvider.NewQuery<T>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2));
        return this.OrmProvider.NewQuery<T1, T2>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3));
        return this.OrmProvider.NewQuery<T1, T2, T3>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this.DbContext, visitor);
    }
    #endregion

    #region From SubQuery
    public virtual IQuery<T> From<T>(IQuery<T> subQuery)
    {
        var visitor = this.CreateQueryVisitor();
        visitor.From(typeof(T), subQuery);
        return this.OrmProvider.NewQuery<T>(this.DbContext, visitor);
    }
    public virtual IQuery<T> From<T>(Func<IFromQuery, IQuery<T>> subQuery)
    {
        var visitor = this.CreateQueryVisitor();
        visitor.From(typeof(T), this.DbContext, subQuery);
        return this.OrmProvider.NewQuery<T>(this.DbContext, visitor);
    }
    #endregion

    #region GetById
    public virtual TEntity GetById<TEntity>(object whereKey)
    {
        return this.DbContext.QueryById<TEntity, TEntity>(whereKey, true, reader =>
        {
            TEntity result = default;
            var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this.DbContext);
            if (reader.Read())
                result = (TEntity)deserializer.Invoke(reader);
            return result;
        });
    }
    public virtual async Task<TEntity> GetByIdAsync<TEntity>(object whereKey, CancellationToken cancellationToken = default)
    {
        return await this.DbContext.QueryByIdAsync<TEntity, TEntity>(whereKey, true, async (reader, cancellationToken) =>
        {
            TEntity result = default;
            var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this.DbContext);
            if (await reader.ReadAsync(cancellationToken))
                result = (TEntity)deserializer.Invoke(reader);
            return result;
        }, cancellationToken);
    }
    #endregion

    #region GetByIds
    public virtual List<TEntity> GetByIds<TEntity>(IEnumerable whereKeys)
    {
        return this.DbContext.QueryById<TEntity, List<TEntity>>(whereKeys, false, reader =>
        {
            var result = new List<TEntity>();
            var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this.DbContext);
            while (reader.Read())
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        });
    }
    public virtual async Task<List<TEntity>> GetByIdsAsync<TEntity>(IEnumerable whereKeys, CancellationToken cancellationToken = default)
    {
        return await this.DbContext.QueryByIdAsync<TEntity, List<TEntity>>(whereKeys, false, async (reader, cancellationToken) =>
        {
            var result = new List<TEntity>();
            var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this.DbContext);
            while (await reader.ReadAsync(cancellationToken))
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        }, cancellationToken);
    }
    #endregion

    #region QueryScalar
    public virtual TValue QueryScalar<TValue>(string rawSql, object parameters = null, CommandType commandType = CommandType.Text)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters != null)
        {
            var whereObjType = parameters.GetType();
            if (!whereObjType.IsEntityType(out _))
                throw new NotSupportedException("不支持的参数类型，QueryFirst方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");
        }

        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand();
        if (parameters != null)
        {
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
            commandInitializer.Invoke(command.Parameters, this.OrmProvider, parameters);
        }
        command.CommandText = rawSql;
        command.CommandType = commandType;

        connection.Open();
        TValue result = default;
        var objResult = command.ExecuteScalar(CommandSqlType.Select);
        if (objResult != null && objResult is not DBNull)
            result = (TValue)Convert.ChangeType(objResult, typeof(TValue));

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<TValue> QueryScalarAsync<TValue>(string rawSql, object parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters != null)
        {
            var whereObjType = parameters.GetType();
            if (!whereObjType.IsEntityType(out _))
                throw new NotSupportedException("不支持的参数类型，QueryFirstAsync方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");
        }

        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand();
        if (parameters != null)
        {
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
            commandInitializer.Invoke(command.Parameters, this.OrmProvider, parameters);
        }
        command.CommandText = rawSql;
        command.CommandType = commandType;

        await connection.OpenAsync(cancellationToken);
        TValue result = default;
        var objResult = await command.ExecuteScalarAsync(CommandSqlType.Select, cancellationToken);
        if (objResult != null && objResult is not DBNull)
            result = (TValue)Convert.ChangeType(objResult, typeof(TValue));

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public virtual TValue QueryScalar<TValue>(CommandType commandType, string rawSql, params DbParameter[] parameters)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters != null)
        {
            var whereObjType = parameters.GetType();
            if (!whereObjType.IsEntityType(out _))
                throw new NotSupportedException("不支持的参数类型，QueryFirst方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");
        }

        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand();
        command.CommandText = rawSql;
        command.CommandType = commandType;
        if (parameters != null)
            Array.ForEach(parameters, f => command.Parameters.Add(f));

        connection.Open();
        TValue result = default;
        var objResult = command.ExecuteScalar(CommandSqlType.Select);
        if (objResult != null && objResult is not DBNull)
            result = (TValue)Convert.ChangeType(objResult, typeof(TValue));

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<TValue> QueryScalarAsync<TValue>(CommandType commandType, string rawSql, DbParameter[] parameters = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters != null)
        {
            var whereObjType = parameters.GetType();
            if (!whereObjType.IsEntityType(out _))
                throw new NotSupportedException("不支持的参数类型，QueryFirstAsync方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");
        }

        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand();
        command.CommandText = rawSql;
        command.CommandType = commandType;
        if (parameters != null)
            Array.ForEach(parameters, f => command.Parameters.Add(f));

        await connection.OpenAsync(cancellationToken);
        TValue result = default;
        var objResult = await command.ExecuteScalarAsync(CommandSqlType.Select, cancellationToken);
        if (objResult != null && objResult is not DBNull)
            result = (TValue)Convert.ChangeType(objResult, typeof(TValue));

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region QueryFirst
    public virtual TEntity QueryFirst<TEntity>(string rawSql, object parameters = null, CommandType commandType = CommandType.Text)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters != null)
        {
            var whereObjType = parameters.GetType();
            if (!whereObjType.IsEntityType(out _))
                throw new NotSupportedException("不支持的参数类型，QueryFirst方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");
        }

        var entityType = typeof(TEntity);
        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand();
        if (parameters != null)
        {
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
            commandInitializer.Invoke(command.Parameters, this.OrmProvider, parameters);
        }
        command.CommandText = rawSql;
        command.CommandType = commandType;

        connection.Open();
        TEntity result = default;

        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this.DbContext);
        if (reader.Read())
            result = (TEntity)deserializer.Invoke(reader);
        reader.Dispose();

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, object parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters != null)
        {
            var whereObjType = parameters.GetType();
            if (!whereObjType.IsEntityType(out _))
                throw new NotSupportedException("不支持的参数类型，QueryFirstAsync方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");
        }

        var entityType = typeof(TEntity);
        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand();
        if (parameters != null)
        {
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
            commandInitializer.Invoke(command.Parameters, this.OrmProvider, parameters);
        }
        command.CommandText = rawSql;
        command.CommandType = commandType;

        await connection.OpenAsync(cancellationToken);
        TEntity result = default;

        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this.DbContext);
        if (await reader.ReadAsync(cancellationToken))
            result = (TEntity)deserializer.Invoke(reader);
        await reader.DisposeAsync();

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public virtual TEntity QueryFirst<TEntity>(CommandType commandType, string rawSql, params DbParameter[] parameters)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        var entityType = typeof(TEntity);
        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand();
        command.CommandText = rawSql;
        command.CommandType = commandType;
        if (parameters != null)
            Array.ForEach(parameters, f => command.Parameters.Add(f));

        connection.Open();
        TEntity result = default;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this.DbContext);
        if (reader.Read())
            result = (TEntity)deserializer.Invoke(reader);
        reader.Dispose();

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<TEntity> QueryFirstAsync<TEntity>(CommandType commandType, string rawSql, DbParameter[] parameters = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        var entityType = typeof(TEntity);
        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand();
        command.CommandText = rawSql;
        command.CommandType = commandType;
        if (parameters != null)
            Array.ForEach(parameters, f => command.Parameters.Add(f));

        await connection.OpenAsync(cancellationToken);
        TEntity result = default;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this.DbContext);
        if (await reader.ReadAsync(cancellationToken))
            result = (TEntity)deserializer.Invoke(reader);
        await reader.DisposeAsync();

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }

    public virtual TEntity QueryFirst<TEntity>(object whereObj)
    {
        return this.DbContext.Query<TEntity, TEntity>(whereObj, true, reader =>
        {
            var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this.DbContext);
            if (reader.Read())
                return (TEntity)deserializer.Invoke(reader);
            return default;
        });
    }
    public virtual async Task<TEntity> QueryFirstAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        return await this.DbContext.QueryAsync<TEntity, TEntity>(whereObj, true, async (reader, cancellationToken) =>
        {
            var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this.DbContext);
            if (await reader.ReadAsync(cancellationToken))
                return (TEntity)deserializer.Invoke(reader);
            return default;
        }, cancellationToken);
    }
    #endregion

    #region Query
    public virtual List<TEntity> Query<TEntity>(string rawSql, object parameters = null, CommandType commandType = CommandType.Text)
        => this.DbContext.Query<TEntity>(rawSql, parameters, CommandType.Text);
    public virtual async Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, object parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryAsync<TEntity>(rawSql, parameters, commandType, cancellationToken);
    public virtual List<TEntity> Query<TEntity>(CommandType commandType, string rawSql, params DbParameter[] parameters)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand();
        command.CommandText = rawSql;
        command.CommandType = commandType;
        if (parameters != null)
            Array.ForEach(parameters, f => command.Parameters.Add(f));

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(CommandSqlType.Select, behavior);
        var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this.DbContext);
        var result = new List<TEntity>();
        while (reader.Read())
            result.Add((TEntity)deserializer.Invoke(reader));

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<List<TEntity>> QueryAsync<TEntity>(CommandType commandType, string rawSql, DbParameter[] parameters = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand();
        command.CommandText = rawSql;
        command.CommandType = commandType;
        if (parameters != null)
            Array.ForEach(parameters, f => command.Parameters.Add(f));

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Select, behavior, cancellationToken);
        var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this.DbContext);
        var result = new List<TEntity>();
        while (await reader.ReadAsync(cancellationToken))
            result.Add((TEntity)deserializer.Invoke(reader));
        await reader.DisposeAsync();

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public virtual List<TEntity> Query<TEntity>(object whereObj)
    {
        return this.DbContext.Query<TEntity, List<TEntity>>(whereObj, false, reader =>
        {
            var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this.DbContext);
            var result = new List<TEntity>();
            while (reader.Read())
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        });
    }
    public virtual async Task<List<TEntity>> QueryAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        return await this.DbContext.QueryAsync<TEntity, List<TEntity>>(whereObj, false, async (reader, cancellationToken) =>
        {
            var deserializer = reader.GetReaderDeserializer(typeof(TEntity), this.DbContext);
            var result = new List<TEntity>();
            while (await reader.ReadAsync(cancellationToken))
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        }, cancellationToken);
    }
    #endregion

    #region Create
    public virtual ICreate<TEntity> Create<TEntity>() => this.OrmProvider.NewCreate<TEntity>(this.DbContext);
    public virtual int Create<TEntity>(object insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        bool isBulk = insertObjs is IEnumerable && insertObjs is not string && insertObjs is not IDictionary<string, object>;
        var entityType = typeof(TEntity);
        if (isBulk)
        {
            var entities = insertObjs as IEnumerable;
            var commandExecutor = RepositoryHelper.BuildCreateBulkCommandExecutor(this.DbContext, entityType, entities);
            connection.Open();
            result = commandExecutor.Invoke(this.DbContext, command, entities, bulkCount);
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildCreateCommandInitializer(this.DbContext, entityType, insertObjs, false);
            commandInitializer.Invoke(this.DbContext, command, insertObjs);
            connection.Open();
            result = command.ExecuteNonQuery(CommandSqlType.Insert);
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<int> CreateAsync<TEntity>(object insertObjs, int bulkCount = 500, CancellationToken cancellationToken = default)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        bool isBulk = insertObjs is IEnumerable && insertObjs is not string && insertObjs is not IDictionary<string, object>;
        var entityType = typeof(TEntity);
        if (isBulk)
        {
            var entities = insertObjs as IEnumerable;
            var commandExecutor = RepositoryHelper.BuildCreateBulkAsyncCommandExecutor(this.DbContext, entityType, entities);
            await connection.OpenAsync(cancellationToken);
            result = await commandExecutor.Invoke(this.DbContext, command, entities, bulkCount, cancellationToken);
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildCreateCommandInitializer(this.DbContext, entityType, insertObjs, false);
            commandInitializer.Invoke(this.DbContext, command, insertObjs);
            await connection.OpenAsync(cancellationToken);
            result = await command.ExecuteNonQueryAsync(CommandSqlType.Insert, cancellationToken);
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public virtual int CreateIdentity<TEntity>(object insertObj) => this.DbContext.CreateIdentity<TEntity, int>(insertObj);
    public virtual async Task<int> CreateIdentityAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default)
        => await this.DbContext.CreateIdentityAsync<TEntity, int>(insertObj, cancellationToken);
    public virtual long CreateIdentityLong<TEntity>(object insertObj) => this.DbContext.CreateIdentity<TEntity, long>(insertObj);
    public virtual async Task<long> CreateIdentityLongAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default)
        => await this.DbContext.CreateIdentityAsync<TEntity, long>(insertObj, cancellationToken);
    #endregion

    #region Update
    public virtual IUpdate<TEntity> Update<TEntity>() => this.OrmProvider.NewUpdate<TEntity>(this.DbContext);
    public virtual int Update<TEntity>(object updateObjs, int bulkCount = 500)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));

        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        bool isBulk = updateObjs is IEnumerable && updateObjs is not string && updateObjs is not IDictionary<string, object>;
        var entityType = typeof(TEntity);

        if (isBulk)
        {
            int index = 0;
            var entities = updateObjs as IEnumerable;
            Type updateObjType = null;
            foreach (var updateObj in entities)
            {
                updateObjType = updateObj.GetType();
                break;
            }

            var commandInitializer = RepositoryHelper.BuildUpdateCommandInitializer(this.DbContext, entityType, updateObjType, true, false);
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            var builder = new StringBuilder();

            connection.Open();
            foreach (var updateObj in entities)
            {
                if (index > 0) builder.Append(';');
                typedCommandInitializer.Invoke(command.Parameters, builder, this.DbContext, updateObj, index.ToString());
                index++;

                if (index >= bulkCount)
                {
                    command.CommandText = builder.ToString();
                    result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                    command.Parameters.Clear();
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
        else
        {
            var updateObjType = updateObjs.GetType();
            var commandInitializer = RepositoryHelper.BuildUpdateCommandInitializer(this.DbContext, entityType, updateObjType, false, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
            command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this.DbContext, updateObjs);
            connection.Open();
            result = command.ExecuteNonQuery(CommandSqlType.Update);
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<int> UpdateAsync<TEntity>(object updateObjs, int bulkCount = 500, CancellationToken cancellationToken = default)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));

        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        bool isBulk = updateObjs is IEnumerable && updateObjs is not string && updateObjs is not IDictionary<string, object>;
        var entityType = typeof(TEntity);

        if (isBulk)
        {
            int index = 0;
            var entities = updateObjs as IEnumerable;
            Type updateObjType = null;
            foreach (var updateObj in entities)
            {
                updateObjType = updateObj.GetType();
                break;
            }
            var commandInitializer = RepositoryHelper.BuildUpdateCommandInitializer(this.DbContext, entityType, updateObjType, true, false);
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            var builder = new StringBuilder();

            await connection.OpenAsync(cancellationToken);
            foreach (var updateObj in entities)
            {
                if (index > 0) builder.Append(';');
                typedCommandInitializer.Invoke(command.Parameters, builder, this.DbContext, updateObj, index.ToString());
                index++;

                if (index >= bulkCount)
                {
                    command.CommandText = builder.ToString();
                    result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                    command.Parameters.Clear();
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
        else
        {
            var updateObjType = updateObjs.GetType();
            var commandInitializer = RepositoryHelper.BuildUpdateCommandInitializer(this.DbContext, entityType, updateObjType, false, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
            command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this.DbContext, updateObjs);

            await connection.OpenAsync(cancellationToken);
            result = await command.ExecuteNonQueryAsync(CommandSqlType.Update, cancellationToken);
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region Delete
    public virtual IDelete<TEntity> Delete<TEntity>() => this.OrmProvider.NewDelete<TEntity>(this.DbContext);
    public virtual int Delete<TEntity>(object whereKeys)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));

        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var entityType = typeof(TEntity);
        this.BuildDeleteCommand(command, entityType, whereKeys);
        connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.Delete);

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<int> DeleteAsync<TEntity>(object whereKeys, CancellationToken cancellationToken = default)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));

        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var entityType = typeof(TEntity);
        this.BuildDeleteCommand(command, entityType, whereKeys);
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.Delete, cancellationToken);

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    private void BuildDeleteCommand(ITheaCommand command, Type entityType, object whereKeys)
    {
        Type whereObjType = null;
        var isBulk = whereKeys is IEnumerable && whereKeys is not string && whereKeys is not IDictionary<string, object>;
        var entities = whereKeys as IEnumerable;
        if (isBulk)
        {
            foreach (var entity in entities)
            {
                whereObjType = entity.GetType();
                break;
            }
        }
        else whereObjType = whereKeys.GetType();
        (var isMultiKeys, var tableName, var headSqlSetter, var whereSqlParametersSetter) = RepositoryHelper.BuildDeleteCommandInitializer(this.DbContext, entityType, whereObjType, false, isBulk);

        int index = 0;
        var builder = new StringBuilder();
        var whereSqlBuilder = new StringBuilder();
        if (isBulk)
        {
            var jointMark = isMultiKeys ? " OR " : ",";
            var typedWhereSqlParametersSetter = whereSqlParametersSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;

            foreach (var entity in entities)
            {
                if (index > 0) whereSqlBuilder.Append(jointMark);
                typedWhereSqlParametersSetter.Invoke(command.Parameters, whereSqlBuilder, this.DbContext, entity, $"{index}");
                index++;
            }
            if (!isMultiKeys) whereSqlBuilder.Append(')');
        }
        else
        {
            var typedWhereSqlParametersSetter = whereSqlParametersSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object>;
            typedWhereSqlParametersSetter.Invoke(command.Parameters, whereSqlBuilder, this.DbContext, whereKeys);
        }
        headSqlSetter.Invoke(builder, tableName);
        builder.Append(whereSqlBuilder);
        command.CommandText = builder.ToString();
        builder.Clear();
        whereSqlBuilder.Clear();
    }
    #endregion

    #region Exists
    public virtual bool Exists<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        bool isBulk = whereObj is IEnumerable && whereObj is not string && whereObj is not IDictionary<string, object>;
        var whereObjType = whereObj.GetType();

        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand();
        if (isBulk)
        {
            (var isInExpr, var headSql, var commandInitializer) = ((bool, string, object))RepositoryHelper.BuildExistsSqlParameters(this.DbContext, entityType, whereObjType, whereObj, false, isBulk);
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            var parameters = whereObj as IEnumerable;
            int index = 0;
            var builder = new StringBuilder(headSql);
            var jointMark = isInExpr ? "," : " OR ";
            foreach (var parameter in parameters)
            {
                if (index > 0) builder.Append(jointMark);
                typedCommandInitializer.Invoke(command.Parameters, builder, this.DbContext, parameter, index.ToString());
                index++;
            }
            if (isInExpr) builder.Append(')');
            command.CommandText = builder.ToString();
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildExistsSqlParameters(this.DbContext, entityType, whereObjType, whereObj, false, isBulk);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
            command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this.DbContext, whereObj);
        }

        int result = 0;
        connection.Open();
        var objResult = command.ExecuteScalar(CommandSqlType.Select);
        if (objResult != null && objResult is not DBNull)
            result = (int)Convert.ChangeType(objResult, typeof(int));

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result > 0;
    }
    public virtual async Task<bool> ExistsAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var entityType = typeof(TEntity);
        bool isBulk = whereObj is IEnumerable && whereObj is not string && whereObj is not IDictionary<string, object>;
        var whereObjType = whereObj.GetType();

        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand();
        if (isBulk)
        {
            (var isInExpr, var headSql, var commandInitializer) = ((bool, string, object))RepositoryHelper.BuildExistsSqlParameters(this.DbContext, entityType, whereObjType, whereObj, false, isBulk);
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            var parameters = whereObj as IEnumerable;
            int index = 0;
            var builder = new StringBuilder(headSql);
            var jointMark = isInExpr ? "," : " OR ";
            foreach (var parameter in parameters)
            {
                if (index > 0) builder.Append(jointMark);
                typedCommandInitializer.Invoke(command.Parameters, builder, this.DbContext, parameter, index.ToString());
                index++;
            }
            if (isInExpr) builder.Append(')');
            command.CommandText = builder.ToString();
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildExistsSqlParameters(this.DbContext, entityType, whereObjType, whereObj, false, isBulk);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
            command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this.DbContext, whereObj);
        }
        int result = 0;
        await connection.OpenAsync(cancellationToken);
        var objResult = await command.ExecuteScalarAsync(CommandSqlType.Select, cancellationToken);
        if (objResult != null && objResult is not DBNull)
            result = (int)Convert.ChangeType(objResult, typeof(int));

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result > 0;
    }
    public virtual bool Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate = null)
    {
        if (wherePredicate != null)
            return this.From<TEntity>().Where(wherePredicate).Count() > 0;
        return this.From<TEntity>().Count() > 0;
    }
    public virtual async Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
    {
        if (wherePredicate != null)
            return await this.From<TEntity>().Where(wherePredicate).CountAsync(cancellationToken) > 0;
        return await this.From<TEntity>().CountAsync(cancellationToken) > 0;
    }
    #endregion

    #region Execute
    public virtual int Execute(string rawSql, object parameters = null, CommandType commandType = CommandType.Text)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        command.CommandText = rawSql;
        command.CommandType = commandType;
        if (parameters != null)
        {
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
            commandInitializer.Invoke(command.Parameters, this.OrmProvider, parameters);
        }
        connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.RawExecute);
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<int> ExecuteAsync(string rawSql, object parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        command.CommandText = rawSql;
        command.CommandType = commandType;
        if (parameters != null)
        {
            var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
            commandInitializer.Invoke(command.Parameters, this.OrmProvider, parameters);
        }
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.RawExecute, cancellationToken);
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public virtual int Execute(CommandType commandType, string rawSql, params DbParameter[] parameters)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        command.CommandText = rawSql;
        command.CommandType = commandType;
        if (parameters != null)
            Array.ForEach(parameters, f => command.Parameters.Add(f));
        connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.RawExecute);
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<int> ExecuteAsync(CommandType commandType, string rawSql, DbParameter[] parameters = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        command.CommandText = rawSql;
        command.CommandType = commandType;
        if (parameters != null)
            Array.ForEach(parameters, f => command.Parameters.Add(f));
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.RawExecute, cancellationToken);
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region QueryMultiple
    public virtual IMultiQueryReader QueryMultiple(Action<IMultipleQuery> subQueries)
    {
        if (subQueries == null)
            throw new ArgumentNullException(nameof(subQueries));

        using var multiQuery = this.OrmProvider.NewMultipleQuery(this.DbContext);
        subQueries.Invoke(multiQuery);
        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand(multiQuery.Command);
        multiQuery.Command.Connection = connection.BaseConnection;
        command.CommandText = multiQuery.BuildSql(out var readerAfters);
        connection.Open();
        var reader = command.ExecuteReader(CommandSqlType.MultiQuery, CommandBehavior.SequentialAccess);
        //多语句查询，在最后reader读取后，自动关闭
        return new MultiQueryReader(this.DbContext, connection, command, reader, readerAfters, isNeedClose);
    }
    public virtual async Task<IMultiQueryReader> QueryMultipleAsync(Action<IMultipleQuery> subQueries, CancellationToken cancellationToken = default)
    {
        if (subQueries == null)
            throw new ArgumentNullException(nameof(subQueries));

        using var multiQuery = this.OrmProvider.NewMultipleQuery(this.DbContext);
        subQueries.Invoke(multiQuery);
        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand(multiQuery.Command);
        multiQuery.Command.Connection = connection.BaseConnection;
        command.CommandText = multiQuery.BuildSql(out var readerAfters);
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(CommandSqlType.MultiQuery, CommandBehavior.SequentialAccess, cancellationToken);
        //多语句查询，在最后reader读取后，自动关闭
        return new MultiQueryReader(this.DbContext, connection, command, reader, readerAfters, isNeedClose);
    }
    #endregion

    #region MultipleExecute
    public virtual int MultipleExecute(List<MultipleCommand> commands)
    {
        if (commands == null || commands.Count == 0)
            throw new ArgumentNullException(nameof(commands));

        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        int commandIndex = 0;
        var sqlBuilder = new StringBuilder();
        var visitors = new Dictionary<MultipleCommandType, object>();
        foreach (var multiCcommand in commands)
        {
            bool isFirst = false;
            if (!visitors.TryGetValue(multiCcommand.CommandType, out var visitor))
            {
                visitor = multiCcommand.CommandType switch
                {
                    MultipleCommandType.Insert => this.OrmProvider.NewCreateVisitor(this.DbContext),
                    MultipleCommandType.Update => this.OrmProvider.NewUpdateVisitor(this.DbContext),
                    MultipleCommandType.Delete => this.OrmProvider.NewDeleteVisitor(this.DbContext),
                    _ => this.OrmProvider.NewUpdateVisitor(this.DbContext)
                };
                visitors.Add(multiCcommand.CommandType, visitor);
                isFirst = true;
            }
            switch (multiCcommand.CommandType)
            {
                case MultipleCommandType.Insert:
                    var insertVisitor = visitor as ICreateVisitor;
                    insertVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                    insertVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                    break;
                case MultipleCommandType.Update:
                    var updateVisitor = visitor as IUpdateVisitor;
                    updateVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                    updateVisitor.BuildMultiCommand(this.DbContext, command, sqlBuilder, multiCcommand, commandIndex);
                    break;
                case MultipleCommandType.Delete:
                    var deleteVisitor = visitor as IDeleteVisitor;
                    deleteVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                    deleteVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                    break;
            }
            commandIndex++;
        }
        command.CommandText = sqlBuilder.ToString();
        connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.MultiCommand);

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<int> MultipleExecuteAsync(List<MultipleCommand> commands, CancellationToken cancellationToken = default)
    {
        if (commands == null || commands.Count == 0)
            throw new ArgumentNullException(nameof(commands));

        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        int commandIndex = 0;
        var sqlBuilder = new StringBuilder();
        var visitors = new Dictionary<MultipleCommandType, object>();
        foreach (var multiCcommand in commands)
        {
            bool isFirst = false;
            if (!visitors.TryGetValue(multiCcommand.CommandType, out var visitor))
            {
                visitor = multiCcommand.CommandType switch
                {
                    MultipleCommandType.Insert => this.OrmProvider.NewCreateVisitor(this.DbContext),
                    MultipleCommandType.Update => this.OrmProvider.NewUpdateVisitor(this.DbContext),
                    MultipleCommandType.Delete => this.OrmProvider.NewDeleteVisitor(this.DbContext),
                    _ => this.OrmProvider.NewUpdateVisitor(this.DbContext)
                };
                visitors.Add(multiCcommand.CommandType, visitor);
                isFirst = true;
            }
            switch (multiCcommand.CommandType)
            {
                case MultipleCommandType.Insert:
                    var insertVisitor = visitor as ICreateVisitor;
                    insertVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                    insertVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                    break;
                case MultipleCommandType.Update:
                    var updateVisitor = visitor as IUpdateVisitor;
                    updateVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                    updateVisitor.BuildMultiCommand(this.DbContext, command, sqlBuilder, multiCcommand, commandIndex);
                    break;
                case MultipleCommandType.Delete:
                    var deleteVisitor = visitor as IDeleteVisitor;
                    deleteVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                    deleteVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                    break;
            }
            commandIndex++;
        }
        command.CommandText = sqlBuilder.ToString();
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.MultiCommand, cancellationToken);

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region Others 
    public virtual void Close() => this.DbContext.Connection.Close();
    public virtual async Task CloseAsync() => await this.DbContext.Connection.CloseAsync();
    public virtual void BeginTransaction() => this.DbContext.BeginTransaction();
    public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.BeginTransactionAsync(cancellationToken);
    public virtual void Commit() => this.DbContext.Commit();
    public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.CommitAsync(cancellationToken);
    public virtual void Rollback() => this.DbContext.Rollback();
    public virtual async Task RollbackAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.RollbackAsync(cancellationToken);
    //抛异常的时候，会走到析构函数，但是Transaction，没有提交也没有回滚
    private IQueryVisitor CreateQueryVisitor(char tableAsStart = 'a')
        => this.OrmProvider.NewQueryVisitor(this.DbContext, tableAsStart);
    #endregion
}