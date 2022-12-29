﻿using System;
using System.Collections;
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

/// <summary>
/// PostgreSql:
/// UPDATE sys_order a 
/// SET "TotalAmount"=50, "BuyerCompany"=c."Name"
/// FROM sys_user b,sys_company c
/// WHERE a."BuyerId"=b."Id" AND b."CompanyId"=c."Id" AND c."Id"=1;
/// 
/// MSSql:
/// UPDATE sys_order
/// SET [TotalAmount]=50, [BuyerCompany]=c.[Name]
/// FROM sys_user b,sys_company c
/// WHERE sys_order.[BuyerId]=b.[Id] AND b.[CompanyId]=c.[Id] AND c.[Id]=1;
///
/// MySql:
/// UPDATE sys_order a 
/// INNER JOIN sys_user b ON a.`BuyerId` = b.`Id`
/// INNER JOIN sys_company c ON b.`CompanyId`=c.`Id`
/// SET a.`TotalAmount`=50, a.`BuyerCompany`=c.`Name`
/// WHERE c.`Id`=1;
/// 
/// UPDATE sys_order a 
/// LEFT JOIN sys_user b ON a.`BuyerId` = b.`Id`
/// SET a.`TotalAmount`=50
/// WHERE a.`TotalAmount` IS NULL;
/// </summary>
/// <typeparam name="TEntity"></typeparam>
class Update<TEntity> : IUpdate<TEntity>
{
    private static ConcurrentDictionary<int, object> commandInitializerCache = new();
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;

    public Update(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
    }
    public IUpdateSet<TEntity> RawSql(string rawSql, object parameters = null)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        return new UpdateSet<TEntity>(dbFactory, connection, transaction, rawSql, parameters);
    }
    public IUpdateSet<TEntity> WithBy<TUpdateObject>(TUpdateObject updateObj, int bulkCount = 500)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        return new UpdateSet<TEntity>(dbFactory, connection, transaction, null, updateObj, bulkCount);
    }
    public IUpdateSetting<TEntity> Set<TMember>(Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        var visitor = new UpdateVisitor(this.dbFactory, this.connection.OrmProvider, typeof(TEntity));
        return new UpdateSetting<TEntity>(this.connection, this.transaction, visitor.Set(fieldExpr, fieldValue));
    }
    public IUpdateSetting<TEntity> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        var visitor = new UpdateVisitor(this.dbFactory, this.connection.OrmProvider, typeof(TEntity));
        if (condition)
            visitor.Set(fieldExpr, fieldValue);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, visitor);
    }
    public IUpdateFrom<TEntity, T> From<T>()
    {
        var visitor = new UpdateVisitor(this.dbFactory, this.connection.OrmProvider, typeof(TEntity))
            .From(typeof(T));
        return new UpdateFrom<TEntity, T>(this.connection, this.transaction, visitor);
    }
    public IUpdateFrom<TEntity, T1, T2> From<T1, T2>()
    {
        var visitor = new UpdateVisitor(this.dbFactory, this.connection.OrmProvider, typeof(TEntity))
            .From(typeof(T1), typeof(T2));
        return new UpdateFrom<TEntity, T1, T2>(this.connection, this.transaction, visitor);
    }
    public IUpdateFrom<TEntity, T1, T2, T3> From<T1, T2, T3>()
    {
        var visitor = new UpdateVisitor(this.dbFactory, this.connection.OrmProvider, typeof(TEntity))
             .From(typeof(T1), typeof(T2), typeof(T3));
        return new UpdateFrom<TEntity, T1, T2, T3>(this.connection, this.transaction, visitor);
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>()
    {
        var visitor = new UpdateVisitor(this.dbFactory, this.connection.OrmProvider, typeof(TEntity))
             .From(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new UpdateFrom<TEntity, T1, T2, T3, T4>(this.connection, this.transaction, visitor);
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
    {
        var visitor = new UpdateVisitor(this.dbFactory, this.connection.OrmProvider, typeof(TEntity))
             .From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new UpdateFrom<TEntity, T1, T2, T3, T4, T5>(this.connection, this.transaction, visitor);
    }
    public IUpdateJoin<TEntity, T> InnerJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        var visitor = new UpdateVisitor(this.dbFactory, this.connection.OrmProvider, typeof(TEntity))
            .Join("INNER JOIN", typeof(T), joinOn);
        return new UpdateJoin<TEntity, T>(this.connection, this.transaction, visitor);
    }
    public IUpdateJoin<TEntity, T> LeftJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        var visitor = new UpdateVisitor(this.dbFactory, this.connection.OrmProvider, typeof(TEntity))
           .Join("INNER JOIN", typeof(T), joinOn);
        return new UpdateJoin<TEntity, T>(this.connection, this.transaction, visitor);
    }
}
class UpdateSet<TEntity> : IUpdateSet<TEntity>
{
    private static ConcurrentDictionary<int, object> commandInitializerCache = new();
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private string rawSql = null;
    private object parameters = null;
    private int? bulkCount = null;

    public UpdateSet(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, string rawSql, object parameters, int? bulkCount = null)
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
            var ormProvider = this.connection.OrmProvider;
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
            command.Dispose();
            return result;
        }
        else
        {
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
                sql = this.rawSql;
                Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
                if (isDictionary)
                    commandInitializer = this.BuildCommandInitializer(sql);
                else commandInitializer = this.BuildCommandInitializer(sql, entityType, parameterType);
                commandInitializer.Invoke(command, this.connection.OrmProvider, this.parameters);
            }
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
            command.Dispose();
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
                sql = this.rawSql;
                Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
                if (isDictionary)
                    commandInitializer = this.BuildCommandInitializer(sql);
                else commandInitializer = this.BuildCommandInitializer(sql, entityType, parameterType);
                commandInitializer.Invoke(cmd, this.connection.OrmProvider, this.parameters);
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
    }
    public string ToSql()
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
        var cacheKey = HashCode.Combine("UpdateBatch", connection.OrmProvider, string.Empty, entityType, parameterType);
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
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            blockParameters.Add(typedParameterExpr);
            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

            var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
            var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var methodInfo3 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });

            var greatThenExpr = Expression.GreaterThan(Expression.Property(builderExpr, nameof(StringBuilder.Length)), Expression.Constant(0, typeof(int)));
            blockBodies.Add(Expression.IfThen(greatThenExpr, Expression.Call(builderExpr, methodInfo1, Expression.Constant(';'))));
            blockBodies.Add(Expression.Call(builderExpr, methodInfo2, Expression.Constant($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ")));

            foreach (var parameterMemberMapper in parameterMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(parameterMemberMapper.MemberName, out var propMapper) || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsKey)
                    continue;

                var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                var suffixExpr = Expression.Call(indexExpr, typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes));
                var parameterNameExpr = Expression.Call(methodInfo3, Expression.Constant(parameterName), suffixExpr);

                //生成SQL
                if (columnIndex > 0)
                    blockBodies.Add(Expression.Call(builderExpr, methodInfo1, Expression.Constant(',')));
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, Expression.Constant(ormProvider.GetFieldName(propMapper.FieldName) + "=")));
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, parameterNameExpr));

                RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, parameterMemberMapper.MemberName, blockBodies);
                columnIndex++;
            }
            columnIndex = 0;
            var whereBuilder = new StringBuilder(" WHERE ");
            foreach (var keyMemberMapper in entityMapper.KeyMembers)
            {
                if (!parameterMapper.TryGetMemberMap(keyMemberMapper.MemberName, out var parameterMemberMapper))
                    throw new Exception($"参数类型{parameterMapper.EntityType.FullName}，丢失{keyMemberMapper.MemberName}主键成员");

                var parameterName = ormProvider.ParameterPrefix + "k" + keyMemberMapper.MemberName;
                var suffixExpr = Expression.Call(indexExpr, typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes));
                var parameterNameExpr = Expression.Call(methodInfo3, Expression.Constant(parameterName), suffixExpr);

                if (columnIndex > 0)
                    whereBuilder.Append(" AND ");
                whereBuilder.Append(ormProvider.GetFieldName(keyMemberMapper.FieldName) + "=");
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, Expression.Constant(whereBuilder.ToString())));
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, parameterNameExpr));

                RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, parameterMemberMapper.MemberName, blockBodies);
                columnIndex++;
            }
            blockBodies.Add(Expression.Call(builderExpr, methodInfo2, Expression.Constant(whereBuilder.ToString())));

            commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, StringBuilder, int, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
            commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
        }
        return (Action<IDbCommand, IOrmProvider, StringBuilder, int, object>)commandInitializerDelegate;
    }
    private Func<IDbCommand, IOrmProvider, object, string> BuildCommandInitializer(Type entityType, Type parameterType)
    {
        var cacheKey = HashCode.Combine("Update", connection.OrmProvider, string.Empty, entityType, parameterType);
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

            var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
            var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var methodInfo3 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });

            var sqlBuilder = new StringBuilder($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
            foreach (var parameterMemberMapper in parameterMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(parameterMemberMapper.MemberName, out var propMapper) || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsKey)
                    continue;

                //生成SQL
                var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                if (columnIndex > 0)
                    sqlBuilder.Append(',');
                sqlBuilder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                var parameterNameExpr = Expression.Constant(parameterName);
                RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, parameterMemberMapper.MemberName, blockBodies);
                columnIndex++;
            }
            columnIndex = 0;
            sqlBuilder.Append(" WHERE ");
            foreach (var keyMemberMapper in entityMapper.KeyMembers)
            {
                if (!parameterMapper.TryGetMemberMap(keyMemberMapper.MemberName, out var parameterMemberMapper))
                    throw new Exception($"参数类型{parameterMapper.EntityType.FullName}，丢失{keyMemberMapper.MemberName}主键成员");

                var parameterName = ormProvider.ParameterPrefix + "k" + keyMemberMapper.MemberName;
                var parameterNameExpr = Expression.Constant(parameterName);

                if (columnIndex > 0)
                    sqlBuilder.Append(" AND ");
                sqlBuilder.Append($"{ormProvider.GetFieldName(keyMemberMapper.FieldName)}={parameterName}");
                RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, parameterMemberMapper.MemberName, blockBodies);
                columnIndex++;
            }
            var resultLabelExpr = Expression.Label(typeof(string));
            var returnExpr = Expression.Constant(sqlBuilder.ToString());
            blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
            blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

            commandInitializerDelegate = Expression.Lambda<Func<IDbCommand, IOrmProvider, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
            commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
        }
        return (Func<IDbCommand, IOrmProvider, object, string>)commandInitializerDelegate;
    }
    private Action<IDbCommand, IOrmProvider, object> BuildCommandInitializer(string sql, Type entityType, Type parameterType)
    {
        var cacheKey = HashCode.Combine("Update", connection.OrmProvider, sql, entityType, parameterType);
        if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
        {
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
            int updateIndex = 0, whereIndex = 0;
            var dict = parameter as Dictionary<string, object>;
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var updateBuilder = new StringBuilder($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
            var whereBuilder = new StringBuilder(" WHERE ");

            foreach (var item in dict)
            {
                if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper))
                    continue;

                string parameterName = null;
                StringBuilder sqlBuilder = null;
                if (propMapper.IsKey)
                {
                    parameterName = ormProvider.ParameterPrefix + "k" + item.Key + index.ToString();
                    sqlBuilder = whereBuilder;
                    if (whereIndex > 0)
                        sqlBuilder.Append(',');
                    whereIndex++;
                }
                else
                {
                    parameterName = ormProvider.ParameterPrefix + item.Key + index.ToString();
                    sqlBuilder = updateBuilder;
                    if (updateIndex > 0)
                        sqlBuilder.Append(',');
                    updateIndex++;
                }
                var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                sqlBuilder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                command.Parameters.Add(dbParameter);
            }
            updateBuilder.Append(whereBuilder);
            if (builder.Length > 0)
                builder.Append(';');
            builder.Append(updateBuilder);
        };
    }
    private Func<IDbCommand, IOrmProvider, object, string> BuildCommandInitializer(Type entityType)
    {
        return (command, ormProvider, parameter) =>
        {
            int updateIndex = 0, whereIndex = 0;
            var dict = parameter as Dictionary<string, object>;
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var updateBuilder = new StringBuilder($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
            var whereBuilder = new StringBuilder(" WHERE ");

            foreach (var item in dict)
            {
                if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper))
                    continue;

                string parameterName = null;
                StringBuilder sqlBuilder = null;
                if (propMapper.IsKey)
                {
                    parameterName = ormProvider.ParameterPrefix + "k" + item.Key;
                    sqlBuilder = whereBuilder;
                    if (whereIndex > 0)
                        sqlBuilder.Append(',');
                    whereIndex++;
                }
                else
                {
                    parameterName = ormProvider.ParameterPrefix + item.Key;
                    sqlBuilder = updateBuilder;
                    if (updateIndex > 0)
                        sqlBuilder.Append(',');
                    updateIndex++;
                }
                var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                sqlBuilder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                command.Parameters.Add(dbParameter);
            }
            updateBuilder.Append(whereBuilder);
            return updateBuilder.ToString();
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
class UpdateSetting<TEntity> : IUpdateSetting<TEntity>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly UpdateVisitor visitor;

    public UpdateSetting(TheaConnection connection, IDbTransaction transaction, UpdateVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IUpdateSetting<TEntity> Set<TMember>(Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default)
    {
        this.visitor.Set(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateSetting<TEntity> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldExpr.NodeType != ExpressionType.MemberAccess)
            throw new Exception($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");
        if (condition)
            this.visitor.Set(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateSetting<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateSetting<TEntity> Where(Expression<Func<IWhereSql, TEntity, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateSetting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> predicate)
    {
        if (condition)
            this.visitor.And(predicate);
        return this;
    }
    public IUpdateSetting<TEntity> And(bool condition, Expression<Func<IWhereSql, TEntity, bool>> predicate)
    {
        if (condition)
            this.visitor.And(predicate);
        return this;
    }
    public int Execute()
    {
        var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;
        var sql = this.visitor.BuildSql(out var dbParameters);
        command.CommandText = sql;
        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));
        connection.Open();
        var result = command.ExecuteNonQuery();
        command.Dispose();
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        var sql = this.visitor.BuildSql(out var dbParameters);
        cmd.CommandText = sql;
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
class UpdateFrom<TEntity, T1> : IUpdateFrom<TEntity, T1>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly UpdateVisitor visitor = null;

    public UpdateFrom(TheaConnection connection, IDbTransaction transaction, UpdateVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IUpdateFrom<TEntity, T1> Set<TSetObject>(Expression<Func<TEntity, T1, TSetObject>> setExpr)
    {
        this.visitor.Set(setExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        if (condition)
            this.visitor.Set(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateFrom<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1> Where(Expression<Func<IWhereSql, TEntity, T1, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> predicate)
    {
        if (condition)
            this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, bool>> predicate)
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
class UpdateJoin<TEntity, T1> : IUpdateJoin<TEntity, T1>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly UpdateVisitor visitor = null;

    public UpdateJoin(TheaConnection connection, IDbTransaction transaction, UpdateVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IUpdateJoin<TEntity, T1, T2> InnerJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", typeof(T2), joinOn);
        return new UpdateJoin<TEntity, T1, T2>(this.connection, this.transaction, visitor);
    }
    public IUpdateJoin<TEntity, T1, T2> LeftJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", typeof(T2), joinOn);
        return new UpdateJoin<TEntity, T1, T2>(this.connection, this.transaction, visitor);
    }
    public IUpdateJoin<TEntity, T1> Set<TSetObject>(Expression<Func<TEntity, T1, TSetObject>> setExpr)
    {
        this.visitor.Set(setExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        if (condition)
            this.visitor.Set(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateJoin<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1> Where(Expression<Func<IWhereSql, TEntity, T1, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> predicate)
    {
        if (condition)
            this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, bool>> predicate)
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
class UpdateFrom<TEntity, T1, T2> : IUpdateFrom<TEntity, T1, T2>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly UpdateVisitor visitor = null;

    public UpdateFrom(TheaConnection connection, IDbTransaction transaction, UpdateVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IUpdateFrom<TEntity, T1, T2> Set<TSetObject>(Expression<Func<TEntity, T1, T2, TSetObject>> setExpr)
    {
        this.visitor.Set(setExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        if (condition)
            this.visitor.Set(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> Where(Expression<Func<IWhereSql, TEntity, T1, T2, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> predicate)
    {
        if (condition)
            this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, T2, bool>> predicate)
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
class UpdateJoin<TEntity, T1, T2> : IUpdateJoin<TEntity, T1, T2>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly UpdateVisitor visitor = null;

    public UpdateJoin(TheaConnection connection, IDbTransaction transaction, UpdateVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> InnerJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", typeof(T3), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3>(this.connection, this.transaction, visitor);
    }
    public IUpdateJoin<TEntity, T1, T2, T3> LeftJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", typeof(T3), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3>(this.connection, this.transaction, visitor);
    }
    public IUpdateJoin<TEntity, T1, T2> Set<TSetObject>(Expression<Func<TEntity, T1, T2, TSetObject>> setExpr)
    {
        this.visitor.Set(setExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        if (condition)
            this.visitor.Set(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> Where(Expression<Func<IWhereSql, TEntity, T1, T2, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> predicate)
    {
        if (condition)
            this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, T2, bool>> predicate)
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
class UpdateFrom<TEntity, T1, T2, T3> : IUpdateFrom<TEntity, T1, T2, T3>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly UpdateVisitor visitor = null;

    public UpdateFrom(TheaConnection connection, IDbTransaction transaction, UpdateVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> Set<TSetObject>(Expression<Func<TEntity, T1, T2, T3, TSetObject>> setExpr)
    {
        this.visitor.Set(setExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        if (condition)
            this.visitor.Set(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> Where(Expression<Func<IWhereSql, TEntity, T1, T2, T3, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
    {
        if (condition)
            this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, T2, T3, bool>> predicate)
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
class UpdateJoin<TEntity, T1, T2, T3> : IUpdateJoin<TEntity, T1, T2, T3>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly UpdateVisitor visitor = null;

    public UpdateJoin(TheaConnection connection, IDbTransaction transaction, UpdateVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> InnerJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", typeof(T4), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3, T4>(this.connection, this.transaction, visitor);
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> LeftJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", typeof(T4), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3, T4>(this.connection, this.transaction, visitor);
    }
    public IUpdateJoin<TEntity, T1, T2, T3> Set<TSetObject>(Expression<Func<TEntity, T1, T2, T3, TSetObject>> setExpr)
    {
        this.visitor.Set(setExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        if (condition)
            this.visitor.Set(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> Where(Expression<Func<IWhereSql, TEntity, T1, T2, T3, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
    {
        if (condition)
            this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, T2, T3, bool>> predicate)
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
class UpdateFrom<TEntity, T1, T2, T3, T4> : IUpdateFrom<TEntity, T1, T2, T3, T4>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly UpdateVisitor visitor = null;

    public UpdateFrom(TheaConnection connection, IDbTransaction transaction, UpdateVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TSetObject>(Expression<Func<TEntity, T1, T2, T3, T4, TSetObject>> setExpr)
    {
        this.visitor.Set(setExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        if (condition)
            this.visitor.Set(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Where(Expression<Func<IWhereSql, TEntity, T1, T2, T3, T4, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
    {
        if (condition)
            this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, T2, T3, T4, bool>> predicate)
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
class UpdateJoin<TEntity, T1, T2, T3, T4> : IUpdateJoin<TEntity, T1, T2, T3, T4>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly UpdateVisitor visitor = null;

    public UpdateJoin(TheaConnection connection, IDbTransaction transaction, UpdateVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", typeof(T5), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3, T4, T5>(this.connection, this.transaction, visitor);
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", typeof(T5), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3, T4, T5>(this.connection, this.transaction, visitor);
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TSetObject>(Expression<Func<TEntity, T1, T2, T3, T4, TSetObject>> setExpr)
    {
        this.visitor.Set(setExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        if (condition)
            this.visitor.Set(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Where(Expression<Func<IWhereSql, TEntity, T1, T2, T3, T4, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
    {
        if (condition)
            this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, T2, T3, T4, bool>> predicate)
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
class UpdateFrom<TEntity, T1, T2, T3, T4, T5> : IUpdateFrom<TEntity, T1, T2, T3, T4, T5>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly UpdateVisitor visitor = null;

    public UpdateFrom(TheaConnection connection, IDbTransaction transaction, UpdateVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TSetObject>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TSetObject>> setExpr)
    {
        this.visitor.Set(setExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        if (condition)
            this.visitor.Set(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<IWhereSql, TEntity, T1, T2, T3, T4, T5, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (condition)
            this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, T2, T3, T4, T5, bool>> predicate)
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
class UpdateJoin<TEntity, T1, T2, T3, T4, T5> : IUpdateJoin<TEntity, T1, T2, T3, T4, T5>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly UpdateVisitor visitor = null;

    public UpdateJoin(TheaConnection connection, IDbTransaction transaction, UpdateVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TSetObject>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TSetObject>> setExpr)
    {
        this.visitor.Set(setExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        if (condition)
            this.visitor.Set(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<IWhereSql, TEntity, T1, T2, T3, T4, T5, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (condition)
            this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (condition)
            this.visitor.Where(predicate);
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