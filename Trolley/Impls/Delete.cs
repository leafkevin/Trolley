using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
    public IDeleted<TEntity> RawSql(string rawSql, object parameters)
        => new Deleted<TEntity>(this.dbFactory, this.connection, this.transaction, rawSql, parameters);
    public IDeleted<TEntity> Where(object keys)
        => new Deleted<TEntity>(this.dbFactory, this.connection, this.transaction, null, keys);

    public IDeleting<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        var visitor = new DeleteVisitor(this.dbFactory, this.connection.OrmProvider, typeof(TEntity));
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
    private string rawSql = null;
    private object parameters = null;

    public Deleted(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, string rawSql, object parameters)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.rawSql = rawSql;
        this.parameters = parameters;
    }
    public int Execute()
    {
        bool isDictionary = false;
        var entityType = typeof(TEntity);
        if (this.parameters is Dictionary<string, object> dict)
            isDictionary = true;
        var parameterType = parameters.GetType();
        string sql = null;
        var command = this.connection.CreateCommand();
        var ormProvider = this.connection.OrmProvider;
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
        var result = command.ExecuteNonQuery();
        command.Dispose();
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        bool isDictionary = false;
        var entityType = typeof(TEntity);
        if (this.parameters is Dictionary<string, object> dict)
            isDictionary = true;
        var parameterType = parameters.GetType();
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
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        command.Dispose();
        return result;
    }
    public string ToSql()
    {
        bool isDictionary = false;
        var entityType = typeof(TEntity);
        if (this.parameters is Dictionary<string, object> dict)
            isDictionary = true;
        var parameterType = parameters.GetType();
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
                    throw new Exception($"参数类型{parameterType.FullName}不存在字段{keyMapper.MemberName}");

                var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                if (index > 0)
                    builder.Append(',');
                builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                var parameterNameExpr = Expression.Constant(parameterName);
                RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, propMapper.MemberName, blockBodies);
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
                    throw new Exception($"参数字典中不存在字段{keyMapper.MemberName}");

                if (index > 0)
                    builder.Append(',');
                var parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName;
                var dbParameter = ormProvider.CreateParameter(parameterName, fieldValue);
                builder.Append($"{ormProvider.GetFieldName(keyMapper.MemberName)}={parameterName}");
                command.Parameters.Add(dbParameter);
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
        if (condition)
            this.visitor.And(predicate);
        return this;
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
        var result = command.ExecuteNonQuery();
        command.Dispose();
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var sql = this.visitor.BuildSql(out var dbParameters);
        var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => cmd.Parameters.Add(f));
        if (cmd is not DbCommand command)
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        command.Dispose();
        return result;
    }
    public string ToSql() => this.visitor.BuildSql(out _);
}

