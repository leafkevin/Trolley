using System;
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
    public new IMySqlContinuedUpdate<TEntity> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public new IMySqlContinuedUpdate<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
        => base.Set(condition, updateObj) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public new IMySqlContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.Set(condition, fieldSelector, fieldValue) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public new IMySqlContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
        => base.Set(condition, fieldsAssignment) as IMySqlContinuedUpdate<TEntity>;
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
    public new IMySqlContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public new IMySqlContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.WherePredicate(predicateInitializer) as IMySqlContinuedUpdate<TEntity>;
    #endregion

    #region And
    public new IMySqlContinuedUpdate<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public new IMySqlContinuedUpdate<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => base.AndPredicate(predicateInitializer) as IMySqlContinuedUpdate<TEntity>;
    #endregion

    #region Or
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
                    (var updateObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                    Type updateObjType = null;
                    foreach (var updateObj in updateObjs)
                    {
                        updateObjType = updateObj.GetType();
                        break;
                    }
                    if (updateObjType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
                    var fromMapper = this.Visitor.Tables[0].Mapper;
                    var memberMappers = this.Visitor.GetRefMemberMappers(updateObjType, fromMapper, true);
                    var tableName = $"{fromMapper.TableName}_{Guid.NewGuid():N}";

                    //添加临时表
                    var builder = new StringBuilder();
                    builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName(tableName)}(");
                    var pkColumns = new List<string>();
                    foreach ((var refMemberMapper, _) in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                        builder.Append($"{fieldName} {refMemberMapper.DbColumnType}");
                        if (refMemberMapper.IsKey)
                        {
                            builder.Append(" NOT NULL");
                            pkColumns.Add(fieldName);
                        }
                        builder.AppendLine(",");
                    }
                    builder.AppendLine($"PRIMARY KEY({string.Join(",", pkColumns)})");
                    builder.AppendLine(");");
                    if (this.Visitor.IsNeedFetchShardingTables)
                        builder.Append(this.Visitor.BuildTableShardingsSql());
                    var bulkCopySql = builder.ToString();

                    builder.Clear();
                    void Execute(string target, string source)
                    {
                        builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a INNER JOIN {this.OrmProvider.GetTableName(source)} b ON ");
                        for (int i = 0; i < pkColumns.Count; i++)
                        {
                            if (i > 0) builder.Append(" AND ");
                            builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                        }
                        builder.Append(" SET ");
                        int setIndex = 0;
                        foreach ((var refMemberMapper, _) in memberMappers)
                        {
                            var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                            if (pkColumns.Contains(fieldName)) continue;
                            if (setIndex > 0) builder.Append(',');
                            builder.Append($"a.{fieldName}=b.{fieldName}");
                            setIndex++;
                        }
                    }
                    if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                    {
                        var tableNames = this.Visitor.ShardingTables[0].TableNames;
                        for (int i = 0; i < tableNames.Count; i++)
                        {
                            if (i > 0) builder.Append(';');
                            Execute(tableNames[i], tableName);
                        }
                    }
                    else Execute(this.Visitor.Tables[0].Body ?? fromMapper.TableName, tableName);
                    builder.Append($";DROP TABLE {this.OrmProvider.GetTableName(tableName)}");
                    var updateSql = builder.ToString();

                    command.CommandText = bulkCopySql;
                    connection.Open();
                    command.ExecuteNonQuery(CommandSqlType.BulkCopyUpdate);

                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var sqlVisitor = this.Visitor as SqlVisitor;
                    result = dialectOrmProvider.ExecuteBulkCopy(true, this.DbContext, sqlVisitor, connection, updateObjType, updateObjs, timeoutSeconds, tableName);
                    if (result == 0) updateSql = $"DROP TABLE {this.OrmProvider.GetTableName(tableName)}";
                    command.CommandText = updateSql;
                    result = command.ExecuteNonQuery(CommandSqlType.BulkCopyUpdate);
                    builder.Clear();
                }
                break;
            case ActionMode.Bulk:
                {
                    var builder = new StringBuilder();
                    (var updateObjs, var bulkCount, var tableName, var fixedParameterSetter, var firstSqlSetter, var sqlSetter, _) = this.Visitor.BuildWithBulk(command);
                    Func<int, string> suffixGetter = index => this.Visitor.IsMultiple ? $"_m{this.Visitor.CommandIndex}{index}" : $"{index}";

                    Action<object, int> sqlExecute = null;
                    if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                    {
                        sqlExecute = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            var tableNames = this.Visitor.ShardingTables[0].TableNames;
                            firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableNames[0], updateObj, suffixGetter.Invoke(index));
                            for (int i = 1; i < tableNames.Count; i++)
                            {
                                builder.Append(';');
                                sqlSetter.Invoke(builder, this.DbContext, tableNames[i], updateObj, suffixGetter.Invoke(index));
                            }
                        };
                    }
                    else
                    {
                        sqlExecute = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, suffixGetter.Invoke(index));
                        };
                    }
                    if (this.Visitor.IsNeedFetchShardingTables)
                        this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                    int index = 0;
                    fixedParameterSetter?.Invoke(command.Parameters);
                    connection.Open();
                    foreach (var updateObj in updateObjs)
                    {
                        sqlExecute.Invoke(updateObj, index);
                        index++;

                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                            command.Parameters.Clear();
                            fixedParameterSetter?.Invoke(command.Parameters);
                            builder.Clear();
                            index = 0;
                        }
                    }
                    if (index > 0)
                    {
                        command.CommandText = builder.ToString();
                        result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                    }
                    builder.Clear();
                }
                break;
            default:
                {
                    if (!this.Visitor.HasWhere)
                        throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

                    if (this.Visitor.IsNeedFetchShardingTables)
                        this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                    command.CommandText = this.Visitor.BuildCommand(this.DbContext, command, out _);
                    connection.Open();
                    result = command.ExecuteNonQuery(CommandSqlType.Update);
                }
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
                    (var updateObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                    Type updateObjType = null;
                    foreach (var updateObj in updateObjs)
                    {
                        updateObjType = updateObj.GetType();
                        break;
                    }
                    if (updateObjType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
                    var fromMapper = this.Visitor.Tables[0].Mapper;
                    var memberMappers = this.Visitor.GetRefMemberMappers(updateObjType, fromMapper, true);
                    var tableName = $"{fromMapper.TableName}_{Guid.NewGuid():N}";

                    //添加临时表
                    var builder = new StringBuilder();
                    builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName(tableName)}(");
                    var pkColumns = new List<string>();
                    foreach ((var refMemberMapper, _) in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                        builder.Append($"{fieldName} {refMemberMapper.DbColumnType}");
                        if (refMemberMapper.IsKey)
                        {
                            builder.Append(" NOT NULL");
                            pkColumns.Add(fieldName);
                        }
                        builder.AppendLine(",");
                    }
                    builder.AppendLine($"PRIMARY KEY({string.Join(",", pkColumns)})");
                    builder.AppendLine(");");
                    if (this.Visitor.IsNeedFetchShardingTables)
                        builder.Append(this.Visitor.BuildTableShardingsSql());
                    var bulkCopySql = builder.ToString();

                    builder.Clear();
                    void Execute(string target, string source)
                    {
                        builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a INNER JOIN {this.OrmProvider.GetTableName(source)} b ON ");
                        for (int i = 0; i < pkColumns.Count; i++)
                        {
                            if (i > 0) builder.Append(" AND ");
                            builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                        }
                        builder.Append(" SET ");
                        int setIndex = 0;
                        foreach ((var refMemberMapper, _) in memberMappers)
                        {
                            var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                            if (pkColumns.Contains(fieldName)) continue;
                            if (setIndex > 0) builder.Append(',');
                            builder.Append($"a.{fieldName}=b.{fieldName}");
                            setIndex++;
                        }
                    }
                    if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                    {
                        var tableNames = this.Visitor.ShardingTables[0].TableNames;
                        for (int i = 0; i < tableNames.Count; i++)
                        {
                            if (i > 0) builder.Append(';');
                            Execute(tableNames[i], tableName);
                        }
                    }
                    else Execute(this.Visitor.Tables[0].Body ?? fromMapper.TableName, tableName);
                    builder.Append($";DROP TABLE {this.OrmProvider.GetTableName(tableName)}");
                    var updateSql = builder.ToString();

                    command.CommandText = bulkCopySql;
                    await connection.OpenAsync(cancellationToken);
                    await command.ExecuteNonQueryAsync(CommandSqlType.BulkCopyUpdate, cancellationToken);

                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var sqlVisitor = this.Visitor as SqlVisitor;
                    result = await dialectOrmProvider.ExecuteBulkCopyAsync(true, this.DbContext, sqlVisitor, connection, updateObjType, updateObjs, timeoutSeconds, cancellationToken, tableName);
                    if (result == 0) updateSql = $"DROP TABLE {this.OrmProvider.GetTableName(tableName)}";
                    command.CommandText = updateSql;
                    result = await command.ExecuteNonQueryAsync(CommandSqlType.BulkCopyUpdate, cancellationToken);
                    builder.Clear();
                }
                break;
            case ActionMode.Bulk:
                {
                    var builder = new StringBuilder();
                    (var updateObjs, var bulkCount, var tableName, var fixedParameterSetter, var firstSqlSetter, var sqlSetter, _) = this.Visitor.BuildWithBulk(command);
                    Func<int, string> suffixGetter = index => this.Visitor.IsMultiple ? $"_m{this.Visitor.CommandIndex}{index}" : $"{index}";

                    Action<object, int> sqlExecute = null;
                    if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                    {
                        sqlExecute = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            var tableNames = this.Visitor.ShardingTables[0].TableNames;
                            firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableNames[0], updateObj, suffixGetter.Invoke(index));
                            for (int i = 1; i < tableNames.Count; i++)
                            {
                                builder.Append(';');
                                sqlSetter.Invoke(builder, this.DbContext, tableNames[i], updateObj, suffixGetter.Invoke(index));
                            }
                        };
                    }
                    else
                    {
                        sqlExecute = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, suffixGetter.Invoke(index));
                        };
                    }
                    if (this.Visitor.IsNeedFetchShardingTables)
                        await this.DbContext.FetchShardingTablesAsync(this.Visitor as SqlVisitor, cancellationToken);

                    int index = 0;
                    fixedParameterSetter?.Invoke(command.Parameters);
                    await connection.OpenAsync(cancellationToken);
                    foreach (var updateObj in updateObjs)
                    {
                        sqlExecute.Invoke(updateObj, index);
                        index++;

                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                            command.Parameters.Clear();
                            fixedParameterSetter?.Invoke(command.Parameters);
                            builder.Clear();
                            index = 0;
                        }
                    }
                    if (index > 0)
                    {
                        command.CommandText = builder.ToString();
                        result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                    }
                    builder.Clear();
                }
                break;
            default:
                {
                    if (!this.Visitor.HasWhere)
                        throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

                    if (this.Visitor.IsNeedFetchShardingTables)
                        this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                    command.CommandText = this.Visitor.BuildCommand(this.DbContext, command, out _);
                    await connection.OpenAsync(cancellationToken);
                    result = await command.ExecuteNonQueryAsync(CommandSqlType.Update, cancellationToken);
                }
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
            (var updateObjs, _) = this.DialectVisitor.BuildWithBulkCopy();
            Type updateObjType = null;
            foreach (var updateObj in updateObjs)
            {
                updateObjType = updateObj.GetType();
                break;
            }
            if (updateObjType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
            var fromMapper = this.Visitor.Tables[0].Mapper;
            var memberMappers = this.Visitor.GetRefMemberMappers(updateObjType, fromMapper, true);
            var tableName = $"{fromMapper.TableName}_{Guid.NewGuid():N}";

            //添加临时表
            builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName(tableName)}(");
            var pkColumns = new List<string>();
            foreach ((var refMemberMapper, _) in memberMappers)
            {
                var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                builder.Append($"{fieldName} {refMemberMapper.DbColumnType}");
                if (refMemberMapper.IsKey)
                {
                    builder.Append(" NOT NULL");
                    pkColumns.Add(fieldName);
                }
                builder.AppendLine(",");
            }
            builder.AppendLine($"PRIMARY KEY({string.Join(",", pkColumns)})");
            builder.AppendLine(");");

            if (this.Visitor.IsNeedFetchShardingTables)
            {
                builder.Append(this.Visitor.BuildTableShardingsSql());
                builder.Append(';');
            }

            void Execute(string target, string source)
            {
                builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a INNER JOIN {this.OrmProvider.GetTableName(source)} b ON ");
                for (int i = 0; i < pkColumns.Count; i++)
                {
                    if (i > 0) builder.Append(" AND ");
                    builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                }
                builder.Append(" SET ");
                int setIndex = 0;
                foreach ((var refMemberMapper, _) in memberMappers)
                {
                    var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                    if (pkColumns.Contains(fieldName)) continue;
                    if (setIndex > 0) builder.Append(',');
                    builder.Append($"a.{fieldName}=b.{fieldName}");
                    setIndex++;
                }
            }
            if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
            {
                var tableNames = this.Visitor.ShardingTables[0].TableNames;
                for (int i = 0; i < tableNames.Count; i++)
                {
                    if (i > 0) builder.Append(';');
                    Execute(tableNames[i], tableName);
                }
            }
            else Execute(this.Visitor.Tables[0].Body ?? fromMapper.TableName, tableName);
            builder.Append($";DROP TABLE {this.OrmProvider.GetTableName(tableName)}");
            sql = builder.ToString();
        }
        else
        {
            if (this.Visitor.IsNeedFetchShardingTables)
            {
                this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                builder.Append(this.Visitor.BuildTableShardingsSql());
                builder.Append(';');
            }
            (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
            sql = this.Visitor.BuildCommand(this.DbContext, command, out _);
            if (this.Visitor.IsNeedFetchShardingTables)
            {
                builder.Append(sql);
                sql = builder.ToString();
            }
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
                    (var updateObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                    Type updateObjType = null;
                    foreach (var updateObj in updateObjs)
                    {
                        updateObjType = updateObj.GetType();
                        break;
                    }
                    if (updateObjType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
                    var fromMapper = this.Visitor.Tables[0].Mapper;
                    var memberMappers = this.Visitor.GetRefMemberMappers(updateObjType, fromMapper, true);
                    var tableName = $"{fromMapper.TableName}_{Guid.NewGuid():N}";

                    //添加临时表
                    var builder = new StringBuilder();
                    builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName(tableName)}(");
                    var pkColumns = new List<string>();
                    foreach ((var refMemberMapper, _) in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                        builder.Append($"{fieldName} {refMemberMapper.DbColumnType}");
                        if (refMemberMapper.IsKey)
                        {
                            builder.Append(" NOT NULL");
                            pkColumns.Add(fieldName);
                        }
                        builder.AppendLine(",");
                    }
                    builder.AppendLine($"PRIMARY KEY({string.Join(",", pkColumns)})");
                    builder.AppendLine(");");
                    if (this.Visitor.IsNeedFetchShardingTables)
                        builder.Append(this.Visitor.BuildTableShardingsSql());
                    var bulkCopySql = builder.ToString();

                    builder.Clear();
                    void Execute(string target, string source)
                    {
                        builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a INNER JOIN {this.OrmProvider.GetTableName(source)} b ON ");
                        for (int i = 0; i < pkColumns.Count; i++)
                        {
                            if (i > 0) builder.Append(" AND ");
                            builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                        }
                        builder.Append(" SET ");
                        int setIndex = 0;
                        foreach ((var refMemberMapper, _) in memberMappers)
                        {
                            var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                            if (pkColumns.Contains(fieldName)) continue;
                            if (setIndex > 0) builder.Append(',');
                            builder.Append($"a.{fieldName}=b.{fieldName}");
                            setIndex++;
                        }
                    }
                    if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                    {
                        var tableNames = this.Visitor.ShardingTables[0].TableNames;
                        for (int i = 0; i < tableNames.Count; i++)
                        {
                            if (i > 0) builder.Append(';');
                            Execute(tableNames[i], tableName);
                        }
                    }
                    else Execute(this.Visitor.Tables[0].Body ?? fromMapper.TableName, tableName);
                    builder.Append($";DROP TABLE {this.OrmProvider.GetTableName(tableName)}");
                    var updateSql = builder.ToString();

                    command.CommandText = bulkCopySql;
                    connection.Open();
                    command.ExecuteNonQuery(CommandSqlType.BulkCopyUpdate);

                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var sqlVisitor = this.Visitor as SqlVisitor;
                    result = dialectOrmProvider.ExecuteBulkCopy(true, this.DbContext, sqlVisitor, connection, updateObjType, updateObjs, timeoutSeconds, tableName);
                    if (result == 0) updateSql = $"DROP TABLE {this.OrmProvider.GetTableName(tableName)}";
                    command.CommandText = updateSql;
                    result = command.ExecuteNonQuery(CommandSqlType.BulkCopyUpdate);
                    builder.Clear();
                }
                break;
            case ActionMode.Bulk:
                {
                    var builder = new StringBuilder();
                    (var updateObjs, var bulkCount, var tableName, var fixedParameterSetter, var firstSqlSetter, var sqlSetter, _) = this.Visitor.BuildWithBulk(command);
                    Func<int, string> suffixGetter = index => this.Visitor.IsMultiple ? $"_m{this.Visitor.CommandIndex}{index}" : $"{index}";

                    Action<object, int> sqlExecute = null;
                    if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                    {
                        sqlExecute = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            var tableNames = this.Visitor.ShardingTables[0].TableNames;
                            firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableNames[0], updateObj, suffixGetter.Invoke(index));
                            for (int i = 1; i < tableNames.Count; i++)
                            {
                                builder.Append(';');
                                sqlSetter.Invoke(builder, this.DbContext, tableNames[i], updateObj, suffixGetter.Invoke(index));
                            }
                        };
                    }
                    else
                    {
                        sqlExecute = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, suffixGetter.Invoke(index));
                        };
                    }
                    if (this.Visitor.IsNeedFetchShardingTables)
                        this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                    int index = 0;
                    fixedParameterSetter?.Invoke(command.Parameters);
                    connection.Open();
                    foreach (var updateObj in updateObjs)
                    {
                        sqlExecute.Invoke(updateObj, index);
                        index++;

                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                            command.Parameters.Clear();
                            fixedParameterSetter?.Invoke(command.Parameters);
                            builder.Clear();
                            index = 0;
                        }
                    }
                    if (index > 0)
                    {
                        command.CommandText = builder.ToString();
                        result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                    }
                    builder.Clear();
                }
                break;
            default:
                {
                    if (!this.Visitor.HasWhere)
                        throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

                    if (this.Visitor.IsNeedFetchShardingTables)
                        this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                    command.CommandText = this.Visitor.BuildCommand(this.DbContext, command, out _);
                    connection.Open();
                    result = command.ExecuteNonQuery(CommandSqlType.Update);
                }
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
                    (var updateObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                    Type updateObjType = null;
                    foreach (var updateObj in updateObjs)
                    {
                        updateObjType = updateObj.GetType();
                        break;
                    }
                    if (updateObjType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
                    var fromMapper = this.Visitor.Tables[0].Mapper;
                    var memberMappers = this.Visitor.GetRefMemberMappers(updateObjType, fromMapper, true);
                    var tableName = $"{fromMapper.TableName}_{Guid.NewGuid():N}";

                    //添加临时表
                    var builder = new StringBuilder();
                    builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName(tableName)}(");
                    var pkColumns = new List<string>();
                    foreach ((var refMemberMapper, _) in memberMappers)
                    {
                        var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                        builder.Append($"{fieldName} {refMemberMapper.DbColumnType}");
                        if (refMemberMapper.IsKey)
                        {
                            builder.Append(" NOT NULL");
                            pkColumns.Add(fieldName);
                        }
                        builder.AppendLine(",");
                    }
                    builder.AppendLine($"PRIMARY KEY({string.Join(",", pkColumns)})");
                    builder.AppendLine(");");
                    if (this.Visitor.IsNeedFetchShardingTables)
                        builder.Append(this.Visitor.BuildTableShardingsSql());
                    var bulkCopySql = builder.ToString();

                    builder.Clear();
                    void Execute(string target, string source)
                    {
                        builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a INNER JOIN {this.OrmProvider.GetTableName(source)} b ON ");
                        for (int i = 0; i < pkColumns.Count; i++)
                        {
                            if (i > 0) builder.Append(" AND ");
                            builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                        }
                        builder.Append(" SET ");
                        int setIndex = 0;
                        foreach ((var refMemberMapper, _) in memberMappers)
                        {
                            var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                            if (pkColumns.Contains(fieldName)) continue;
                            if (setIndex > 0) builder.Append(',');
                            builder.Append($"a.{fieldName}=b.{fieldName}");
                            setIndex++;
                        }
                    }
                    if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                    {
                        var tableNames = this.Visitor.ShardingTables[0].TableNames;
                        for (int i = 0; i < tableNames.Count; i++)
                        {
                            if (i > 0) builder.Append(';');
                            Execute(tableNames[i], tableName);
                        }
                    }
                    else Execute(this.Visitor.Tables[0].Body ?? fromMapper.TableName, tableName);
                    builder.Append($";DROP TABLE {this.OrmProvider.GetTableName(tableName)}");
                    var updateSql = builder.ToString();

                    command.CommandText = bulkCopySql;
                    await connection.OpenAsync(cancellationToken);
                    await command.ExecuteNonQueryAsync(CommandSqlType.BulkCopyUpdate, cancellationToken);

                    var dialectOrmProvider = this.OrmProvider as MySqlProvider;
                    var sqlVisitor = this.Visitor as SqlVisitor;
                    result = await dialectOrmProvider.ExecuteBulkCopyAsync(true, this.DbContext, sqlVisitor, connection, updateObjType, updateObjs, timeoutSeconds, cancellationToken, tableName);
                    if (result == 0) updateSql = $"DROP TABLE {this.OrmProvider.GetTableName(tableName)}";
                    command.CommandText = updateSql;
                    result = await command.ExecuteNonQueryAsync(CommandSqlType.BulkCopyUpdate, cancellationToken);
                    builder.Clear();
                }
                break;
            case ActionMode.Bulk:
                {
                    var builder = new StringBuilder();
                    (var updateObjs, var bulkCount, var tableName, var fixedParameterSetter, var firstSqlSetter, var sqlSetter, _) = this.Visitor.BuildWithBulk(command);
                    Func<int, string> suffixGetter = index => this.Visitor.IsMultiple ? $"_m{this.Visitor.CommandIndex}{index}" : $"{index}";

                    Action<object, int> sqlExecute = null;
                    if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                    {
                        sqlExecute = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            var tableNames = this.Visitor.ShardingTables[0].TableNames;
                            firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableNames[0], updateObj, suffixGetter.Invoke(index));
                            for (int i = 1; i < tableNames.Count; i++)
                            {
                                builder.Append(';');
                                sqlSetter.Invoke(builder, this.DbContext, tableNames[i], updateObj, suffixGetter.Invoke(index));
                            }
                        };
                    }
                    else
                    {
                        sqlExecute = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, suffixGetter.Invoke(index));
                        };
                    }
                    if (this.Visitor.IsNeedFetchShardingTables)
                        await this.DbContext.FetchShardingTablesAsync(this.Visitor as SqlVisitor, cancellationToken);

                    int index = 0;
                    fixedParameterSetter?.Invoke(command.Parameters);
                    await connection.OpenAsync(cancellationToken);
                    foreach (var updateObj in updateObjs)
                    {
                        sqlExecute.Invoke(updateObj, index);
                        index++;

                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                            command.Parameters.Clear();
                            fixedParameterSetter?.Invoke(command.Parameters);
                            builder.Clear();
                            index = 0;
                        }
                    }
                    if (index > 0)
                    {
                        command.CommandText = builder.ToString();
                        result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                    }
                    builder.Clear();
                }
                break;
            default:
                {
                    if (!this.Visitor.HasWhere)
                        throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

                    if (this.Visitor.IsNeedFetchShardingTables)
                        this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                    command.CommandText = this.Visitor.BuildCommand(this.DbContext, command, out _);
                    await connection.OpenAsync(cancellationToken);
                    result = await command.ExecuteNonQueryAsync(CommandSqlType.Update, cancellationToken);
                }
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
            (var updateObjs, _) = this.DialectVisitor.BuildWithBulkCopy();
            Type updateObjType = null;
            foreach (var updateObj in updateObjs)
            {
                updateObjType = updateObj.GetType();
                break;
            }
            if (updateObjType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
            var fromMapper = this.Visitor.Tables[0].Mapper;
            var memberMappers = this.Visitor.GetRefMemberMappers(updateObjType, fromMapper, true);
            var tableName = $"{fromMapper.TableName}_{Guid.NewGuid():N}";

            //添加临时表
            builder.AppendLine($"CREATE TEMPORARY TABLE {this.OrmProvider.GetTableName(tableName)}(");
            var pkColumns = new List<string>();
            foreach ((var refMemberMapper, _) in memberMappers)
            {
                var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                builder.Append($"{fieldName} {refMemberMapper.DbColumnType}");
                if (refMemberMapper.IsKey)
                {
                    builder.Append(" NOT NULL");
                    pkColumns.Add(fieldName);
                }
                builder.AppendLine(",");
            }
            builder.AppendLine($"PRIMARY KEY({string.Join(",", pkColumns)})");
            builder.AppendLine(");");

            if (this.Visitor.IsNeedFetchShardingTables)
            {
                builder.Append(this.Visitor.BuildTableShardingsSql());
                builder.Append(';');
            }

            void Execute(string target, string source)
            {
                builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a INNER JOIN {this.OrmProvider.GetTableName(source)} b ON ");
                for (int i = 0; i < pkColumns.Count; i++)
                {
                    if (i > 0) builder.Append(" AND ");
                    builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                }
                builder.Append(" SET ");
                int setIndex = 0;
                foreach ((var refMemberMapper, _) in memberMappers)
                {
                    var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                    if (pkColumns.Contains(fieldName)) continue;
                    if (setIndex > 0) builder.Append(',');
                    builder.Append($"a.{fieldName}=b.{fieldName}");
                    setIndex++;
                }
            }
            if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
            {
                var tableNames = this.Visitor.ShardingTables[0].TableNames;
                for (int i = 0; i < tableNames.Count; i++)
                {
                    if (i > 0) builder.Append(';');
                    Execute(tableNames[i], tableName);
                }
            }
            else Execute(this.Visitor.Tables[0].Body ?? fromMapper.TableName, tableName);
            builder.Append($";DROP TABLE {this.OrmProvider.GetTableName(tableName)}");
            sql = builder.ToString();
        }
        else
        {
            if (this.Visitor.IsNeedFetchShardingTables)
            {
                this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                builder.Append(this.Visitor.BuildTableShardingsSql());
                builder.Append(';');
            }
            (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
            sql = this.Visitor.BuildCommand(this.DbContext, command, out _);
            if (this.Visitor.IsNeedFetchShardingTables)
            {
                builder.Append(sql);
                sql = builder.ToString();
            }
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