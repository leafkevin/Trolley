using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

/// <summary>
/// INSERT INTO table1 (column1, column2, column3, ...)
/// SELECT column1, column2, column3, ...
/// FROM table2,table3
/// WHERE condition;
/// </summary>
/// <typeparam name="TEntity"></typeparam>
class Create<TEntity> : ICreate<TEntity>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly bool isParameterized;

    public Create(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.isParameterized = isParameterized;
    }
    public ICreated<TEntity> RawSql(string rawSql, object parameters)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        return new Created<TEntity>(this.connection, this.transaction, this.ormProvider, this.mapProvider).RawSql(rawSql, parameters);
    }
    public IContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        if (insertObj is IEnumerable && insertObj is not string)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithByBulk方法");

        return new ContinuedCreate<TEntity>(this.connection, this.transaction, this.ormProvider, this.mapProvider).WithBy(insertObj);
    }
    public ICreated<TEntity> WithBulkBy(IEnumerable insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        return new Created<TEntity>(this.connection, this.transaction, this.ormProvider, this.mapProvider).WithByBulk(insertObjs, bulkCount);
    }
    public IContinuedCreate<TEntity, TSource> From<TSource>(Expression<Func<TSource, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = this.ormProvider.NewCreateVisitor(this.connection.DbKey, this.mapProvider, entityType, this.isParameterized).From(fieldSelector);
        return new ContinuedCreate<TEntity, TSource>(this.connection, this.transaction, visitor);
    }
    public IContinuedCreate<TEntity, T1, T2> From<T1, T2>(Expression<Func<T1, T2, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);

        var visitor = this.ormProvider.NewCreateVisitor(this.connection.DbKey, this.mapProvider, entityType, this.isParameterized).From(fieldSelector);
        return new ContinuedCreate<TEntity, T1, T2>(this.connection, this.transaction, visitor);
    }
    public IContinuedCreate<TEntity, T1, T2, T3> From<T1, T2, T3>(Expression<Func<T1, T2, T3, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = this.ormProvider.NewCreateVisitor(this.connection.DbKey, this.mapProvider, entityType, this.isParameterized).From(fieldSelector);
        return new ContinuedCreate<TEntity, T1, T2, T3>(this.connection, this.transaction, visitor);
    }
    public IContinuedCreate<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = this.ormProvider.NewCreateVisitor(this.connection.DbKey, this.mapProvider, entityType, this.isParameterized).From(fieldSelector);
        return new ContinuedCreate<TEntity, T1, T2, T3, T4>(this.connection, this.transaction, visitor);
    }
    public IContinuedCreate<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = this.ormProvider.NewCreateVisitor(this.connection.DbKey, this.mapProvider, entityType, this.isParameterized).From(fieldSelector);
        return new ContinuedCreate<TEntity, T1, T2, T3, T4, T5>(this.connection, this.transaction, visitor);
    }
}
class ContinuedCreate<TEntity> : IContinuedCreate<TEntity>
{
    private static ConcurrentDictionary<int, object> builderCache = new();
    private readonly List<BuilderCache> builders = new();
    private readonly TheaConnection connection;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly IDbTransaction transaction;

    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
    }
    public IContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        var parameterType = insertObj.GetType();
        if (insertObj is IEnumerable && insertObj is not string)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithByBulk方法");
        if (parameterType == typeof(Dictionary<string, object>))
        {
            this.builders.Add(new BuilderCache
            {
                CommandInitializer = this.BuildCommandInitializer(),
                Parameters = insertObj
            });
        }
        else
        {
            if (!parameterType.IsEntityType())
                throw new NotSupportedException("只支持对象，不支持基础类型");

            this.builders.Add(new BuilderCache
            {
                CommandInitializer = this.BuildCommandInitializer(this.builders.Count, parameterType),
                Parameters = insertObj
            });
        }
        return this;
    }
    public IContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
    {
        if (condition)
        {
            if (insertObj == null)
                throw new ArgumentNullException(nameof(insertObj));

            return this.WithBy<TInsertObject>(insertObj);
        }
        return this;
    }
    public int Execute()
    {
        int result = 0;
        var entityType = typeof(TEntity);
        var entityMapper = this.mapProvider.GetEntityMap(entityType);
        using var command = this.connection.CreateCommand();
        var insertBuilder = new StringBuilder($"INSERT INTO {this.ormProvider.GetTableName(entityMapper.TableName)} (");
        var valuesBuilder = new StringBuilder(" VALUES(");
        int index = 0;
        foreach (var builder in this.builders)
        {
            if (index > 0)
            {
                insertBuilder.Append(',');
                valuesBuilder.Append(',');
            }
            builder.CommandInitializer.Invoke(command, this.ormProvider, builder.Parameters, insertBuilder, valuesBuilder);
            index++;
        }
        insertBuilder.Append(')');
        valuesBuilder.Append(')');

        if (entityMapper.IsAutoIncrement)
            valuesBuilder.AppendFormat(this.ormProvider.SelectIdentitySql, entityMapper.AutoIncrementField);
        var sql = insertBuilder.ToString() + valuesBuilder.ToString();

        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;
        this.connection.Open();

        if (entityMapper.IsAutoIncrement)
        {
            using var reader = command.ExecuteReader();
            if (reader.Read()) result = reader.To<int>();
            reader.Close();
            reader.Dispose();
            command.Dispose();
            return result;
        }
        result = command.ExecuteNonQuery();
        command.Dispose();
        this.builders.Clear();
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        var entityType = typeof(TEntity);
        var entityMapper = this.mapProvider.GetEntityMap(entityType);
        using var cmd = this.connection.CreateCommand();
        var insertBuilder = new StringBuilder($"INSERT INTO {this.ormProvider.GetTableName(entityMapper.TableName)} (");
        var valuesBuilder = new StringBuilder(" VALUES(");
        int index = 0;
        foreach (var builder in this.builders)
        {
            if (index > 0)
            {
                insertBuilder.Append(',');
                valuesBuilder.Append(',');
            }
            builder.CommandInitializer.Invoke(cmd, this.ormProvider, builder.Parameters, insertBuilder, valuesBuilder);
            index++;
        }
        insertBuilder.Append(')');
        valuesBuilder.Append(')');

        if (entityMapper.IsAutoIncrement)
            valuesBuilder.AppendFormat(this.ormProvider.SelectIdentitySql, entityMapper.AutoIncrementField);
        var sql = insertBuilder.ToString() + valuesBuilder.ToString();

        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        if (entityMapper.IsAutoIncrement)
        {
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync())
                result = reader.To<int>();
            await reader.CloseAsync();
            await reader.DisposeAsync();
            await command.DisposeAsync();
            return result;
        }
        result = await command.ExecuteNonQueryAsync(cancellationToken);
        await command.DisposeAsync();
        this.builders.Clear();
        return result;
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        var entityType = typeof(TEntity);
        var entityMapper = this.mapProvider.GetEntityMap(entityType);
        using var command = this.connection.CreateCommand();
        var insertBuilder = new StringBuilder($"INSERT INTO {this.ormProvider.GetTableName(entityMapper.TableName)} (");
        var valuesBuilder = new StringBuilder(" VALUES(");
        int index = 0;
        foreach (var builder in this.builders)
        {
            if (index > 0)
            {
                insertBuilder.Append(',');
                valuesBuilder.Append(',');
            }
            builder.CommandInitializer.Invoke(command, this.ormProvider, builder.Parameters, insertBuilder, valuesBuilder);
            index++;
        }
        insertBuilder.Append(')');
        valuesBuilder.Append(')');

        if (entityMapper.IsAutoIncrement)
            valuesBuilder.AppendFormat(this.ormProvider.SelectIdentitySql, entityMapper.AutoIncrementField);
        var sql = insertBuilder.ToString() + valuesBuilder.ToString();

        dbParameters = null;
        if (command.Parameters != null && command.Parameters.Count > 0)
            dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Cancel();
        command.Dispose();
        this.builders.Clear();
        return sql;
    }
    private Action<IDbCommand, IOrmProvider, object, StringBuilder, StringBuilder> BuildCommandInitializer(int index, Type parameterType)
    {
        var cacheKey = HashCode.Combine(this.connection, this.ormProvider, index, parameterType);
        if (!builderCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
        {
            int columnIndex = 0;
            var entityType = typeof(TEntity);
            var entityMapper = this.mapProvider.GetEntityMap(entityType);
            var parameterMapper = this.mapProvider.GetEntityMap(parameterType);
            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var parameterExpr = Expression.Parameter(typeof(object), "parameter");
            var insertBuilderExpr = Expression.Parameter(typeof(StringBuilder), "insertBuilder");
            var valueBuilderExpr = Expression.Parameter(typeof(StringBuilder), "valueBuilder");

            var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            var localParameters = new Dictionary<string, int>();
            blockParameters.Add(typedParameterExpr);
            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

            var insertBuilder = new StringBuilder();
            var valuesBuilder = new StringBuilder();
            var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
            var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            foreach (var parameterMemberMapper in parameterMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(parameterMemberMapper.MemberName, out var propMapper)
                    || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;

                if (columnIndex > 0)
                {
                    blockBodies.Add(Expression.Call(insertBuilderExpr, methodInfo1, Expression.Constant(',')));
                    blockBodies.Add(Expression.Call(valueBuilderExpr, methodInfo1, Expression.Constant(',')));
                }
                var fieldName = this.ormProvider.GetFieldName(propMapper.FieldName);
                var parameterName = this.ormProvider.ParameterPrefix + propMapper.MemberName;
                var parameterNameExpr = Expression.Constant(parameterName);
                blockBodies.Add(Expression.Call(insertBuilderExpr, methodInfo2, Expression.Constant(fieldName)));
                blockBodies.Add(Expression.Call(valueBuilderExpr, methodInfo2, parameterNameExpr));

                RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, true, propMapper, this.ormProvider, localParameters, blockParameters, blockBodies);
                columnIndex++;
            }

            commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object, StringBuilder, StringBuilder>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr, insertBuilderExpr, valueBuilderExpr).Compile();
            builderCache.TryAdd(cacheKey, commandInitializerDelegate);
        }
        return (Action<IDbCommand, IOrmProvider, object, StringBuilder, StringBuilder>)commandInitializerDelegate;
    }
    private Action<IDbCommand, IOrmProvider, object, StringBuilder, StringBuilder> BuildCommandInitializer()
    {
        return (command, ormProvider, parameter, insertBuilder, valuesBuilder) =>
        {
            int index = 0;
            var dict = parameter as Dictionary<string, object>;
            var entityType = typeof(TEntity);
            var entityMapper = this.mapProvider.GetEntityMap(entityType);
            foreach (var item in dict)
            {
                if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                    || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;

                if (index > 0)
                {
                    insertBuilder.Append(',');
                    valuesBuilder.Append(',');
                }
                var parameterName = ormProvider.ParameterPrefix + item.Key;
                insertBuilder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                valuesBuilder.Append(parameterName);

                if (propMapper.NativeDbType != null)
                    command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                index++;
            }
        };
    }
    struct BuilderCache
    {
        public Action<IDbCommand, IOrmProvider, object, StringBuilder, StringBuilder> CommandInitializer { get; set; }
        public object Parameters { get; set; }
    }
}
class Created<TEntity> : ICreated<TEntity>
{
    private static ConcurrentDictionary<int, object> commandInitializerCache = new();
    private readonly List<IDbDataParameter> dbParameters = new();
    private readonly TheaConnection connection;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly IDbTransaction transaction;
    private string rawSql = null;
    private object parameters = null;
    private int? bulkCount;

    public Created(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
    }
    public ICreated<TEntity> RawSql(string rawSql, object parameters)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        this.rawSql = rawSql;
        this.parameters = parameters;
        return this;
    }
    public ICreated<TEntity> WithByBulk(IEnumerable insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));
        this.parameters = insertObjs;
        this.bulkCount = bulkCount;
        return this;
    }
    public int Execute()
    {
        var entityType = typeof(TEntity);
        Type parameterType = null;
        if (!string.IsNullOrEmpty(this.rawSql))
        {
            if (this.parameters != null)
                parameterType = parameters.GetType();

            Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
            if (this.parameters is Dictionary<string, object>)
                commandInitializer = this.BuildCommandInitializer(entityType);
            else commandInitializer = this.BuildCommandInitializer(entityType, parameterType);

            int result = 0;
            using var command = this.connection.CreateCommand();
            var sql = commandInitializer.Invoke(command, this.ormProvider, this.parameters);

            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            command.Transaction = this.transaction;
            this.connection.Open();
            var entityMapper = this.mapProvider.GetEntityMap(entityType);
            if (entityMapper.IsAutoIncrement)
            {
                using var reader = command.ExecuteReader();
                if (reader.Read()) result = reader.To<int>();
                reader.Close();
                reader.Dispose();
                command.Dispose();
                return result;
            }
            result = command.ExecuteNonQuery();
            command.Dispose();
            return result;
        }
        else
        {
            bool isDictionary = false;
            var entities = this.parameters as IEnumerable;
            foreach (var entity in entities)
            {
                if (entity is Dictionary<string, object>)
                    isDictionary = true;
                else parameterType = entity.GetType();
                break;
            }
            Action<IDbCommand, IOrmProvider, StringBuilder, int, object> commandInitializer = null;
            if (isDictionary)
                commandInitializer = this.BuildBatchCommandInitializer(entityType);
            else commandInitializer = this.BuildBatchCommandInitializer(entityType, parameterType);

            this.bulkCount ??= 500;
            int result = 0, index = 0;
            var sqlBuilder = new StringBuilder();
            using var command = this.connection.CreateCommand();
            command.Transaction = this.transaction;
            foreach (var entity in entities)
            {
                commandInitializer.Invoke(command, this.ormProvider, sqlBuilder, index, entity);
                if (index >= this.bulkCount)
                {
                    command.CommandText = sqlBuilder.ToString();
                    command.CommandType = CommandType.Text;
                    this.connection.Open();
                    result += command.ExecuteNonQuery();
                    command.Parameters.Clear();
                    sqlBuilder.Clear();
                    index = 0;
                    continue;
                }
                index++;
            }
            if (index > 0)
            {
                command.CommandText = sqlBuilder.ToString();
                command.CommandType = CommandType.Text;
                this.connection.Open();
                result += command.ExecuteNonQuery();
            }
            command.Dispose();
            return result;
        }
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        Type parameterType = null;
        if (!string.IsNullOrEmpty(this.rawSql))
        {
            if (this.parameters != null)
                parameterType = parameters.GetType();

            Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
            if (this.parameters is Dictionary<string, object>)
                commandInitializer = this.BuildCommandInitializer(entityType);
            else commandInitializer = this.BuildCommandInitializer(entityType, parameterType);

            int result = 0;
            using var cmd = this.connection.CreateCommand();
            var sql = commandInitializer.Invoke(cmd, this.ormProvider, this.parameters);

            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = this.transaction;
            if (cmd is not DbCommand command)
                throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

            await this.connection.OpenAsync(cancellationToken);
            var entityMapper = this.mapProvider.GetEntityMap(entityType);
            if (entityMapper.IsAutoIncrement)
            {
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync())
                    result = reader.To<int>();
                await reader.CloseAsync();
                await reader.DisposeAsync();
                await command.DisposeAsync();
                return result;
            }
            result = await command.ExecuteNonQueryAsync(cancellationToken);
            await command.DisposeAsync();
            return result;
        }
        else
        {
            bool isDictionary = false;
            var entities = this.parameters as IEnumerable;
            foreach (var entity in entities)
            {
                if (entity is Dictionary<string, object>)
                    isDictionary = true;
                else parameterType = entity.GetType();
                break;
            }
            Action<IDbCommand, IOrmProvider, StringBuilder, int, object> commandInitializer = null;
            if (isDictionary)
                commandInitializer = this.BuildBatchCommandInitializer(entityType);
            else commandInitializer = this.BuildBatchCommandInitializer(entityType, parameterType);

            this.bulkCount ??= 500;
            int result = 0, index = 0;
            var sqlBuilder = new StringBuilder();
            using var cmd = this.connection.CreateCommand();
            cmd.Transaction = this.transaction;
            if (cmd is not DbCommand command)
                throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

            foreach (var entity in entities)
            {
                commandInitializer.Invoke(command, this.ormProvider, sqlBuilder, index, entity);
                if (index >= this.bulkCount)
                {
                    command.CommandText = sqlBuilder.ToString();
                    command.CommandType = CommandType.Text;
                    await this.connection.OpenAsync(cancellationToken);
                    result += await command.ExecuteNonQueryAsync(cancellationToken);
                    command.Parameters.Clear();
                    sqlBuilder.Clear();
                    index = 0;
                    continue;
                }
                index++;
            }
            if (index > 0)
            {
                command.CommandText = sqlBuilder.ToString();
                command.CommandType = CommandType.Text;
                await this.connection.OpenAsync(cancellationToken);
                result += await command.ExecuteNonQueryAsync(cancellationToken);
            }
            await command.DisposeAsync();
            return result;
        }
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        bool isMulti = false;
        bool isDictionary = false;
        var entityType = typeof(TEntity);
        Type parameterType = null;
        IEnumerable entities = null;
        if (this.parameters is Dictionary<string, object> dict)
            isDictionary = true;
        else if (this.parameters is IEnumerable && parameterType != typeof(string))
        {
            isMulti = true;
            entities = this.parameters as IEnumerable;
            foreach (var entity in entities)
            {
                if (entity is Dictionary<string, object>)
                    isDictionary = true;
                else parameterType = entity.GetType();
                break;
            }
        }
        else parameterType = this.parameters.GetType();

        if (isMulti)
        {
            Action<IDbCommand, IOrmProvider, StringBuilder, int, object> commandInitializer = null;
            if (isDictionary)
                commandInitializer = this.BuildBatchCommandInitializer(entityType);
            else commandInitializer = this.BuildBatchCommandInitializer(entityType, parameterType);
            this.bulkCount ??= 500;
            int index = 0;
            var sqlBuilder = new StringBuilder();
            using var command = this.connection.CreateCommand();
            foreach (var entity in entities)
            {
                commandInitializer.Invoke(command, this.ormProvider, sqlBuilder, index, entity);
                if (index >= this.bulkCount)
                    break;
                index++;
            }
            string sql = null;
            if (index > 0)
                sql = sqlBuilder.ToString();
            if (command.Parameters != null && command.Parameters.Count > 0)
                dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
            command.Cancel();
            command.Dispose();
            return sql;
        }
        else
        {
            Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
            if (isDictionary)
                commandInitializer = this.BuildCommandInitializer(entityType);
            else commandInitializer = this.BuildCommandInitializer(entityType, parameterType);
            using var command = this.connection.CreateCommand();
            var sql = commandInitializer?.Invoke(command, this.ormProvider, this.parameters);
            if (command.Parameters != null && command.Parameters.Count > 0)
                dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
            command.Cancel();
            command.Dispose();
            return sql;
        }
    }
    private Action<IDbCommand, IOrmProvider, StringBuilder, int, object> BuildBatchCommandInitializer(Type entityType, Type parameterType)
    {
        var cacheKey = HashCode.Combine("CreateBatch", this.connection, string.Empty, entityType, parameterType);
        if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
        {
            int columnIndex = 0;
            var entityMapper = this.mapProvider.GetEntityMap(entityType);
            var parameterMapper = this.mapProvider.GetEntityMap(parameterType);
            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
            var indexExpr = Expression.Parameter(typeof(int), "index");
            var parameterExpr = Expression.Parameter(typeof(object), "parameter");

            var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            var localParameters = new Dictionary<string, int>();
            blockParameters.Add(typedParameterExpr);
            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

            var insertBuilder = new StringBuilder($"INSERT INTO {this.ormProvider.GetTableName(entityMapper.TableName)} (");
            foreach (var parameterMemberMapper in parameterMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(parameterMemberMapper.MemberName, out var propMapper)
                    || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;

                if (columnIndex > 0)
                    insertBuilder.Append(',');
                insertBuilder.Append(this.ormProvider.GetFieldName(propMapper.FieldName));
                columnIndex++;
            }
            insertBuilder.Append(") VALUES ");

            var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
            var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var methodInfo3 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });

            //添加INSERT INTO xxx() VALUES
            var addInsertExpr = Expression.Call(builderExpr, methodInfo2, Expression.Constant(insertBuilder.ToString()));
            var addCommaExpr = Expression.Call(builderExpr, methodInfo1, Expression.Constant(','));
            var greatThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0, typeof(int)));
            blockBodies.Add(Expression.IfThenElse(greatThenExpr, addCommaExpr, addInsertExpr));
            blockBodies.Add(Expression.Call(builderExpr, methodInfo1, Expression.Constant('(')));
            columnIndex = 0;
            foreach (var parameterMemberMapper in parameterMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(parameterMemberMapper.MemberName, out var propMapper)
                    || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;

                if (columnIndex > 0)
                    blockBodies.Add(Expression.Call(builderExpr, methodInfo1, Expression.Constant(',')));

                var parameterName = this.ormProvider.ParameterPrefix + propMapper.MemberName;
                var suffixExpr = Expression.Call(indexExpr, typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes));
                var parameterNameExpr = Expression.Call(methodInfo3, Expression.Constant(parameterName), suffixExpr);
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, parameterNameExpr));

                RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, true, propMapper, this.ormProvider, localParameters, blockParameters, blockBodies);
                columnIndex++;
            }
            blockBodies.Add(Expression.Call(builderExpr, methodInfo1, Expression.Constant(')')));

            commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, StringBuilder, int, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, builderExpr, indexExpr, parameterExpr).Compile();
            commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
        }
        return (Action<IDbCommand, IOrmProvider, StringBuilder, int, object>)commandInitializerDelegate;
    }
    private Func<IDbCommand, IOrmProvider, object, string> BuildCommandInitializer(Type entityType, Type parameterType)
    {
        var cacheKey = HashCode.Combine("Create", this.connection, string.Empty, entityType, parameterType);
        if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
        {
            int columnIndex = 0;
            var entityMapper = this.mapProvider.GetEntityMap(entityType);
            var parameterMapper = this.mapProvider.GetEntityMap(parameterType);
            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var parameterExpr = Expression.Parameter(typeof(object), "parameter");

            var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            var localParameters = new Dictionary<string, int>();
            blockParameters.Add(typedParameterExpr);
            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

            var insertBuilder = new StringBuilder($"INSERT INTO {this.ormProvider.GetTableName(entityMapper.TableName)} (");
            var valuesBuilder = new StringBuilder(" VALUES(");
            foreach (var parameterMemberMapper in parameterMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(parameterMemberMapper.MemberName, out var propMapper)
                    || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;

                if (columnIndex > 0)
                {
                    insertBuilder.Append(',');
                    valuesBuilder.Append(',');
                }
                insertBuilder.Append(this.ormProvider.GetFieldName(propMapper.FieldName));
                var parameterName = this.ormProvider.ParameterPrefix + propMapper.MemberName;
                valuesBuilder.Append(parameterName);

                var parameterNameExpr = Expression.Constant(parameterName);
                RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, true, propMapper, this.ormProvider, localParameters, blockParameters, blockBodies);
                columnIndex++;
            }
            insertBuilder.Append(')');
            valuesBuilder.Append(')');

            if (entityMapper.IsAutoIncrement)
                valuesBuilder.AppendFormat(this.ormProvider.SelectIdentitySql, entityMapper.AutoIncrementField);
            var sql = insertBuilder.ToString() + valuesBuilder.ToString();

            var returnExpr = Expression.Constant(sql, typeof(string));

            var resultLabelExpr = Expression.Label(typeof(string));
            blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
            blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));

            commandInitializerDelegate = Expression.Lambda<Func<IDbCommand, IOrmProvider, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
            commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
        }
        return (Func<IDbCommand, IOrmProvider, object, string>)commandInitializerDelegate;
    }
    private Action<IDbCommand, IOrmProvider, object> BuildCommandInitializer(string sql, Type entityType, Type parameterType)
    {
        var cacheKey = HashCode.Combine("Create", this.connection, sql, entityType, parameterType);
        if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
        {
            var parameterMapper = this.mapProvider.GetEntityMap(parameterType);
            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var parameterExpr = Expression.Parameter(typeof(object), "parameter");

            var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            var localParameters = new Dictionary<string, int>();
            blockParameters.Add(typedParameterExpr);
            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

            foreach (var parameterMemberMapper in parameterMapper.MemberMaps)
            {
                var parameterName = this.ormProvider.ParameterPrefix + parameterMemberMapper.MemberName;
                if (!Regex.IsMatch(sql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                    continue;

                var parameterNameExpr = Expression.Constant(parameterName);
                //明确的SQL，传入的参数有可能为NULL,为NULL则插入NULL值
                RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, true, parameterMemberMapper, this.ormProvider, localParameters, blockParameters, blockBodies);
            }
            commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
            commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
        }
        return (Action<IDbCommand, IOrmProvider, object>)commandInitializerDelegate;
    }
    private Action<IDbCommand, IOrmProvider, StringBuilder, int, object> BuildBatchCommandInitializer(Type entityType)
    {
        return (command, ormProvider, builder, index, parameter) =>
        {
            int columnIndex = 0;
            var dict = parameter as Dictionary<string, object>;
            var entityMapper = this.mapProvider.GetEntityMap(entityType);
            if (index == 0)
            {
                builder.Append($"INSERT INTO {ormProvider.GetTableName(entityMapper.TableName)} (");
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;
                    if (columnIndex > 0)
                        builder.Append(',');
                    builder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                    columnIndex++;
                }
                builder.Append(") VALUES ");
            }
            else builder.Append(',');
            columnIndex = 0;
            builder.Append('(');
            foreach (var item in dict)
            {
                if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                    || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;

                if (columnIndex > 0)
                    builder.Append(',');
                var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName + index.ToString();
                builder.Append(parameterName);

                if (propMapper.NativeDbType != null)
                    command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                columnIndex++;
            }
            builder.Append(')');
        };
    }
    private Func<IDbCommand, IOrmProvider, object, string> BuildCommandInitializer(Type entityType)
    {
        return (command, ormProvider, parameter) =>
        {
            int index = 0;
            var dict = parameter as Dictionary<string, object>;
            var entityMapper = this.mapProvider.GetEntityMap(entityType);
            var insertBuilder = new StringBuilder($"INSERT INTO {ormProvider.GetTableName(entityMapper.TableName)} (");
            var valuesBuilder = new StringBuilder(" VALUES(");
            foreach (var item in dict)
            {
                if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                    || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;

                if (index > 0)
                {
                    insertBuilder.Append(',');
                    valuesBuilder.Append(',');
                }
                var parameterName = ormProvider.ParameterPrefix + item.Key;
                insertBuilder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                valuesBuilder.Append(parameterName);

                if (propMapper.NativeDbType != null)
                    command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                index++;
            }
            insertBuilder.Append(')');
            valuesBuilder.Append(')');
            if (entityMapper.IsAutoIncrement)
                valuesBuilder.AppendFormat(ormProvider.SelectIdentitySql, entityMapper.AutoIncrementField);
            return insertBuilder.ToString() + valuesBuilder.ToString();
        };
    }
    private Action<IDbCommand, IOrmProvider, object> BuildCommandInitializer(string sql)
    {
        return (command, ormProvider, parameter) =>
        {
            var dict = parameter as Dictionary<string, object>;
            foreach (var item in dict)
            {
                var parameterName = ormProvider.ParameterPrefix + item.Key;
                if (!Regex.IsMatch(sql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                    continue;
                var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                command.Parameters.Add(dbParameter);
            }
        };
    }
}
class ContinuedCreateBase
{
    protected readonly TheaConnection connection;
    protected readonly IDbTransaction transaction;
    protected readonly ICreateVisitor visitor;

    public ContinuedCreateBase(TheaConnection connection, IDbTransaction transaction, ICreateVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public int Execute()
    {
        var sql = this.visitor.BuildSql(out var dbParameters);
        using var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));

        this.connection.Open();
        var result = command.ExecuteNonQuery();
        command.Dispose();
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var sql = this.visitor.BuildSql(out var dbParameters);
        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));

        await this.connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        await command.DisposeAsync();
        return result;
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters);
}
class ContinuedCreate<TEntity, T1> : ContinuedCreateBase, IContinuedCreate<TEntity, T1>
{
    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, ICreateVisitor visitor)
        : base(connection, transaction, visitor) { }
    public IContinuedCreate<TEntity, T1> Where(Expression<Func<T1, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1> And(bool condition, Expression<Func<T1, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.And(predicate);
        return this;
    }
}
class ContinuedCreate<TEntity, T1, T2> : ContinuedCreateBase, IContinuedCreate<TEntity, T1, T2>
{
    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, ICreateVisitor visitor)
        : base(connection, transaction, visitor) { }
    public IContinuedCreate<TEntity, T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.And(predicate);
        return this;
    }
}
class ContinuedCreate<TEntity, T1, T2, T3> : ContinuedCreateBase, IContinuedCreate<TEntity, T1, T2, T3>
{
    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, ICreateVisitor visitor)
        : base(connection, transaction, visitor) { }
    public IContinuedCreate<TEntity, T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.And(predicate);
        return this;
    }
}
class ContinuedCreate<TEntity, T1, T2, T3, T4> : ContinuedCreateBase, IContinuedCreate<TEntity, T1, T2, T3, T4>
{
    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, ICreateVisitor visitor)
        : base(connection, transaction, visitor) { }
    public IContinuedCreate<TEntity, T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.And(predicate);
        return this;
    }
}
class ContinuedCreate<TEntity, T1, T2, T3, T4, T5> : ContinuedCreateBase, IContinuedCreate<TEntity, T1, T2, T3, T4, T5>
{
    public ContinuedCreate(TheaConnection connection, IDbTransaction transaction, ICreateVisitor visitor)
        : base(connection, transaction, visitor) { }
    public IContinuedCreate<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IContinuedCreate<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.And(predicate);
        return this;
    }
}
