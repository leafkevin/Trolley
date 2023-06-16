using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

/// <summary>
/// PostgreSql:
/// UPDATE sys_order a 
/// SET "TotalAmount"=a."TotalAmount"+b."TotalAmount"+50
/// FROM sys_order_detail b
/// WHERE a."Id"=b."OrderId";
/// 
/// MSSql:
/// UPDATE sys_order
/// SET [TotalAmount]=sys_order.[TotalAmount]+b.[TotalAmount]+50
/// FROM sys_order_detail b
/// WHERE sys_order.[Id]=b.[OrderId];
///
/// MySql:
/// UPDATE sys_order a 
/// INNER JOIN sys_order_detail b ON a.`Id` = b.`OrderId`
/// SET `TotalAmount`=a.`TotalAmount`+b.`TotalAmount`+50
/// WHERE a.`Id`=1;
/// 
/// UPDATE sys_order a 
/// INNER JOIN sys_order_detail b ON a.`Id` = b.`OrderId`
/// SET a.`TotalAmount`=a.`TotalAmount`+b.`TotalAmount`+50
/// WHERE a.`Id`=1;
/// 
/// UPDATE sys_order a 
/// LEFT JOIN sys_order_detail b ON a.`Id` = b.`OrderId`
/// SET a.`TotalAmount`=a.`TotalAmount`+b.`TotalAmount`+50
/// WHERE a.`TotalAmount` IS NULL;
/// 
/// Oracle
/// UPDATE sys_order a 
/// SET a.TotalAmount=(SELECT a.TotalAmount+b.TotalAmount+50 FROM sys_order_detail b WHERE a.Id=b.OrderId)
/// WHERE a.`Id`=1;
/// 
/// UPDATE sys_order a 
/// SET (a.OrderNo,a.TotalAmount)=(SELECT 'ON_'||a.OrderNo,a.`TotalAmount`+b.`TotalAmount`+50 FROM sys_order_detail b WHERE a.Id=b.OrderId)
/// WHERE a.`Id`=1;
/// </summary>
/// <typeparam name="TEntity"></typeparam>
class Update<TEntity> : IUpdate<TEntity>
{
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly bool isParameterized;

    public Update(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.isParameterized = isParameterized;
    }
    public IUpdateSet<TEntity> WithBy<TField>(TField parameters)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        return new UpdateSet<TEntity>(this.connection, this.transaction, this.ormProvider, this.mapProvider, false, parameters);
    }
    public IUpdateSet<TEntity> WithBy<TFields>(Expression<Func<TEntity, TFields>> fieldsExpr, object parameters)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));
        if (fieldsExpr.Body.NodeType != ExpressionType.MemberAccess && fieldsExpr.Body.NodeType != ExpressionType.New)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持MemberAccess或New类型表达式");

        return new UpdateSet<TEntity>(this.connection, this.transaction, this.ormProvider, this.mapProvider, false, parameters, fieldsExpr);
    }
    public IUpdateSet<TEntity> WithBulkBy<TFields>(IEnumerable<TFields> parameters, int bulkCount = 500)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        return new UpdateSet<TEntity>(this.connection, this.transaction, this.ormProvider, this.mapProvider, true, parameters, null, bulkCount);
    }
    public IUpdateSet<TEntity> WithBulkBy<TFields>(Expression<Func<TEntity, TFields>> fieldsExpr, IEnumerable parameters, int bulkCount = 500)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        return new UpdateSet<TEntity>(this.connection, this.transaction, this.ormProvider, this.mapProvider, true, parameters, fieldsExpr, bulkCount);
    }
    public IUpdateSetting<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, visitor.Set(fieldsExpr));
    }
    public IUpdateSetting<TEntity> SetIf<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        if (condition) visitor.Set(fieldsExpr);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, visitor);
    }
    public IUpdateSetting<TEntity> Set<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, visitor.Set(fieldsExpr));
    }
    public IUpdateSetting<TEntity> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        if (condition) visitor.Set(fieldsExpr);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, visitor);
    }
    public IUpdateSetting<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, visitor.SetValue(fieldExpr, fieldValueExpr));
    }
    public IUpdateSetting<TEntity> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        if (condition) visitor.SetValue(fieldExpr, fieldValueExpr);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, visitor);
    }
    public IUpdateSetting<TEntity> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, visitor.SetValue(fieldExpr, fieldValue));
    }
    public IUpdateSetting<TEntity> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        if (condition) visitor.SetValue(fieldExpr, fieldValue);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, visitor);
    }

    public IUpdateFrom<TEntity, T> From<T>()
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized)
            .From(typeof(T));
        return new UpdateFrom<TEntity, T>(this.connection, this.transaction, visitor);
    }
    public IUpdateFrom<TEntity, T1, T2> From<T1, T2>()
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized)
            .From(typeof(T1), typeof(T2));
        return new UpdateFrom<TEntity, T1, T2>(this.connection, this.transaction, visitor);
    }
    public IUpdateFrom<TEntity, T1, T2, T3> From<T1, T2, T3>()
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized)
             .From(typeof(T1), typeof(T2), typeof(T3));
        return new UpdateFrom<TEntity, T1, T2, T3>(this.connection, this.transaction, visitor);
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>()
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized)
             .From(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new UpdateFrom<TEntity, T1, T2, T3, T4>(this.connection, this.transaction, visitor);
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized)
             .From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new UpdateFrom<TEntity, T1, T2, T3, T4, T5>(this.connection, this.transaction, visitor);
    }

    public IUpdateJoin<TEntity, T> InnerJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized)
            .Join("INNER JOIN", typeof(T), joinOn);
        return new UpdateJoin<TEntity, T>(this.connection, this.transaction, visitor);
    }
    public IUpdateJoin<TEntity, T> LeftJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized)
           .Join("INNER JOIN", typeof(T), joinOn);
        return new UpdateJoin<TEntity, T>(this.connection, this.transaction, visitor);
    }
}
class UpdateSet<TEntity> : IUpdateSet<TEntity>
{
    private static ConcurrentDictionary<int, object> objCommandInitializerCache = new();
    private static ConcurrentDictionary<int, object> sqlCommandInitializerCache = new();

    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;

    private bool isBulk = false;
    private Expression fieldsExpr = null;
    private object parameters = null;
    private int bulkCount;

    public UpdateSet(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isBulk, object parameters, Expression fieldsExpr = null, int bulkCount = 500)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.isBulk = isBulk;
        this.fieldsExpr = fieldsExpr;
        this.parameters = parameters;
        this.bulkCount = bulkCount;
    }

    public int Execute()
    {
        bool isDictionary = false;
        var entityType = typeof(TEntity);
        Type parameterType = null;
        IEnumerable entities = null;
        if (this.isBulk)
        {
            entities = this.parameters as IEnumerable;
            foreach (var entity in entities)
            {
                if (entity is Dictionary<string, object>)
                    isDictionary = true;
                else parameterType = entity.GetType();
                break;
            }
        }
        else
        {
            if (this.parameters is Dictionary<string, object>)
                isDictionary = true;
            else parameterType = this.parameters.GetType();
        }
        using var command = this.connection.CreateCommand();
        command.Transaction = this.transaction;

        if (this.isBulk)
        {
            int result = 0, index = 0;
            IUpdateVisitor visitor = null;
            Action<IDbCommand, IOrmProvider, StringBuilder, int, object> commandInitializer = null;
            if (this.fieldsExpr != null)
                visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), true);
            else
            {
                if (isDictionary)
                    commandInitializer = this.BuildBatchCommandInitializer(entityType);
                else commandInitializer = this.BuildBatchCommandInitializer(entityType, parameterType);
            }
            var sqlBuilder = new StringBuilder();
            List<IDbDataParameter> fixedDbParameters = null;
            foreach (var entity in entities)
            {
                if (index > 0) sqlBuilder.Append(';');
                if (this.fieldsExpr != null)
                {
                    var dbParameters = visitor.WithBulkBy(fieldsExpr, sqlBuilder, entity, index, out fixedDbParameters);
                    if (dbParameters != null && dbParameters.Count > 0)
                        dbParameters.ForEach(f => command.Parameters.Add(f));
                }
                else commandInitializer.Invoke(command, this.ormProvider, sqlBuilder, index, entity);

                if (index >= this.bulkCount)
                {
                    if (fixedDbParameters.Count > 0)
                        fixedDbParameters.ForEach(f => command.Parameters.Add(f));

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
                if (fixedDbParameters.Count > 0)
                    fixedDbParameters.ForEach(f => command.Parameters.Add(f));

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
            Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
            if (this.fieldsExpr != null)
            {
                var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), true);
                sql = visitor.WithBy(fieldsExpr, parameters, out var dbParameters);
                if (dbParameters != null && dbParameters.Count > 0)
                    dbParameters.ForEach(f => command.Parameters.Add(f));
            }
            else
            {
                if (isDictionary)
                    commandInitializer = this.BuildCommandInitializer(entityType);
                else commandInitializer = this.BuildCommandInitializer(entityType, parameterType);
                sql = commandInitializer.Invoke(command, this.ormProvider, this.parameters);
            }
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            this.connection.Open();
            var result = command.ExecuteNonQuery();
            command.Dispose();
            return result;
        }
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        bool isDictionary = false;
        var entityType = typeof(TEntity);
        Type parameterType = null;
        IEnumerable entities = null;
        if (this.isBulk)
        {
            entities = this.parameters as IEnumerable;
            foreach (var entity in entities)
            {
                if (entity is Dictionary<string, object>)
                    isDictionary = true;
                else parameterType = entity.GetType();
                break;
            }
        }
        else
        {
            if (this.parameters is Dictionary<string, object>)
                isDictionary = true;
            else parameterType = this.parameters.GetType();
        }
        using var cmd = this.connection.CreateCommand();
        cmd.Transaction = this.transaction;

        if (this.isBulk)
        {
            int result = 0, index = 0;
            IUpdateVisitor visitor = null;
            Action<IDbCommand, IOrmProvider, StringBuilder, int, object> commandInitializer = null;
            if (this.fieldsExpr != null)
                visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), true);
            else
            {
                if (isDictionary)
                    commandInitializer = this.BuildBatchCommandInitializer(entityType);
                else commandInitializer = this.BuildBatchCommandInitializer(entityType, parameterType);
            }
            if (cmd is not DbCommand command)
                throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

            var sqlBuilder = new StringBuilder();
            List<IDbDataParameter> fixedDbParameters = null;
            foreach (var entity in entities)
            {
                if (index > 0) sqlBuilder.Append(';');
                if (this.fieldsExpr != null)
                {
                    var dbParameters = visitor.WithBulkBy(fieldsExpr, sqlBuilder, entity, index, out fixedDbParameters);
                    if (dbParameters != null && dbParameters.Count > 0)
                        dbParameters.ForEach(f => command.Parameters.Add(f));
                }
                else commandInitializer.Invoke(command, this.ormProvider, sqlBuilder, index, entity);

                if (index >= this.bulkCount)
                {
                    if (fixedDbParameters.Count > 0)
                        fixedDbParameters.ForEach(f => command.Parameters.Add(f));

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
        else
        {
            string sql = null;
            Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
            if (this.fieldsExpr != null)
            {
                var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), true);
                sql = visitor.WithBy(fieldsExpr, parameters, out var dbParameters);
                if (dbParameters != null && dbParameters.Count > 0)
                    dbParameters.ForEach(f => cmd.Parameters.Add(f));
            }
            else
            {
                if (isDictionary)
                    commandInitializer = this.BuildCommandInitializer(entityType);
                else commandInitializer = this.BuildCommandInitializer(entityType, parameterType);
                sql = commandInitializer.Invoke(cmd, this.ormProvider, this.parameters);
            }
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            if (cmd is not DbCommand command)
                throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

            await this.connection.OpenAsync(cancellationToken);
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            await command.DisposeAsync();
            return result;
        }
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        bool isDictionary = false;
        var entityType = typeof(TEntity);
        Type parameterType = null;
        IEnumerable entities = null;
        if (this.isBulk)
        {
            entities = this.parameters as IEnumerable;
            foreach (var entity in entities)
            {
                if (entity is Dictionary<string, object>)
                    isDictionary = true;
                else parameterType = entity.GetType();
                break;
            }
        }
        else
        {
            if (this.parameters is Dictionary<string, object>)
                isDictionary = true;
            else parameterType = this.parameters.GetType();
        }
        using var command = this.connection.CreateCommand();
        if (this.isBulk)
        {
            int index = 0;
            string sql = null;
            IUpdateVisitor visitor = null;
            Action<IDbCommand, IOrmProvider, StringBuilder, int, object> commandInitializer = null;
            if (this.fieldsExpr != null)
                visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), true);
            else
            {
                if (isDictionary)
                    commandInitializer = this.BuildBatchCommandInitializer(entityType);
                else commandInitializer = this.BuildBatchCommandInitializer(entityType, parameterType);
            }
            var sqlBuilder = new StringBuilder();
            if (this.fieldsExpr != null) dbParameters = new();
            List<IDbDataParameter> fixedDbParameters = null;
            foreach (var entity in entities)
            {
                if (index > 0) sqlBuilder.Append(';');
                if (this.fieldsExpr != null)
                {
                    var dbDataParameters = visitor.WithBulkBy(fieldsExpr, sqlBuilder, entity, index, out fixedDbParameters);
                    if (dbDataParameters != null && dbDataParameters.Count > 0)
                        dbParameters.AddRange(dbDataParameters);
                }
                else commandInitializer.Invoke(command, this.ormProvider, sqlBuilder, index, entity);

                if (index >= this.bulkCount)
                {
                    sql = sqlBuilder.ToString();
                    if (fixedDbParameters != null && fixedDbParameters.Count > 0)
                        dbParameters.AddRange(fixedDbParameters);
                    if (command.Parameters != null && command.Parameters.Count > 0)
                        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
                    index = 0;
                    break;
                }
                index++;
            }
            if (index > 0)
            {
                sql = sqlBuilder.ToString();
                if (fixedDbParameters != null && fixedDbParameters.Count > 0)
                    dbParameters.AddRange(fixedDbParameters);
                if (command.Parameters != null && command.Parameters.Count > 0)
                    dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
            }
            command.Dispose();
            return sql;
        }
        else
        {
            string sql = null;
            Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
            if (this.fieldsExpr != null)
            {
                var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), true);
                sql = visitor.WithBy(fieldsExpr, parameters, out dbParameters);
            }
            else
            {
                if (isDictionary)
                    commandInitializer = this.BuildCommandInitializer(entityType);
                else commandInitializer = this.BuildCommandInitializer(entityType, parameterType);
                sql = commandInitializer.Invoke(command, this.ormProvider, this.parameters);
                if (command.Parameters != null && command.Parameters.Count > 0)
                    dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
            }
            command.Dispose();
            return sql;
        }
    }

    private Action<IDbCommand, IOrmProvider, StringBuilder, int, object> BuildBatchCommandInitializer(Type entityType, Type parameterType)
    {
        var cacheKey = HashCode.Combine("UpdateBatch", this.connection, entityType, parameterType);
        if (!objCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
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
            var parameterNameExpr = Expression.Variable(typeof(string), "parameterName");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            var localParameters = new Dictionary<string, int>();
            blockParameters.Add(typedParameterExpr);
            blockParameters.Add(parameterNameExpr);
            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

            var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
            var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var methodInfo3 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });

            blockBodies.Add(Expression.Call(builderExpr, methodInfo2, Expression.Constant($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ")));
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (!parameterMapper.TryGetMemberMap(propMapper.MemberName, out var parameterMemberMapper)
                    || propMapper.IsIgnore || propMapper.IsNavigation
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;
                if (propMapper.IsKey) continue;

                var parameterName = this.ormProvider.ParameterPrefix + propMapper.MemberName;
                var suffixExpr = Expression.Call(indexExpr, typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes));
                var concatExpr = Expression.Call(methodInfo3, Expression.Constant(parameterName), suffixExpr);
                blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));

                if (columnIndex > 0)
                    blockBodies.Add(Expression.Call(builderExpr, methodInfo1, Expression.Constant(',')));
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, Expression.Constant(this.ormProvider.GetFieldName(propMapper.FieldName) + "=")));
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, parameterNameExpr));

                RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, false, propMapper, this.ormProvider, localParameters, blockParameters, blockBodies);
                columnIndex++;
            }
            if (columnIndex == 0)
                throw new Exception("没有设置任何可以更新的字段");
            columnIndex = 0;
            blockBodies.Add(Expression.Call(builderExpr, methodInfo2, Expression.Constant(" WHERE ")));
            foreach (var keyMapper in entityMapper.KeyMembers)
            {
                if (!parameterMapper.TryGetMemberMap(keyMapper.MemberName, out var parameterMemberMapper))
                    throw new ArgumentNullException($"参数类型{parameterType.FullName}缺少主键字段{keyMapper.MemberName}", "parameters");

                if (columnIndex > 0)
                    blockBodies.Add(Expression.Call(builderExpr, methodInfo2, Expression.Constant(" AND ")));
                var fieldExpr = Expression.Constant(this.ormProvider.GetFieldName(keyMapper.FieldName) + "=");
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, fieldExpr));

                var parameterName = this.ormProvider.ParameterPrefix + "k" + keyMapper.MemberName;
                var suffixExpr = Expression.Call(indexExpr, typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes));
                var concatExpr = Expression.Call(methodInfo3, Expression.Constant(parameterName), suffixExpr);
                blockBodies.Add(Expression.Assign(parameterNameExpr, concatExpr));
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, parameterNameExpr));

                RepositoryHelper.AddKeyMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, keyMapper, this.ormProvider, blockBodies);
                columnIndex++;
            }
            commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, StringBuilder, int, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, builderExpr, indexExpr, parameterExpr).Compile();
            objCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
        }
        return (Action<IDbCommand, IOrmProvider, StringBuilder, int, object>)commandInitializerDelegate;
    }
    private Func<IDbCommand, IOrmProvider, object, string> BuildCommandInitializer(Type entityType, Type parameterType)
    {
        var cacheKey = HashCode.Combine("Update", this.connection, entityType, parameterType);
        if (!objCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
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

            var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
            var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var methodInfo3 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });

            var sqlBuilder = new StringBuilder($"UPDATE {this.ormProvider.GetTableName(entityMapper.TableName)} SET ");
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (!parameterMapper.TryGetMemberMap(propMapper.MemberName, out var parameterMemberMapper)
                    || propMapper.IsIgnore || propMapper.IsNavigation
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;
                if (propMapper.IsKey) continue;

                if (columnIndex > 0)
                    sqlBuilder.Append(',');
                var parameterName = this.ormProvider.ParameterPrefix + propMapper.MemberName;
                sqlBuilder.Append($"{this.ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                var parameterNameExpr = Expression.Constant(parameterName);
                RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, false, propMapper, this.ormProvider, localParameters, blockParameters, blockBodies);
                columnIndex++;
            }
            if (columnIndex == 0)
                throw new Exception("没有设置任何可以更新的字段");
            columnIndex = 0;
            sqlBuilder.Append(" WHERE ");
            foreach (var keyMapper in entityMapper.KeyMembers)
            {
                if (!parameterMapper.TryGetMemberMap(keyMapper.MemberName, out var parameterMemberMapper))
                    throw new ArgumentNullException($"参数类型{parameterType.FullName}缺少主键字段{keyMapper.MemberName}", "parameters");

                if (columnIndex > 0)
                    sqlBuilder.Append(" AND ");
                var parameterName = this.ormProvider.ParameterPrefix + "k" + keyMapper.MemberName;
                sqlBuilder.Append($"{this.ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");
                var parameterNameExpr = Expression.Constant(parameterName);
                RepositoryHelper.AddKeyMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, keyMapper, this.ormProvider, blockBodies);
                columnIndex++;
            }
            var resultLabelExpr = Expression.Label(typeof(string));
            var returnExpr = Expression.Constant(sqlBuilder.ToString());
            blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
            blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

            commandInitializerDelegate = Expression.Lambda<Func<IDbCommand, IOrmProvider, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
            objCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
        }
        return (Func<IDbCommand, IOrmProvider, object, string>)commandInitializerDelegate;
    }
    private Action<IDbCommand, IOrmProvider, StringBuilder, int, object> BuildBatchCommandInitializer(Type entityType)
    {
        return (command, ormProvider, builder, index, parameter) =>
        {
            int updateIndex = 0;
            var dict = parameter as Dictionary<string, object>;
            var entityMapper = this.mapProvider.GetEntityMap(entityType);
            builder.Append($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");

            foreach (var item in dict)
            {
                if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                    || propMapper.IsIgnore || propMapper.IsNavigation
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;
                if (propMapper.IsKey) continue;

                if (updateIndex > 0)
                    builder.Append(',');

                var parameterName = ormProvider.ParameterPrefix + item.Key + index.ToString();
                builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                if (propMapper.NativeDbType != null)
                    command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                updateIndex++;
            }
            updateIndex = 0;
            builder.Append(" WHERE ");
            foreach (var keyMapper in entityMapper.KeyMembers)
            {
                if (!dict.ContainsKey(keyMapper.MemberName))
                    throw new ArgumentNullException($"字典参数中缺少主键字段{keyMapper.MemberName}", "parameters");

                if (updateIndex > 0)
                    builder.Append(',');
                var parameterName = ormProvider.ParameterPrefix + "k" + keyMapper.MemberName + index.ToString();
                builder.Append($"{ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");

                if (keyMapper.NativeDbType != null)
                    command.Parameters.Add(ormProvider.CreateParameter(parameterName, keyMapper.NativeDbType, dict[keyMapper.MemberName]));
                else command.Parameters.Add(ormProvider.CreateParameter(parameterName, dict[keyMapper.MemberName]));
                updateIndex++;
            }
        };
    }
    private Func<IDbCommand, IOrmProvider, object, string> BuildCommandInitializer(Type entityType)
    {
        return (command, ormProvider, parameter) =>
        {
            int index = 0;
            var dict = parameter as Dictionary<string, object>;
            var entityMapper = this.mapProvider.GetEntityMap(entityType);
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");

            foreach (var item in dict)
            {
                if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                    || propMapper.IsIgnore || propMapper.IsNavigation
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;
                if (propMapper.IsKey) continue;

                if (index > 0)
                    sqlBuilder.Append(',');

                var parameterName = ormProvider.ParameterPrefix + item.Key;
                sqlBuilder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                if (propMapper.NativeDbType != null)
                    command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                index++;
            }

            index = 0;
            sqlBuilder.Append(" WHERE ");
            foreach (var keyMapper in entityMapper.KeyMembers)
            {
                if (!dict.ContainsKey(keyMapper.MemberName))
                    throw new ArgumentNullException($"字典参数中缺少主键字段{keyMapper.MemberName}", "parameters");

                if (index > 0)
                    sqlBuilder.Append(',');
                var parameterName = ormProvider.ParameterPrefix + "k" + keyMapper.MemberName;
                sqlBuilder.Append($"{ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");

                if (keyMapper.NativeDbType != null)
                    command.Parameters.Add(ormProvider.CreateParameter(parameterName, keyMapper.NativeDbType, dict[keyMapper.MemberName]));
                else command.Parameters.Add(ormProvider.CreateParameter(parameterName, dict[keyMapper.MemberName]));
                index++;
            }
            return sqlBuilder.ToString();
        };
    }
}
class UpdateBase
{
    protected readonly TheaConnection connection;
    protected readonly IDbTransaction transaction;
    protected readonly IUpdateVisitor visitor;

    public UpdateBase(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }

    public int Execute()
    {
        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;
        var sql = this.visitor.BuildSql(out var dbParameters);
        command.CommandText = sql;
        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));
        this.connection.Open();
        var result = command.ExecuteNonQuery();
        command.Dispose();
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        var sql = this.visitor.BuildSql(out var dbParameters);
        cmd.CommandText = sql;
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
class UpdateSetting<TEntity> : UpdateBase, IUpdateSetting<TEntity>
{
    public UpdateSetting(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }

    public IUpdateSetting<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateSetting<TEntity> SetIf<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateSetting<TEntity> Set<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateSetting<TEntity> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateSetting<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        return new UpdateSetting<TEntity>(this.connection, this.transaction, this.visitor.SetValue(fieldExpr, fieldValueExpr));
    }
    public IUpdateSetting<TEntity> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition) this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, this.visitor);
    }
    public IUpdateSetting<TEntity> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        return new UpdateSetting<TEntity>(this.connection, this.transaction, this.visitor.SetValue(fieldExpr, fieldValue));
    }
    public IUpdateSetting<TEntity> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition) this.visitor.SetValue(fieldExpr, fieldValue);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, this.visitor);
    }

    #region Where/And
    public IUpdateSetting<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateSetting<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateSetting<TEntity> And(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IUpdateSetting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
class UpdateFrom<TEntity, T1> : UpdateBase, IUpdateFrom<TEntity, T1>
{
    public UpdateFrom(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }

    public IUpdateFrom<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateFrom<TEntity, T1> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }

    #region Where/And
    public IUpdateFrom<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
class UpdateJoin<TEntity, T1> : UpdateBase, IUpdateJoin<TEntity, T1>
{
    public UpdateJoin(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }

    public IUpdateJoin<TEntity, T1, T2> InnerJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(T2), joinOn);
        return new UpdateJoin<TEntity, T1, T2>(this.connection, this.transaction, this.visitor);
    }
    public IUpdateJoin<TEntity, T1, T2> LeftJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(T2), joinOn);
        return new UpdateJoin<TEntity, T1, T2>(this.connection, this.transaction, this.visitor);
    }

    public IUpdateJoin<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateJoin<TEntity, T1> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }

    #region Where/And
    public IUpdateJoin<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
class UpdateFrom<TEntity, T1, T2> : UpdateBase, IUpdateFrom<TEntity, T1, T2>
{
    public UpdateFrom(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }

    public IUpdateFrom<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }

    #region Where/And
    public IUpdateFrom<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
class UpdateJoin<TEntity, T1, T2> : UpdateBase, IUpdateJoin<TEntity, T1, T2>
{
    public UpdateJoin(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }

    public IUpdateJoin<TEntity, T1, T2, T3> InnerJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(T3), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3>(this.connection, this.transaction, this.visitor);
    }
    public IUpdateJoin<TEntity, T1, T2, T3> LeftJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(T3), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3>(this.connection, this.transaction, this.visitor);
    }

    public IUpdateJoin<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }

    #region Where/And
    public IUpdateJoin<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
class UpdateFrom<TEntity, T1, T2, T3> : UpdateBase, IUpdateFrom<TEntity, T1, T2, T3>
{
    public UpdateFrom(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }

    public IUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }

    #region Where/And
    public IUpdateFrom<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
class UpdateJoin<TEntity, T1, T2, T3> : UpdateBase, IUpdateJoin<TEntity, T1, T2, T3>
{
    public UpdateJoin(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }

    public IUpdateJoin<TEntity, T1, T2, T3, T4> InnerJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(T4), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3, T4>(this.connection, this.transaction, this.visitor);
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> LeftJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(T4), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3, T4>(this.connection, this.transaction, this.visitor);
    }

    public IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }

    #region Where/And
    public IUpdateJoin<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
class UpdateFrom<TEntity, T1, T2, T3, T4> : UpdateBase, IUpdateFrom<TEntity, T1, T2, T3, T4>
{
    public UpdateFrom(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }

    public IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }

    #region Where/And
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
class UpdateJoin<TEntity, T1, T2, T3, T4> : UpdateBase, IUpdateJoin<TEntity, T1, T2, T3, T4>
{
    public UpdateJoin(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }

    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(T5), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3, T4, T5>(this.connection, this.transaction, this.visitor);
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(T5), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3, T4, T5>(this.connection, this.transaction, this.visitor);
    }

    public IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }

    #region Where/And
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
class UpdateFrom<TEntity, T1, T2, T3, T4, T5> : UpdateBase, IUpdateFrom<TEntity, T1, T2, T3, T4, T5>
{
    public UpdateFrom(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }

    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }

    #region Where/And
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
class UpdateJoin<TEntity, T1, T2, T3, T4, T5> : UpdateBase, IUpdateJoin<TEntity, T1, T2, T3, T4, T5>
{
    public UpdateJoin(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }

    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        if (fieldsExpr.Body.NodeType != ExpressionType.New && fieldsExpr.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsExpr)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValueExpr == null)
            throw new ArgumentNullException(nameof(fieldValueExpr));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValueExpr);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldExpr)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetValue(fieldExpr, fieldValue);
        return this;
    }

    #region Where/And
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion
}