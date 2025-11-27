using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class Create<TEntity> : CreateInternal, ICreate<TEntity>
{
    #region Constructor
    public Create(DbContext dbContext)
    {
        this.DbContext = dbContext;
        this.Visitor = this.DbContext.OrmProvider.NewCreateVisitor(typeof(TEntity), dbContext);
    }
    #endregion

    #region Sharding
    public virtual ICreate<TEntity> UseTable(string tableName)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableName);
        return this;
    }
    public virtual ICreate<TEntity> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual ICreate<TEntity> UseTable<TInsertObj>(Func<string, TInsertObj, string> tableNameGetter)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, tableNameGetter);
        return this;
    }
    public virtual ICreate<TEntity> UseTableByOthers(params object[] otherFieldValues)
    {
        this.Visitor.UseTableByOthers(TableShardingUsageMode.WriteOnly, false, otherFieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual ICreate<TEntity> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithBy
    public virtual IContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
    {
        base.WithByInternal(true, insertObj);
        return this.OrmProvider.NewContinuedCreate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithBulk
    public virtual IContinuedCreate<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount = 500)
    {
        this.WithBulkInternal(insertObjs, bulkCount);
        return this.OrmProvider.NewContinuedCreate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region From
    public virtual IFromCommand<TEntity, T> From<T>()
    {
        var queryVisitor = this.FromInternal(typeof(T));
        return this.OrmProvider.NewFromCommand<TEntity, T>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<TEntity, T1, T2> From<T1, T2>()
    {
        var queryVisitor = this.FromInternal(typeof(T1), typeof(T2));
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3> From<T1, T2, T3>()
    {
        var queryVisitor = this.FromInternal(typeof(T1), typeof(T2), typeof(T3));
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>()
    {
        var queryVisitor = this.FromInternal(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
    {
        var queryVisitor = this.FromInternal(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, T5>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>()
    {
        var queryVisitor = this.FromInternal(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, T5, T6>(this.DbContext, queryVisitor);
    }
    #endregion

    #region FromQuery
    public virtual IFromCommand<TEntity, T> FromQuery<T>(IQuery<T> subQuery)
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.IsFromCommand = true;
        queryVisitor.IsFromQuery = true;
        queryVisitor.UseQuery(typeof(T), subQuery, true);
        return this.OrmProvider.NewFromCommand<TEntity, T>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<TEntity, T> FromQuery<T>(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.IsFromCommand = true;
        queryVisitor.IsFromQuery = true;
        queryVisitor.UseNewQuery(typeof(T), subQueryExpr, true);
        return this.OrmProvider.NewFromCommand<TEntity, T>(this.DbContext, queryVisitor);
    }
    #endregion
}
public class Created<TEntity> : CreateInternal, ICreated<TEntity>
{
    #region Constructor
    public Created(DbContext dbContext, ICreateVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Execute
    public virtual int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.Bulk:
                (var shardingType, var shardingTables, var insertObjs, var bulkCount, var firstSqlSetter,
                    var loopSqlSetter, _, var readerFields) = this.Visitor.BuildWithBulk(command);

                int index = 0;
                var builder = new StringBuilder();
                void TabledExecute(string tableName, IEnumerable insertObjs)
                {
                    foreach (var insertObj in insertObjs)
                    {
                        loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                        index++;
                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                            builder.Clear();
                            command.Parameters.Clear();
                            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                            index = 0;
                        }
                    }
                }
                connection.Open();
                if (shardingType == ShardingTableType.SplitTables)
                {
                    var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                    foreach (var tableName in tabledInsertObjs.Keys)
                    {
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        var tableParameters = tabledInsertObjs[tableName];
                        TabledExecute(tableName, tableParameters);
                    }
                }
                else
                {
                    var tableName = shardingTables as string;
                    firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                    TabledExecute(tableName, insertObjs);
                }
                if (index > 0)
                {
                    command.CommandText = builder.ToString();
                    result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                }
                builder.Clear();
                break;
            default:
                //默认单条
                command.CommandText = this.Visitor.BuildCommand(command, out _);
                connection.Open();
                result = command.ExecuteNonQuery(CommandSqlType.Insert);
                break;
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.Bulk:
                (var shardingType, var shardingTables, var insertObjs, var bulkCount, var firstSqlSetter,
                     var loopSqlSetter, _, var readerFields) = this.Visitor.BuildWithBulk(command);

                int index = 0;
                var builder = new StringBuilder();
                async Task TabledExecute(string tableName, IEnumerable insertObjs, CancellationToken cancellationToken)
                {
                    foreach (var insertObj in insertObjs)
                    {
                        loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                        index++;
                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                            builder.Clear();
                            command.Parameters.Clear();
                            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                            index = 0;
                        }
                    }
                }
                await connection.OpenAsync(cancellationToken);
                if (shardingType == ShardingTableType.SplitTables)
                {
                    var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                    foreach (var tableName in tabledInsertObjs.Keys)
                    {
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        var tableParameters = tabledInsertObjs[tableName];
                        await TabledExecute(tableName, tableParameters, cancellationToken);
                    }
                }
                else
                {
                    var tableName = shardingTables as string;
                    firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                    await TabledExecute(tableName, insertObjs, cancellationToken);
                }
                if (index > 0)
                {
                    command.CommandText = builder.ToString();
                    result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                }
                builder.Clear();
                break;
            default:
                //默认单条
                command.CommandText = this.Visitor.BuildCommand(command, out _);
                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(CommandSqlType.Insert, cancellationToken);
                break;
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region ExecuteIdentity
    public virtual int ExecuteIdentity() => this.DbContext.CreateIdentity<int>(this.Visitor);
    public virtual async Task<int> ExecuteIdentityAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.CreateIdentityAsync<int>(this.Visitor, cancellationToken);
    public virtual long ExecuteIdentityLong() => this.DbContext.CreateIdentity<long>(this.Visitor);
    public virtual async Task<long> ExecuteIdentityLongAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.CreateIdentityAsync<long>(this.Visitor, cancellationToken);
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var sql = this.Visitor.BuildCommand(command, out _);
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        this.Visitor.Dispose();
        return sql;
    }
    #endregion

    #region Close
    public virtual void Close(ITheaConnection connection)
    {
        connection.Close();
        this.Visitor.Dispose();
        this.Visitor = null;
    }
    public virtual async ValueTask CloseAsync(ITheaConnection connection)
    {
        await connection.CloseAsync();
        this.Visitor.Dispose();
        this.Visitor = null;
    }
    #endregion
}
public class ContinuedCreate<TEntity> : Created<TEntity>, IContinuedCreate<TEntity>
{
    #region Constructor
    public ContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region WithBy
    public virtual IContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => this.WithBy(true, insertObj);
    public virtual IContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
    {
        base.WithByInternal(condition, insertObj);
        return this;
    }
    public virtual IContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.WithBy(true, fieldSelector, fieldValue);
    public virtual IContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        base.WithByInternal(condition, fieldSelector, fieldValue);
        return this;
    }
    #endregion

    #region IgnoreFields
    public virtual IContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames)
    {
        base.IgnoreFieldsInternal(fieldNames);
        return this;
    }
    public virtual IContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        base.IgnoreFieldsInternal(fieldsSelector);
        return this;
    }
    #endregion

    #region OnlyFields
    public virtual IContinuedCreate<TEntity> OnlyFields(params string[] fieldNames)
    {
        base.OnlyFieldsInternal(fieldNames);
        return this;
    }
    public virtual IContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        base.OnlyFieldsInternal(fieldsSelector);
        return this;
    }
    #endregion
}