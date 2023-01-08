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

class Delete<TEntity> : IDelete<TEntity>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;

    public Delete(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
    }
    public IDeleted<TEntity> Where(object keys)
    {
        if (keys == null)
            throw new ArgumentNullException(nameof(keys));

        return new Deleted<TEntity>(this.dbFactory, this.connection, this.transaction, keys);
    }
    public IDeleting<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var visitor = new DeleteVisitor(this.dbFactory, this.connection, this.transaction, typeof(TEntity));
        visitor.Where(predicate);
        return new Deleting<TEntity>(this.connection, this.transaction, visitor);
    }
}
class Deleted<TEntity> : IDeleted<TEntity>
{
    private static ConcurrentDictionary<int, object> commandInitializerCache = new();
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private object parameters = null;

    public Deleted(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, object parameters)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.parameters = parameters;
    }
    public int Execute()
    {
        bool isMulti = false;
        bool isDictionary = false;
        var entityType = typeof(TEntity);
        var parameterType = this.parameters.GetType();
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

            int index = 0;
            var sqlBuilder = new StringBuilder();
            using var command = this.connection.CreateCommand();
            foreach (var entity in entities)
            {
                commandInitializer.Invoke(command, this.connection.OrmProvider, sqlBuilder, index, entity);
                index++;
            }
            command.CommandText = sqlBuilder.ToString();
            command.CommandType = CommandType.Text;
            command.Transaction = this.transaction;
            connection.Open();
            var result = command.ExecuteNonQuery();
            command.Dispose();
            return result;
        }
        else
        {
            string sql = null;
            using var command = this.connection.CreateCommand();
            var ormProvider = this.connection.OrmProvider;
            Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
            if (isDictionary)
                commandInitializer = this.BuildCommandInitializer(entityType);
            else commandInitializer = this.BuildCommandInitializer(entityType, parameterType);
            sql = commandInitializer.Invoke(command, this.connection.OrmProvider, this.parameters);

            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            command.Transaction = this.transaction;
            connection.Open();
            var result = command.ExecuteNonQuery();
            command.Dispose();
            return result;
        }
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        bool isMulti = false;
        bool isDictionary = false;
        var entityType = typeof(TEntity);
        var parameterType = this.parameters.GetType();
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

            int index = 0;
            var sqlBuilder = new StringBuilder();
            using var cmd = this.connection.CreateCommand();
            foreach (var entity in entities)
            {
                commandInitializer.Invoke(cmd, this.connection.OrmProvider, sqlBuilder, index, entity);
                index++;
            }
            cmd.CommandText = sqlBuilder.ToString();
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = this.transaction;
            if (cmd is not DbCommand command)
                throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

            await this.connection.OpenAsync(cancellationToken);
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            command.Dispose();
            return result;
        }
        else
        {
            string sql = null;
            using var cmd = this.connection.CreateCommand();
            Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
            if (isDictionary)
                commandInitializer = this.BuildCommandInitializer(entityType);
            else commandInitializer = this.BuildCommandInitializer(entityType, parameterType);
            sql = commandInitializer.Invoke(cmd, this.connection.OrmProvider, this.parameters);

            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = this.transaction;
            if (cmd is not DbCommand command)
                throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

            await this.connection.OpenAsync(cancellationToken);
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            command.Dispose();
            return result;
        }
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        bool isDictionary = false;
        var entityType = typeof(TEntity);
        if (this.parameters is Dictionary<string, object> dict)
            isDictionary = true;
        var parameterType = parameters.GetType();
        Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
        if (isDictionary)
            commandInitializer = this.BuildCommandInitializer(entityType);
        else commandInitializer = this.BuildCommandInitializer(entityType, parameterType);
        using var command = this.connection.CreateCommand();
        var sql = commandInitializer?.Invoke(command, this.connection.OrmProvider, this.parameters);
        if (command.Parameters != null && command.Parameters.Count > 0)
            dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Cancel();
        command.Dispose();
        return sql;
    }
    private Action<IDbCommand, IOrmProvider, StringBuilder, int, object> BuildBatchCommandInitializer(Type entityType, Type parameterType)
    {
        var cacheKey = HashCode.Combine("DeleteBatch", connection.OrmProvider, string.Empty, entityType, parameterType);
        if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
        {
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var parameterMapper = dbFactory.GetEntityMap(parameterType);
            var ormProvider = this.connection.OrmProvider;
            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
            var indexExpr = Expression.Parameter(typeof(int), "index");
            var parameterExpr = Expression.Parameter(typeof(object), "parameter");

            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            ParameterExpression typedParameterExpr = null;
            var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");

            if (parameterType.IsEntityType())
            {
                typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));
            }
            blockParameters.Add(parameterNameExpr);

            var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
            var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var methodInfo3 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });

            var addCommaExpr = Expression.Call(builderExpr, methodInfo1, Expression.Constant(';'));
            var greatThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0, typeof(int)));
            blockBodies.Add(Expression.IfThen(greatThenExpr, addCommaExpr));
            var sql = $"DELETE FROM {ormProvider.GetFieldName(entityMapper.TableName)} WHERE ";
            blockBodies.Add(Expression.Call(builderExpr, methodInfo2, Expression.Constant(sql)));

            int index = 0;
            foreach (var keyMapper in entityMapper.KeyMembers)
            {
                if (index > 0)
                    blockBodies.Add(Expression.Call(builderExpr, methodInfo2, Expression.Constant(" AND ")));

                var parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName;
                var suffixExpr = Expression.Call(indexExpr, typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes));
                var concatExpr = Expression.Call(methodInfo3, Expression.Constant(parameterName), suffixExpr);
                blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));

                var constantExpr = Expression.Constant(ormProvider.GetFieldName(keyMapper.FieldName) + "=");
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, constantExpr));
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, parameterNameExpr));

                if (parameterType.IsEntityType())
                    RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, keyMapper.NativeDbType, keyMapper.MemberName, blockBodies);
                else RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, parameterExpr, parameterNameExpr, keyMapper.NativeDbType, blockBodies);
            }
            commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, StringBuilder, int, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, builderExpr, indexExpr, parameterExpr).Compile();
            commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
        }
        return (Action<IDbCommand, IOrmProvider, StringBuilder, int, object>)commandInitializerDelegate;
    }
    private Action<IDbCommand, IOrmProvider, StringBuilder, int, object> BuildBatchCommandInitializer(Type entityType)
    {
        var entityMapper = this.dbFactory.GetEntityMap(entityType);
        if (entityMapper.KeyMembers.Count > 1)
        {
            return (command, ormProvider, builder, index, parameter) =>
            {
                var dict = parameter as Dictionary<string, object>;
                if (index > 0) builder.Append(';');
                else builder.Append($"DELETE FROM {ormProvider.GetFieldName(entityMapper.TableName)} WHERE ");
                int keyIndex = 0;
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    if (keyIndex > 0) builder.Append(" AND ");
                    var fieldName = ormProvider.GetFieldName(keyMapper.FieldName);
                    string parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName + index.ToString();
                    builder.Append($"{fieldName}={parameterName}");

                    if (keyMapper.NativeDbType.HasValue)
                        command.Parameters.Add(ormProvider.CreateParameter(parameterName, keyMapper.NativeDbType.Value, dict[keyMapper.MemberName]));
                    else command.Parameters.Add(ormProvider.CreateParameter(parameterName, dict[keyMapper.MemberName]));
                }
            };
        }
        else
        {
            return (command, ormProvider, builder, index, parameter) =>
            {
                var dict = parameter as Dictionary<string, object>;
                if (index > 0) builder.Append(',');
                else builder.Append($"DELETE FROM {ormProvider.GetFieldName(entityMapper.TableName)} WHERE {ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)} IN (");
                var keyMapper = entityMapper.KeyMembers[0];
                string parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName + index.ToString();
                builder.Append(parameterName);

                if (keyMapper.NativeDbType.HasValue)
                    command.Parameters.Add(ormProvider.CreateParameter(parameterName, keyMapper.NativeDbType.Value, dict[keyMapper.MemberName]));
                else command.Parameters.Add(ormProvider.CreateParameter(parameterName, dict[keyMapper.MemberName]));
            };
        }
    }
    private Func<IDbCommand, IOrmProvider, object, string> BuildCommandInitializer(Type entityType, Type parameterType)
    {
        var cacheKey = HashCode.Combine("Delete", connection.OrmProvider, string.Empty, entityType, parameterType);
        if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
        {
            int index = 0;
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

            var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
            var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var methodInfo3 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });

            var builder = new StringBuilder($"DELETE FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
            foreach (var keyMapper in entityMapper.KeyMembers)
            {
                if (!parameterMapper.TryGetMemberMap(keyMapper.MemberName, out var propMapper))
                    throw new ArgumentNullException($"参数类型{parameterType.FullName}缺少主键字段{keyMapper.MemberName}", "keys");

                var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                if (index > 0)
                    builder.Append(" AND ");
                builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                var parameterNameExpr = Expression.Constant(parameterName);
                RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, propMapper.NativeDbType, propMapper.MemberName, blockBodies);
                index++;
            }
            var resultLabelExpr = Expression.Label(typeof(string));
            var returnExpr = Expression.Constant(builder.ToString());
            blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
            blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

            commandInitializerDelegate = Expression.Lambda<Func<IDbCommand, IOrmProvider, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
            commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
        }
        return (Func<IDbCommand, IOrmProvider, object, string>)commandInitializerDelegate;
    }
    private Action<IDbCommand, IOrmProvider, object> BuildCommandInitializer(string sql, Type entityType, Type parameterType)
    {
        var cacheKey = HashCode.Combine("Delete", connection.OrmProvider, sql, entityType, parameterType);
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
    private Func<IDbCommand, IOrmProvider, object, string> BuildCommandInitializer(Type entityType)
    {
        return (command, ormProvider, parameter) =>
        {
            int index = 0;
            var dict = parameter as Dictionary<string, object>;
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var builder = new StringBuilder($"DELETE FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");

            foreach (var keyMapper in entityMapper.KeyMembers)
            {
                if (!dict.TryGetValue(keyMapper.MemberName, out var fieldValue))
                    throw new ArgumentNullException($"字典参数中缺少主键字段{keyMapper.MemberName}", "keys");

                if (index > 0)
                    builder.Append(',');
                var parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName;

                builder.Append($"{ormProvider.GetFieldName(keyMapper.MemberName)}={parameterName}");

                if (keyMapper.NativeDbType.HasValue)
                    command.Parameters.Add(ormProvider.CreateParameter(parameterName, keyMapper.NativeDbType.Value, fieldValue));
                else command.Parameters.Add(ormProvider.CreateParameter(parameterName, fieldValue));
                index++;
            }
            return builder.ToString();
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
class Deleting<TEntity> : IDeleting<TEntity>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly DeleteVisitor visitor;

    public Deleting(TheaConnection connection, IDbTransaction transaction, DeleteVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }

    public IDeleting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (condition)
            this.visitor.And(predicate);
        return this;
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
        connection.Open();
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
        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => cmd.Parameters.Add(f));
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        await command.DisposeAsync();
        return result;
    }
    public string ToSql(out List<IDbDataParameter> dbParameters) => this.visitor.BuildSql(out dbParameters);
}

