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

public class Create<TEntity> : ICreate<TEntity>
{
    #region Properties
    public DbContext DbContext { get; protected set; }
    public ICreateVisitor Visitor { get; protected set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

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
    public virtual ICreate<TEntity> UseTable(Func<string, object, string> tableNameGetter)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, tableNameGetter);
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
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        if (insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithBulkBy方法");
        var insertObjType = typeof(TInsertObject);
        if (!insertObjType.IsEntityType(out _))
            throw new NotSupportedException($"方法WithBy只支持类对象参数，不支持基础类型参数, insertObj类型: {insertObjType.FullName}");

        this.Visitor.WithBy(insertObj);
        return this.OrmProvider.NewContinuedCreate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithBulk
    public virtual IContinuedCreate<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        if (insertObjs is string || insertObjs is IDictionary<string, object>)
            throw new NotSupportedException("批量插入，单个对象类型只支持命名对象、匿名对象或是字典对象");
        bool isEmpty = true;
        foreach (var insertObj in insertObjs)
        {
            isEmpty = false;
            break;
        }
        if (isEmpty) throw new Exception("批量插入，insertObjs参数至少要有一条数据");

        this.Visitor.WithBulk(insertObjs, bulkCount);
        return this.OrmProvider.NewContinuedCreate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region From
    public virtual IFromCommand<TEntity, T> From<T>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('a', typeof(T));
        queryVisitor.IsFromCommand = true;
        return this.OrmProvider.NewFromCommand<TEntity, T>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<TEntity, T1, T2> From<T1, T2>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('a', typeof(T1), typeof(T2));
        queryVisitor.IsFromCommand = true;
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3> From<T1, T2, T3>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('a', typeof(T1), typeof(T2), typeof(T3));
        queryVisitor.IsFromCommand = true;
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('a', typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        queryVisitor.IsFromCommand = true;
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('a', typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        queryVisitor.IsFromCommand = true;
        return this.OrmProvider.NewFromCommand<TEntity, T1, T2, T3, T4, T5>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('a', typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        queryVisitor.IsFromCommand = true;
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
public class Created<TEntity> : ICreated<TEntity>
{
    #region Properties
    public DbContext DbContext { get; protected set; }
    public ICreateVisitor Visitor { get; protected set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

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
                            builder.Remove(builder.Length - 1, 1);
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
                    builder.Remove(builder.Length - 1, 1);
                    command.CommandText = builder.ToString();
                    result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                }
                builder.Clear();
                break;
            default:
                command.CommandText = this.Visitor.BuildSql(command, out _);
                connection.Open();
                result = command.ExecuteNonQuery(CommandSqlType.Insert);
                break;
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        this.Visitor.Dispose();
        this.Visitor = null;
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
                            builder.Remove(builder.Length - 1, 1);
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
                    builder.Remove(builder.Length - 1, 1);
                    command.CommandText = builder.ToString();
                    result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                }
                builder.Clear();
                break;
            default:
                command.CommandText = this.Visitor.BuildSql(command, out _);
                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(CommandSqlType.Insert, cancellationToken);
                break;
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        this.Visitor.Dispose();
        this.Visitor = null;
        return result;
    }
    #endregion

    #region ExecuteIdentity
    public virtual int ExecuteIdentity()
    {
        var result = this.DbContext.CreateIdentity<int>(this.Visitor);
        this.Visitor.Dispose();
        this.Visitor = null;
        return result;
    }
    public virtual async Task<int> ExecuteIdentityAsync(CancellationToken cancellationToken = default)
    {
        var result = await this.DbContext.CreateIdentityAsync<int>(this.Visitor, cancellationToken);
        this.Visitor.Dispose();
        this.Visitor = null;
        return result;
    }
    public virtual long ExecuteIdentityLong()
    {
        var result = this.DbContext.CreateIdentity<long>(this.Visitor); this.Visitor.Dispose();
        this.Visitor.Dispose();
        this.Visitor = null;
        return result;
    }
    public virtual async Task<long> ExecuteIdentityLongAsync(CancellationToken cancellationToken = default)
    {
        var result = await this.DbContext.CreateIdentityAsync<long>(this.Visitor, cancellationToken);
        this.Visitor.Dispose();
        this.Visitor = null;
        return result;
    }
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        (_, _, var command) = this.DbContext.UseMasterCommand();
        var sql = this.Visitor.BuildSql(command, out _);
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        this.Visitor.Dispose();
        this.Visitor = null;
        return sql;
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
    public virtual IContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        this.Visitor.WithByField(fieldSelector, fieldValue);
        return this;
    }
    public virtual IContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (!condition) return this;
        return this.WithBy(fieldSelector, fieldValue);
    }
    #endregion
}