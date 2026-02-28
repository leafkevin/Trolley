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

public class Create : ICreate
{
    #region Properties
    public DbContext DbContext { get; protected set; }
    public ICreateVisitor Visitor { get; protected set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public Create(Type entityType, DbContext dbContext)
    {
        this.DbContext = dbContext;
        this.Visitor = this.DbContext.OrmProvider.NewCreateVisitor(entityType, dbContext);
    }
    #endregion

    #region Sharding
    public virtual ICreate UseTable(string tableName)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableName);
        return this;
    }
    public virtual ICreate UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual ICreate UseTable(Func<object, string> tableNameGetter)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, tableNameGetter);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual ICreate UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithBy
    public virtual IContinuedCreate WithBy(object insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        if (insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithBulkBy方法");
        var insertObjType = insertObj.GetType();
        if (!insertObjType.IsEntityType(out _))
            throw new NotSupportedException($"方法WithBy只支持类对象参数，不支持基础类型参数, insertObj类型: {insertObjType.FullName}");

        this.Visitor.WithBy(insertObj);
        return this.OrmProvider.NewContinuedCreate(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithBulk
    public virtual IBulkContinuedCreate WithBulk(IEnumerable insertObjs, int bulkCount = 500)
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
        return this.OrmProvider.NewBulkContinuedCreate(this.DbContext, this.Visitor);
    }
    #endregion     
}
public class Created : ICreated
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
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand(this.Visitor);
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
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand(this.Visitor);
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

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        (_, _, var command) = this.DbContext.UseMasterCommand(this.Visitor);
        var sql = this.Visitor.BuildSql(command, out _);
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        this.Visitor.Dispose();
        this.Visitor = null;
        return sql;
    }
    #endregion
}
public class IdentitiedCreated : Created, IIdentitiedCreated
{
    public IdentitiedCreated(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }

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
}
public class ContinuedCreate : IdentitiedCreated, IContinuedCreate
{
    #region Constructor
    public ContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region WithBy   
    public virtual IContinuedCreate WithBy(string fieldName, object fieldValue)
    {
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentNullException(nameof(fieldName));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        this.Visitor.WithByField(fieldName, fieldValue);
        return this;
    }
    public virtual IContinuedCreate WithBy(bool condition, string fieldName, object fieldValue)
    {
        if (!condition) return this;
        return this.WithBy(fieldName, fieldValue);
    }
    #endregion
}
public class BulkContinuedCreate : Created, IBulkContinuedCreate
{
    #region Constructor
    public BulkContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region WithBy
    public virtual IBulkContinuedCreate WithBy(object insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        if (insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithBulkBy方法");
        var insertObjType = insertObj.GetType();
        if (!insertObjType.IsEntityType(out _))
            throw new NotSupportedException($"方法WithBy只支持类对象参数，不支持基础类型参数, insertObj类型: {insertObjType.FullName}");

        this.Visitor.WithBy(insertObj);
        return this;
    }
    public virtual IBulkContinuedCreate WithBy(bool condition, object insertObj)
    {
        if (!condition) return this;
        return this.WithBy(insertObj);
    }
    public virtual IBulkContinuedCreate WithBy(string fieldName, object fieldValue)
    {
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentNullException(nameof(fieldName));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        this.Visitor.WithByField(fieldName, fieldValue);
        return this;
    }
    public virtual IBulkContinuedCreate WithBy(bool condition, string fieldName, object fieldValue)
    {
        if (!condition) return this;
        return this.WithBy(fieldName, fieldValue);
    }
    #endregion
}
public class ResultCreated<TResult> : IResultCommand<TResult>
{
    #region Properties
    public DbContext DbContext { get; protected set; }
    public ICreateVisitor Visitor { get; protected set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public ResultCreated(DbContext dbContext, ICreateVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Execute
    public TResult Execute() => this.DbContext.CreateResult<TResult>(this.Visitor);
    public async Task<TResult> ExecuteAsync(CancellationToken cancellationToken)
        => await this.DbContext.CreateResultAsync<TResult>(this.Visitor, cancellationToken);
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        (_, _, var command) = this.DbContext.UseMasterCommand(this.Visitor);
        var sql = this.Visitor.BuildSql(command, out _);
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        this.Visitor.Dispose();
        this.Visitor = null;
        return sql;
    }
    #endregion
}
public class BulkResultCreated<TResult> : IBulkResultCommand<TResult>
{
    #region Properties
    public DbContext DbContext { get; protected set; }
    public ICreateVisitor Visitor { get; protected set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public BulkResultCreated(DbContext dbContext, ICreateVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Execute
    public List<TResult> Execute() => this.DbContext.CreateResult<List<TResult>>(this.Visitor);
    public async Task<List<TResult>> ExecuteAsync(CancellationToken cancellationToken)
        => await this.DbContext.CreateResultAsync<List<TResult>>(this.Visitor, cancellationToken);
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        (_, _, var command) = this.DbContext.UseMasterCommand(this.Visitor);
        var sql = this.Visitor.BuildSql(command, out _);
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        this.Visitor.Dispose();
        this.Visitor = null;
        return sql;
    }
    #endregion
}

public class Create<TEntity> : Create, ICreate<TEntity>
{
    #region Constructor
    public Create(DbContext dbContext)
        : base(typeof(TEntity), dbContext) { }
    #endregion

    #region Sharding
    public new ICreate<TEntity> UseTable(string tableName)
    {
        base.UseTable(tableName);
        return this;
    }
    public new ICreate<TEntity> UseTableBy(params object[] fieldValues)
    {
        base.UseTableBy(fieldValues);
        return this;
    }
    public new ICreate<TEntity> UseTable(Func<object, string> tableNameGetter)
    {
        base.UseTable(tableNameGetter);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new ICreate<TEntity> UseTableSchema(string tableSchema)
    {
        base.UseTableSchema(tableSchema);
        return this;
    }
    #endregion

    #region WithBy
    public new IContinuedCreate<TEntity> WithBy(object insertObj)
    {
        base.WithBy(insertObj);
        return this.OrmProvider.NewContinuedCreate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithBulk
    public new IBulkContinuedCreate<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount = 500)
    {
        base.WithBulk(insertObjs, bulkCount);
        return this.OrmProvider.NewBulkContinuedCreate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion  
}
public class ContinuedCreate<TEntity> : ContinuedCreate, IContinuedCreate<TEntity>
{
    #region Constructor
    public ContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region WithBy
    public new IContinuedCreate<TEntity> WithBy(string fieldName, object fieldValue)
    {
        base.WithBy(fieldName, fieldValue);
        return this;
    }
    public new IContinuedCreate<TEntity> WithBy(bool condition, string fieldName, object fieldValue)
    {
        base.WithBy(condition, fieldName, fieldValue);
        return this;
    }
    public virtual IContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, object fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        this.Visitor.WithByFieldExpr(fieldSelector, fieldValue);
        return this;
    }
    public virtual IContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, object fieldValue)
    {
        if (!condition) return this;
        return this.WithBy(fieldSelector, fieldValue);
    }
    #endregion
}
public class BulkContinuedCreate<TEntity> : BulkContinuedCreate, IBulkContinuedCreate<TEntity>
{
    #region Constructor
    public BulkContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region WithBy
    public new IBulkContinuedCreate<TEntity> WithBy(object insertObj)
    {
        base.WithBy(insertObj);
        return this;
    }
    public new IBulkContinuedCreate<TEntity> WithBy(bool condition, object insertObj)
    {
        base.WithBy(condition, insertObj);
        return this;
    }
    public new IBulkContinuedCreate<TEntity> WithBy(string fieldName, object fieldValue)
    {
        base.WithBy(fieldName, fieldValue);
        return this;
    }
    public new IBulkContinuedCreate<TEntity> WithBy(bool condition, string fieldName, object fieldValue)
    {
        base.WithBy(condition, fieldName, fieldValue);
        return this;
    }
    public IBulkContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, object fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        this.Visitor.WithByFieldExpr(fieldSelector, fieldValue);
        return this;
    }
    public IBulkContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, object fieldValue)
    {
        if (!condition) return this;
        return this.WithBy(fieldSelector, fieldValue);
    }
    #endregion
}
public class FromCreated : Created, IFromCreated
{
    #region Constructor
    public FromCreated(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IFromCreated UseTable(string tableName)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableName);
        return this;
    }
    public virtual IFromCreated UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IFromCreated UseTable(Func<object, string> tableNameGetter)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, tableNameGetter);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IFromCreated UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion
}
public class FromCreated<TEntity> : FromCreated, IFromCreated<TEntity>
{
    #region Constructor
    public FromCreated(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public new IFromCreated<TEntity> UseTable(string tableName)
    {
        base.UseTable(tableName);
        return this;
    }
    public new IFromCreated<TEntity> UseTableBy(params object[] fieldValues)
    {
        base.UseTableBy(fieldValues);
        return this;
    }
    public new IFromCreated<TEntity> UseTable(Func<object, string> tableNameGetter)
    {
        base.UseTable(tableNameGetter);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IFromCreated<TEntity> UseTableSchema(string tableSchema)
    {
        base.UseTableSchema(tableSchema);
        return this;
    }
    #endregion
}