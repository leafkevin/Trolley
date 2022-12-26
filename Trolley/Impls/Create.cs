﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class Create<TEntity> : ICreate<TEntity>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;

    public Create(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
    }
    public ICreated<TEntity> RawSql(string rawSql, object parameters)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        return new Created<TEntity>(this.dbFactory, this.connection, this.transaction, rawSql, parameters);
    }
    public ICreated<TEntity> WithBy<TInsertObject>(TInsertObject insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));
        return new Created<TEntity>(this.dbFactory, this.connection, this.transaction, null, insertObjs, bulkCount);
    }
    public ICreate<TEntity, TSource> From<TSource>(Expression<Func<TSource, object>> fieldSelector)
    {
        var entityType = typeof(TEntity);
        var visitor = new CreateVisitor(this.dbFactory, this.connection.OrmProvider, entityType).From(fieldSelector);
        return new Create<TEntity, TSource>(this.connection, this.transaction, visitor);
    }
    public ICreate<TEntity, T1, T2> From<T1, T2>(Expression<Func<T1, T2, object>> fieldSelector)
    {
        var entityType = typeof(TEntity);
        var visitor = new CreateVisitor(this.dbFactory, this.connection.OrmProvider, entityType).From(fieldSelector);
        return new Create<TEntity, T1, T2>(this.connection, this.transaction, visitor);
    }
    public ICreate<TEntity, T1, T2, T3> From<T1, T2, T3>(Expression<Func<T1, T2, T3, object>> fieldSelector)
    {
        var entityType = typeof(TEntity);
        var visitor = new CreateVisitor(this.dbFactory, this.connection.OrmProvider, entityType).From(fieldSelector);
        return new Create<TEntity, T1, T2, T3>(this.connection, this.transaction, visitor);
    }
    public ICreate<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, object>> fieldSelector)
    {
        var entityType = typeof(TEntity);
        var visitor = new CreateVisitor(this.dbFactory, this.connection.OrmProvider, entityType).From(fieldSelector);
        return new Create<TEntity, T1, T2, T3, T4>(this.connection, this.transaction, visitor);
    }
    public ICreate<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, object>> fieldSelector)
    {
        var entityType = typeof(TEntity);
        var visitor = new CreateVisitor(this.dbFactory, this.connection.OrmProvider, entityType).From(fieldSelector);
        return new Create<TEntity, T1, T2, T3, T4, T5>(this.connection, this.transaction, visitor);
    }
}
class Created<TEntity> : ICreated<TEntity>
{
    private static ConcurrentDictionary<int, object> commandInitializerCache = new();
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private string rawSql = null;
    private object parameters = null;
    private int? bulkCount;

    public Created(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, string rawSql, object parameters, int? bulkCount = null)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.rawSql = rawSql;
        this.parameters = parameters;
        this.bulkCount = bulkCount;
    }
    public int Execute()
    {
        bool isMulti = false;
        bool isDictionary = false;
        Type parameterType = null;
        IEnumerable entities = null;
        var entityType = typeof(TEntity);

        if (this.parameters is Dictionary<string, object> dict)
            isDictionary = true;
        else if (this.parameters is IEnumerable)
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
        if (isMulti)
        {
            Action<IDbCommand, IOrmProvider, StringBuilder, int, object> commandInitializer = null;
            if (isDictionary)
                commandInitializer = this.BuildBatchCommandInitializer(entityType);
            else commandInitializer = this.BuildBatchCommandInitializer(entityType, parameterType);

            this.bulkCount ??= 500;
            int result = 0, index = 0;
            var sqlBuilder = new StringBuilder();
            var command = this.connection.CreateCommand();
            foreach (var entity in entities)
            {
                commandInitializer.Invoke(command, this.connection.OrmProvider, sqlBuilder, index, entity);
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
            return result;
        }
        else
        {
            string sql = null;
            var command = this.connection.CreateCommand();
            if (string.IsNullOrEmpty(this.rawSql))
            {
                Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
                if (isDictionary)
                    commandInitializer = this.BuildCommandInitializer(entityType);
                else commandInitializer = this.BuildCommandInitializer(entityType, parameterType);
                sql = commandInitializer.Invoke(command, this.connection.OrmProvider, this.parameters);
            }
            else
            {
                Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
                if (isDictionary)
                    commandInitializer = this.BuildCommandInitializer(sql);
                else commandInitializer = this.BuildCommandInitializer(sql, entityType, parameterType);
                commandInitializer.Invoke(command, this.connection.OrmProvider, this.parameters);
                sql = this.rawSql;
            }
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            command.Transaction = this.transaction;
            connection.Open();
            return command.ExecuteNonQuery();
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
        else if (this.parameters is IEnumerable)
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
        if (isMulti)
        {
            Action<IDbCommand, IOrmProvider, StringBuilder, int, object> commandInitializer = null;
            if (isDictionary)
                commandInitializer = this.BuildBatchCommandInitializer(entityType);
            else commandInitializer = this.BuildBatchCommandInitializer(entityType, parameterType);

            this.bulkCount ??= 500;
            int result = 0, index = 0;
            var sqlBuilder = new StringBuilder();

            var cmd = this.connection.CreateCommand();
            if (cmd is not DbCommand command)
                throw new Exception("当前数据库驱动不支持异步SQL查询");

            foreach (var entity in entities)
            {
                commandInitializer.Invoke(command, this.connection.OrmProvider, sqlBuilder, index, entity);
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
            return result;
        }
        else
        {
            string sql = null;
            var cmd = this.connection.CreateCommand();
            if (string.IsNullOrEmpty(this.rawSql))
            {
                Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
                if (isDictionary)
                    commandInitializer = this.BuildCommandInitializer(entityType);
                else commandInitializer = this.BuildCommandInitializer(entityType, parameterType);
                sql = commandInitializer.Invoke(cmd, this.connection.OrmProvider, this.parameters);
            }
            else
            {
                Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
                if (isDictionary)
                    commandInitializer = this.BuildCommandInitializer(sql);
                else commandInitializer = this.BuildCommandInitializer(sql, entityType, parameterType);
                commandInitializer.Invoke(cmd, this.connection.OrmProvider, this.parameters);
                sql = this.rawSql;
            }
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = this.transaction;
            if (cmd is not DbCommand command)
                throw new Exception("当前数据库驱动不支持异步SQL查询");

            await this.connection.OpenAsync(cancellationToken);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
    public string ToSql()
    {
        bool isMulti = false;
        bool isDictionary = false;
        var entityType = typeof(TEntity);
        Type parameterType = null;
        IEnumerable entities = null;
        if (this.parameters is Dictionary<string, object> dict)
            isDictionary = true;
        else if (this.parameters is IEnumerable)
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
        if (isMulti)
        {
            Action<IDbCommand, IOrmProvider, StringBuilder, int, object> commandInitializer = null;
            if (isDictionary)
                commandInitializer = this.BuildBatchCommandInitializer(entityType);
            else commandInitializer = this.BuildBatchCommandInitializer(entityType, parameterType);
            this.bulkCount ??= 500;
            int index = 0;
            var sqlBuilder = new StringBuilder();
            var command = this.connection.CreateCommand();
            foreach (var entity in entities)
            {
                commandInitializer.Invoke(command, this.connection.OrmProvider, sqlBuilder, index, entity);
                if (index >= this.bulkCount)
                    return sqlBuilder.ToString();
                index++;
            }
            string sql = null;
            if (index > 0)
                sql = sqlBuilder.ToString();
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
            var command = this.connection.CreateCommand();
            var sql = commandInitializer?.Invoke(command, this.connection.OrmProvider, this.parameters);
            command.Cancel();
            command.Dispose();
            return sql;
        }
    }
    private Action<IDbCommand, IOrmProvider, StringBuilder, int, object> BuildBatchCommandInitializer(Type entityType, Type parameterType)
    {
        var cacheKey = HashCode.Combine("CreateBatch", connection.OrmProvider, string.Empty, entityType, parameterType);
        if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
        {
            int columnIndex = 0;
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var parameterMapper = dbFactory.GetEntityMap(parameterType);
            var ormProvider = this.connection.OrmProvider;
            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
            var indexExpr = Expression.Parameter(typeof(int), "index");
            var parameterExpr = Expression.Parameter(typeof(object), "parameter");

            var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
            var insertBuilderExpr = Expression.Variable(typeof(StringBuilder), "insertBuilder");
            var valuesBuilderExpr = Expression.Variable(typeof(StringBuilder), "valuesBuilderExpr");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            blockParameters.Add(typedParameterExpr);
            blockParameters.Add(insertBuilderExpr);
            blockParameters.Add(valuesBuilderExpr);
            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

            var ctor = typeof(StringBuilder).GetConstructor(new Type[] { typeof(string) });
            var insertSqlExpr = Expression.Constant($"INSERT INTO {ormProvider.GetTableName(entityMapper.TableName)} (");
            blockBodies.Add(Expression.Assign(insertBuilderExpr, Expression.New(ctor, insertSqlExpr)));
            blockBodies.Add(Expression.Assign(valuesBuilderExpr, Expression.New(ctor, Expression.Constant(" VALUES("))));

            var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
            var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var methodInfo3 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(StringBuilder) });
            var methodInfo4 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
            foreach (var parameterMemberMapper in parameterMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(parameterMemberMapper.MemberName, out var propMapper) || propMapper.IsIgnore || propMapper.IsNavigation)
                    continue;

                if (columnIndex > 0)
                {
                    blockBodies.Add(Expression.Call(insertBuilderExpr, methodInfo1, Expression.Constant(',')));
                    blockBodies.Add(Expression.Call(valuesBuilderExpr, methodInfo1, Expression.Constant(',')));
                }
                blockBodies.Add(Expression.Call(insertBuilderExpr, methodInfo2, Expression.Constant(ormProvider.GetFieldName(propMapper.FieldName))));

                var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                var suffixExpr = Expression.Call(indexExpr, typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes));
                var parameterNameExpr = Expression.Call(methodInfo4, Expression.Constant(parameterName), suffixExpr);
                blockBodies.Add(Expression.Call(valuesBuilderExpr, methodInfo2, parameterNameExpr));
                RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, parameterMemberMapper.MemberName, blockBodies);
                columnIndex++;
            }
            blockBodies.Add(Expression.Call(insertBuilderExpr, methodInfo1, Expression.Constant(')')));
            blockBodies.Add(Expression.Call(valuesBuilderExpr, methodInfo1, Expression.Constant(')')));

            var greatThenExpr = Expression.GreaterThan(Expression.Property(builderExpr, nameof(StringBuilder.Length)), Expression.Constant(0, typeof(int)));
            blockBodies.Add(Expression.IfThen(greatThenExpr, Expression.Call(builderExpr, methodInfo1, Expression.Constant(';'))));
            blockBodies.Add(Expression.Call(builderExpr, methodInfo3, insertBuilderExpr));
            blockBodies.Add(Expression.Call(builderExpr, methodInfo3, valuesBuilderExpr));

            commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, StringBuilder, int, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
            commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
        }
        return (Action<IDbCommand, IOrmProvider, StringBuilder, int, object>)commandInitializerDelegate;
    }
    private Func<IDbCommand, IOrmProvider, object, string> BuildCommandInitializer(Type entityType, Type parameterType)
    {
        var cacheKey = HashCode.Combine("Create", connection.OrmProvider, string.Empty, entityType, parameterType);
        if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
        {
            int columnIndex = 0;
            var entityMapper = this.dbFactory.GetEntityMap(entityType);
            var parameterMapper = this.dbFactory.GetEntityMap(parameterType);
            var ormProvider = this.connection.OrmProvider;
            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var parameterExpr = Expression.Parameter(typeof(object), "parameter");
            var typedParameterExpr = Expression.Parameter(parameterType, "typedParameter");

            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            blockParameters.Add(typedParameterExpr);
            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

            var insertBuilder = new StringBuilder($"INSERT INTO {ormProvider.GetTableName(entityMapper.TableName)} (");
            var valuesBuilder = new StringBuilder(" VALUES(");
            foreach (var parameterMemberMapper in parameterMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(parameterMemberMapper.MemberName, out var propMapper) || propMapper.IsIgnore || propMapper.IsNavigation)
                    continue;

                if (columnIndex > 0)
                {
                    insertBuilder.Append(',');
                    valuesBuilder.Append(',');
                }
                insertBuilder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                valuesBuilder.Append(parameterName);
                var parameterNameExpr = Expression.Constant(parameterName);
                RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, parameterMemberMapper.MemberName, blockBodies);
                columnIndex++;
            }
            insertBuilder.Append(')');
            valuesBuilder.Append(')');

            if (entityMapper.IsAutoIncrement)
                valuesBuilder.AppendFormat(ormProvider.SelectIdentitySql, entityMapper.AutoIncrementField);
            var sql = insertBuilder.ToString() + valuesBuilder.ToString();

            var returnExpr = Expression.Constant(sql, typeof(string));

            var resultLabelExpr = Expression.Label(typeof(string));
            blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
            blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

            commandInitializerDelegate = Expression.Lambda<Func<IDbCommand, IOrmProvider, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
            commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
        }
        return (Func<IDbCommand, IOrmProvider, object, string>)commandInitializerDelegate;
    }
    private Action<IDbCommand, IOrmProvider, object> BuildCommandInitializer(string sql, Type entityType, Type parameterType)
    {
        var cacheKey = HashCode.Combine("Create", connection.OrmProvider, sql, entityType, parameterType);
        if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
        {
            var parameterMapper = this.dbFactory.GetEntityMap(parameterType);
            var ormProvider = this.connection.OrmProvider;
            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var parameterExpr = Expression.Parameter(typeof(object), "parameter");
            var typedParameterExpr = Expression.Parameter(parameterType, "typedParameter");

            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            blockParameters.Add(typedParameterExpr);
            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

            foreach (var parameterMemberMapper in parameterMapper.MemberMaps)
            {
                var parameterName = ormProvider.ParameterPrefix + parameterMemberMapper.MemberName;
                if (!Regex.IsMatch(sql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                    continue;
                var parameterNameExpr = Expression.Constant(parameterName);
                RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, parameterMemberMapper.MemberName, blockBodies);
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
            var entityMapper = this.dbFactory.GetEntityMap(entityType);
            var insertBuilder = new StringBuilder($"INSERT INTO {ormProvider.GetTableName(entityMapper.TableName)} (");
            var valuesBuilder = new StringBuilder(" VALUES(");
            foreach (var item in dict)
            {
                if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || !propMapper.IsKey)
                    continue;

                var parameterName = ormProvider.ParameterPrefix + item.Key + index.ToString();
                var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                if (columnIndex > 0)
                {
                    insertBuilder.Append(',');
                    valuesBuilder.Append(',');
                }
                insertBuilder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                valuesBuilder.Append(parameterName);
                command.Parameters.Add(dbParameter);
                columnIndex++;
            }
            insertBuilder.Append(')');
            valuesBuilder.Append(')');
            if (builder.Length > 0)
                builder.Append(';');
            builder.Append(insertBuilder);
            builder.Append(valuesBuilder);
        };
    }
    private Func<IDbCommand, IOrmProvider, object, string> BuildCommandInitializer(Type entityType)
    {
        return (command, ormProvider, parameter) =>
        {
            int index = 0;
            var dict = parameter as Dictionary<string, object>;
            var entityMapper = this.dbFactory.GetEntityMap(entityType);
            var insertBuilder = new StringBuilder($"INSERT INTO {ormProvider.GetTableName(entityMapper.TableName)} (");
            var valuesBuilder = new StringBuilder(" VALUES(");
            foreach (var item in dict)
            {
                if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || !propMapper.IsKey)
                    continue;

                var parameterName = ormProvider.ParameterPrefix + item.Key;
                var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                if (index > 0)
                {
                    insertBuilder.Append(',');
                    valuesBuilder.Append(',');
                }
                insertBuilder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                valuesBuilder.Append(parameterName);
                command.Parameters.Add(dbParameter);
                index++;
            }
            insertBuilder.Append(')');
            valuesBuilder.Append(')');
            if (entityMapper.IsAutoIncrement)
                valuesBuilder.AppendFormat(connection.OrmProvider.SelectIdentitySql, entityMapper.AutoIncrementField);
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
        var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));

        connection.Open();
        return command.ExecuteNonQuery();
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var sql = this.visitor.BuildSql(out var dbParameters);
        var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;

        if (cmd is not DbCommand command)
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));

        await this.connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
    public string ToSql() => this.visitor.BuildSql(out _);
}
class Create<TEntity, TSource> : CreateBase, ICreate<TEntity, TSource>
{
    public Create(TheaConnection connection, IDbTransaction transaction, CreateVisitor visitor)
       : base(connection, transaction, visitor) { }
    public ICreate<TEntity, TSource> Where(Expression<Func<TSource, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
}
class Create<TEntity, T1, T2> : CreateBase, ICreate<TEntity, T1, T2>
{
    public Create(TheaConnection connection, IDbTransaction transaction, CreateVisitor visitor)
      : base(connection, transaction, visitor) { }
    public ICreate<TEntity, T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
}
class Create<TEntity, T1, T2, T3> : CreateBase, ICreate<TEntity, T1, T2, T3>
{
    public Create(TheaConnection connection, IDbTransaction transaction, CreateVisitor visitor)
       : base(connection, transaction, visitor) { }
    public ICreate<TEntity, T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
}
class Create<TEntity, T1, T2, T3, T4> : CreateBase, ICreate<TEntity, T1, T2, T3, T4>
{
    public Create(TheaConnection connection, IDbTransaction transaction, CreateVisitor visitor)
       : base(connection, transaction, visitor) { }
    public ICreate<TEntity, T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
}
class Create<TEntity, T1, T2, T3, T4, T5> : CreateBase, ICreate<TEntity, T1, T2, T3, T4, T5>
{
    public Create(TheaConnection connection, IDbTransaction transaction, CreateVisitor visitor)
      : base(connection, transaction, visitor) { }
    public ICreate<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
}