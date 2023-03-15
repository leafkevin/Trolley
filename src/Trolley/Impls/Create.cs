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

    public Create(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider)
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

        return new Created<TEntity>(this.connection, this.transaction, this.ormProvider, this.mapProvider, rawSql, parameters);
    }
    public ICreated<TEntity> WithBy<TInsertObject>(TInsertObject insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        return new Created<TEntity>(this.connection, this.transaction, this.ormProvider, this.mapProvider, null, insertObjs, bulkCount);
    }
    public ICreate<TEntity, TSource> From<TSource>(Expression<Func<TSource, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = new CreateVisitor(this.connection.DbKey, this.ormProvider, this.mapProvider, entityType).From(fieldSelector);
        return new Create<TEntity, TSource>(this.connection, this.transaction, visitor);
    }
    public ICreate<TEntity, T1, T2> From<T1, T2>(Expression<Func<T1, T2, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = new CreateVisitor(this.connection.DbKey, this.ormProvider, this.mapProvider, entityType).From(fieldSelector);
        return new Create<TEntity, T1, T2>(this.connection, this.transaction, visitor);
    }
    public ICreate<TEntity, T1, T2, T3> From<T1, T2, T3>(Expression<Func<T1, T2, T3, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = new CreateVisitor(this.connection.DbKey, this.ormProvider, this.mapProvider, entityType).From(fieldSelector);
        return new Create<TEntity, T1, T2, T3>(this.connection, this.transaction, visitor);
    }
    public ICreate<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = new CreateVisitor(this.connection.DbKey, this.ormProvider, this.mapProvider, entityType).From(fieldSelector);
        return new Create<TEntity, T1, T2, T3, T4>(this.connection, this.transaction, visitor);
    }
    public ICreate<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = new CreateVisitor(this.connection.DbKey, this.ormProvider, this.mapProvider, entityType).From(fieldSelector);
        return new Create<TEntity, T1, T2, T3, T4, T5>(this.connection, this.transaction, visitor);
    }
}
class Created<TEntity> : ICreated<TEntity>
{
    private static ConcurrentDictionary<int, object> commandInitializerCache = new();
    private readonly TheaConnection connection;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly IDbTransaction transaction;
    private string rawSql = null;
    private object parameters = null;
    private int? bulkCount;

    public Created(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, string rawSql, object parameters, int? bulkCount = null)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.rawSql = rawSql;
        this.parameters = parameters;
        this.bulkCount = bulkCount;
    }
    public int Execute()
    {
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
        else
        {
            string sql = null;
            int result = 0;
            using var command = this.connection.CreateCommand();
            if (string.IsNullOrEmpty(this.rawSql))
            {
                Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
                if (isDictionary)
                    commandInitializer = this.BuildCommandInitializer(entityType);
                else commandInitializer = this.BuildCommandInitializer(entityType, parameterType);
                sql = commandInitializer.Invoke(command, this.ormProvider, this.parameters);
            }
            else
            {
                sql = this.rawSql;
                Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
                if (isDictionary)
                    commandInitializer = this.BuildCommandInitializer(sql);
                else commandInitializer = this.BuildCommandInitializer(sql, entityType, parameterType);
                commandInitializer.Invoke(command, this.ormProvider, this.parameters);
            }
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
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
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

        int result = 0, index = 0;
        if (isMulti)
        {
            Action<IDbCommand, IOrmProvider, StringBuilder, int, object> commandInitializer = null;
            if (isDictionary)
                commandInitializer = this.BuildBatchCommandInitializer(entityType);
            else commandInitializer = this.BuildBatchCommandInitializer(entityType, parameterType);

            this.bulkCount ??= 500;
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
        else
        {
            string sql = null;
            using var cmd = this.connection.CreateCommand();
            if (string.IsNullOrEmpty(this.rawSql))
            {
                Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
                if (isDictionary)
                    commandInitializer = this.BuildCommandInitializer(entityType);
                else commandInitializer = this.BuildCommandInitializer(entityType, parameterType);
                sql = commandInitializer.Invoke(cmd, this.ormProvider, this.parameters);
            }
            else
            {
                sql = this.rawSql;
                Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
                if (isDictionary)
                    commandInitializer = this.BuildCommandInitializer(sql);
                else commandInitializer = this.BuildCommandInitializer(sql, entityType, parameterType);
                commandInitializer.Invoke(cmd, this.ormProvider, this.parameters);
            }
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

                var isNullable = parameterMemberMapper.MemberType.IsNullableType(out _);
                RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, isNullable, propMapper.NativeDbType, propMapper, this.ormProvider, localParameters, blockParameters, blockBodies);
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
                var isNullable = parameterMemberMapper.MemberType.IsNullableType(out _);
                RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, isNullable, propMapper.NativeDbType, propMapper, this.ormProvider, localParameters, blockParameters, blockBodies);
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
                var isNullable = parameterMemberMapper.MemberType.IsNullableType(out _);
                RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, isNullable, parameterMemberMapper.NativeDbType, parameterMemberMapper, this.ormProvider, localParameters, blockParameters, blockBodies);
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
class CreateBase
{
    protected readonly TheaConnection connection;
    protected readonly IDbTransaction transaction;
    protected readonly CreateVisitor visitor;

    public CreateBase(TheaConnection connection, IDbTransaction transaction, CreateVisitor visitor)
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
class Create<TEntity, T1> : CreateBase, ICreate<TEntity, T1>
{
    public Create(TheaConnection connection, IDbTransaction transaction, CreateVisitor visitor)
        : base(connection, transaction, visitor) { }
    public ICreate<TEntity, T1> Where(Expression<Func<T1, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public ICreate<TEntity, T1> And(bool condition, Expression<Func<T1, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.And(predicate);
        return this;
    }
}
class Create<TEntity, T1, T2> : CreateBase, ICreate<TEntity, T1, T2>
{
    public Create(TheaConnection connection, IDbTransaction transaction, CreateVisitor visitor)
        : base(connection, transaction, visitor) { }
    public ICreate<TEntity, T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public ICreate<TEntity, T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.And(predicate);
        return this;
    }
}
class Create<TEntity, T1, T2, T3> : CreateBase, ICreate<TEntity, T1, T2, T3>
{
    public Create(TheaConnection connection, IDbTransaction transaction, CreateVisitor visitor)
        : base(connection, transaction, visitor) { }
    public ICreate<TEntity, T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public ICreate<TEntity, T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.And(predicate);
        return this;
    }
}
class Create<TEntity, T1, T2, T3, T4> : CreateBase, ICreate<TEntity, T1, T2, T3, T4>
{
    public Create(TheaConnection connection, IDbTransaction transaction, CreateVisitor visitor)
        : base(connection, transaction, visitor) { }
    public ICreate<TEntity, T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public ICreate<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.And(predicate);
        return this;
    }
}
class Create<TEntity, T1, T2, T3, T4, T5> : CreateBase, ICreate<TEntity, T1, T2, T3, T4, T5>
{
    public Create(TheaConnection connection, IDbTransaction transaction, CreateVisitor visitor)
        : base(connection, transaction, visitor) { }
    public ICreate<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public ICreate<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.And(predicate);
        return this;
    }
}
