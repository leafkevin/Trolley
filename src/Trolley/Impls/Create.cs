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
        this.Visitor = this.DbContext.OrmProvider.NewCreateVisitor(dbContext.DbKey, this.DbContext.MapProvider, this.DbContext.ShardingProvider, this.DbContext.IsParameterized);
        this.Visitor.Initialize(typeof(TEntity));
        this.DbContext = dbContext;
    }
    #endregion

    #region Sharding
    public ICreate<TEntity> UseTable(string tableName)
    {
        var entityType = typeof(TEntity);
        this.Visitor.UseTable(entityType, tableName);
        return this;
    }
    public ICreate<TEntity> UseTableBy(object field1Value, object field2Value = null)
    {
        var entityType = typeof(TEntity);
        this.Visitor.UseTableBy(entityType, field1Value, field2Value);
        return this;
    }
    #endregion

    #region WithBy
    public IContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        if (insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithBulk方法");

        var insertObjType = typeof(TInsertObject);
        if (!insertObjType.IsEntityType(out _))
            throw new NotSupportedException("单个实体必须是类或结构类型，不能是基础类型");

        this.Visitor.WithBy(insertObj);
        return new ContinuedCreate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithBulk
    public IContinuedCreate<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        if (insertObjs is string || insertObjs is IDictionary<string, object>)
            throw new NotSupportedException("批量插入，单个对象类型只支持命名对象、匿名对象或是字典对象");

        this.Visitor.WithBulk(insertObjs, bulkCount);
        return new ContinuedCreate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region From
    public IFromCommand<T> From<T>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('b', null, typeof(T));
        return this.OrmProvider.NewFromCommand<T>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<T1, T2> From<T1, T2>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('b', null, typeof(T1), typeof(T2));
        return this.OrmProvider.NewFromCommand<T1, T2>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<T1, T2, T3> From<T1, T2, T3>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('b', null, typeof(T1), typeof(T2), typeof(T3));
        return this.OrmProvider.NewFromCommand<T1, T2, T3>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<T1, T2, T3, T4> From<T1, T2, T3, T4>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('b', null, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('b', null, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('b', null, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, T6>(typeof(TEntity), this.DbContext, queryVisitor);
    }

    public IFromCommand<TTarget> From<TTarget>(IQuery<TTarget> subQuery)
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From(typeof(TTarget), subQuery);
        return this.OrmProvider.NewFromCommand<TTarget>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<TTarget> From<TTarget>(Func<IFromQuery, IQuery<TTarget>> cteSubQuery)
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From(typeof(TTarget), this.DbContext, cteSubQuery);
        return this.OrmProvider.NewFromCommand<TTarget>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    #endregion
}
public class Created<TEntity> : ICreated<TEntity>
{
    #region Properties
    public DbContext DbContext { get; protected set; }
    public ICreateVisitor Visitor { get; protected set; }
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
        Exception exception = null;
        bool isNeedClose = this.DbContext.IsNeedClose;
        using var command = this.DbContext.CreateCommand();
        try
        {
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.Bulk:
                    var sqlBuilder = new StringBuilder();
                    (var isNeedSplit, var tableName, var insertObjs, var bulkCount, var firstInsertObj,
                        var headSqlSetter, var commandInitializer) = this.Visitor.BuildWithBulk(command);

                    Action<string, object> clearCommand = (tableName, insertObj) =>
                    {
                        sqlBuilder.Clear();
                        command.Parameters.Clear();
                        headSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName, insertObj);
                    };
                    Func<string, IEnumerable, int> executor = (tableName, insertObjs) =>
                    {
                        var isFirst = true;
                        int count = 0, index = 0;
                        foreach (var insertObj in insertObjs)
                        {
                            if (index > 0) sqlBuilder.Append(',');
                            commandInitializer.Invoke(command.Parameters, sqlBuilder, insertObj, index.ToString());
                            if (index >= bulkCount)
                            {
                                command.CommandText = sqlBuilder.ToString();
                                if (isFirst)
                                {
                                    this.DbContext.Open();
                                    isFirst = false;
                                }
                                count += command.ExecuteNonQuery();
                                clearCommand.Invoke(tableName, insertObj);
                                index = 0;
                                continue;
                            }
                            index++;
                        }
                        if (index > 0)
                        {
                            command.CommandText = sqlBuilder.ToString();
                            if (isFirst) this.DbContext.Open();
                            count += command.ExecuteNonQuery();
                        }
                        return count;
                    };

                    if (isNeedSplit)
                    {
                        var entityType = typeof(TEntity);
                        var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                        foreach (var tabledInsertObj in tabledInsertObjs)
                        {
                            headSqlSetter.Invoke(command.Parameters, sqlBuilder, tabledInsertObj.Key, tabledInsertObj);
                            result += executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
                        }
                    }
                    else
                    {
                        headSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName, firstInsertObj);
                        result = executor.Invoke(tableName, insertObjs);
                    }
                    sqlBuilder.Clear();
                    sqlBuilder = null;
                    break;
                default:
                    //默认单条
                    command.CommandText = this.Visitor.BuildCommand(command, false);
                    this.DbContext.Open();
                    result = command.ExecuteNonQuery();
                    break;
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
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
        using var command = this.DbContext.CreateDbCommand();
        try
        {
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.Bulk:
                    var sqlBuilder = new StringBuilder();
                    (var isNeedSplit, var tableName, var insertObjs, var bulkCount, var firstInsertObj,
                        var headSqlSetter, var commandInitializer) = this.Visitor.BuildWithBulk(command);

                    Action<string, object> clearCommand = (tableName, insertObj) =>
                    {
                        sqlBuilder.Clear();
                        command.Parameters.Clear();
                        headSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName, insertObj);
                    };
                    Func<string, IEnumerable, Task<int>> executor = async (tableName, insertObjs) =>
                    {
                        var isFirst = true;
                        int count = 0, index = 0;
                        foreach (var insertObj in insertObjs)
                        {
                            if (index > 0) sqlBuilder.Append(',');
                            commandInitializer.Invoke(command.Parameters, sqlBuilder, insertObj, index.ToString());
                            if (index >= bulkCount)
                            {
                                command.CommandText = sqlBuilder.ToString();
                                if (isFirst)
                                {
                                    await this.DbContext.OpenAsync(cancellationToken);
                                    isFirst = false;
                                }
                                count += await command.ExecuteNonQueryAsync(cancellationToken);
                                clearCommand.Invoke(tableName, insertObj);
                                index = 0;
                                continue;
                            }
                            index++;
                        }
                        if (index > 0)
                        {
                            command.CommandText = sqlBuilder.ToString();
                            if (isFirst) await this.DbContext.OpenAsync(cancellationToken);
                            count += await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                        return count;
                    };

                    if (isNeedSplit)
                    {
                        var entityType = typeof(TEntity);
                        var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, insertObjs);
                        foreach (var tabledInsertObj in tabledInsertObjs)
                        {
                            headSqlSetter.Invoke(command.Parameters, sqlBuilder, tabledInsertObj.Key, tabledInsertObj);
                            result += await executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
                        }
                    }
                    else
                    {
                        headSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName, firstInsertObj);
                        result = await executor.Invoke(tableName, insertObjs);
                    }
                    sqlBuilder.Clear();
                    sqlBuilder = null;
                    break;
                default:
                    //默认单条
                    command.CommandText = this.Visitor.BuildCommand(command, false);
                    await this.DbContext.OpenAsync(cancellationToken);
                    result = await command.ExecuteNonQueryAsync(cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region ExecuteIdentity
    public int ExecuteIdentity() => this.DbContext.CreateIdentity<int>(this.Visitor);
    public async Task<int> ExecuteIdentityAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.CreateIdentityAsync<int>(this.Visitor, cancellationToken);
    public long ExecuteIdentityLong() => this.DbContext.CreateIdentity<long>(this.Visitor);
    public async Task<long> ExecuteIdentityLongAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.CreateIdentityAsync<long>(this.Visitor, cancellationToken);
    #endregion

    #region ToMultipleCommand
    public MultipleCommand ToMultipleCommand()
    {
        var result = this.Visitor.CreateMultipleCommand();
        this.Visitor.Dispose();
        this.Visitor = null;
        return result;
    }
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        using var command = this.DbContext.CreateCommand();
        var sql = this.Visitor.BuildCommand(command, false);
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        return sql;
    }
    #endregion

    #region Close
    public void Close()
    {
        this.DbContext.Close();
        this.Visitor.Dispose();
        this.Visitor = null;
    }
    public async ValueTask CloseAsync()
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
    public IContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => this.WithBy(true, insertObj);
    public IContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        if (insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithBulkBy方法");
        if (!typeof(TInsertObject).IsEntityType(out _))
            throw new NotSupportedException("方法WithBy<TInsertObject>(TInsertObject insertObj)只支持类对象参数，不支持基础类型参数");

        if (condition) this.Visitor.WithBy(insertObj);
        return this;
    }
    public IContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.WithBy(true, fieldSelector, fieldValue);
    public IContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        if (condition) this.Visitor.WithByField(fieldSelector, fieldValue);
        return this;
    }
    #endregion

    #region IgnoreFields
    public IContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.IgnoreFields(fieldNames);
        return this;
    }
    public IContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));
        if (fieldsSelector.Body.NodeType != ExpressionType.New && fieldsSelector.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelector)},只支持New或MemberInit类型表达式");

        this.Visitor.IgnoreFields(fieldsSelector);
        return this;
    }
    #endregion

    #region OnlyFields
    public IContinuedCreate<TEntity> OnlyFields(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.OnlyFields(fieldNames);
        return this;
    }
    public IContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));
        if (fieldsSelector.Body.NodeType != ExpressionType.New && fieldsSelector.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelector)},只支持New或MemberInit类型表达式");

        this.Visitor.OnlyFields(fieldsSelector);
        return this;
    }
    #endregion
}