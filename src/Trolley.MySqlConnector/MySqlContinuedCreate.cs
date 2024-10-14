using System;
using System.Collections;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public class MySqlContinuedCreate<TEntity> : ContinuedCreate<TEntity>, IMySqlCreated<TEntity>, IMySqlContinuedCreate<TEntity>
{
    #region Properties
    public MySqlCreateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public MySqlContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as MySqlCreateVisitor;
    }
    #endregion

    #region WithBy
    public new IMySqlContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => this.WithBy(true, insertObj);
    public new IMySqlContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
        => base.WithBy(condition, insertObj) as IMySqlContinuedCreate<TEntity>;
    public new IMySqlContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.WithBy(true, fieldSelector, fieldValue);
    public new IMySqlContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.WithBy(condition, fieldSelector, fieldValue) as IMySqlContinuedCreate<TEntity>;
    #endregion

    #region IgnoreFields
    public new IMySqlContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames)
        => base.IgnoreFields(fieldNames) as IMySqlContinuedCreate<TEntity>;
    public new IMySqlContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.IgnoreFields(fieldsSelector) as IMySqlContinuedCreate<TEntity>;
    #endregion

    #region OnlyFields
    public new IMySqlContinuedCreate<TEntity> OnlyFields(params string[] fieldNames)
        => base.OnlyFields(fieldNames) as IMySqlContinuedCreate<TEntity>;
    public new IMySqlContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.OnlyFields(fieldsSelector) as IMySqlContinuedCreate<TEntity>;
    #endregion

    #region Returnning
    public IMySqlCreated<TEntity, TResult> Returning<TResult>(params string[] fieldNames)
    {
        this.DialectVisitor.Returning(fieldNames);
        return new MySqlCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public IMySqlCreated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Returning(fieldsSelector);
        return new MySqlCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OnDuplicateKeyUpdate
    public IMySqlCreated<TEntity> OnDuplicateKeyUpdate<TUpdateFields>(TUpdateFields updateObj)
    {
        this.DialectVisitor.OnDuplicateKeyUpdate(updateObj);
        return this;
    }
    public IMySqlCreated<TEntity> OnDuplicateKeyUpdate<TUpdateFields>(Expression<Func<IMySqlCreateDuplicateKeyUpdate<TEntity>, TUpdateFields>> fieldsAssignment)
    {
        this.DialectVisitor.OnDuplicateKeyUpdate(fieldsAssignment);
        return this;
    }
    #endregion     

    #region Execute
    public override int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        bool isNeedSplit = false;
        var entityType = typeof(TEntity);
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                (var insertObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                Type insertObjType = null;
                foreach (var insertObj in insertObjs)
                {
                    insertObjType = insertObj.GetType();
                    break;
                }
                var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                var sqlVisitor = this.Visitor as SqlVisitor;
                if (this.DbContext.ShardingProvider != null && this.DbContext.ShardingProvider.TryGetTableSharding(entityType, out var shardingTable))
                {
                    isNeedSplit = this.Visitor.Tables[0].Body == null;
                    if (isNeedSplit)
                    {
                        var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                        foreach (var tabledInsertObj in tabledInsertObjs)
                        {
                            result += dialectOrmProvider.ExecuteBulkCopy(false, this.DbContext, sqlVisitor, connection, insertObjType, tabledInsertObj.Value, timeoutSeconds, tabledInsertObj.Key);
                        }
                    }
                    else result = dialectOrmProvider.ExecuteBulkCopy(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, timeoutSeconds, this.Visitor.Tables[0].Body);
                }
                else result = dialectOrmProvider.ExecuteBulkCopy(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, timeoutSeconds);
                break;
            case ActionMode.Bulk:
                var builder = new StringBuilder();
                (isNeedSplit, var tableName, insertObjs, var bulkCount,
                    var firstSqlSetter, var loopSqlSetter, _) = this.Visitor.BuildWithBulk(command.BaseCommand);
                int executor(string tableName, IEnumerable insertObjs)
                {
                    int count = 0, index = 0;
                    foreach (var insertObj in insertObjs)
                    {
                        if (index > 0) builder.Append(',');
                        loopSqlSetter.Invoke(command.Parameters, builder, insertObj, index.ToString());
                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            count += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                            builder.Clear();
                            command.Parameters.Clear();
                            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                            index = 0;
                            continue;
                        }
                        index++;
                    }
                    if (index > 0)
                    {
                        command.CommandText = builder.ToString();
                        count += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                        builder.Clear();
                        command.Parameters.Clear();
                    }
                    return count;
                };
                connection.Open();
                if (isNeedSplit)
                {
                    var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                    foreach (var tabledInsertObj in tabledInsertObjs)
                    {
                        firstSqlSetter.Invoke(command.Parameters, builder, tabledInsertObj.Key);
                        result += executor(tabledInsertObj.Key, tabledInsertObj.Value);
                    }
                }
                else
                {
                    firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                    result = executor(tableName, insertObjs);
                }
                builder.Clear();
                break;
            default:
                //默认单条
                command.CommandText = this.Visitor.BuildCommand(command.BaseCommand, false, out _);
                connection.Open();
                result = command.ExecuteNonQuery(CommandSqlType.Insert);
                break;
        }
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        bool isNeedSplit;
        var entityType = typeof(TEntity);
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                (var insertObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                Type insertObjType = null;
                foreach (var insertObj in insertObjs)
                {
                    insertObjType = insertObj.GetType();
                    break;
                }
                var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                var sqlVisitor = this.Visitor as SqlVisitor;
                if (this.DbContext.ShardingProvider != null && this.DbContext.ShardingProvider.TryGetTableSharding(entityType, out var shardingTable))
                {
                    isNeedSplit = this.Visitor.Tables[0].Body == null;
                    if (isNeedSplit)
                    {
                        var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                        foreach (var tabledInsertObj in tabledInsertObjs)
                        {
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(false, this.DbContext, sqlVisitor, connection, insertObjType, tabledInsertObj.Value, timeoutSeconds, cancellationToken, tabledInsertObj.Key);
                        }
                    }
                    else result = await dialectOrmProvider.ExecuteBulkCopyAsync(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, timeoutSeconds, cancellationToken, this.Visitor.Tables[0].Body);
                }
                else result = await dialectOrmProvider.ExecuteBulkCopyAsync(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, timeoutSeconds, cancellationToken);
                break;
            case ActionMode.Bulk:
                var builder = new StringBuilder();
                (isNeedSplit, var tableName, insertObjs, var bulkCount,
                    var firstSqlSetter, var loopSqlSetter, _) = this.Visitor.BuildWithBulk(command.BaseCommand);
                async Task<int> executor(string tableName, IEnumerable insertObjs)
                {
                    int count = 0, index = 0;
                    foreach (var insertObj in insertObjs)
                    {
                        if (index > 0) builder.Append(',');
                        loopSqlSetter.Invoke(command.Parameters, builder, insertObj, index.ToString());
                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            count += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                            builder.Clear();
                            command.Parameters.Clear();
                            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                            index = 0;
                            continue;
                        }
                        index++;
                    }
                    if (index > 0)
                    {
                        command.CommandText = builder.ToString();
                        count += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                        builder.Clear();
                        command.Parameters.Clear();
                    }
                    return count;
                };
                await connection.OpenAsync(cancellationToken);
                if (isNeedSplit)
                {
                    var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                    foreach (var tabledInsertObj in tabledInsertObjs)
                    {
                        firstSqlSetter.Invoke(command.Parameters, builder, tabledInsertObj.Key);
                        result += await executor(tabledInsertObj.Key, tabledInsertObj.Value);
                    }
                }
                else
                {
                    firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                    result = await executor(tableName, insertObjs);
                }
                builder.Clear();
                break;
            default:
                //默认单条
                command.CommandText = this.Visitor.BuildCommand(command.BaseCommand, false, out _);
                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(CommandSqlType.Insert, cancellationToken);
                break;
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion
}
public class MySqlBulkContinuedCreate<TEntity> : ContinuedCreate<TEntity>, IMySqlCreated<TEntity>, IMySqlBulkContinuedCreate<TEntity>
{
    #region Properties
    public MySqlCreateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public MySqlBulkContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as MySqlCreateVisitor;
    }
    #endregion

    #region WithBy
    public new IMySqlBulkContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => this.WithBy(true, insertObj);
    public new IMySqlBulkContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
        => base.WithBy(condition, insertObj) as IMySqlBulkContinuedCreate<TEntity>;
    public new IMySqlBulkContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.WithBy(true, fieldSelector, fieldValue);
    public new IMySqlBulkContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.WithBy(condition, fieldSelector, fieldValue) as IMySqlBulkContinuedCreate<TEntity>;
    #endregion

    #region IgnoreFields
    public new IMySqlBulkContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames)
        => base.IgnoreFields(fieldNames) as IMySqlBulkContinuedCreate<TEntity>;
    public new IMySqlBulkContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.IgnoreFields(fieldsSelector) as IMySqlBulkContinuedCreate<TEntity>;
    #endregion

    #region OnlyFields
    public new IMySqlBulkContinuedCreate<TEntity> OnlyFields(params string[] fieldNames)
        => base.OnlyFields(fieldNames) as IMySqlBulkContinuedCreate<TEntity>;
    public new IMySqlBulkContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.OnlyFields(fieldsSelector) as IMySqlBulkContinuedCreate<TEntity>;
    #endregion

    #region Returnning
    public IMySqlBulkCreated<TEntity, TResult> Returning<TResult>(params string[] fieldNames)
    {
        this.DialectVisitor.Returning(fieldNames);
        return new MySqlBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public IMySqlBulkCreated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Returning(fieldsSelector);
        return new MySqlBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OnDuplicateKeyUpdate
    public IMySqlCreated<TEntity> OnDuplicateKeyUpdate<TUpdateFields>(TUpdateFields updateObj)
    {
        this.DialectVisitor.OnDuplicateKeyUpdate(updateObj);
        return this;
    }
    public IMySqlCreated<TEntity> OnDuplicateKeyUpdate<TUpdateFields>(Expression<Func<IMySqlCreateDuplicateKeyUpdate<TEntity>, TUpdateFields>> fieldsAssignment)
    {
        this.DialectVisitor.OnDuplicateKeyUpdate(fieldsAssignment);
        return this;
    }
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        bool isNeedSplit = false;
        var entityType = typeof(TEntity);
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                (var insertObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                Type insertObjType = null;
                foreach (var insertObj in insertObjs)
                {
                    insertObjType = insertObj.GetType();
                    break;
                }
                var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                var sqlVisitor = this.Visitor as SqlVisitor;
                if (this.DbContext.ShardingProvider != null && this.DbContext.ShardingProvider.TryGetTableSharding(entityType, out var shardingTable))
                {
                    isNeedSplit = this.Visitor.Tables[0].Body == null;
                    if (isNeedSplit)
                    {
                        var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                        foreach (var tabledInsertObj in tabledInsertObjs)
                        {
                            result += dialectOrmProvider.ExecuteBulkCopy(false, this.DbContext, sqlVisitor, connection, insertObjType, tabledInsertObj.Value, timeoutSeconds, tabledInsertObj.Key);
                        }
                    }
                    else result = dialectOrmProvider.ExecuteBulkCopy(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, timeoutSeconds, this.Visitor.Tables[0].Body);
                }
                else result = dialectOrmProvider.ExecuteBulkCopy(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, timeoutSeconds);
                break;
            case ActionMode.Bulk:
                var builder = new StringBuilder();
                (isNeedSplit, var tableName, insertObjs, var bulkCount,
                    var firstSqlSetter, var loopSqlSetter, _) = this.Visitor.BuildWithBulk(command.BaseCommand);
                int executor(string tableName, IEnumerable insertObjs)
                {
                    int count = 0, index = 0;
                    foreach (var insertObj in insertObjs)
                    {
                        if (index > 0) builder.Append(',');
                        loopSqlSetter.Invoke(command.Parameters, builder, insertObj, index.ToString());
                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            count += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                            builder.Clear();
                            command.Parameters.Clear();
                            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                            index = 0;
                            continue;
                        }
                        index++;
                    }
                    if (index > 0)
                    {
                        command.CommandText = builder.ToString();
                        count += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                        builder.Clear();
                        command.Parameters.Clear();
                    }
                    return count;
                };
                connection.Open();
                if (isNeedSplit)
                {
                    var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                    foreach (var tabledInsertObj in tabledInsertObjs)
                    {
                        firstSqlSetter.Invoke(command.Parameters, builder, tabledInsertObj.Key);
                        result += executor(tabledInsertObj.Key, tabledInsertObj.Value);
                    }
                }
                else
                {
                    firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                    result = executor(tableName, insertObjs);
                }
                builder.Clear();
                break;
            default:
                //默认单条
                command.CommandText = this.Visitor.BuildCommand(command.BaseCommand, false, out _);
                connection.Open();
                result = command.ExecuteNonQuery(CommandSqlType.Insert);
                break;
        }
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        bool isNeedSplit;
        var entityType = typeof(TEntity);
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                (var insertObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                Type insertObjType = null;
                foreach (var insertObj in insertObjs)
                {
                    insertObjType = insertObj.GetType();
                    break;
                }
                var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                var sqlVisitor = this.Visitor as SqlVisitor;
                if (this.DbContext.ShardingProvider != null && this.DbContext.ShardingProvider.TryGetTableSharding(entityType, out var shardingTable))
                {
                    isNeedSplit = this.Visitor.Tables[0].Body == null;
                    if (isNeedSplit)
                    {
                        var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                        foreach (var tabledInsertObj in tabledInsertObjs)
                        {
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(false, this.DbContext, sqlVisitor, connection, insertObjType, tabledInsertObj.Value, timeoutSeconds, cancellationToken, tabledInsertObj.Key);
                        }
                    }
                    else result = await dialectOrmProvider.ExecuteBulkCopyAsync(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, timeoutSeconds, cancellationToken, this.Visitor.Tables[0].Body);
                }
                else result = await dialectOrmProvider.ExecuteBulkCopyAsync(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, timeoutSeconds, cancellationToken);
                break;
            case ActionMode.Bulk:
                var sqlBuilder = new StringBuilder();
                (isNeedSplit, var tableName, insertObjs, var bulkCount,
                    var firstSqlSetter, var loopSqlSetter, _) = this.Visitor.BuildWithBulk(command.BaseCommand);
                async Task<int> executor(string tableName, IEnumerable insertObjs)
                {
                    int count = 0, index = 0;
                    foreach (var insertObj in insertObjs)
                    {
                        if (index > 0) sqlBuilder.Append(',');
                        loopSqlSetter.Invoke(command.Parameters, sqlBuilder, insertObj, index.ToString());
                        if (index >= bulkCount)
                        {
                            command.CommandText = sqlBuilder.ToString();
                            count += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                            sqlBuilder.Clear();
                            command.Parameters.Clear();
                            firstSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName);
                            index = 0;
                            continue;
                        }
                        index++;
                    }
                    if (index > 0)
                    {
                        command.CommandText = sqlBuilder.ToString();
                        count += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                        sqlBuilder.Clear();
                        command.Parameters.Clear();
                    }
                    return count;
                };
                await connection.OpenAsync(cancellationToken);
                if (isNeedSplit)
                {
                    var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                    foreach (var tabledInsertObj in tabledInsertObjs)
                    {
                        firstSqlSetter.Invoke(command.Parameters, sqlBuilder, tabledInsertObj.Key);
                        result += await executor(tabledInsertObj.Key, tabledInsertObj.Value);
                    }
                }
                else
                {
                    firstSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName);
                    result = await executor(tableName, insertObjs);
                }
                sqlBuilder.Clear();
                break;
            default:
                //默认单条
                command.CommandText = this.Visitor.BuildCommand(command.BaseCommand, false, out _);
                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(CommandSqlType.Insert, cancellationToken);
                break;
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion
}