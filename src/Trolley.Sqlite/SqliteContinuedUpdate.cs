using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.Sqlite;

public class SqliteContinuedUpdate<TEntity> : ContinuedUpdate<TEntity>, ISqliteUpdated<TEntity>, ISqliteContinuedUpdate<TEntity>
{
    #region Properties
    public SqliteUpdateVisitor DialectVisitor { get; private set; }
    public IOrmProvider OrmProvider => this.Visitor.OrmProvider;
    #endregion

    #region Constructor
    public SqliteContinuedUpdate(DbContext dbContext, IUpdateVisitor visitor) : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as SqliteUpdateVisitor;
    }
    #endregion

    #region Set
    public new ISqliteContinuedUpdate<TEntity> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public new ISqliteContinuedUpdate<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
        => base.Set(condition, updateObj) as ISqliteContinuedUpdate<TEntity>;
    public new ISqliteContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public new ISqliteContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.Set(condition, fieldSelector, fieldValue) as ISqliteContinuedUpdate<TEntity>;
    public new ISqliteContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public new ISqliteContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
        => base.Set(condition, fieldsAssignment) as ISqliteContinuedUpdate<TEntity>;
    #endregion

    #region SetFrom
    public new ISqliteContinuedUpdate<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public new ISqliteContinuedUpdate<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => base.SetFrom(condition, fieldSelector, valueSelector) as ISqliteContinuedUpdate<TEntity>;
    public new ISqliteContinuedUpdate<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public new ISqliteContinuedUpdate<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => base.SetFrom(condition, fieldsAssignment) as ISqliteContinuedUpdate<TEntity>;
    #endregion

    #region IgnoreFields
    public new ISqliteContinuedUpdate<TEntity> IgnoreFields(params string[] fieldNames)
        => base.IgnoreFields(fieldNames) as ISqliteContinuedUpdate<TEntity>;
    public new ISqliteContinuedUpdate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.IgnoreFields(fieldsSelector) as ISqliteContinuedUpdate<TEntity>;
    #endregion

    #region OnlyFields
    public new ISqliteContinuedUpdate<TEntity> OnlyFields(params string[] fieldNames)
        => base.OnlyFields(fieldNames) as ISqliteContinuedUpdate<TEntity>;
    public new ISqliteContinuedUpdate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.OnlyFields(fieldsSelector) as ISqliteContinuedUpdate<TEntity>;
    #endregion

    #region Where/And
    public new ISqliteUpdated<TEntity> Where<TWhereObj>(TWhereObj whereObj)
    {
        base.Where(whereObj);
        return this;
    }
    public new ISqliteContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public new ISqliteContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as ISqliteContinuedUpdate<TEntity>;
    public new ISqliteContinuedUpdate<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public new ISqliteContinuedUpdate<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as ISqliteContinuedUpdate<TEntity>;
    #endregion

    #region Returnning
    public ISqliteUpdated<TEntity, TResult> Returning<TResult>(string fieldNames)
    {
        this.DialectVisitor.Returning(fieldNames);
        return new SqliteUpdated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public ISqliteUpdated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Returning(fieldsSelector);
        return new SqliteUpdated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.Bulk:
                var builder = new StringBuilder();
                (var updateObjs, var bulkCount, var tableName, var fixedParameterSetter, var firstSqlSetter, var sqlSetter) = this.Visitor.BuildWithBulk(command);
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
                    if (index >= bulkCount)
                    {
                        command.CommandText = builder.ToString();
                        result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                        command.Parameters.Clear();
                        fixedParameterSetter?.Invoke(command.Parameters);
                        builder.Clear();
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    command.CommandText = builder.ToString();
                    result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                }
                builder.Clear();
                break;
            default:
                if (!this.Visitor.HasWhere)
                    throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");

                if (this.Visitor.IsNeedFetchShardingTables)
                    this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                command.CommandText = this.Visitor.BuildCommand(this.DbContext, command, out _);
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
            case ActionMode.Bulk:
                var builder = new StringBuilder();
                (var updateObjs, var bulkCount, var tableName, var fixedParameterSetter, var firstSqlSetter, var sqlSetter) = this.Visitor.BuildWithBulk(command);
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
                    if (index >= bulkCount)
                    {
                        command.CommandText = builder.ToString();
                        result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                        command.Parameters.Clear();
                        fixedParameterSetter?.Invoke(command.Parameters);
                        builder.Clear();
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    command.CommandText = builder.ToString();
                    result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                }
                builder.Clear();
                break;
            default:
                if (!this.Visitor.HasWhere)
                    throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");

                if (this.Visitor.IsNeedFetchShardingTables)
                    this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                command.CommandText = this.Visitor.BuildCommand(this.DbContext, command, out _);
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
    public new string ToSql(out List<IDbDataParameter> dbParameters)
    {
        var builder = new StringBuilder();
        if (this.Visitor.IsNeedFetchShardingTables)
        {
            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
            builder.Append(this.Visitor.BuildTableShardingsSql());
            builder.Append(';');
        }
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var sql = this.Visitor.BuildCommand(this.DbContext, command, out _);
        if (this.Visitor.IsNeedFetchShardingTables)
        {
            builder.Append(sql);
            sql = builder.ToString();
        }
        dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        if (isNeedClose) connection.Close();
        this.Visitor.Dispose();
        builder.Clear();
        return sql;
    }
    #endregion
}
public class SqliteBulkContinuedUpdate<TEntity> : BulkContinuedUpdate<TEntity>, ISqliteUpdated<TEntity>, ISqliteBulkContinuedUpdate<TEntity>
{
    #region Properties
    public SqliteUpdateVisitor DialectVisitor { get; private set; }
    public IOrmProvider OrmProvider => this.Visitor.OrmProvider;
    #endregion

    #region Constructor
    public SqliteBulkContinuedUpdate(DbContext dbContext, IUpdateVisitor visitor) : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as SqliteUpdateVisitor;
    }
    #endregion

    #region Set
    public new ISqliteBulkContinuedUpdate<TEntity> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public new ISqliteBulkContinuedUpdate<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
        => base.Set(condition, updateObj) as ISqliteBulkContinuedUpdate<TEntity>;
    public new ISqliteBulkContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public new ISqliteBulkContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.Set(condition, fieldSelector, fieldValue) as ISqliteBulkContinuedUpdate<TEntity>;
    public new ISqliteBulkContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public new ISqliteBulkContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
        => base.Set(condition, fieldsAssignment) as ISqliteBulkContinuedUpdate<TEntity>;
    #endregion

    #region IgnoreFields
    public new ISqliteBulkContinuedUpdate<TEntity> IgnoreFields(params string[] fieldNames)
        => base.IgnoreFields(fieldNames) as ISqliteBulkContinuedUpdate<TEntity>;
    public new ISqliteBulkContinuedUpdate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.IgnoreFields(fieldsSelector) as ISqliteBulkContinuedUpdate<TEntity>;
    #endregion

    #region OnlyFields
    public new ISqliteBulkContinuedUpdate<TEntity> OnlyFields(params string[] fieldNames)
        => base.OnlyFields(fieldNames) as ISqliteBulkContinuedUpdate<TEntity>;
    public new ISqliteBulkContinuedUpdate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
        => base.OnlyFields(fieldsSelector) as ISqliteBulkContinuedUpdate<TEntity>;
    #endregion

    #region Where/And
    public new ISqliteUpdated<TEntity> Where<TWhereObj>(TWhereObj whereObj)
    {
        base.Where(whereObj);
        return this;
    }
    public new ISqliteBulkContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public new ISqliteBulkContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.Where(condition, ifPredicate, elsePredicate) as ISqliteBulkContinuedUpdate<TEntity>;
    public new ISqliteBulkContinuedUpdate<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public new ISqliteBulkContinuedUpdate<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => base.And(condition, ifPredicate, elsePredicate) as ISqliteBulkContinuedUpdate<TEntity>;
    #endregion

    #region Returnning
    public ISqliteBulkUpdated<TEntity, TResult> Returning<TResult>(string fieldNames)
    {
        this.DialectVisitor.Returning(fieldNames);
        return new SqliteBulkUpdated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    public ISqliteBulkUpdated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        this.DialectVisitor.Returning(fieldsSelector);
        return new SqliteBulkUpdated<TEntity, TResult>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.Bulk:
                var builder = new StringBuilder();
                (var updateObjs, var bulkCount, var tableName, var fixedParameterSetter, var firstSqlSetter, var sqlSetter) = this.Visitor.BuildWithBulk(command);
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
                    if (index >= bulkCount)
                    {
                        command.CommandText = builder.ToString();
                        result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                        command.Parameters.Clear();
                        fixedParameterSetter?.Invoke(command.Parameters);
                        builder.Clear();
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    command.CommandText = builder.ToString();
                    result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                }
                builder.Clear();
                break;
            default:
                if (!this.Visitor.HasWhere)
                    throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");

                if (this.Visitor.IsNeedFetchShardingTables)
                    this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                command.CommandText = this.Visitor.BuildCommand(this.DbContext, command, out _);
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
            case ActionMode.Bulk:
                var builder = new StringBuilder();
                (var updateObjs, var bulkCount, var tableName, var fixedParameterSetter, var firstSqlSetter, var sqlSetter) = this.Visitor.BuildWithBulk(command);
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
                    if (index >= bulkCount)
                    {
                        command.CommandText = builder.ToString();
                        result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                        command.Parameters.Clear();
                        fixedParameterSetter?.Invoke(command.Parameters);
                        builder.Clear();
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    command.CommandText = builder.ToString();
                    result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                }
                builder.Clear();
                break;
            default:
                if (!this.Visitor.HasWhere)
                    throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");

                if (this.Visitor.IsNeedFetchShardingTables)
                    this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                command.CommandText = this.Visitor.BuildCommand(this.DbContext, command, out _);
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
    public new string ToSql(out List<IDbDataParameter> dbParameters)
    {
        string sql;
        dbParameters = null;
        var builder = new StringBuilder();
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
        this.Visitor.Dispose();
        builder.Clear();
        return sql;
    }
    #endregion
}