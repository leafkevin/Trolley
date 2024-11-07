using System;
using System.Collections;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.Sqlite;

public class SqliteBulkContinuedCreate<TEntity> : ContinuedCreate<TEntity>, ISqliteCreated<TEntity>, ISqliteBulkContinuedCreate<TEntity>
{
    #region Properties
    public SqliteCreateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public SqliteBulkContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as SqliteCreateVisitor;
    }
    #endregion

    #region WithBy
    public new ISqliteBulkContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => this.WithBy(true, insertObj);
    public new ISqliteBulkContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
        => base.WithBy(condition, insertObj) as ISqliteBulkContinuedCreate<TEntity>;
    public new ISqliteBulkContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.WithBy(true, fieldSelector, fieldValue);
    public new ISqliteBulkContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.WithBy(condition, fieldSelector, fieldValue) as ISqliteBulkContinuedCreate<TEntity>;
    #endregion

    #region IgnoreFields
    public new ISqliteBulkContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames)
        => base.IgnoreFields(fieldNames) as ISqliteBulkContinuedCreate<TEntity>;
    public new ISqliteBulkContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.IgnoreFields(fieldsSelector) as ISqliteBulkContinuedCreate<TEntity>;
    #endregion

    #region OnlyFields
    public new ISqliteBulkContinuedCreate<TEntity> OnlyFields(params string[] fieldNames)
        => base.OnlyFields(fieldNames) as ISqliteBulkContinuedCreate<TEntity>;
    public new ISqliteBulkContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.OnlyFields(fieldsSelector) as ISqliteBulkContinuedCreate<TEntity>;
    #endregion

    #region OnConflict
    public ISqliteBulkContinuedCreate<TEntity> OnConflict<TUpdateFields>(Expression<Func<ISqliteCreateConflictDoUpdate<TEntity>, TUpdateFields>> fieldsAssignment)
    {
        this.DialectVisitor.OnConflict(fieldsAssignment);
        return this;
    }
    #endregion

    #region Returning
    public ISqliteBulkCreated<TEntity, TResult> Returning<TResult>(params string[] fieldNames)
    {
        this.DialectVisitor.Returning(fieldNames);
        return new SqliteBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public ISqliteBulkCreated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Returning(fieldsSelector);
        return new SqliteBulkCreated<TEntity, TResult>(this.DbContext, this.Visitor);
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
            //case ActionMode.BulkCopy:
            //    (var insertObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
            //    Type insertObjType = null;
            //    foreach (var insertObj in insertObjs)
            //    {
            //        insertObjType = insertObj.GetType();
            //        break;
            //    }
            //    var dialectOrmProvider = this.OrmProvider as SqliteProvider;
            //    var sqlVisitor = this.Visitor as SqlVisitor;
            //    if (this.DbContext.ShardingProvider != null && this.DbContext.ShardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            //    {
            //        isNeedSplit = this.Visitor.Tables[0].Body == null;
            //        if (isNeedSplit)
            //        {
            //            var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
            //            foreach (var tabledInsertObj in tabledInsertObjs)
            //            {
            //                result += dialectOrmProvider.ExecuteBulkCopy(false, this.DbContext, sqlVisitor, connection, insertObjType, tabledInsertObj.Value, timeoutSeconds, tabledInsertObj.Key);
            //            }
            //        }
            //        else result = dialectOrmProvider.ExecuteBulkCopy(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, timeoutSeconds, this.Visitor.Tables[0].Body);
            //    }
            //    else result = dialectOrmProvider.ExecuteBulkCopy(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, timeoutSeconds);
            //    break;
            case ActionMode.Bulk:
                var builder = new StringBuilder();
                (isNeedSplit, var tableName, var insertObjs, var bulkCount,
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
            //case ActionMode.BulkCopy:
            //    (var insertObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
            //    Type insertObjType = null;
            //    foreach (var insertObj in insertObjs)
            //    {
            //        insertObjType = insertObj.GetType();
            //        break;
            //    }
            //    var dialectOrmProvider = this.OrmProvider as SqliteProvider;
            //    var sqlVisitor = this.Visitor as SqlVisitor;
            //    if (this.DbContext.ShardingProvider != null && this.DbContext.ShardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            //    {
            //        isNeedSplit = this.Visitor.Tables[0].Body == null;
            //        if (isNeedSplit)
            //        {
            //            var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
            //            foreach (var tabledInsertObj in tabledInsertObjs)
            //            {
            //                result += await dialectOrmProvider.ExecuteBulkCopyAsync(false, this.DbContext, sqlVisitor, connection, insertObjType, tabledInsertObj.Value, timeoutSeconds, cancellationToken, tabledInsertObj.Key);
            //            }
            //        }
            //        else result = await dialectOrmProvider.ExecuteBulkCopyAsync(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, timeoutSeconds, cancellationToken, this.Visitor.Tables[0].Body);
            //    }
            //    else result = await dialectOrmProvider.ExecuteBulkCopyAsync(false, this.DbContext, sqlVisitor, connection, insertObjType, insertObjs, timeoutSeconds, cancellationToken);
            //    break;
            case ActionMode.Bulk:
                var builder = new StringBuilder();
                (isNeedSplit, var tableName, var insertObjs, var bulkCount,
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