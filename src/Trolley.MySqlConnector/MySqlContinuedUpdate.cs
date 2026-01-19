using MySqlConnector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public class MySqlContinuedUpdate<TEntity> : ContinuedUpdate<TEntity>, IMySqlContinuedUpdate<TEntity>
{
    #region Properties
    public MySqlUpdateVisitor DialectVisitor { get; private set; }
    public IOrmProvider OrmProvider => this.Visitor.OrmProvider;
    #endregion

    #region Constructor
    public MySqlContinuedUpdate(DbContext dbContext, IUpdateVisitor visitor) : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as MySqlUpdateVisitor;
    }
    #endregion

    #region Set
    public new IMySqlContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public new IMySqlContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.Set(condition, fieldSelector, fieldValue) as IMySqlContinuedUpdate<TEntity>;
    #endregion

    #region SetFrom
    public new IMySqlContinuedUpdate<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public new IMySqlContinuedUpdate<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => base.SetFrom(condition, fieldSelector, valueSelector) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public new IMySqlContinuedUpdate<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => base.SetFrom(condition, fieldsAssignment) as IMySqlContinuedUpdate<TEntity>;
    #endregion

    #region IgnoreFields
    public new IMySqlContinuedUpdate<TEntity> IgnoreFields(params string[] fieldNames)
        => base.IgnoreFields(fieldNames) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.IgnoreFields(fieldsSelector) as IMySqlContinuedUpdate<TEntity>;
    #endregion

    #region OnlyFields
    public new IMySqlContinuedUpdate<TEntity> OnlyFields(params string[] fieldNames)
        => base.OnlyFields(fieldNames) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.OnlyFields(fieldsSelector) as IMySqlContinuedUpdate<TEntity>;
    #endregion

    #region Where
    public new IMySqlContinuedUpdate<TEntity> WhereBy(object whereObj)
        => this.AndBy(true, whereObj);
    public new IMySqlContinuedUpdate<TEntity> WhereBy(bool condition, object whereObj)
        => this.AndBy(condition, whereObj);
    public new IMySqlContinuedUpdate<TEntity> WhereById(object whereKey)
        => this.AndById(whereKey);
    public new IMySqlContinuedUpdate<TEntity> WhereById(bool condition, object whereKey)
        => this.AndById(condition, whereKey);
    public new IMySqlContinuedUpdate<TEntity> WhereByIds(IEnumerable whereKeys)
        => this.AndByIds(whereKeys);
    public new IMySqlContinuedUpdate<TEntity> WhereByIds(bool condition, IEnumerable whereKeys)
        => this.AndByIds(condition, whereKeys);
    public new IMySqlContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public new IMySqlContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IMySqlContinuedUpdate<TEntity>;
    #endregion

    #region And
    public new IMySqlContinuedUpdate<TEntity> AndBy(object whereObj)
        => this.AndBy(true, whereObj);
    public new IMySqlContinuedUpdate<TEntity> AndBy(bool condition, object whereObj)
        => base.AndBy(condition, whereObj) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> AndById(object whereKey)
        => this.AndById(true, whereKey);
    public new IMySqlContinuedUpdate<TEntity> AndById(bool condition, object whereKey)
        => base.AndById(condition, whereKey) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> AndByIds(IEnumerable whereKeys)
        => this.AndByIds(true, whereKeys);
    public new IMySqlContinuedUpdate<TEntity> AndByIds(bool condition, IEnumerable whereKeys)
        => base.AndByIds(condition, whereKeys) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public new IMySqlContinuedUpdate<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IMySqlContinuedUpdate<TEntity>;
    #endregion

    #region Or
    public new IMySqlContinuedUpdate<TEntity> OrBy(object whereObj)
        => this.OrBy(true, whereObj);
    public new IMySqlContinuedUpdate<TEntity> OrBy(bool condition, object whereObj)
        => base.OrBy(condition, whereObj) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> OrById(object whereKey)
        => this.OrById(true, whereKey);
    public new IMySqlContinuedUpdate<TEntity> OrById(bool condition, object whereKey)
        => base.OrById(condition, whereKey) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> OrByIds(IEnumerable whereKeys)
        => this.OrByIds(true, whereKeys);
    public new IMySqlContinuedUpdate<TEntity> OrByIds(bool condition, IEnumerable whereKeys)
        => base.OrByIds(condition, whereKeys) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> Or(Expression<Func<TEntity, bool>> predicate)
        => this.Or(true, predicate);
    public new IMySqlContinuedUpdate<TEntity> Or(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> OrPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IMySqlContinuedUpdate<TEntity>;
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var updateObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.DialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var mySqlConnection = connection.BaseConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.BaseTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    var tableId = $"{Guid.NewGuid():N}";
                    var pkColumns = memberMappers.Where(f => f.IsKey).Select(f => this.OrmProvider.GetFieldName(f.FieldName)).ToList();
                    var pkColumnSql = string.Join(",", pkColumns);
                    var builder = new StringBuilder();

                    foreach (var memberMapper in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        builder.Append($"{fieldName} {memberMapper.DbColumnType}");
                        if (memberMapper.IsKey)
                            builder.Append(" NOT NULL");
                        builder.AppendLine(",");
                    }
                    builder.AppendLine($"PRIMARY KEY({pkColumnSql})");
                    builder.AppendLine(");");
                    var createFieldsSql = builder.ToString();

                    builder.Clear();
                    for (int i = 0; i < pkColumns.Count; i++)
                    {
                        if (i > 0) builder.Append(" AND ");
                        builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                    }
                    builder.Append(" SET ");
                    for (int i = 0; i < memberMappers.Count; i++)
                    {
                        var memberMapper = memberMappers[i];
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        if (memberMapper.IsKey) continue;
                        if (i > 0) builder.Append(',');
                        builder.Append($"a.{fieldName}=b.{fieldName}");
                    }
                    var updateFieldsSql = builder.ToString();

                    //添加临时表
                    void BuildCreateSql(string tableName)
                    {
                        builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName(tableName)}(");
                        builder.AppendLine(createFieldsSql);
                    }
                    void BuildUpdateSql(List<string> pkColumns, string target, string source)
                    {
                        builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a INNER JOIN {this.OrmProvider.GetTableName(source)} b ON ");
                        builder.Append(updateFieldsSql);
                        builder.Append($";DROP TABLE {this.OrmProvider.GetTableName(target)};");
                    }
                    void Execute(string sql)
                    {
                        command.CommandText = sql;
                        command.ExecuteNonQuery(CommandSqlType.BulkCopyUpdate);
                    }

                    connection.Open();
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var myTableName = $"{tableName}_{tableId}";
                            BuildCreateSql(myTableName);
                        }
                        Execute(builder.ToString());
                        builder.Clear();
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var myTableName = $"{tableName}_{tableId}";
                            bulkCopyObj.DestinationTableName = myTableName;
                            var data = this.Visitor.ToDataTable(myTableName, tabledUpdateObjs[tableName], memberMappers, valueGetters);
                            result += dialectOrmProvider.ExecuteBulkCopy(myTableName, bulkCopyObj, connection, this.DbContext, data);
                            BuildUpdateSql(pkColumns, tableName, myTableName);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var myTableName = $"{shardingTables as string}_{tableId}";
                        BuildCreateSql(myTableName);
                        Execute(builder.ToString());
                        var data = this.Visitor.ToDataTable(myTableName, updateObjs, memberMappers, valueGetters);
                        result = dialectOrmProvider.ExecuteBulkCopy(myTableName, bulkCopyObj, connection, this.DbContext, data);
                        BuildUpdateSql(pkColumns, tableName, myTableName);
                    }
                    Execute(builder.ToString());
                    builder.Clear();
                    break;
                }
            case ActionMode.Bulk:
                {
                    (var shardingType, var shardingTables, var updateObjs, var bulkCount,
                        var fixedSqlSetter, var loopSqlSetter, var readerFields) = this.Visitor.BuildWithBulk(command);

                    int index = 0;
                    var builder = new StringBuilder();
                    void TabledExecute(string tableName, IEnumerable updateObjs)
                    {
                        foreach (var updateObj in updateObjs)
                        {
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                            index++;
                            if (index >= bulkCount)
                            {
                                builder.Remove(builder.Length - 1, 1);
                                command.CommandText = builder.ToString();
                                result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                                builder.Clear();
                                command.Parameters.Clear();
                                fixedSqlSetter.Invoke(command.Parameters);
                                index = 0;
                            }
                        }
                    }
                    connection.Open();
                    switch (shardingType)
                    {
                        case ShardingTableType.SplitTables:
                            var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                            foreach (var tableName in tabledUpdateObjs.Keys)
                            {
                                fixedSqlSetter.Invoke(command.Parameters);
                                var tableParameters = tabledUpdateObjs[tableName];
                                TabledExecute(tableName, tableParameters);
                            }
                            break;
                        case ShardingTableType.MultiTable:
                            var tableNames = shardingTables as List<string>;
                            foreach (var tableName in tableNames)
                            {
                                fixedSqlSetter.Invoke(command.Parameters);
                                TabledExecute(tableName, updateObjs);
                            }

                            break;
                        default:
                            fixedSqlSetter.Invoke(command.Parameters);
                            TabledExecute(shardingTables as string, updateObjs);
                            break;
                    }
                    if (index > 0)
                    {
                        builder.Remove(builder.Length - 1, 1);
                        command.CommandText = builder.ToString();
                        result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                    }
                    builder.Clear();
                    break;
                }
            default:
                if (!this.Visitor.HasWhere)
                    throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

                command.CommandText = this.Visitor.BuildSql(command, out _);
                connection.Open();
                result = command.ExecuteNonQuery(CommandSqlType.Update);
                break;
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        this.Visitor.Dispose();
        return result;
    }
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var updateObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.DialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var mySqlConnection = connection.BaseConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.BaseTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    var tableId = $"{Guid.NewGuid():N}";
                    var pkColumns = memberMappers.Where(f => f.IsKey).Select(f => this.OrmProvider.GetFieldName(f.FieldName)).ToList();
                    var pkColumnSql = string.Join(",", pkColumns);
                    var builder = new StringBuilder();

                    foreach (var memberMapper in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        builder.Append($"{fieldName} {memberMapper.DbColumnType}");
                        if (memberMapper.IsKey)
                            builder.Append(" NOT NULL");
                        builder.AppendLine(",");
                    }
                    builder.AppendLine($"PRIMARY KEY({pkColumnSql})");
                    builder.AppendLine(");");
                    var createFieldsSql = builder.ToString();

                    builder.Clear();
                    for (int i = 0; i < pkColumns.Count; i++)
                    {
                        if (i > 0) builder.Append(" AND ");
                        builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                    }
                    builder.Append(" SET ");
                    for (int i = 0; i < memberMappers.Count; i++)
                    {
                        var memberMapper = memberMappers[i];
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        if (memberMapper.IsKey) continue;
                        if (i > 0) builder.Append(',');
                        builder.Append($"a.{fieldName}=b.{fieldName}");
                    }
                    var updateFieldsSql = builder.ToString();

                    //添加临时表
                    void BuildCreateSql(string tableName)
                    {
                        builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName(tableName)}(");
                        builder.AppendLine(createFieldsSql);
                    }
                    void BuildUpdateSql(List<string> pkColumns, string target, string source)
                    {
                        builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a INNER JOIN {this.OrmProvider.GetTableName(source)} b ON ");
                        builder.Append(updateFieldsSql);
                        builder.Append($";DROP TABLE {this.OrmProvider.GetTableName(target)};");
                    }
                    async Task Execute(string sql, CancellationToken cancellationToken)
                    {
                        command.CommandText = sql;
                        await command.ExecuteNonQueryAsync(CommandSqlType.BulkCopyUpdate, cancellationToken);
                    }

                    await connection.OpenAsync(cancellationToken);
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var myTableName = $"{tableName}_{tableId}";
                            BuildCreateSql(myTableName);
                        }
                        await Execute(builder.ToString(), cancellationToken);
                        builder.Clear();
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var myTableName = $"{tableName}_{tableId}";
                            bulkCopyObj.DestinationTableName = myTableName;
                            var data = this.Visitor.ToDataTable(myTableName, tabledUpdateObjs[tableName], memberMappers, valueGetters);
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(myTableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                            BuildUpdateSql(pkColumns, tableName, myTableName);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var myTableName = $"{shardingTables as string}_{tableId}";
                        BuildCreateSql(myTableName);
                        await Execute(builder.ToString(), cancellationToken);
                        var data = this.Visitor.ToDataTable(myTableName, updateObjs, memberMappers, valueGetters);
                        result = await dialectOrmProvider.ExecuteBulkCopyAsync(myTableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                        BuildUpdateSql(pkColumns, tableName, myTableName);
                    }
                    await Execute(builder.ToString(), cancellationToken);
                    builder.Clear();
                    break;
                }
            case ActionMode.Bulk:
                {
                    (var shardingType, var shardingTables, var updateObjs, var bulkCount,
                         var fixedSqlSetter, var loopSqlSetter, var readerFields) = this.Visitor.BuildWithBulk(command);

                    int index = 0;
                    var builder = new StringBuilder();
                    async Task TabledExecute(string tableName, IEnumerable updateObjs, CancellationToken cancellationToken)
                    {
                        foreach (var updateObj in updateObjs)
                        {
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                            index++;
                            if (index >= bulkCount)
                            {
                                builder.Remove(builder.Length - 1, 1);
                                command.CommandText = builder.ToString();
                                result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                                builder.Clear();
                                command.Parameters.Clear();
                                fixedSqlSetter.Invoke(command.Parameters);
                                index = 0;
                            }
                        }
                    }
                    await connection.OpenAsync(cancellationToken);
                    switch (shardingType)
                    {
                        case ShardingTableType.SplitTables:
                            var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                            foreach (var tableName in tabledUpdateObjs.Keys)
                            {
                                fixedSqlSetter.Invoke(command.Parameters);
                                var tableParameters = tabledUpdateObjs[tableName];
                                await TabledExecute(tableName, tableParameters, cancellationToken);
                            }
                            break;
                        case ShardingTableType.MultiTable:
                            var tableNames = shardingTables as List<string>;
                            foreach (var tableName in tableNames)
                            {
                                fixedSqlSetter.Invoke(command.Parameters);
                                await TabledExecute(tableName, updateObjs, cancellationToken);
                            }
                            break;
                        default:
                            fixedSqlSetter.Invoke(command.Parameters);
                            await TabledExecute(shardingTables as string, updateObjs, cancellationToken);
                            break;
                    }
                    if (index > 0)
                    {
                        builder.Remove(builder.Length - 1, 1);
                        command.CommandText = builder.ToString();
                        result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                    }
                    builder.Clear();
                    break;
                }
            default:
                if (!this.Visitor.HasWhere)
                    throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

                command.CommandText = this.Visitor.BuildSql(command, out _);
                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(CommandSqlType.Update, cancellationToken);
                break;
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        this.Visitor.Dispose();
        return result;
    }
    #endregion  
}
public class MySqlBulkContinuedUpdate<TEntity> : BulkContinuedUpdate<TEntity>, IMySqlBulkContinuedUpdate<TEntity>
{
    #region Properties
    public MySqlUpdateVisitor DialectVisitor { get; private set; }
    public IOrmProvider OrmProvider => this.Visitor.OrmProvider;
    #endregion

    #region Constructor
    public MySqlBulkContinuedUpdate(DbContext dbContext, IUpdateVisitor visitor) : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as MySqlUpdateVisitor;
    }
    #endregion

    #region Set
    public new IMySqlBulkContinuedUpdate<TEntity> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public new IMySqlBulkContinuedUpdate<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
        => base.Set(condition, updateObj) as IMySqlBulkContinuedUpdate<TEntity>;
    public new IMySqlBulkContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public new IMySqlBulkContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.Set(condition, fieldSelector, fieldValue) as IMySqlBulkContinuedUpdate<TEntity>;
    public new IMySqlBulkContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public new IMySqlBulkContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
        => base.Set(condition, fieldsAssignment) as IMySqlBulkContinuedUpdate<TEntity>;
    #endregion   

    #region IgnoreFields
    public new IMySqlBulkContinuedUpdate<TEntity> IgnoreFields(params string[] fieldNames)
        => base.IgnoreFields(fieldNames) as IMySqlBulkContinuedUpdate<TEntity>;
    public new IMySqlBulkContinuedUpdate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.IgnoreFields(fieldsSelector) as IMySqlBulkContinuedUpdate<TEntity>;
    #endregion

    #region OnlyFields
    public new IMySqlBulkContinuedUpdate<TEntity> OnlyFields(params string[] fieldNames)
        => base.OnlyFields(fieldNames) as IMySqlBulkContinuedUpdate<TEntity>;
    public new IMySqlBulkContinuedUpdate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.OnlyFields(fieldsSelector) as IMySqlBulkContinuedUpdate<TEntity>;
    #endregion

    #region Where   
    public new IMySqlBulkContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public new IMySqlBulkContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IMySqlBulkContinuedUpdate<TEntity>;
    public new IMySqlBulkContinuedUpdate<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IMySqlBulkContinuedUpdate<TEntity>;
    #endregion

    #region And   
    public new IMySqlBulkContinuedUpdate<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public new IMySqlBulkContinuedUpdate<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IMySqlBulkContinuedUpdate<TEntity>;
    public new IMySqlBulkContinuedUpdate<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IMySqlBulkContinuedUpdate<TEntity>;
    #endregion

    #region Or
    public new IMySqlBulkContinuedUpdate<TEntity> Or(Expression<Func<TEntity, bool>> predicate)
        => this.Or(true, predicate);
    public new IMySqlBulkContinuedUpdate<TEntity> Or(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IMySqlBulkContinuedUpdate<TEntity>;
    public new IMySqlBulkContinuedUpdate<TEntity> OrPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IMySqlBulkContinuedUpdate<TEntity>;
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.DialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var mySqlConnection = connection.BaseConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.BaseTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    var tableId = $"{Guid.NewGuid():N}";
                    var pkColumns = memberMappers.Where(f => f.IsKey).Select(f => this.OrmProvider.GetFieldName(f.FieldName)).ToList();
                    var pkColumnSql = string.Join(",", pkColumns);
                    var builder = new StringBuilder();

                    foreach (var memberMapper in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        builder.Append($"{fieldName} {memberMapper.DbColumnType}");
                        if (memberMapper.IsKey)
                            builder.Append(" NOT NULL");
                        builder.AppendLine(",");
                    }
                    builder.AppendLine($"PRIMARY KEY({pkColumnSql})");
                    builder.AppendLine(");");
                    var createFieldsSql = builder.ToString();

                    builder.Clear();
                    for (int i = 0; i < pkColumns.Count; i++)
                    {
                        if (i > 0) builder.Append(" AND ");
                        builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                    }
                    builder.Append(" SET ");
                    for (int i = 0; i < memberMappers.Count; i++)
                    {
                        var memberMapper = memberMappers[i];
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        if (memberMapper.IsKey) continue;
                        if (i > 0) builder.Append(',');
                        builder.Append($"a.{fieldName}=b.{fieldName}");
                    }
                    var updateFieldsSql = builder.ToString();

                    //添加临时表
                    void BuildCreateSql(string tableName)
                    {
                        builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName(tableName)}(");
                        builder.AppendLine(createFieldsSql);
                    }
                    void BuildUpdateSql(List<string> pkColumns, string target, string source)
                    {
                        builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a INNER JOIN {this.OrmProvider.GetTableName(source)} b ON ");
                        builder.Append(updateFieldsSql);
                        builder.Append($";DROP TABLE {this.OrmProvider.GetTableName(target)};");
                    }
                    void Execute(string sql)
                    {
                        command.CommandText = sql;
                        command.ExecuteNonQuery(CommandSqlType.BulkCopyUpdate);
                    }

                    connection.Open();
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var myTableName = $"{tableName}_{tableId}";
                            BuildCreateSql(myTableName);
                        }
                        Execute(builder.ToString());
                        builder.Clear();
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var myTableName = $"{tableName}_{tableId}";
                            bulkCopyObj.DestinationTableName = myTableName;
                            var data = this.Visitor.ToDataTable(myTableName, tabledUpdateObjs[tableName], memberMappers, valueGetters);
                            result += dialectOrmProvider.ExecuteBulkCopy(myTableName, bulkCopyObj, connection, this.DbContext, data);
                            BuildUpdateSql(pkColumns, tableName, myTableName);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var myTableName = $"{shardingTables as string}_{tableId}";
                        BuildCreateSql(myTableName);
                        Execute(builder.ToString());
                        var data = this.Visitor.ToDataTable(myTableName, insertObjs, memberMappers, valueGetters);
                        result = dialectOrmProvider.ExecuteBulkCopy(myTableName, bulkCopyObj, connection, this.DbContext, data);
                        BuildUpdateSql(pkColumns, tableName, myTableName);
                    }
                    Execute(builder.ToString());
                    builder.Clear();
                    break;
                }
            case ActionMode.Bulk:
                {
                    (var shardingType, var shardingTables, var updateObjs, var bulkCount,
                        var fixedSqlSetter, var loopSqlSetter, var readerFields) = this.Visitor.BuildWithBulk(command);

                    int index = 0;
                    var builder = new StringBuilder();
                    void TabledExecute(string tableName, IEnumerable updateObjs)
                    {
                        foreach (var updateObj in updateObjs)
                        {
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                            index++;
                            if (index >= bulkCount)
                            {
                                builder.Remove(builder.Length - 1, 1);
                                command.CommandText = builder.ToString();
                                result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                                builder.Clear();
                                command.Parameters.Clear();
                                fixedSqlSetter.Invoke(command.Parameters);
                                index = 0;
                            }
                        }
                    }
                    connection.Open();
                    switch (shardingType)
                    {
                        case ShardingTableType.SplitTables:
                            var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                            foreach (var tableName in tabledUpdateObjs.Keys)
                            {
                                fixedSqlSetter.Invoke(command.Parameters);
                                var tableParameters = tabledUpdateObjs[tableName];
                                TabledExecute(tableName, tableParameters);
                            }
                            break;
                        case ShardingTableType.MultiTable:
                            var tableNames = shardingTables as List<string>;
                            foreach (var tableName in tableNames)
                            {
                                fixedSqlSetter.Invoke(command.Parameters);
                                TabledExecute(tableName, updateObjs);
                            }

                            break;
                        default:
                            fixedSqlSetter.Invoke(command.Parameters);
                            TabledExecute(shardingTables as string, updateObjs);
                            break;
                    }
                    if (index > 0)
                    {
                        builder.Remove(builder.Length - 1, 1);
                        command.CommandText = builder.ToString();
                        result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                    }
                    builder.Clear();
                    break;
                }
            default:
                if (!this.Visitor.HasWhere)
                    throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

                command.CommandText = this.Visitor.BuildSql(command, out _);
                connection.Open();
                result = command.ExecuteNonQuery(CommandSqlType.Update);
                break;
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        this.Visitor.Dispose();
        return result;
    }
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.DialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var mySqlConnection = connection.BaseConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.BaseTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    var tableId = $"{Guid.NewGuid():N}";
                    var pkColumns = memberMappers.Where(f => f.IsKey).Select(f => this.OrmProvider.GetFieldName(f.FieldName)).ToList();
                    var pkColumnSql = string.Join(",", pkColumns);
                    var builder = new StringBuilder();

                    foreach (var memberMapper in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        builder.Append($"{fieldName} {memberMapper.DbColumnType}");
                        if (memberMapper.IsKey)
                            builder.Append(" NOT NULL");
                        builder.AppendLine(",");
                    }
                    builder.AppendLine($"PRIMARY KEY({pkColumnSql})");
                    builder.AppendLine(");");
                    var createFieldsSql = builder.ToString();

                    builder.Clear();
                    for (int i = 0; i < pkColumns.Count; i++)
                    {
                        if (i > 0) builder.Append(" AND ");
                        builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                    }
                    builder.Append(" SET ");
                    for (int i = 0; i < memberMappers.Count; i++)
                    {
                        var memberMapper = memberMappers[i];
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        if (memberMapper.IsKey) continue;
                        if (i > 0) builder.Append(',');
                        builder.Append($"a.{fieldName}=b.{fieldName}");
                    }
                    var updateFieldsSql = builder.ToString();

                    //添加临时表
                    void BuildCreateSql(string tableName)
                    {
                        builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName(tableName)}(");
                        builder.AppendLine(createFieldsSql);
                    }
                    void BuildUpdateSql(List<string> pkColumns, string target, string source)
                    {
                        builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a INNER JOIN {this.OrmProvider.GetTableName(source)} b ON ");
                        builder.Append(updateFieldsSql);
                        builder.Append($";DROP TABLE {this.OrmProvider.GetTableName(target)};");
                    }
                    async Task Execute(string sql, CancellationToken cancellationToken)
                    {
                        command.CommandText = sql;
                        await command.ExecuteNonQueryAsync(CommandSqlType.BulkCopyUpdate, cancellationToken);
                    }

                    await connection.OpenAsync(cancellationToken);
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var myTableName = $"{tableName}_{tableId}";
                            BuildCreateSql(myTableName);
                        }
                        await Execute(builder.ToString(), cancellationToken);
                        builder.Clear();
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var myTableName = $"{tableName}_{tableId}";
                            bulkCopyObj.DestinationTableName = myTableName;
                            var data = this.Visitor.ToDataTable(myTableName, tabledUpdateObjs[tableName], memberMappers, valueGetters);
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(myTableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                            BuildUpdateSql(pkColumns, tableName, myTableName);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var myTableName = $"{shardingTables as string}_{tableId}";
                        BuildCreateSql(myTableName);
                        await Execute(builder.ToString(), cancellationToken);
                        var data = this.Visitor.ToDataTable(myTableName, insertObjs, memberMappers, valueGetters);
                        result = await dialectOrmProvider.ExecuteBulkCopyAsync(myTableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                        BuildUpdateSql(pkColumns, tableName, myTableName);
                    }
                    await Execute(builder.ToString(), cancellationToken);
                    builder.Clear();
                    break;
                }
            case ActionMode.Bulk:
                {
                    (var shardingType, var shardingTables, var updateObjs, var bulkCount,
                         var fixedSqlSetter, var loopSqlSetter, var readerFields) = this.Visitor.BuildWithBulk(command);

                    int index = 0;
                    var builder = new StringBuilder();
                    async Task TabledExecute(string tableName, IEnumerable updateObjs, CancellationToken cancellationToken)
                    {
                        foreach (var updateObj in updateObjs)
                        {
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                            index++;
                            if (index >= bulkCount)
                            {
                                builder.Remove(builder.Length - 1, 1);
                                command.CommandText = builder.ToString();
                                result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                                builder.Clear();
                                command.Parameters.Clear();
                                fixedSqlSetter.Invoke(command.Parameters);
                                index = 0;
                            }
                        }
                    }
                    await connection.OpenAsync(cancellationToken);
                    switch (shardingType)
                    {
                        case ShardingTableType.SplitTables:
                            var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                            foreach (var tableName in tabledUpdateObjs.Keys)
                            {
                                fixedSqlSetter.Invoke(command.Parameters);
                                var tableParameters = tabledUpdateObjs[tableName];
                                await TabledExecute(tableName, tableParameters, cancellationToken);
                            }
                            break;
                        case ShardingTableType.MultiTable:
                            var tableNames = shardingTables as List<string>;
                            foreach (var tableName in tableNames)
                            {
                                fixedSqlSetter.Invoke(command.Parameters);
                                await TabledExecute(tableName, updateObjs, cancellationToken);
                            }
                            break;
                        default:
                            fixedSqlSetter.Invoke(command.Parameters);
                            await TabledExecute(shardingTables as string, updateObjs, cancellationToken);
                            break;
                    }
                    if (index > 0)
                    {
                        builder.Remove(builder.Length - 1, 1);
                        command.CommandText = builder.ToString();
                        result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                    }
                    builder.Clear();
                    break;
                }
            default:
                if (!this.Visitor.HasWhere)
                    throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

                command.CommandText = this.Visitor.BuildSql(command, out _);
                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(CommandSqlType.Update, cancellationToken);
                break;
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        this.Visitor.Dispose();
        return result;
    }
    #endregion
}
public class MySqlBulkCopyContinuedUpdate<TEntity> : BulkCopyContinuedUpdate<TEntity>, IMySqlBulkCopyContinuedUpdate<TEntity>
{
    #region Properties
    public MySqlUpdateVisitor DialectVisitor { get; private set; }
    public IOrmProvider OrmProvider => this.Visitor.OrmProvider;
    #endregion

    #region Constructor
    public MySqlBulkCopyContinuedUpdate(DbContext dbContext, IUpdateVisitor visitor) : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as MySqlUpdateVisitor;
    }
    #endregion

    #region Where
    public new IMySqlBulkCopyContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public new IMySqlBulkCopyContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IMySqlBulkCopyContinuedUpdate<TEntity>;
    public new IMySqlBulkCopyContinuedUpdate<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IMySqlBulkCopyContinuedUpdate<TEntity>;
    #endregion

    #region And
    public new IMySqlBulkCopyContinuedUpdate<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public new IMySqlBulkCopyContinuedUpdate<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IMySqlBulkCopyContinuedUpdate<TEntity>;
    public new IMySqlBulkCopyContinuedUpdate<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IMySqlBulkCopyContinuedUpdate<TEntity>;
    #endregion

    #region Or
    public new IMySqlBulkCopyContinuedUpdate<TEntity> Or(Expression<Func<TEntity, bool>> predicate)
        => this.Or(true, predicate);
    public new IMySqlBulkCopyContinuedUpdate<TEntity> Or(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Or(condition, ifPredicate, elsePredicate) as IMySqlBulkCopyContinuedUpdate<TEntity>;
    public new IMySqlBulkCopyContinuedUpdate<TEntity> OrPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.OrPredicate(predicateInitializer) as IMySqlBulkCopyContinuedUpdate<TEntity>;
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.DialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var mySqlConnection = connection.BaseConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.BaseTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    var tableId = $"{Guid.NewGuid():N}";
                    var pkColumns = memberMappers.Where(f => f.IsKey).Select(f => this.OrmProvider.GetFieldName(f.FieldName)).ToList();
                    var pkColumnSql = string.Join(",", pkColumns);
                    var builder = new StringBuilder();

                    foreach (var memberMapper in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        builder.Append($"{fieldName} {memberMapper.DbColumnType}");
                        if (memberMapper.IsKey)
                            builder.Append(" NOT NULL");
                        builder.AppendLine(",");
                    }
                    builder.AppendLine($"PRIMARY KEY({pkColumnSql})");
                    builder.AppendLine(");");
                    var createFieldsSql = builder.ToString();

                    builder.Clear();
                    for (int i = 0; i < pkColumns.Count; i++)
                    {
                        if (i > 0) builder.Append(" AND ");
                        builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                    }
                    builder.Append(" SET ");
                    for (int i = 0; i < memberMappers.Count; i++)
                    {
                        var memberMapper = memberMappers[i];
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        if (memberMapper.IsKey) continue;
                        if (i > 0) builder.Append(',');
                        builder.Append($"a.{fieldName}=b.{fieldName}");
                    }
                    var updateFieldsSql = builder.ToString();

                    //添加临时表
                    void BuildCreateSql(string tableName)
                    {
                        builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName(tableName)}(");
                        builder.AppendLine(createFieldsSql);
                    }
                    void BuildUpdateSql(List<string> pkColumns, string target, string source)
                    {
                        builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a INNER JOIN {this.OrmProvider.GetTableName(source)} b ON ");
                        builder.Append(updateFieldsSql);
                        builder.Append($";DROP TABLE {this.OrmProvider.GetTableName(target)};");
                    }
                    void Execute(string sql)
                    {
                        command.CommandText = sql;
                        command.ExecuteNonQuery(CommandSqlType.BulkCopyUpdate);
                    }

                    connection.Open();
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var myTableName = $"{tableName}_{tableId}";
                            BuildCreateSql(myTableName);
                        }
                        Execute(builder.ToString());
                        builder.Clear();
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var myTableName = $"{tableName}_{tableId}";
                            bulkCopyObj.DestinationTableName = myTableName;
                            var data = this.Visitor.ToDataTable(myTableName, tabledUpdateObjs[tableName], memberMappers, valueGetters);
                            result += dialectOrmProvider.ExecuteBulkCopy(myTableName, bulkCopyObj, connection, this.DbContext, data);
                            BuildUpdateSql(pkColumns, tableName, myTableName);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var myTableName = $"{shardingTables as string}_{tableId}";
                        BuildCreateSql(myTableName);
                        Execute(builder.ToString());
                        var data = this.Visitor.ToDataTable(myTableName, insertObjs, memberMappers, valueGetters);
                        result = dialectOrmProvider.ExecuteBulkCopy(myTableName, bulkCopyObj, connection, this.DbContext, data);
                        BuildUpdateSql(pkColumns, tableName, myTableName);
                    }
                    Execute(builder.ToString());
                    builder.Clear();
                    break;
                }
            case ActionMode.Bulk:
                {
                    (var shardingType, var shardingTables, var updateObjs, var bulkCount,
                        var fixedSqlSetter, var loopSqlSetter, var readerFields) = this.Visitor.BuildWithBulk(command);

                    int index = 0;
                    var builder = new StringBuilder();
                    void TabledExecute(string tableName, IEnumerable updateObjs)
                    {
                        foreach (var updateObj in updateObjs)
                        {
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                            index++;
                            if (index >= bulkCount)
                            {
                                builder.Remove(builder.Length - 1, 1);
                                command.CommandText = builder.ToString();
                                result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                                builder.Clear();
                                command.Parameters.Clear();
                                fixedSqlSetter.Invoke(command.Parameters);
                                index = 0;
                            }
                        }
                    }
                    connection.Open();
                    switch (shardingType)
                    {
                        case ShardingTableType.SplitTables:
                            var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                            foreach (var tableName in tabledUpdateObjs.Keys)
                            {
                                fixedSqlSetter.Invoke(command.Parameters);
                                var tableParameters = tabledUpdateObjs[tableName];
                                TabledExecute(tableName, tableParameters);
                            }
                            break;
                        case ShardingTableType.MultiTable:
                            var tableNames = shardingTables as List<string>;
                            foreach (var tableName in tableNames)
                            {
                                fixedSqlSetter.Invoke(command.Parameters);
                                TabledExecute(tableName, updateObjs);
                            }

                            break;
                        default:
                            fixedSqlSetter.Invoke(command.Parameters);
                            TabledExecute(shardingTables as string, updateObjs);
                            break;
                    }
                    if (index > 0)
                    {
                        builder.Remove(builder.Length - 1, 1);
                        command.CommandText = builder.ToString();
                        result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                    }
                    builder.Clear();
                    break;
                }
            default:
                if (!this.Visitor.HasWhere)
                    throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

                command.CommandText = this.Visitor.BuildSql(command, out _);
                connection.Open();
                result = command.ExecuteNonQuery(CommandSqlType.Update);
                break;
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        this.Visitor.Dispose();
        return result;
    }
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.BulkCopy:
                {
                    (var shardingType, var shardingTables, var insertObjs, var timeoutSeconds,
                        var memberMappers, var valueGetters) = this.DialectVisitor.BuildWithBulkCopy();
                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var mySqlConnection = connection.BaseConnection as MySqlConnection;
                    var mySqlTransaction = this.DbContext.Transaction?.BaseTransaction as MySqlTransaction;
                    var bulkCopyObj = new MySqlBulkCopy(mySqlConnection, mySqlTransaction);
                    if (timeoutSeconds.HasValue)
                        bulkCopyObj.BulkCopyTimeout = timeoutSeconds.Value;

                    var tableId = $"{Guid.NewGuid():N}";
                    var pkColumns = memberMappers.Where(f => f.IsKey).Select(f => this.OrmProvider.GetFieldName(f.FieldName)).ToList();
                    var pkColumnSql = string.Join(",", pkColumns);
                    var builder = new StringBuilder();

                    foreach (var memberMapper in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        builder.Append($"{fieldName} {memberMapper.DbColumnType}");
                        if (memberMapper.IsKey)
                            builder.Append(" NOT NULL");
                        builder.AppendLine(",");
                    }
                    builder.AppendLine($"PRIMARY KEY({pkColumnSql})");
                    builder.AppendLine(");");
                    var createFieldsSql = builder.ToString();

                    builder.Clear();
                    for (int i = 0; i < pkColumns.Count; i++)
                    {
                        if (i > 0) builder.Append(" AND ");
                        builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                    }
                    builder.Append(" SET ");
                    for (int i = 0; i < memberMappers.Count; i++)
                    {
                        var memberMapper = memberMappers[i];
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        if (memberMapper.IsKey) continue;
                        if (i > 0) builder.Append(',');
                        builder.Append($"a.{fieldName}=b.{fieldName}");
                    }
                    var updateFieldsSql = builder.ToString();

                    //添加临时表
                    void BuildCreateSql(string tableName)
                    {
                        builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName(tableName)}(");
                        builder.AppendLine(createFieldsSql);
                    }
                    void BuildUpdateSql(List<string> pkColumns, string target, string source)
                    {
                        builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a INNER JOIN {this.OrmProvider.GetTableName(source)} b ON ");
                        builder.Append(updateFieldsSql);
                        builder.Append($";DROP TABLE {this.OrmProvider.GetTableName(target)};");
                    }
                    async Task Execute(string sql, CancellationToken cancellationToken)
                    {
                        command.CommandText = sql;
                        await command.ExecuteNonQueryAsync(CommandSqlType.BulkCopyUpdate, cancellationToken);
                    }

                    await connection.OpenAsync(cancellationToken);
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var myTableName = $"{tableName}_{tableId}";
                            BuildCreateSql(myTableName);
                        }
                        await Execute(builder.ToString(), cancellationToken);
                        builder.Clear();
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var myTableName = $"{tableName}_{tableId}";
                            bulkCopyObj.DestinationTableName = myTableName;
                            var data = this.Visitor.ToDataTable(myTableName, tabledUpdateObjs[tableName], memberMappers, valueGetters);
                            result += await dialectOrmProvider.ExecuteBulkCopyAsync(myTableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                            BuildUpdateSql(pkColumns, tableName, myTableName);
                        }
                    }
                    else
                    {
                        var tableName = shardingTables as string;
                        var myTableName = $"{shardingTables as string}_{tableId}";
                        BuildCreateSql(myTableName);
                        await Execute(builder.ToString(), cancellationToken);
                        var data = this.Visitor.ToDataTable(myTableName, insertObjs, memberMappers, valueGetters);
                        result = await dialectOrmProvider.ExecuteBulkCopyAsync(myTableName, bulkCopyObj, connection, this.DbContext, data, cancellationToken);
                        BuildUpdateSql(pkColumns, tableName, myTableName);
                    }
                    await Execute(builder.ToString(), cancellationToken);
                    builder.Clear();
                    break;
                }
            case ActionMode.Bulk:
                {
                    (var shardingType, var shardingTables, var updateObjs, var bulkCount,
                         var fixedSqlSetter, var loopSqlSetter, var readerFields) = this.Visitor.BuildWithBulk(command);

                    int index = 0;
                    var builder = new StringBuilder();
                    async Task TabledExecute(string tableName, IEnumerable updateObjs, CancellationToken cancellationToken)
                    {
                        foreach (var updateObj in updateObjs)
                        {
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                            index++;
                            if (index >= bulkCount)
                            {
                                builder.Remove(builder.Length - 1, 1);
                                command.CommandText = builder.ToString();
                                result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                                builder.Clear();
                                command.Parameters.Clear();
                                fixedSqlSetter.Invoke(command.Parameters);
                                index = 0;
                            }
                        }
                    }
                    await connection.OpenAsync(cancellationToken);
                    switch (shardingType)
                    {
                        case ShardingTableType.SplitTables:
                            var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                            foreach (var tableName in tabledUpdateObjs.Keys)
                            {
                                fixedSqlSetter.Invoke(command.Parameters);
                                var tableParameters = tabledUpdateObjs[tableName];
                                await TabledExecute(tableName, tableParameters, cancellationToken);
                            }
                            break;
                        case ShardingTableType.MultiTable:
                            var tableNames = shardingTables as List<string>;
                            foreach (var tableName in tableNames)
                            {
                                fixedSqlSetter.Invoke(command.Parameters);
                                await TabledExecute(tableName, updateObjs, cancellationToken);
                            }
                            break;
                        default:
                            fixedSqlSetter.Invoke(command.Parameters);
                            await TabledExecute(shardingTables as string, updateObjs, cancellationToken);
                            break;
                    }
                    if (index > 0)
                    {
                        builder.Remove(builder.Length - 1, 1);
                        command.CommandText = builder.ToString();
                        result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                    }
                    builder.Clear();
                    break;
                }
            default:
                if (!this.Visitor.HasWhere)
                    throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

                command.CommandText = this.Visitor.BuildSql(command, out _);
                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(CommandSqlType.Update, cancellationToken);
                break;
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        this.Visitor.Dispose();
        return result;
    }
    #endregion

    #region ToSql
    public override string ToSql(out List<IDbDataParameter> dbParameters)
    {
        string sql;
        dbParameters = null;
        var builder = new StringBuilder();
        if (this.Visitor.ActionMode == ActionMode.BulkCopy)
        {
            (var shardingType, var shardingTables, var updateObjs, var timeoutSeconds,
                var memberMappers, var valueGetters) = this.DialectVisitor.BuildWithBulkCopy();

            var tableId = $"{Guid.NewGuid():N}";
            var pkColumns = memberMappers.Where(f => f.IsKey).Select(f => this.OrmProvider.GetFieldName(f.FieldName)).ToList();
            var pkColumnSql = string.Join(",", pkColumns);

            foreach (var memberMapper in memberMappers)
            {
                var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                builder.Append($"{fieldName} {memberMapper.DbColumnType}");
                if (memberMapper.IsKey)
                    builder.Append(" NOT NULL");
                builder.AppendLine(",");
            }
            builder.AppendLine($"PRIMARY KEY({pkColumnSql})");
            builder.AppendLine(");");
            var createFieldsSql = builder.ToString();

            builder.Clear();
            for (int i = 0; i < pkColumns.Count; i++)
            {
                if (i > 0) builder.Append(" AND ");
                builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
            }
            builder.Append(" SET ");
            for (int i = 0; i < memberMappers.Count; i++)
            {
                var memberMapper = memberMappers[i];
                var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                if (memberMapper.IsKey) continue;
                if (i > 0) builder.Append(',');
                builder.Append($"a.{fieldName}=b.{fieldName}");
            }
            var updateFieldsSql = builder.ToString();

            //添加临时表
            void BuildCreateSql(string tableName)
            {
                builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName(tableName)}(");
                builder.AppendLine(createFieldsSql);
            }
            void BuildUpdateSql(List<string> pkColumns, string target, string source)
            {
                builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a INNER JOIN {this.OrmProvider.GetTableName(source)} b ON ");
                builder.Append(updateFieldsSql);
                builder.Append($";DROP TABLE {this.OrmProvider.GetTableName(target)};");
            }
            if (shardingType == ShardingTableType.SplitTables)
            {
                var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                foreach (var tableName in tabledUpdateObjs.Keys)
                {
                    var myTableName = $"{tableName}_{tableId}";
                    BuildCreateSql(myTableName);
                }
                foreach (var tableName in tabledUpdateObjs.Keys)
                {
                    var myTableName = $"{tableName}_{tableId}";
                    BuildUpdateSql(pkColumns, tableName, myTableName);
                }
            }
            else
            {
                var tableName = shardingTables as string;
                var myTableName = $"{shardingTables as string}_{tableId}";
                BuildCreateSql(myTableName);
                BuildUpdateSql(pkColumns, tableName, myTableName);
            }
            sql = builder.ToString();
        }
        else
        {
            (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
            sql = this.Visitor.BuildSql(command, out _);
            dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
            command.Dispose();
            if (isNeedClose) connection.Close();
        }
        this.Visitor.Dispose();
        builder.Clear();
        return sql;
    }
    #endregion
}