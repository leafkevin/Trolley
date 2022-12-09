using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class Create<T> : ICreate<T>
{
    private static ConcurrentDictionary<int, object> commandInitializerCache = new();
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private bool isWithBy = false;
    private IQuery<T> query = null;
    private object parameters = null;
    private int? bulkCount = null;

    public Create(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
    }
    public ICreate<T> WithBy<TInsertObject>(TInsertObject insertObjs, int bulkCount = 500)
    {
        this.isWithBy = true;
        this.parameters = insertObjs;
        this.bulkCount = bulkCount;
        return this;
    }
    public ICreate<T> From(Func<IFromQuery, IQuery<T>> subQuery)
    {
        this.isWithBy = false;
        var fromQuery = new FromQuery(this.dbFactory, this.connection);
        this.query = subQuery.Invoke(fromQuery);
        return this;
    }
    public int Execute()
    {
        if (this.isWithBy)
        {
            bool isMulti = false;
            bool isDictionary = false;
            var entityType = typeof(T);
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
                Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
                if (isDictionary)
                    commandInitializer = this.BuildCommandInitializer(entityType);
                else commandInitializer = this.BuildCommandInitializer(entityType, parameterType);
                var command = this.connection.CreateCommand();
                command.CommandText = commandInitializer?.Invoke(command, this.connection.OrmProvider, this.parameters);
                command.CommandType = CommandType.Text;
                command.Transaction = this.transaction;

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }
        else
        {
            var sql = query.ToSql(out var dbParameters, out var readerFields);
            var builder = new StringBuilder("INSERT INTO (");
            for (int i = 0; i < readerFields.Count; i++)
            {
                var readerField = readerFields[i];
                var fieldName = this.connection.OrmProvider.GetFieldName(readerField.MemberMapper.FieldName);
                if (i > 0) builder.Append(',');
                builder.Append(fieldName);
            }
            builder.Append(") ");
            builder.Append(sql);
            var command = this.connection.CreateCommand();
            command.CommandText = builder.ToString();
            command.CommandType = CommandType.Text;
            command.Transaction = this.transaction;

            if (dbParameters != null && dbParameters.Count > 0)
                dbParameters.ForEach(f => command.Parameters.Add(f));

            connection.Open();
            return command.ExecuteNonQuery();
        }
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (this.isWithBy)
        {
            bool isMulti = false;
            bool isDictionary = false;
            var entityType = typeof(T);
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
                Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
                if (isDictionary)
                    commandInitializer = this.BuildCommandInitializer(entityType);
                else commandInitializer = this.BuildCommandInitializer(entityType, parameterType);

                var cmd = this.connection.CreateCommand();
                cmd.Transaction = this.transaction;
                if (cmd is not DbCommand command)
                    throw new Exception("当前数据库驱动不支持异步SQL查询");

                command.CommandType = CommandType.Text;
                command.CommandText = commandInitializer?.Invoke(command, this.connection.OrmProvider, this.parameters);
                await this.connection.OpenAsync(cancellationToken);
                return await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        else
        {
            var sql = this.query.ToSql(out var dbParameters, out var readerFields);
            var builder = new StringBuilder("INSERT INTO (");
            for (int i = 0; i < readerFields.Count; i++)
            {
                var readerField = readerFields[i];
                var fieldName = this.connection.OrmProvider.GetFieldName(readerField.MemberMapper.FieldName);
                if (i > 0) builder.Append(',');
                builder.Append(fieldName);
            }
            builder.Append(") ");
            builder.Append(sql);
            var cmd = this.connection.CreateCommand();
            cmd.Transaction = this.transaction;
            if (cmd is not DbCommand command)
                throw new Exception("当前数据库驱动不支持异步SQL查询");

            command.CommandText = builder.ToString();
            command.CommandType = CommandType.Text;
            if (dbParameters != null && dbParameters.Count > 0)
                dbParameters.ForEach(f => command.Parameters.Add(f));

            await this.connection.OpenAsync(cancellationToken);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
    public string ToSql()
    {
        if (this.isWithBy)
        {
            bool isMulti = false;
            bool isDictionary = false;
            var entityType = typeof(T);
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
                if (index > 0)
                    return sqlBuilder.ToString();
                return null;
            }
            else
            {
                Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
                if (isDictionary)
                    commandInitializer = this.BuildCommandInitializer(entityType);
                else commandInitializer = this.BuildCommandInitializer(entityType, parameterType);
                var command = this.connection.CreateCommand();
                return commandInitializer?.Invoke(command, this.connection.OrmProvider, this.parameters);
            }
        }
        else
        {
            var sql = this.query.ToSql(out var dbParameters, out var readerFields);
            var builder = new StringBuilder("INSERT INTO (");
            for (int i = 0; i < readerFields.Count; i++)
            {
                var readerField = readerFields[i];
                var fieldName = this.connection.OrmProvider.GetFieldName(readerField.MemberMapper.FieldName);
                if (i > 0) builder.Append(',');
                builder.Append(fieldName);
            }
            builder.Append(") ");
            builder.Append(sql);
            return builder.ToString();
        }
    }
    private Action<IDbCommand, IOrmProvider, StringBuilder, int, object> BuildBatchCommandInitializer(Type entityType, Type parameterType)
    {
        var cacheKey = HashCode.Combine("CreateBatch", connection.OrmProvider, entityType, parameterType);
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
                RepositoryHelper.AddWhereParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, parameterMemberMapper.MemberName, blockBodies);
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
        var cacheKey = HashCode.Combine("Create", connection.OrmProvider, entityType, parameterType);
        if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
        {
            int columnIndex = 0;
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var parameterMapper = dbFactory.GetEntityMap(parameterType);
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
                RepositoryHelper.AddWhereParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, parameterMemberMapper.MemberName, blockBodies);
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
    private Action<IDbCommand, IOrmProvider, StringBuilder, int, object> BuildBatchCommandInitializer(Type entityType)
    {
        return (command, ormProvider, builder, index, parameter) =>
        {
            int columnIndex = 0;
            var dict = parameter as Dictionary<string, object>;
            var entityMapper = dbFactory.GetEntityMap(entityType);
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
            var entityMapper = dbFactory.GetEntityMap(entityType);
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
}
