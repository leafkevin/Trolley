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
        this.Visitor = this.DbContext.OrmProvider.NewCreateVisitor(dbContext);
        this.Visitor.Initialize(typeof(TEntity));
        this.DbContext = dbContext;
    }
    #endregion

    #region Sharding
    public virtual ICreate<TEntity> UseTable(string tableName)
    {
        this.Visitor.UseTable(false, tableName);
        return this;
    }
    public virtual ICreate<TEntity> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
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
    public virtual IFromCommand<T> From<T>()
    {
        var queryVisitor = this.FromInternal(typeof(T));
        return this.OrmProvider.NewFromCommand<T>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<T1, T2> From<T1, T2>()
    {
        var queryVisitor = this.FromInternal(typeof(T1), typeof(T2));
        return this.OrmProvider.NewFromCommand<T1, T2>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<T1, T2, T3> From<T1, T2, T3>()
    {
        var queryVisitor = this.FromInternal(typeof(T1), typeof(T2), typeof(T3));
        return this.OrmProvider.NewFromCommand<T1, T2, T3>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4> From<T1, T2, T3, T4>()
    {
        var queryVisitor = this.FromInternal(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
    {
        var queryVisitor = this.FromInternal(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>()
    {
        var queryVisitor = this.FromInternal(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, T6>(this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<T> From<T>(IQuery<T> subQuery)
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.AddTable(this.Visitor.Tables[0]);
        queryVisitor.From(typeof(T), subQuery);
        queryVisitor.IsFromCommand = true;
        queryVisitor.IsFromQuery = true;
        return this.OrmProvider.NewFromCommand<T>(this.DbContext, queryVisitor);
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
        Exception exception = null;
        bool isNeedClose = this.DbContext.IsNeedClose;
        CommandEventArgs eventArgs = null;
        using var command = this.DbContext.CreateCommand();
        try
        {
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.Bulk:
                    {
                        var builder = new StringBuilder();
                        (var isNeedSplit, var tableName, var insertObjs, var bulkCount,
                            var firstSqlSetter, var loopSqlSetter, _) = this.Visitor.BuildWithBulk(command);

                        Action<string> clearCommand = tableName =>
                        {
                            builder.Clear();
                            command.Parameters.Clear();
                            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        };
                        Func<string, IEnumerable, int> executor = (tableName, insertObjs) =>
                        {
                            int count = 0, index = 0;
                            foreach (var insertObj in insertObjs)
                            {
                                if (index > 0) builder.Append(',');
                                loopSqlSetter.Invoke(command.Parameters, builder, insertObj, index.ToString());
                                if (index >= bulkCount)
                                {
                                    command.CommandText = builder.ToString();
                                    eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.BulkInsert, eventArgs);
                                    count += command.ExecuteNonQuery();
                                    clearCommand.Invoke(tableName);
                                    index = 0;
                                    continue;
                                }
                                index++;
                            }
                            if (index > 0)
                            {
                                command.CommandText = builder.ToString();
                                eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.BulkInsert, eventArgs);
                                count += command.ExecuteNonQuery();
                            }
                            return count;
                        };
                        this.DbContext.Open();
                        if (isNeedSplit)
                        {
                            var entityType = this.Visitor.Tables[0].EntityType;
                            var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                            foreach (var tabledInsertObj in tabledInsertObjs)
                            {
                                firstSqlSetter.Invoke(command.Parameters, builder, tabledInsertObj.Key);
                                result += executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
                            }
                        }
                        else
                        {
                            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                            result = executor.Invoke(tableName, insertObjs);
                        }
                        builder.Clear();
                        builder = null;
                    }
                    break;
                default:
                    {
                        //默认单条
                        command.CommandText = this.Visitor.BuildCommand(command, false, out _);
                        this.DbContext.Open();
                        eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.Insert);
                        result = command.ExecuteNonQuery();
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            var sqlType = this.Visitor.ActionMode == ActionMode.Bulk ? CommandSqlType.BulkInsert : CommandSqlType.Insert;
            this.DbContext.AddCommandFailedFilter(command, sqlType, eventArgs, exception);
        }
        finally
        {
            var sqlType = this.Visitor.ActionMode == ActionMode.Bulk ? CommandSqlType.BulkInsert : CommandSqlType.Insert;
            this.DbContext.AddCommandAfterFilter(command, sqlType, eventArgs, exception == null, exception);
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.Close();
        }
        if (exception != null) throw exception;
        return result;
    }
    public virtual async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        Exception exception = null;
        bool isNeedClose = this.DbContext.IsNeedClose;
        CommandEventArgs eventArgs = null;
        using var command = this.DbContext.CreateDbCommand();
        try
        {
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.Bulk:
                    {
                        var builder = new StringBuilder();
                        (var isNeedSplit, var tableName, var insertObjs, var bulkCount,
                            var firstSqlSetter, var loopSqlSetter, _) = this.Visitor.BuildWithBulk(command);

                        Action<string> clearCommand = tableName =>
                        {
                            builder.Clear();
                            command.Parameters.Clear();
                            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        };
                        Func<string, IEnumerable, Task<int>> executor = async (tableName, insertObjs) =>
                        {
                            int count = 0, index = 0;
                            foreach (var insertObj in insertObjs)
                            {
                                if (index > 0) builder.Append(',');
                                loopSqlSetter.Invoke(command.Parameters, builder, insertObj, index.ToString());
                                if (index >= bulkCount)
                                {
                                    command.CommandText = builder.ToString();
                                    eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.BulkInsert, eventArgs);
                                    count += await command.ExecuteNonQueryAsync(cancellationToken);
                                    clearCommand.Invoke(tableName);
                                    index = 0;
                                    continue;
                                }
                                index++;
                            }
                            if (index > 0)
                            {
                                command.CommandText = builder.ToString();
                                eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.BulkInsert, eventArgs);
                                count += await command.ExecuteNonQueryAsync(cancellationToken);
                            }
                            return count;
                        };
                        await this.DbContext.OpenAsync(cancellationToken);
                        if (isNeedSplit)
                        {
                            var entityType = this.Visitor.Tables[0].EntityType;
                            var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                            foreach (var tabledInsertObj in tabledInsertObjs)
                            {
                                firstSqlSetter.Invoke(command.Parameters, builder, tabledInsertObj.Key);
                                result += await executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
                            }
                        }
                        else
                        {
                            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                            result = await executor.Invoke(tableName, insertObjs);
                        }
                        builder.Clear();
                        builder = null;
                    }
                    break;
                default:
                    {
                        //默认单条
                        command.CommandText = this.Visitor.BuildCommand(command, false, out _);
                        await this.DbContext.OpenAsync(cancellationToken);
                        eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.Insert);
                        result = await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            var sqlType = this.Visitor.ActionMode == ActionMode.Bulk ? CommandSqlType.BulkInsert : CommandSqlType.Insert;
            this.DbContext.AddCommandFailedFilter(command, sqlType, eventArgs, exception);
        }
        finally
        {
            var sqlType = this.Visitor.ActionMode == ActionMode.Bulk ? CommandSqlType.BulkInsert : CommandSqlType.Insert;
            this.DbContext.AddCommandAfterFilter(command, sqlType, eventArgs, exception == null, exception);
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region ExecuteIdentity
    public virtual int ExecuteIdentity() => this.DbContext.CreateResult<int>((command, dbContext) =>
    {
        command.CommandText = this.Visitor.BuildCommand(command, true, out var readerFields);
        return readerFields;
    });
    public virtual async Task<int> ExecuteIdentityAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.CreateResultAsync<int>((command, dbContext) =>
        {
            command.CommandText = this.Visitor.BuildCommand(command, true, out var readerFields);
            return readerFields;
        }, cancellationToken);
    public virtual long ExecuteIdentityLong() => this.DbContext.CreateResult<long>((command, dbContext) =>
    {
        command.CommandText = this.Visitor.BuildCommand(command, true, out var readerFields);
        return readerFields;
    });
    public virtual async Task<long> ExecuteIdentityLongAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.CreateResultAsync<long>((command, dbContext) =>
        {
            command.CommandText = this.Visitor.BuildCommand(command, true, out var readerFields);
            return readerFields;
        }, cancellationToken);
    #endregion

    #region ToMultipleCommand
    public virtual MultipleCommand ToMultipleCommand()
    {
        var result = this.Visitor.CreateMultipleCommand();
        this.Visitor.Dispose();
        this.Visitor = null;
        return result;
    }
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        using var command = this.DbContext.CreateCommand();
        var sql = this.Visitor.BuildCommand(command, false, out _);
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        return sql;
    }
    #endregion

    #region Close
    public virtual void Close()
    {
        this.DbContext.Close();
        this.Visitor.Dispose();
        this.Visitor = null;
    }
    public virtual async ValueTask CloseAsync()
    {
        await this.DbContext.CloseAsync();
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
        base.WithByInternal<TField>(condition, fieldSelector, fieldValue);
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